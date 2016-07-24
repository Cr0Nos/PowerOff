using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PowerOff
{
    class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
        private static IntPtr mThisConsole = GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static bool __debug = false;
        static int minutesbeforeSD = 0;

        static NotifyIcon trayIcon1 = new NotifyIcon();
        static Icon iconTimer = PowerOff.Properties.Resources.icon;

        static System.Threading.Timer _timer = new System.Threading.Timer(TimerTick, null, 0, 60000);   

        #region "functions"
        public static bool IsNumeric(object Expression)
        {
            if (Expression == null || Expression is DateTime) return false;
            if (Expression is Int16 || Expression is Int32 || Expression is Int64 || Expression is Decimal || Expression is Single || Expression is Double || Expression is Boolean) return true;
            try
            {
                if (Expression is string)
                    Double.Parse(Expression as string);
                else
                    Double.Parse(Expression.ToString());
                return true;
            }
            catch { }
            return false;
        }

        public static void CMDRun(string cmd)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/c " + cmd;
            process.StartInfo = startInfo;
            process.Start();
            if (__debug) Console.WriteLine("[DEBUG] " + cmd);
        }

        public static void SD(string time)
        {
            try
            {
                if(time == "0")
                {
                    CMDRun("shutdown -a ");
                    trayIcon1.Visible = false;
                } else {
                    string tmp = time.Substring(1);
                    if(!IsNumeric(tmp))
                    {
                        Console.WriteLine("Неверный формат");
                        if (__debug) Console.WriteLine("[DEBUG] " + time);
                    } else {
                        if (__debug) Console.WriteLine("[DEBUG] " + time.Substring(0, 1));
                        trayIcon1.Visible = true;
                        ShowWindow(GetConsoleWindow(), 0);
                        switch (time.Substring(0, 1))
                        {
                            case "м": case "ь": case "v": case "m":
                            {
                                CMDRun("shutdown -s -t " + Convert.ToInt32(tmp) * 60);
                                minutesbeforeSD = Convert.ToInt32(tmp);
                                break;
                            }
                            case "ч": case "р": case "x": case "h":
                            {
                                CMDRun("shutdown -s -t " + Convert.ToInt32(tmp) * 3600);
                                minutesbeforeSD = Convert.ToInt32(tmp) * 60;
                                break;
                            }
                            case "с": case "ы": case "c": case "s":
                            {
                                CMDRun("shutdown -s -t " + Convert.ToInt32(tmp));
                                minutesbeforeSD = Convert.ToInt32(tmp) / 60;
                                break;
                            }
                        }
                        trayIcon1.Text = minutesbeforeSD + " минут осталось до завершения работы";
                    }
                }            
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        static void Main(string[] args)
        {
            trayIcon1.Icon = iconTimer;

            int w = 60, h = 10;
            Console.SetWindowSize(w, h);
            Console.Title = "PowerOff v0.2";
            Console.ForegroundColor = ConsoleColor.White;
            if (args.Length > 0)
            {
                if (args[0] == "debug")
                {
                    __debug = true;
                    Console.WriteLine("Debug mode");
                } else {
                    SD(args[0]);
                    if (args[0] == "0")
                    {
                        trayIcon1.Visible = false;
                        Application.Exit();
                    }
                }
                if (__debug) Console.Title = Console.Title + " (Debug)";
                while (true)
                {
                    Console.Write("Введи время до отключения: ");
                    string tmp = Console.ReadLine();
                    if (tmp.Length > 0) SD(tmp);
                }
            } else {
                while (true)
                {
                    Console.WriteLine("Пример: м12 (отключение через 12 минут)");
                    Console.Write("Введи время до отключения: ");
                    string tmp = Console.ReadLine();
                    if (tmp.Length > 0) SD(tmp);
                }
            }
        }

        public static void TimerTick(object o)
        {
            minutesbeforeSD--;
            trayIcon1.Text = minutesbeforeSD + " минут осталось до завершения работы";
        }
    }
}
