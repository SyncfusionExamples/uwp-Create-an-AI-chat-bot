# Create-an-AI-chat-bot
This repository contains information on how to create an AI chat bot using LUIS and Microsoft bot framework with Azure portal.

Ensure to do the following for proper working of the bot.

1. Go to folder With LUIS -> Open `appSettings.JSON` file. Enter the required id's and keys of the LUIS APP you created.
2. Go to folder Bot with SfChat -> Open folder Model-> Open file -> BotService.cs -> Find the variable `directLineKey` and enter your Azure Bot service's key from the channels menu in your Azure Bot service portal.
