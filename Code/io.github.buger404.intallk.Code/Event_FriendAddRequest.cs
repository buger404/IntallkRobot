using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.github.buger404.intallk.Code
{
    public class Event_FriendAddRequest:IFriendAddRequest
    {
        public static int recordtime = 0;
        private static void Log(string log, ConsoleColor color = ConsoleColor.White)
        {
            if (recordtime < DateTime.Now.Hour) { recordtime = DateTime.Now.Hour; log = "##TIMELINE(" + recordtime + ")####################################################\r\n" + log; }
            File.AppendAllText(@"C:\DataArrange\Log\[Friends]-" + MainThread.MessagePoster.logid + ".txt", log + "\r\n");
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }

        public void FriendAddRequest(object sender, CQFriendAddRequestEventArgs e)
        {
            Log("Accepted a new friend.");
            e.CQApi.SetFriendAddRequest(e.ResponseFlag, Native.Csharp.Sdk.Cqp.Enum.CQResponseType.PASS,"机器人用户");
            e.FromQQ.SendPrivateMessage("thanks for making friends with me, my dear.");
        }
    }
    public class Event_FriendAdd : IFriendAdd
    {
        public static int recordtime = 0;
        private static void Log(string log, ConsoleColor color = ConsoleColor.White)
        {
            if (recordtime < DateTime.Now.Hour) { recordtime = DateTime.Now.Hour; log = "##TIMELINE(" + recordtime + ")####################################################\r\n" + log; }
            File.AppendAllText(@"C:\DataArrange\Log\[Friends]-" + MainThread.MessagePoster.logid + ".txt", log + "\r\n");
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }

        public void FriendAdd(object sender, CQFriendAddEventArgs e)
        {
            Log("Accepted a new friend.");
            //e.CQApi.SetFriendAddRequest(e., Native.Csharp.Sdk.Cqp.Enum.CQResponseType.PASS, "机器人用户");
            e.FromQQ.SendPrivateMessage("thanks for making friends with me, my dear.");
        }
    }
}
