using System;
using System.Diagnostics;
using Topshelf;

namespace Vps.DoctorMonkeyService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                HostFactory.Run(configure =>
                {
                    configure.Service<DoctorMonkeyService>(service =>
                    {
                        service.ConstructUsing(s => new DoctorMonkeyService());
                        service.WhenStarted(s => s.Start());
                        service.WhenStopped(s => s.Stop());
                    });

                    configure.RunAsLocalSystem();
                    configure.SetServiceName("DoctorMonkeyService");
                    configure.SetDisplayName("DoctorMonkeyService");
                    configure.SetDescription("Doctor Monkey Windows Service");
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