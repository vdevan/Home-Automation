using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace HomeAutomation
{
    /// <summary>
    /// Entry point to the application.  Declares a Webfront class and  Instantiates it
    /// Note: the Application is just a background application without any user Interface. This allows us to start
    /// the application whenever the Pi restarts.
    /// </summary>
    public sealed class StartupTask : IBackgroundTask
    {
        private BackgroundTaskDeferral deferral;

        public async void Run (IBackgroundTaskInstance taskInstance)

        {
            deferral = taskInstance.GetDeferral();
            var ws = new Webfront();
            await ThreadPool.RunAsync(wi =>
            {
                ws.Start();
            });
        }

        public void WriteDebugInfo()
        {

        }
    }
}
