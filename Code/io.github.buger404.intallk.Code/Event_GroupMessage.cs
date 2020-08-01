using Native.Csharp.Sdk.Cqp;
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
using System.Drawing;
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
using System.Net;
using Repeater;
using Lemony.SystemInfo;
using System.Windows.Forms.DataVisualization.Charting;
using MSXML2;

namespace io.github.buger404.intallk.Code
{
    // 添加引用 IGroupMessage
    public class Event_GroupMessage: IGroupMessage
    {
        public static Storage info = new Storage("userinfo");
        public static Storage ignore = new Storage("ignore");
        public static Storage board = new Storage("board");
        public static Storage achive = new Storage("achivement");
        public static Group Current;
        public bool force = false;
        public SystemInfo sysinfo = new SystemInfo();
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
            JAILPermission = -1, //监狱
            AirPermission = 0, //参观级别的权限
            UserPermission = 1, //正常级别的权限
            SupermanPermission = 2, //可以访问有关群管理的内容
            MasterPermission = 3, //可以访问直接关系到我自己的内容
            HeavenPermission = 4, //可以访问我自己的计算机
            LOVEPermission = 32766, //我的亲密好友
            Error404 = 32767 //我自己
        }
        private struct personmsg
        {
            public long tick;
            public double anger;
            public long qq;
            public string lmsg;
            public List<QQMessage> msglist;
        }
        public static int recordtime = 0;
        private static bool IsWalling = false;
        public long UNCMD = 0;
        private static string GetPermissionName(PermissionName pe)
        {
            switch (pe)
            {
                case (PermissionName.JAILPermission): return "监禁在小黑屋中";
                case (PermissionName.AirPermission): return "Air";
                case (PermissionName.UserPermission): return "User";
                case (PermissionName.SupermanPermission): return "Superman";
                case (PermissionName.MasterPermission): return "Master";
                case (PermissionName.HeavenPermission): return "Heaven";
                case (PermissionName.LOVEPermission): return "LOVE";
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
        public static void Achive(long qq,string goal,Group group)
        {
            string lsa = achive.getkey(qq.ToString(), "achivements");
            if (lsa == null) lsa = "";
            if (lsa.IndexOf(goal + "\n") >= 0) return;
            achive.putkey(qq.ToString(), "achivements", lsa + DateTime.Today.ToShortDateString() + "  " + goal + "\n");
            group.SendGroupMessage(CQApi.CQCode_Record("goal.mp3"));
            group.SendGroupMessage(CQApi.CQCode_At(qq) + " 获得了成就『" + goal + "』！");
        }
        public void PutRepeat(string name, string text)
        {
            int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
            Manager.wordcollect.putkey("repeat", "item" + Count, text);
            Manager.wordcollect.putkey("owner" + Count, "name", name);
            Count++;
            Manager.wordcollect.putkey("repeat", "count", Count.ToString());
        }
        public int PutRepeat(string name,string text, Group FromGroup,CQApi CQApi,bool SkipCheck = true)
        {
            if (SkipCheck) { goto DontCheck; }
            bool exit = true;MainThread.MessagePoster.HotMsg hmsg;
            for (int i = 0; i < Manager.Hots.data.Count; i++)
            {
                hmsg = (MainThread.MessagePoster.HotMsg)Manager.Hots.data[i];
                if(FromGroup.Id == hmsg.group)
                {
                    long qq = Convert.ToInt64(hmsg.qq.Split(';')[0]);
                    GroupMemberInfo g = FromGroup.GetGroupMemberInfo(qq);
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
                            Nick = CQApi.GetStrangerInfo(qq).Nick;
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
            public long LastQQ;
            public long SingleTicks;
        }
        public static List<GroupBUGCheck> bugs = new List<GroupBUGCheck>();
        public static int ProtectCount = 0;
        private void DownLoadFace(long qq)
        {
            WebClient w = new WebClient();
            w.DownloadFile("http://q.qlogo.cn/headimg_dl?dst_uin=" + qq + "&spec=100", 
                           MessagePoster.workpath + "\\data\\image\\" + qq + ".jpg");
            w.Dispose();
        }
        private void SummarWord(string name,string content, CQGroupMessageEventArgs e)
        {
            int rp = Convert.ToInt32(16 / 0.75);
            Bitmap b = new Bitmap(
                       40 + content.Length * rp + 10, 
                       40 + content.Split(new string[]{"\r\n"},StringSplitOptions.None).Length * rp
                       );
            Graphics g = Graphics.FromImage(b);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Font font = new Font("Microsoft Yahei", 16);
            g.Clear(Color.White);
            g.DrawString(content, font, Brushes.Red, new PointF(20, 20 - 8 + 1));

            DownLoadFace(e.FromQQ.Id);
            Image i = Image.FromFile(MessagePoster.workpath + "\\data\\image\\" + e.FromQQ.Id + ".jpg");
            g.DrawImage(i, new Point(0, 0));
            
            b.Save(MessagePoster.workpath + "\\data\\image\\test.png");
            e.FromGroup.SendGroupMessage(CQApi.CQCode_Image("test.png"));

            i.Dispose();b.Dispose();g.Dispose();font.Dispose();
        }
        private bool JudgePermission(long qq,PermissionName Required)
        {
            PermissionName pe = PermissionName.AirPermission;
            pe = (PermissionName)Convert.ToInt64(info.getkey(qq.ToString(), "permission"));
            return (pe >= Required || force);
        }
        private string JP(long qq, PermissionName Required,string cmd,long group)
        {
            PermissionName pe = PermissionName.AirPermission;
            pe = (PermissionName)Convert.ToInt64(info.getkey(qq.ToString(), "permission"));
            
            if (ignore.getkey(group.ToString(),cmd.Replace(".","").Split(' ')[0]) == "√")
            {
                if (force)
                {
                    return "";
                }
                else
                {
                    return "OFF";
                }
            }
            else
            {
                return (pe >= Required || force ? "" : ((int)Required).ToString());
            }
                
        }
        public static void Artifical(Group g)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            string word = "";
            
            if (ignore.getkey(g.Id.ToString(), "ai") == "√") { return; }
            int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
            word = Manager.wordcollect.getkey("repeat", "item" + r.Next(0, Count));

            if (r.Next(0,3) == 1)
            {
                try
                {
                    word = ArtificalAI.Talk(word, "tieba");
                    if (word != "")
                    {
                        string[] tsa = word.Split('。');
                        word = tsa[r.Next(0, tsa.Length)];
                        if (word.Length <= 350)
                        {
                             MessagePoster.LetSay(word, g.Id, 0, r.Next(0, 2) == 1);
                        }
                    }
                }
                catch
                {
                    
                }
                return;
            }

            g.SendGroupMessage(word);
        }
        public static bool IsCmd(string s)
        {
            return( s.StartsWith(".") ||
                    s.IndexOf("云") >= 0 ||
                    s.IndexOf("居然") >= 0 ||
                    s.IndexOf("语录集") >= 0 ||
                    s.StartsWith("switch"));
        }
        public string GetName(Group FromGroup, CQApi CQApi, long qq, bool shortname = true)
        {
            string name = "";
            try
            {
                name = FromGroup.GetGroupMemberInfo(qq).Card;
                if (name == "") name = FromGroup.GetGroupMemberInfo(qq).Nick;
            }
            catch
            {
                name = qq.ToString();
            }
            if (name.Length > 6 && shortname) { name = name.Substring(0, 6) + "..."; }
            name = name.Replace(",", "，");
            return name;
        }
        public string GetFullName(Group FromGroup,long qq)
        {
            string name = "";
            try
            {
                name = FromGroup.GetGroupMemberInfo(qq).Card;
                name = name + "<" + FromGroup.GetGroupMemberInfo(qq).Nick + ">";
            }
            catch
            {
                name = qq.ToString();
            }
            return name;
        }
        private void AddCmdTab(PermissionName pms,string cmd,string describe,List<DrawTable.tabs> tab,QQ FromQQ,Group FromGroup)
        {
            string str = JP(FromQQ.Id, pms, cmd.Replace(".", ""), FromGroup.Id);
            if(str != "")
            {
                UNCMD++;
                return;
            }
            //tab.Add(new DrawTable.tabs(str, Color.Red, Color.Transparent));
            tab.Add(new DrawTable.tabs(cmd, Color.Black, Color.Transparent));
            tab.Add(new DrawTable.tabs(describe, Color.Gray, Color.Transparent));
        }
        public static double CompareStr(string s1, string s2)
        {
            int c1 = 0; string s; string s3;
            s = (s1.Length > s2.Length ? s1 : s2);
            s3 = (s1.Length < s2.Length ? s1 : s2);
            for (int i = 0; i < s3.Length; i++)
            {
                for (int j = 0; j < s.Length; j++)
                {
                    if (s[j] == s3[i])
                    {
                        s.Remove(j, 1);
                        c1++; break;
                    }
                }
            }
            double ret = (s3.Length * 1f / s.Length * 1f) * 0.3 + (c1 * 1f / s.Length * 1f) * 0.7;
            ret = Math.Pow(ret * 2, 2);
            return Math.Pow(ret / 2, 2);
        }
        // 接收事件
        public void GroupMessage(object sender, CQGroupMessageEventArgs e)
        {
            Current = e.FromGroup;
            if(e.FromQQ.Id == 2487411076)
            {
                try
                {
                    if((DateTime.Now - DreamYCheater.FightTime).TotalMilliseconds <= 1250)
                    {
                        if(e.Message.Text.IndexOf("活力不足") >= 0)
                        {
                            DreamYCheater.PauseFight = true;
                            e.FromGroup.SendGroupMessage("本机器人已悉知活力不足，暂停刷怪。");
                        }
                    }
                    if(e.Message.Text.IndexOf("dy 挑战1/") >= 0)
                    {
                        string[] dys = e.Message.Text.Split(new string[] { "dy 挑战" }, StringSplitOptions.None)[1].Split('：')[0].Split('/');
                        DreamYCheater.LastestFight = dys[dys.Length - 1];
                    }
                }
                catch
                {
                    DreamYCheater.LastestFight = "5";
                }
            }
            ExcuteCmd(e.FromGroup, e.FromQQ, e.Message, e.CQApi, e.Message.Text);
            if(DateTime.Now.Hour == 4)
            {
                Achive(e.FromQQ.Id, "深夜霸王", e.FromGroup);
            }
            if (DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
            {
                Achive(e.FromQQ.Id, "时间操纵", e.FromGroup);
            }
            e.Handler = true;
        }
        public long LongTime()
        {
            return (DateTime.Now.Hour * 100 + DateTime.Now.Minute);
        }
        public static string SeekSX(string word)
        {
            XMLHTTP x = new XMLHTTP();
            x.open("POST", "https://lab.magiconch.com/api/nbnhhsh/guess",false);
            x.setRequestHeader("content-type", "application/json");
            x.send("{\"text\":\"" + word + "\"}");
            return x.responseText.Split('[')[2].Split(']')[0];
        }
        public void ExcuteCmd(Group FromGroup,QQ FromQQ,QQMessage Message,CQApi CQApi,string cmd)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            try
            {
                //WHEN BUG Protection
                int bi = bugs.FindIndex(m => m.Group == FromGroup.Id); GroupBUGCheck gbc;
                long biT = 0;
                if (ProtectCount >= 3) 
                {
                    if (bi != -1)
                    {
                        if (GetTickCount() - bugs[bi].Tick <= 32) 
                        {
                            ProtectCount++; 
                            MessagePoster.ReportBUGTime = GetTickCount() + 3000;
                            Log($"Refresh protection:{ProtectCount},TIME:{MessagePoster.ReportBUGTime}");
                        }
                        gbc = bugs[bi]; gbc.Tick = GetTickCount(); bugs[bi] = gbc;
                    }
                    if (cmd == ".bugclose")
                    {
                        if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                        ProtectCount = 0;
                        FromGroup.SendGroupMessage("保护模式已解除。");
                        return;
                    }
                    if (cmd == ".okay?")
                    {
                        int ti = r.Next(0, 5);
                        FromGroup.SendGroupMessage(CQApi.CQCode_Image("die.png"));
                        /**if (ti == 0) FromGroup.SendGroupMessage("啊我死了(￣﹃￣)");
                        if (ti == 1) FromGroup.SendGroupMessage("(o′┏▽┓｀o) 黑嘴黑嘴死了。。。");
                        if (ti == 2) FromGroup.SendGroupMessage("(っ °Д °;)っ黑嘴死翘翘了。");
                        if (ti == 3) FromGroup.SendGroupMessage("黑嘴 R.I.P");
                        if (ti == 4) FromGroup.SendGroupMessage("(´。＿。｀)黑嘴已死，有事烧纸");**/
                        return;
                    }
                    if (IsCmd(cmd))
                    {
                        string bugr = 
                                    "发现消息流量异常，" +
                                    "正处于保护模式。\n" +
                                    "请发送.bugclose解除切断（需要32766权限）。" +
                                    $"\n异常消息流量：{Event_GroupMessage.ProtectCount}条/毫秒，切断将自动在{Event_GroupMessage.ProtectCount * 10}秒后解除。";
                        FromGroup.SendGroupMessage(CQApi.CQCode_Image("dietip.png"));
                    }
                    return; 
                }
                if(bi == -1)
                {
                    bugs.Add(new GroupBUGCheck { Group = FromGroup.Id,Tick = GetTickCount() });
                    Log("New group");
                }
                else
                {
                    //消息间隔小于32ms的群不是抽风就是机器人不正常
                    biT = GetTickCount() - bugs[bi].Tick;
                    if (GetTickCount() - bugs[bi].Tick <= 32) 
                    {
                        ProtectCount++;
                        Log($"Refresh protection:{ProtectCount},TIME:{MessagePoster.ReportBUGTime}");
                    }
                    if (ProtectCount >= 3) 
                    {
                        Achive(FromQQ.Id, "黑嘴杀手", FromGroup);
                        MessagePoster.ReportBUGTime = GetTickCount()+3000;
                        Log("!!!!!!!!!\nBUG , STOPPED WORKING.\n!!!!!!", ConsoleColor.Red);
                    }
                    if (GetTickCount() - bugs[bi].Tick >= 3000 && ProtectCount > 0) 
                    {
                        Log("Close protection");
                        ProtectCount = 0; 
                    }
                    gbc = bugs[bi];gbc.Tick = GetTickCount();
                    if (gbc.LastQQ != FromQQ.Id)
                    {
                        gbc.LastQQ = FromQQ.Id;
                        gbc.SingleTicks = 0;
                    }
                    else
                    {
                        gbc.SingleTicks++;
                        if (gbc.SingleTicks == 5) { Achive(FromQQ.Id, "自言自语", FromGroup); }
                        if (gbc.SingleTicks == 10) { Achive(FromQQ.Id, "盛夏飘雪", FromGroup); }
                        if (gbc.SingleTicks == 20) { Achive(FromQQ.Id, "赤道暴风雪", FromGroup); }
                    }
                    bugs[bi] = gbc;
                }

                if (ignore.getkey(FromGroup.Id.ToString(), "group") == "√" && force == false) { return; }
                string boid = board.getkey(FromGroup.Id.ToString(), "links");
                if (boid != "" && boid != null && boid != "null")
                {
                    Group boardcast = new Group(CQApi,long.Parse(boid));
                    boardcast.SendGroupMessage(GetFullName(FromGroup, FromQQ.Id) + "：\n" + cmd);
                    Log("Boardcast : " + boid, ConsoleColor.Yellow);
                }
                if (!IsCmd(cmd)) Repeaters.SpeakOnce(FromQQ.Id, FromGroup.Id);
                Log($"({FromGroup.Id},{biT}ms)Message:" + FromQQ.Id + "," + cmd, ConsoleColor.Cyan);
                //Moring Protection
                Storage sys = new Storage("system");
                if (sys.getkey("root", "sleep") == "zzz")
                {
                    if (cmd.StartsWith("."))
                    {
                        FromGroup.SendGroupMessage(CQApi.CQCode_Image("sleep.png"));
                    }
                    return;
                }

                int sid; double qq = 0;
                List<Native.Csharp.Sdk.Cqp.Model.GroupMemberInfo> mem;
                string ques = "";
                int hIndex = -1; MainThread.MessagePoster.HotMsg hhmsg = new MainThread.MessagePoster.HotMsg();
                force = false;
                //Force
                if (cmd.EndsWith("(force)"))
                {
                    if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                    force = true;
                }
                //劝学Master
                if (FromQQ.Id == 1361778219)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "劝学master") != "√" || force == true)
                    {
                        int mai = r.Next(0, 6);
                        string[] mas = new string[]
                        {
                        "给我滚去学习！",
                        "干啥啥不行，水群第一名！",
                        "再不去学习我就把你吃掉!",
                        "滚！去！学！习！！！",
                        "水群个鬼，去学习！",
                        "学习完了吗？"
                        };
                        if (LongTime() >= 1430 && LongTime() <= 1705)
                        {
                            FromGroup.SendGroupMessage(FromQQ.CQCode_At(), mas[mai]);
                        }
                        if (LongTime() >= 800 && LongTime() <= 1130)
                        {
                            FromGroup.SendGroupMessage(FromQQ.CQCode_At(), mas[mai]);
                        }
                    }
                        
                }
                //保护模式
                if (cmd == ".bugclose")
                {
                    FromGroup.SendGroupMessage("no necessity, the bot works properly.");
                }
                if (cmd == ".okay?")
                {
                    int ti = r.Next(0, 5);
                    FromGroup.SendGroupMessage(CQApi.CQCode_Image("alive.gif"));
                    /**if (ti == 0) FromGroup.SendGroupMessage("/_ \\黑嘴活着呀。");
                    if (ti == 1) FromGroup.SendGroupMessage("⊙﹏⊙∥咋啦咋啦，黑嘴活着哦。");
                    if (ti == 2) FromGroup.SendGroupMessage("(*￣3￣)╭黑嘴健在呀。");
                    if (ti == 3) FromGroup.SendGroupMessage("黑嘴黑嘴活着呢。o(^▽^)o");
                    if (ti == 4) FromGroup.SendGroupMessage("（￣︶￣）↗　你觉得疯疯的黑嘴可能死掉吗？");**/
                    return;
                }

                //缩写查找
                if ((ignore.getkey(FromGroup.Id.ToString(), "sxauto") != "√" || force == true) && IsCmd(cmd) == false)
                {
                    string sxf = cmd.Replace("。", "").Replace("！", "").Replace("？", "")
                                .Replace("!", "").Replace("?", "");
                    string echar = "ABCDEFGHIJKLNMOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                    long echarc = 0;
                    for (int i = 0; i < sxf.Length; i++)
                        if (echar.Where(s => s == sxf[i]).Count() > 0) echarc++;
                    if(echarc >= sxf.Length - 1)
                    {
                        string sxr = SeekSX(cmd);
                        if(sxr != "") FromGroup.SendGroupMessage(FromQQ.CQCode_At(), " 翻译：" + sxr);
                    }
                }

                //Undertale Gameing
                if (UT.targetg == FromGroup.Id)
                {
                    if (cmd.ToLower() == UT.role)
                    {
                        //e.FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id),"回答正确！！！");
                        UT.winstr = UT.winstr + CQApi.CQCode_At(FromQQ.Id) + " correct +" + (int)(UT.prise * 10) / 10 + " points\n";
                        SolvePlayer(UT.prise, FromQQ.Id); UT.prise *= 0.8f;
                    }
                    else
                    {
                        for (int i = 0; i < UT.pos.Count; i++)
                        {
                            if (cmd.ToLower() == UT.pos[i].name)
                            {
                                UT.winstr = UT.winstr + CQApi.CQCode_At(FromQQ.Id) + " incorrect -5 points\n";
                                SolvePlayer(-5f, FromQQ.Id);
                            }
                        }
                        for (int i = 0; i < 8; i++)
                        {
                            if (cmd.ToLower() == (new string(new char[] { (char)('a' + i) })))
                            {
                                UT.winstr = UT.winstr + CQApi.CQCode_At(FromQQ.Id) + " incorrect -5 points\n";
                                SolvePlayer(-5f, FromQQ.Id);
                            }
                        }
                    }

                }

                //Library Check
                foreach(MessagePoster.flowlibrary fl in MessagePoster.flibrary)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), fl.name + "检测") != "√" || force == true)
                    {
                        foreach (string c in fl.lib)
                        {
                            if (cmd.IndexOf(c) >= 0 && c != "")
                            {
                                Log("(" + FromGroup.Id + ")Baned " + FromQQ.Id + " for " + fl.name + " , " + cmd + " , key :" + c, ConsoleColor.Red);
                                Message.RemoveMessage();
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "疑似[" + fl.name + "]");
                                return;
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
                    if (hmsg.group == FromGroup.Id) { hhmsg = hmsg; hIndex = i; break; }
                }
                string fstr = ""; string estr = ""; string[] qtemp;
                for (int i = 0; i < Manager.Hots.data.Count; i++)
                {
                nexthmsg:
                    if (i >= Manager.Hots.data.Count) { break; }
                    hmsg = (MainThread.MessagePoster.HotMsg)Manager.Hots.data[i];
                    //是否在目标群
                    if (hmsg.group == FromGroup.Id)
                    {
                        if (hmsg.delaymsg) //延迟复读
                        {
                            if (hmsg.hasre == false)
                            {
                                if (hmsg.canre)
                                {
                                    Log("(" + FromGroup.Id + ")(" + i + ")Delay repeat:" + hmsg.msg, ConsoleColor.Yellow);
                                    FromGroup.SendGroupMessage(hmsg.msg);
                                    hmsg.delaymsg = false; hmsg.hasre = true;
                                    Log("(" + FromGroup.Id + ")(" + i + ")Conntinue-repeat:" + hmsg.delaymsg, ConsoleColor.Yellow);
                                }
                            }
                        }
                        if (cmd != hmsg.msg) { hmsg.hot--; } //如果当前的发言和上句发言不同，上句发言热度-1
                                                                        //如果当前的发言和上句发言一直，上句发言热度+1
                        if (cmd == hmsg.msg && hmsg.hot > -2)
                        {
                            hothander = hmsg.id;
                            // 不是同一个QQ在刷屏
                            if (hmsg.qq.IndexOf(FromQQ.Id.ToString() + ";") < 0)
                            {
                                string[] qqtemp = hmsg.qq.Split(';');
                                if (!IsCmd(cmd)) Repeaters.ZeroRepeat(long.Parse(qqtemp[0]), FromGroup.Id);
                                if (qqtemp.Length == 2)
                                {
                                    if (!IsCmd(cmd)) Repeaters.FirstRepeat(FromQQ.Id, FromGroup.Id);
                                }
                                Log("(" + FromGroup.Id + ")(" + i + ")Heat:" + hmsg.msg + " , hot :" + hmsg.hot);
                                hmsg.hasup = true;
                                hmsg.qq = hmsg.qq + FromQQ.Id.ToString() + ";";
                                Log("(" + FromGroup.Id + ")(" + i + ")Members:" + hmsg.qq);
                                hmsg.hot++;
                                if (hIndex > -1)
                                {
                                    if (hmsg.hot >= hhmsg.hot)
                                    {
                                        hhmsg = hmsg; hhmsg.hasup = false;
                                        Manager.mHot.data[hIndex] = hhmsg;
                                        Log("(" + FromGroup.Id + ")(" + i + ")Broke records! :" + hmsg.qq + "," + hmsg.msg, ConsoleColor.Green);
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
                                
                                hmsg.banqq = hmsg.banqq + FromQQ.Id.ToString() + ";";
                                int bancount = GotCount(hmsg.banqq, FromQQ.Id.ToString());
                                Log("(" + FromGroup.Id + ")(" + i + ")Boring-repeat:" + FromQQ.Id + " x " + bancount, ConsoleColor.Red);
                            }
                        }
                        //如果发言冷却，移除
                        if (hmsg.hot <= -10)
                        {
                            if (hmsg.hasup)
                            {
                                string[] qqtemp2 = hmsg.qq.Split(';');
                                if (!IsCmd(hmsg.msg) && qqtemp2.Length > 3) Repeaters.EndRepeat(long.Parse(qqtemp2[qqtemp2.Length - 2]), FromGroup.Id);
                                string QQName = CQApi.GetGroupMemberInfo(FromGroup.Id, Convert.ToInt64(hmsg.qq.Split(';')[0])).Card;
                                string Nick = CQApi.GetGroupMemberInfo(FromGroup.Id, Convert.ToInt64(hmsg.qq.Split(';')[0])).Nick;
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
                                    PutRepeat(QQName + "(" + Nick + ")", hmsg.msg, FromGroup, CQApi);
                                }
                                Log("(" + FromGroup.Id + ")(" + i + ")Disapper repeat:" + hmsg.msg, ConsoleColor.Red);
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
                                            Log("(" + FromGroup.Id + ")(" + i + ")Repeat:" + hmsg.msg + ",impossible:" + (100 - hmsg.hot * 20 * (hmsg.hot / 2)), ConsoleColor.Yellow);
                                            FromGroup.SendGroupMessage(hmsg.msg);
                                        }
                                        hmsg.hasre = true;
                                    }
                                }
                                else
                                {
                                    Log("(" + FromGroup.Id + ")(" + i + ")Delay set:" + hmsg.msg, ConsoleColor.Yellow);
                                }
                            }
                        }
                        if (i >= Manager.Hots.data.Count) { break; }
                        Manager.Hots.data[i] = hmsg;
                    }
                }
                if (hothander == 0)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "repeater") != "√" || force == true) 
                    {
                        hmsg.group = FromGroup.Id; hmsg.msg = cmd; hmsg.hot = 1; hmsg.delaymsg = false;
                        hmsg.qq = FromQQ.Id.ToString() + ";"; hmsg.banqq = ""; hmsg.id = DateTime.Now.Ticks;
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
                            Log("(" + FromGroup.Id + ")Stay breaking record ...", ConsoleColor.Green);
                        }
                    }
                }

                //Summar Pictures
                if (cmd == ".drawlist")
                {
                    string pr = "";
                    foreach(string ptext in MessagePoster.ptList)
                    {
                        pr += ptext + "、";
                    }
                    FromGroup.SendGroupMessage(pr);
                    return;
                }
                if (cmd.StartsWith(".draw") || r.Next(0,666) == 233)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "artist") == "√" && force == false){return;}
                    string pqq = FromQQ.Id.ToString();string pmsg = cmd;
                    string targett = MessagePoster.ptList[r.Next(0, MessagePoster.ptList.Count)];
                    if (cmd.StartsWith(".draw"))
                    {
                        if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
                        string[] par = cmd.Split(' ');
                        if (par.Length > 1) { targett = par[1] == "?" ? targett : par[1]; }
                        if (par.Length > 2) { pqq = par[2].Replace("[CQ:at,qq=", "").Replace("]", ""); }
                        if (par.Length > 3) { pmsg = par[3]; }
                    }
                    GroupMemberInfo gmi;
                    try
                    {
                        gmi = FromGroup.GetGroupMemberInfo(Convert.ToInt64(pqq));
                    }
                    catch
                    {
                        FromGroup.SendGroupMessage("目标不在此群。");
                        return;
                    }
                    Log("Drawed " + targett + "!", ConsoleColor.Green);
                    if(!File.Exists("C:\\DataArrange\\PTemple\\" + targett + ".txt"))
                    {
                        FromGroup.SendGroupMessage("模板'" + targett + "'不存在。");
                        return;
                    }
                    StrangerInfo si = CQApi.GetStrangerInfo(Convert.ToInt64(pqq));
                    ScriptDrawer.Draw("C:\\DataArrange\\PTemple\\" + targett + ".txt", 
                                      MessagePoster.workpath + "\\data\\image\\" + targett + ".png",
                                      "[msg]", pmsg,
                                      "[qq]", pqq,
                                      "[nick]", gmi.Nick,
                                      "[card]", gmi.Card == "" ? gmi.Nick : gmi.Card,
                                      "[sex]", gmi.Sex.ToString(),
                                      "[age]", si.Age.ToString(),
                                      "[group]", FromGroup.Id.ToString()
                                      );
                    FromGroup.SendGroupMessage(CQApi.CQCode_Image(targett + ".png"));
                    return;
                }

                //CheckBack
                if (cmd.IndexOf("撤回了啥") >= 0)
                {
                    string cqq = cmd.Replace("[CQ:at,qq=", "").Replace("]", "").Replace("撤回了啥", "").Trim().Replace(" ", "");
                    int cindex = 1; string cret = ""; MessagePoster.HotMsg cmsg;
                    for (int i = 0; i < Manager.Hots.data.Count; i++)
                    {
                        cmsg = (MessagePoster.HotMsg)Manager.Hots.data[i];
                        Log("check: " + cmsg.qq + " , " + cmsg.group + " , " + cmsg.msg);
                        if ((cmsg.qq.IndexOf(cqq) >= 0) && (cmsg.group == FromGroup.Id))
                        {
                            cret = cret + "最近的第" + cindex + "条消息：" + "\n" + cmsg.msg + "\n";
                            cindex++;
                        }
                    }
                    FromGroup.SendGroupMessage(FromQQ.CQCode_At(), "请在黑嘴私聊中查收。");
                    FromQQ.SendPrivateMessage("消息回溯功能自动触发。\n" + cret);
                    Log("feed back to (" + cqq + "):\n" + cret);
                }

                //More Artifical
                MessagePoster.CheckProcessMsg(cmd, FromGroup.Id, 0);
                if (r.Next(0, 666) == 555)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "repeater") == "√" && force == false) { return; }
                    FromGroup.SendGroupMessage(cmd);
                }
                if (r.Next(0, 800) == 444)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "ai") == "√" && force == false) { return; }
                    int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                    FromGroup.SendGroupMessage(Manager.wordcollect.getkey("repeat", "item" + r.Next(0, Count)));
                }

                //Screen Checker
                if (FromGroup.Id == 577344771) { goto SkipChecker; }
                if (ignore.getkey(FromGroup.Id.ToString(), "msgcheck") == "√" && force == false) { goto SkipChecker; }

                int ssid = -1; personmsg pem = new personmsg();
                for (int i = 0; i < Manager.scrBan.data.Count; i++)
                {
                    pem = (personmsg)Manager.scrBan.data[i];
                    if (pem.qq == FromQQ.Id)
                    {
                        ssid = i;
                        if (GetTickCount() - pem.tick >= 3000) pem.msglist.Clear();
                        pem.msglist.Add(Message);
                        pem.anger += CompareStr(pem.lmsg,cmd);
                        if (cmd.Length >= 160) { pem.anger += (cmd.Length / 160); }
                        pem.anger -= (GetTickCount() - pem.tick) / 666;
                        pem.lmsg = cmd;
                        if (cmd.IndexOf(pem.lmsg) >= 0 || pem.lmsg.IndexOf(cmd) >= 0) 
                        { pem.anger = pem.anger + 1; }
                        if (pem.anger < 0) { pem.anger = 0; }
                        if (GetTickCount() - pem.tick <= 3000 && pem.msglist.Count >= 3)
                        {
                            switch (Math.Floor(pem.anger).ToString())
                            {
                                case ("4"):
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "不许刷屏");
                                    break;
                                case ("5"):
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "不要刷屏了啦");
                                    break;
                                case ("6"):
                                case ("7"):
                                case ("8"):
                                case ("9"):
                                case ("10"):
                                    FromGroup.SendGroupMessage(CQApi.CQCode_Image("angry1.png"));
                                    break;
                                default:
                                    break;
                            }
                            Log("(" + FromGroup.Id + ")(" + i + ")anger incrasing:" + FromQQ.Id + "x" + pem.anger, ConsoleColor.Red);
                        }
                        if(pem.anger >= 2 && pem.msglist.Count >= 3)
                        {
                            if (IsCmd(cmd)) Repeaters.SpeakOnce(FromQQ.Id, FromGroup.Id);

                            Repeaters.BoringRepeat(FromQQ.Id, FromGroup.Id);
                        }
                        if (pem.anger >= 6)
                        {

                            double bant = Math.Pow(pem.anger, 3) / 200 * 15 / 2;
                            double band;double banh;double banm;
                            banm = bant;banh = banm / 60;band = banh / 24;
                            banm %= 60;banh %= 24;
                            if (band > 29) { band = 29; }
                            TimeSpan bantime = new TimeSpan((int)band, (int)banh,(int)banm, 0);
                            FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(FromQQ.Id), bantime);

                            try
                            {
                                foreach (QQMessage qmsg in pem.msglist)
                                    qmsg.RemoveMessage();
                            }
                            catch { }

                            pem.msglist.Clear();

                            if (cmd.StartsWith(".") || 
                                cmd.IndexOf("云") >= 0 || 
                                cmd.IndexOf("居然") >= 0 ||
                                cmd.IndexOf("语录集") >= 0 ||
                                cmd.StartsWith("switch"))
                            {
                                info.putkey(FromQQ.Id.ToString(), "permission", "-1");
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id),
                                                             "我非常生气...检测到您滥用机器人指令，系统自动封禁了您的权限，请联系开发者恢复使用权。");
                                Log("Auto baned " + FromQQ.Id, ConsoleColor.Red);
                            }
                        }
                        pem.tick = GetTickCount();
                        Manager.scrBan.data[i] = pem;
                    }
                }
                if (ssid == -1)
                {
                    pem.qq = FromQQ.Id; pem.anger = 0; pem.tick = GetTickCount();
                    pem.lmsg = cmd;pem.msglist = new List<QQMessage>();
                    Manager.scrBan.data.Add(pem);
                }
            SkipChecker:

                //Prise All
                if (r.Next(0, 1500) == 66 || cmd.ToLower() == ".nice")
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "greeting") == "√" && force == false) { return; }

                    if (cmd.ToLower() == ".nice")
                    {
                        if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
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
                    string qname = FromQQ.GetGroupMemberInfo(FromGroup.Id).Card;
                    if (qname == "") qname = FromQQ.GetGroupMemberInfo(FromGroup.Id).Nick;
                    FromGroup.SendGroupMessage(pri.Replace("<n>", qname));
                    Log("Greeting:" + qname, ConsoleColor.Yellow);
                    return;
                }

                //Random Topic
                string tsay = "";
                if (cmd.IndexOf("[CQ:") < 0)
                {             
                    if ((r.Next(0, 100) == 88) || (FailAI == true))
                    {
                        if (ignore.getkey(FromGroup.Id.ToString(), "ai") == "√" && force == false) { return; }
                        try
                        {
                            tsay = ArtificalAI.Talk(cmd, "tieba");
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
                                    FailAI = false; MessagePoster.LetSay(tsay, FromGroup.Id,0,r.Next(0,2) == 1);
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
                sid = Manager.LCards.SearchFor(FromQQ.Id);
                if (sid != -1)
                {
                    Log("Lock target:" + sid, ConsoleColor.Yellow);
                    string tname = Manager.LCards.data[sid + 1].ToString();
                    if (FromQQ.CQApi.GetGroupMemberInfo(FromGroup.Id, FromQQ.Id, false).Card != tname)
                    {
                        if (FromGroup.SetGroupMemberVisitingCard(FromQQ.Id, tname))
                        {
                            FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "unmatced card , changed:", tname);
                        }
                        else
                        {
                            FromGroup.SendGroupMessage(CQApi.CQCode_At(1361778219), "unable to change unmatched card");
                        }

                    }
                    return;
                }

                //Word Collection
                if (cmd.IndexOf("云") >= 0)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "collection") == "√" && force == false) { return; }
                    if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
                    string name;
                    if (cmd.IndexOf("云") == cmd.Length - 1)
                    {
                        name = cmd.Substring(0, cmd.Length - 1).ToLower();
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
                        FromGroup.SendGroupMessage(name + "：" + t[r.Next(0, t.Count)]);
                        return;
                    }
                    if (cmd.IndexOf("云：") < cmd.Length - 2)
                    {
                        string[] t = cmd.Split(new string[] { "云：" }, StringSplitOptions.None);
                        if (t.Length < 2) { return; }
                        if (PutRepeat(t[0], t[1], FromGroup, CQApi, false) == 1)
                        {
                            FromGroup.SendGroupMessage("采集成功！");
                        }
                        else
                        {
                            FromGroup.SendGroupMessage(t[0] + "：我没说过");
                        }
                        return;
                    }
                }
                if (cmd.IndexOf("居然") >= 0)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "collection") == "√" && force == false) { return; }
                    if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
                    string[] p = cmd.Split(new string[] { "居然" },StringSplitOptions.None);
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
                    FromGroup.SendGroupMessage(name + "：" + t[r.Next(0, t.Count)]);
                    return;
                }
                if (cmd.IndexOf("语录集")>= 0)
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "collection") == "√" && force == false) { return; }
                    if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                    string[] p = cmd.Split(new string[] { "语录集" }, StringSplitOptions.None);
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
                    if (t.Count == 0) { FromGroup.SendGroupMessage(name + "没有任何语录。"); return; }
                    FromGroup.SendGroupMessage("已将语录集推送至消息队列");
                    for(int i = t.Count - 1;i >= 0; i--)
                    {
                        MessagePoster.SimSay(name + "语录 No." + (i+1) + "\n" + t[i], FromGroup.Id,(t.Count - i) * 2500);
                    }
                    return;
                }

                //Anti-Anti-Robot
                if(r.Next(0,3) == 2 && FromQQ.Id == 1724673579)
                {
                    string[] tp = cmd.Split(']');
                    FromGroup.SendGroupMessage(CQApi.CQCode_At(1724673579),
                                                 "不许" + tp[tp.Length - 1]);
                }

                //Function Switch
                if (cmd.StartsWith("switch"))
                {
                    if (!JudgePermission(FromQQ.Id, PermissionName.HeavenPermission)) { FromGroup.SendGroupMessage("no permission"); return; }
                    string[] p = cmd.Split(' ');
                    if (p[1] == "t") { p[1] = FromGroup.Id.ToString(); }
                    switch (p[2])
                    {
                        case ("clear"):
                            FromGroup.SendGroupMessage("cleared");
                            ignore.putkey(p[1], "log", "");
                            break;
                        case ("show"):
                            CQApi.SendGroupMessage(FromGroup.Id,
                                                     "本次机器人关闭了以下功能....\n" +
                                                     ignore.getkey(p[1], "log"));
                            break;
                        case ("send"):
                            CQApi.SendGroupMessage(Convert.ToInt64(p[1]),
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
                            if (p[3] == "artist") { logg = "自动P表情包"; }
                            ignore.putkey(p[1], "log", ignore.getkey(p[1], "log") + "\n" + p[3] + " " + logg);
                            FromGroup.SendGroupMessage(p[3] + " : off");
                            ignore.putkey(p[1], p[3], "√");
                            break;
                        case ("on"):
                            FromGroup.SendGroupMessage(p[3] + " : on");
                            ignore.putkey(p[1], p[3], "");
                            break;
                        default:
                            FromGroup.SendGroupMessage("unknown switch command");
                            break;
                    }
                    return;
                }

                //Console Processing
                if (cmd.StartsWith("."))
                {
                    if (ignore.getkey(FromGroup.Id.ToString(), "command") == "√" && force == false) { return; }


                    long lstime = 0;

                    try
                    {
                        string user = FromQQ.Id.ToString();
                        PermissionName pe = PermissionName.AirPermission;
                        pe = (PermissionName)Convert.ToInt64(info.getkey(FromQQ.Id.ToString(), "permission"));
                        string pename = GetPermissionName(pe);

                        string pcmd = cmd.Replace("[CQ:at,qq=", "").Replace("]", "");
                        do
                        {
                            pcmd = pcmd.Replace("  ", " ");
                        } while (pcmd.IndexOf("  ") >= 0);
                        pcmd = pcmd.Remove(0, 1).Insert(0, "ik ");
                        string[] p = pcmd.Split(' ');
                        string word = "";string ret = "";int longc = 0;

                        if (ignore.getkey(FromGroup.Id.ToString(), p[1]) == "√" && force == false) { return; }
                        switch (p[1])
                        {
                            case ("dycheat"):
                                int dyr = r.Next(0, 5);
                                if (dyr == 0) FromGroup.SendGroupMessage("什么...游戏作弊啊，好恶心哦...");
                                if (dyr == 1) FromGroup.SendGroupMessage("你真的信了？");
                                if (dyr == 2) FromGroup.SendGroupMessage("可是你能给黑嘴什么呢？你在期待着什么呢？");
                                if (dyr == 3) FromGroup.SendGroupMessage("你在想peach吧...");
                                if (dyr == 4) FromGroup.SendGroupMessage("痴心妄想！");
                                break;
                            case ("dyn"):
                                //偷偷读取DreamY存档
                                Storage dy = new Storage("dreamy");
                                FromGroup.SendGroupMessage("您的DreamY数据：\n等级：" + dy.getkey(FromQQ.Id.ToString(), "level")
                                                            , "，天赋：" + dy.getkey(FromQQ.Id.ToString(), "talent")
                                                            , "，VIP等级：" + dy.getkey(FromQQ.Id.ToString(), "VIPrank"));
                                FromGroup.SendGroupMessage("嘘！回复.dycheat可以让黑嘴偷偷帮你把DreamY天赋值和等级和VIP等级提升10倍！");
                                break;
                            case ("state"):
                                FromGroup.SendGroupMessage("服务器运行参数\nCPU利用率：" + sysinfo.CpuLoad + "%(x" + sysinfo.ProcessorCount + ")\n" +
                                                            "空闲内存：" + Math.Floor(sysinfo.MemoryAvailable / 1024f / 1024f) + "MB/" + 
                                                                          Math.Ceiling(sysinfo.PhysicalMemory / 1024f / 1024f) + "MB"); 
                                break;
                            case ("crash"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.Error404)) { return; }
                                FromGroup.SendGroupMessage("你杀了黑嘴！！！");
                                ProtectCount = 404;
                                break;
                            case ("broadcast"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                board.putkey(FromGroup.Id.ToString(), "links", p[2]);
                                FromGroup.SendGroupMessage("broadcast:" + p[2]);
                                break;
                            case ("fake"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                string newcmd = "";
                                for (int i = 3; i < p.Length; i++)
                                    newcmd = newcmd + p[i] + " ";

                                Group ngroup = new Group(CQApi, long.Parse(p[2]));
                                ngroup.SendGroupMessage("来自 " + GetName(FromGroup, CQApi, FromQQ.Id,false) + " 的跨群指令：\n" + newcmd);
                                ExcuteCmd(ngroup, FromQQ, Message, CQApi, newcmd);
                                FromGroup.SendGroupMessage("applied in another group.");
                                break;
                            case ("msgrk"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
                                List<Repeaters.member> meml = new List<Repeaters.member>();
                                List<GroupMemberInfo> gmis = FromGroup.GetGroupMemberList();
                                double wmin = 0;
                                foreach(Repeaters.member rmm in Repeaters.List.data)
                                {
                                    if(rmm.group == FromGroup.Id) wmin += rmm.wcount;
                                }
                                wmin = wmin / Repeaters.List.data.Count;
                                wmin *= 0.33;
                                if(p.Length >= 4)
                                {
                                    if(p[3] == "all") { wmin = 0; }
                                }
                                if (p[2] == "msg")
                                {
                                    meml = Repeaters.List.data.FindAll(
                                        m => m.group == FromGroup.Id && m.wcount > wmin && gmis.FindIndex(me => me.QQ == m.qq) != -1);
                                }
                                else
                                {
                                    if (p[2] == "zerore")
                                        meml = Repeaters.List.data.FindAll(
                                            m => m.group == FromGroup.Id && m.zfcount > 0 && m.wcount > wmin && gmis.FindIndex(me => me.QQ == m.qq) != -1);
                                    if (p[2] == "firstre")
                                        meml = Repeaters.List.data.FindAll(
                                            m => m.group == FromGroup.Id && m.frcount > 0 && m.wcount > wmin && gmis.FindIndex(me => me.QQ == m.qq) != -1);
                                    if (p[2] == "endre")
                                        meml = Repeaters.List.data.FindAll(
                                            m => m.group == FromGroup.Id && m.encount > 0 && m.wcount > wmin && gmis.FindIndex(me => me.QQ == m.qq) != -1);
                                    if (p[2] == "ban")
                                        meml = Repeaters.List.data.FindAll(
                                            m => m.group == FromGroup.Id && m.bacount > 0 && m.wcount > wmin && gmis.FindIndex(me => me.QQ == m.qq) != -1);
                                    if (p[2] == "re")
                                        meml = Repeaters.List.data.FindAll(
                                            m => m.group == FromGroup.Id && m.wcount > wmin && gmis.FindIndex(me => me.QQ == m.qq) != -1);
                                }
                                if (p[2] == "msg") { meml.Sort((m2, m1) => m1.wcount.CompareTo(m2.wcount)); }
                                if (p[2] == "zerore") { meml.Sort((m2, m1) => (m1.zfcount / m1.wcount).CompareTo(m2.zfcount / m2.wcount)); }
                                if (p[2] == "firstre") { meml.Sort((m2, m1) => (m1.frcount / m1.wcount).CompareTo(m2.frcount / m2.wcount)); }
                                if (p[2] == "endre") { meml.Sort((m2, m1) => (m1.encount / m1.wcount).CompareTo(m2.encount / m2.wcount)); }
                                if (p[2] == "ban") { meml.Sort((m2, m1) => (m1.bacount / m1.wcount).CompareTo(m2.bacount / m2.wcount)); }
                                if (p[2] == "re") 
                                { 
                                    meml.Sort((m2, m1) => 
                                    ((m1.encount + m1.frcount) / m1.wcount * 0.8 + m1.zfcount / m1.wcount * 0.2)
                                    .CompareTo((m2.encount + m2.frcount) / m2.wcount * 0.8 + m2.zfcount / m2.wcount * 0.2)); 
                                }
                                List<DrawTable.tabs> ltab = new List<DrawTable.tabs>();
                                ltab.Add(new DrawTable.tabs("排序", Color.Black, Color.FromArgb(255,232,232,232)));
                                ltab.Add(new DrawTable.tabs("名称", Color.Black, Color.FromArgb(255, 232, 232, 232)));
                                ltab.Add(new DrawTable.tabs("消息条数", Color.Black, Color.FromArgb(255, 232, 232, 232)));
                                ltab.Add(new DrawTable.tabs("复读发起", Color.Black, Color.FromArgb(255, 232, 232, 232)));
                                ltab.Add(new DrawTable.tabs("初次复读", Color.Black, Color.FromArgb(255, 232, 232, 232)));
                                ltab.Add(new DrawTable.tabs("终结复读", Color.Black, Color.FromArgb(255, 232, 232, 232)));
                                ltab.Add(new DrawTable.tabs("刷屏", Color.Black, Color.FromArgb(255, 232, 232, 232)));
                                ltab.Add(new DrawTable.tabs("复读率", Color.Black, Color.FromArgb(255, 232, 232, 232)));
                                double wmax = meml.Max(m => m.wcount);
                                if (wmax == 0) wmax = 1;
                                double zmax = meml.Max(m => (m.zfcount / m.wcount));
                                if (zmax == 0) zmax = 1;
                                double fmax = meml.Max(m => (m.frcount / m.wcount));
                                if (fmax == 0) fmax = 1;
                                double emax = meml.Max(m => (m.encount / m.wcount));
                                if (emax == 0) emax = 1;
                                double bmax = meml.Max(m => (m.bacount / m.wcount));
                                if (bmax == 0) bmax = 1;
                                double rmax = meml.Max(m => ((m.encount + m.frcount) / m.wcount * 0.8 + m.zfcount / m.wcount * 0.2));
                                if (rmax == 0) rmax = 1;
                                int mmi = 0;
                                try
                                {
                                    for (int i = 0; i < meml.Count; i++)
                                    {
                                        mmi = i;
                                        Log(i + "." + GetName(FromGroup, CQApi, meml[i].qq));
                                        ltab.Add(new DrawTable.tabs((i + 1).ToString(), Color.Gray, Color.White));
                                        ltab.Add(new DrawTable.tabs(GetName(FromGroup, CQApi, meml[i].qq), Color.Black, Color.White));
                                        ltab.Add(
                                            new DrawTable.tabs(
                                                meml[i].wcount.ToString(),
                                                p[2] == "msg" ? Color.Black : Color.Gray,
                                                Color.FromArgb((int)(meml[i].wcount / wmax * 100), 0, 176, 240),
                                                p[2] == "msg"));
                                        ltab.Add(
                                            new DrawTable.tabs(
                                                ((int)(meml[i].zfcount / meml[i].wcount * 10000) / 100.0f).ToString() + "%",
                                                p[2] == "zerore" ? Color.Black : Color.Gray,
                                                Color.FromArgb((int)(meml[i].zfcount / meml[i].wcount / zmax * 100), 0, 176, 240),
                                                p[2] == "zerore"));
                                        ltab.Add(
                                            new DrawTable.tabs(
                                                ((int)(meml[i].frcount / meml[i].wcount * 10000) / 100.0f).ToString() + "%",
                                                p[2] == "firstre" ? Color.Black : Color.Gray,
                                                Color.FromArgb((int)(meml[i].frcount / meml[i].wcount / fmax * 100), 0, 176, 240),
                                                p[2] == "firstre"));
                                        ltab.Add(
                                            new DrawTable.tabs(
                                                ((int)(meml[i].encount / meml[i].wcount * 10000) / 100.0f).ToString() + "%",
                                                p[2] == "endre" ? Color.Black : Color.Gray,
                                                Color.FromArgb((int)(meml[i].encount / meml[i].wcount / emax * 100), 0, 176, 240),
                                                p[2] == "endre"));
                                        ltab.Add(
                                            new DrawTable.tabs(
                                                ((int)(meml[i].bacount / meml[i].wcount * 10000) / 100.0f).ToString() + "%",
                                                p[2] == "ban" ? Color.Black : Color.Gray,
                                                Color.FromArgb((int)(meml[i].bacount / meml[i].wcount / bmax * 100), 0, 176, 240),
                                                p[2] == "ban"));
                                        ltab.Add(
                                            new DrawTable.tabs(
                                                ((int)(((meml[i].frcount + meml[i].encount) / meml[i].wcount * 0.8 + meml[i].zfcount / meml[i].wcount * 0.2) * 10000) / 100.0f).ToString() + "%",
                                                p[2] == "re" ? Color.Black : Color.Gray,
                                                Color.FromArgb((int)(((meml[i].frcount + meml[i].encount) / meml[i].wcount * 0.8 + meml[i].zfcount / meml[i].wcount * 0.2) / rmax * 100), 0, 176, 240),
                                                p[2] == "re"));
                                    }
                                    DrawTable.ExportTable("按照" + p[2] + "排序(阈值：" + Math.Floor(wmin) + ")", 900,
                                                          new float[] { 0.06f, 0.18f, 0.13f, 0.13f, 0.13f, 0.13f, 0.13f, 0.13f },
                                                          ltab);
                                    FromGroup.SendGroupMessage(CQApi.CQCode_Image("table.png"));
                                }
                                catch
                                {
                                    FromGroup.SendGroupMessage("crashed at " + mmi + "\nqq:" + meml[mmi].qq + "(" + meml.Count + ")" );
                                    FromGroup.SendGroupMessage(meml.ToString() );
                                }
                                
                                break;
                            case ("msginfo"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
                                long tarqq = FromQQ.Id;
                                if (p.Length > 2) { tarqq = long.Parse(p[2].Replace("[CQ:at,qq=", "").Replace("]", "")); }
                                try
                                {
                                    GroupMemberInfo gmit = FromGroup.GetGroupMemberInfo(tarqq);
                                }
                                catch
                                {
                                    FromGroup.SendGroupMessage("目标不在此群。");
                                    return;
                                }
                                Repeaters.member meminfo = Repeaters.Information(tarqq, FromGroup.Id);
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(tarqq) ,"\n",
                                                             "总发言条数：" + meminfo.wcount + "条\n"
                                                             + "复读发起：" + meminfo.zfcount + "次("
                                                             + (int)(meminfo.zfcount / meminfo.wcount * 10000) / 100.0f + "%)\n"
                                                             + "初次复读：" + meminfo.frcount + "次("
                                                             + (int)(meminfo.frcount / meminfo.wcount * 10000) / 100.0f + "%)\n"
                                                             + "终结复读：" + meminfo.encount + "次("
                                                             + (int)(meminfo.encount / meminfo.wcount * 10000) / 100.0f + "%)\n"
                                                             + "刷屏：" + meminfo.bacount + "次("
                                                             + (int)(meminfo.bacount / meminfo.wcount * 10000) / 100.0f + "%)\n"
                                                             );
                                break;
                            case ("wex"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                                string[] words = cmd.Split('\n');
                                string oname = p[2].Split('\n')[0].Replace("\r","").Replace("\n","");
                                for(int i = 1;i < words.Length; i++)
                                {
                                    PutRepeat(oname, words[i]);
                                }
                                FromGroup.SendGroupMessage("成功为" + oname + "导入" + (words.Length - 1) + "条语录。");
                                break;
                            case ("wreport"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                                ProtectCount = 404;
                                FromGroup.SendGroupMessage("已将保护值设置到404防止其他消息干扰该操作。");
                                longc = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                int cmdc = 0;int icec = 0;int rec = 0;
                                string reret = "";
                                for (int i = 0; i < longc; i++)
                                {
                                    word = Manager.wordcollect.getkey("repeat", "item" + i);
                                    for (int s = i; s < longc; s++)
                                    {
                                        if (word == Manager.wordcollect.getkey("repeat", "item" + s) && i != s)
                                        {
                                            rec++; reret = reret + i + " ";
                                            break;
                                        }
                                    }
                                    if(word.StartsWith(".") || word.StartsWith("/")
                                       || word.StartsWith("\\") || word.StartsWith("?")
                                       || word.StartsWith("!") || word.StartsWith("#")
                                       || word.StartsWith("dy") || word.StartsWith("stop")
                                       || word.StartsWith("闭嘴") || word.StartsWith("别说了")
                                       || word.IndexOf("撤回了啥") >= 0 || word.IndexOf("居然") >= 0
                                       )
                                    {
                                        cmdc++;
                                    }
                                    if (word.StartsWith("冰棍") 
                                       || word.ToLower().StartsWith("ice"))
                                    {
                                        icec++;
                                    }
                                }
                                FromGroup.SendGroupMessage("语录集数据库总收集量：" + longc + "条" + "\n" +
                                                             "重复语录：" + rec + "条(" 
                                                             + (int)(Convert.ToDouble(rec) / Convert.ToDouble(longc) * 100) 
                                                             + "%)，位于：\n" + reret + "\n"
                                                             + "指令语录：" + cmdc + "条("
                                                             + (int)(Convert.ToDouble(cmdc) / Convert.ToDouble(longc) * 100)
                                                             + "%)\n"
                                                             + "提及冰棍的语录：" + icec + "条("
                                                             + (int)(Convert.ToDouble(icec) / Convert.ToDouble(longc) * 100)
                                                             + "%)");
                                
                                ProtectCount = 0;
                                break;
                            case ("wclear"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.Error404)) { return; }
                                ProtectCount = 404;
                                FromGroup.SendGroupMessage("已将保护值设置到404防止其他消息干扰该操作。");
                                string uifile = Guid.NewGuid().ToString();
                                File.Copy(@"C:\DataArrange\wordcollections-userdata.json",
                                    @"C:\DataArrange\[backup]wordcollections-" + uifile + ".json");
                                FromGroup.SendGroupMessage("备份[" + uifile + "]已创建。");
                                longc = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                List<string> owners = new List<string>();
                                List<string> repeates = new List<string>();
                                List<bool> removemap = new List<bool>();
                                for (int i = 0;i < longc; i++)
                                {
                                    repeates.Add(Manager.wordcollect.getkey("repeat", "item" + i));
                                    owners.Add(Manager.wordcollect.getkey("owner" + i, "name"));
                                    removemap.Add(false);
                                }
                                for(int i = 2;i < p.Length; i++)
                                {
                                    removemap[int.Parse(p[i])] = true;
                                }
                                int repi = 0;
                                Storage newword = new Storage("wordoper");
                                for (int i = 0;i < owners.Count; i++)
                                {
                                    if (!removemap[i])
                                    {
                                        newword.putkey("owner" + repi, "name", owners[i],false);
                                        newword.putkey("repeat", "item" + repi, repeates[i],false);
                                        repi++;
                                    }
                                }
                                newword.putkey("repeat", "count", repi.ToString(),false);
                                newword.Store();
                                File.Delete(@"C:\DataArrange\wordcollections-userdata.json");
                                File.Copy(@"C:\DataArrange\wordoper-userdata.json",
                                          @"C:\DataArrange\wordcollections-userdata.json");
                                File.Delete(@"C:\DataArrange\wordoper-userdata.json");
                                Manager.wordcollect.Restore();
                                FromGroup.SendGroupMessage("removed");
                                ProtectCount = 0;
                                break;
                            case ("wdetail"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.Error404)) { return; }
                                word = Manager.wordcollect.getkey("repeat", "item" + p[2]);
                                FromGroup.SendGroupMessage("NO." + p[2] + "\n" + word);
                                break;
                            case ("wfetchs"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                                longc = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                for (int i = 0; i < longc; i++)
                                {
                                    word = Manager.wordcollect.getkey("repeat", "item" + i);
                                    if (word.StartsWith(p[2])) ret = ret + i + " ";
                                }
                                FromGroup.SendGroupMessage(ret);
                                break;
                            case ("wfetch"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                                longc = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                for(int i = 0;i < longc; i++)
                                {
                                    word = Manager.wordcollect.getkey("repeat", "item" + i);
                                    if (word.IndexOf(p[2]) >= 0) ret = ret + i + " ";
                                }
                                FromGroup.SendGroupMessage(ret);
                                break;
                            case ("clearwords"):
                                if (!JudgePermission(FromQQ.Id,PermissionName.Error404)){ return; }
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
                                FromGroup.SendGroupMessage("本次删除了" + (RCount - SCount) + "条语录，感觉自己萌萌哒~");
                                break;
                            case ("word"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
                                int Count = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                int Index = r.Next(0, Count);
                                FromGroup.SendGroupMessage(
                                    Manager.wordcollect.getkey("owner" + Index, "name") + "：" +
                                    Manager.wordcollect.getkey("repeat", "item" + Index)
                                    );
                                break;
                            case ("utf"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
                                if (UT.targetg == 0)
                                {
                                    FromGroup.SendGroupMessage("a new game: 'undertale guessing' has been on");
                                    UT.mode = 1;
                                    UT.targetg = FromGroup.Id; UT.round = 0; UT.ps.Clear();
                                    UT.tick = GetTickCount();
                                    UT.nextRound();
                                    FromGroup.SendGroupMessage("round " + UT.round + "(result:20s later)" + UT.dialog);
                                }
                                else
                                {
                                    if (UT.targetg == FromGroup.Id)
                                    {
                                        FromGroup.SendGroupMessage("the game has been on");
                                    }
                                    else
                                    {
                                        FromGroup.SendGroupMessage("another game has been on");
                                    }
                                }
                                break;
                            case ("ut"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
                                if (UT.targetg == 0)
                                {
                                    FromGroup.SendGroupMessage("a new game: 'undertale guessing characters' has been on");
                                    UT.mode = 0;
                                    UT.targetg = FromGroup.Id; UT.round = 0; UT.ps.Clear();
                                    UT.tick = GetTickCount();
                                    UT.nextRound();
                                    FromGroup.SendGroupMessage("round " + UT.round + "(result:20s later)" + UT.dialog);
                                }
                                else
                                {
                                    if (UT.targetg == FromGroup.Id)
                                    {
                                        FromGroup.SendGroupMessage("the game has been on");
                                    }
                                    else
                                    {
                                        FromGroup.SendGroupMessage("another game has been on");
                                    }
                                }
                                break;
                            case ("pop"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
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
                                    FromGroup.SendGroupMessage(hhmsg.msg);
                                }
                                else
                                {
                                    FromGroup.SendGroupMessage("none");
                                }
                                break;
                            case ("bvoid"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (VoidLifes.IsStart())
                                {
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " the game has been on");
                                }
                                else
                                {
                                    if (p.Length < 3) { throw new Exception("ERROR: please name for the character:ink bvoid <name>"); }
                                    VoidLifes.BeginGame(p[2]); VoidLifes.TargetGroup = FromGroup.Id;
                                    VoidLifes.JoinGame(FromQQ.Id); VoidLifes.OwnerQQ = FromQQ.Id;
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " game begins");
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " you've joined the game");
                                }
                                break;
                            case ("jvoid"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (FromGroup.Id != VoidLifes.TargetGroup)
                                {
                                    if (VoidLifes.TargetGroup == 0)
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " not game is playing");
                                    }
                                    else
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " the game has been on in other group");
                                    }

                                }
                                else
                                {
                                    if (VoidLifes.IsJoined(FromQQ.Id))
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " repeative operation");
                                    }
                                    else
                                    {
                                        VoidLifes.JoinGame(FromQQ.Id);
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " you've joined the game");
                                    }
                                }
                                break;
                            case ("svoid"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (FromGroup.Id != VoidLifes.TargetGroup)
                                {
                                    if (VoidLifes.TargetGroup == 0)
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " not game is playing");
                                    }
                                    else
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " the game has been on in other group");
                                    }

                                }
                                else
                                {
                                    if (VoidLifes.OwnerQQ != FromQQ.Id)
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " you are not the game's leader");
                                    }
                                    else
                                    {
                                        if (VoidLifes.IsPlaying())
                                        {
                                            FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " the game has been on");
                                        }
                                        else
                                        {
                                            Log("game starts", ConsoleColor.Green);
                                            FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " game starts");
                                            VoidLifes.StartRound();
                                        }
                                    }
                                }
                                break;
                            case ("cvoid"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
                                if (FromGroup.Id != VoidLifes.TargetGroup)
                                {
                                    if (VoidLifes.TargetGroup == 0)
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " not game is playing");
                                    }
                                    else
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " the game has been on in other group");
                                    }

                                }
                                else
                                {
                                    if (VoidLifes.OwnerQQ != FromQQ.Id)
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " you are not the game's leader");
                                    }
                                    else
                                    {
                                        if (!VoidLifes.IsPlaying())
                                        {
                                            FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " not game is playing");
                                        }
                                        else
                                        {
                                            Log("game closed", ConsoleColor.Green);
                                            FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + " game closed");
                                            VoidLifes.EndGame();
                                        }
                                    }
                                }
                                break;
                            case ("msdn"):
                                //msdn searching
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "该功能正在回炉改造中..."); return;
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "msdn"));
                                break;
                            case ("csdn"):
                                //csdn searching
                                if (!JudgePermission(FromQQ.Id, PermissionName.UserPermission)) { return; }
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "该功能正在回炉改造中..."); return;
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "csdn"));
                                break;
                            case ("wall"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
                                if (IsWalling) { FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), " last task is processing."); return; }
                                IsWalling = true;
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), " downloading...");
                                WebClient wcd = new WebClient();
                                string wurl = Wallpaper.GetWallpaper();
                                wcd.DownloadFile(wurl, MessagePoster.workpath + "\\data\\image\\wall.jpg");
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id),CQApi.CQCode_Image("wall.jpg"),wurl);
                                wcd.Dispose();
                                IsWalling = false;
                                break;
                            case ("fsleep"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(1361778219), " goodbye, see you next time~");
                                ShellExecute(IntPtr.Zero, "open", @"shutdown", "-r -t 0", "", ShowCommands.SW_SHOWNORMAL);
                                break;
                            case ("ban"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 4) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]);
                                if (Convert.ToInt16(p[3]) > 0)
                                {
                                    TimeSpan ctime = new TimeSpan(0, Convert.ToInt16(p[3]), 0);
                                    if (FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(qq), ctime))
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operated successfully");
                                    }
                                    else
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operation denied");
                                    }
                                }
                                else
                                {
                                    if (FromGroup.RemoveGroupMemberBanSpeak(Convert.ToInt64(qq)))
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operated successfully");
                                    }
                                    else
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operation denied");
                                    }
                                }
                                break;
                            case ("aban"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                bool bswitch = Convert.ToBoolean(p[2]);
                                if (bswitch)
                                {
                                    if (FromGroup.SetGroupBanSpeak())
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operated successfully");
                                    }
                                    else
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operation denied");
                                    }
                                }
                                else
                                {
                                    FromGroup.RemoveGroupBanSpeak();
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operated successfully");
                                }
                                break;
                            case ("man"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.MasterPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]);
                                FromGroup.SetGroupManage(Convert.ToInt64(qq));
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(Convert.ToInt64(qq)), "you are manager now");
                                break;
                            case ("unman"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.MasterPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]);
                                FromGroup.RemoveGroupManage(Convert.ToInt64(qq));
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(Convert.ToInt64(qq)), "you are not manager now");
                                break;
                            case ("sgdnightf♂"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                mem = FromGroup.GetGroupMemberList();
                                FromGroup.SendGroupMessage("为您转发拉~");
                                for (int i = 0; i < mem.Count; i++)
                                {
                                    mem[i].QQ.SendPrivateMessage(p[2]);
                                    Log("Wish:" + i, ConsoleColor.Yellow);
                                    //e.FromGroup.SendGroupMessage(atstr, p[2]);
                                    Thread.Sleep(500);
                                }
                                break;
                            case ("sgdnight"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) { return; }
                                string atstr = "";
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                mem = FromGroup.GetGroupMemberList();
                                FromGroup.SendGroupMessage("为您转发拉~");
                                for (int i = 0; i < mem.Count; i += 10)
                                {
                                    atstr = "";
                                    for (int s = i; s < i + 10; s++)
                                    {
                                        Log("Wish:" + s, ConsoleColor.Yellow);
                                        if (s > mem.Count) { break; }
                                        atstr = atstr + CQApi.CQCode_At(mem[s].QQ.Id);
                                    }
                                    FromGroup.SendGroupMessage(atstr, p[2]);
                                    Thread.Sleep(3000);
                                }
                                break;
                            case ("pms"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]);
                                string luser = qq.ToString();
                                sid = (info.FirstStore ? -1 : 1);
                                if (sid != -1)
                                {
                                    if (p.Length < 4) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                    if (Convert.ToInt64(info.getkey(luser, "permission")) >= Convert.ToInt64(pe)) { FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied to change pms '" + GetPermissionName((PermissionName)(Convert.ToInt64(info.getkey(qq.ToString(), "permission")))) + "'(level " + Convert.ToInt64(info.getkey(qq.ToString(), "permission")) + ") ", CQApi.CQCode_At(Convert.ToInt64(qq))); return; }
                                    if (Convert.ToInt64(info.getkey(luser, "permission")) == -1) { FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "you can't do that, please edit the pms file on the server ('C:\\DataArrange\\') then restart the bot."); return; }
                                    if (Convert.ToInt64(p[3]) < 0) 
                                    {
                                        if (!JudgePermission(FromQQ.Id, PermissionName.LOVEPermission)) 
                                        {
                                            FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "this pms is for baning somebody, you can't do that.");
                                            return; 
                                        }
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "baned the target!"); 
                                    }

                                    if (Convert.ToInt64(p[3]) >= Convert.ToInt64(pe)) { FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied to give pms '" + GetPermissionName((PermissionName)(Convert.ToInt64(p[3]))) + "'(level " + Convert.ToInt64(p[3]) + ")"); return; }
                                    info.putkey(luser, "permission", p[3]);
                                    pe = (PermissionName)Convert.ToInt64(p[3]);
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)) + " pms:" + GetPermissionName(pe) + "(level " + Convert.ToInt64(pe) + ") pid:" + sid);
                                }
                                else
                                {
                                    info.putkey(luser, "permission", "1");
                                    sid = 1;
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id),
                                        CQApi.CQCode_At(Convert.ToInt64(qq)) + " has permission now, pid:" + sid);
                                }
                                break;
                            case ("honor"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                if (FromQQ.SetGroupMemberForeverExclusiveTitle(FromGroup.Id, p[2]) == false)
                                {
                                    throw new Exception("operation denied");
                                }
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operated successfully");
                                break;
                            case ("prs"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
                                if (FromQQ.SendPraise(10) == false)
                                {
                                    throw new Exception("operation denied");
                                }
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), "operated successfully");
                                break;
                            case ("tkv"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                for (int i = 0; i < MessagePoster.delays.Count; i++)
                                {
                                    MessagePoster.delaymsg d = MessagePoster.delays[i];
                                    if (d.group == FromQQ.Id)
                                    {
                                        lstime = d.time; return;
                                    }
                                }
                                if (GetTickCount() - lstime > 3000)
                                {
                                    tsay = ArtificalAI.Talk(ques, "tieba");
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id));
                                    if (tsay != "")
                                    {
                                        MessagePoster.LetSay(tsay, FromGroup.Id,0,true);
                                    }
                                    else
                                    {
                                        MessagePoster.LetSay("no search result", FromGroup.Id, 0, true);
                                    }
                                }
                                break;
                            case ("tk"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                for (int i = 0; i < MessagePoster.delays.Count; i++)
                                {
                                    MessagePoster.delaymsg d = MessagePoster.delays[i];
                                    if (d.group == FromQQ.Id)
                                    {
                                        lstime = d.time; return;
                                    }
                                }
                                if (GetTickCount() - lstime > 3000)
                                {
                                    tsay = ArtificalAI.Talk(ques, "tieba");
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id));
                                    if (tsay != "")
                                    {
                                        MessagePoster.LetSay(tsay, FromGroup.Id);
                                    }
                                    else
                                    {
                                        MessagePoster.LetSay("no search result", FromGroup.Id);
                                    }
                                }
                                break;
                            case ("tko"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                MessagePoster.LetSay("tko was not longer support.", FromGroup.Id);
                                return;
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
                                tsay = ArtificalAI.Talk(ques, "baidu");
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id));
                                if (tsay != "")
                                {
                                    MessagePoster.LetSay(tsay, FromGroup.Id);
                                }
                                else
                                {
                                    MessagePoster.LetSay("no search result", FromGroup.Id);
                                }
                                break;
                            case ("sx"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) { return; }
                                if (p.Length < 2) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                string sxr = SeekSX(p[2]);
                                if (sxr != "")
                                {
                                    FromGroup.SendGroupMessage(FromQQ.CQCode_At(), " 翻译：" + sxr);
                                }
                                else
                                {
                                    FromGroup.SendGroupMessage(FromQQ.CQCode_At(), " 无查询结果");
                                }
                                break;
                            case ("ach"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission))
                                {
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ), "\n非常抱歉，您被机器人封禁，无法使用任何指令。\n为什么会这样？\n您可能滥用指令或指令刷屏，或由开发者亲自封禁。\n怎么恢复？\n联系开发者。");
                                    return;
                                }
                                user = FromQQ.Id.ToString();
                                if (p.Length > 2) { user = p[2]; }
                                FromGroup.SendGroupMessage(CQApi.CQCode_At(long.Parse(user)) + " 获得的成就：\n" + achive.getkey(user, "achivements"));
                                break;
                            case ("info"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission))
                                {
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ), "\n非常抱歉，您被机器人封禁，无法使用任何指令。\n为什么会这样？\n您可能滥用指令或指令刷屏，或由开发者亲自封禁。\n怎么恢复？\n联系开发者。");
                                    return;
                                }
                                if (p.Length > 2) { user = p[2]; }
                                pe = (PermissionName)Convert.ToInt64(info.getkey(user, "permission"));
                                pename = GetPermissionName(pe);
                                GroupMemberInfo gmi;
                                gmi = FromGroup.GetGroupMemberInfo(Convert.ToInt64(user));
                                int fCount = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                int uCount = 0;string todayS = "";
                                for (int i = 0; i < fCount; i++)
                                {
                                    if (Manager.wordcollect.getkey("owner" + i, "name").ToLower().IndexOf(gmi.Nick.ToLower()) >= 0)
                                    {
                                        if (todayS == "") { todayS = Manager.wordcollect.getkey("repeat", "item" + i); }
                                        if (r.Next(0,3) == 2) { todayS = Manager.wordcollect.getkey("repeat", "item" + i); }
                                        uCount++;
                                    }
                                }
                                double percent = Convert.ToDouble(uCount) / Convert.ToDouble(fCount);
                                double BPercent = Math.Floor(percent * 100.0d);
                                Log(percent + "," + BPercent);
                                ScriptDrawer.Draw("C:\\DataArrange\\information.txt",
                                                  MessagePoster.workpath + "\\data\\image\\info.png",
                                                  "[msg]", cmd,
                                                  "[qq]", user,
                                                  "[nick]", gmi.Nick,
                                                  "[card]", gmi.Card == "" ? gmi.Nick : gmi.Card,
                                                  "[sex]", gmi.Sex.ToString(),
                                                  "[age]", gmi.Age.ToString(),
                                                  "[permission]", Convert.ToInt64(pe).ToString(),
                                                  "[permissionname]", pename,
                                                  "[word]","“" + todayS + "”",
                                                  "[words]",uCount.ToString(),
                                                  "[twords]",fCount.ToString(),
                                                  "[wordpercentf]", percent.ToString(),
                                                  "[wordpercent]",BPercent.ToString(),
                                                  "[coin]", Convert.ToDouble(info.getkey(user, "coins")).ToString(),
                                                  "[group]", FromGroup.Id.ToString()
                                                  );
                                FromGroup.SendGroupMessage(CQApi.CQCode_Image("info.png"));
                                break;
                            case ("lkc"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length == 3)
                                {
                                    qq = Convert.ToDouble(p[2]);
                                    sid = Manager.LCards.SearchFor(qq);
                                    if (sid != -1)
                                    {
                                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)),
                                        "unlocked !");
                                        Manager.LCards.data[sid] = "removed";
                                        Manager.LCards.SaveData();
                                        return;
                                    }
                                }
                                if (p.Length < 4) { throw new Exception("'" + p[1] + "' was given incorrect params"); }
                                qq = Convert.ToDouble(p[2]); string lname = p[3];
                                sid = Manager.LCards.SearchFor(qq);
                                if (sid != -1)
                                {
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)),
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
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)),
                                        "operated successfully add:" + sid +
                                        " lkc:" + Manager.LCards.data[sid + 1]);
                                }
                                break;
                            case ("help"):
                                if (!JudgePermission(FromQQ.Id, PermissionName.AirPermission)) 
                                {
                                    FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ), "\n非常抱歉，您被机器人封禁，无法使用任何指令。\n为什么会这样？\n您可能滥用指令或指令刷屏，或由开发者亲自封禁。\n怎么恢复？\n联系开发者。");
                                    return; 
                                }

                                List<DrawTable.tabs> cmdtabs = new List<DrawTable.tabs>();

                                UNCMD = 0;
                                PermissionName upms = (PermissionName)Convert.ToInt64(info.getkey(FromQQ.Id.ToString(), "permission"));
                                cmdtabs.Add(new DrawTable.tabs("您的权限", Color.White, Color.FromArgb(0, 176, 240)));
                                cmdtabs.Add(new DrawTable.tabs($"{GetPermissionName(upms)}({(int)upms})", Color.Black, Color.Transparent));

                                cmdtabs.Add(new DrawTable.tabs("命令", Color.Black, Color.Transparent));
                                cmdtabs.Add(new DrawTable.tabs("用途", Color.Black, Color.Transparent));

                                AddCmdTab(PermissionName.AirPermission, ".info [QQ]", "查看个人资料卡", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".ach [QQ]", "查看个人成就", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".wall", "获取一张随即壁纸", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".sx <内容>", "查询中文缩写原文", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.LOVEPermission, ".fsleep", "命令机器人所在服务器立即重启[关键操作]", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".ban <QQ> <分钟>", "禁言某人", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".aban true/false", "开关全员禁言", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.MasterPermission, ".sgdnight <内容>", "群内艾特所有人发送指定内容", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.MasterPermission, ".sgdnightf♂ <内容>", "私聊群内所有人发送指定内容", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.MasterPermission, ".man <QQ>", "设置管理员", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.MasterPermission, ".unman <QQ>", "取消管理员", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.UserPermission, ".pms <QQ> <权限>", "授予他人机器人使用权限", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".tk <内容>", "和黑嘴谈话", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".tko <内容>", "和黑嘴谈话（旧的引擎）", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".tkv <内容>", "和黑嘴语音谈话", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".honor <内容>", "给予自己一个专属头衔", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".prs", "为自己点赞十次", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".lkc <QQ> <名称>", "锁定指定群友的群名片", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".lkc <QQ>", "解除锁定指定群友的群名片", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".pop", "今日最热门的复读是什么呢？", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.UserPermission, ".ut", "传说之下猜角色游戏", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.UserPermission, ".utf", "传说之下完形填空游戏", cmdtabs, FromQQ, FromGroup);
                                //AddCmdTab(PermissionName.UserPermission, ".bvoid <角色名>", "发起一场虚假人生游戏", cmdtabs, FromQQ, FromGroup);
                                //AddCmdTab(PermissionName.UserPermission, ".jvoid", "加入已经发起的虚假人生游戏", cmdtabs, FromQQ, FromGroup);
                                //AddCmdTab(PermissionName.UserPermission, ".cvoid", "关闭正在进行的虚假人生游戏", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".nice", "让机器人和自己问好", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.UserPermission, ".word", "随机输出一条语录", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.MasterPermission, "switch t/<群号> on/off <功能>", "开关群内机器人功能", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".draw ?/[模板名] [QQ]", "根据某人的资料生成表情包", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".drawlist", "查看所有支持的表情包模板名", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, "[艾特QQ]撤回了啥", "查看最近的10条消息", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, "<名字>云", "查看某人的一条语录", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, "<名字>云：<内容>", "为某人收集一条语录", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, "<名字>居然<内容>", "查看某人有关内容的一条语录", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, "<名字>语录集", "查看某人的所有语录", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.Error404, ".clearwords", "清理语录集数据库", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.LOVEPermission, ".wfetch <内容>", "列出包含指定内容的数据序号", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.LOVEPermission, ".wfetchs <内容>", "列出以指定内容开头的数据序号", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.Error404, ".wclear <序号，用' '隔开>", "删除对应的语录数据（创建备份）", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.LOVEPermission, ".wreport", "分析语录集数据库状态", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.LOVEPermission, ".wex", "批量导入语录集", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".msginfo", "显示你的水群记录", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".msgrk msg [all]", "按照发言条数排序", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".msgrk zerore [all]", "按照复读发起率排序", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".msgrk firstre [all]", "按照初次幅度率排序", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".msgrk endre [all]", "按照终结复读率排序", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".msgrk re [all]", "按照复读率排序", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".msgrk ban [all]", "按照刷屏率排序", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".broadcast <group>", "将该群消息广播到指定群", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.SupermanPermission, ".fake <group> <cmd>", "跨群发送指令", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.LOVEPermission, ".bugclose", "解除致命错误自动保护模式", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".okay?", "确认一下黑嘴活着没有", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.Error404, ".crash", "杀掉黑嘴", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".state", "查看服务器运行参数", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.LOVEPermission , "<cmd> (force)", "无视权限限制执行指令", cmdtabs, FromQQ, FromGroup);
                                AddCmdTab(PermissionName.AirPermission, ".help", "查看说明书（你自己的视角）", cmdtabs, FromQQ, FromGroup);
                                
                                cmdtabs.Add(new DrawTable.tabs("注意", Color.White, Color.FromArgb(0,176,240)));
                                cmdtabs.Add(new DrawTable.tabs("还有" + UNCMD + "条指令因为您权限不足被折叠。", Color.Black, Color.Transparent));
                                DrawTable.ExportTable("黑嘴扰民机器人 说明书", 700, new float[] { 0.4f, 0.6f }, cmdtabs);

                                FromGroup.SendGroupMessage(FromQQ.CQCode_At(), "请在黑嘴私聊中查收。");
                                FromQQ.SendPrivateMessage(CQApi.CQCode_Image("table.png"),"\n","具体用法欢迎咨询黑嘴！~");
                                break;
                        }
                    }
                    catch (Exception err)
                    {
                        FromGroup.SendGroupMessage(CQApi.CQCode_At(FromQQ.Id), err.Message);
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
