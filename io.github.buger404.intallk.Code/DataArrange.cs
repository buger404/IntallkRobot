using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using RestoreData.Manager;
using System.Runtime.InteropServices;
using Undertale.Dialogs;
using DataArrange.Storages;
using Native.Csharp.Sdk.Cqp.Model;

namespace MainThread
{
    public static class MessagePoster
    {
        [DllImport("kernel32.dll",
            CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();
        public static bool TenClockLock = false;
        public static CQApi pCQ;
        public static string logid = "";
        public struct HotMsg
        {
            public long id;
            public string msg;
            public int hot;
            public long group;
            public string qq;
            public string banqq;
            public bool delaymsg;
            public bool hasup;
            public bool hasre;
            public bool canre;
        }
        public static int recordtime = 0;
        public static string workpath;
        public struct delaymsg
        {
            public string msg;
            public long time;
            public long group;
            public int kind;
            public void SetTime(long ntime)
            {
                time = ntime;
            }
        }
        public static List<delaymsg> delays = new List<delaymsg>();
        public static void LetSay(string Comments, long Group,int k = 0)
        {

            string[] w = Comments.Replace(',', '，').Replace('.', '，').Replace('。', '，')
                                     .Replace('！', '，').Replace('!', '，')
                                     .Replace('\"', ' ').Replace('“', ' ').Replace('”', ' ')
                                     .Replace('？', '，').Replace('?', '，').Split('，');

            Random r = new Random(Guid.NewGuid().GetHashCode());
            long now = GetTickCount();
            for (int j = 0; j < w.Length; j++)
            {
                w[j] = w[j].Trim();
                if (w[j].Length > 0 && w[j].Length < 40)
                {
                    delaymsg d = new delaymsg();
                    d.msg = w[j]; d.kind = k;
                    d.time = now + (w[j].Length * (long)(300 * (Convert.ToDouble(r.Next(70, 120)) / 100f)));
                    d.group = Group;
                    delays.Add(d);
                    now = d.time;
                    now += 1000;
                }
            }
            Log("delay msg sheet added successfully", ConsoleColor.Green);
        }
        private static void Log(string log, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }
        private static bool CanMatch(string target, params string[] matches)
        {
            bool b = false;
            for (int i = 0; i < matches.Length; i++)
            {
                b = (b || (target.IndexOf(matches[i]) == 0));
            }
            return b;
        }
        public static void CheckProcessMsg(string msg,long number,int k)
        {
            int action = 0;
            //催促
            if (CanMatch(msg.ToLower(),
                "quick", "quickly", "fast", "more quickly","faster",
                "快", "快点", "加速", "快一点"))
            { action = 1; }
            //停止
            if (CanMatch(msg.ToLower(),
                "stop", "shut up", "shut it", "shut that", 
                "闭嘴", "别说了", "不要说了","停","停下"))
            { action = 2; }
            //暂停
            if (CanMatch(msg.ToLower(),
                "pause", "wait",  
                "暂停", "等等", "等一等","等一下",
                "等下","停一下","停一会儿","等一会儿"))
            { action = 3; }
            //减慢
            if (CanMatch(msg.ToLower(),
                "slow", "slowly", "slower", 
                "慢", "慢点", "减速", "慢一点"))
            { action = 4; }

            if (action == 0) { return; } //爬

            Random r = new Random(Guid.NewGuid().GetHashCode());
            delaymsg fd = new delaymsg();fd.group = 0;
            List<delaymsg> t = delays.FindAll(m => m.group == number && m.kind == k);
            if (t.Count == 0) { return; }

            foreach (delaymsg d in t)
            {
                if (action == 1) { d.SetTime(Convert.ToInt64(d.time * 0.9)); }
                if (action == 2) { delays.Remove(d); }
                if (action == 3) { d.SetTime(Convert.ToInt64(d.time + 20000 * r.Next(85,115) / 100)); }
                if (action == 4) { d.SetTime(Convert.ToInt64(d.time * 1.1)); }
                if (fd.group == 0) { fd = d; }
            }

            if (action == 3)
            {
                fd.time = fd.time - 2000 * r.Next(85, 115) / 100;
                fd.msg = "对了，我们刚才说到 " + fd.msg;
                delays.Add(fd);
            }

            string tmsg = "";
            if (action == 1) { tmsg = "好吧，那我说快点。"; }
            if (action == 2) { tmsg = "好吧，那就不说了。"; }
            if (action == 3) { tmsg = "好吧，那你想和我说什么呢。"; }
            if (action == 4) { tmsg = "好吧，那我说慢点。"; }

            if(k == 0)
            {
                new Group(pCQ, number).SendGroupMessage(tmsg);
            }
            else
            {
                new QQ(pCQ, number).SendPrivateMessage(tmsg);
            }
        }
        public static void Poster()
        {
        posthead:
            //Moring Protection
            Storage sys = new Storage("system");
            if (DateTime.Now.Hour >= 3 && DateTime.Now.Hour <= 5)
            {
                if (sys.getkey("root", "sleep") != "zzz")
                {
                    sys.putkey("root", "sleep", "zzz");
                    QQ master = new QQ(pCQ, 1361778219);
                    master.SendPrivateMessage("主人晚安~");
                    Console.Clear();
                    Log("[SLEEP] zzzzzzz");
                    logid = Guid.NewGuid().ToString();
                    //Application.Restart();
                    //System.Diagnostics.Process.Start(workpath + "\\CQA.exe", "/account 3529296290");
                    //System.Environment.Exit(0);
                }
                return;
            }
            if (sys.getkey("root", "sleep") == "zzz")
            {
                Log("[WAKE UP] ouch");
                QQ master = new QQ(pCQ, 1361778219);
                master.SendPrivateMessage("主人早上好~");
                sys.putkey("root", "sleep", "！！");
            }

            Group g;
            //Say
            try
            {
                for (int i = 0; i < delays.Count; i++)
                {
                resay:
                    delaymsg dm = delays[i];
                    if (GetTickCount() >= dm.time)
                    {
                        if(dm.kind == 0)
                        {
                            new Group(pCQ, dm.group).SendGroupMessage(dm.msg);
                        }
                        else
                        {
                            new QQ(pCQ, dm.group).SendPrivateMessage(dm.msg);
                        }
                        delays.Remove(dm); goto resay;
                    }
                }
            }
            catch
            {
                
            }

            //Undertale
            if (UT.targetg != 0)
            {
                if (GetTickCount() - UT.tick >= 20000)
                {
                    g = new Native.Csharp.Sdk.Cqp.Model.Group(pCQ, UT.targetg);
                    if (UT.winstr == "")
                    {
                        g.SendGroupMessage("nobody past round" + UT.round + ",answer:" + UT.role);
                    }
                    else
                    {
                        g.SendGroupMessage("answer:" + UT.role + "\n" + UT.winstr);
                    }
                    
                    if (UT.round == 5)
                    {
                        string playstr = "";
                        for (int i = 0; i < UT.ps.Count; i++)
                        {
                            playstr = playstr + CQApi.CQCode_At(UT.ps[i].qq) + " " + (int)(UT.ps[i].score * 10) / 10 + " points\n";
                        }
                        UT.targetg = 0;
                        g.SendGroupMessage("game closed\n" + playstr);
                    }
                    else
                    {
                        UT.nextRound(); UT.tick = GetTickCount();
                        g.SendGroupMessage("round " + UT.round + "（result:20s laters）：" + UT.dialog);
                    }
                }
            }

            //Hot Poster
            string fstr = ""; string estr = ""; string[] qtemp;
            HotMsg hhmsg = new HotMsg();
            for (int s = 0; s < Manager.mHot.data.Count; s++)
            {
                hhmsg = (HotMsg)Manager.mHot.data[s];
                if (DateTime.Now.Hour >= 22 && hhmsg.hasup == false && TenClockLock==false)
                {
                    TenClockLock = true;
                    Log("Annouce:" + hhmsg.group, ConsoleColor.Green);
                    qtemp = hhmsg.banqq.Split(';');
                    for (int i = 0; i < qtemp.Length - 1; i++)
                    {
                        estr = estr + CQApi.CQCode_At(Convert.ToInt64(qtemp[i]));
                    }
                    qtemp = hhmsg.qq.Split(';');
                    for (int i = 0; i < qtemp.Length - 1; i++)
                    {
                        fstr = fstr + CQApi.CQCode_At(Convert.ToInt64(qtemp[i]));
                    }
                    hhmsg.hasup = true;
                    g = new Native.Csharp.Sdk.Cqp.Model.Group(pCQ, Convert.ToInt64(hhmsg.group));
                    g.SendGroupMessage(hhmsg.msg);
                    Manager.mHot.data[s] = hhmsg;
                }
            }
            //Homework network
            string f = "0";
            if (File.Exists("C:\\DataArrange\\homeworklock.bin")) { f = File.ReadAllText("C:\\DataArrange\\homeworklock.bin", Encoding.UTF8); }
            if (Convert.ToInt64(f) == 1)
            {
                Log("New homework recevied !", ConsoleColor.Green);
                f = File.ReadAllText("C:\\DataArrange\\homework.bin", Encoding.UTF8);
                g = new Native.Csharp.Sdk.Cqp.Model.Group(pCQ, 817755769);
                g.SendGroupMessage("[今日作业推送消息]\n" + f + "\n————来自黑嘴稽气人的自动推送");
                File.WriteAllText("C:\\DataArrange\\homeworklock.bin", "0");
            }
            f = "";
            if (File.Exists("C:\\DataArrange\\announcer.bin")) { f = File.ReadAllText("C:\\DataArrange\\announcer.bin", Encoding.UTF8); }
            if (f != "")
            {
                Log("Announce:" + f, ConsoleColor.Green);
                string[] p = f.Split('\\'); long gr = 0;
                if (p[0] == "class") { gr = 817755769; }
                if (p[0] == "inter") { gr = 554272507; }
                g = new Native.Csharp.Sdk.Cqp.Model.Group(pCQ, gr);
                switch (p[1])
                {
                    case ("hlesson"):
                        f = "今天上午的网课出炉啦~\n地址：{url}\n往期网课精彩回顾：https://space.bilibili.com/313086171/channel/detail?cid=103565 ".Replace("{url}", p[2]);
                        break;
                    case("lesson"):
                        f = "今天全天的网课出炉啦~\n地址：{url}\n往期网课精彩回顾：https://space.bilibili.com/313086171/channel/detail?cid=103565 ".Replace("{url}", p[2]);
                        break;
                    case("default"):
                        f = p[2];
                        break;
                    default:
                        Log("Unkown announce .", ConsoleColor.Red);
                        return;
                }
                g.SendGroupMessage("[通知]\n" + f + "\n————来自黑嘴稽气人的自动推送");
                File.WriteAllText("C:\\DataArrange\\announcer.bin", "");
            }
            Thread.Sleep(1000);
            goto posthead;
        }
    }
}

namespace RestoreData.Manager
{
    using io.github.buger404.intallk.Code;
    public class Manager
    {
        public static Storage wordcollect;
        public static DataArrange LCards;
        public static DataArrange Hots;
        public static DataArrange mHot;
        public static DataArrange scrBan;
    }
}

namespace io.github.buger404.intallk.Code
{
    public class DataArrange
    {
        public ArrayList data = new ArrayList();
        public string name;

