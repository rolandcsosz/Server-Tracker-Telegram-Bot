using System;
using Serilog;
using Topshelf;

// logging into => log.txt
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt")
    .CreateLogger();

// service configuration and running
var exitCode = HostFactory.Run(x =>
{

    x.Service<Runner>(s =>
    {
        s.ConstructUsing(serverMonitor => new Runner());
        s.WhenStarted(serverMonitor => serverMonitor.Start());
        s.WhenStopped(serverMonitor => serverMonitor.Stop());

    });

    // service informations
    x.RunAsLocalService();
    x.SetServiceName("ServiceMonitor");
    x.SetDisplayName("ServiceMonitor");
    x.SetDescription("This is a service for tarcking the status of other running services.");

});


//exit of the running
int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
Environment.ExitCode = exitCodeValue;




