using System.Threading.Tasks;
using System.Text;
using System.Collections.ObjectModel;
using System;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;
using OnePlusBot.Helpers;

namespace OnePlusBot.Base {
    public class ConfigurationStep : IReactionCallback 
    {

        public enum StepType { Text, Reaction};

        public object Result;
        public SocketCommandContext Context { get; }
        public InteractiveService Interactive { get; private set; }
        public IUserMessage Message { get; private set; }

        public Collection<IMessage> PostedMessages = new Collection<IMessage>();


        // we could not use postedmessages, because the embed step went directly back to an ambed step
        // so the posted messages were cleared when the message was setup, therefore this behaviour was needed
        public Collection<IMessage> MessagesToRemoveOnNextProgression = new Collection<IMessage>();
        public TimeSpan? Timeout => TimeSpan.FromMinutes(3);

        public ICriterion<SocketReaction> Criterion => _criterion;

        private readonly ICriterion<SocketReaction> _criterion;

        public RunMode RunMode => RunMode.Sync;
        private StepType type;

        private ConfigurationStep parent;
        public Collection<string> additionalPosts = new Collection<string>();

        public Func<String, ConfigurationStep, Task> TextCallback { get; set; }

        public Func<ConfigurationStep, Task> beforeTextPosted { get; set; }

        private bool deleteMessages = true;


        // the text to post to end the step
        public static string EXIT_TEXT = "exit";

        public ConfigurationStep(string description, InteractiveService interactive, SocketCommandContext context, StepType type, ConfigurationStep parent, bool deleteMessages=false)
        {
            this.Interactive = interactive;
            this.description = description;
            Context = context;
            _criterion =  new ReactionSameUserCriterion();
            this.type = type;
            this.parent = parent;
            if(type == StepType.Reaction)
            {
                var abortAction = new ReactionAction(new Emoji("🆘"));
                abortAction.Action = (ConfigurationStep a) => 
                {
                    a.Result = null;
                    return Task.CompletedTask;
                };
                this.Actions.Add(abortAction);
            }
            this.deleteMessages = deleteMessages;
        }
        private string description { get; set; }

        public Collection<ReactionAction> Actions = new Collection<ReactionAction>();

