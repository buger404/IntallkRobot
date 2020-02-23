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

namespace io.github.buger404.intallk.Code
{
    public class Event_PrivateMessage:IPrivateMessage 
    {
        public void PrivateMessage(object sender, CQPrivateMessageEventArgs e)
        {
            if (VoidLifes.TargetQQ == e.FromQQ.Id)
            {
                VoidLifes.Solve(e.Message.Text);
            }
            else
            {
                if (e.Message.Text.StartsWith("post\r\n"))
                {
                    string code = e.Message.Text.Replace("post\r\n", "");
                    string state = "";
                    state = VoidLifes.LoadEvent(code);
                    if (state == "")
                    {
                        File.WriteAllText(@"D:\DataArrange\VoidLife\" + e.FromQQ.Id.ToString() + "-" + 
                            DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + 
                            "." + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Second + ".txt", code);
                        e.FromQQ.SendPrivateMessage("congratulations! your post has been used in the game!");
                    }
                    else
                    {
                        e.FromQQ.SendPrivateMessage(state + "\n\ntype 'post help' for help.");
                    }
                }
                if (e.Message.Text.StartsWith("post help"))
                {
                    e.FromQQ.SendPrivateMessage(File.ReadAllText(@"D:\DataArrange\voidlife.example.txt"));
                }

            }

        }
    }
}
