using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using VoidLife.Simulator;
using RestoreData.Manager;
using System.IO;
using DataArrange.Storages;
using ArtificalA.Intelligence;
using MainThread;
using System.Runtime.InteropServices;
using Native.Csharp.Sdk.Cqp.Model;

namespace io.github.buger404.intallk.Code
{
    public class Event_PrivateMessage:IPrivateMessage 
    {
        [DllImport("kernel32.dll",
            CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();
        public static int recordtime = 0;
        private static void Log(string log, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }
        private struct Person
        {
            public long UnHanded;
            public long QQ;
            public long Time;
            public Person(long qq)
            {
                UnHanded = 0;QQ = qq;Time = 0;
            }
        }
        private static bool CanMatch(string target, params string[] matches)
        {
            bool b = false;
            for (int i = 0; i < matches.Length; i++)
            {
                b = (b || (target.IndexOf(matches[i]) >= 0));
            }
            return b;
        }
        private List<Person> pe = new List<Person>();
        public void PrivateMessage(object sender, CQPrivateMessageEventArgs e)
        {
            return;
            try
            {
                Log("(" + e.FromQQ.Id + ")Private: " + e.Message.Text, ConsoleColor.Green);
                Storage sys = new Storage("system");
                if (sys.getkey("root", "sleep") == "zzz")
                {
                    e.FromQQ.SendPrivateMessage(
                            "zzzzz 黑嘴在睡觉觉~");
                    return;
                }
                Random r = new Random(Guid.NewGuid().GetHashCode());
                bool noVoice = false;
                int i = pe.FindIndex(m => m.QQ == e.FromQQ.Id),t;
                if (i == -1)
                {
                    pe.Add(new Person(e.FromQQ.Id));
                    i = pe.FindIndex(m => m.QQ == e.FromQQ.Id);
                }
                Person p = pe[i];
                string reply = "";
                string greeting = ""; int hour = DateTime.Now.Hour;
                if (hour >= 6 && hour < 11) greeting = "早上";
                if (hour >= 11 && hour < 13) greeting = "中午";
                if (hour >= 13 && hour < 17) greeting = "下午";
                if (hour >= 17 && hour < 24) greeting = "晚上";
                if (hour >= 0 && hour < 6) greeting = "凌晨。。。。。。？";
                MessagePoster.CheckProcessMsg(e.Message.Text, e.FromQQ.Id, 1);
                if (GetTickCount() - p.Time >= 300000)
                {
                    t = r.Next(0, 5);
                    if (t == 0) reply += "在的哦，请问找黑嘴有什么事呢？(*￣3￣)╭。\n";
                    if (t == 1) reply += greeting + "好呀，黑嘴在哦。ヾ(•ω•`)o。\n";
                    if (t == 2) reply += "我在啦~。\n";
                    if (t == 3) reply += greeting + "好，亲爱的，黑嘴一直在这里哦。o(￣▽￣)ｄ。\n";
                    if (t == 4) reply += "(╹ڡ╹ )黑嘴来啦，" + greeting + "好啊~。\n";
                    p.UnHanded = 0;
                }
                if (e.Message.Text.StartsWith("#tell") && e.FromQQ.Id == 1361778219)
                {
                    string[] te = e.Message.Text.Split(' ');
                    new QQ(e.CQApi, pe[int.Parse(te[1])].QQ).SendPrivateMessage($"(。・・)ノ主人说。。。\n{te[2]}");
                    e.FromQQ.SendPrivateMessage("好嘞好嘞~(๐॔˃̶ᗜ˂̶๐॓)");
                    return;
                }
                if (e.Message.Text.StartsWith("#thanks") && e.FromQQ.Id == 1361778219)
                {
                    t = r.Next(0, 5);
                    if (t == 0) reply = "嗯嗯，不客气哦(^人^)。\n";
                    if (t == 1) reply = "不用谢，╰(￣ω￣ｏ)下次再找黑嘴玩吧。\n";
                    if (t == 2) reply = "you're welcome(ง •_•)ง。\n";
                    if (t == 3) reply = "不用谢啦(*/ω＼*)，下次再找黑嘴玩吧~\n";
                    if (t == 4) reply = "举手之劳，(✿◠‿◠)不用谢啦~\n";
                    string[] te = e.Message.Text.Split(' ');
                    new QQ(e.CQApi, pe[int.Parse(te[1])].QQ).SendPrivateMessage(reply);
                    e.FromQQ.SendPrivateMessage("好嘞好嘞~(๐॔˃̶ᗜ˂̶๐॓)");
                    return;
                }
                foreach (MessagePoster.flowlibrary fl in MessagePoster.flibrary)
                {
                    foreach (string c in fl.lib)
                    {
                        if (e.Message.Text.IndexOf(c) >= 0 && c != "")
                        {
                            if (fl.name == "色情")
                            {
                                t = r.Next(0, 5);
                                if (t == 0) reply += "你为什么要和黑嘴聊色色的东西！\n";
                                if (t == 1) reply += "黑嘴承受不住哇，不许开车。\n";
                                if (t == 2) reply += "黑嘴表示疑车无据呢。\n";
                                if (t == 3) reply += "黑嘴黑嘴表示不希望你开车车。\n";
                                if (t == 4) reply += "不要和黑嘴聊色色的东西嘛！\n";
                            }
                            else
                            {
                                t = r.Next(0, 5);
                                if (t == 0) reply += "黑嘴不知道该怎么说。\n";
                                if (t == 1) reply += "黑嘴觉得你换个话题比较好哦。\n";
                                if (t == 2) reply += "黑嘴对你说的话表示很困扰欸。\n";
                                if (t == 3) reply += "换个话题可以嘛。\n";
                                if (t == 4) reply += "黑嘴黑嘴觉得这个话题不好。\n";
                            }
                            if (p.UnHanded >= 3) 
                            { 
                                reply += "当然你说的这句话黑嘴不会告诉主人啦。\n";
                                new QQ(e.CQApi, 1361778219).SendPrivateMessage($"({i})QQ{e.FromQQ.Id}居然(。﹏。)：\n涉嫌{fl.name}");
                            }
                            MessagePoster.LetSay(reply, e.FromQQ.Id, 1, r.Next(0, 3) == 1, true);
                            p.UnHanded++; p.Time = GetTickCount(); pe[i] = p; e.Handler = true;
                            return;
                        }
                    }
                }
                if (p.UnHanded > 2)
                {
                    new QQ(e.CQApi, 1361778219).SendPrivateMessage($"({i})QQ{e.FromQQ.Id}说(。﹏。)：\n{e.Message.Text}");
                    return;
                }
                if (CanMatch(e.Message.Text, "帮助","怎么用","怎么玩","功能","有什么","可以","说明"))
                {
                    t = r.Next(0, 5);
                    if (t == 0) reply += "(๐॔˃̶ᗜ˂̶๐॓)在群里发送“.help”就可以知道黑嘴可以干嘛了哦~\n";
                    if (t == 1) reply += "(￣▽￣)在群里发送“.help”就可以啦~\n";
                    if (t == 2) reply += "在群里发“.help”就行啦(╹ڡ╹ )\n";
                    if (t == 3) reply += "嗯嗯，可以在群里发“.help”哦~\n";
                    if (t == 4) reply += "(～￣▽￣)～在群里发“.help”呀~\n";
                }
                if (CanMatch(e.Message.Text, "举报", "投诉"))
                {
                    t = r.Next(0, 5);
                    if (t == 0) reply += "？本黑嘴做错什么啦，不满可以删好友啊。\n";
                    if (t == 1) reply += "啥啊，黑嘴又没做错什么，你把我删掉嘛！\n";
                    if (t == 2) reply += "哼，黑嘴生气了，你把我删掉不就好了吗！\n";
                    if (t == 3) reply += "不高兴的话就把黑嘴删掉哇！\n";
                    if (t == 4) reply += "黑嘴黑嘴表示你把黑嘴删掉就完事了。\n";
                }
                if (CanMatch(e.Message.Text, "谢", "3q","than","thx"))
                {
                    t = r.Next(0, 5);
                    if (t == 0) reply += "嗯嗯，不客气哦(^人^)。\n";
                    if (t == 1) reply += "不用谢，╰(￣ω￣ｏ)下次再找黑嘴玩吧。\n";
                    if (t == 2) reply += "you're welcome(ง •_•)ง。\n";
                    if (t == 3) reply += "不用谢啦(*/ω＼*)，下次再找黑嘴玩吧~\n";
                    if (t == 4) reply += "举手之劳，(✿◠‿◠)不用谢啦~\n";
                }
                if (CanMatch(e.Message.Text, "封禁","解封","封杀","封锁","解锁"))
                {
                    t = r.Next(0, 5);
                    if (t == 0) reply += "这个问题建议您咨询一下我的主人哦。\n";
                    if (t == 1) reply += "(￣y▽,￣)╭ 嘛，这个嘛，你得问问我的主人呀。\n";
                    if (t == 2) reply += "嗯，可以咨询我的主人看看的。(╹ڡ╹ )\n";
                    if (t == 3) reply += "我的主人或许可以帮助你解决这个问题哦。\n";
                    if (t == 4) reply += "(○｀ 3′○)试着问问我主人嘛！\n";
                }
                if (CanMatch(e.Message.Text, "为什么","怎么","为啥","为何","如何"))
                {
                    t = r.Next(0, 5);
                    if (t == 0) reply += "嗯。黑嘴也不太懂呢，你可以在群里发“.help”看看呢( •̀ ω •́ )✧\n";
                    if (t == 1) reply += "试试在群里发送“.help”怎样~\n";
                    if (t == 2) reply += "你是说指令的问题嘛，(╹ڡ╹ )可以在群里发“.help”呀\n";
                    if (t == 3) reply += "嗯嗯，哦哦，黑嘴也不太懂呢。试试在群里发“.help”？\n";
                    if (t == 4) reply += "嗯。黑嘴似乎不好解决你的问题，要不在群里发“.help”试试嘛(❁´◡`❁)？\n";
                    string guidence = "";
                    t = r.Next(0, 5);
                    if (t == 0) guidence = "还有还有\n指令使用的方法也很重要哦`(*>﹏<*)′。\n";
                    if (t == 1) guidence = "φ(゜▽゜*)♪黑嘴顺便和你讲讲指令的用法吧~\n";
                    if (t == 2) guidence = "(‾◡◝)黑嘴顺便给你科普一下指令的用法吧~\n不然黑嘴真的超无聊的说。\n";
                    if (t == 3) guidence = "(☆▽☆)黑嘴来当老师啦\n黑嘴这节课给你讲讲指令的用法，汪呜~！\n";
                    if (t == 4) guidence = "(❁´◡`❁)黑嘴黑嘴想问你哦\n你知道黑嘴的指令怎么用嘛\n其实挺简单哦~(*^▽^*)\n";
                    reply = reply + guidence + "说明书上面用<>符号包括的内容，表示必须要输入哦(。・・)ノ。\n" +
                        "说明书上用[]符号包括的内容，意思就是不想填也可以呢。\n" +
                        "还有还有~，/符号表示前后两个东西可以选择一个填写哦，\n" +
                        "对啦，最后注意指令里两个单词之间有空格的哦つ﹏⊂~，\n" +
                        "还有一些指令可能您用不了，因为您的机器人权限不足够呢。(￣_￣|||)\n" +
                        "还有还有哦(⊙o⊙)，艾特必须用标注的艾特哦。\n";
                    noVoice = true;
                }
                if (p.UnHanded == 2)
                {
                    t = r.Next(0, 5);
                    if (t == 0) reply = "(+_+)?嗯。。。黑嘴听不懂啦，我帮你转告给主人大人吧\n";
                    if (t == 1) reply = "抱歉，黑嘴一直在听你说话，但是听不懂。(。﹏。)我帮你转告给主任大人吧~\n";
                    if (t == 2) reply = "(ˉ▽ˉ；)黑嘴听不懂，嘤嘤嘤，帮你转告主人大人~\n";
                    if (t == 3) reply = "⊙﹏⊙∥黑嘴不明白你的意思哦，我帮你转告主人吧~\n";
                    if (t == 4) reply = "(◎﹏◎)呜呜，黑嘴听不懂，帮你转告主人大人~\n";
                    new QQ(e.CQApi, 1361778219).SendPrivateMessage("QQ " + e.FromQQ.Id + " 不知道在和人家说什么，人家转告给你啦(＠_＠;)。");
                    MessagePoster.LetSay(reply, e.FromQQ.Id, 1, r.Next(0, 3) == 1, true);
                    p.UnHanded++; p.Time = GetTickCount(); pe[i] = p;e.Handler = true;
                    return;
                }
                if (reply != "") { MessagePoster.LetSay(reply,e.FromQQ.Id,1,r.Next(0,4) == 1 && noVoice == false,true); p.UnHanded = 0; }
                if (reply == "") p.UnHanded++; 
                p.Time = GetTickCount(); pe[i] = p;
                e.Handler = true;
            }
            catch(Exception ex)
            {
                Log(ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException, ConsoleColor.Red);
            }
 

        }
    }
}
