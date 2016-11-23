# BookerBot
Demo of a restaurant bot using Microsoft Bot Framework.

####Here’s some examples of what you can ask it: (I’ve trained LUIS so you could ask these in different ways too)
-	Can I make a booking tomorrow at 6pm? (or you can use any other date/time combination, basically it allows you to make a booking)
-	Show me my bookings
-	Cancel a booking
-	What food do you have?
-	What time do you shut today?
-	What’s your address?

**NOTE: LUIS's datetime parsing is still a bit hard to work with so if you want to use times like 6.30pm you'll have to enter 6:30pm instead.

If you want to talk to it on Telegram, create a new chat and look for the username “TheBookerBot”. If you want to talk to it on Facebook Messenger, please let me know your Facebook email and I will add you as a tester so that you can talk to the bot on Messenger. (I have not yet published the bot publicly). You can run it locally too - you’ll need to get the Bot Framework Channel Emulator: https://download.botframework.com/bf-v3/tools/emulator/publish.htm. After you’ve installed the emulator, just download the source code and run it in VS. Make sure the localhost port in the emulator is set correctly, and then you should be able to just talk to it. 
