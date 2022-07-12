using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Serilog;
using ServiceTrackerTelegramBot;


internal class TelegramMessenger
{
    String TELEGRAM_TOKEN = EnvironmentVariables.TELEGRAM_TOKEN;

    String CHAT_ID = EnvironmentVariables.CHAT_ID;

    TelegramBotClient botClient;

    CancellationTokenSource cts;

    ReceiverOptions receiverOptions;

    // public events
    public event Action<bool> runningStatusChanged;
    public event Action listingRequested;
    public event Action<String> addingRequested;
    public event Action<String> removeingRequested;
    public event Action helpRequested;
    public event Action<int> setTimerRequested;

    bool shouldSendMessages = true;

    public TelegramMessenger()
    {
        botClient = new TelegramBotClient(TELEGRAM_TOKEN);
        cts = new CancellationTokenSource();
        receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        Log.Information("Monitoring is started");
        sendMessageAsync("Monitoring is started.");

    }

    public void StartReceiving()
    {
        Log.Information("Message receiving is started.");

        botClient.StartReceiving(
        updateHandler: HandleUpdateAsync,
        pollingErrorHandler: HandlePollingErrorAsync,
        receiverOptions: receiverOptions,
        cancellationToken: cts.Token);

    }


    public void StopReceiving()
    {
        Log.Information("Message receiving is stopped.");
        cts.Cancel();

    }

    async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Only process Message updates: https://core.telegram.org/bots/api#message
        if (update.Message is not { } message)
            return;
        // Only process text messages
        if (message.Text is not { } messageText)
            return;

        Log.Information($"Message '{messageText}' is received.");

        if (messageText.Equals("Start"))
        {
            runningStatusChanged?.Invoke(true);
            if (shouldSendMessages)
            {
                sendMessageAsync("Monitoring is already started.");
                return;
            }
            shouldSendMessages = true;
            sendMessageAsync("Monitoring is started.");
        }

        if (messageText.Equals("Help"))
        {
            helpRequested?.Invoke();
        }

        if (messageText.Equals("List"))
        {
            listingRequested?.Invoke();

        }

        if (messageText.StartsWith("Add "))
        {
            String formatedText = messageText.Replace("Add ", "");
            addingRequested?.Invoke(formatedText);

        }

        if (messageText.StartsWith("Remove "))
        {
            String formatedText = messageText.Replace("Remove ", "");
            removeingRequested?.Invoke(formatedText);

        }

        if (messageText.StartsWith("Set timer "))
        {
            String formatedText = messageText.Replace("Set timer ", "");
            int seconds;
            try
            {
                seconds = int.Parse(formatedText);
            }
            catch (Exception)
            {
                seconds = 10;
            }

            setTimerRequested?.Invoke(seconds);
        }

        if (messageText.Equals("Stop"))
        {

            if (!shouldSendMessages)
            {
                sendMessageAsync("Monitoring is already stopped.");
                return;
            }
            shouldSendMessages = false;

            sendMessageAsync("Monitoring is stopped.");
            runningStatusChanged?.Invoke(false);

        }

    }

    public async void sendMessageAsync(String textMessage)
    {
        Message message = await botClient.SendTextMessageAsync(
        parseMode: ParseMode.Html,
        chatId: CHAT_ID,
        text: textMessage);

        Log.Information($"Message '{textMessage}' is sent.");

    }

    Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Log.Error(ErrorMessage);
        return Task.CompletedTask;
    }


}


