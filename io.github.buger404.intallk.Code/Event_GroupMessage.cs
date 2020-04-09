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
        private static bool IsWalling = false;
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
            return (pe >= Required);
        }
        private string JP(long qq, PermissionName Required)
        {
            PermissionName pe = PermissionName.AirPermission;
            pe = (PermissionName)Convert.ToInt64(info.getkey(qq.ToString(), "permission"));
            return (pe >= Required ? "" : "[x]");
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

                Repeaters.SpeakOnce(e.FromQQ.Id, e.FromGroup.Id);
                Log("(" + e.FromGroup.Id + ")Message:" + e.FromQQ.Id + "," + e.Message.Text, ConsoleColor.Cyan);
                //Moring Protection
                Storage sys = new Storage("system");
                if (sys.getkey("root", "sleep") == "zzz")
                {
                    if (e.Message.Text.StartsWith("."))
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

                //Library Check
                foreach(MessagePoster.flowlibrary fl in MessagePoster.flibrary)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), fl.name + "检测") == "onk")
                    {
                        foreach (string c in fl.lib)
                        {
                            if (e.Message.Text.IndexOf(c) >= 0 && c != "")
                            {
                                Log("(" + e.FromGroup.Id + ")Baned " + e.FromQQ.Id + " for " + fl.name + " , " + e.Message.Text + " , key :" + c, ConsoleColor.Red);
                                e.Message.RemoveMessage();
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "疑似[" + fl.name + "]");
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
                        if (e.Message.Text == hmsg.msg && hmsg.hot > -2)
                        {
                            hothander = hmsg.id;
                            // 不是同一个QQ在刷屏
                            if (hmsg.qq.IndexOf(e.FromQQ.Id.ToString() + ";") < 0)
                            {
                                string[] qqtemp = hmsg.qq.Split(';');
                                Repeaters.ZeroRepeat(long.Parse(qqtemp[0]), e.FromGroup.Id);
                                if (qqtemp.Length == 2)
                                {
                                    Repeaters.FirstRepeat(e.FromQQ.Id, e.FromGroup.Id);
                                }
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
                                Repeaters.BoringRepeat(e.FromQQ.Id, e.FromGroup.Id);
                                hmsg.banqq = hmsg.banqq + e.FromQQ.Id.ToString() + ";";
                                int bancount = GotCount(hmsg.banqq, e.FromQQ.Id.ToString());
                                Log("(" + e.FromGroup.Id + ")(" + i + ")Boring-repeat:" + e.FromQQ.Id + " x " + bancount, ConsoleColor.Red);
                            }
                        }
                        //如果发言冷却，移除
                        if (hmsg.hot <= -10)
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

                //Summar Pictures
                if (e.Message.Text == ".drawlist")
                {
                    string pr = "";
                    foreach(string ptext in MessagePoster.ptList)
                    {
                        pr += ptext + "、";
                    }
                    e.FromGroup.SendGroupMessage(pr);
                    return;
                }
                if (e.Message.Text.StartsWith(".draw") || r.Next(0,666) == 233)
                {
                    if (ignore.getkey(e.FromGroup.Id.ToString(), "artist") == "√"){return;}
                    string pqq = e.FromQQ.Id.ToString();string pmsg = e.Message.Text;
                    string targett = MessagePoster.ptList[r.Next(0, MessagePoster.ptList.Count)];
                    if (e.Message.Text.StartsWith(".draw"))
                    {
                        if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                        string[] par = e.Message.Text.Split(' ');
                        if (par.Length > 1) { targett = par[1] == "?" ? targett : par[1]; }
                        if (par.Length > 2) { pqq = par[2].Replace("[CQ:at,qq=", "").Replace("]", ""); }
                        if (par.Length > 3) { pmsg = par[3]; }
                    }
                    GroupMemberInfo gmi;
                    try
                    {
                        gmi = e.FromGroup.GetGroupMemberInfo(Convert.ToInt64(pqq));
                    }
                    catch
                    {
                        e.FromGroup.SendGroupMessage("目标不在此群。");
                        return;
                    }
                    Log("Drawed " + targett + "!", ConsoleColor.Green);
                    if(!File.Exists("C:\\DataArrange\\PTemple\\" + targett + ".txt"))
                    {
                        e.FromGroup.SendGroupMessage("模板'" + targett + "'不存在。");
                        return;
                    }
                    StrangerInfo si = e.CQApi.GetStrangerInfo(Convert.ToInt64(pqq));
                    ScriptDrawer.Draw("C:\\DataArrange\\PTemple\\" + targett + ".txt", 
                                      MessagePoster.workpath + "\\data\\image\\" + targett + ".png",
                                      "[msg]", pmsg,
                                      "[qq]", pqq,
                                      "[nick]", gmi.Nick,
                                      "[card]", gmi.Card == "" ? gmi.Nick : gmi.Card,
                                      "[sex]", gmi.Sex.ToString(),
                                      "[age]", si.Age.ToString(),
                                      "[group]", e.FromGroup.Id.ToString()
                                      );
                    e.FromGroup.SendGroupMessage(CQApi.CQCode_Image(targett + ".png"));
                    return;
                }

                //CheckBack
                if (e.Message.Text.IndexOf("撤回了啥") >= 0)
                {
                    string cqq = e.Message.Text.Replace("[CQ:at,qq=", "").Replace("]", "").Replace("撤回了啥", "").Trim().Replace(" ", "");
                    int cindex = 1; string cret = ""; MessagePoster.HotMsg cmsg;
                    for (int i = 0; i < Manager.Hots.data.Count; i++)
                    {
                        cmsg = (MessagePoster.HotMsg)Manager.Hots.data[i];
                        Log("check: " + cmsg.qq + " , " + cmsg.group + " , " + cmsg.msg);
                        if ((cmsg.qq.IndexOf(cqq) >= 0) && (cmsg.group == e.FromGroup.Id))
                        {
                            cret = cret + "最近的第" + cindex + "条消息：" + "\n" + cmsg.msg + "\n";
                            cindex++;
                        }
                    }
                    e.FromQQ.SendPrivateMessage("消息回溯功能自动触发。\n" + cret);
                    Log("feed back to (" + cqq + "):\n" + cret);
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
                        pem.anger -= (GetTickCount() - pem.tick) / 666;
                        pem.lmsg = e.Message.Text;
                        if (e.Message.Text.IndexOf(pem.lmsg) >= 0 || pem.lmsg.IndexOf(e.Message.Text) >= 0) 
                        { pem.anger = pem.anger + 1.5; }
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
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "不理你了！");
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
                                double bant = Math.Pow(pem.anger, 3) / 200 * 15 / 2;
                                double band;double banh;double banm;
                                banm = bant;banh = banm / 60;band = banh / 24;
                                banm %= 60;banh %= 24;
                                if (band > 29) { band = 29; }
                                TimeSpan bantime = new TimeSpan((int)band, (int)banh,(int)banm, 0);
                                e.FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(e.FromQQ.Id), bantime);
                            }
                            if (e.Message.Text.StartsWith(".") || 
                                e.Message.Text.IndexOf("云") >= 0 || 
                                e.Message.Text.IndexOf("居然") >= 0 ||
                                e.Message.Text.IndexOf("语录集") >= 0 ||
                                e.Message.Text.StartsWith("switch"))
                            {
                                info.putkey(e.FromQQ.Id.ToString(), "permission", "-1");
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                                             "我非常生气...检测到您滥用机器人指令，系统自动封禁了您的权限，请联系开发者恢复使用权。");
                                Log("Auto baned " + e.FromQQ.Id, ConsoleColor.Red);
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
                    if ((r.Next(0, 100) == 88) || (FailAI == true))
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
                                    FailAI = false; MessagePoster.LetSay(tsay, e.FromGroup.Id,0,r.Next(0,2) == 1);
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

                //Anti-Anti-Robot
                if(r.Next(0,3) == 2 && e.FromQQ.Id == 1724673579)
                {
                    string[] tp = e.Message.Text.Split(']');
                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(1724673579),
                                                 "不许" + tp[tp.Length - 1]);
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
                            if (p[3] == "artist") { logg = "自动P表情包"; }
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
                    long lstime = 0;

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
                        string word = "";string ret = "";int longc = 0;

                        if (ignore.getkey(e.FromGroup.Id.ToString(), p[1]) == "√") { return; }
                        switch (p[1])
                        {
                            case ("msginfo"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.UserPermission)) { return; }
                                long tarqq = e.FromQQ.Id;
                                if (p.Length > 2) { tarqq = long.Parse(p[2].Replace("[CQ:at,qq=", "").Replace("]", "")); }
                                try
                                {
                                    GroupMemberInfo gmit = e.FromGroup.GetGroupMemberInfo(tarqq);
                                }
                                catch
                                {
                                    e.FromGroup.SendGroupMessage("目标不在此群。");
                                    return;
                                }
                                Repeaters.member meminfo = Repeaters.Information(tarqq, e.FromGroup.Id);
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(tarqq) ,"\n",
                                                             "总发言条数：" + meminfo.wcount + "条\n"
                                                             + "复读发起次数：" + meminfo.zfcount + "条("
                                                             + (int)(meminfo.zfcount / meminfo.wcount * 100) + "%)\n"
                                                             + "首次复读次数：" + meminfo.frcount + "条("
                                                             + (int)(meminfo.frcount / meminfo.wcount * 100) + "%)\n"
                                                             + "刷屏次数：" + meminfo.bacount + "条("
                                                             + (int)(meminfo.bacount / meminfo.wcount * 100) + "%)\n"
                                                             );
                                break;
                            case ("wreport"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.Error404)) { return; }
                                ProtectCount = 404;
                                e.FromGroup.SendGroupMessage("已将保护值设置到404防止其他消息干扰该操作。");
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
                                e.FromGroup.SendGroupMessage("语录集数据库总收集量：" + longc + "条" + "\n" +
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
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.Error404)) { return; }
                                ProtectCount = 404;
                                e.FromGroup.SendGroupMessage("已将保护值设置到404防止其他消息干扰该操作。");
                                string uifile = Guid.NewGuid().ToString();
                                File.Copy(@"C:\DataArrange\wordcollections-userdata.json",
                                    @"C:\DataArrange\[backup]wordcollections-" + uifile + ".json");
                                e.FromGroup.SendGroupMessage("备份[" + uifile + "]已创建。");
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
                                e.FromGroup.SendGroupMessage("removed");
                                ProtectCount = 0;
                                break;
                            case ("wdetail"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.Error404)) { return; }
                                word = Manager.wordcollect.getkey("repeat", "item" + p[2]);
                                e.FromGroup.SendGroupMessage("NO." + p[2] + "\n" + word);
                                break;
                            case ("wfetchs"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.Error404)) { return; }
                                longc = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                for (int i = 0; i < longc; i++)
                                {
                                    word = Manager.wordcollect.getkey("repeat", "item" + i);
                                    if (word.StartsWith(p[2])) ret = ret + i + " ";
                                }
                                e.FromGroup.SendGroupMessage(ret);
                                break;
                            case ("wfetch"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.Error404)) { return; }
                                longc = Convert.ToInt32(Manager.wordcollect.getkey("repeat", "count"));
                                for(int i = 0;i < longc; i++)
                                {
                                    word = Manager.wordcollect.getkey("repeat", "item" + i);
                                    if (word.IndexOf(p[2]) >= 0) ret = ret + i + " ";
                                }
                                e.FromGroup.SendGroupMessage(ret);
                                break;
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
                                if (IsWalling) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), " last task is processing ."); return; }
                                IsWalling = true;
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), " downloading ...");
                                WebClient wcd = new WebClient();
                                string wurl = Wallpaper.GetWallpaper();
                                wcd.DownloadFile(wurl, MessagePoster.workpath + "\\data\\image\\wall.jpg");
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),CQApi.CQCode_Image("wall.jpg"),wurl);
                                wcd.Dispose();
                                IsWalling = false;
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
                                    if (Convert.ToInt64(info.getkey(luser, "permission")) == -1) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "you can't do that , please edit the pms file on the server ('C:\\DataArrange\\') then restart the bot ."); return; }
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
                            case ("tkv"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
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
                                        MessagePoster.LetSay(tsay, e.FromGroup.Id,0,true);
                                    }
                                    else
                                    {
                                        MessagePoster.LetSay("no search result", e.FromGroup.Id, 0, true);
                                    }
                                }
                                break;
                            case ("tk"):
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.SupermanPermission)) { return; }
                                if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                for (int i = 2; i < p.Length; i++)
                                {
                                    ques = ques + p[i] + " ";
                                }
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
                                if (!JudgePermission(e.FromQQ.Id, PermissionName.AirPermission)) { return; }
                                if (p.Length > 2) { user = p[2]; }
                                pe = (PermissionName)Convert.ToInt64(info.getkey(user, "permission"));
                                pename = GetPermissionName(pe);
                                GroupMemberInfo gmi;
                                gmi = e.FromGroup.GetGroupMemberInfo(Convert.ToInt64(user));
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
                                                  "[msg]", e.Message.Text,
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
                                                  "[group]", e.FromGroup.Id.ToString()
                                                  );
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_Image("info.png"));
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
                                if (CanMatch(e.Message.Text,"*",  "壁纸", "桌面", "背景", "图片", "风景", "wall", "param")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.UserPermission) + ".wall 取得壁纸\n"; }
                                if (CanMatch(e.Message.Text,"*",  "404", "睡觉", "晚安", "关机", "强制", "force", "sleep")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.MasterPermission) + ".fsleep 服务器关机\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "禁言", "违规", "刷屏", "shit", "bun")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + ".ban <QQ> <分钟> 禁言\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "禁言", "违规", "刷屏", "shit", "全员", "所有人", "bun")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + ".aban true/false 开关全体禁言\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "危险", "祝福", "超级", "问候", "全员", "所有人", "super", "goodnight")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.MasterPermission) + ".sgdnight <内容> 艾特所有人发送\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "危险", "祝福", "超级", "问候", "全员", "所有人", "super", "goodnight")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.MasterPermission) + ".sgdnightf♂ <内容> 私聊所有人发送\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "新", "任命", "我要", "提权", "权限", "地位", "manger")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.MasterPermission) + ".man <QQ> 设置管理员\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "撤", "罢免", "我要", "降权", "权限", "地位", "manger")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.MasterPermission) + ".unman <QQ> 取消管理员\n"; }
                                if (CanMatch(e.Message.Text,"*",  "permission", "令牌", "修改", "系统", "更改", "权限", "地位", "permiss", "pemission", "permision")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.UserPermission) + ".pms <QQ> <权限> 授予权限\n"; }
                                if (CanMatch(e.Message.Text,"*",  "聊天", "无逻辑", "无聊", "百度", "搜索", "骚")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + ".tk <内容> 谈话\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "头衔", "名片", "永久")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + ".honor <内容> 专属头衔设置\n"; }
                                if (CanMatch(e.Message.Text,"*",  "赞", "资料卡", "名片", "个人", "prise", "prase")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + ".prs 点赞十次\n"; }
                                if (CanMatch(e.Message.Text,"*",  "管理", "头衔", "名片", "锁定", "昵称", "lock")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + ".lkc <QQ> <名称> 锁定名片\n"; }
                                if (CanMatch(e.Message.Text,"*",  "复读", "信息", "统计", "数据", "最热", "今日", "发言", "热词", "poplular")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.AirPermission) + ".pop 今日最热复读\n"; }
                                if (CanMatch(e.Message.Text,"*",  "信息", "统计", "用户", "数据", "人数")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.AirPermission) + ".info [QQ] 显示信息\n"; }
                                if (CanMatch(e.Message.Text,"*",  "game", "游戏", "UT", "ut", "传说之下", "undertale", "Undertale", "UnderTale", "猜词", "猜台词")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.UserPermission) + ".ut 传说之下猜角色游戏\n"; }
                                if (CanMatch(e.Message.Text,"*",  "game", "游戏", "UT", "ut", "传说之下", "undertale", "Undertale", "UnderTale", "猜词", "猜台词")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.UserPermission) + ".ut 传说之下完型游戏\n"; }
                                if (CanMatch(e.Message.Text,"*",  "game", "游戏", "void", "Void", "虚拟", "人生", "life", "虚假", "live")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.UserPermission) + ".bvoid <角色名> 开始虚假人生游戏\njvoid 加入游戏\ncvoid 关闭游戏\n"; }
                                if (CanMatch(e.Message.Text,"*",  "praise", "表扬", "问候", "祝福", "好")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.UserPermission) + ".nice 向你问好\n"; }
                                if (CanMatch(e.Message.Text,"*",  "word", "collect", "复读", "语录", "录")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.UserPermission) + ".word 一个语录\n"; }
                                if (CanMatch(e.Message.Text, "*", "switch", "开关", "开启", "关闭", "功能")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.MasterPermission) + "switch t/<群号> on/off <功能> 开关功能\n"; }
                                if (CanMatch(e.Message.Text, "*", "draw", "art", "paint", "ps", "图"))
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + ".draw ?/[模板名] [QQ] P图\n"; }
                                if (CanMatch(e.Message.Text, "*", "draw", "art", "paint", "ps", "图")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.AirPermission) + ".drawlist 所有P图模板\n"; }
                                if (CanMatch(e.Message.Text, "*", "word", "collect", "复读", "语录", "录","云")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + "<名字>云 输出对应语录\n"; }
                                if (CanMatch(e.Message.Text, "*", "word", "collect", "复读", "语录", "录", "云")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + "<名字>云：<内容> 收集语录\n"; }
                                if (CanMatch(e.Message.Text, "*", "word", "collect", "复读", "语录", "录", "云")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + "<名字>居然<内容> 输出相关语录\n"; }
                                if (CanMatch(e.Message.Text, "*", "word", "collect", "复读", "语录", "录", "云")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.SupermanPermission) + "<名字>语录集 输出所有语录\n"; }
                                if (CanMatch(e.Message.Text, "*", "word", "time", "撤回", "回")) 
                                { cmdstr += JP(e.FromQQ.Id, PermissionName.AirPermission) + "<艾特QQ>撤回了啥 回溯消息\n"; }

                                if (cmdstr == "")
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                    "未知指令'" + p[1] + "', 发送'.help *'取得详细信息。");
                                }
                                else
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                    cmdstr + "[x]表示无法操作对应指令，发送.info查看权限。");
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
