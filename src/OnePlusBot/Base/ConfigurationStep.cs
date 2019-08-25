using System.Text;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;

namespace OnePlusBot.Base {
    public class ConfigurationStep : IReactionCallback 
    {

        public enum StepType { Text, Reaction};

        public object Result;
        public SocketCommandContext Context { get; }
        public InteractiveService Interactive { get; private set; }
        public IUserMessage Message { get; private set; }

        public Collection<IMessage> PostedMessages = new Collection<IMessage>();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(3);

        public ICriterion<SocketReaction> Criterion => _criterion;

        private readonly ICriterion<SocketReaction> _criterion;

        public RunMode RunMode => RunMode.Sync;
        private StepType type;

        private ConfigurationStep parent;
        public Collection<string> additionalPosts = new Collection<string>();

        public Func<String, bool> TextCallback { get; set; }

        public Func<ConfigurationStep, bool> beforeTextPosted { get; set; }

        public ConfigurationStep(string description, InteractiveService interactive, SocketCommandContext context, StepType type, ConfigurationStep parent)
        {
            this.Interactive = interactive;
            this.description = description;
            Context = context;
            _criterion =  new ReactionSameUserCriterion();
            this.type = type;
            this.parent = parent;
        }
        private string description { get; set; }

        public Collection<ReactionAction> Actions = new Collection<ReactionAction>();

        public async Task SetupMessage()
        {
            PostedMessages = new Collection<IMessage>();
            Interactive.ClearReactionCallbacks();
            if(this.beforeTextPosted != null)
            {
                this.beforeTextPosted(this);
            }
            if(additionalPosts.Count > 0)
            {
                var additionalPostsString = new StringBuilder();
                foreach(var text in additionalPosts)
                {
                    additionalPostsString.Append(text + Environment.NewLine);
                }
                var value = additionalPostsString.ToString();
                if(value != string.Empty)
                {
                    var message = await Context.Channel.SendMessageAsync(value);
                    PostedMessages.Add(message);
                }
            }
           
            this.Message = await Context.Channel.SendMessageAsync(description);
            PostedMessages.Add(this.Message);
            if(type == StepType.Reaction)
            {
                var emotes = new Collection<IEmote>();
                foreach(var action in Actions)
                {
                    emotes.Add(action.Emote);
                }
                await Message.AddReactionsAsync(emotes.ToArray());
                Interactive.AddReactionCallback(this.Message, this);
            }
            else 
            {
                var response = await Interactive.NextMessageAsync(Context, true, true, TimeSpan.FromMinutes(2));
                Result = response.Content;
                await response.DeleteAsync();
                reactBasedOnResult();
            }

            if (Timeout.HasValue && Timeout.Value != null)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(async _ =>
                {
                    if(Message != null)
                    {
                        Interactive.RemoveReactionCallback(Message);
                    }
                    
                    foreach(var message in PostedMessages)
                    {
                        await message.DeleteAsync();
                    }  
                    
                });
            }
            
            
        }

        private async Task reactBasedOnResult()
        {
            foreach(var message in PostedMessages)
            {
                await Context.Channel.DeleteMessageAsync(message);
            }
            PostedMessages.Clear();
            if(Result != null){
                if(Result is ConfigurationStep)
                {
                    var casted = Result as ConfigurationStep;
                    await casted.SetupMessage();
                }
                else if(Result is string)
                {
                    this.TextCallback(Result as string);
                    await this.parent.SetupMessage();
                }
            }
           
        }

        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;
            foreach(var action in Actions)
            {
                if(action.Emote.Equals(emote))
                {
                    action.Action(this);
                }
            }
            await Task.Delay(500);

            reactBasedOnResult();
            
            return true;
        }
        
    }

    public class ReactionAction 
    {
        public IEmote Emote { get; set; }
        public Func<ConfigurationStep,bool> Action { get; set; }

        public ReactionAction(IEmote emote) 
        {
            this.Emote = emote;
        }

    }

    
}