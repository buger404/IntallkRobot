using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace RestoreData.Manager
{
    using io.github.buger404.intallk.Code;
    public class Manager
    {
        public static DataArrange LCards;
        public static DataArrange CPms;
        public static DataArrange Hots;
    }
}

namespace io.github.buger404.intallk.Code
{
    public class DataArrange
    {
        public ArrayList data = new ArrayList();
        public string name;

        public DataArrange()
        {
            MessageBox.Show("未能成功触发构造函数");
        }

        public DataArrange(string dataname)
        {
            this.name = dataname;
            if (Directory.Exists(@"D:\DataArrange\") == false) { Directory.CreateDirectory(@"D:\DataArrange\"); }
            this.ReadData();
        }

        public int SearchFor(object o)
        {
            string t = o.ToString();
            for (int i = 0; i < this.data.Count; i++)
            {
                if (this.data[i].ToString() == t) { return i; }
            }
            return -1;
        }

        public void SaveData()
        {
            string content = "";
            for (int i = 0; i < this.data.Count; i++)
            {
                content = content + this.data[i].ToString() + "`";
            }
            
            File.WriteAllText("D:\\DataArrange\\" + this.name + ".bin", content);
        }

        public void ReadData()
        {
            if (File.Exists("D:\\DataArrange\\" + this.name + ".bin") == false) { return; }
            string[] item = File.ReadAllText("D:\\DataArrange\\" + this.name + ".bin", Encoding.UTF8).Split('`');
            for (int i = 0; i < item.Length; i++)
            {
                if (item[i] != "")
                {
                    this.data.Add(item[i]);
                }
            }
        }

        
    }
}
