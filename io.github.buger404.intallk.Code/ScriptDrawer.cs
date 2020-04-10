using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Data;
using System.Net;

namespace io.github.buger404.intallk.Code
{
    public class ScriptDrawer
    {
        private struct paramd
        {
            public string key;
            public string name;
            public string width;
            public string height;
        }
        private static List<paramd> plist = new List<paramd>();
        private static DataTable dt = new DataTable();
        private static FontFamily font = new FontFamily("Microsoft Yahei");
        private static StringFormat stf = new StringFormat();
        private static SolidBrush brush = new SolidBrush(Color.Transparent);
        private static Pen pen = new Pen(Color.Transparent);
        public static string AssetsPath = "";
        
        public static object Eval(string s)
        {
            return dt.Compute(s, null);
        }
        public static object FinalValue(string s)
        {
            string r = RepValue(s);
            return Eval(r);
        }
        public static string RepValue(string s)
        {
            string r = s;
            foreach (paramd pa in plist)
            {
                r = r.Replace(pa.name + "s", pa.key.Length.ToString())
                     .Replace(pa.name + "w", pa.width)
                     .Replace(pa.name + "h", pa.height)
                     .Replace(pa.name, pa.key);
            }
            return r;
        }
        private static Color GoColor(string s)
        {
            string[] t = s.Split('-');
            int a = Convert.ToInt32(FinalValue(t[0]));
            int r = Convert.ToInt32(FinalValue(t[1]));
            int g = Convert.ToInt32(FinalValue(t[2]));
            int b = Convert.ToInt32(FinalValue(t[3]));
            return Color.FromArgb(a,r,g,b);
        }
        private static void DownLoad(string url,string path)
        {
            WebClient w = new WebClient();
            w.DownloadFile(url,path);
            w.Dispose();
        }