        public DataArrange()
        {
            MessageBox.Show("未能成功触发构造函数");
        }

        public DataArrange(string dataname)
        {
            this.name = dataname;
            if (Directory.Exists(@"C:\DataArrange\") == false) { Directory.CreateDirectory(@"C:\DataArrange\"); }
            if (Directory.Exists(@"C:\DataArrange\Debug\") == false) { Directory.CreateDirectory(@"C:\DataArrange\Debug\"); }
            this.ReadData();
        }

        public int SearchFor(object o)
        {
            string t = o.ToString();
            for (int i = 0; i < this.data.Count; i++)
            {
                if (this.data[i].ToString() == t) { return i; }
            }
            return -1;
        }

        public void SaveData()
        {
            string content = "";
            for (int i = 0; i < this.data.Count; i++)
            {
                content = content + this.data[i].ToString() + "`";
            }
            
            File.WriteAllText("C:\\DataArrange\\" + this.name + ".bin", content);
        }

        public void ReadData()
        {
            if (File.Exists("C:\\DataArrange\\" + this.name + ".bin") == false) { return; }
            string[] item = File.ReadAllText("C:\\DataArrange\\" + this.name + ".bin", Encoding.UTF8).Split('`');
            for (int i = 0; i < item.Length; i++)
            {
                if (item[i] != "")
                {
                    this.data.Add(item[i]);
                }
            }
        }

        
    }
}
