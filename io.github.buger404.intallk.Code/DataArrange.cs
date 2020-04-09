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
using io.github.buger404.intallk.Code;
using SpeechLib;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;

namespace MainThread
{
    public static class MessagePoster
    {
        [DllImport("kernel32.dll",
            CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();
        public static bool TenClockLock = false;
        public static CQApi pCQ;
        public static long LastOSUTime = 0;
        public static string logid = "";
        private static bool HasSendDie = false;
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
            public bool voice;
            public void SetTime(long ntime)
            {
                this.time = ntime;
            }
        }
        public struct flowlibrary
        {
            public string[] lib;
            public string name;
        }
        public static List<delaymsg> delays = new List<delaymsg>();
        public static List<string> ptList;
        public static List<flowlibrary> flibrary = new List<flowlibrary>();
        public static void SimSay(string Comments, long Group, long delay, bool Voice = false)
        {

            Random r = new Random(Guid.NewGuid().GetHashCode());
            long now = GetTickCount();
            delaymsg d = new delaymsg();
            d.msg = Comments; d.kind = 0; d.voice = Voice;
            d.time = now + delay;
            d.group = Group;
            delays.Add(d);
        }
        public static void LetSay(string Comments, long Group,int k = 0,bool Voice = false)
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
                    d.msg = w[j]; d.kind = k; d.voice = Voice;
                    d.time = now + 
                            (w[j].Length * 
                            (long)(300 * (Convert.ToDouble(r.Next(70, 120)) / 100f)
                            * (Voice ? 1.5 : 1))
                            );
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
        public static void LoadFlows()
        {
            string[] files = Directory.GetFiles(@"C:\DataArrange\Library");
            foreach(string f in files)
            {
                flowlibrary fl = new flowlibrary();
                fl.lib = File.ReadAllText(f,Encoding.UTF8)
                             .Split(new string[] { "\r\n" }, StringSplitOptions.None);
                fl.name = f.Replace("词库.txt", "").Replace(@"C:\DataArrange\Library\", "");
                flibrary.Add(fl);
                Log("词库[" + fl.name + "]已加载，共" + fl.lib.Length + "条数据。");
            }
        }
        public static void LoadPTemples()
        {
            string[] files = Directory.GetFiles(@"C:\DataArrange\PTemple");
            ptList = new List<string>();
            foreach (string f in files)
            {
                string[] temp = f.Split('\\');
                ptList.Add(temp[temp.Length - 1].Split('.')[0]);
            }
            Log("已加载" + ptList.Count + "个P图模板");
        }
        public static bool CheckProcessMsg(string msg,long number,int k)
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

            if (action == 0) { return false; } //爬

            Random r = new Random(Guid.NewGuid().GetHashCode());
            delaymsg fd = new delaymsg();fd.group = 0;
            bool CanAction = false;

            for (int i = 0;i < delays.Count;i++)
            {
            delayhead:
                if (i >= delays.Count) { break; }
                delaymsg d = delays[i];
                if (d.group == number && d.kind == k)
                {
                    CanAction = true;
                    if (action == 1) { d.time = Convert.ToInt64(GetTickCount() + (d.time - GetTickCount()) * 0.9); }
                    if (action == 2) { delays.Remove(delays[i]);goto delayhead; }
                    if (action == 3) { d.time = (Convert.ToInt64(d.time + 30000 * r.Next(85, 115) / 100)); }
                    if (action == 4) { d.time = (Convert.ToInt64(GetTickCount() + (d.time - GetTickCount()) * 1.1)); }
                    delays[i] = d;
                    if (fd.group == 0) { fd = d; }
                }
            }

            if (!CanAction) { return false; }

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

            return true;
        }
        public static void Poster()
        {
        posthead:
            Random r = new Random(Guid.NewGuid().GetHashCode());
            try
            {
                if (r.Next(0,500) == 88)
                {
                    List<GroupInfo> lg = pCQ.GetGroupList();
                    Event_GroupMessage.Artifical(lg[r.Next(0,lg.Count)].Group);
                }

                //OSU
                if (LastOSUTime == DateTime.Now.Hour && DateTime.Now.Minute == 30)
                {
                    Storage ignore = new Storage("ignore");
                    if (ignore.getkey("577344771", "artist") == "√") { goto NoPOSU; }
                    LastOSUTime = (DateTime.Now.Hour + 1) % 24;
                    Group droid = new Group(pCQ, 577344771);
                    List<GroupMemberInfo> gml = droid.GetGroupMemberList();
                    GroupMemberInfo gmi = gml[r.Next(0, gml.Count)];
                    long qq = gmi.QQ.Id;
                    string targett = MessagePoster.ptList[r.Next(0, MessagePoster.ptList.Count)];
                    ScriptDrawer.Draw("C:\\DataArrange\\PTemple\\" + targett + ".txt",
                                      MessagePoster.workpath + "\\data\\image\\" + targett + ".png",
                                      "[qq]", qq.ToString(),
                                      "[nick]", gmi.Nick,
                                      "[card]", gmi.Card == "" ? gmi.Nick : gmi.Card,
                                      "[sex]", gmi.Sex.ToString(),
                                      "[age]", gmi.Age.ToString(),
                                      "[group]", "577344771"
                                      );
                    droid.SendGroupMessage("现在是 " + DateTime.Now.Hour +
                                           "时30分 不整，恭喜幸运小朋友：" + CQApi.CQCode_At(qq) + "\n"
                                           + CQApi.CQCode_Image(targett + ".png"));

                }
            NoPOSU:

                //BlackDied
                if (DateTime.Now.Hour == 24 || DateTime.Now.Hour == 0)
                {
                    if (DateTime.Now.Month == 3 && DateTime.Now.Day == 27)
                    {
                        if (!HasSendDie)
                        {
                            List<GroupInfo> gi = pCQ.GetGroupList();
                            foreach (GroupInfo gii in gi)
                            {
                                gii.Group.SendGroupMessage("今天。是黑嘴去世" + (DateTime.Now.Year - 2015) + "周年的日子。在这里打扰了大家，非常抱歉。\n黑嘴，名字来源于本机作者的一只狗，这只狗在本机作者的精神支柱上有很大的作用【虽然这听起来很荒唐】，它也渐渐在本机主人的脑子里逐渐扭曲抽象成了一种精神依靠。\n祝你在天堂快乐，黑嘴。       -3.27\n不接受任何对此条消息的议论。");
                            }
                            HasSendDie = true;
                        }
                    }
                }

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
                    Thread.Sleep(1000);
                    goto posthead;
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
                            if (dm.voice == true)
                            {
                                SpeechSynthesizer reader = new SpeechSynthesizer();
                                string fname = GetTickCount().ToString();
                                reader.SetOutputToWaveFile(
                                                workpath + "\\data\\record\\say_" + fname + ".wav",
                                                new SpeechAudioFormatInfo(32000, AudioBitsPerSample.Sixteen, AudioChannel.Mono)
                                                            );
                                reader.Rate = -2 + new Random(Guid.NewGuid().GetHashCode()).Next(0, 4);
                                reader.Volume = 100;
                                //reader.SelectVoice("Microsoft Lili");
                                PromptBuilder builder = new PromptBuilder();
                                builder.AppendText(dm.msg);
                                Log("Speak started at :" + GetTickCount());
                                reader.Speak(builder);
                                Log("Speak successfully :" + GetTickCount());
                                reader.Dispose();
                                if (dm.kind == 0)
                                { 
                                    new Group(pCQ, dm.group).SendGroupMessage(CQApi.CQCode_Record("say_" + fname + ".wav"));
                                }
                                else
                                {
                                    new QQ(pCQ, dm.group).SendPrivateMessage(CQApi.CQCode_Record("say_" + fname + ".wav"));
                                }
                            }
                            else
                            {
                                Log("Send successfully.");
                                if (dm.kind == 0)
                                {
                                    new Group(pCQ, dm.group).SendGroupMessage(dm.msg);
                                }
                                else
                                {
                                    new QQ(pCQ, dm.group).SendPrivateMessage(dm.msg);
                                }
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
                    if (DateTime.Now.Hour >= 22 && hhmsg.hasup == false && TenClockLock == false)
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
                        case ("lesson"):
                            f = "今天全天的网课出炉啦~\n地址：{url}\n往期网课精彩回顾：https://space.bilibili.com/313086171/channel/detail?cid=103565 ".Replace("{url}", p[2]);
                            break;
                        case ("default"):
                            f = p[2];
                            break;
                        default:
                            Log("Unkown announce .", ConsoleColor.Red);
                            return;
                    }
                    g.SendGroupMessage("[通知]\n" + f + "\n————来自黑嘴稽气人的自动推送");
                    File.WriteAllText("C:\\DataArrange\\announcer.bin", "");
                }
            }
            catch(Exception err)
            {
                Log(err.StackTrace + "\n" + err.Message, ConsoleColor.Red);
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
