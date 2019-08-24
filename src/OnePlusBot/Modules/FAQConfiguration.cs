using System;
using Discord.Commands;
using Discord;
using System.Linq;
using System.Threading.Tasks;
using OnePlusBot.Helpers;
using Discord.Addons.Interactive;
using OnePlusBot.Base;
using OnePlusBot.Data;
using OnePlusBot.Data.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;    


namespace OnePlusBot.Modules
{
    public class FAQConfiguration : InteractiveBase<SocketCommandContext>
    {

        [
            Command("configureFAQ", RunMode = RunMode.Async),
            Summary("Configures the faq command entries."),
            RequireRole("staff")
        ]
        public async Task ConfigureFAQ()
        {
            var guild = Global.Bot.GetGuild(Global.ServerID);
            var configurationStep = new ConfigurationStep("What do you want to do? (➕ add command, ➖ remove command in channel, ☠ delete command, ❌ abort)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var addStep = new ConfigurationStep("What kind of post do you want to add? (💌 embed, 📖 textpost, ✅ nothing further)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);

            var aliasesStep = new ConfigurationStep("Please write the aliases you want for this command (comma separated), type 'none' for none", Interactive, Context, ConfigurationStep.StepType.Text, addStep);
            var channelStep = new ConfigurationStep("Which channels should the command be active in? Please mention the channels with #", Interactive, Context, ConfigurationStep.StepType.Text, aliasesStep);

            var commandStep = new ConfigurationStep("What should the name of the command be?", Interactive, Context, ConfigurationStep.StepType.Text, channelStep);
            var textStep = new ConfigurationStep("Please post what the text of the text post should be", Interactive, Context, ConfigurationStep.StepType.Text, addStep);
            var embedStep = new ConfigurationStep("What do you want to configure of your embedPost? (🤖 test current embed, 🖼 set a picture, 📖 set text, 💁 decide authorship, 🎨 choose color, ✅ nothing further)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var colorStep = new ConfigurationStep("What color do you want for your embed?", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var imageUrlStep = new ConfigurationStep("Please enter the url of the Image you want to set", Interactive, Context, ConfigurationStep.StepType.Text, embedStep);
            var embedTextStep = new ConfigurationStep("Please enter the text you want to give your embed", Interactive, Context, ConfigurationStep.StepType.Text, embedStep);
            var embedColorStep = new ConfigurationStep("Please choose a color you want to use for the embed", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var authorStep = new ConfigurationStep("Do you want to be marked as author? (✅ yes, ❌ no)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var deletionStep = new ConfigurationStep("React to the command in the channel you want to remove (❌ to abort, ◀ seek backward, ▶ seek forward)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var commandDeletionStep = new ConfigurationStep("React to the command you want to completely remove (❌ to abort, ◀ seek backward, ▶ seek forward)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            
            var existingCommands = Global.FAQCommands.ToList();
            var existingCommandChannels = new List<FAQCommandChannel>();
            foreach(var comandElement in existingCommands)
            {
                if(comandElement.CommandChannels != null)
                {
                    foreach(var commandChannelelement in comandElement.CommandChannels)
                    {
                        existingCommandChannels.Add(commandChannelelement);
                    }    
                }
                       
            }

            FAQCommand command = new FAQCommand();
            List<FAQCommandChannel> commandChannels = new List<FAQCommandChannel>();
            List<FAQCommandChannelEntry> entries = new List<FAQCommandChannelEntry>();
            var builder = new FaqCommandChannelEntryBuilder();
            builder.defaultValues();
            List<IPaginatable> commandChannelPaginatable = existingCommandChannels.ConvertAll(x => (IPaginatable)x);
            List<IPaginatable> commandPaginatable = existingCommands.ConvertAll(x => (IPaginatable)x);

            var deletePagination = new CommandChannelPagination(deletionStep, configurationStep, commandChannelPaginatable);
            deletePagination.setup();
            deletePagination.actionOnIndex = (object obj) => 
            {
                var channel = obj as FAQCommandChannel;
                using(var db = new Database())
                {
                    db.FAQCommandChannels.Remove(channel);
                    db.SaveChanges();
                }
                existingCommands.Where(com => com.ID == channel.FAQCommandId).First().CommandChannels.Remove(channel);
                commandChannelPaginatable.Remove(obj as IPaginatable);
                return deletionStep;
            };

            var deleteCommandPagination = new CommandChannelPagination(commandDeletionStep, configurationStep, commandPaginatable);
            deleteCommandPagination.setup();
            deleteCommandPagination.actionOnIndex = (object obj) => 
            {
                var commandToDelete = obj as FAQCommand;
                if(obj == null) return commandDeletionStep;
                using(var db = new Database())
                {
                    db.FAQCommands.Remove(commandToDelete); 
                    db.SaveChanges();
                }

                existingCommands.Remove(commandToDelete);
                commandPaginatable.Remove(commandToDelete as IPaginatable);
                return commandDeletionStep;
            };

            aliasesStep.TextCallback = (string text) => 
            {
                if(text != "none")
                {
                    command.Aliases = text;
                }
                else
                {
                    command.Aliases = "";
                }
                return true;
            };
    
            commandStep.TextCallback = (string text) => 
            {
                command.Name = text;
                return true;
            };

            imageUrlStep.TextCallback = (string text) => 
            {
                builder = builder.withImageUrl(text);
                return true;
            };

            embedTextStep.TextCallback = (string text) => 
            {
                builder = builder.withText(text);
                return true;
            };

            textStep.TextCallback = (string text) => 
            {
                builder = builder.withText(text);
                var entry = builder.Build();
                entry.IsEmbed = false;
                entry.Position = (uint) entries.Count();
                entries.Add(entry);
                builder = new FaqCommandChannelEntryBuilder();
                return true;
            };

            channelStep.TextCallback = (string text) => 
            {
                var channelIds = Regex.Matches(text, @"(?:\<#(?<channelId>\d{18})\>)*", 
                RegexOptions.Multiline | 
                RegexOptions.ExplicitCapture)
                  .OfType<Match>()
                  .Select (mt => mt.Groups["channelId"].Value);

                foreach(var channelId in channelIds)
                {
                    if(channelId != string.Empty)
                    {
                        var channelIdLong = ulong.Parse(channelId);
                        var channelObj = Global.FullChannels.Where(ch => ch.ChannelID == channelIdLong).DefaultIfEmpty(null).First();
                        if(channelObj != null)
                        {
                            var commandChannel = new FAQCommandChannel();
                            commandChannel.ChannelId = channelObj.ID;
                            commandChannel.CommandChannelEntries = new List<FAQCommandChannelEntry>();
                            commandChannels.Add(commandChannel);
                        } 

                    }
                }
                return true;
            };

            var addAction = new ReactionAction(new Emoji("➕"));
            addAction.Action = (ConfigurationStep a) => 
            {
                a.Result = commandStep;
                return false;
            };

            var deleteCommandChannelAction = new ReactionAction(new Emoji("➖"));
            deleteCommandChannelAction.Action = (ConfigurationStep a) => 
            {
                a.Result = deletionStep;
                return false;
            };

            var deleteCommandAction = new ReactionAction(new Emoji("☠"));
            deleteCommandAction.Action = (ConfigurationStep a) => 
            {
                a.Result = commandDeletionStep;
                return false;
            };

            var exitAction = new ReactionAction(new Emoji("❌"));
            exitAction.Action = (ConfigurationStep a) => 
            {
                a.Result = null;
                return false;
            };

            configurationStep.Actions.Add(addAction);
            configurationStep.Actions.Add(deleteCommandChannelAction);
            configurationStep.Actions.Add(deleteCommandAction);
            configurationStep.Actions.Add(exitAction);

            // blue
            var blueChoice = new ReactionAction(new Emoji("📘"));
            blueChoice.Action = (ConfigurationStep a) => 
            {
                builder.withHexColor(5614830);
                a.Result = embedStep;
                return false;
            };

            // red
             var redChoice = new ReactionAction(new Emoji("📕"));
            redChoice.Action = (ConfigurationStep a) => 
            {
                builder.withHexColor(14495300);
                a.Result = embedStep;
                return false;
            };

            // green
             var greenChoice = new ReactionAction(new Emoji("📗"));
            greenChoice.Action = (ConfigurationStep a) => 
            {
                builder.withHexColor(7844437);
                a.Result = embedStep;
                return false;
            };

            // yellow
             var yellowChoice = new ReactionAction(new Emoji("📙"));
            yellowChoice.Action = (ConfigurationStep a) => 
            {
                builder.withHexColor(16755763);
                a.Result = embedStep;
                return false;
            };

            // white
             var whiteChoice = new ReactionAction(new Emoji("🔖"));
            whiteChoice.Action = (ConfigurationStep a) => 
            {
                builder.withHexColor(14805229);
                a.Result = embedStep;
                return false;
            };

            // black
             var blackChoice = new ReactionAction(new Emoji("⬛"));
            blackChoice.Action = (ConfigurationStep a) => 
            {
                builder.withHexColor(2699059);
                a.Result = embedStep;
                return false;
            };

            colorStep.Actions.Add(blueChoice);
            colorStep.Actions.Add(redChoice);
            colorStep.Actions.Add(greenChoice);
            colorStep.Actions.Add(yellowChoice);
            colorStep.Actions.Add(whiteChoice);
            colorStep.Actions.Add(blackChoice);

            var imageUrlAction = new ReactionAction(new Emoji("🖼"));
            imageUrlAction.Action = (ConfigurationStep a) => 
            {
                a.Result = imageUrlStep;
                return false;
            };

            var embedTextAction = new ReactionAction(new Emoji("📖"));
            embedTextAction.Action = (ConfigurationStep a) => 
            {
                a.Result = embedTextStep;
                return false;
            };

            var chooseColorAction = new ReactionAction(new Emoji("🎨"));
            chooseColorAction.Action = (ConfigurationStep a) => 
            {
                a.Result = colorStep;
                return false;
            };

            var testEmbedAction = new ReactionAction(new Emoji("🤖"));
            testEmbedAction.Action =  (ConfigurationStep a ) => 
            {
                // TODO delete the embed afterwards, we cant right now, because we would need to await it
                Context.Channel.SendMessageAsync(embed: OnePlusBot.Helpers.Extensions.FaqCommandEntryToBuilder(builder.Build()).Build());
                a.Result = embedStep;
                return false;
            };

            var authorSettingAction = new ReactionAction(new Emoji("💁"));
            authorSettingAction.Action = (ConfigurationStep a) => 
            {
                a.Result = authorStep;
                return false;
            };

            var finishEmbedAction = new ReactionAction(new Emoji("✅"));
            finishEmbedAction.Action = (ConfigurationStep a) => 
            {
                a.Result = addStep;
                var entry = builder.Build();
                entry.Position = (uint) entries.Count();
                entry.IsEmbed = true;
                entries.Add(entry);                
                builder = new FaqCommandChannelEntryBuilder();
                return false;
            };

            var abortEmbedAction = new ReactionAction(new Emoji("❌"));
            abortEmbedAction.Action = (ConfigurationStep a) => 
            {
                a.Result = addStep;
                return false;
            };

            embedStep.Actions.Add(testEmbedAction);
            embedStep.Actions.Add(imageUrlAction);
            embedStep.Actions.Add(embedTextAction);
            embedStep.Actions.Add(chooseColorAction);
            embedStep.Actions.Add(authorSettingAction);
            embedStep.Actions.Add(finishEmbedAction);

            var authorAgreeAction = new ReactionAction(new Emoji("✅"));
            authorAgreeAction.Action = (ConfigurationStep a) => 
            {
                builder = builder.withAuthor(Context.Message.Author.Username);
                builder = builder.withAuthorAvatarUrl(Context.Message.Author.GetAvatarUrl());
                a.Result = embedStep;
                return false;
            };

            var authorDenyAction = new ReactionAction(new Emoji("❌"));
            authorDenyAction.Action = (ConfigurationStep a) => 
            {
                a.Result = embedStep;
                // TODO do not hardcode
                builder = builder.withAuthor("r/Oneplus");
                builder = builder.withAuthorAvatarUrl("https://cdn.discordapp.com/avatars/426015562595041280/cab7dde68e8da9bcfd61842bd98e950b.png");
                return false;
            };

            authorStep.Actions.Add(authorAgreeAction);
            authorStep.Actions.Add(authorDenyAction);

            var embedAction = new ReactionAction(new Emoji("💌"));
            embedAction.Action = (ConfigurationStep a) => 
            {
                builder.defaultValues();
                a.Result = embedStep;
                return false;
            };

            var textAction = new ReactionAction(new Emoji("📖"));
            textAction.Action = (ConfigurationStep a) => 
            {
                a.Result = textStep;
                return false;
            };

            var commandFinished = new ReactionAction(new Emoji("✅"));
            commandFinished.Action = (ConfigurationStep a ) => 
            {

                // run this in parallel, so it doesnt block, should be fast enough in order for any additional configuration to not happen yet from the user
                Task.Run(() => {
                    foreach(var entry in entries){
                        foreach(var commandChannel in commandChannels){
                            commandChannel.CommandChannelEntries.Add(entry.clone());
                        }
                    }
                    command.CommandChannels = commandChannels;
                    using (var db = new Database()){
                        db.FAQCommands.Add(command);
                        db.SaveChanges();
                     }
                    command = new FAQCommand();
                    commandChannels = new List<FAQCommandChannel>();
                    entries = new List<FAQCommandChannelEntry>();
                });
              
                a.Result = configurationStep;
                return false;
            };

            addStep.Actions.Add(embedAction);
            addStep.Actions.Add(textAction);
            addStep.Actions.Add(commandFinished);
            
            await configurationStep.SetupMessage();
        }

    }

    public class CommandChannelPagination {
        public ConfigurationStep step { get; set; }
        public ConfigurationStep parent { get; set; }

        public System.Collections.Generic.List<IPaginatable> elements { get; set;}
        private int currentPage = 0;
        private int elementOnPage = 5;

        public Func<object, ConfigurationStep> actionOnIndex { get; set; } 


        public CommandChannelPagination(ConfigurationStep step, ConfigurationStep parent, System.Collections.Generic.List<IPaginatable> elements)
        {
            this.step = step;
            this.parent = parent;
            this.elements = elements;
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
                return false;
            };
            var forwardAction = new ReactionAction(new Emoji("▶"));

            forwardAction.Action = (ConfigurationStep a ) => 
            {
                if(currentPage < (elements.Count - 1) / elementOnPage)
                {
                    currentPage++;
                }
                a.Result = step;
                return false;
            };

            Func<int, ConfigurationStep> processAtIndex = (int index) => 
            {
                index = currentPage * elementOnPage + index;
                if(index < elements.Count)
                {
                    var channel = elements[index];
                    return actionOnIndex(channel);
                }
                return null;
            };
            var firstAction = new ReactionAction(new Emoji("\u0031\u20e3"));
            firstAction.Action = (ConfigurationStep a) => 
            {
                var otherStep = processAtIndex(0);
                a.Result = otherStep ?? step;
                return false;
            };
            var secondAction = new ReactionAction(new Emoji("\u0032\u20e3"));
            secondAction.Action = (ConfigurationStep a) => 
            {
                var otherStep = processAtIndex(1);
                a.Result = otherStep ?? step;
                a.Result = step;
                return false;
            };
            var thirdAction = new ReactionAction(new Emoji("\u0033\u20e3"));
            thirdAction.Action = (ConfigurationStep a) => 
            {
                 var otherStep = processAtIndex(2);
                a.Result = otherStep ?? step;
                return false;
            };
            var fourthAction = new ReactionAction(new Emoji("\u0034\u20e3"));
            fourthAction.Action = (ConfigurationStep a) => 
            {
                 var otherStep = processAtIndex(3);
                a.Result = otherStep ?? step;
                return false;
            };
            var fifthAction = new ReactionAction(new Emoji("\u0035\u20e3"));
            fifthAction.Action = (ConfigurationStep a) => 
            {
                 var otherStep = processAtIndex(4);
                a.Result = otherStep ?? step;
                return false;
            };

            var abortDeletionAction = new ReactionAction(new Emoji("❌"));
            abortDeletionAction.Action = (ConfigurationStep a) => 
            {
                a.Result = parent;
                return false;
            };
          
            step.Actions.Add(prevAction);
            step.Actions.Add(firstAction);
            step.Actions.Add(secondAction);
            step.Actions.Add(thirdAction);
            step.Actions.Add(fourthAction);
            step.Actions.Add(fifthAction);
            step.Actions.Add(forwardAction);
            step.Actions.Add(abortDeletionAction);

            step.beforeTextPosted = (ConfigurationStep a) => 
            {
                a.additionalPosts.Clear();
                for(int i = currentPage * elementOnPage; i < currentPage * elementOnPage + elementOnPage && i < elements.Count; i++)
                {
                    var cmd = elements[i];
                    a.additionalPosts.Add(cmd.display());
                }
                return false;
            };
        }
    }
}