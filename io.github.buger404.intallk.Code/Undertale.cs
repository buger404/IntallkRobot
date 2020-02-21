using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Undertale.Dialogs
{
    public class UT
    {
        [DllImport("kernel32.dll",
            CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();
        public static string role = "";
        public static string dialog = "";
        public static long targetg = 0;
        public static long round = 0;
        public static long tick = 0;
        public static string[] says;
        public struct player
        {
            public long qq;
            public float score;
        }
        public struct rolepos
        {
            public int pos;
            public string name;
        }
        public static List<rolepos> pos = new List<rolepos>();
        public static List<player> ps = new List<player>();
        public static string winstr = "";
        public static float prise = 10;
        public static int mode = 0;

        public static void inits()
        {
            //加载UT对话库
            Console.WriteLine("Undertale:Loading...");
            says = File.ReadAllText("D:\\DataArrange\\undertale.bin", Encoding.UTF8).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < says.Length; i++)
            {
                if (says[i].StartsWith("#"))
                {
                    rolepos p;
                    p.pos = i; p.name = says[i].Replace("#", "").Replace("\n","");
                    Console.WriteLine("Undertale Loaded:" + p.name + " at " + p.pos);
                    pos.Add(p);
                }
                says[i] = says[i].Replace("\n", "").ToLower();
            }
            Console.WriteLine("Undertale:" + says.Length + " sentences.");
        }

        public static void nextRound()
        {
            if (mode == 0)
            {
                puzzleInit1();
            }
            else
            {
                puzzleInit2();
            }
        }

        public static void puzzleInit1()
        {
            Random r = new Random();
            int p = r.Next(0, says.Length);
            for (int i = 0; i < pos.Count; i++)
            {
                if (pos[i].pos <= p)
                {
                    role = pos[i].name;
                    if (pos[i].pos == p) { p++; }
                }
            }
            dialog = says[p]; prise = 10; winstr = "";
            round++;
        }

        public static void puzzleInit2()
        {
            Random r = new Random();
            int p = r.Next(0, says.Length);
            for (int i = 0; i < pos.Count; i++)
            {
                if (pos[i].pos <= p)
                {
                    if (pos[i].pos == p) { p++; }
                }
            }
            string[] t = says[p].Split(' ');
            rerole:
            role = t[r.Next(0, t.Length)];
            if (role.Length <= 2) { goto rerole; }
            int po = r.Next(0, 8); string choicestr = "";string word = "";
            int p2 = 0;

            for (int j = 0; j < 8; j++)
            {
                if (j == po)
                {
                    choicestr = choicestr + (new string(new char[] { (char)('a' + j) })) + ". " + role;
                }
                else
                {
                    p2 = r.Next(0, says.Length);
                    for (int i = 0; i < pos.Count; i++)
                    {
                        if (pos[i].pos <= p2)
                        {
                            if (pos[i].pos == p2) { p2++; }
                        }
                    }
                    t = says[p2].Split(' ');
                    rerole2:
                    word = t[r.Next(0, t.Length)];
                    if (word.Length <= 2) { goto rerole2; }
                    choicestr = choicestr + (new string(new char[] { (char)('a' + j) })) + ". " + word;
                }
                if (j < 7) { choicestr += "  |  "; }
            }

            dialog = says[p].Replace(role, "________") + "\n\n" + choicestr; prise = 10; winstr = "";

            role = new string(new char[] {(char)('a' + po)});

            round++;
        }

    }
}
