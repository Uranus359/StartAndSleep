using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using DataPair = System.Collections.Generic.Dictionary<int, StartAndSleep.ProcessData>;

namespace StartAndSleep
{
    public class TableAcess
    {
        private ListView table;
        private ContextMenuStrip menu;
        private DataPair dict;
        private int selectedPID = -1;

        public TableAcess(ListView view, ContextMenuStrip cms)
        {
            table = view;
            menu = cms;
            table.ContextMenuStrip = menu;
            table.Font = new System.Drawing.Font("Consolas", 11f);
            table.View = View.Details;
            table.Activation = ItemActivation.OneClick;
            table.FullRowSelect = true;
            table.Columns.Add("Id",50,HorizontalAlignment.Left);
            table.Columns.Add("Name", 350, HorizontalAlignment.Left);
            table.Columns.Add("Suspended", 100, HorizontalAlignment.Left);
            table.Columns.Add("Hidden", 100, HorizontalAlignment.Left);
            table.Columns.Add("Start Time", 100, HorizontalAlignment.Left);
            dict = new DataPair();
            view.SelectedIndexChanged += View_SelectedIndexChanged;
            menu.Opening += Menu_Opening;
        }

        void RefreshEntry(int pid)
        {
            table.Items[table.Items.IndexOf(table.SelectedItems[0])] = new ListViewItem(new string[] {
                    $"{dict[pid].Id}",
                    dict[pid].Name,
                    $"{dict[pid].IsSuspend}",
                    $"{dict[pid].IsHidden}",
                    dict[pid].StartBy,
                });
        }


        private void Menu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (selectedPID > -1)
            {
                menu.Visible = true;
                menu.Items["cmsFocus"].Enabled = (dict[selectedPID].WindowHandle != IntPtr.Zero);
                menu.Items["cmsSuspendHide"].Enabled = (!dict[selectedPID].IsSuspend || !dict[selectedPID].IsHidden);
                menu.Items["cmsResumeShow"].Enabled = (dict[selectedPID].IsSuspend || dict[selectedPID].IsHidden);
                menu.Items["cmsSuspend"].Enabled = (!dict[selectedPID].IsSuspend);
                menu.Items["cmsResume"].Enabled = (dict[selectedPID].IsSuspend);
                menu.Items["cmsHide"].Enabled = (!dict[selectedPID].IsHidden);
                menu.Items["cmsShow"].Enabled = (dict[selectedPID].IsHidden);
            }
            else
            {
                menu.Items["cmsFocus"].Enabled = false;
                menu.Items["cmsSuspendHide"].Enabled = false;
                menu.Items["cmsResumeShow"].Enabled = false;
                menu.Items["cmsSuspend"].Enabled = false;
                menu.Items["cmsResume"].Enabled = false;
                menu.Items["cmsHide"].Enabled = false;
                menu.Items["cmsShow"].Enabled = false;
            }
        }

        public void Focus()
        {
            if (selectedPID >= 0)
            {
                dict[selectedPID].Focus();
            }
        }

        public void SuspendAndHide()
        {
            if (selectedPID >= 0 && (!dict[selectedPID].IsSuspend || !dict[selectedPID].IsHidden))
            {
                dict[selectedPID].Hide();
                dict[selectedPID].Suspend();
                RefreshEntry(selectedPID);
            }
        }

        public void ResumeAndShow()
        {
            if (selectedPID >= 0 && (dict[selectedPID].IsSuspend || dict[selectedPID].IsHidden))
            {
                dict[selectedPID].Resume();
                dict[selectedPID].Show();
                RefreshEntry(selectedPID);
            }
        }

        public void Suspend()
        {
            if (selectedPID >= 0 && !dict[selectedPID].IsSuspend)
            {
                dict[selectedPID].Suspend();
                RefreshEntry(selectedPID);
            }
        }

        public void Resume()
        {
            if (selectedPID >= 0 && dict[selectedPID].IsSuspend)
            {
                dict[selectedPID].Resume();
                RefreshEntry(selectedPID);
            }
        }

        public void Hide()
        {
            if (selectedPID >= 0 && !dict[selectedPID].IsHidden)
            {
                dict[selectedPID].Hide();
                RefreshEntry(selectedPID);
            }
        }

        public void Show()
        {
            if (selectedPID >= 0 && dict[selectedPID].IsHidden)
            {
                dict[selectedPID].Show();
                RefreshEntry(selectedPID);
            }
        }

        private void View_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (table.SelectedItems.Count == 0)
            {
                selectedPID = -1; // ?
                return;
            } else
            {
                selectedPID = int.Parse(table.SelectedItems[0].Text);
            }
            //System.Windows.Forms.MessageBox.Show(selectedPID.ToString());
        }

        public void Add(ProcessData data)
        {
            ListViewItem entry = new ListViewItem(new string[] {
                $"{data.Id}",
                data.Name,
                $"{data.IsSuspend}",
                $"{data.IsHidden}",
                data.StartBy,
            });
            table.Items.Add(entry);
            dict.Add(data.Id, data);
        }

        public void RefreshData(bool withWnd)
        {
            int lastpid = selectedPID;
            bool isselected = (lastpid >= 0);
            ProcessData pd = null;
            if (isselected)
            {
                pd = dict[lastpid];
            }
            dict.Clear();
            table.Items.Clear();
            IEnumerable<Process> allproc;
            if (withWnd)
            {
                allproc = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.ProcessName) && p.MainWindowHandle != IntPtr.Zero);

            } else
            {
                allproc = Process.GetProcesses().Where(p => !string.IsNullOrEmpty(p.ProcessName));
            }         
            foreach (var proc in allproc) {
                if (isselected && proc.Id == selectedPID)
                {
                    Add(pd);
                    table.Items[table.Items.Count-1].Selected = true;
                } else
                {
                    Add(new ProcessData(proc));
                }
            }
            selectedPID = lastpid;
        }
    }
}