        public static void Draw(string infile,string oufile,params string[] param)
        {
            int fail = 0;int fi = 0; string[] cmd = new string[] { "" };
            Graphics g = Graphics.FromHwnd(IntPtr.Zero); Bitmap b = new Bitmap(1, 1);

        tryagain:

            try
            {
                fail++;
                plist.Clear();
                for (int j = 0; j < param.Length; j += 2)
                {
                    paramd pa = new paramd();
                    pa.name = param[j]; pa.key = param[j + 1];
                    pa.width = (param[j + 1].Length * 20).ToString();//g.MeasureString(param[j + 1], font).Width.ToString();
                    pa.height = "20";  //g.MeasureString(param[j + 1], font).Height.ToString();
                    plist.Add(pa);
                }

                cmd = File.ReadAllText(infile).Split(new string[] { "\r\n" }, StringSplitOptions.None);
                for (int i = 0; i < cmd.Length; i++)
                {
                    fi = i;
                    string[] p = cmd[i].Split(',');
                    for (int s = 0; s < p.Length; s++)
                    {
                        p[s] = p[s].Trim();
                    }
                    switch (p[0])
                    {
                        case ("img"):
                            if (p[1].StartsWith("net:"))
                            {
                                string f = "";
                                if (p[1].StartsWith("net:<qq>"))
                                {
                                    f = RepValue(p[1].Replace("net:<qq>", ""));
                                    DownLoad("http://q.qlogo.cn/headimg_dl?dst_uin=" + f + "&spec=100", AssetsPath + f + "_face.jpg");
                                }
                                p[1] = f + "_face.jpg";
                            }
                            Image im = Image.FromFile(AssetsPath + RepValue(p[1]));
                            switch (p.Length)
                            {
                                case (4):
                                    g.DrawImage(im,
                                                new Point(
                                                    Convert.ToInt32(FinalValue(p[2])), Convert.ToInt32(FinalValue(p[3]))
                                                )
                                                );
                                    break;
                                case (5):
                                    g.DrawImage(im,
                                                new Rectangle(
                                                    Convert.ToInt32(FinalValue(p[2])), Convert.ToInt32(FinalValue(p[3])),
                                                    Convert.ToInt32(Convert.ToDouble(FinalValue(p[4])) * im.Width),
                                                    Convert.ToInt32(Convert.ToDouble(FinalValue(p[4])) * im.Height)
                                                )
                                                );
                                    break;
                                case (6):
                                    g.DrawImage(im,
                                                new Rectangle(
                                                    Convert.ToInt32(FinalValue(p[2])), Convert.ToInt32(FinalValue(p[3])),
                                                    Convert.ToInt32(FinalValue(p[4])), Convert.ToInt32(FinalValue(p[5]))
                                                )
                                                );
                                    break;
                            }
                            im.Dispose();
                            break;
                        case ("load"):
                            //Console.WriteLine(p[1] + "and" + FinalValue(p[1]).ToString() + "and" + RepValue(p[1]));
                            b = new Bitmap(Convert.ToInt32(FinalValue(p[1])), Convert.ToInt32(FinalValue(p[2])));
                            g = Graphics.FromImage(b);
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                            g.Clear(GoColor(p[3]));
                            break;
                        case ("rectl"):
                            pen.Color = GoColor(p[5]);
                            g.DrawRectangle(pen,
                                            new Rectangle(
                                            Convert.ToInt32(FinalValue(p[1])), Convert.ToInt32(FinalValue(p[2])),
                                            Convert.ToInt32(FinalValue(p[3])), Convert.ToInt32(FinalValue(p[4])))
                                            );
                            break;
                        case ("rect"):
                            brush.Color = GoColor(p[5]);
                            g.FillRectangle(brush,
                                            new Rectangle(
                                            Convert.ToInt32(FinalValue(p[1])), Convert.ToInt32(FinalValue(p[2])),
                                            Convert.ToInt32(FinalValue(p[3])), Convert.ToInt32(FinalValue(p[4])))
                                            );
                            break;
                        case ("oval"):
                            brush.Color = GoColor(p[5]);
                            g.FillEllipse(brush,
                                            new Rectangle(
                                            Convert.ToInt32(FinalValue(p[1])), Convert.ToInt32(FinalValue(p[2])),
                                            Convert.ToInt32(FinalValue(p[3])), Convert.ToInt32(FinalValue(p[4])))
                                            );
                            break;
                        case ("rrect"):
                            break;
                        case ("blur"):
                            Rectangle r = new Rectangle(0, 0, b.Width, b.Height);
                            b.GaussianBlur(ref r, Convert.ToInt32(FinalValue(p[1])), false);
                            break;
                        case ("write"):
                            int fsize = 20;int fstyle = (int)FontStyle.Regular;
                            if (p.Length > 6) { fsize = Convert.ToInt32(RepValue(p[6])); }
                            if (p.Length > 7) { fstyle = Convert.ToInt32(RepValue(p[7])); }
                            brush.Color = GoColor(p[4]);
                            GraphicsPath gp = new GraphicsPath(FillMode.Winding);
                            stf.Alignment = (StringAlignment)Convert.ToInt32(p[5]);
                            gp.AddString(RepValue(p[3]), font, fstyle, fsize,
                                        new Point(Convert.ToInt32(FinalValue(p[1])), Convert.ToInt32(FinalValue(p[2]))),
                                        stf);
                            g.FillPath(brush, gp);
                            gp.Dispose();
                            //g.DrawString(RepValue(p[3]), font, brush,
                            //             new Point(Convert.ToInt32(FinalValue(p[1])), Convert.ToInt32(FinalValue(p[2]))));
                            break;
                    }
                }

                //System.Drawing.Imaging.ImageAttributes attr = new System.Drawing.Imaging.ImageAttributes();
                //float[][] colorMatrixElements = {
                //new float[] {.33f,  .33f,  .33f,  0, 0},        // r = (r+g+b)/3
                //new float[] {.33f,  .33f,  .33f,  0, 0},        // g = (r+g+b)/3
                //new float[] {.33f,  .33f,  .33f,  0, 0},        // b = (r+g+b)/3
                //new float[] {0,  0,  0,  1, 0},        // alpha scaling factor of 1
                //new float[] {0,  0,  0,  0, 1}};    // 
                //System.Drawing.Imaging.ColorMatrix matrix = new System.Drawing.Imaging.ColorMatrix(colorMatrixElements);
                //attr.SetColorMatrix(matrix);
                //g.DrawImage(b, new Rectangle(0,0,b.Width+1,b.Height+1), 0, 0, b.Width + 1, b.Height + 1, GraphicsUnit.Pixel, attr);

                b.Save(oufile);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Draw succeed :" + oufile);
            }
            catch(Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Draw failed , retry :" + fail + "\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.TargetSite 
                                  + "\n\n" + cmd[fi]);
                g.Dispose(); b.Dispose();
                if (fail >= 13) { return; }
                goto tryagain;
            }
            g.Dispose(); b.Dispose();
        }
    }
}