        public async Task SetupMessage()
        {
          PostedMessages = new Collection<IMessage>();
          Interactive.ClearReactionCallbacks();
          if(this.beforeTextPosted != null)
          {
            await this.beforeTextPosted(this);
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
            if(this.deleteMessages) {
              await response.DeleteAsync();
            }
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
              if(this.deleteMessages) {
                foreach(var message in PostedMessages)
                {
                  await message.DeleteAsync();
                }
              }
            });
          }
            
            
        }

        private async Task RemoveMessagesOnNextProgression()
        {
          if(this.deleteMessages) {
            foreach(var message in MessagesToRemoveOnNextProgression)
            {
              await message.DeleteAsync();
            }
          }
          MessagesToRemoveOnNextProgression.Clear();
        }

        private async Task reactBasedOnResult()
        {
          if(this.deleteMessages) {
            foreach(var message in PostedMessages)
            {
              await Context.Channel.DeleteMessageAsync(message);
            }
          }

          PostedMessages.Clear();
          if(Result != null)
          {
            if(Result is ConfigurationStep)
            {
              var casted = Result as ConfigurationStep;
              await casted.SetupMessage();
            }
            else if(Result is string)
            {
              if(Result as string == EXIT_TEXT)
              {
                return;
              }
              this.TextCallback(Result as string, this);
              if(this.Result != null && this.Result is ConfigurationStep)
              {
                // text posts can now also return a configuration step, so we do not always go back to the parent
                await (this.Result as ConfigurationStep).SetupMessage();
              }
              else
              {
                await this.parent.SetupMessage();
              }
            }
          }
           
        }

        /// <summary>
        /// Checks what kind of action should be taked based on the kind of reaction which was added to the message
        /// </summary>
        /// <param name="reaction">The <see cref="Discord.WebSocket.SocketReaction"> reaction which was added by the user</param>
        /// <returns></returns>
        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            await RemoveMessagesOnNextProgression();
            var emote = reaction.Emote;
            foreach(var actionToExecute in Actions)
            {
                if(actionToExecute.Emote.Equals(emote))
                {
                    try
                    {
                      await actionToExecute.Action(this);
                    }
                    catch(Exception e)
                    {
                      await Context.Channel.SendMessageAsync(e.Message);
                    }
                    break;
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
        
        public Func<ConfigurationStep, Task> Action { get; set; }

        public ReactionAction(IEmote emote) 
        {
            this.Emote = emote;
        }

    }

    public class PaginationWithAction 
    {
        public ConfigurationStep step { get; set; }
        public ConfigurationStep parent { get; set; }

        public System.Collections.Generic.List<IPaginatable> elements { get; set; }
        private int currentPage = 0;
        private int elementOnPage = 5;

        public Func<object, ConfigurationStep> actionOnIndex { get; set; } 

        private bool needsConfirmation = false;

        private SocketCommandContext context;

        private InteractiveService interactive;


        public PaginationWithAction(ConfigurationStep step, ConfigurationStep parent, System.Collections.Generic.List<IPaginatable> elements, 
        bool confirmation, SocketCommandContext context, InteractiveService interactive)
        {
            this.step = step;
            this.parent = parent;
            this.elements = elements;
            this.needsConfirmation = confirmation;
            this.context = context;
            this.interactive = interactive;
        }

        public void setup()
        {
            var prevAction = new ReactionAction(new Emoji("◀"));
            prevAction.Action = (ConfigurationStep a )=> 
            {
                if(currentPage > 0)
                {
                    currentPage--;
                }
                a.Result = step;
                 return Task.CompletedTask;
            };
            var forwardAction = new ReactionAction(new Emoji("▶"));

            forwardAction.Action = (ConfigurationStep a ) => 
            {
                if(currentPage < (elements.Count - 1) / elementOnPage)
                {
                    currentPage++;
                }
                a.Result = step;
                return Task.CompletedTask;
            };

            Func<int, Task<ConfigurationStep>> processAtIndex = async (int index) => 
            {
                index = currentPage * elementOnPage + index;
                if(index < elements.Count)
                {
                    var element = elements[index];
                    if(needsConfirmation)
                    {
                        return await AskForConfirmation(element);
                    }
                    else
                    {
                        actionOnIndex(element);
                    }
                }
                return null;
            };
            var firstAction = new ReactionAction(new Emoji("\u0031\u20e3"));
            firstAction.Action = async (ConfigurationStep a) => 
            {
                var otherStep = await processAtIndex(0);
                a.Result = otherStep ?? step;
                await Task.CompletedTask;
            };
            var secondAction = new ReactionAction(new Emoji("\u0032\u20e3"));
            secondAction.Action = async (ConfigurationStep a) => 
            {
                var otherStep = await processAtIndex(1);
                a.Result = otherStep ?? step;
                await Task.CompletedTask;
            };
            var thirdAction = new ReactionAction(new Emoji("\u0033\u20e3"));
            thirdAction.Action = async (ConfigurationStep a) => 
            {
                var otherStep = await processAtIndex(2);
                a.Result = otherStep ?? step;
                await Task.CompletedTask;
            };
            var fourthAction = new ReactionAction(new Emoji("\u0034\u20e3"));
            fourthAction.Action = async (ConfigurationStep a) => 
            {
                var otherStep = await processAtIndex(3);
                a.Result = otherStep ?? step;
                await Task.CompletedTask;
            };
            var fifthAction = new ReactionAction(new Emoji("\u0035\u20e3"));
            fifthAction.Action = async (ConfigurationStep a) => 
            {
                var otherStep = await processAtIndex(4);
                a.Result = otherStep ?? step;
                await Task.CompletedTask;
            };

            var abortDeletionAction = new ReactionAction(new Emoji("❌"));
            abortDeletionAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = parent;
                await Task.CompletedTask;
            };
          
            step.Actions.Add(prevAction);
            step.Actions.Add(firstAction);
            step.Actions.Add(secondAction);
            step.Actions.Add(thirdAction);
            step.Actions.Add(fourthAction);
            step.Actions.Add(fifthAction);
            step.Actions.Add(forwardAction);
            step.Actions.Add(abortDeletionAction);

            step.beforeTextPosted = async (ConfigurationStep a) => 
            {
                a.additionalPosts.Clear();
                for(int i = currentPage * elementOnPage; i < currentPage * elementOnPage + elementOnPage && i < elements.Count; i++)
                {
                    var cmd = elements[i];
                    a.additionalPosts.Add(cmd.display());
                }
                await Task.CompletedTask;
            };
        }

        private async Task<ConfigurationStep> AskForConfirmation(IPaginatable element) 
        {
            var deletionConfirmationStep = new ConfigurationStep("Do you really want to do that?", interactive, context, ConfigurationStep.StepType.Reaction, null);
            var message = await context.Channel.SendMessageAsync($"delete: { element.display() }");
            deletionConfirmationStep.MessagesToRemoveOnNextProgression.Add(message);
            var deniedStep = new ReactionAction(new Emoji("✅"));
            deniedStep.Action = async (ConfigurationStep a) => 
            {
                a.Result = actionOnIndex(element);
                await Task.CompletedTask;
            };

            var confirmStep = new ReactionAction(new Emoji("❌"));
            confirmStep.Action = async (ConfigurationStep a) => 
            {
                a.Result = step;
                await Task.CompletedTask;
            };

            deletionConfirmationStep.Actions.Add(confirmStep);
            deletionConfirmationStep.Actions.Add(deniedStep);

            return deletionConfirmationStep;
        }
    }
    
}