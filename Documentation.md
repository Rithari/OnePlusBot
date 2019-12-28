Welcome! You can find here documentation for [/r/oneplus Discord](https://discord.gg/oneplus) server bot.

Its prefix is `;` but commands can also be triggered by pinging bot.\
Note that commands only works in guilds and that as of writing bot doesn't support being present in several servers at same time.

# Features:

## Owner

### Emergency bot shutdown

`;x`\
Terminates bot process.

### SQL commands

`;sql [insert SQL command]`\
Allow owner to edit content of SQL databases directly via a Discord message.

### Update

`;update`
Updates the database with current roles and channels of the server.

## Administration

### Modmail

There exists a category (configurable in database) with the purpose of modmail. When a user dms the bot, a channel in this category the modmail channels will be created and staff role will be pinged. There also exists another separate channel for modmaillog, in which the messages will get stored when the thread has been closed.

Note: while ID of category can be changed in database, it requires command `;reloaddb` to be issued to be taken into effect.

The user will get a response, that the inquiry is being handled and moderators get a notification in modqueue.\
The following commands can only be used within an existing modmail thread and will execute actions in that modmail-thread.\
Staff members can then answer the thread by executing the `;reply` command. This command will then pass on the message to the user and log the message in the current thread as well. This message will be in the form of an embed, with the author being marked.   There also exists the functionality of `;anonreply` for which the author will be replaced with a OP logo.\
The user can then answer in his DM and this message will be passed along to this particular modmail thread and the user will get a reaction, that his message has been processed.\
This conversation can then go on. In case a staff member wants to get pinged in case the user answers, `;subscribe` can be used. If this should be disabled again `;unsubscribe` serves that purpose.\
If a staff member wants to delete/edit a message in the DM `;edit <messageId> <new Text>` and `;delete <messageId>` serve that purpose. This will edit/delete the message in the thread and also update it in the DM with the user.\
When the conversation is over a staff member can then use the `;close <note>` command, which will close the thread and log all  the messages between staff members and the users into the modmaillog channel. The actual thread channel will be deleted.\
There also exists the possibility to use `;disablethread <duration> <note>` (eg `;disablethread 1d2h1m test purpose`). This will also close the thread and make the user unable to contact modmail for the duration set.\
In case the user tries to contact modmail, he will get an answer at which point in time, the user will be able to contact modmail again.

The following commands are available globally.\
Modmail can be disabled for a specific user via `;disableModmail <user>`. After modmail get disabled for a user, he will only get the notification about when he can contact again once. If a staff member wants to contact a user directly, without waiting for the user to write `;contact <user>` serves this purpose. In case there is already a thread going on, an embed will be posted containing a link.
Modmail for a specific user can be enabled again with `;enableModmail <user>`. 

Current implementation only supports one image attachment per message.

Note: Users with **Staff** role can't open modmail threads when they DM bot. It will instead send a message to a channel (configurable in database) which is used to provide anonymous feedback to the admins.
    
### Ban

`;ban @Username#1234 reason`

Ban user from server, reason parameter is optional. Only users with **Staff** role can use that command.\
Command can also be used with [user IDs](https://dis.gd/userid) to ban users not present in server with `;banid` command.\
It is made so that other bots can't be banned with it.\
Bans are logged into #modlog channel.

### Kick

`;kick @Username#1234 reason`

Kick user from server. Only users with **Staff** role can use that command.\
Command can also be used with [user IDs](https://dis.gd/userid).\
It is made so that other bots can't be kicked with it.

### Mute

Mute users in both text and voice chats. It will give them the roles "Voice muted" and "Text muted" (requires prior setup in database). Only users with **Staff** role can use that command.
Syntax is `;mute @Username#1234 duration reason`
Possible durations are days (d), hours (h), minutes (m) and seconds (s).\
Eg `;mute @Username#1234 5d2h1m3s Get muted` to mute user Username#1234 for 5 days, 2 hours, 1 minute and 3 seconds.\
Command can also be used with [user IDs](https://dis.gd/userid).\
It is made so that other bots can't be muted with it.

### Bulk delete

Self explanatory, use `;purge` followed by number of messages you want to prune. Only users with **Staff** role can use that command.\
Currently limited to 100 messages.

### Warnings

Allow moderators (users with **Staff** role) to warn users.\
A warning can be given by using command `;warn @Username#1234 reason` (reason field is optional). Also works with [user IDs](https://dis.gd/userid).\
When someone is warned their warning is named as "active": three active warnings will lead to a server ban (as of writing banning command has to be issued by a moderator).\
Every 90 days (delay configurable in database and taken into account after use of `;reloaddb` command) warnings are decayed so that we can actually keep track of persons infractions.\
The check for that action happens everyday at 00:00 UTC.\

Warnings list can be queried by moderators by issuing `;warnings` command. If list takes more than one embed it is possible to navigate using the arrows reaction. Warning of a specific user can be queried by using command `;warnings @Username#1234` which also works with [user IDs](https://dis.gd/userid). Active warnings and decayed warnings will be showed. Clicking on wastebasket reaction will delete the embed, otherwise the embed will delete by itself after two minutes without interaction with embed by user that requested list of warnings. 

A warning can be manually cleared by a moderator by using command `;clearwarn case_id` (case ID is given with list of warnings).\
Note that however clearing warnings isn't logged in #mod-log channel unlike the automatic decay check and unlike when warns are given.
    
Normal users can check their own warnings by using `;warnings` command, which will tell them how many active and total warnings they have. If they wish to get the reasons as well they need to ask moderators to check for them as it was decided that for "privacy" we won't let other users know why a user was warned. They will automatically receive a message from bot when a warning they had will be decayed (except if they turned off ability to receive direct messages from server members).

### User info

Display user information: status (offline/DND/idle/online), activity (Rich Presence state), Discord account registration date, join server date and their nickname.\
Syntax is `;userinfo @Username#1234`.\
Also works with [user IDs](https://dis.gd/userid) `;userinfo user_id`

### Profanity checker

Checks profane words based on regex present in a database and post a message in a #modqueue channel when use has been detected. This detection is triggered on both posts and edits.\
This message contains the username, discrim and [user ID](https://dis.gd/userid) of user that triggered the filter as well as message content and a jump link to it. It will also tell which type of profanity was detected (they're defined by a label in database). Mind that if message that contained profanity was already reported (which can happen in case of edits), there won't be any duplicated reports.

Note: when regex is updated in database, it needs `;reloaddb` command to be executed to be taken into effect.

Positive and negative triggers are tracked: when an item goes in #modqueue channel, staff members can click on "yes" and "no" reactions. Once a threshold defined in database is reached, message either counts as a profanity or get ignored in database.

It is possible to know the number of false positive and correct profanities by using command `;profanities @Username#1234` (also works with [user IDs](https://dis.gd/userid)). That command is restricted to users with **Staff** role. 

### Illegal character checker

Check username of users joining server so that it triggers a message in #modqueue when a user join with an illegal character at the first position. Bot will automatically react to that message with a :postbox: emoji. Clicking on it will open a modmail thread with that user.

Illegal characters are in a regex stored in database. Changing that regex will require use of `;reloaddb` command for changes to be taken into account.

### Server suggestions

Allow server members to suggest improvements to server.\
Can be used by anyone.\
Syntax is `;suggest stuff`.\
Suggestion will be posted to a #suggestions channel and bot will automatically react to this message using "Yes" and "No" emotes.

### Invite filter

To avoid unsollicited advertising an invite filter is present. Some channels and invitations can however be whitelisted (configurable in database). Changes will require command `;reloaddb` to be issued to be taken into effect.

### Set nickname

Set a new nickname for a user. Reset nickname if parameter is empty. It can only be used by users with **Staff** role.\
Syntax is `;setnickame @Username#1234 nickname`.\
It also works with [user IDs](https://dis.gd/userid).

## Specific to server

### Role ping to send news

Ping News role after execution of command `;news your_text`. Supports one attachment.\
Destination channel can only be an [announcement channel](https://support.discordapp.com/hc/articles/360032008192).\
Can only be used by users with **Journalist** role.\
Last news item can be edited by person that sent it if she edits her command message and if bot didn't restart since news item has been posted.

### FAQ 

Elements stored in a database to answer frequently answered queries.\
Elements can be added/edited/removed by using `;configurefaq` command (command can only be issued by users with **Staff** role).\
FAQ answers can be embeds, textposts, include images (need to provide an URL) and have an authorship attached to them following choices made at configuration.\
Answers are configurable on a per channel basis.
If embeds are chosen during configuration it is possible to set a custom color for them.\
Once changes are made FAQ database need to be reloaded by using `;reloaddb` command (again staff only command).
    
Normal users can use `;faq module` to display answer related to the module.
    
### Timeleft

Currently hardcoded in bot code https://github.com/Rithari/OnePlusBot/blob/master/src/OnePlusBot/Modules/Utility.cs#L369 and disabled.
Tell how much time lefts before an event when someone queries `;timeleft`.\
Known issue: it outputs a negative time when event is over.

### Referral codes handling

Users can post their smartphone and headphones referral links they obtained [from OnePlus](https://www.oneplus.com/referral) in #referralcodes channel. Bot will delete them automatically to post them into an embed with username, discrim and [user ID](https://dis.gd/userid) of who sent the codes. Users are able to post new referral links after two weeks.

### Role attribution

Users can assign themselves roles by reacting to the corresponding emote in a message posted in #info channel. They will lose the role if they remove their reaction. As of writing list of assignable roles (devices, Helper and News) is hardcoded at https://github.com/Rithari/OnePlusBot/blob/master/src/OnePlusBot/Base/RoleReactionAction.cs .\

ID of message mentioned previously is stored in database for the purpose of being able to edit message whenever needed instead of deleting it and reposting it. Changing it will require `;reloaddb` command to be issued.

## Fun

### Starboard

Basically a starboard, users can put a post in a special channel by reacting with :star: emote to it.\
Minimum amount of stars required to put a post in #starboard channel can be set by users with * by issuing command `;setstars integer` (eg: `;setstars 5`). Note that higher levels (only configurable in database) are displayed with a different star emote in #starboard channel. Changing those higher levels require `;reloaddb` command to be issued for changes to be taken into effect.

`;starstats` command is usable by everyone and display statistics of starboard posts.\
Note that system messages (eg: *Username pinned a message*) are ignored by bot and therefore can't go to #starboard channel.

### 8ball

An 8ball command which can be queried by using `;8ball text`.\
Answers can be "It is certain.", "It is decidedly so.", "Without a doubt.", "Yes, definitely.", "You may rely on it.", "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.", "Reply hazy to try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.", "Don't count on it.", "My reply is no.", "Outlook not so good.", "Very doubtful." and "My sources say no."

### Lovecalc

A command which calculates love. It is mostly used between users but can also be with custom text for the first and second parameters.\
Eg `;lovecalc Username Username2` and `;lovecalc "Pastas" "Margarita pizza"`.

### YouTube search

Search for YouTube videos after `;yt search_terms` command is used and outputs first result as a message in the channel in which one command was executed.

### Urban Dictionary definitions

Returns first result of Urban Dictionary definitions.\
Syntax is `;define query`\
It is usable by everyone.

### Steam profile banner displayer

`;steamp steam_id` Example for user https://steamcommunity.com/id/azaza1 command will be `;steamp azaza1`.\
Bot response is an image with the avatar and Steam level. Note that if user queried doesn't exist, bot response will be an empty png file.

## Misc

### Server info

`;serverinfo`\
Returns server information using an embed.\
It includes server name, server ID, server region, owner of the server, number of members, number of text and voice channels, number of roles, server creation date and guild features (defined at https://github.com/discordapp/discord-api-docs/blob/master/docs/resources/Guild.md#guild-features ). Custom emojis number is also displayed in same embed and emojis themselves (including animated ones) are sent as well in embed previously mentionned.

### User avatar display

`;showavatar Username#1234`.
Also works with [user IDs](https://dis.gd/userid).

### Roulette

`;roulette`\
A version of russian roulette.

### Show bigger versions of emotes

Syntax is `;se :your_emote:`\
Supports static and animated emotes.\
[Twemoji](https://github.com/twitter/twemoji) emojis which are used by Discord as native emojis aren't supported.\
It is usable by everyone.

### Echo

This command echo messages.\
Syntax is `;echo your_message`.\
It can only be used by users with **Staff** role

### Ping

This command displays the ping between computer on which one bot is running and Discord servers.\
It can be used by everyone.

### Reminder

Allows you to ask bot to remind you something.\
Syntax is `;remind lenght your_text` with lenght being days, hours, minutes and seconds.\
Eg `;remind 2d1h3m write documentation`\
After executing the remind command, the bot will ping you and inform you of the id this reminder has.\
You can cancel the reminder by using `;unremind remind_ID`

Users can check their active reminders by using command `;reminders`.\
It will list them, give their ID, tell their content, when they are due and add a jump link towards the original reminders.
