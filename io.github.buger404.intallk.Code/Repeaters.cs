using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Threading;

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
            public long group;                 //群
            public member(long qqnum,long groupnum)
            {
                qq = qqnum;group = groupnum;
                wcount = 0;frcount = 0;zfcount = 0;bacount = 0;
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
        }
        public static void FirstRepeat(long qq, long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq, group)); i = List.data.Count - 1; }
            member me = List.data[i];
            me.frcount++; List.data[i] = me;
        }
        public static void ZeroRepeat(long qq, long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq, group)); i = List.data.Count - 1; }
            member me = List.data[i];
            me.zfcount++; List.data[i] = me;
        }
        public static void BoringRepeat(long qq, long group)
        {
            int i = List.data.FindIndex(m => m.qq == qq && m.group == group);
            if (i == -1) { List.data.Add(new member(qq, group)); i = List.data.Count - 1; }
            member me = List.data[i];
            me.bacount++; List.data[i] = me;
        }
    }
}
