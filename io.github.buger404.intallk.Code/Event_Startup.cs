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
using VoidLife.Simulator;
using DataArrange.Storages;

namespace io.github.buger404.intallk.Code
{
    public class Event_Startup:ICQStartup
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        // 接收事件
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            MessagePoster.LastOSUTime = DateTime.Now.Hour + 1;
            MessagePoster.workpath = Application.StartupPath;
            ScriptDrawer.AssetsPath = Application.StartupPath + "\\data\\image\\";
            MessagePoster.logid = Guid.NewGuid().ToString();
            MessagePoster.pCQ = e.CQApi;
            MessagePoster.TenClockLock = (DateTime.Now.Hour >= 22);
            VoidLifes.pCQ = e.CQApi;
            ArtificalA.Intelligence.ArtificalAI.DebugLog = true;
            AllocConsole();
            Console.WriteLine("Startup:" + MessagePoster.workpath);
            Manager.wordcollect = new Storage("wordcollections");
            Manager.LCards = new DataArrange("lockcards");
            Manager.mHot = new DataArrange("mosthotmessages");
            Manager.Hots = new DataArrange("messagehots");
            Manager.scrBan = new DataArrange("screenmsgbanners");
            UT.inits();
            MessagePoster.LoadPTemples();
            MessagePoster.LoadFlows();
            Thread thread = new Thread(new ThreadStart(MessagePoster.Poster));//创建线程
            thread.Start();
            VoidLifes.LoadGame();
            Console.WriteLine("Message poster thread works properly .");
        }
    }
}
