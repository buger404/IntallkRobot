using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
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
            HeavenPermission = 4 //可以访问我自己的计算机
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
        // 接收事件
        public void GroupMessage(object sender,CQGroupMessageEventArgs e)
        {
            int sid; double qq = 0;
            List<Native.Csharp.Sdk.Cqp.Model.GroupMemberInfo> mem;
            Random r = new Random(); string ques = "";
            int hIndex = -1; MainThread.MessagePoster.HotMsg hhmsg = new MainThread.MessagePoster.HotMsg();

            Log("(" + e.FromGroup.Id + ")Message:" + e.FromQQ.Id + "," + e.Message.Text,ConsoleColor.Cyan);

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
                        if (e.FromGroup.Id != 490623220)
                        {
                            TimeSpan bantime = new TimeSpan(0, 10, 0);
                            e.FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(e.FromQQ.Id), bantime);
                        }
                        Log("(" + e.FromGroup.Id + ")(" + i + ")This human is too too boring:" + e.FromQQ.Id + "x" + pem.anger, ConsoleColor.Red);
                    }
                    else
                    {
                        pem.anger = 0;
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
                if ((r.Next(0, 100) == 50) || (FailAI == true))
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
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "检测到您的群名片与主人设定的群名片不相符，已强制修正为：", tname);
                    }
                    else
                    {
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(1361778219), "强制修正目标群员的群名片失败，可能本机没有权限操作。");
                    }

                }
                return;
            }

            //Console Processing
            PermissionName pe = PermissionName.AirPermission; string pename = "";

            if (e.Message.Text.StartsWith("intallk"))
            {
                sid = Manager.CPms.SearchFor(e.FromQQ.Id);
                if (e.FromGroup == 490623220) 
                {
                    if (sid == -1)
                    {
                        Manager.CPms.data.Add(e.FromQQ.Id);
                        Manager.CPms.data.Add(PermissionName.UserPermission);
                        Manager.CPms.SaveData();
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您已通过此群的特殊地位拿到访问权限：User");
                        sid = Manager.CPms.SearchFor(e.FromQQ.Id);
                    }
                }
                if(sid == -1)
                {
                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您没有任何权限，禁止访问Intallk QQ机器人控制台。");
                    return;
                }

                try
                {
                    pe = (PermissionName)Convert.ToInt64(Manager.CPms.data[sid + 1].ToString());
                    pename = GetPermissionName(pe);
                    string[] p = e.Message.Text.Split(' ');
                    if (p.Length < 2) 
                    {
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "关于黑嘴Intallk稽气人（1.6.0），垃圾臭狗机器人的同义词。\n制作：Error 404（QQ1361778219）\nGithub地址：https://github.com/buger404\nPowered by CoolQ\n" +
                            "您当前所持权限：" + pename + "(级别：" + Convert.ToInt64(pe) + "）\n" +
                            "拥有智能复读，刷屏处理，自主发言，激怒他人，引战（划掉）等功能。");
                        return;
                    }
                    switch (p[1])
                    {
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
                                e.FromGroup.SendGroupMessage("截止刚才，本群今日最热发言：\n" + hhmsg.msg + "\n复读发起人：" + CQApi.CQCode_At(Convert.ToInt64(qtemp[0])) + "\n复读热度：" + hhmsg.hot + "\n该热度发言被这些成员复读过：" + fstr + "\n在该热度发言发起时，这些成员太过激动试图刷屏：" + estr);
                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage("截止刚才我群今日暂无最热发言。");
                            }
                            break;
                        case ("msdn"):
                            //msdn searching
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "该功能正在回炉改造中..."); return;
                            for (int i = 2; i < p.Length; i++)
                            {
                                ques = ques + p[i] + " ";
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "msdn"));
                            break;
                        case ("csdn"):
                            //csdn searching
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "该功能正在回炉改造中..."); return;
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            for (int i = 2; i < p.Length; i++)
                            {
                                ques = ques + p[i] + " ";
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "csdn"));
                            break;
                        case ("wallpaper"):
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "您喜欢这个壁纸吗？\n" + Wallpaper.GetWallpaper());
                            break;
                        case ("forcesleep"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(1361778219), " 滚去睡觉吧！");
                            ShellExecute(IntPtr.Zero, "open", @"shutdown", "-s -t 120", "", ShowCommands.SW_SHOWNORMAL);
                            break;
                        case ("ban"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            qq = Convert.ToDouble(p[2]);
                            TimeSpan ctime = new TimeSpan(0,Convert.ToInt16(p[3]),0);
                            if (e.FromGroup.SetGroupMemberBanSpeak(Convert.ToInt64(qq), ctime))
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "禁言目标成员成功。");
                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "禁言目标成员失败，可能本机没有权限。");
                            }
                            break;
                        case ("allban"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            bool bswitch = Convert.ToBoolean(p[2]);
                            if (bswitch)
                            {
                                if (e.FromGroup.SetGroupBanSpeak())
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "禁言全体成员成功。");
                                }
                                else
                                {
                                    e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "禁言全体成员失败，可能本机没有权限。");
                                }
                            }
                            else
                            {
                                e.FromGroup.RemoveGroupBanSpeak();
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "解禁全体成员成功。");
                            }
                            break;
                        case ("manager"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            qq = Convert.ToDouble(p[2]);
                            e.FromGroup.SetGroupManage(Convert.ToInt64(qq));
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(Convert.ToInt64(qq)), "您已被授予管理员。");
                            break;
                        case ("unmanager"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            qq = Convert.ToDouble(p[2]);
                            e.FromGroup.RemoveGroupManage(Convert.ToInt64(qq));
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(Convert.ToInt64(qq)), "您的管理员宝座被移除。");
                            break;
                        case ("supergoodnightfancy♂"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            mem = e.FromGroup.GetGroupMemberList();
                            for (int i = 0; i < mem.Count; i++)
                            {
                                mem[i].QQ.SendPrivateMessage(p[2]);
                                Log("Wish:" + i, ConsoleColor.Yellow);
                                //e.FromGroup.SendGroupMessage(atstr, p[2]);
                                Thread.Sleep(500);
                            }
                            break;
                        case ("supergoodnight"):
                            if (pe < PermissionName.MasterPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            string atstr = "";
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
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
                        case ("permission"):
                            if (pe < PermissionName.HeavenPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            qq = Convert.ToDouble(p[2]);
                            sid = Manager.CPms.SearchFor(qq);
                            if (sid != -1)
                            {
                                Manager.CPms.data[sid + 1] = p[3];
                                Manager.CPms.SaveData();
                                pe = (PermissionName)Convert.ToInt64(p[3]);
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "\n" +
                                    CQApi.CQCode_At(Convert.ToInt64(qq)) + "的权限已经更新。\n" +
                                    "权限：" + GetPermissionName(pe) + "(级别：" + Convert.ToInt64(pe) + ")\n" +
                                    "权限编号：" + sid + "\n");
                            }
                            else
                            {
                                Manager.CPms.data.Add(qq);
                                Manager.CPms.data.Add(PermissionName.UserPermission);
                                Manager.CPms.SaveData();
                                sid = Manager.CPms.SearchFor(qq);
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                    "成功给予" + CQApi.CQCode_At(Convert.ToInt64(qq)) + "权限！\n" +
                                    "权限编号：" + sid);
                            }
                            break;
                        case ("honor"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            if (e.FromQQ.SetGroupMemberForeverExclusiveTitle(e.FromGroup.Id,p[2]) == false)
                            {
                                throw new Exception("设置您的专属头衔失败，可能没有权限");
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "设置成功！");
                            break;
                        case ("praise"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (e.FromQQ.SendPraise(10) == false)
                            {
                                throw new Exception("点赞失败");
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "成功为您刷入十个赞");
                            break;
                        case ("talk"):
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
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
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "怀疑超出人类认知范围。");
                            }
                            break;
                        case ("talk_old"):
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
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
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "怀疑超出人类认知范围。");
                            }
                            break;
                        case ("info"):
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "黑嘴酱用户人数：" + (int)(Manager.CPms.data.Count / 2) + "\n消息受理人数：" + Manager.scrBan.data.Count + "\n活跃的复读个数：" + Manager.Hots.data.Count);
                            break;
                        case ("permissioninfo"):
                            if (pe < PermissionName.HeavenPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            string omsg = "";
                            for (int i = 0; i < Manager.CPms.data.Count; i+=2)
                            {
                                try
                                {
                                    omsg = omsg + "用户" + (i / 2 + 1) + " " + Manager.CPms.data[i] + " " + "权限 " + (PermissionName)Convert.ToInt64(Manager.CPms.data[i + 1].ToString()) + "\n";
                                }
                                catch
                                {
                                    omsg = omsg + "用户" + (i / 2 + 1) + " " + Manager.CPms.data[i] + " " + "权限 <取得权限失败>\n";
                                }
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "黑嘴酱用户人数：" + Manager.CPms.data.Count + "\n" + omsg);
                            break;
                        case ("lockcard"):
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 3) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            qq = Convert.ToDouble(p[2]); string lname = p[3];
                            sid = Manager.LCards.SearchFor(qq);
                            if (sid != -1)
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), CQApi.CQCode_At(Convert.ToInt64(qq)),
                                    "目标成员的群名片已经被锁定过了。\n"+ 
                                    "锁定地址：" + sid + 
                                    "\n上次锁定的名片：" + Manager.LCards.data[sid+1] + 
                                    "\n已修改锁定的名片为当前新设定的名片。");
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
                                    "锁定目标成员的群名片成功！\n" +    
                                    "锁定地址：" + sid + 
                                    "\n锁定的名片：" + Manager.LCards.data[sid+1]);
                            }
                            break;
                        default:
                            string cmdstr = "";
                            if (CanMatch(e.Message.Text,"msdn","搜索","资料","文档","国外","微软","巨硬","microsoft")) { cmdstr += "*msdn [content]] 检索msdn并返回页面内容（容易失败）\n"; }
                            if (CanMatch(e.Message.Text, "csdn", "搜索", "资料", "文档", "国内")) { cmdstr += "csdn [content] 检索csdn并返回页面内容（阅读效果不佳）\n"; }
                            if (CanMatch(e.Message.Text, "壁纸", "桌面", "背景", "图片", "风景","wall","param")) { cmdstr += "wallpaper 获取一张随机桌面壁纸\n" ; }
                            if (CanMatch(e.Message.Text, "404", "睡觉", "晚安", "关机", "强制", "force", "sleep")) { cmdstr += "#forcesleep 强制设定404的电脑在2分钟后关机\n"; }
                            if (CanMatch(e.Message.Text, "管理", "禁言", "违规", "刷屏", "shit","bun")) { cmdstr += "*ban [qq] [time] 设定群成员禁言（分钟）\n" ; }
                            if (CanMatch(e.Message.Text, "管理", "禁言", "违规", "刷屏", "shit","全员","所有人","bun")) { cmdstr += "*allban [bool] 开关全体禁言\n"; }
                            if (CanMatch(e.Message.Text, "管理", "危险", "祝福", "超级", "问候", "全员", "所有人","super","goodnight")) { cmdstr += "#supergoodnight [content] 将所有成员艾特一遍发送指定消息\n"; }
                            if (CanMatch(e.Message.Text, "管理", "危险", "祝福", "超级", "问候", "全员", "所有人", "super", "goodnight")) { cmdstr += "#supergoodnightfancy♂ [content] 将所有成员私聊一遍发送指定消息\n"; }
                            if (CanMatch(e.Message.Text, "管理", "新", "任命", "我要", "提权", "权限", "地位","manger")) { cmdstr += "#manager [qq] 设置新的管理员\n"; }
                            if (CanMatch(e.Message.Text, "管理", "撤", "罢免", "我要", "降权", "权限", "地位", "manger")) { cmdstr += "#unmanager [qq] 解除管理员\n"; }
                            if (CanMatch(e.Message.Text, "permission", "令牌", "修改", "系统", "更改", "权限", "地位","permiss","pemission","permision")) { cmdstr += "!permission [qq] [pid] 设置权限\n"; }
                            if (CanMatch(e.Message.Text, "聊天", "无逻辑", "无聊", "百度", "搜索", "骚")) { cmdstr += "talk [content] 无逻辑谈话（百度贴吧）\n"; }
                            if (CanMatch(e.Message.Text, "聊天", "无逻辑", "无聊", "百度", "搜索", "骚")) { cmdstr += "talk_old [content] 无逻辑谈话（百度知道）\n"; }
                            if (CanMatch(e.Message.Text, "管理", "头衔", "名片", "永久")) { cmdstr += "*honor [content] 给予自己永久头衔\n"; }
                            if (CanMatch(e.Message.Text, "赞", "资料卡", "名片", "个人","prise","prase")) { cmdstr += "*praise 为自己发送10个赞\n"; }
                            if (CanMatch(e.Message.Text, "管理", "头衔", "名片", "锁定", "昵称","lock")) { cmdstr += "*lockcard [qq] [name] 检测到指定成员名片与设定不符时自动修改\n"; }
                            if (CanMatch(e.Message.Text, "复读", "信息", "统计", "数据", "最热", "今日", "发言", "热词", "poplular")) { cmdstr += "pop 输出今日本群最热发言\n"; }
                            if (CanMatch(e.Message.Text, "信息", "统计", "用户", "数据","人数")) { cmdstr += "info 输出用户信息\n"; }
                            if (CanMatch(e.Message.Text, "信息", "统计", "用户", "数据", "权限")) { cmdstr += "#permissioninfo 输出所有用户的权限信息\n"; }

                            if (cmdstr == "")
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                "'" + p[1] + "'不是有效的指令，表示不知道您要做什么噢。");
                            }
                            else
                            {
                                e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                                "'" + p[1] + "'不是有效的指令，您可能需要...？\n（无标注:User，*Superman，#Master，!Heaven）\n" +
                                cmdstr + "您当前所持权限：" + pename + "(级别：" + Convert.ToInt64(pe) + "）");
                            }
                            break;
                    }
                }
                catch(Exception err)
                {
                    if (e.FromGroup == 490623220)
                    {
                        string[] statemp = err.StackTrace.Split('在');
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(1361778219),
                        "异常描述：" + err.Message +
                        "\n顶异常堆栈：" + statemp[statemp.Length - 1] +
                        "\n导致异常的指令：" + e.Message.Text);
                    }
                    else
                    {
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                        "给予的指令可能错误。\n异常描述：" + err.Message +
                        "\n请联系QQ1361778219取得解决方案");
                    }

                }
                return;
            }
            
            //throw new NotImplementedException();
        }
    }
}
