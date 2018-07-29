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
