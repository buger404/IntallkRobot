using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestoreData.Manager;
using System.Runtime.InteropServices;
using MainThread;
using Undertale.Dialogs;

namespace io.github.buger404.intallk.Code
{
    public class Event_Startup:ICQStartup
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        // 接收事件
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            MessagePoster.pCQ = e.CQApi;
            ArtificalA.Intelligence.ArtificalAI.DebugLog = true;
            AllocConsole();
            Manager.LCards = new DataArrange("lockcards");
            Manager.CPms = new DataArrange("consolepermissions");
            Manager.mHot = new DataArrange("mosthotmessages");
            Manager.Hots = new DataArrange("messagehots");
            Manager.scrBan = new DataArrange("screenmsgbanners");
            UT.inits();
            Thread thread = new Thread(new ThreadStart(MessagePoster.Poster));//创建线程
            thread.Start();
            Console.WriteLine("Message poster thread works properly .");
        }
    }
}
