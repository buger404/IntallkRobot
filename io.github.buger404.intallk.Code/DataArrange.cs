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

namespace MainThread
{
    public static class MessagePoster
    {
        [DllImport("kernel32.dll",
            CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();
        public static CQApi pCQ;
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
        }
        private static void Log(string log, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }
        public static void Poster()
        {
            posthead:
            Native.Csharp.Sdk.Cqp.Model.Group g;

            //Undertale
            if (UT.targetg != 0)
            {
                if (GetTickCount() - UT.tick >= 20000)
                {
                    g = new Native.Csharp.Sdk.Cqp.Model.Group(pCQ, UT.targetg);
                    if (UT.winstr == "")
                    {
                        g.SendGroupMessage("无人能解答第" + UT.round + "轮！答案是：" + UT.role);
                    }
                    else
                    {
                        g.SendGroupMessage("答案是：" + UT.role + "，本轮信息：\n" + UT.winstr);
                    }
                    
                    if (UT.round == 5)
                    {
                        string playstr = "";
                        for (int i = 0; i < UT.ps.Count; i++)
                        {
                            playstr = playstr + CQApi.CQCode_At(UT.ps[i].qq) + " " + (int)(UT.ps[i].score * 10) / 10 + " points\n";
                        }
                        UT.targetg = 0;
                        g.SendGroupMessage("游戏结束，本次游戏结果\n" + playstr);
                    }
                    else
                    {
                        UT.nextRound(); UT.tick = GetTickCount();
                        g.SendGroupMessage("第" + UT.round + "轮（20s后公布本轮结果）：" + UT.dialog);
                    }
                }
            }

            //Hot Poster
            string fstr = ""; string estr = ""; string[] qtemp;
            HotMsg hhmsg = new HotMsg();
            for (int s = 0; s < Manager.mHot.data.Count; s++)
            {
                hhmsg = (HotMsg)Manager.mHot.data[s];
                if (DateTime.Now.Hour >= 22 && hhmsg.hasup == false)
                {
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
                    g.SendGroupMessage("截止刚才，本群今日最热发言：\n" + hhmsg.msg + "\n复读发起人：" + CQApi.CQCode_At(Convert.ToInt64(qtemp[0])) + "\n复读热度：" + hhmsg.hot + "\n该热度发言被这些成员复读过：" + fstr + "\n在该热度发言发起时，这些成员太过激动试图刷屏：" + estr);
                    Manager.mHot.data[s] = hhmsg;
                }
            }
            //Homework network
            string f = "0";
            if (File.Exists("D:\\DataArrange\\homeworklock.bin")) { f = File.ReadAllText("D:\\DataArrange\\homeworklock.bin", Encoding.UTF8); }
            if (Convert.ToInt64(f) == 1)
            {
                Log("New homework recevied !", ConsoleColor.Green);
                f = File.ReadAllText("D:\\DataArrange\\homework.bin", Encoding.UTF8);
                g = new Native.Csharp.Sdk.Cqp.Model.Group(pCQ, 817755769);
                g.SendGroupMessage("[今日作业推送消息]\n" + f + "\n————来自黑嘴稽气人的自动推送");
                File.WriteAllText("D:\\DataArrange\\homeworklock.bin", "0");
            }
            f = "";
            if (File.Exists("D:\\DataArrange\\announcer.bin")) { f = File.ReadAllText("D:\\DataArrange\\announcer.bin", Encoding.UTF8); }
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
                File.WriteAllText("D:\\DataArrange\\announcer.bin", "");
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
        public static DataArrange LCards;
        public static DataArrange CPms;
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
            if (Directory.Exists(@"D:\DataArrange\") == false) { Directory.CreateDirectory(@"D:\DataArrange\"); }
            if (Directory.Exists(@"D:\DataArrange\Debug\") == false) { Directory.CreateDirectory(@"D:\DataArrange\Debug\"); }
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
            
            File.WriteAllText("D:\\DataArrange\\" + this.name + ".bin", content);
        }

        public void ReadData()
        {
            if (File.Exists("D:\\DataArrange\\" + this.name + ".bin") == false) { return; }
            string[] item = File.ReadAllText("D:\\DataArrange\\" + this.name + ".bin", Encoding.UTF8).Split('`');
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
