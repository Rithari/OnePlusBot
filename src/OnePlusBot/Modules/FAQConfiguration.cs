using System.Text;
using System.Reflection.Metadata;
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
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;    


namespace OnePlusBot.Modules
{
    [
      Summary("Module containing the command to configure the faq commands.")
    ]
    public class FAQConfiguration : InteractiveBase<SocketCommandContext>
    {

        [
            Command("configureFAQ", RunMode = RunMode.Async),
            Summary("Configures the faq command entries."),
            RequireRole("staff"),
            CommandDisabledCheck
        ]
        public async Task ConfigureFAQ()
        {
            var guild = Global.Bot.GetGuild(Global.ServerID);
            var configurationStep = new ConfigurationStep("What do you want to do? (➕ add command, ➖ remove command in channel group, ☠ delete command)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            configurationStep.additionalPosts.Add("To exit react with 🆘 or type exit, depending on the type of step you are in");
            var addStep = new ConfigurationStep("What kind of post do you want to add? (💌 embed, 📖 textpost, ✅ nothing further)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);

            var aliasesStep = new ConfigurationStep("Please write the aliases you want for this command (comma separated), type 'none' for none", Interactive, Context, ConfigurationStep.StepType.Text, addStep);
            var channelStep = new ConfigurationStep("In which channel groups should the command be active in?", Interactive, Context, ConfigurationStep.StepType.Text, aliasesStep);

            var commandStep = new ConfigurationStep("What should the name of the command be?", Interactive, Context, ConfigurationStep.StepType.Text, channelStep);
            var textStep = new ConfigurationStep("Please post what the text of the text post should be", Interactive, Context, ConfigurationStep.StepType.Text, addStep);
            var embedStep = new ConfigurationStep("What do you want to configure of your embedPost? (🤖 test current embed, 🖼 set a picture, 📖 set text, 💁 decide authorship, 🎨 choose color, ✅ nothing further)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var colorStep = new ConfigurationStep("What color do you want for your embed?", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var imageUrlStep = new ConfigurationStep("Please enter the url of the Image you want to set", Interactive, Context, ConfigurationStep.StepType.Text, embedStep);
            var embedTextStep = new ConfigurationStep("Please enter the text you want to give your embed", Interactive, Context, ConfigurationStep.StepType.Text, embedStep);
            var embedColorStep = new ConfigurationStep("Please choose a color you want to use for the embed", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var authorStep = new ConfigurationStep("Do you want to be marked as author? (✅ yes, ❌ no)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var deletionStep = new ConfigurationStep("React to the command in the channel you want to remove (❌ go back, ◀ seek backward, ▶ seek forward)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var commandDeletionStep = new ConfigurationStep("React to the command you want to completely remove (❌ go back, ◀ seek backward, ▶ seek forward)", Interactive, Context, ConfigurationStep.StepType.Reaction, null);
            var chooseExistingEntryStep = new ConfigurationStep("Which post do you want to use?", Interactive, Context, ConfigurationStep.StepType.Text, addStep);

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

            var deletePagination = new PaginationWithAction(deletionStep, configurationStep, commandChannelPaginatable, true, Context, Interactive);
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

            var deleteCommandPagination = new PaginationWithAction(commandDeletionStep, configurationStep, commandPaginatable, true, Context, Interactive);
            deleteCommandPagination.setup();
            deleteCommandPagination.actionOnIndex = (object obj) => 
            {
                var commandToDelete = obj as FAQCommand;
                if(obj == null) return commandDeletionStep;
                commandChannelPaginatable.RemoveAll(ch => (ch as FAQCommandChannel).Command.ID == commandToDelete.ID);
                using(var db = new Database())
                {
                    db.FAQCommands.Remove(commandToDelete); 
                    db.SaveChanges();
                }

                existingCommands.Remove(commandToDelete);
                commandPaginatable.Remove(commandToDelete as IPaginatable);
                // if we do not remove them from the channel commands, then a the command is a null pointer
                return commandDeletionStep;
            };

            aliasesStep.TextCallback = (string text, ConfigurationStep a) => 
            {
                var editingAlias = command.Aliases != null;
                if(text != "none")
                {
                    if(editingAlias)
                    {
                        command.Aliases = command.Aliases + ',' + text; 
                    }
                    else 
                    {
                        command.Aliases = text;
                    }
                }
                else
                {
                    if(!editingAlias)
                    {
                        command.Aliases = "";
                    }                
                }
                return Task.CompletedTask;
            };
    
            commandStep.TextCallback = (string text, ConfigurationStep a) => 
            {
                var weGotOne = existingCommands.Where(cmd => cmd.Name == text);
                if(weGotOne.Any())
                {
                    command = weGotOne.First();
                }
                else
                {
                     command.Name = text;
                }
                return Task.CompletedTask;
            };

            imageUrlStep.TextCallback = (string text, ConfigurationStep a) => 
            {
                builder = builder.withImageUrl(text);
                return Task.CompletedTask;
            };

            embedTextStep.TextCallback = (string text, ConfigurationStep a) => 
            {
                builder = builder.withText(text);
                return Task.CompletedTask;
            };

            textStep.TextCallback = (string text, ConfigurationStep a) => 
            {
                builder = builder.withText(text);
                var entry = builder.Build();
                entry.IsEmbed = false;
                entry.Position = (uint) entries.Count();
                entries.Add(entry);
                builder = new FaqCommandChannelEntryBuilder();
                return Task.CompletedTask;
            };

            channelStep.beforeTextPosted = async (ConfigurationStep step) => {
                var embeds = new ChannelManager().GetChannelListEmbed(ChannelGroupType.FAQ);
                foreach(var embed in embeds)
                {
                    var postedMessage = await Context.Channel.SendMessageAsync(embed: embed);
                    channelStep.MessagesToRemoveOnNextProgression.Add(postedMessage);
                    await Task.Delay(100);
                }
                
                await Task.CompletedTask;
            };

            channelStep.TextCallback = (string text, ConfigurationStep a) => 
            {
                using(var db = new Database())
                {
                    var groupNames = text.Split(' ');
                    bool foundAnyMatchingGroup = false;
                    foreach(var groupName in groupNames){
                        var channelGroup = db.ChannelGroups.Where(ch => ch.Name == groupName && ch.ChannelGroupType == ChannelGroupType.FAQ).FirstOrDefault();

                        if(channelGroup != null)
                        {
                           foundAnyMatchingGroup = true;
                           a.Result = aliasesStep;
                        }
                        else
                        {
                            continue;
                        }

                        var existingCommand = db.FAQCommandChannels
                            .Include(faqComand => faqComand.Command)
                            .Include(faqComand => faqComand.ChannelGroupReference)
                            .Where(c => c.Command.Name == command.Name)
                            .Where(c => c.ChannelGroupReference.Name == groupName).FirstOrDefault();
                    
                        if(existingCommand == null)
                        {
                            var commandChannel = new FAQCommandChannel();
                            commandChannel.ChannelGroupId = channelGroup.Id;
                            commandChannel.CommandChannelEntries = new List<FAQCommandChannelEntry>();
                            commandChannels.Add(commandChannel);
                        } 
                    }

                    if(!foundAnyMatchingGroup)
                    {
                        a.Result = channelStep;
                    }
                   
                }
                return Task.CompletedTask;
            };

            chooseExistingEntryStep.TextCallback = (string text, ConfigurationStep a) => 
            {
                var channelCommands = existingCommands.Where(c => c.Name == command.Name).First();

                int index = 0;
                if(int.TryParse(text, out index))
                {
                    // it is 1 based, therefore -1
                    var entriesChosen = channelCommands.CommandChannels.ToList()[index - 1];
                    foreach(var neededEntry  in entriesChosen.CommandChannelEntries)
                    {
                        entries.Add(neededEntry.clone());
                    }
                }
                else
                {
                    a.Result = chooseExistingEntryStep;
                }
                   
                return Task.CompletedTask;
               
            };

            var addAction = new ReactionAction(new Emoji("➕"));
            addAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = commandStep;
                await Task.CompletedTask;
            };

            var deleteCommandChannelAction = new ReactionAction(new Emoji("➖"));
            deleteCommandChannelAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = deletionStep;
                await Task.CompletedTask;
            };

            var deleteCommandAction = new ReactionAction(new Emoji("☠"));
            deleteCommandAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = commandDeletionStep;
                await Task.CompletedTask;
            };

            configurationStep.Actions.Add(addAction);
            configurationStep.Actions.Add(deleteCommandChannelAction);
            configurationStep.Actions.Add(deleteCommandAction);
            // blue
            var blueChoice = new ReactionAction(new Emoji("📘"));
            blueChoice.Action = async (ConfigurationStep a) => 
            {
                builder.withHexColor(5614830);
                a.Result = embedStep;
                await Task.CompletedTask;
            };

            // red
             var redChoice = new ReactionAction(new Emoji("📕"));
            redChoice.Action = async (ConfigurationStep a) => 
            {
                builder.withHexColor(14495300);
                a.Result = embedStep;
                 await Task.CompletedTask;
            };

            // green
             var greenChoice = new ReactionAction(new Emoji("📗"));
            greenChoice.Action = async (ConfigurationStep a) => 
            {
                builder.withHexColor(7844437);
                a.Result = embedStep;
                await Task.CompletedTask;
            };

            // yellow
             var yellowChoice = new ReactionAction(new Emoji("📙"));
            yellowChoice.Action = async (ConfigurationStep a) => 
            {
                builder.withHexColor(16755763);
                a.Result = embedStep;
                await Task.CompletedTask;
            };

            // white
             var whiteChoice = new ReactionAction(new Emoji("🔖"));
            whiteChoice.Action = async (ConfigurationStep a) => 
            {
                builder.withHexColor(14805229);
                a.Result = embedStep;
                await Task.CompletedTask;
            };

            // black
             var blackChoice = new ReactionAction(new Emoji("⬛"));
            blackChoice.Action = async (ConfigurationStep a) => 
            {
                builder.withHexColor(2699059);
                a.Result = embedStep;
                await Task.CompletedTask;
            };

            colorStep.Actions.Add(blueChoice);
            colorStep.Actions.Add(redChoice);
            colorStep.Actions.Add(greenChoice);
            colorStep.Actions.Add(yellowChoice);
            colorStep.Actions.Add(whiteChoice);
            colorStep.Actions.Add(blackChoice);

            var imageUrlAction = new ReactionAction(new Emoji("🖼"));
            imageUrlAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = imageUrlStep;
                 await Task.CompletedTask;
            };

            var embedTextAction = new ReactionAction(new Emoji("📖"));
            embedTextAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = embedTextStep;
                await Task.CompletedTask;
            };

            var chooseColorAction = new ReactionAction(new Emoji("🎨"));
            chooseColorAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = colorStep;
               await Task.CompletedTask;
            };

