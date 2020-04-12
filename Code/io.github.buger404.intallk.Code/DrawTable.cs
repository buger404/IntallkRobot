using MainThread;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace io.github.buger404.intallk.Code
{
    public class DrawTable
    {
        public struct tabs
        {
            public bool IsBold;
            public string text;
            public Color color;
            public Color bgcolor;
            public tabs(string content,Color tcolor,Color bcolor,bool Bold = false)
            {
                text = content;IsBold = Bold;color = tcolor;bgcolor = bcolor;
            }
        }
        public static void ExportTable(string title,int width,float[] tab,List<tabs> data)
        {
            string code = 
                "load," + width + "," + (data.Count / tab.Length * 30 + 120) + ",255-255-255-255\r\n";
            code += "write," + width / 2 + ",32," + title + ",255-0-0-0,1,18,1\r\n";
            int row,x,w;
            for(int i = 0;i < data.Count; i+=tab.Length)
            {
                if (i + tab.Length - 1 >= data.Count) { break; }
                row = (i + 1) / tab.Length + 3;
                x = 40;
                for(int s = 0;s < tab.Length; s++)
                {
                    w = (int)((width - 80) * tab[s]);
                    code += "rect," + (x + 1) + "," + row * 30 +
                            "," + (w + 1) + ",30," +
                            data[i + s].bgcolor.A + "-" + data[i + s].bgcolor.R + "-" + data[i + s].bgcolor.G + "-" + data[i + s].bgcolor.B
                            + "\r\n";
                    code += "rectl," + (x + 1) + "," + row * 30 + 
                            "," + (w + 1) + ",30,255-232-232-232\r\n";
                    code += "write," + (x + w / 2) + "," + (row * 30 + 2) + "," +
                            data[i + s].text +
                            "," + data[i + s].color.A + "-" + data[i + s].color.R + "-"
                            + data[i + s].color.G + "-" + data[i + s].color.B
                            + ",1,18," + (data[i + s].IsBold ? "1" : "0") + "\r\n";
                    x += w;
                }
            }
            File.WriteAllText(MessagePoster.workpath + "\\table.txt",code);
            ScriptDrawer.Draw(MessagePoster.workpath + "\\table.txt", MessagePoster.workpath + "\\data\\image\\table.png","nothing","nothing");
        }
    }
}
