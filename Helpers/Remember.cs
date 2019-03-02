using JAMTech.Extensions;
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
        public List<Timer> Timers { get; set; } = new List<Timer>();
        public string Uid { get; set; }
        public Models.RememberConfig Config { get; set; }
        public Thread _rememberThread = null;
        readonly int _interval;

        internal bool _exit = false;

        Models.UserRememberConfig User { get; set; }

        public Remember(Models.RememberConfig config, Models.UserRememberConfig user)
        {
            Config = config;
            User = user;
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
            Console.WriteLine($"Starting '{Config.Name}' remember task at {Config.StartDate}");
            //create task
            if(Config.RememberEarlier!=null)
                Config.RememberEarlier.ForEach(earlier =>
                {
                    var milis = GetIntervalInMillis(earlier.Item1, earlier.Item2);
                    Timers.Add(ScheduleTask(Config.StartDate.AddMilliseconds(milis * -1), null, 0, () => RememberNow(false)));
                });
            var intervalInMillis = GetIntervalInMillis(Config.RecurrenceType, Config.RecurrenceValue);
            Timers.Add(ScheduleTask(Config.StartDate, Config.LastRun, intervalInMillis, () => RememberNow()));
            Thread.Sleep(Timeout.Infinite);//wait forever
        }

        private static int GetIntervalInMillis(Models.RememberConfig.RecurrenceTypes recurrenceType, int value)
        {
            var intervalInMillis = 0;
            switch (recurrenceType)
            {
                case Models.RememberConfig.RecurrenceTypes.Minutes:
                    intervalInMillis = value * 60 * 1000;
                    break;
                case Models.RememberConfig.RecurrenceTypes.Hours:
                    intervalInMillis = value * 60 * 60 * 1000;
                    break;
                case Models.RememberConfig.RecurrenceTypes.Days:
                    intervalInMillis = value * 60 * 60 * 24 * 1000;
                    break;
                case Models.RememberConfig.RecurrenceTypes.Weeks:
                    intervalInMillis = value * 60 * 60 * 24 * 7 * 1000;
                    break;
                case Models.RememberConfig.RecurrenceTypes.Months:
                    intervalInMillis = value * 60 * 60 * 24 * 7 * 30 * 1000;
                    break;
                case Models.RememberConfig.RecurrenceTypes.Years:
                    intervalInMillis = value * 60 * 60 * 24 * 7 * 364 * 1000;
                    break;
            }
            return intervalInMillis;
        }

        private async void RememberNow(bool isNow = true)
        {
            if(isNow)
                Console.WriteLine($"Time to remember '{Config.Name}' timer !!");
            //execute action -> Mail, SMS, Push
            switch (Config.Type)
            {
                case Models.RememberConfig.RememberType.Email:
                    if (await RememberByEmail(Config, isNow))
                    {
                        Config.LastRun = DateTime.Now;
                        await UpdateConfig();
                    }
                    break;
                default:
                    throw new NotImplementedException($"Remember action not implemented for type '{Config.Type.ToString()}'");
            }
        }

        static bool disableSendEmail = Environment.GetEnvironmentVariable("disableSendEmail", EnvironmentVariableTarget.Process).Trim().ToLower().Equals("true");
        private static async Task<bool> RememberByEmail(Models.RememberConfig config, bool isNow)
        {
            var subject = $"Hi, this mail is a remembering for '{config.Name}'";
            var to = config.RememberTo.Select(mail => new SendGrid.Helpers.Mail.EmailAddress(mail));
            
            if (!disableSendEmail)
            {
                var response = await Email.SendLegacy(Email.defaultFrom, to.ToList(), subject, config.TemplateId, config.Substitutions);
                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    Console.WriteLine($"Error ocurred when tried to remember '{config.Name}'");
                    return false;
                }
            }
            return true;
        }
        private async Task UpdateConfig()
        {
            User.Data = new List<Models.RememberConfig>() { Config };
            await User.ToMongoDB<Models.UserRememberConfig>(true);
        }
        public Timer ScheduleTask(DateTime initialDate, DateTime? lastRun, int intervalInMilis, Action task)
        {
            var dueTime = (lastRun ?? initialDate + TimeSpan.FromMilliseconds(intervalInMilis)) - DateTime.Now;
            if (dueTime.TotalMilliseconds < 0) dueTime = TimeSpan.FromSeconds(0);
            return new Timer(_ => task.Invoke(), null, dueTime, TimeSpan.FromMilliseconds(intervalInMilis));
        }
    }
}
