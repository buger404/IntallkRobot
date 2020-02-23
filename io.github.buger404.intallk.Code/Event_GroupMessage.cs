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
using ArtificalA.Intelligence;
using System.Threading;
using System.Runtime.InteropServices;
using MainThread;
using Wallpapers.Searcher;
using Undertale.Dialogs;

namespace io.github.buger404.intallk.Code
{
    // 添加引用 IGroupMessage
    public class Event_GroupMessage: IGroupMessage
    {
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
            public long anger;
            public long qq;
        }
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
        // 接收事件
        public void GroupMessage(object sender,CQGroupMessageEventArgs e)
        {
            int sid; double qq = 0;
            List<Native.Csharp.Sdk.Cqp.Model.GroupMemberInfo> mem;
            Random r = new Random(); string ques = "";
            int hIndex = -1; MainThread.MessagePoster.HotMsg hhmsg = new MainThread.MessagePoster.HotMsg();

            Log("(" + e.FromGroup.Id + ")Message:" + e.FromQQ.Id + "," + e.Message.Text,ConsoleColor.Cyan);

            //Undertale Gameing
            if (UT.targetg == e.FromGroup.Id)
            {
                if (e.Message.Text.ToLower() == UT.role)
                {
                    //e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),"回答正确！！！");
                    UT.winstr = UT.winstr + CQApi.CQCode_At(e.FromQQ.Id) + " correct +" + (int)(UT.prise * 10) / 10 + " points\n";
                    SolvePlayer(UT.prise,e.FromQQ.Id); UT.prise *= 0.8f;
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
                if (hmsg.group == e.FromGroup.Id) { hhmsg = hmsg;hIndex = i; break; }
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
                            Log("(" + e.FromGroup.Id + ")(" + i + ")Delay repeat:" + hmsg.msg, ConsoleColor.Yellow);
                            e.FromGroup.SendGroupMessage(hmsg.msg);
                            hmsg.delaymsg = false; hmsg.hasre = true;
                            Log("(" + e.FromGroup.Id + ")(" + i + ")Conntinue-repeat:" + hmsg.delaymsg, ConsoleColor.Yellow);
                        }
                    }
                    if (e.Message.Text != hmsg.msg) { hmsg.hot--;} //如果当前的发言和上句发言不同，上句发言热度-1
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
                                    hhmsg = hmsg;hhmsg.hasup = false;
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
                            int bancount = GotCount(hmsg.banqq,e.FromQQ.Id.ToString());
                            Log("(" + e.FromGroup.Id + ")(" + i + ")Boring-repeat:" + e.FromQQ.Id + " x " + bancount, ConsoleColor.Red);
                        }
                    }
                    //如果发言冷却，移除
                    if (hmsg.hot <= -2) 
                    {
                        if (hmsg.hasup) { Log("(" + e.FromGroup.Id + ")(" + i + ")Disapper repeat:" + hmsg.msg, ConsoleColor.Red); }
                        Manager.Hots.data.RemoveAt(i); goto nexthmsg; 
                    } 
                    //发言热度越高，越容易引发复读
                    if (r.Next(0, 100) > 100 - hmsg.hot * 20 * (hmsg.hot / 2))
                    {
                        hmsg.delaymsg = (r.Next(0, 10) > 6); //设置延迟一回合
                        hothander = hmsg.id;
                        if (!hmsg.delaymsg)
                        {
                            if (hmsg.hasre == false)
                            {
                                Log("(" + e.FromGroup.Id + ")(" + i + ")Repeat:" + hmsg.msg + ",impossible:" + (100 - hmsg.hot * 20 * (hmsg.hot / 2)), ConsoleColor.Yellow);
                                e.FromGroup.SendGroupMessage(hmsg.msg);
                                hmsg.hasre = true;
                            }
                        }
                        else
                        {
                            Log("(" + e.FromGroup.Id + ")(" + i + ")Delay set:" + hmsg.msg, ConsoleColor.Yellow);
                        }
                    }
                    if (i >= Manager.Hots.data.Count) { break; }
                    Manager.Hots.data[i] = hmsg;
                }
            }
            //如果当前发言没有被处理，则加入新热点
            if (hothander == 0)
            {
                hmsg.group = e.FromGroup.Id; hmsg.msg = e.Message.Text; hmsg.hot = 1; hmsg.delaymsg = false;
                hmsg.qq = e.FromQQ.Id.ToString() + ";"; hmsg.banqq = ""; hmsg.id = DateTime.Now.Ticks;
                hmsg.hasup = false; hmsg.hasre = false;
                Manager.Hots.data.Add(hmsg);
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

            //Screen Checker
            int ssid = -1; personmsg pem = new personmsg();
            for (int i = 0; i < Manager.scrBan.data.Count; i++)
            {
                pem = (personmsg)Manager.scrBan.data[i];
                if (pem.qq == e.FromQQ.Id) 
                {
                    ssid = i;
                    if (GetTickCount() - pem.tick <= 2000)
                    {
                        pem.anger++;
                        switch (pem.anger)
                        {
                            case (2):
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "不许刷屏");
                                break;
                            case (3):
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "不要刷屏了啦");
                                break;
                            case (4):
                                if (e.FromGroup.Id != 490623220)
                                {
                                    TimeSpan bantime = new TimeSpan(0, 10, 0);
                                    e.FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(e.FromQQ.Id), bantime);
                                }
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "为什么你不愿意听我的警告呢");
                                break;
                            case (5):
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "别再刷屏了，好吗？");
                                break;
                            case (6):
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "不理你了！欺负人家不是管理员！");
                                break;
                            default:
                                break;
                        }
                        Log("(" + e.FromGroup.Id + ")(" + i + ")This human is too too boring:" + e.FromQQ.Id + "x" + pem.anger, ConsoleColor.Red);
                    }
                    else
                    {
                        pem.anger -= (GetTickCount() - pem.tick) / 2000;
                        if (pem.anger < 0) { pem.anger = 0; }
                    }
                    pem.tick = GetTickCount();
                    Manager.scrBan.data[i] = pem;
                }
            }
            if (ssid == -1)
            {
                pem.qq = e.FromQQ.Id; pem.anger = 0; pem.tick = GetTickCount();
                Manager.scrBan.data.Add(pem);
            }

            //Random Topic
            string tsay = "";
            if (e.Message.Text.IndexOf("[CQ:") < 0)
            {
                if ((r.Next(0, 200) == 88) || (FailAI == true))
                {
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
                                FailAI = false; e.FromGroup.SendGroupMessage(tsay);
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

            //Console Processing
            PermissionName pe = PermissionName.AirPermission; string pename = "";

            if (e.Message.Text.StartsWith("ik "))
            {
                sid = Manager.CPms.SearchFor(e.FromQQ.Id);
                if (e.FromGroup == 490623220) 
                {
                    if (sid == -1)
                    {
                        Manager.CPms.data.Add(e.FromQQ.Id);
                        Manager.CPms.data.Add(PermissionName.UserPermission);
                        Manager.CPms.SaveData();
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "you've got the permission：User");
                        sid = Manager.CPms.SearchFor(e.FromQQ.Id);
                    }
                }
                if(sid == -1)
                {
                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation denied , you have not any permission");
                    return;
                }

                try
                {
                    pe = (PermissionName)Convert.ToInt64(Manager.CPms.data[sid + 1].ToString());
                    pename = GetPermissionName(pe);
                    string[] p = e.Message.Text.Replace("[CQ:at,qq=", "").Replace("]", "").Split(' ');
                    if (p.Length < 2) 
                    {
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "Intallk Robot（1.7.0）, powered by CoolQ");
                        return;
                    }
                    switch (p[1])
                    {
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
                        case("ut"):
                            if (UT.targetg == 0)
                            {
                                e.FromGroup.SendGroupMessage("a new game : 'undertale guessing characters' has been on");
                                UT.mode = 0;
                                UT.targetg = e.FromGroup.Id;UT.round = 0; UT.ps.Clear();
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
                        case("pop"):
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
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (VoidLifes.IsStart())
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " the game has been on");
                            }
                            else
                            {
                                if (p.Length < 3) { throw new Exception("please name for the character:ink bvoid [name]"); }
                                VoidLifes.BeginGame(p[2]); VoidLifes.TargetGroup = e.FromGroup.Id;
                                VoidLifes.JoinGame(e.FromQQ.Id); VoidLifes.OwnerQQ = e.FromQQ.Id;
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " game begins");
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + " you've joined the game");
                            }
                            break;
                        case ("jvoid"):
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
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
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
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
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
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
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
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
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "该功能正在回炉改造中..."); return;
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            for (int i = 2; i < p.Length; i++)
                            {
                                ques = ques + p[i] + " ";
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "csdn"));
                            break;
                        case ("wall"):
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + Wallpaper.GetWallpaper());
                            break;
                        case ("fsleep"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(1361778219), " sudo sleep !");
                            ShellExecute(IntPtr.Zero, "open", @"shutdown", "-s -t 120", "", ShowCommands.SW_SHOWNORMAL);
                            break;
                        case ("ban"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (p.Length < 4) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            qq = Convert.ToDouble(p[2]);
                            TimeSpan ctime = new TimeSpan(0,Convert.ToInt16(p[3]),0);
                            if (e.FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(qq), ctime))
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation succesfully");
                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation denied");
                            }
                            break;
                        case ("aban"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            bool bswitch = Convert.ToBoolean(p[2]);
                            if (bswitch)
                            {
                                if (e.FromGroup.SetGroupBanSpeak())
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation succesfully");
                                }
                                else
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation denied");
                                }
                            }
                            else
                            {
                                e.FromGroup.RemoveGroupBanSpeak();
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation succesfully");
                            }
                            break;
                        case ("man"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            qq = Convert.ToDouble(p[2]);
                            e.FromGroup.SetGroupManage(Convert.ToInt64(qq));
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(Convert.ToInt64(qq)), "you are manager now");
                            break;
                        case ("unman"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            qq = Convert.ToDouble(p[2]);
                            e.FromGroup.RemoveGroupManage(Convert.ToInt64(qq));
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(Convert.ToInt64(qq)), "you are not manager now");
                            break;
                        case ("sgdnightf♂"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
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
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
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
                                e.FromGroup.SendGroupMessage(atstr,p[2]);
                                Thread.Sleep(3000);
                            }
                            break;
                        case ("pms"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            qq = Convert.ToDouble(p[2]);
                            sid = Manager.CPms.SearchFor(qq);
                            if (sid != -1)
                            {
                                if (p.Length < 4) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                                if (Convert.ToInt64(Manager.CPms.data[sid + 1]) >= Convert.ToInt64(pe)) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied to change pms '" + GetPermissionName((PermissionName)(Convert.ToInt64(Manager.CPms.data[sid + 1]))) + "'(level " + Convert.ToInt64(Manager.CPms.data[sid + 1]) + ") ", CQApi.CQCode_At(Convert.ToInt64(qq))); return; }
                                if (Convert.ToInt64(p[3]) >= Convert.ToInt64(pe)) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied to give pms '" + GetPermissionName((PermissionName)(Convert.ToInt64(p[3]))) + "'(level " + Convert.ToInt64(p[3]) + ")"); return; }
                                Manager.CPms.data[sid + 1] = p[3];
                                Manager.CPms.SaveData();
                                pe = (PermissionName)Convert.ToInt64(p[3]);
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)) + " pms:" + GetPermissionName(pe) + "(level " + Convert.ToInt64(pe) + ") pid:" + sid);
                            }
                            else
                            {
                                Manager.CPms.data.Add(qq);
                                Manager.CPms.data.Add(1);
                                Manager.CPms.SaveData();
                                sid = Manager.CPms.SearchFor(qq);
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                    CQApi.CQCode_At(Convert.ToInt64(qq)) + " has permission now , pid:"+ sid);
                            }
                            break;
                        case ("honor"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            if (e.FromQQ.SetGroupMemberForeverExclusiveTitle(e.FromGroup.Id,p[2]) == false)
                            {
                                throw new Exception("operation denied");
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation succeeded");
                            break;
                        case ("prs"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (e.FromQQ.SendPraise(10) == false)
                            {
                                throw new Exception("operation denied");
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "operation succeeded");
                            break;
                        case ("tk"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            for (int i = 2; i < p.Length; i++)
                            {
                                ques = ques + p[i] + " ";
                            }
                            tsay = ArtificalAI.Talk(ques, "tieba");
                            if (tsay != "")
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + tsay);
                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "not search result");
                            }
                            break;
                        case ("tko"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'was given incorrect params"); }
                            for (int i = 2; i < p.Length; i++)
                            {
                                ques = ques + p[i] + " ";
                            }
                            tsay = ArtificalAI.Talk(ques, "baidu");
                            if (tsay != "")
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + tsay);
                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "not search result");
                            }
                            break;
                        case ("info"):
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "user:" + (int)(Manager.CPms.data.Count / 2) + " msg:" + Manager.scrBan.data.Count + " repeats:" + Manager.Hots.data.Count);
                            break;
                        case ("pmsi"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
                            string omsg = "";
                            for (int i = 0; i < Manager.CPms.data.Count; i+=2)
                            {
                                try
                                {
                                    omsg = omsg + "user(" + (i / 2 + 1) + "):" + Manager.CPms.data[i] + " " + "pms:" + (PermissionName)Convert.ToInt64(Manager.CPms.data[i + 1].ToString()) + "\n";
                                }
                                catch
                                {
                                    omsg = omsg + "user(" + (i / 2 + 1) + "):" + Manager.CPms.data[i] + " " + "pms:<failure>\n";
                                }
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "users:" + (int)(Manager.CPms.data.Count / 2) + "\n" + omsg);
                            break;
                        case ("lkc"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "your pms '" + pename + "'(level " + Convert.ToInt64(pe) + ") denied"); return; }
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
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),CQApi.CQCode_At(Convert.ToInt64(qq)),
                                    "operation succeeded add:" + sid + 
                                    " lkc:" + Manager.LCards.data[sid+1]);
                            }
                            break;
                        default:
                            string cmdstr = "";
                            if (CanMatch(e.Message.Text, "help", "msdn", "搜索", "资料", "文档", "国外", "微软", "巨硬", "microsoft")) { cmdstr += "*msdn [content] to search in msdn <weak>\n"; }
                            if (CanMatch(e.Message.Text, "help", "csdn", "搜索", "资料", "文档", "国内")) { cmdstr += "csdn [content] to search in csdn <weak>\n"; }
                            if (CanMatch(e.Message.Text, "help", "壁纸", "桌面", "背景", "图片", "风景", "wall", "param")) { cmdstr += "wall to get a wallpaper\n"; }
                            if (CanMatch(e.Message.Text, "help", "404", "睡觉", "晚安", "关机", "强制", "force", "sleep")) { cmdstr += "#fsleep to force 404 to shutdown in 2 min\n"; }
                            if (CanMatch(e.Message.Text, "help", "管理", "禁言", "违规", "刷屏", "shit", "bun")) { cmdstr += "*ban [qq] [minutes] to ban somebody some time\n"; }
                            if (CanMatch(e.Message.Text, "help", "管理", "禁言", "违规", "刷屏", "shit", "全员", "所有人", "bun")) { cmdstr += "*aban [bool] to ban all\n"; }
                            if (CanMatch(e.Message.Text, "help", "管理", "危险", "祝福", "超级", "问候", "全员", "所有人", "super", "goodnight")) { cmdstr += "#sgdnight [content] to at all and send msg\n"; }
                            if (CanMatch(e.Message.Text, "help", "管理", "危险", "祝福", "超级", "问候", "全员", "所有人", "super", "goodnight")) { cmdstr += "#sgdnightf♂ [content] to send private msg to all\n"; }
                            if (CanMatch(e.Message.Text, "help", "管理", "新", "任命", "我要", "提权", "权限", "地位", "manger")) { cmdstr += "#man [qq] to set a new manager\n"; }
                            if (CanMatch(e.Message.Text, "help", "管理", "撤", "罢免", "我要", "降权", "权限", "地位", "manger")) { cmdstr += "#unman [qq] to cancel a manager\n"; }
                            if (CanMatch(e.Message.Text, "help", "permission", "令牌", "修改", "系统", "更改", "权限", "地位", "permiss", "pemission", "permision")) { cmdstr += "!pms [qq] [pid] to give others pms\n"; }
                            if (CanMatch(e.Message.Text, "help", "聊天", "无逻辑", "无聊", "百度", "搜索", "骚")) { cmdstr += "tk [content] to talk with ik\n"; }
                            if (CanMatch(e.Message.Text, "help", "聊天", "无逻辑", "无聊", "百度", "搜索", "骚")) { cmdstr += "tko [content] to talk with ik\n"; }
                            if (CanMatch(e.Message.Text, "help", "管理", "头衔", "名片", "永久")) { cmdstr += "*honor [content] to give yourself a title\n"; }
                            if (CanMatch(e.Message.Text, "help","赞", "资料卡", "名片", "个人","prise","prase")) { cmdstr += "*prs to praise you 10s\n"; }
                            if (CanMatch(e.Message.Text, "help", "管理", "头衔", "名片", "锁定", "昵称", "lock")) { cmdstr += "*lkc [qq] [name] to lock one's card\n"; }
                            if (CanMatch(e.Message.Text, "help", "复读", "信息", "统计", "数据", "最热", "今日", "发言", "热词", "poplular")) { cmdstr += "pop to output today's popular sentence\n"; }
                            if (CanMatch(e.Message.Text, "help", "信息", "统计", "用户", "数据", "人数")) { cmdstr += "info to output all user info\n"; }
                            if (CanMatch(e.Message.Text, "help","信息", "统计", "用户", "数据", "权限")) { cmdstr += "#pmsi to output all user pms info\n"; }
                            if (CanMatch(e.Message.Text, "game", "游戏", "UT", "ut", "传说之下", "undertale","Undertale","UnderTale","猜词","猜台词")) { cmdstr += "ut to start a ut guessing characters game\n"; }
                            if (CanMatch(e.Message.Text, "game", "游戏", "UT", "ut", "传说之下", "undertale", "Undertale", "UnderTale", "猜词", "猜台词")) { cmdstr += "ut to start a ut guessing game\n"; }

                            if (cmdstr == "")
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                "unknown command '" + p[1] + "'");
                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                "unknown command '" + p[1] + "' , you may mean ...\n" +
                                cmdstr + "your pms:" + pename + "(level " + Convert.ToInt64(pe) + ")");
                            }
                            break;
                    }
                }
                catch(Exception err)
                {
                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), err.Message);
                    Log(err.StackTrace + "\n" + err.Message, ConsoleColor.Red);
                }
                return;
            }
        }
    }
}
