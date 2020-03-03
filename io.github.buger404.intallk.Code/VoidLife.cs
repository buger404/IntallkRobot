using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using Native.Csharp.Sdk.Cqp.Model;
using System.IO;

namespace VoidLife.Simulator
{
    public class VoidLifes
    {
        public static CQApi pCQ;                //发消息工具

        public struct LifeChoice                //人生事件选项
        {
            public string Name;                 //选项标题
            public string Description;          //选项引发的事件描述
            public long Money;                  //选项财产加成
            public long Live;                   //选项寿命加成
            public long Spirit;                 //选项情绪加成
        }
        public struct LifeEvent                 //人生事件
        {
            public string Tag;                  //事件标签
            public string Description;          //事件描述
            //事件选项
            public List<LifeChoice> Choices;
        }

        //所有事件
        public static List<LifeEvent> Events = new List<LifeEvent>(); 

        //角色参数
        //随机数
        public static Random ran = new Random(Guid.NewGuid().GetHashCode());        
        public static string VoidName = "";             //角色名称
        public static int WinState = 0;                 //输赢指示
        public static long VoidLive = 100;              //角色寿命
        public static long TimeZone = 0;                //角色余下寿命
        public static long VoidMoney = 1000;            //角色财产
        public static long VoidSpirit = 50;             //角色情绪
        public static long TargetQQ = 0;                //目标对话QQ
        public static long TargetGroup = 0;             //目标群
        public static long OwnerQQ = 0;                 //发起者QQ
        public static LifeEvent CurrentEvent;           //当前事件
        //标签问题
        public static List<string> TagQues = new List<string>();
        //标签
        public static List<int> TagList = new List<int>();

        //玩家数据
        public struct Player
        {
            public bool IsMaker;                        //是否为制造者
            public long QQ;                             //QQ号
            public long Round;                          //剩余回合数
            //根据QQ号构造结构体
            public Player(long q) { IsMaker = false; QQ = q; Round = 0; }
            //重置回合
            public Player Reset() { Round = 1; return this; }             
            public void Next() { Round--; }             //回合终止
            public Player Turn()                        //回合开始
            {
                Next();TargetQQ = QQ;
                CreateLife();
                return this;
            }
        }

        //所有玩家
        public static List<Player> Players = new List<Player>();
        public static int recordtime = 0;
        private static void Log(string log, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }

