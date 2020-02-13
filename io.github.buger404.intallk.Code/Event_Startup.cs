using Native.Csharp.Sdk.Cqp;
using Native.Csharp.Sdk.Cqp.EventArgs;
using Native.Csharp.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestoreData.Manager;
using System.Runtime.InteropServices;

namespace io.github.buger404.intallk.Code
{
    public class Event_Startup:ICQStartup
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        // 接收事件
        public void CQStartup(object sender, CQStartupEventArgs e)
        {
            ArtificalA.Intelligence.ArtificalAI.DebugLog = true;
            AllocConsole();
            Manager.LCards = new DataArrange("lockcards");
            Manager.CPms = new DataArrange("consolepermissions");
            Manager.Hots = new DataArrange("messagehots");
        }
    }
}
