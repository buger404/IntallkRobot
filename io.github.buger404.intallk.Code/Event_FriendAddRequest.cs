using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.github.buger404.intallk.Code
{
    public class Event_FriendAddRequest:IFriendAddRequest
    {
        public void FriendAddRequest(object sender, CQFriendAddRequestEventArgs e)
        {
            Console.WriteLine("Accepted a new friend.");
            e.CQApi.SetFriendAddRequest(e.ResponseFlag, Native.Csharp.Sdk.Cqp.Enum.CQResponseType.PASS,"机器人用户");
            e.FromQQ.SendPrivateMessage("thanks for making friends with me, my dear.");
        }
    }
    public class Event_FriendAdd : IFriendAdd
    {
        public void FriendAdd(object sender, CQFriendAddEventArgs e)
        {
            Console.WriteLine("Accepted a new friend.");
            //e.CQApi.SetFriendAddRequest(e., Native.Csharp.Sdk.Cqp.Enum.CQResponseType.PASS, "机器人用户");
            e.FromQQ.SendPrivateMessage("thanks for making friends with me, my dear.");
        }
    }
}
