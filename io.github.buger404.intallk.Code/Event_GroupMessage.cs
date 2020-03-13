﻿using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using VoidLife.Simulator;
using RestoreData.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using ArtificalA.Intelligence;
using System.Threading;
using System.Runtime.InteropServices;
using MainThread;
using Wallpapers.Searcher;
using Undertale.Dialogs;
using DataArrange.Storages;
using Native.Csharp.Sdk.Cqp.Model;
using System.IO;
using System.Diagnostics;

namespace io.github.buger404.intallk.Code
{
    // 添加引用 IGroupMessage
    public class Event_GroupMessage: IGroupMessage
    {
        public static Storage info = new Storage("userinfo");
        public static Storage ignore = new Storage("ignore");
        public enum ShowCommands : int
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_FORCEMINIMIZE = 11,
            SW_MAX = 11
        }
        [DllImport("shell32.dll")]
        static extern IntPtr ShellExecute(
            IntPtr hwnd,
            string lpOperation,
            string lpFile,
            string lpParameters,
            string lpDirectory,
            ShowCommands nShowCmd);
        [DllImport("kernel32.dll",
            CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();
        private bool FailAI = false;
        private enum PermissionName
        {
            AirPermission = 0, //参观级别的权限
            UserPermission = 1, //正常级别的权限
            SupermanPermission = 2, //可以访问有关群管理的内容
            MasterPermission = 3, //可以访问直接关系到我自己的内容
            HeavenPermission = 4, //可以访问我自己的计算机
            Error404 = 32767 //我自己
        }
        private struct personmsg
        {
            public long tick;
            public double anger;
            public long qq;
            public string lmsg;
        }
        public static int recordtime = 0;
        private static string GetPermissionName(PermissionName pe)
        {
            switch (pe)
            {
                case (PermissionName.AirPermission): return "Air";
                case (PermissionName.UserPermission): return "User";
                case (PermissionName.SupermanPermission): return "Superman";
                case (PermissionName.MasterPermission): return "Master";
                case (PermissionName.HeavenPermission): return "Heaven";
                case (PermissionName.Error404): return "<Error 404>";
            }
            return "Unknown";
        }
        private static bool CanMatch(string target,params string[] matches)
        {
            bool b = false;
            for (int i = 0; i < matches.Length; i++)
            {
                b = (b || (target.IndexOf(matches[i]) >= 0));
            }
            return b;
        }
        private static void Log(string log, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }
        private static int GotCount(string str, string cstr)
        {
            string[] t = str.Split(';');int c = 0;
            for (int i = 0; i < t.Length; i++)
            {
                if (t[i] == cstr) { c++; }
            }
            return c;
        }
        public void SolvePlayer(float score,long qq)
        {
            bool findplayer = false; UT.player utplay = new UT.player();
            for (int i = 0; i < UT.ps.Count; i++)
            {
                if (UT.ps[i].qq == qq)
                {
                    findplayer = true;
                    utplay = UT.ps[i];
                    utplay.score += score; 
                    UT.ps[i] = utplay;
                    break;
                }
            }
            if (!findplayer)
            {
                utplay.score = score; utplay.qq = qq; 
                UT.ps.Add(utplay);
            }
        }
        public int PutRepeat(string name,string text, CQGroupMessageEventArgs e,bool SkipCheck = true)
        {
            if (SkipCheck) { goto DontCheck; }
            bool exit = true;MainThread.MessagePoster.HotMsg hmsg;
            for (int i = 0; i < Manager.Hots.data.Count; i++)
            {
                hmsg = (MainThread.MessagePoster.HotMsg)Manager.Hots.data[i];
                if(e.FromGroup.Id == hmsg.group)
                {
                    long qq = Convert.ToInt64(hmsg.qq.Split(';')[0]);
                    GroupMemberInfo g = e.FromGroup.GetGroupMemberInfo(qq);
                    string QQName = ""; string Nick = "";
                    try
                    {
                        QQName = g.Card;
                        Nick = g.Nick;
                    }
                    catch
                    {
                        try
                        {
                            Nick = e.CQApi.GetStrangerInfo(qq).Nick;
                        }
                        catch
                        {
                            Nick = QQName;
                        }
                    }
                    if (hmsg.msg.ToLower() == text.ToLower() && (QQName.ToLower().IndexOf(name) >= 0 || Nick.ToLower().IndexOf(name) >= 0)) { exit = false; break; }
                }
            }
            if (exit) { return 0; }
            DontCheck:
            //if (text.IndexOf("[CQ:") >= 0) { text = text.Replace("[CQ:", "["); }
            int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
            Manager.wordcollect.putkey("repeat", "item" + Count, text);
            Manager.wordcollect.putkey("owner" + Count, "name", name);
            Count++;
            Manager.wordcollect.putkey("repeat", "count", Count.ToString());
            return 1;
        }
        public struct GroupBUGCheck
        {
            public long Group;
            public long Tick;
        }
        List<GroupBUGCheck> bugs = new List<GroupBUGCheck>();
        int ProtectCount = 0;

        private bool JudgePermission(long qq,PermissionName Required)
        {
            PermissionName pe = PermissionName.AirPermission;
            pe = (PermissionName)Convert.ToInt64(info.getkey(qq.ToString(), "permission"));
            return (pe >= Required);
        }
        // 接收事件
        public void GroupMessage(object sender,CQGroupMessageEventArgs e)
        {
            //WHEN BUG Protection
            if (ProtectCount >= 3) { return; }
            int bi = bugs.FindIndex(m => m.Group == e.FromGroup.Id);
            if(bi == -1)
            {
                bugs.Add(new GroupBUGCheck { Group = e.FromGroup.Id,Tick = GetTickCount() });
            }
            else
            {
                //消息间隔小于32ms的群不是抽风就是机器人不正常
                if(GetTickCount() - bugs[bi].Tick <= 32) { ProtectCount++; }
                if (ProtectCount >= 3) { Log("!!!!!!!!!\nBUG , STOPPED WORKING.\n!!!!!!", ConsoleColor.Red); }
                if (GetTickCount() - bugs[bi].Tick >= 3000) { ProtectCount = 0; }
                GroupBUGCheck gbc = bugs[bi];gbc.Tick = GetTickCount();bugs[bi] = gbc;
            }

            try
            {
                if (ignore.getkey(e.FromGroup.Id.ToString(), "group") == "√") { return; }

                Log("(" + e.FromGroup.Id + ")Message:" + e.FromQQ.Id + "," + e.Message.Text, ConsoleColor.Cyan);
                //Moring Protection
                Storage sys = new Storage("system");
                if (sys.getkey("root", "sleep") == "zzz")
                {
                    if (e.Message.Text.StartsWith("ik "))
                    {
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                            " I'm now sleeping , please wake me up later.");
                    }
                    return;
                }

                int sid; double qq = 0;
                List<Native.Csharp.Sdk.Cqp.Model.GroupMemberInfo> mem;
                Random r = new Random(Guid.NewGuid().GetHashCode()); string ques = "";
                int hIndex = -1; MainThread.MessagePoster.HotMsg hhmsg = new MainThread.MessagePoster.HotMsg();

                //Undertale Gameing
                if (UT.targetg == e.FromGroup.Id)
                {
                    if (e.Message.Text.ToLower() == UT.role)
                    {
                        //e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),"回答正确！！！");
                        UT.winstr = UT.winstr + CQApi.CQCode_At(e.FromQQ.Id) + " correct +" + (int)(UT.prise * 10) / 10 + " points\n";
                        SolvePlayer(UT.prise, e.FromQQ.Id); UT.prise *= 0.8f;
                    }
                    else
                    {
                        for (int i = 0; i < UT.pos.Count; i++)
                        {
                            if (e.Message.Text.ToLower() == UT.pos[i].name)
                            {
                                UT.winstr = UT.winstr + CQApi.CQCode_At(e.FromQQ.Id) + " incorrect -5 points\n";
                                SolvePlayer(-5f, e.FromQQ.Id);
                            }
                        }
                        for (int i = 0; i < 8; i++)
                        {
                            if (e.Message.Text.ToLower() == (new string(new char[] { (char)('a' + i) })))
                            {
                                UT.winstr = UT.winstr + CQApi.CQCode_At(e.FromQQ.Id) + " incorrect -5 points\n";
                                SolvePlayer(-5f, e.FromQQ.Id);
                            }
                        }
                    }

                }

                //Artifical Repeater
                long hothander = 0;
                MainThread.MessagePoster.HotMsg hmsg = new MainThread.MessagePoster.HotMsg();
                for (int i = 0; i < Manager.mHot.data.Count; i++)
                {
                    hmsg = (MainThread.MessagePoster.HotMsg)Manager.mHot.data[i];
                    if (hmsg.group == e.FromGroup.Id) { hhmsg = hmsg; hIndex = i; break; }
                }
                string fstr = ""; string estr = ""; string[] qtemp;
                for (int i = 0; i < Manager.Hots.data.Count; i++)
                {
                nexthmsg:
                    if (i >= Manager.Hots.data.Count) { break; }
                    hmsg = (MainThread.MessagePoster.HotMsg)Manager.Hots.data[i];
                    //是否在目标群
                    if (hmsg.group == e.FromGroup.Id)
                    {
                        if (hmsg.delaymsg) //延迟复读
                        {
                            if (hmsg.hasre == false)
                            {
                                if (hmsg.canre)
                                {
                                    Log("(" + e.FromGroup.Id + ")(" + i + ")Delay repeat:" + hmsg.msg, ConsoleColor.Yellow);
                                    e.FromGroup.SendGroupMessage(hmsg.msg);
                                    hmsg.delaymsg = false; hmsg.hasre = true;
                                    Log("(" + e.FromGroup.Id + ")(" + i + ")Conntinue-repeat:" + hmsg.delaymsg, ConsoleColor.Yellow);
                                }
                            }
                        }
                        if (e.Message.Text != hmsg.msg) { hmsg.hot--; } //如果当前的发言和上句发言不同，上句发言热度-1
                                                                        //如果当前的发言和上句发言一直，上句发言热度+1
                        if (e.Message.Text == hmsg.msg)
                        {
                            hothander = hmsg.id;
                            // 不是同一个QQ在刷屏
                            if (hmsg.qq.IndexOf(e.FromQQ.Id.ToString() + ";") < 0)
                            {
                                Log("(" + e.FromGroup.Id + ")(" + i + ")Heat:" + hmsg.msg + " , hot :" + hmsg.hot);
                                hmsg.hasup = true;
                                hmsg.qq = hmsg.qq + e.FromQQ.Id.ToString() + ";";
                                Log("(" + e.FromGroup.Id + ")(" + i + ")Members:" + hmsg.qq);
                                hmsg.hot++;
                                if (hIndex > -1)
                                {
                                    if (hmsg.hot >= hhmsg.hot)
                                    {
                                        hhmsg = hmsg; hhmsg.hasup = false;
                                        Manager.mHot.data[hIndex] = hhmsg;
                                        Log("(" + e.FromGroup.Id + ")(" + i + ")Broke records! :" + hmsg.qq + "," + hmsg.msg, ConsoleColor.Green);
                                    }
                                }
                                else
                                {
                                    hhmsg = hmsg; hhmsg.hasup = false;
                                    Manager.mHot.data.Add(hhmsg);
                                }
                            }
                            else
                            {
                                hmsg.banqq = hmsg.banqq + e.FromQQ.Id.ToString() + ";";
                                int bancount = GotCount(hmsg.banqq, e.FromQQ.Id.ToString());
                                Log("(" + e.FromGroup.Id + ")(" + i + ")Boring-repeat:" + e.FromQQ.Id + " x " + bancount, ConsoleColor.Red);
                            }
                        }
                        //如果发言冷却，移除
                        if (hmsg.hot <= -4)
                        {
                            if (hmsg.hasup)
                            {
                                string QQName = e.CQApi.GetGroupMemberInfo(e.FromGroup.Id, Convert.ToInt64(hmsg.qq.Split(';')[0])).Card;
                                string Nick = e.CQApi.GetGroupMemberInfo(e.FromGroup.Id, Convert.ToInt64(hmsg.qq.Split(';')[0])).Nick;
                                string[] ctemp = hmsg.msg.ToLower().Split(new string[] { "[cq:image,file=" },StringSplitOptions.None);
                                string cmsg = ctemp[0];
                                for (int j = 1; j < ctemp.Length; j++)
                                {
                                    string[] temp = ctemp[j].Split(']');
                                    if (temp.Length > 1) { cmsg += temp[1]; }
                                }
                                if (cmsg.Trim().Length > 3 && hmsg.qq.Split(';').Length >= 3)
                                {
                                    //如果删除图片以外的内容字数大于3则记录
                                    PutRepeat(QQName + "(" + Nick + ")", hmsg.msg, e);
                                }
                                Log("(" + e.FromGroup.Id + ")(" + i + ")Disapper repeat:" + hmsg.msg, ConsoleColor.Red);
                            }
                            Manager.Hots.data.RemoveAt(i); goto nexthmsg;
                        }
                        //发言热度越高，越容易引发复读
                        if (hmsg.hot > 0)
                        {
                            if (r.Next(0, 100) > 100 - hmsg.hot * 20 * (hmsg.hot / 2))
                            {
                                hmsg.delaymsg = (r.Next(0, 10) > 6); //设置延迟一回合
                                hothander = hmsg.id;
                                if (!hmsg.delaymsg)
                                {
                                    if (hmsg.hasre == false)
                                    {
                                        if (hmsg.canre)
                                        {
                                            Log("(" + e.FromGroup.Id + ")(" + i + ")Repeat:" + hmsg.msg + ",impossible:" + (100 - hmsg.hot * 20 * (hmsg.hot / 2)), ConsoleColor.Yellow);
                                            e.FromGroup.SendGroupMessage(hmsg.msg);
                                        }
                                        hmsg.hasre = true;
                                    }
                                }
                                else
                                {
                                    Log("(" + e.FromGroup.Id + ")(" + i + ")Delay set:" + hmsg.msg, ConsoleColor.Yellow);
                                }
                            }
                        }
                        if (i >= Manager.Hots.data.Count) { break; }
                        Manager.Hots.data[i] = hmsg;
                    }
                }
                if (hothander == 0)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "repeater") != "√") 
                    {
                        hmsg.group = e.FromGroup.Id; hmsg.msg = e.Message.Text; hmsg.hot = 1; hmsg.delaymsg = false;
                        hmsg.qq = e.FromQQ.Id.ToString() + ";"; hmsg.banqq = ""; hmsg.id = DateTime.Now.Ticks;
                        hmsg.hasup = false; hmsg.hasre = false;
                        hmsg.canre = (r.Next(0, 6) == 3);
                        Manager.Hots.data.Add(hmsg);
                    }
                }
                else
                {
                    if (hIndex > -1)
                    {
                        if (hhmsg.id == hmsg.id)
                        {
                            hhmsg = hmsg; hhmsg.hasup = false;
                            Manager.mHot.data[hIndex] = hhmsg;
                            Log("(" + e.FromGroup.Id + ")Stay breaking record ...", ConsoleColor.Green);
                        }
                    }
                }

