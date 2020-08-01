using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;
using io.github.buger404.intallk.Code;

namespace Repeater
{
    public class Repeaters
    {
        [Serializable]
        public struct member
        {
            public long qq;                    //QQ
            public double wcount;                //总发言次数
            public double frcount;               //第一个跟读次数
            public double zfcount;               //发起复读次数
            public double bacount;               //刷屏次数
            public double encount;               //终结次数
            public long group;                 //群
            public member(long qqnum,long groupnum)
            {
                qq = qqnum;group = groupnum;
                wcount = 1;frcount = 0;zfcount = 0;bacount = 0;encount = 0;
            }
        }
        [Serializable]
        public struct memlist
        {
            public List<member> data;
        }
        public static memlist List = new memlist { data = new List<member>()};
        public static void LoadInfo()
        {
            if (!File.Exists(@"C:\DataArrange\messages.json")) {  return; }
            DataContractJsonSerializer r = new DataContractJsonSerializer(typeof(memlist));
            FileStream f = File.Open(@"C:\DataArrange\messages.json", FileMode.Open);
            List = (memlist)r.ReadObject(f);
        }
        public static void SaveInfo()
        {
            try
            {
                DataContractJsonSerializer w = new DataContractJsonSerializer(typeof(memlist));
                FileStream f = File.Create(@"C:\DataArrange\messages.json");
                w.WriteObject(f, List);
                f.Close();
            }
            catch
            {
                Console.WriteLine("msg profile failed !");
            }
            
        }
        public static void AutoSave()
        {
        restart:
            SaveInfo();
            Console.WriteLine("msg profile saved.");
            Thread.Sleep(60000);
            goto restart;
        }
        public static member Information(long qq,long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq, group)); i = List.data.Count - 1; }
            return (List.data[i]);
        }
        public static void SpeakOnce(long qq, long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq,group));i = List.data.Count - 1; }
            member me = List.data[i];
            me.wcount++; List.data[i] = me;
            if (me.wcount == 1) Event_GroupMessage.Achive(qq, "Hello World", Event_GroupMessage.Current);
            if (me.wcount == 1000) Event_GroupMessage.Achive(qq, "聊得火热", Event_GroupMessage.Current);
            if (me.wcount == 10000) Event_GroupMessage.Achive(qq, "水群大湿", Event_GroupMessage.Current);
            if (me.wcount == 100000) Event_GroupMessage.Achive(qq, "远古居民", Event_GroupMessage.Current);
        }
        public static void FirstRepeat(long qq, long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq, group)); i = List.data.Count - 1; }
            member me = List.data[i];
            me.frcount++; List.data[i] = me;
            if (me.wcount == 100) Event_GroupMessage.Achive(qq, "最受欢迎群员", Event_GroupMessage.Current);
            if (me.wcount == 233) Event_GroupMessage.Achive(qq, "鲁迅", Event_GroupMessage.Current);
            if (me.wcount == 666) Event_GroupMessage.Achive(qq, "精髓", Event_GroupMessage.Current);
        }
        public static void ZeroRepeat(long qq, long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq, group)); i = List.data.Count - 1; }
            member me = List.data[i];
            me.zfcount++; List.data[i] = me;
            if (me.wcount == 200) Event_GroupMessage.Achive(qq, "鼓舞者", Event_GroupMessage.Current);
            if (me.wcount == 400) Event_GroupMessage.Achive(qq, "热度风暴", Event_GroupMessage.Current);
        }
        public static void BoringRepeat(long qq, long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq, group)); i = List.data.Count - 1; }
            member me = List.data[i];
            me.bacount++; List.data[i] = me;
            if (me.wcount == 5) Event_GroupMessage.Achive(qq, "刷屏带师", Event_GroupMessage.Current);
            if (me.wcount == 10) Event_GroupMessage.Achive(qq, "垃圾制造者", Event_GroupMessage.Current);
            if (me.wcount == 20) Event_GroupMessage.Achive(qq, "无语", Event_GroupMessage.Current);
        }
        public static void EndRepeat(long qq, long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq, group)); i = List.data.Count - 1; }
            member me = List.data[i];
            me.encount++; List.data[i] = me;
            if (me.wcount == 10) Event_GroupMessage.Achive(qq, "寒冷气息", Event_GroupMessage.Current);
            if (me.wcount == 30) Event_GroupMessage.Achive(qq, "绝对零度", Event_GroupMessage.Current);
            if (me.wcount == 60) Event_GroupMessage.Achive(qq, "-273.15℃", Event_GroupMessage.Current);
        }

    }
}
