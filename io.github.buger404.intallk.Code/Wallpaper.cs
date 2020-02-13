//桌面背景随机获取工具
//作者：Buger404
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using MSXML2;

namespace Wallpapers.Searcher
{
    public class Wallpaper
    {
        private static string GetHTML(string url)
        {
            XMLHTTP x = new XMLHTTP();
            x.open("GET", url, false);
            x.send();
            return x.responseText;
        }
        private static bool Like(string src, string match)
        {
            string buff = ""; int lp = -1;
            for (int i = 0; i < match.Length; i++)
            {
                if (match[i] == '*')
                {
                    int sp = src.IndexOf(buff);
                    if (sp < lp) { return false; }
                    lp = sp; buff = "";
                }
                else
                {
                    buff += match[i];
                }
            }
            return true;
        }
        private static List<string> GetWallpaperList()
        {
            Random r = new Random();
            string html = GetHTML("http://desk.zol.com.cn/fengjing/" + r.Next(0, 80) + ".html");
            List<string> papers = new List<string>();

            string[] temp = html.Split(new[] { "src=\"" }, StringSplitOptions.None);
            for (int i = 1; i < temp.Length; i++)
            {
                string[] temp2 = temp[i].Split('\"');
                if(Like(temp2[0],"*://desk-fd.zol-img.com.cn/t_s*c5/*.jpg*")){
                    string[] f = temp2[0].Split(new[] {"desk-fd.zol-img.com.cn/t_s"}, StringSplitOptions.None)[1].Split(new[] {"c5/"}, StringSplitOptions.None);
                    string url = "http://desk-fd.zol-img.com.cn/t_s" + SystemInformation.PrimaryMonitorSize.Width + "x" + SystemInformation.PrimaryMonitorSize.Height + "c5/" + f[1];
                    papers.Add(url);
                }
            }

            return papers;
        }
        public static string GetWallpaper()
        {
            List<string> papers = GetWallpaperList();
            Random r = new Random();

            return (papers[r.Next(0, papers.Count)]);
        }
    }
}
