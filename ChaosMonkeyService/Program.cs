using System;
using System.Diagnostics;
using Topshelf;

namespace ChaosMonkeyService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                HostFactory.Run(configure =>
                {
                    configure.Service<ChaosMonkeyService>(service =>
                    {
                        service.ConstructUsing(s => new ChaosMonkeyService());
                        service.WhenStarted(s => s.Start());
                        service.WhenStopped(s => s.Stop());
                    });

                    configure.RunAsLocalSystem();
                    configure.SetServiceName("ChaosMonkeyService");
                    configure.SetDisplayName("ChaosMonkeyService");
                    configure.SetDescription("Chaos Monkey Windows Service");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                EventLog.WriteEntry("Application", ex.ToString(), EventLogEntryType.Error);
            }
        }
    }
}
