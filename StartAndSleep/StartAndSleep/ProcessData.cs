using System;
using System.Diagnostics;

namespace StartAndSleep
{
    public class ProcessData
    {
        private Process proc;
        public int Id { private set; get; }
        public string Name { private set; get; }
        public bool IsSuspend { private set; get; }
        public bool IsHidden { private set; get; }
        public string StartBy { private set; get; }
        public IntPtr WindowHandle { private set; get; }

        public ProcessData(Process which)
        {
            proc = which;
            Id = proc.Id;
            Name = proc.ProcessName;
            IsSuspend = false; // must fix
            try
            {
                StartBy = proc.StartTime.ToShortTimeString();
            } catch
            {
                StartBy = "Not Access";
            }
            WindowHandle = proc.MainWindowHandle;
            IsHidden = (WindowHandle == IntPtr.Zero);
        }

        public void Focus()
        {
            WRing.Window.Find(proc.MainWindowHandle).SetForeground();
        }

        public void Hide()
        {
            WRing.Window.Find(proc.MainWindowHandle).Show(false);
            IsHidden = true;
        }

        public void Show()
        {
            WRing.Window.Find(proc.MainWindowHandle).Show(true);
            IsHidden = false;
        }

        public void Suspend()
        {
            WinAPI.SuspendProcess(Id);
            IsSuspend = true;
        }

        public void Resume()
        {
            WinAPI.ResumeProcess(Id);
            IsSuspend = false;
        }
    }
}