        public static void JudgeWin()
        {
            VoidSpirit += (50 - VoidSpirit) / 10;
            if (VoidMoney > 10) { VoidMoney -= 10; }
            if (VoidSpirit >= 70 && VoidSpirit < 100) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'心情很好，做起事来都非常顺心，寿命增加了。"); VoidLive += 5; }
            if (VoidSpirit >= 100 && VoidSpirit < 150) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'心情好到了极点，这促使他的寿命增加了。"); VoidLive += 10; }
            if (VoidSpirit > 150) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'心情好到有点过分了，最后还是被送进了精神病院，还花了不少钱。"); VoidMoney -= 500; }

            if (VoidSpirit >= -10 && VoidSpirit < 10) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'心情十分低落，没有食欲，这导致了它的寿命减少了。"); VoidLive -= 5; }
            if (VoidSpirit >= -50 && VoidSpirit < -10) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'心情坏到了极点，它因此作出了自残的行为，寿命减少了。"); VoidLive -= 10; }
            if (VoidSpirit < -50) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'对生活已经失去了信心，采取各种行动尝试离开这个世界。"); VoidLive -= 20; }

            if (VoidMoney >= -10 && VoidMoney < 10) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'已经没有钱吃饱饭了，寿命减少了。"); VoidLive -= 1; VoidSpirit -= 10; }
            if (VoidMoney >= -1000 && VoidMoney < -10) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'根本没有钱吃饭了，在街上到处讨饭，捡垃圾吃，寿命减少了。"); VoidLive -= 5; VoidSpirit -= 20; }
            if (VoidMoney < -1000) { GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'欠下了一大笔债，一边要忍受饥饿一边讨债。寿命减少了。"); VoidLive -= 7; VoidSpirit -= 30; }
            if (VoidMoney > 1000 && ran.Next(0,3) == 2) {int m = ran.Next((int)(VoidMoney * 0.2),(int)(VoidMoney * 0.8));VoidMoney -= m; GruMsg(TargetGroup, "虚拟角色'" + VoidName + "'花了" + m + "元，买到了自己心仪的东西，非常开心。"); VoidSpirit += (m / 50); }
            if (VoidSpirit > 200) { VoidSpirit = 200; }
            if (VoidSpirit < -100) { VoidSpirit = -100; }

            WinState = (TimeZone > VoidLive ? (VoidLive >= 70 ? 1 : 2) : 0);
            if (WinState == 0) { TimeZone += 10; }
            bool MakerWin = (WinState == 1);
            if (WinState > 0)
            {
                string winner = "";
                for (int i = 0; i < Players.Count; i++)
                {
                    PriMsg(Players[i].QQ, "这次游戏，" +
                                    (MakerWin ? "虚拟角色'" + VoidName + "'活到了平均寿命" : "虚拟角色没有活到平均寿命") +
                                    "，因此，身为" + (Players[i].IsMaker ? "制造者" : "破坏者") + "的你" + 
                                    (Players[i].IsMaker == MakerWin ? "胜利了！" : "失败了"));
                    winner = winner + (Players[i].IsMaker == MakerWin ? CQApi.CQCode_At(Players[i].QQ).ToString() : "");
                }
                GruMsg(TargetGroup, "the character died at the age of " + TimeZone + ".\nthis game , " + (MakerWin ? "'制造者'" : "'破坏者'") + " won! so winner(s) are " + winner + "! congratulations!\nthe game closed automatically.");
                GruMsg(TargetGroup, "WELCOME to post new EVENTS , please send 'post help' to me privatly for details.");
                EndGame(); return;
            }

            string DataC = "虚拟角色'" + VoidName + "' 持有金额:" + VoidMoney + "元，心情指数：" + VoidSpirit + "，剩余寿命：" + (VoidLive - TimeZone) + "年（年龄：" + TimeZone + "）";
            PriMsg(TargetQQ, DataC); GruMsg(TargetGroup, DataC);
            Log(DataC, ConsoleColor.Yellow);
        }

        public static void Solve(string msg)
        {
            int c = -1;
            if (TagQues.Count == 0)
            {
                for(int i = 0;i< CurrentEvent.Choices.Count;i++)
                {
                    if (msg.ToUpper() == (new string(new char[] { (char)('A' + i) }))) { c = i; break; }
                }
                if (c == -1) { PriMsg(TargetQQ, "游戏还在进行。\n请回复指定的选项序号，例如“A”。"); return; }

                VoidLive += CurrentEvent.Choices[c].Live;
                VoidMoney += CurrentEvent.Choices[c].Money;
                VoidSpirit += CurrentEvent.Choices[c].Spirit;
                Log(CurrentEvent.Choices[c].Description);
                PriMsg(TargetQQ, CurrentEvent.Choices[c].Description.Replace("<void>","'" + VoidName + "'"));
                GruMsg(TargetGroup, "目标玩家选择了'" + CurrentEvent.Choices[c].Name + "'\n" + CurrentEvent.Choices[c].Description.Replace("<void>", "'" + VoidName + "'") + "\n" + "\n正在等待目标玩家选取新标签...");
                JudgeWin();
                if (WinState != 0) { return; }
                WriteTagQues(); string DataC = "";
                for (int i = 0; i < TagQues.Count; i++)
                    DataC = DataC + (new string(new char[] { (char)('A' + i) })) + "." + TagQues[i] + "   ";

                PriMsg(TargetQQ, DataC + "\n请为虚拟角色'" + VoidName + "'贴上一个新标签。");

            }
            else
            {
                for (int i = 0; i < TagQues.Count; i++)
                {
                    if (msg.ToUpper() == (new string(new char[] { (char)('A' + i) }))) { c = i; break; }
                }
                if (c == -1) { PriMsg(TargetQQ, "游戏还在进行。\n请回复指定的选项序号，例如“A”。"); return; }

                NewTag(TagQues[c]);
                PriMsg(TargetQQ, "谢谢，请继续关注群内消息。");
                GruMsg(TargetGroup, "目标玩家选取新标签完毕。");
                TagQues.Clear();
                NextRound();
            }

        }

        public static string LoadEvent(string code)
        {
            try
            {
                string[] lines = code.Split(new char[] { '\r', '\n' }, 
                                            StringSplitOptions.RemoveEmptyEntries);
                LifeEvent events = new LifeEvent();
                events.Description = "";events.Tag = "";
                if (!lines[0].StartsWith("#")) { throw new Exception("the tag is not defined, at line 1"); }
                events.Tag = lines[0].Remove(0,1);
                if (events.Tag.Trim() == "") { throw new Exception("the tag is full of space, at line 1"); }
                events.Choices = new List<LifeChoice>();
                bool InBlock = false; string InnerText = ""; string DataLine = "";string ChoiceLine = "";
                for (int i = 1; i < lines.Length; i++)
                {
                    switch (lines[i]) 
                    { 
                        case("{"):
                            if (InBlock) { throw new Exception("last arrange is not closed, at line " + (i+1)); }
                            InBlock = true;
                            break;
                        case("}"):
                            if (!InBlock) { throw new Exception("not arrange is opened, at line " + (i + 1)); }
                            if (InnerText == "") { throw new Exception("empty arrange, at line " + (i + 1)); }
                            InnerText = InnerText.Remove(InnerText.Length - 1);
                            if (InnerText.Trim() == "") { throw new Exception("empty arrange data, at line " + (i + 1)); }
                            InBlock = false;
                            if (events.Description == ""){ events.Description = InnerText;}
                            else
                            {
                                if (DataLine == "") { throw new Exception("empty data changes." + "\n  -wrong script:" + DataLine); }
                                LifeChoice lc = new LifeChoice();
                                if (ChoiceLine == "") { throw new Exception("empty choice name." + "\n  -wrong script:" + ChoiceLine); }
                                if (!ChoiceLine.StartsWith("#")) { throw new Exception("the choice name is not defined." + "\n  -wrong script:" + ChoiceLine); }
                                lc.Name = ChoiceLine.Remove(0, 1);
                                if (lc.Name.Trim() == "") { throw new Exception("the choice name is full of space." + "\n  -wrong script:" + ChoiceLine); }
                                string[] s = DataLine.Split(';');
                                if (s.Length != 4) { throw new Exception("wrong data changes, out of index." + "\n  -wrong script:" + DataLine + "(" + s.Length + ")"); }
                                for (int j = 0; j < 3; j++)
                                {
                                    if (s[j] == "") { throw new Exception("wrong data changes, no data." + "\n  -wrong script:" + s[j]); }
                                    try
                                    {
                                        switch (s[j][0])
                                        {
                                            case ('$'):
                                                s[j] = s[j].Remove(0, 1);
                                                lc.Money = Convert.ToInt64(s[j]);
                                                break;
                                            case ('@'):
                                                s[j] = s[j].Remove(0, 1);
                                                lc.Live = Convert.ToInt64(s[j]);
                                                break;
                                            case ('*'):
                                                s[j] = s[j].Remove(0, 1);
                                                lc.Spirit = Convert.ToInt64(s[j]);
                                                break;
                                            default:
                                                throw new Exception("unknown data." + "\n  -wrong script:" + s[j]);
                                                break;
                                        }
                                    }
                                    catch
                                    {
                                        throw new Exception("wrong choice change data.\n  -wrong script:" + s[j]);
                                    }
                                }
                                lc.Description = InnerText;
                                events.Choices.Add(lc);
                            }
                            InnerText = "";ChoiceLine = "";DataLine = "";
                            break;
                        default:
                            if (events.Description != "" && ChoiceLine == "") { ChoiceLine = lines[i]; }
                            else
                            {
                                if (events.Description != "" && DataLine == "") { DataLine = lines[i]; }
                                else { InnerText += (lines[i] + "\n"); }
                            }
                            break;
                    }
                }
                if (events.Description == "") { throw new Exception("empty description."); }
                if (events.Choices.Count < 3) { throw new Exception("there must be at least 3 choices."); }
                Events.Add(events);
                Log("Loaded life: " + events.Tag + ", " + events.Choices.Count + " choices", ConsoleColor.Green);
            }
            catch(Exception e)
            {
                Log("Loading life error:" + e.Message , ConsoleColor.Red);
                return e.Message;
            }
            return "";
        }

        public static void LoadGame()
        {
            foreach (string file in Directory.GetFiles(@"C:\DataArrange\VoidLife\"))
                LoadEvent(File.ReadAllText(file));
        }

        public static bool IsPlaying()
        {
            return (TargetQQ != 0);
        }

        public static bool IsStart()
        {
            return (Players.Count != 0);
        }

        public static bool IsJoined(long QQ)
        {
            return (Players.FindIndex(m => m.QQ == QQ) != -1);
        }

        public static void JoinGame(long QQ)
        {
            Log(QQ + " joined the game", ConsoleColor.Green);
            Players.Add(new Player(QQ));
        }

        public static bool IsNextRound()
        {
            return (Players.FindIndex(m => m.Round > 0) == -1);
        }

        public static void EndGame()
        {
            TargetQQ = 0; TargetGroup = 0;
            Players.Clear(); TagList.Clear(); TagQues.Clear();
        }

        public static void BeginGame(string chaname)
        {
            TargetQQ = 0; WinState = 0;
            NewTag(Events[ran.Next(0, Events.Count)].Tag);
            TimeZone = 0; VoidLive = 100; VoidMoney = 1000;
            VoidName = chaname; VoidSpirit = 50;
            Log("Game begins !", ConsoleColor.Green);
        }

        public static void NewTag(string Tag)
        {
            int i = Events.FindIndex(m => m.Tag == Tag);
            if (i != -1) { TagList.Add(i); Log("new tag avaliable: " + Events[i].Tag); }
        }

        public static void WriteTagQues()
        {
            for (int i = 1; i <= 4; i++)
                TagQues.Add(Events[ran.Next(0, Events.Count)].Tag);
        }

        public static void CreateLife()
        {
            CurrentEvent = Events[TagList[ran.Next(0, TagList.Count)]];
        }

        public static void StartRound()
        {
            int nC = (int)Math.Ceiling(Players.Count / 2.0); int neC = 0;
            do
            {
            rechoose:
                int i = ran.Next(0, Players.Count);
                if (Players[i].IsMaker) { goto rechoose; }
                Player p = Players[i]; p.IsMaker = true; Players[i] = p;
                Log(p.QQ + " was choosen to be the maker.", ConsoleColor.Green);
                PriMsg(p.QQ, "**恭喜，您被选中成为了该轮虚拟角色'" + VoidName + "'的制造者，请照顾好您的角色，让他活过平均寿命。");
                nC++;
            } while (nC < neC);
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].IsMaker == false)
                {
                    PriMsg(Players[i].QQ, "**您被选中成为了该轮虚拟角色'" + VoidName + "'的破坏者，您需要想方设法让他不能活过平均寿命。");
                }
            }
            NextRound();
        }

