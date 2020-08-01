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
using Repeater;
using Native.Csharp.Sdk.Cqp.Model;

namespace io.github.buger404.intallk.Code
{
    public class DreamYCheater
    {
        public static string LastestFight = "";
        public static bool PauseFight = false;
        public static DateTime FightTime;
        public static void AutoFightSign()
        {
            Group dyHander = new Group(MessagePoster.pCQ, 1078432121);
            DateTime d = DateTime.MinValue;
            Storage dy = new Storage("dreamy");
            d = DateTime.Parse(dy.getkey("3529296290", "signintime"));
            dyHander.SendGroupMessage("dreamy 自动刷怪模块启动，已读取dreamy存档，得知最后签到日期为：" + d.ToString());
            dyHander.SendGroupMessage("dy 帮助");
            do { Thread.Sleep(1000); } while (LastestFight == "");
            dyHander.SendGroupMessage("本机器人已悉知最新副本编号为：" + LastestFight);
        Again:
            if((DateTime.Now - d).TotalDays >= 1)
            {
                LastestFight = ""; PauseFight = false;
                dyHander.SendGroupMessage("dy 帮助");
                do { Thread.Sleep(1000); } while (LastestFight == "");
                dyHander.SendGroupMessage("本机器人已悉知最新副本编号为：" + LastestFight);
                d = DateTime.Now;
                dyHander.SendGroupMessage("dy 签到");
            }
            FightTime = DateTime.Now;
            if(!PauseFight) dyHander.SendGroupMessage("dy 挑战" + LastestFight);
            Thread.Sleep(180000);
            goto Again;
        }
    }
    public class Event_Startup:ICQStartup
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        // 接收事件
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            Thread thread;
            Repeaters.LoadInfo();
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
            Thread thread2 = new Thread(new ThreadStart(MessagePoster.Poster));//创建线程
            thread2.Start();
            thread = new Thread(new ThreadStart(Repeaters.AutoSave));
            thread.Start();
            Thread thread3 = new Thread(new ThreadStart(DreamYCheater.AutoFightSign));//创建线程
            thread3.Start();
            VoidLifes.LoadGame();
            Console.WriteLine("Message poster thread works properly .");
            new QQ(e.CQApi, 1361778219).SendPrivateMessage("机器人服务启动成功。");
        }
    }
}
