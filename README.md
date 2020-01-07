# OnePlusBot
OnePlus bot for the [/r/OnePlus Discord](https://discord.gg/oneplus). 

![Azure DevOps builds](https://img.shields.io/azure-devops/build/rithari/044dda56-4057-4abc-b061-e442d1177f99/1)

# Getting started
Create a new application [here](https://discordapp.com/developers/applications) and create a bot for that application.
Afterwards you need to create a Discord server to test the bot and invite the bot to this server. You can invite the bot, by visiting your discord application and checkout the Oauth2 section. In the scopes there are many options and if you select only 'bot' you will see a link in the lower section which you need to visit.
If this steps are not enough, [these](https://discordpy.readthedocs.io/en/latest/discord.html) explain the necessary steps as well.
To have an instance to try it out, you can use `docker-compose`.
Put your token from the discord bot site and put it in seed_data.sql in the two places and the server id of the server you want to connect in the line with `server_id`.
Execute `docker-compose up --build`. With this you should have a running instance of the bot.

With that you should have a running instance, there are some errors right now (which need an improvement), but the instance should be running and functional.
In order for the posts to individual channels to work and the edit log, you need to fill out the channel ids in `seed_data.sql`.

To change the DB, after the initial setup you need to stop the containers and do `docker container rm mysqlcontainer` and if the `oneplusbot_bot_1` is also present, you can remove it as well, although it should be done when doing `docker-compose up --build` as well.
