using System;
using Topshelf;
using Serilog;

namespace MQTT_Client_SM_Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("c:/logs/MQTT_Client_SM.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var serviceCode = HostFactory.Run(x =>
            {
                x.Service<Service>(s =>
                {
                    s.ConstructUsing(service => new Service());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                });

                x.RunAsLocalSystem();
                                
                x.SetServiceName("MQTT_Client_SM_Service");
                x.SetDisplayName("MQTT to System Manager");
                x.SetDescription("Test application to communicate between MQTT and System Manager");

                

            });

            int serviceCodeValue = (int)Convert.ChangeType(serviceCode, serviceCode.GetType());
            Environment.ExitCode = serviceCodeValue;
            if (serviceCodeValue != 0)
            {
                Log.Error("ExitCode: " + serviceCodeValue);
            }
        }
    }
}
