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
using System.Runtime.InteropServices;
using System.Configuration;
using System.Collections.Specialized;
using System.Threading;
using System.IO;

/*
 * The MIT License (MIT)
 * 
 * Copyright © 2015, Tiavor
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

namespace GW2Helper
{
    public partial class Form1 : Form
    {
        public string Version = "2.0";
        private options fO = new options();
        private info fI = new info();
        internal help hlp = new help();
        int oldX=0,oldY=0;
        Label[] lastloginlabels, accountlabels;
        Button[] startButtons, setButtons;
        RadioButton[] lastloginsign;
        internal string pid = "";
        //BackgroundWorker bw_copy = new BackgroundWorker();
        private string[] args = Environment.GetCommandLineArgs();
        private int lastStarted=-1;
        
        public Form1()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;

            // list all labels and buttons for easier access while coding
            lastloginlabels = new Label[] { labelLastlogin0, labelLastlogin1, labelLastlogin2, labelLastlogin3, labelLastlogin4,
                                            labelLastlogin5, labelLastlogin6, labelLastlogin7, labelLastlogin8, labelLastlogin9};
            accountlabels = new Label[] { labelAccname0, labelAccname1, labelAccname2, labelAccname3, labelAccname4 ,
                                          labelAccname5, labelAccname6, labelAccname7, labelAccname8, labelAccname9};
            startButtons = new Button[] { button_startacc0, button_startacc1, button_startacc2, button_startacc3, button_startacc4,
                                          button_startacc5, button_startacc6, button_startacc7, button_startacc8, button_startacc9};
            setButtons = new Button[] { buttonSetLL0, buttonSetLL1, buttonSetLL2, buttonSetLL3, buttonSetLL4,
                                        buttonSetLL5, buttonSetLL6, buttonSetLL7, buttonSetLL8, buttonSetLL9};
            lastloginsign = new RadioButton[] {radioButton1, radioButton2, radioButton3, radioButton4, radioButton5,
                                               radioButton6, radioButton7, radioButton8, radioButton9, radioButton10};
            toolTip1.SetToolTip(setButtons[0], "Sets the last login to now without starting GW2");
            //toolTip1.SetToolTip(buttonOptionen, "Beware: if not setup correctly, gw2 will remain white on startup.\n But it may also take a little longer as usual. (10s ony my system till char select)");
            // adding a reference of this form to the options form for accessing functions etc
            fO.thatParentForm = this;
            // register function to an asyc thread
            //bw_copy.DoWork += new DoWorkEventHandler(runCopyGw2localdat);
            //bw_copy.WorkerSupportsCancellation = true;
            //loading all variables into labels and textboxes
            if (args.Length > 0)
                for (int i = 0; i < args.Length; i++)
                    if (args[i] == "-id"&& i<args.Length-1)
                    {
                        pid = args[i + 1];
                    }
            configLoad();
        }

        //variables and imports for moving the form without the top bar
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        // setup the process and start
        private void startGW(int index)
        {
            //start gw if gw is not running and index is ok 
            Process p = Process.GetProcessesByName("gw2").FirstOrDefault();
            
            if (p == null)
            {
                string file = Path.Combine(fO.path, ConfigGet("acc." + index.ToString() + ".file"));
                if (!File.Exists(file)) {
                    file = Path.Combine(fO.path, pid + "#" + index.ToString() + "Local.dat");
                    if (File.Exists(file))
                    {
                        configPut("acc." + index.ToString() + ".file", pid+"#" + index.ToString() + "Local.dat");
                        configPut("acc." + index.ToString() + ".name", index.ToString());
                    }
                }
                if (File.Exists(file))
                {
                    ConfigSetLastlogin(index);
                    File.Copy(file, Path.Combine(fO.path, "Local.dat"),true);
                    lastloginsign[index].ForeColor = Color.Lime;
                    if (lastStarted>=0)
                        lastloginsign[lastStarted].ForeColor = Color.RoyalBlue;
                    lastloginsign[index].Checked = true;
                    lastStarted = index;
                }
                else
                {
                    MessageBox.Show("no file at target found\n" +
                        file + "\n");
                    return;
                }

                string gw2path = ConfigGet("path").Substring(0, ConfigGet("path").LastIndexOf("\\"));
                string dllFileCopy=Path.Combine(gw2path,"bin\\d3d9.dll2");
                string dllFile=Path.Combine(gw2path,"bin\\d3d9.dll");

                //check for shader files and usage
                if (ConfigGet("acc." + index.ToString() + ".useshader") == "True") {
                    Process shUnl = new Process();
                    shUnl.StartInfo.FileName = ConfigGet("pathUnlocker");
                    if (File.Exists(shUnl.StartInfo.FileName) && (File.Exists(dllFile)||File.Exists(dllFileCopy))) {
                        if (File.Exists(dllFileCopy) && !File.Exists(dllFile))
                            File.Move(dllFileCopy,dllFile);
                        shUnl.Start();
                        Thread.Sleep(100);
                    }
                }
                else
                {
                    if (File.Exists(dllFile))
                        File.Move(dllFile, dllFileCopy);
                }
                //start gw
                Process gw2pro = new Process();
                gw2pro.StartInfo.FileName = ConfigGet("path");
                gw2pro.StartInfo.Arguments = ConfigGet("cmd");
                gw2pro.Start();
            }
        }
        
        // get ID from accessibleName, all accessible names used in this program are plain numbers
        // used to access all buttons or textboxes with only one fuction assigned
        // the ID is used as index in the lists of all labels/boxes
        private int getID(object sender){
            Button button = sender as Button;
            int id=Convert.ToInt32(button.AccessibleName);
            if (id >= 0 && id <= 100)
                return id;
            else
                return 0;
        }
        private void setLastlogin(int ID, DateTime date)
        {
            DateTime now=DateTime.Now.ToUniversalTime();
            DateTime dateUTC = date.ToUniversalTime();
            now = now.AddHours(0 - now.Hour);
            now = now.AddMinutes(0 - now.Minute);
            now = now.AddSeconds(0 - now.Second);
            if (ID > 0)
                ;
            double days=now.Subtract(dateUTC).TotalDays;
            double daysi = Math.Floor(days)+1;
            if (daysi < 1)
            {
                lastloginlabels[ID].Text = "today";
                if (lastStarted==ID)
                    lastloginsign[ID].ForeColor = Color.Lime;
                else
                    lastloginsign[ID].ForeColor = Color.RoyalBlue;
            }
            else
                if (daysi < 2)
                {
                    lastloginsign[ID].ForeColor = Color.Goldenrod;
                    lastloginlabels[ID].Text = daysi.ToString() + " day ago";
                }
                else {
                    lastloginlabels[ID].Text = daysi.ToString() + " days ago";
                    lastloginsign[ID].ForeColor = Color.Red;
                }
            lastloginsign[ID].Checked = true;
        }
        private void setLastlogin(int ID, string date)
        {
            if (date == null || date.Length<8)
            {
                lastloginlabels[ID].Text = "";
                return;
            }
            setLastlogin(ID, Convert.ToDateTime(date));
        }

        //get the position of another application, code by DataDink
        /*
        [StructLayout(LayoutKind.Sequential)]
        internal struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        public static Point GetHwndPos(IntPtr hwnd)
        {
            RECT lpRect = new RECT();
            GetWindowRect(hwnd, out lpRect);
            return new Point(lpRect.Left, lpRect.Top);
        }*/
        //////////////////////
        // App.config methods
        internal void configLoad()
        {
            for (int i = 0; i < 10;i++ )
            {
                if (ConfigGet("acc." + i.ToString() + ".name") != "")
                {
                    fO.setEntry(i,
                        ConfigGet("acc." + i.ToString() + ".name"),
                        ConfigGet("acc." + i.ToString() + ".useshader"));
                    setLastlogin(i, ConfigGet("acc." + i.ToString() + ".lastlogin"));
                    accountlabels[i].Text = ConfigGet("acc." + i.ToString() + ".name");
                }
                else
                    if (File.Exists(Path.Combine(fO.path, pid+"#" + i.ToString() + "Local.dat")))
                    {
                        lastloginlabels[i].Text = "unknown";
                        fO.setEntry(i, "acc#" + i.ToString(), "false");
                    }
                    else
                    {
                        lastloginlabels[i].Text = "";
                        lastloginsign[i].Checked = false;
                    }
                    
            }
            fO.setPath(ConfigGet("path"),0);
            fO.setCmd(ConfigGet("cmd"));
            fO.setPath(ConfigGet("pathUnlocker"),1);
        }
        internal void configPut(string key, string value)
        {
            if (pid!="")
                key = pid + "-" + key;
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
            ConfigurationManager.AppSettings.Set(key, value);
            config.Save(ConfigurationSaveMode.Minimal);
        }
        internal string ConfigGet(string key)
        {
            string ret="";
            try {
                if (pid != "")
                    key = pid+"-"+key;
                ret=ConfigurationManager.AppSettings.Get(key);
            }
            catch (Exception e) {
                return "";
            }
            if (ret == null)
                return "";
            else
                return ret;
        }
        private void ConfigSetLastlogin(int ID)
        {
            string date=System.DateTime.Now.ToString();
            configPut("acc."+ID.ToString()+".lastlogin", date);
            setLastlogin(ID, System.DateTime.Now);
        }
        //////////////////////
        //button click events
        private void buttonOptionen_Click(object sender, EventArgs e)
        {
            if (fO.Visible)
            {
                fO.Hide();
            }
            else
            {
                fO.Show();
                fO.Top = this.Top;
                fO.Left = this.Left + this.Width;
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button_startacc1_Click(object sender, EventArgs e)
        {
            int i = getID(sender);
            startGW(i);
        }
        private void buttonSetacc_Click(object sender, EventArgs e)
        {
            int i = getID(sender);
            ConfigSetLastlogin(i);
        }

        /////////////////
        // window events
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                /*if (fO.Visible)
                {
                    fO.Focus();
                    this.Focus();
                    fO.Left = this.Left + this.Width;
                    fO.Top = this.Top;
                }*/
            }
        }
        private void Form1_Move(object sender, EventArgs e)
        {
            Point p = this.PointToScreen(new Point(this.Width, this.ClientRectangle.Y));
            this.fO.Location = p;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Opacity<0.8)
            if (e.X < 5 || e.Y < 5 || e.X > Size.Width - 5 || e.Y > Size.Height - 5)
            {
                Opacity = 0.3;
            }
            else
            {
                Opacity = 0.7;
            }
            if (fO.Visible)
                Opacity = 1;
            oldX = e.X;
            oldY = e.Y;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if(!fO.Visible)
            Opacity = 0.3;
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            Opacity = 1;
            configLoad();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.MinimizeBox=true;
        }

        private void buttonInfo_Click(object sender, EventArgs e)
        {
            fI.label1Version.Text = this.Version;
            if (fI.Visible)
                fI.Hide();
            else
                fI.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            configLoad();
        }
    }
}
