using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace StartAndSleep
{
    public partial class Form1 : Form
    {
        Dictionary<string, Process> apps = new Dictionary<string, Process>();
        Process curProc;

        public Form1()
        {
            InitializeComponent();
            FindProcesses();
            listBox1.SelectedIndex = 0;
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
            LockGUI(LockEvent.Before);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            LockGUI(LockEvent.Resume);
            WinAPI.ResumeProcess(curProc.Id);
            WinAPI.ShowWindow(curProc.MainWindowHandle, WinAPI.SW_SHOW);
            curProc.Kill();
            apps.Clear();
            listBox1.Items.Clear();
            FindProcesses();
            listBox1.SelectedIndex = 0;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            apps.Clear();
            listBox1.Items.Clear();
            FindProcesses();
            listBox1.SelectedIndex = 0;
        }

        private void FindProcesses()
        {
            var processes = Process.GetProcesses().Where(p => p.MainWindowHandle != IntPtr.Zero);
            foreach (var proc in processes)
            {
                var pname = $"{proc.ProcessName} [{proc.Id}]"; // without clones
                listBox1.Items.Add(pname);
                apps.Add(pname, proc);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            LockGUI(LockEvent.Suspend);
            curProc = apps[listBox1.SelectedItem.ToString()];
            WinAPI.ShowWindow(curProc.MainWindowHandle, WinAPI.SW_HIDE);
            WinAPI.SuspendProcess(curProc.Id);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            LockGUI(LockEvent.Resume);
            WinAPI.ResumeProcess(curProc.Id);
            WinAPI.ShowWindow(curProc.MainWindowHandle, WinAPI.SW_SHOW);
        }

        void LockGUI(LockEvent which)
        {
            if (which == LockEvent.Before)
            {
                button2.Enabled = false;
                button4.Enabled = false;
            }
            else if (which == LockEvent.Suspend)
            {
                button1.Enabled = false;
                listBox1.Enabled = false;
                button3.Enabled = false;
                button2.Enabled = true;
                button4.Enabled = true;
            }
            else if (which == LockEvent.Resume)
            {
                button1.Enabled = true;
                listBox1.Enabled = true;
                button3.Enabled = true;
                button2.Enabled = false;
                button4.Enabled = false;
            }
        }
    }

    public enum LockEvent
    {
        Before,
        Suspend,
        Resume,
    }

    public static class WinAPI
    {
        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;

        [DllImport("User32")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);


        public static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }

        public static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }
    }
}
