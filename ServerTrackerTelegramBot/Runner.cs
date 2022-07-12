using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using ConsoleTables;
using Serilog;

// Main service which affects the tracked processes and the messages 
internal class Runner
{
    System.Timers.Timer timer;

    TelegramMessanger messenger;

    ServiceChecker serviceChecker;


    public Runner()
    {

        // set timer intervall to 10s
        timer = new System.Timers.Timer(10 * 1000)
        {
            AutoReset = true
        };


        timer.Elapsed += TimerElapsed;

        // init the message sender and the service checker
        messenger = new TelegramMessanger();
        serviceChecker = new ServiceChecker();


        // set events
        messenger.runningStatusChanged += SetRunning;
        messenger.listingRequested += listAllServices;
        messenger.addingRequested += serviceChecker.AddServiceToTheList;
        messenger.removeingRequested += serviceChecker.RemoveServiceFromTheList;
        messenger.setTimerRequested += SetInterval;
        messenger.helpRequested += listCommands;

        // start receiving messeges
        messenger.StartReceiving();


    }

    // set refresh intervall
    void SetInterval(int seconds)
    {
        timer.Interval = seconds * 1000;
        Log.Information($"Service checking intervall was set to: {seconds}sec");
    }

    //list and send to the user all possible commands the bot can interpret
    void listCommands()
    {
        String helpText = "Start - The bot start to monitor the tracked services\n\n" +
            "Stop - the bot stop to monitor the tracked services\n\n" +
            "List - the bot will send the list of the tracked services with their statuses\n\n" +
            "Add [new service] - the bot will add the new service to the tracked list\n\n" +
            "Remove [tracked service] - the bot will remove the service from the tracked list\n\n" +
            "Set timer [seconds] - the bot will update the interval which dictate how often the bot checks the services\n\n" +
            "Help - the bot will send the list of the available commands";
        messenger.sendMessageAsync(helpText);


    }

    //list and send to the user all the tracked processes and their statuses
    void listAllServices()
    {

        String message = BuildMessage("There are the tracked service(s)", serviceChecker.GetServices());
        if (!message.Equals(""))
        {
            messenger.sendMessageAsync(message);
            return;
        }

        messenger.sendMessageAsync("There is no tracked service in the list.");


    }

    //Help to build the reply messages, it takes the caption and the list of the services affected
    //If the list is empty it return an empty string ("")
    String BuildMessage(String caption, List<ServiceInfo> infos)
    {
        var table = new ConsoleTable("Name", "Status");
        if (infos != null)
        {
            if (infos.Count > 0)
            {
                foreach (ServiceInfo info in infos)
                {
                    if (!info.isInsatlled)
                    {
                        table.AddRow(info.Name, "Missing");
                        continue;
                    }

                    table.AddRow(info.Name, info.status.ToString());

                }

                String message = caption + ":\n\n";
                message += "<pre>" + table.ToStringAlternative() + "</pre>";
                return message;
            }
        }

        return "";

    }

    //It sets the running of the main service after a message request
    void SetRunning(bool shouldRun)
    {
        if (shouldRun)
        {
            Start();
            return;
        }

        Stop();
    }

    //It runs every time the main timer is elapsed, it checks if there are any changes from the last check
    void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        String message = BuildMessage("Change(s) occured in the running of the following service(s)", serviceChecker.CheckChangedServices());
        if (!message.Equals(""))
            messenger.sendMessageAsync(message);

    }



    //Starts the monitoring by starting the timer 
    public void Start()
    {
        Log.Information("Service monitoring is started.");

        timer.Start();
    }

    //Stops the monitoring by stopping the timer 
    public void Stop()
    {
        Log.Information("Service monitoring is stopped.");

        timer.Stop();
    }



}
