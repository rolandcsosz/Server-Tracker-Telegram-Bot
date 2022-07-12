using System;
using Serilog;
using Topshelf;


Log.Logger = new LoggerConfiguration()
    .WriteTo.File("log.txt")
    .CreateLogger();


var exitCode = HostFactory.Run(x =>
{

    x.Service<Runner>(s =>
    {
        s.ConstructUsing(serverMonitor => new Runner());
        s.WhenStarted(serverMonitor => serverMonitor.Start());
        s.WhenStopped(serverMonitor => serverMonitor.Stop());

    });

    x.RunAsLocalService();
    x.SetServiceName("ServiceMonitor");
    x.SetDisplayName("ServiceMonitor");
    x.SetDescription("This is a service for tarcking the status of other running services.");

});

int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
Environment.ExitCode = exitCodeValue;