            var testEmbedAction = new ReactionAction(new Emoji("🤖"));
            testEmbedAction.Action = async (ConfigurationStep a) => 
            {
                var message = await Context.Channel.SendMessageAsync(embed: OnePlusBot.Helpers.Extensions.FaqCommandEntryToBuilder(builder.Build()).Build());
                a.MessagesToRemoveOnNextProgression.Add(message);
                a.Result = embedStep;
                await Task.CompletedTask;
            };

            var authorSettingAction = new ReactionAction(new Emoji("💁"));
            authorSettingAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = authorStep;
                await Task.CompletedTask;
            };

            var finishEmbedAction = new ReactionAction(new Emoji("✅"));
            finishEmbedAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = addStep;
                var entry = builder.Build();
                entry.Position = (uint) entries.Count();
                entry.IsEmbed = true;
                entries.Add(entry);                
                builder = new FaqCommandChannelEntryBuilder();
                await Task.CompletedTask;
            };

            var abortEmbedAction = new ReactionAction(new Emoji("❌"));
            abortEmbedAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = addStep;
                await Task.CompletedTask;
            };

            embedStep.Actions.Add(testEmbedAction);
            embedStep.Actions.Add(imageUrlAction);
            embedStep.Actions.Add(embedTextAction);
            embedStep.Actions.Add(chooseColorAction);
            embedStep.Actions.Add(authorSettingAction);
            embedStep.Actions.Add(finishEmbedAction);

            var authorAgreeAction = new ReactionAction(new Emoji("✅"));
            authorAgreeAction.Action = async (ConfigurationStep a) => 
            {
                builder = builder.withAuthor(Context.Message.Author.Username);
                builder = builder.withAuthorAvatarUrl(Context.Message.Author.GetAvatarUrl());
                a.Result = embedStep;
                await Task.CompletedTask;
            };

            var authorDenyAction = new ReactionAction(new Emoji("❌"));
            authorDenyAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = embedStep;
                // TODO do not hardcode
                builder = builder.withAuthor("r/Oneplus");
                builder = builder.withAuthorAvatarUrl("https://cdn.discordapp.com/avatars/426015562595041280/cab7dde68e8da9bcfd61842bd98e950b.png");
                await Task.CompletedTask;
            };

            authorStep.Actions.Add(authorAgreeAction);
            authorStep.Actions.Add(authorDenyAction);

            var embedAction = new ReactionAction(new Emoji("💌"));
            embedAction.Action = async (ConfigurationStep a) => 
            {
                builder.defaultValues();
                a.Result = embedStep;
                await Task.CompletedTask;
            };

            var textAction = new ReactionAction(new Emoji("📖"));
            textAction.Action = async (ConfigurationStep a) => 
            {
                a.Result = textStep;
                await Task.CompletedTask;
            };

            var commandFinished = new ReactionAction(new Emoji("✅"));
            commandFinished.Action = async (ConfigurationStep a ) => 
            {
                if(entries.Count == 0)
                {
                    if(command.ID != 0)
                    {
                        var channelCommands = existingCommands.Where(c => c.Name == command.Name).First();
                        if(!AreAllEntriesTheSame(channelCommands.CommandChannels.ToList()))
                        {
                            chooseExistingEntryStep.beforeTextPosted = (ConfigurationStep step) => 
                            {
                                var stringBuilder = new StringBuilder();
                                var index = 1;
                                foreach(var commandInChannel in channelCommands.CommandChannels)
                                {
                                    stringBuilder.Append($"{index}: {commandInChannel.Command.Name} in {commandInChannel.ChannelGroupReference.Name}" + Environment.NewLine);
                                    index++;
                                }
                                step.additionalPosts.Clear();
                                step.additionalPosts.Add(stringBuilder.ToString());
                                return Task.CompletedTask;
                            };
                            a.Result = chooseExistingEntryStep;
                            return;
                        }
                        else 
                        {
                           var entriesToUse = channelCommands.CommandChannels.First().CommandChannelEntries;
                           foreach(var entry in entriesToUse)
                           {
                               entries.Add(entry.clone());
                           }
                        }
                    }
                }

                // run this in parallel, so it doesnt block, should be fast enough in order for any additional configuration to not happen yet from the user
                await Task.Run(() => 
                {
                    foreach(var entry in entries){
                        foreach(var commandChannel in commandChannels)
                        {
                            commandChannel.CommandChannelEntries.Add(entry.clone());
                        }
                    }
                    command.CommandChannels = commandChannels;
                    using (var db = new Database())
                    {
                        if(command.ID == 0)
                        {
                            db.FAQCommands.Add(command);
                        } 
                        else 
                        {   
                            db.Entry(command).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                            foreach(var channel in command.CommandChannels)
                            {
                                channel.FAQCommandId = command.ID;
                                if(channel.CommandChannelId == 0)
                                {
                                    db.Entry(channel).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                                }
                                foreach(var entry in channel.CommandChannelEntries)
                                {
                                     db.Entry(entry).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                                }
                            }
                        }
                        
                        db.SaveChanges();
                    }
                    command = new FAQCommand();
                    commandChannels = new List<FAQCommandChannel>();
                    entries = new List<FAQCommandChannelEntry>();
                });
              
                a.Result = configurationStep;
                await Task.CompletedTask;
            };

            addStep.Actions.Add(embedAction);
            addStep.Actions.Add(textAction);
            addStep.Actions.Add(commandFinished);
            
            await configurationStep.SetupMessage();
        }

         public bool AreAllEntriesTheSame(List<FAQCommandChannel> channels)
         {
            List<FAQCommandChannelEntry> entriesToCompareWith = channels.First().CommandChannelEntries.ToList();
            foreach(var channel in channels)
            {
                var channelEntries = channel.CommandChannelEntries.ToList();
                if(channelEntries.Count != entriesToCompareWith.Count)
                {
                    return false;
                }
                for(var entryIndex = 0; entryIndex < channelEntries.Count; entryIndex++) 
                {
                    var referenceEntry = entriesToCompareWith[entryIndex];
                    var entry = channelEntries[entryIndex];
                    if(!referenceEntry.Equals(entry))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}