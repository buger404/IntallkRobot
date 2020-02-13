using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using RestoreData.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArtificalA.Intelligence;
using System.Threading;
using System.Runtime.InteropServices;
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
        private bool FailAI = true;
        private struct HotMsg
        {
            public string msg;
            public int hot;
            public long group;
            public string qq;
            public bool delaymsg;
            public bool hasup;
        }
        private enum PermissionName
        {
            AirPermission = 0, //参观级别的权限
            UserPermission = 1, //正常级别的权限
            SupermanPermission = 2, //可以访问有关群管理的内容
            MasterPermission = 3, //可以访问直接关系到我自己的内容
            HeavenPermission = 4 //可以访问我自己的计算机
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
        private static void Log(string log, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }
        // 接收事件
        [STAThread]
        public void GroupMessage(object sender,CQGroupMessageEventArgs e)
        {
            int sid; double qq = 0;
            List<Native.Csharp.Sdk.Cqp.Model.GroupMemberInfo> mem;
            Random r = new Random(); string ques = "";

            Log("Message:" + e.FromQQ.Id + "," + e.Message.Text,ConsoleColor.Cyan);

            //Artifical Repeater
            bool hothander = false;
            HotMsg hmsg;
            for (int i = 0; i < Manager.Hots.data.Count; i++)
            {
            nexthmsg:
                if (i >= Manager.Hots.data.Count) { break; }
                hmsg = (HotMsg)Manager.Hots.data[i];
                //是否在目标群
                if (hmsg.group == e.FromGroup.Id)
                {
                    if (hmsg.delaymsg) //延迟复读
                    {
                        Log("(" + i + ")Delay repeat:" + hmsg.msg, ConsoleColor.Yellow);
                        e.FromGroup.SendGroupMessage(hmsg.msg);
                        hmsg.delaymsg = ((r.Next(0, 4)) == 3);
                        hmsg.hot = -2;
                        Log("(" + i + ")Conntinue-repeat:" + hmsg.delaymsg, ConsoleColor.Yellow);
                    }
                    if (e.Message.Text != hmsg.msg) { hmsg.hot--;} //如果当前的发言和上句发言不同，上句发言热度-1
                    //如果当前的发言和上句发言一直，上句发言热度+1
                    if (e.Message.Text == hmsg.msg) 
                    {
                        hothander = true;
                        // 不是同一个QQ在刷屏
                        if (hmsg.qq.IndexOf(e.FromQQ.Id.ToString() + ";") < 0)
                        {
                            Log("(" + i + ")Heat:" + hmsg.msg + " , hot :" + hmsg.hot);
                            hmsg.hasup = true; 
                            hmsg.qq = hmsg.qq + e.FromQQ.Id.ToString() + ";";
                            Log("(" + i + ")Members:" + hmsg.qq);
                            hmsg.hot++; 
                        }
                        else
                        {
                            Log("(" + i + ")Boring-repeat:" + e.FromQQ.Id, ConsoleColor.Red);
                        }
                    }
                    //如果发言冷却，移除
                    if (hmsg.hot <= -2) 
                    {
                        if (hmsg.hasup) { Log("(" + i + ")Disapper repeat:" + hmsg.msg, ConsoleColor.Red); }
                        Manager.Hots.data.RemoveAt(i); goto nexthmsg; 
                    } 
                    //发言热度越高，越容易引发复读
                    if (r.Next(0, 100) > 100 - hmsg.hot * 20 * (hmsg.hot / 2))
                    {
                        hmsg.delaymsg = (r.Next(0, 10) > 6); //设置延迟一回合
                        if (!hmsg.delaymsg)
                        {
                            Log("(" + i + ")Repeat:" + hmsg.msg + ",impossible:" + (100 - hmsg.hot * 20 * (hmsg.hot / 2)), ConsoleColor.Yellow);
                            e.FromGroup.SendGroupMessage(hmsg.msg);
                            hmsg.hot = -1; 
                            hothander = true;
                        }
                        else
                        {
                            Log("(" + i + ")Delay set:" + hmsg.msg, ConsoleColor.Yellow);
                        }
                    }
                    if (i >= Manager.Hots.data.Count) { break; }
                    Manager.Hots.data[i] = hmsg;
                }
            }
            //如果当前发言没有被处理，则加入新热点
            if (!hothander)
            {
                hmsg.group = e.FromGroup.Id; hmsg.msg = e.Message.Text; hmsg.hot = 1; hmsg.delaymsg = false;
                hmsg.qq = e.FromQQ.Id.ToString() + ";";
                hmsg.hasup = false;
                Manager.Hots.data.Add(hmsg);
            }

            //Random Topic
            if (e.Message.Text.IndexOf("[CQ:") < 0)
            {
                if ((r.Next(0, 80) == 50) || (FailAI == true))
                {
                    try
                    {
                        FailAI = false;
                        if (r.Next(0, 3) < 2)
                        {
                            e.FromGroup.SendGroupMessage(ArtificalAI.Talk(e.Message.Text, "tieba"));
                        }
                        else
                        {
                            e.FromGroup.SendGroupMessage(ArtificalAI.Talk(e.Message.Text, "baidu"));
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
                pe = (PermissionName)Convert.ToInt64(Manager.CPms.data[sid + 1].ToString());
                pename = GetPermissionName(pe);

                try
                {
                    string[] p = e.Message.Text.Split(' ');
                    if (p.Length < 1) 
                    {
                        e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "关于黑嘴Intallk稽气人\n垃圾臭狗机器人的同义词。\n制作：Error 404\nQQ：1361778219\nPowered by CoolQ");
                        return;
                    }
                    switch (p[1])
                    {
                        case ("msdn"):
                            //msdn searching
                            if (pe < PermissionName.SupermanPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            for (int i = 2; i < p.Length; i++)
                            {
                                ques = ques + p[i] + " ";
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "msdn"));
                            break;
                        case ("csdn"):
                            //csdn searching
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            for (int i = 2; i < p.Length; i++)
                            {
                                ques = ques + p[i] + " ";
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "csdn"));
                            break;
                        case ("wallspaper"):
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
                            TimeSpan ctime = new TimeSpan(Convert.ToInt64(p[3]));
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
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques,"tieba"));
                            break;
                        case ("talk_old"):
                            if (pe < PermissionName.UserPermission) { e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id), "您当前持有的权限'" + pename + "'(级别" + Convert.ToInt64(pe) + ")不足以访问该指令。"); return; }
                            if (p.Length < 2) { throw new Exception("'" + p[1] + "'所给的参数个数不正确"); }
                            for (int i = 2; i < p.Length; i++)
                            {
                                ques = ques + p[i] + " ";
                            }
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id) + "\n" + ArtificalAI.Talk(ques, "baidu"));
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
                            e.FromGroup.SendGroupMessage(CQApi.CQCode_At(e.FromQQ.Id),
                            "'" + p[1] + "'不是有效的指令");
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
