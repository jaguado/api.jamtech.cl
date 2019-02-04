using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JAMTech.Helpers
{
    public class Remember : IDisposable
    {
        public Timer Timer { get; set; }
        public string Uid { get; set; }
        public Models.RememberConfig Config { get; set; }
        public Thread _rememberThread = null;
        readonly int _interval;

        internal bool _exit = false;

        public Remember(Models.RememberConfig config, string uid)
        {
            Config = config;
            Uid = uid ?? "";
        }

        public void Start()
        {
            _rememberThread = new Thread(Run);
            _rememberThread.Start();
        }

        public void Dispose()
        {
            if (_rememberThread != null)
                _rememberThread.Abort();
            _rememberThread = null;
        }

        private void Run()
        {
            Console.WriteLine($"Starting '{Config.Name}' remember task at {DateTime.Now}");
            //create task
            var intervalInHours = 0;
            switch (Config.RecurrenceType)
            {
                case Models.RememberConfig.RecurrenceTypes.Minutes:
                    intervalInHours = Config.RecurrenceValue / 60;
                    break;
                case Models.RememberConfig.RecurrenceTypes.Hours:
                    intervalInHours = Config.RecurrenceValue;
                    break;
                case Models.RememberConfig.RecurrenceTypes.Days:
                    intervalInHours = Config.RecurrenceValue / 24;
                    break;
                case Models.RememberConfig.RecurrenceTypes.Weeks:
                    intervalInHours = Config.RecurrenceValue / (24 * 7);
                    break;
                case Models.RememberConfig.RecurrenceTypes.Months:
                    intervalInHours = Config.RecurrenceValue / (24 * 30);
                    break;
                case Models.RememberConfig.RecurrenceTypes.Years:
                    intervalInHours = Config.RecurrenceValue / (24 * 364);
                    break;
            }
            Timer = ScheduleTask(Config.StartDate, intervalInHours, ()=>
            {
                //TODO execute action -> Mail, SMS, Push
                Console.WriteLine($"Time to remember '{Config.Name}' timer !!");
            });
            Thread.Sleep(Timeout.Infinite);//wait forever
        }


        public static bool TestRemember(Models.RememberConfig config)
        {
            throw new NotImplementedException();
        }

        public Timer ScheduleTask(DateTime initialDate, double intervalInHour, Action task)
        {
            var now = DateTime.Now;
            var firstRun = initialDate;
            if (now > firstRun)
                firstRun = firstRun.AddDays(1);

            var timeToGo = firstRun - now;
            if (timeToGo <= TimeSpan.Zero)
                timeToGo = TimeSpan.Zero;

            return new Timer(x =>
            {
                task.Invoke();
            }, null, timeToGo, TimeSpan.FromHours(intervalInHour));
        }
    }
}
