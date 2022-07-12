# Server Monitoring Telegram Bot
**A self-hosted service application to inform the user via Telegram API about status changes of process or running services on a home server.**

## Features

These features are available via Telegram messages:

- [x] List all commands available (Help)
- [x] Start monitoring (Start)
- [x] Adding a new service to the list via Telegram (Add [New service]) 
- [x] Remove service from the list via Telegram (Remove [Old service])
- [x] List all tracked processes and statuses (List)
- [x] Set checking interval in seconds (Set timer [seconds])
- [x] Stop monitoring (Stop)



## Deploying


### Installation

- Clone this git repository.
```sh 
git clone https://github.com/rolandcsosz/Server-Tracker-Telegram-Bot.git
```
- Change Directory
```sh 
cd Server-Tracker-Telegram-Bot
```


### Configuration

Add values to Environment Variables by adding a `EnvironmentVariables` class to the project like this: 

```
class EnvironmentVariables
{
        public static String TELEGRAM_TOKEN = "";

        public static String CHAT_ID = "";

        public static String LOG_FILE = "log.txt";

        public static String SERVICE_LIST_FILE = "services.json";
}
```

### Configuration Values
- `TELEGRAM_TOKEN` - Get it by contacting to [BotFather](https://t.me/botfather)
- `CHAT_ID` - Get it by contacting to [RawDataBot](https://telegram.me/rawdatabot)
- `LOG_FILE` - This will be the path for the logging file.
- `SERVICE_LIST_FILE` - This will be the path for the file where process names will be stored.


### Deploy 
- Build the project.
```sh 
dotnet build
```
- Run the solution.
```sh 
dotnet run
```

## Credits
- [Khalid](https://github.com/khalidabuhakmeh) for creating [ConsoleTables](https://github.com/khalidabuhakmeh/ConsoleTables)

##  Copyright & License

- Copyright (©) 2022 by [Csősz Roalnd](https://github.com/rolandcsosz)

- Licensed under the terms of the [GNU GENERAL PUBLIC LICENSE Version 2, June 1991](./LICENSE)

