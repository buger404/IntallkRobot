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
            e.CQApi.SetFriendAddRequest(e.ResponseFlag, Native.Csharp.Sdk.Cqp.Enum.CQResponseType.FAIL, "本机拒绝所有好友申请");
        }
    }
}