        public static void NextRound()
        {
            if (IsNextRound())
            {
                Log("next round !", ConsoleColor.Yellow);
                for (int i = 0; i < Players.Count; i++)
                    Players[i] = Players[i].Reset();
            }
            List<Player> p = Players.FindAll(m => m.Round > 0); int s = ran.Next(0, p.Count);
            Log("in round players: " + p.Count + ", can next:" + IsNextRound(), ConsoleColor.Yellow);
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].QQ == p[s].QQ) { Players[i] = p[s].Turn(); }
            }

            Log("age " + TimeZone + " , live " + VoidLive);
            Log("new event:\n" + CurrentEvent.Description);
            PriMsg(TargetQQ, CurrentEvent.Description.Replace("<void>","'" + VoidName + "'"));
            string chostr = "";
            for (int i = 0; i < CurrentEvent.Choices.Count;i++ )
                chostr = chostr + (new string(new char[] { (char)('A' + i) })) + ". " + CurrentEvent.Choices[i].Name + "\n";

            PriMsg(TargetQQ, chostr + "您得到了机会！请为本次的虚拟角色'" + VoidName + "'做出决定。\n您需要添加我的QQ，本机才能受到您的回复噢。");
            GruMsg(TargetGroup, CurrentEvent.Description.Replace("<void>", "'" + VoidName + "'") + "\n\n" + "正在等待目标玩家做出选择...");
        }

        public static void PriMsg(long qq,string msg)
        {
            new QQ(pCQ, qq).SendPrivateMessage(msg);
        }
        public static void GruMsg(long group, string msg)
        {
            new Group(pCQ, group).SendGroupMessage(msg);
        }
    }
}
