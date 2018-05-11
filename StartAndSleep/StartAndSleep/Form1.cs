using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace StartAndSleep
{
    public partial class Form1 : Form
    {
        TableAcess access;

        public Form1()
        {
            InitializeComponent();
            access = new TableAcess(listView1, contextMenuStrip1);
            access.RefreshData(checkBox1.Checked);
            listView1.Items[0].Selected = true;
            Timer t = new Timer() { Interval = 10000 };
            t.Tick += T_Tick;
            checkBox2.CheckedChanged += (s, e) => t.Enabled = (s as CheckBox).Checked;
            button1.Click += (s,e) => access.RefreshData(checkBox1.Checked);
            cmsFocus.Click += (s, e) => access.Focus();
            cmsHide.Click += (s, e) => access.Hide();
            cmsShow.Click += (s, e) => access.Show();
            cmsSuspend.Click += (s, e) => access.Suspend();
            cmsResume.Click += (s, e) => access.Resume();
            cmsSuspendHide.Click += (s, e) => access.SuspendAndHide();
            cmsResumeShow.Click += (s, e) => access.ResumeAndShow();
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            access.Add(new ProcessData(Process.Start(textBox1.Text)));
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            var pd = new ProcessData(Process.Start(textBox1.Text));
            pd.Hide();
            pd.Suspend();
            access.Add(pd);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = ofd.FileName;
                }
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            access.RefreshData(checkBox1.Checked);
        }
    }
}