                //More Artifical
                MessagePoster.CheckProcessMsg(e.Message.Text, e.FromGroup.Id, 0);
                if (r.Next(0, 666) == 555)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "repeater") == "√") { return; }
                    e.FromGroup.SendGroupMessage(e.Message.Text);
                }
                if (r.Next(0, 800) == 444)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "ai") == "√") { return; }
                    int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                    e.FromGroup.SendGroupMessage(Manager.wordcollect.getkey("repeat", "item" + r.Next(0, Count)));
                }

                //Screen Checker
                if (e.FromGroup.Id == 577344771) { goto SkipChecker; }
                int ssid = -1; personmsg pem = new personmsg();
                for (int i = 0; i < Manager.scrBan.data.Count; i++)
                {
                    pem = (personmsg)Manager.scrBan.data[i];
                    if (pem.qq == e.FromQQ.Id)
                    {
                        ssid = i;
                        pem.anger += 1.1;
                        if (e.Message.Text.Length >= 160) { pem.anger += (e.Message.Text.Length / 160); }
                        if (e.Message.Text.IndexOf(pem.lmsg) >= 0 || pem.lmsg.IndexOf(e.Message.Text) >= 0) { pem.anger = pem.anger + 1.2; }
                        pem.lmsg = e.Message.Text;
                        pem.anger -= (GetTickCount() - pem.tick) / 666;
                        if (pem.anger < 0) { pem.anger = 0; }
                        if (GetTickCount() - pem.tick <= 3000)
                        {
                            switch (Math.Floor(pem.anger))
                            {
                                case (4):
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "不许刷屏");
                                    break;
                                case (5):
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "不要刷屏了啦");
                                    break;
                                case (6):
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "为什么你不愿意听我的警告呢");
                                    break;
                                case (7):
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "别再刷屏了，好吗？");
                                    break;
                                case (8):
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "不理你了！欺负人家不是管理员！");
                                    break;
                                case (9):
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "讨厌！不理你了！");
                                    break;
                                case (10):
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "哼！不想理你了！");
                                    break;
                                default:
                                    break;
                            }
                            Log("(" + e.FromGroup.Id + ")(" + i + ")anger incrasing:" + e.FromQQ.Id + "x" + pem.anger, ConsoleColor.Red);
                        }
                        if (pem.anger >= 6)
                        {
                            if (e.FromGroup.Id != 490623220 || e.FromQQ.CQApi.GetGroupMemberInfo(e.FromGroup.Id, e.FromQQ.Id).MemberType != Native.Csharp.Sdk.Cqp.Enum.QQGroupMemberType.Manage)
                            {
                                TimeSpan bantime = new TimeSpan(0, 10, 0);
                                e.FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(e.FromQQ.Id), bantime);
                            }
                        }
                        pem.tick = GetTickCount();
                        Manager.scrBan.data[i] = pem;
                    }
                }
                if (ssid == -1)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "msgcheck") == "√") { return; }
                    pem.qq = e.FromQQ.Id; pem.anger = 0; pem.tick = GetTickCount();
                    pem.lmsg = e.Message.Text;
                    Manager.scrBan.data.Add(pem);
                }
            SkipChecker:

                //Prise All
                if (r.Next(0, 1500) == 66 || e.Message.Text.ToLower() == ".nice")
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "greeting") == "√") { return; }

                    if (e.Message.Text.ToLower() == ".nice")
                    {
                        if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                    }
                    
                    string pri = "";
                    string greeting = ""; int hour = DateTime.Now.Hour;
                    if (hour >= 6 && hour < 11) greeting = "早上";
                    if (hour >= 11 && hour < 13) greeting = "中午";
                    if (hour >= 13 && hour < 17) greeting = "下午";
                    if (hour >= 17 && hour < 24) greeting = "晚上";
                    if (hour >= 0 && hour < 6) greeting = "凌晨。。。。。。？";
                    switch (r.Next(0, 9))
                    {
                        case (0): pri = "<n>好可爱（*＾-＾*）"; break;
                        case (1): pri = "<n>好厉害❤"; break;
                        case (2): pri = "(❤ ω ❤)我最喜欢<n>了"; break;
                        case (3): pri = "<n>爱你😘"; break;
                        case (4): pri = "我只爱<n>(✿◡‿◡)"; break;
                        case (5): pri = "<n> " + greeting + "好o(*￣▽￣*)ブ"; break;
                        case (6): pri = "<n> " + greeting + "好(❁´◡`❁)"; break;
                        case (7): pri = "👍🏻<n>好棒"; break;
                        case (8): pri = "( •̀ ω •́ )✧<n> tql"; break;
                        case (9): pri = "( •̀ ω •́ )✧<n> tql"; break;
                    }
                    string qname = e.FromQQ.GetGroupMemberInfo(e.FromGroup.Id).Card;
                    if (qname == "") qname = e.FromQQ.GetGroupMemberInfo(e.FromGroup.Id).Nick;
                    e.FromGroup.SendGroupMessage(pri.Replace("<n>", qname));
                    Log("Greeting:" + qname, ConsoleColor.Yellow);
                    return;
                }

                //Random Topic
                string tsay = "";
                if (e.Message.Text.IndexOf("[CQ:") < 0)
                {             
                    if ((r.Next(0, 200) == 88) || (FailAI == true))
                    {
                        if (ignore.getkey(e.FromGroup.Id.ToString(), "ai") == "√") { return; }
                        try
                        {
                            tsay = ArtificalAI.Talk(e.Message.Text, "tieba");
                            if (tsay == "")
                            {
                                FailAI = true;
                            }
                            else
                            {
                                string[] tsa = tsay.Split('。');
                                tsay = tsa[r.Next(0, tsa.Length)];
                                if (tsay.Length <= 350)
                                {
                                    FailAI = false; MessagePoster.LetSay(tsay, e.FromGroup.Id);
                                }
                                else
                                {
                                    FailAI = true;
                                }

                            }
                        }
                        catch
                        {
                            FailAI = true;
                        }
                    }
                }

                //Lock Processing
                sid = Manager.LCards.SearchFor(e.FromQQ.Id);
                if (sid != -1)
                {
                    Log("Lock target:" + sid, ConsoleColor.Yellow);
                    string tname = Manager.LCards.data[sid + 1].ToString();
                    if (e.FromQQ.CQApi.GetGroupMemberInfo(e.FromGroup.Id, e.FromQQ.Id, false).Card != tname)
                    {
                        if (e.FromGroup.SetGroupMemberVisitingCard(e.FromQQ.Id, tname))
                        {
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "unmatced card , changed:", tname);
                        }
                        else
                        {
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(1361778219), "unable to change unmatched card");
                        }

                    }
                    return;
                }

                //Word Collection
                if (e.Message.Text.IndexOf("云") >= 0)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "collection") == "√") { return; }
                    if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                    string name;
                    if (e.Message.Text.IndexOf("云") == e.Message.Text.Length - 1)
                    {
                        name = e.Message.Text.Substring(0, e.Message.Text.Length - 1).ToLower();
                        int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                        List<String> t = new List<string>();
                        for (int i = 0; i < Count; i++)
                        {
                            if (Manager.wordcollect.getkey("owner" + i, "name").ToLower().IndexOf(name) >= 0)
                            {
                                t.Add(Manager.wordcollect.getkey("repeat", "item" + i));
                            }
                        }
                        if (t.Count == 0) { return; }
                        e.FromGroup.SendGroupMessage(name + "：" + t[r.Next(0, t.Count)]);
                        return;
                    }
                    if (e.Message.Text.IndexOf("云：") < e.Message.Text.Length - 2)
                    {
                        string[] t = e.Message.Text.Split(new string[] { "云：" }, StringSplitOptions.None);
                        if (t.Length < 2) { return; }
                        if (PutRepeat(t[0], t[1], e, false) == 1)
                        {
                            e.FromGroup.SendGroupMessage("采集成功！");
                        }
                        else
                        {
                            e.FromGroup.SendGroupMessage(t[0] + "：我没说过");
                        }
                        return;
                    }
                }
                if (e.Message.Text.IndexOf("居然") >= 0)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "collection") == "√") { return; }
                    if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                    string[] p = e.Message.Text.Split(new string[] { "居然" },StringSplitOptions.None);
                    string name = p[0].ToLower();
                    int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                    List<String> t = new List<string>();
                    for (int i = 0; i < Count; i++)
                    {
                        if (Manager.wordcollect.getkey("owner" + i, "name").ToLower().IndexOf(name) >= 0)
                        {
                            string rep = Manager.wordcollect.getkey("repeat", "item" + i);
                            if(rep.ToLower().IndexOf(p[1].ToLower()) >= 0)
                            {
                                t.Add(rep);
                            }
                        }
                    }
                    if (t.Count == 0) { return; }
                    e.FromGroup.SendGroupMessage(name + "：" + t[r.Next(0, t.Count)]);
                    return;
                }
                if (e.Message.Text.IndexOf("语录集")>= 0)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "collection") == "√") { return; }
                    if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                    string[] p = e.Message.Text.Split(new string[] { "语录集" }, StringSplitOptions.None);
                    string name = p[0].ToLower();
                    int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                    List<String> t = new List<string>();
                    for (int i = 0; i < Count; i++)
                    {
                        if (Manager.wordcollect.getkey("owner" + i, "name").ToLower().IndexOf(name) >= 0)
                        {
                            t.Add(Manager.wordcollect.getkey("repeat", "item" + i));
                        }
                    }
                    if (t.Count == 0) { e.FromGroup.SendGroupMessage(name + "没有任何语录。"); return; }
                    e.FromGroup.SendGroupMessage("已将语录集推送至消息队列");
                    for(int i = t.Count - 1;i >= 0; i--)
                    {
                        MessagePoster.SimSay(name + "语录 No." + (i+1) + "\n" + t[i], e.FromGroup.Id,(t.Count - i) * 2500);
                    }
                    return;
                }

                //Function Switch
                if (e.Message.Text.StartsWith("switch"))
                {
                    if (!JudgePermission(e.FromQQ.Id, PermissionName.HeavenPermission)) { e.FromGroup.SendGroupMessage("no permission"); return; }
                    string[] p = e.Message.Text.Split(' ');
                    if (p[1] == "t") { p[1] = e.FromGroup.Id.ToString(); }
                    switch (p[2])
                    {
                        case ("clear"):
                            e.FromGroup.SendGroupMessage("cleared");
                            ignore.putkey(p[1], "log", "");
                            break;
                        case ("show"):
                            e.CQApi.SendGroupMessage(e.FromGroup.Id,
                                                     "本次机器人关闭了以下功能....\n" +
                                                     ignore.getkey(p[1], "log"));
                            break;
                        case ("send"):
                            e.CQApi.SendGroupMessage(Convert.ToInt64(p[1]),
                                                     "本次机器人关闭了以下功能....\n" + 
                                                     ignore.getkey(p[1], "log"));
                            break;
                        case ("off"):
                            string logg = "";
                            if (p[3] == "ai") { logg = "自主发言"; }
                            if (p[3] == "repeater") { logg = "智能复读"; }
                            if (p[3] == "msgcheck") { logg = "反刷屏"; }
                            if (p[3] == "group") { logg = "所有功能"; }
                            if (p[3] == "collection") { logg = "群友精彩发言收藏夹"; }
                            if (p[3] == "command") { logg = "所有指令"; }
                            if (p[3] == "csdn") { logg = "从CSDN搜索文档"; }
                            if (p[3] == "msdn") { logg = "从MSDN搜索文档"; }
                            if (p[3] == "tk") { logg = "群内聊天"; }
                            if (p[3] == "tko") { logg = "旧版群内聊天"; }
                            if (p[3] == "wall") { logg = "取得随机桌面壁纸"; }
                            if (p[3] == "fsleep") { logg = "强制关闭服务器主机"; }
                            if (p[3] == "ban") { logg = "禁言群内某个成员"; }
                            if (p[3] == "aban") { logg = "全员禁言"; }
                            if (p[3] == "sgdnight") { logg = "顺序艾特所有成员发送问候"; }
                            if (p[3] == "sgdnightf♂") { logg = "私聊所有成员发送问候"; }
                            if (p[3] == "man") { logg = "设置管理员"; }
                            if (p[3] == "unman") { logg = "取消管理员"; }
                            if (p[3] == "pms") { logg = "机器人内部权限操作"; }
                            if (p[3] == "honor") { logg = "设置专属头衔"; }
                            if (p[3] == "prs") { logg = "点赞十次"; }
                            if (p[3] == "lkc") { logg = "监视群员的名片并锁定"; }
                            if (p[3] == "pop") { logg = "宣布本群最长的复读"; }
                            if (p[3] == "info") { logg = "查看自己的机器人内部信息"; }
                            if (p[3] == "ut") { logg = "传说之下猜角色文字游戏"; }
                            if (p[3] == "utf") { logg = "传说之下猜台词文字游戏"; }
                            if (p[3] == "bvoid") { logg = "虚拟人生文字游戏"; }
                            if (p[3] == "word") { logg = "随机语录查看"; }
                            if (p[3] == "greeting") { logg = "随机夸人，发送问候"; }
                            ignore.putkey(p[1], "log", ignore.getkey(p[1], "log") + "\n" + p[3] + " " + logg);
                            e.FromGroup.SendGroupMessage(p[3] + " : off");
                            ignore.putkey(p[1], p[3], "√");
                            break;
                        case ("on"):
                            e.FromGroup.SendGroupMessage(p[3] + " : on");
                            ignore.putkey(p[1], p[3], "");
                            break;
                        default:
                            e.FromGroup.SendGroupMessage("unknown switch command");
                            break;
                    }
                    return;
                }

                //Console Processing
                if (e.Message.Text.StartsWith("."))
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "command") == "√") { return; }

                    try
                    {
                        string user = e.FromQQ.Id.ToString();
                        PermissionName pe = PermissionName.AirPermission;
                        pe = (PermissionName)Convert.ToInt64(info.getkey(e.FromQQ.Id.ToString(), "permission"));
                        string pename = GetPermissionName(pe);

                        string pcmd = e.Message.Text.Replace("[CQ:at,qq=", "").Replace("]", "");
                        do
                        {
                            pcmd = pcmd.Replace("  ", " ");
                        } while (pcmd.IndexOf("  ") >= 0);
                        pcmd = pcmd.Remove(0, 1).Insert(0, "ik ");
                        string[] p = pcmd.Split(' ');

                        if (ignore.getkey(e.FromGroup.Id.ToString(), p[1]) == "√") { return; }
                        switch (p[1])
                        {
                            case ("clearwords"):
                                if (!JudgePermission(e.FromQQ.Id,PermissionName.Error404)){ return; }
                                ProtectCount = 404;
                                int RCount = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                Storage stw = new Storage("word-clear");
                                Storage otw = new Storage("wordcollections");
                                otw.Restore();
                                int SCount = 0;
                                for (int i = 0; i < RCount; i++)
                                {
                                    string[] ctemp = otw.getkey("repeat", "item" + i).ToLower().Split(new string[] { "[cq:image,file=" }, StringSplitOptions.None);
                                    string cmsg = ctemp[0];
                                    for (int j = 1; j < ctemp.Length; j++)
                                    {
                                        string[] temp = ctemp[j].Split(']');
                                        if (temp.Length > 1) { cmsg += temp[1]; }
                                    }
                                        
                                    if (cmsg.Trim().Length >= 3)
                                    {
                                        Log("Reserve " + SCount, ConsoleColor.Green);
                                        stw.putkey("owner" + SCount, "name", otw.getkey("owner" + i, "name"),false);
                                        stw.putkey("repeat", "item" + SCount, otw.getkey("repeat", "item" + i),false);
                                        SCount++;
                                    }
                                    Thread.Sleep(10);
                                    Log("Clearing " + i + "/" + RCount, ConsoleColor.Green);
                                }
                                stw.putkey("repeat", "count", SCount.ToString(),false);
                                stw.Store();
                                e.FromGroup.SendGroupMessage("本次删除了" + (RCount - SCount) + "条语录，感觉自己萌萌哒~");
                                break;
                            case ("word"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                int Index = r.Next(0, Count);
                                e.FromGroup.SendGroupMessage(
                                    Manager.wordcollect.getkey("owner" + Index, "name") + "：" +
                                    Manager.wordcollect.getkey("repeat", "item" + Index)
                                    );
                                break;
                            case ("utf"):
                                if (UT.targetg == 0)
                                {
                                    e.FromGroup.SendGroupMessage("a new game : 'undertale guessing' has been on");
                                    UT.mode = 1;
                                    UT.targetg = e.FromGroup.Id; UT.round = 0; UT.ps.Clear();
                                    UT.tick = GetTickCount();
                                    UT.nextRound();
                                    e.FromGroup.SendGroupMessage("round " + UT.round + "(result:20s later)" + UT.dialog);
                                }
                                else
                                {
                                    if (UT.targetg == e.FromGroup.Id)
                                    {
                                        e.FromGroup.SendGroupMessage("the game has been on");
                                    }
                                    else
                                    {
                                        e.FromGroup.SendGroupMessage("another game has been on");
                                    }
                                }
                                break;
                            case ("ut"):
                                if (UT.targetg == 0)
                                {
                                    e.FromGroup.SendGroupMessage("a new game : 'undertale guessing characters' has been on");
                                    UT.mode = 0;
                                    UT.targetg = e.FromGroup.Id; UT.round = 0; UT.ps.Clear();
                                    UT.tick = GetTickCount();
                                    UT.nextRound();
                                    e.FromGroup.SendGroupMessage("round " + UT.round + "(result:20s later)" + UT.dialog);
                                }
                                else
                                {
                                    if (UT.targetg == e.FromGroup.Id)
                                    {
                                        e.FromGroup.SendGroupMessage("the game has been on");
                                    }
                                    else
                                    {
                                        e.FromGroup.SendGroupMessage("another game has been on");
                                    }
                                }
                                break;
                            case ("pop"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (hIndex > -1)
                                {
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
                                    e.FromGroup.SendGroupMessage(hhmsg.msg);
                                }
                                else
                                {
                                    e.FromGroup.SendGroupMessage("none");
                                }
                                break;
                            case ("bvoid"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (VoidLifes.IsStart())
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " the game has been on");
                                }
                                else
                                {
                                    if (p.Length < 3) { throw new Exception("ERROR:please name for the character:ink bvoid <name>"); }
                                    VoidLifes.BeginGame(p[2]); VoidLifes.TargetGroup = e.FromGroup.Id;
                                    VoidLifes.JoinGame(e.FromQQ.Id); VoidLifes.OwnerQQ = e.FromQQ.Id;
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " game begins");
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " you've joined the game");
                                }
                                break;
                            case ("jvoid"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (e.FromGroup.Id != VoidLifes.TargetGroup)
                                {
                                    if (VoidLifes.TargetGroup == 0)
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " not game is playing");
                                    }
                                    else
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " the game has been on in other group");
                                    }

                                }
                                else
                                {
                                    if (VoidLifes.IsJoined(e.FromQQ.Id))
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " repeative operation");
                                    }
                                    else
                                    {
                                        VoidLifes.JoinGame(e.FromQQ.Id);
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " you've joined the game");
                                    }
                                }
                                break;
                            case ("svoid"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (e.FromGroup.Id != VoidLifes.TargetGroup)
                                {
                                    if (VoidLifes.TargetGroup == 0)
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " not game is playing");
                                    }
                                    else
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " the game has been on in other group");
                                    }

                                }
                                else
                                {
                                    if (VoidLifes.OwnerQQ != e.FromQQ.Id)
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " you are not the game's leader");
                                    }
                                    else
                                    {
                                        if (VoidLifes.IsPlaying())
                                        {
                                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " the game has been on");
                                        }
                                        else
                                        {
                                            Log("game starts", ConsoleColor.Green);
                                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " game starts");
                                            VoidLifes.StartRound();
                                        }
                                    }
                                }
                                break;
                            case ("cvoid"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (e.FromGroup.Id != VoidLifes.TargetGroup)
                                {
                                    if (VoidLifes.TargetGroup == 0)
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " not game is playing");
                                    }
                                    else
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " the game has been on in other group");
                                    }

                                }
                                else
                                {
                                    if (VoidLifes.OwnerQQ != e.FromQQ.Id)
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " you are not the game's leader");
                                    }
                                    else
                                    {
                                        if (!VoidLifes.IsPlaying())
                                        {
                                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " not game is playing");
                                        }
                                        else
                                        {
                                            Log("game closed", ConsoleColor.Green);
                                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " game closed");
                                            VoidLifes.EndGame();
                                        }
                                    }
                                }
                                break;
                            case ("msdn"):
                                //msdn searching
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "该功能正在回炉改造中..."); return;
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "msdn"));
                                break;
                            case ("csdn"):
                                //csdn searching
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "该功能正在回炉改造中..."); return;
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "csdn"));
                                break;
                            case ("wall"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + Wallpaper.GetWallpaper());
                                break;
                            case ("fsleep"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.MasterPermission)) { return; }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(1361778219), " sudo sleep !");
                                ShellExecute(IntPtr.Zero, "open", @"shutdown", "-s -t 120", "", ShowCommands.SW_SHOWNORMAL);
                                break;
                            case ("ban"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 4) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]);
                                if (Convert.ToInt16(p[3]) > 0)
                                {
                                    TimeSpan ctime = new TimeSpan(0, Convert.ToInt16(p[3]), 0);
                                    if (e.FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(qq), ctime))
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operated successfully");
                                    }
                                    else
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation denied");
                                    }
                                }
                                else
                                {
                                    if (e.FromGroup.RemoveGroupMemberBanSpeak(Convert.ToInt64(qq)))
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operated successfully");
                                    }
                                    else
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation denied");
                                    }
                                }
                                break;
                            case ("aban"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                bool bswitch = Convert.ToBoolean(p[2]);
                                if (bswitch)
                                {
                                    if (e.FromGroup.SetGroupBanSpeak())
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operated successfully");
                                    }
                                    else
                                    {
                                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation denied");
                                    }
                                }
                                else
                                {
                                    e.FromGroup.RemoveGroupBanSpeak();
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operated successfully");
                                }
                                break;
                            case ("man"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.MasterPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]);
                                e.FromGroup.SetGroupManage(Convert.ToInt64(qq));
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(Convert.ToInt64(qq)), "you are manager now");
                                break;
                            case ("unman"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.MasterPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]);
                                e.FromGroup.RemoveGroupManage(Convert.ToInt64(qq));
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(Convert.ToInt64(qq)), "you are not manager now");
                                break;
                            case ("sgdnightf♂"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.MasterPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                mem = e.FromGroup.GetGroupMemberList();
                                for (int i = 0; i < mem.Count; i++)
                                {
                                    mem[i].QQ.SendPrivateMessage(p[2]);
                                    Log("Wish:" + i, ConsoleColor.Yellow);
                                    //e.FromGroup.SendGroupMessage(atstr, p[2]);
                                    Thread.Sleep(500);
                                }
                                break;
                            case ("sgdnight"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.MasterPermission)) { return; }
                                string atstr = "";
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                mem = e.FromGroup.GetGroupMemberList();
                                for (int i = 0; i < mem.Count; i += 10)
                                {
                                    atstr = "";
                                    for (int s = i; s < i + 10; s++)
                                    {
                                        Log("Wish:" + s, ConsoleColor.Yellow);
                                        if (s > mem.Count) { break; }
                                        atstr = atstr + CQApi.CQCode_At(mem[s].QQ.Id);
                                    }
                                    e.FromGroup.SendGroupMessage(atstr, p[2]);
                                    Thread.Sleep(3000);
                                }
                                break;
                            case ("pms"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]);
                                string luser = qq.ToString();
                                sid = (info.FirstStore ? -1 : 1);
                                if (sid != -1)
                                {
                                    if (p.Length < 4) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                    if (Convert.ToInt64(info.getkey(luser, "permission")) >= Convert.ToInt64(pe)) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied to change pms '" + GetPermissionName((PermissionName)(Convert.ToInt64(info.getkey(qq.ToString(), "permission")))) + "'(level " + Convert.ToInt64(info.getkey(qq.ToString(), "permission")) + ") ", CQApi.CQCode_At(Convert.ToInt64(qq))); return; }
                                    if (Convert.ToInt64(p[3]) >= Convert.ToInt64(pe)) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied to give pms '" + GetPermissionName((PermissionName)(Convert.ToInt64(p[3]))) + "'(level " + Convert.ToInt64(p[3]) + ")"); return; }
                                    info.putkey(luser, "permission", p[3]);
                                    pe = (PermissionName)Convert.ToInt64(p[3]);
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)) + " pms:" + GetPermissionName(pe) + "(level " + Convert.ToInt64(pe) + ") pid:" + sid);
                                }
                                else
                                {
                                    info.putkey(luser, "permission", "1");
                                    sid = 1;
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                        CQApi.CQCode_At(Convert.ToInt64(qq)) + " has permission now , pid:" + sid);
                                }
                                break;
                            case ("honor"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                if (e.FromQQ.SetGroupMemberForeverExclusiveTitle(e.FromGroup.Id, p[2]) == false)
                                {
                                    throw new Exception("operation denied");
                                }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operated successfully");
                                break;
                            case ("prs"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (e.FromQQ.SendPraise(10) == false)
                                {
                                    throw new Exception("operation denied");
                                }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operated successfully");
                                break;
                            case ("tk"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                long lstime = 0;
                                for (int i = 0; i < MessagePoster.delays.Count; i++)
                                {
                                    MessagePoster.delaymsg d = MessagePoster.delays[i];
                                    if (d.group == e.FromQQ.Id)
                                    {
                                        lstime = d.time; return;
                                    }
                                }
                                if (GetTickCount() - lstime > 3000)
                                {
                                    tsay = ArtificalAI.Talk(ques, "tieba");
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id));
                                    if (tsay != "")
                                    {
                                        MessagePoster.LetSay(tsay, e.FromGroup.Id);
                                    }
                                    else
                                    {
                                        MessagePoster.LetSay("no search result", e.FromGroup.Id);
                                    }
                                }
                                break;
                            case ("tko"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                MessagePoster.LetSay("tko was not longer support.", e.FromGroup.Id);
                                return;
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                tsay = ArtificalAI.Talk(ques, "baidu");
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id));
                                if (tsay != "")
                                {
                                    MessagePoster.LetSay(tsay, e.FromGroup.Id);
                                }
                                else
                                {
                                    MessagePoster.LetSay("no search result", e.FromGroup.Id);
                                }
                                break;
                            case ("info"):
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                    "\nuserid: " + e.FromQQ.Id +
                                    "\npermission: '" + pename + "'(level " + Convert.ToInt64(pe) +
                                    ")\ncoins: " + Convert.ToDouble(info.getkey(user, "coins"))
                                    );
                                break;
                            case ("lkc"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 4) { throw new Exception("'" + p[1] + "' was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]); string lname = p[3];
                                sid = Manager.LCards.SearchFor(qq);
                                if (sid != -1)
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)),
                                        "operation repeated:" + sid +
                                        " , updated states.");
                                    Manager.LCards.data[sid + 1] = lname;
                                    Manager.LCards.SaveData();
                                }
                                else
                                {
                                    Manager.LCards.data.Add(qq);
                                    Manager.LCards.data.Add(lname);
                                    Manager.LCards.SaveData();
                                    sid = Manager.LCards.SearchFor(qq);
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)),
                                        "operated successfully add:" + sid +
                                        " lkc:" + Manager.LCards.data[sid + 1]);
                                }
                                break;
                            case ("help"):
                                string cmdstr = "";
                                if (CanMatch(e.Message.Text,"*","*", "msdn", "搜索", "资料", "文档", "国外", "微软", "巨硬", "microsoft")) { cmdstr += "*msdn <content> to search in msdn (weak)\n"; }
                                if (CanMatch(e.Message.Text,"*", "*", "csdn", "搜索", "资料", "文档", "国内")) { cmdstr += "csdn <content> to search in csdn (weak)\n"; }
                                if (CanMatch(e.Message.Text,"*",  "壁纸", "桌面", "背景", "图片", "风景", "wall", "param")) { cmdstr += "wall to get a wallpaper\n"; }
                                if (CanMatch(e.Message.Text,"*",  "404", "睡觉", "晚安", "关机", "强制", "force", "sleep")) { cmdstr += "#fsleep to force 404 to shutdown in 2 min\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "禁言", "违规", "刷屏", "shit", "bun")) { cmdstr += "*ban <qq> <minutes> to ban somebody some time\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "禁言", "违规", "刷屏", "shit", "全员", "所有人", "bun")) { cmdstr += "*aban <bool> to ban all\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "危险", "祝福", "超级", "问候", "全员", "所有人", "super", "goodnight")) { cmdstr += "#sgdnight <content> to at all and send msg\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "危险", "祝福", "超级", "问候", "全员", "所有人", "super", "goodnight")) { cmdstr += "#sgdnightf♂ <content> to send private msg to all\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "新", "任命", "我要", "提权", "权限", "地位", "manger")) { cmdstr += "#man <qq> to set a new manager\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "撤", "罢免", "我要", "降权", "权限", "地位", "manger")) { cmdstr += "#unman <qq> to cancel a manager\n"; }
                                if (CanMatch(e.Message.Text,"*",  "permission", "令牌", "修改", "系统", "更改", "权限", "地位", "permiss", "pemission", "permision")) { cmdstr += "!pms <qq> <pid> to give others pms\n"; }
                                if (CanMatch(e.Message.Text,"*",  "聊天", "无逻辑", "无聊", "百度", "搜索", "骚")) { cmdstr += "tk <content> to talk with ik\n"; }
                                if (CanMatch(e.Message.Text,"*",  "聊天", "无逻辑", "无聊", "百度", "搜索", "骚")) { cmdstr += "tko <content> to talk with ik\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "头衔", "名片", "永久")) { cmdstr += "*honor <content> to give yourself a title\n"; }
                                if (CanMatch(e.Message.Text,"*",  "赞", "资料卡", "名片", "个人", "prise", "prase")) { cmdstr += "*prs to praise you 10s\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "头衔", "名片", "锁定", "昵称", "lock")) { cmdstr += "*lkc <qq> <name> to lock one's card\n"; }
                                if (CanMatch(e.Message.Text,"*",  "复读", "信息", "统计", "数据", "最热", "今日", "发言", "热词", "poplular")) { cmdstr += "pop to output today's popular sentence\n"; }
                                if (CanMatch(e.Message.Text,"*",  "信息", "统计", "用户", "数据", "人数")) { cmdstr += "info to output your user info\n"; }
                                if (CanMatch(e.Message.Text,"*",  "game", "游戏", "UT", "ut", "传说之下", "undertale", "Undertale", "UnderTale", "猜词", "猜台词")) { cmdstr += "ut to start a ut guessing characters game\n"; }
                                if (CanMatch(e.Message.Text,"*",  "game", "游戏", "UT", "ut", "传说之下", "undertale", "Undertale", "UnderTale", "猜词", "猜台词")) { cmdstr += "ut to start a ut guessing game\n"; }
                                if (CanMatch(e.Message.Text,"*",  "game", "游戏", "void", "Void", "虚拟", "人生", "life", "虚假", "live")) { cmdstr += "bvoid <character name> to create a void game .\njvoid to join current void game .\ncvoid to close the game\n"; }
                                if (CanMatch(e.Message.Text,"*",  "praise", "表扬", "问候", "祝福", "好")) { cmdstr += "nice to greet you\n"; }
                                if (CanMatch(e.Message.Text,"*",  "word", "collect", "复读", "语录", "录")) { cmdstr += "word to output a word collection\n"; }

                                if (cmdstr == "")
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                    "unknown command '" + p[1] + "'");
                                }
                                else
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                    cmdstr + "your pms:" + pename + "(level " + Convert.ToInt64(pe) + ")");
                                }
                                break;
                        }
                    }
                    catch (Exception err)
                    {
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), err.Message);
                        Log(err.StackTrace + "\n" + err.Message, ConsoleColor.Red);
                    }
                    return;
                }

            }
            catch(Exception ex)
            {
                Log(ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException, ConsoleColor.Red);
            }
        }
    }
}
