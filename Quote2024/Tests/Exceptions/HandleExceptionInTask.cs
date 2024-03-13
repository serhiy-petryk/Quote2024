using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Tests.Exceptions
{
    // see 'genveir' comment in https://gist.github.com/morgankenyon/686b8004932be1d8e02356fb6b652cfc
    public static class HandleExceptionInTask
    {
        public static async void Start()
        {
            var pingTasks = new List<IPing>()
            {
                new WorkingPing(),
                new BrokenPing(),
                new WorkingPing(),
                new BrokenPing()
            };
            var pingResult = new List<bool>();

            Debug.Print($"{DateTime.Now.TimeOfDay}. Start");
            var sw = new Stopwatch();
            sw.Start();

            // enumerate the tasks immediately
            var tasks = pingTasks.Select(p => p.Ping()).ToList();

            foreach (var task in tasks)
            {
                try
                {
                    // release control to the caller until the task is done, which will be near immediate for each task following the first
                    pingResult.Add(await task);
                }
                catch (Exception e)
                {
                    Debug.Print($"{DateTime.Now.TimeOfDay}. Error says {e.Message}");
                }
            }
            sw.Stop();
            Debug.Print($"{DateTime.Now.TimeOfDay}. SW: {sw.ElapsedMilliseconds:N0} milliseconds");
        }

        #region ==========  Subclasses  ===========
        public interface IPing
        {
            Task<bool> Ping();
        }

        public class WorkingPing : IPing
        {
            public async Task<bool> Ping()
            {
                await Task.Delay(3000);
                return true;
            }
        }

        public class BrokenPing : IPing
        {
            public async Task<bool> Ping()
            {
                throw new System.Net.WebException("Ping Broken");
            }
        }
        #endregion
    }
}
