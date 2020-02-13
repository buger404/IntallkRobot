// Artifical Artifical Intelligence
// 虚假AI
// 作者：Buger404

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace ArtificalA.Intelligence
{
    public class ArtificalAI
    {
        public static bool URLLoading = false;
        public static bool DebugLog = false;

        [STAThread]
        private static void Log(string log,ConsoleColor color = ConsoleColor.White)
        {
            if (DebugLog == false) { return; }
            Console.ForegroundColor = color;
            Console.WriteLine(log);
        }

        [STAThread]
        public static string Talk(string Question, string engine)
        {
            string ret = "";
            switch (engine)
            {
                case ("baidu"):
                    ret = Search(Question, engine,
                        "https://zhidao.baidu.com/search?lm=0&rn=10&pn=0&fr=search&ie=gbk&word={q}",
                        "a", "ti", "classname", "href",
                        "div", "content-", "id", "!@*`,.*/.");
                    break;
                case ("tieba"):
                    ret = Search(Question, engine,
                        "http://tieba.baidu.com/f/search/res?ie=utf-8&qw={q}&red_tag=e0313206225",
                        "a", "bluelink", "classname", "href",
                        "div", "post_content_", "id", "!@*`,.*/.");
                    break;
                case ("csdn"):
                    ret = Search(Question, engine,
                        "https://so.csdn.net/so/search/s.do?q={q}&t=&u=",
                        "a", "_blank", "target", "href",
                        "div", "content_views", "id", "content_views", "CSDN无相关结果");
                    break;
                case ("msdn"):
                    ret = Search(Question, engine,
                        "https://docs.microsoft.com/zh-cn/search/?search={q}&category=All&scope=Desktop",
                        "a", "searchItem.0", "data-bi-name", "href",
                        "main", "main", "id", "main", "MSDN无相关结果");
                    break;
            }
            return ret;
        }

        [STAThread]
        private static string Search(string Question, string engine, string repStr, string linktag, string linkattrt, string linkattr, string linkattrs, string contag, string conattrt, string conattr, string conattrs, string failstr = "不听不听王八念经")
        {
            WebBrowser web = new WebBrowser();
            web.ScriptErrorsSuppressed = true;
            web.DocumentCompleted += web_DocumentCompleted;
            web.Stop();
            URLLoading = false;
            Log("Engine:" + engine, ConsoleColor.Green);
            Log("Connect:" + repStr.Replace("{q}", HttpUtility.UrlEncode(Question)));
            web.Navigate(repStr.Replace("{q}", HttpUtility.UrlEncode(Question)));
            do { Application.DoEvents(); } while (!URLLoading);
            Log("Pull", ConsoleColor.Green);
            ArrayList link = new ArrayList();
            string url = ""; bool permiss = false;
            foreach (HtmlElement b in web.Document.GetElementsByTagName(linktag))
            {
                if (b.GetAttribute(linkattr) == linkattrt)
                {
                    url = b.GetAttribute(linkattrs);
                    switch (engine)
                    {
                        case ("csdn"):
                            permiss = ((url.IndexOf("blog.csdn.net") >= 0) && (url.IndexOf("article/details") >= 0));
                            break;
                        case ("tieba"):
                            permiss = (url.IndexOf("tieba.baidu.com/p/") >= 0);
                            break;
                        default:
                            permiss = true;
                            break;
                    }
                    if (permiss)
                    {
                        Log("Link:" + url);
                        link.Add(url);
                    }
                }
            }
            if (link.Count == 0) { Log("NULL", ConsoleColor.Red); web.Dispose(); return failstr; }
            Random r = new Random();
            int num = r.Next(0, link.Count);
            URLLoading = false;
            Log("Connect:" + link[num].ToString());
            web.Navigate(link[num].ToString());
            do { Application.DoEvents(); } while (!URLLoading);
            Log("Pull", ConsoleColor.Green);
            if (engine == "baidu")
            {
                foreach (HtmlElement b in web.Document.GetElementsByTagName("div"))
                {
                    if (b.GetAttribute("classname") == "showbtn")
                    {
                        Log("Click:ShowBtn");
                        b.InvokeMember("click");
                    }
                }
            }

            ArrayList ans = new ArrayList();
            string str;
            string[] temp; int gfail = 0;
            tryagain:
            foreach (HtmlElement b in web.Document.GetElementsByTagName(contag))
            {
                str = b.GetAttribute(conattr);
                if (str == null) { str = ""; }
                
                if ((str.IndexOf(conattrt) >= 0) || (str == conattrs))
                {
                    if (b.InnerText != null)
                    {
                        str = b.InnerText.Replace("展开全部", "").Replace("采纳", "夸奖").Replace("向左转|向右转", "").Replace("贴吧", "群").Replace("贴", "群");
                        temp = str.Split('\n');
                        str = "";
                        for (int i = 0; i < temp.Length - 1; i++)
                        {
                            if (temp[i].Trim() != "") { str = str + temp[i] + "\n"; }
                        }
                        str = str + temp[temp.Length - 1];
                        Log("Pull:" + str);
                        ans.Add(str);
                    }
                }
            }
            if ((ans.Count == 0) && (gfail <= 50)) 
            {
                gfail++;
                goto tryagain; 
            }
            //web.Dispose(); return rest;
            if (ans.Count == 0) { Log("NULL", ConsoleColor.Red); web.Dispose(); return failstr; }
            string[] ra = ans[r.Next(0, ans.Count)].ToString().Split('\n');
            string tts = ""; int fail = 0;
            if (engine == "csdn") { tts = ans[r.Next(0, ans.Count)].ToString(); goto donechoose; }
        rechoose:
            tts = ra[r.Next(0, ra.Length)];
            if (tts.StartsWith("\n"))
            {
                tts.Replace("\n", "");
            }
            if (tts.Replace(" ", "") == "")
            {
                fail++;
                if (fail > 100) { return failstr; }
                goto rechoose;
            }
        donechoose:
            web.Stop();
            web.Dispose();
            return tts;
        }

        static void web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            URLLoading = true;
        }

    }
}
