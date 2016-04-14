using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

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
    public partial class MainWindow : Form
    {
        public string Version = "2.2.1";

        private options fO = new options();
        private info fI = new info();
        internal gw2LHelp hlp = new gw2LHelp();
        private CloseWarning warn = new CloseWarning();
        Label[] lastloginlabels, accountlabels;
        Button[] startButtons, setButtons;
        RadioButton[] lastloginsign;
        internal string pid = "";
        //BackgroundWorker bw_copy = new BackgroundWorker();
        private string[] args = Environment.GetCommandLineArgs();
        private int lastStarted = -1;
        public int index = 0;
        Process gw2pro = null;
        private const int snapDist = 30;

        //constants and imports for moving the form without the top bar
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImportAttribute("user32.dll")]
        private static extern bool SetCapture(IntPtr hWnd);
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        public MainWindow()
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
            fO.labelStatus.Text = "";
            warn.thatParentForm = this;
            // register function to an asyc thread
            //bw_copy.DoWork += new DoWorkEventHandler(runCopyGw2localdat);
            //bw_copy.WorkerSupportsCancellation = true;
            //loading all variables into labels and textboxes
            if (args.Length > 0)
                for (int i = 0; i < args.Length; i++)
                    if (args[i] == "-id" && i < args.Length - 1)
                    {
                        pid = args[i + 1];
                    }
            //configLoad(); //called via FormLoadEvent (Form1_Load)
        }

        // setup the process and start
        private void startGW(int i)
        {
            //start gw if gw is not running and index is ok 
            using (Process p = Process.GetProcessesByName("gw2").FirstOrDefault())
            if (p != null)
                return;
            string path = ConfigGet("path");
            if (!File.Exists(path))
            {
                MessageBox.Show("invalid gw2path");
                return;
            }
            if (lastStarted != i) {
                string file = Path.Combine(fO.path, ConfigGet("acc." + i.ToString() + ".file"));
                if (!File.Exists(file)) {
                    file = Path.Combine(fO.path, pid + "#" + i.ToString() + "Local.dat");
                    if (File.Exists(file))
                    {
                        configPut("acc." + i.ToString() + ".file", pid + "#" + i.ToString() + "Local.dat");
                        configPut("acc." + i.ToString() + ".name", i.ToString());
                    }
                }
                if (File.Exists(file))
                {
                    ConfigSetLastlogin(i);
                    File.Copy(file, Path.Combine(fO.path, "Local.dat"), true);
                    lastloginsign[i].ForeColor = Color.Lime;
                    if (lastStarted >= 0) //&& lastStarted != i
                        lastloginsign[lastStarted].ForeColor = Color.RoyalBlue;
                    lastloginsign[i].Checked = true;
                    lastStarted = i;
                }
                else
                {
                    MessageBox.Show("no file at target found\n" +
                        file + "\n");
                    return;
                }
            }
            else
                ConfigSetLastlogin(i);
            string gw2path = path.Substring(0, path.LastIndexOf("\\"));
            string dllFileCopy = Path.Combine(gw2path, "bin\\d3d9.dll2");
            string dllFile = Path.Combine(gw2path, "bin\\d3d9.dll");

            //check for shader files and usage
            if (ConfigGet("acc." + i.ToString() + ".useshader") == "True") {
                Process shUnl = new Process();
                shUnl.StartInfo.FileName = ConfigGet("pathUnlocker");
                if (File.Exists(shUnl.StartInfo.FileName) && (File.Exists(dllFile) || File.Exists(dllFileCopy))) {
                    if (File.Exists(dllFileCopy) && !File.Exists(dllFile))
                        File.Move(dllFileCopy, dllFile);
                    try { shUnl.Start(); }
                    catch (Exception e) { MessageBox.Show(e.Message); }
                    Thread.Sleep(100);
                }
            }
            else
            {
                if (File.Exists(dllFile))
                    File.Move(dllFile, dllFileCopy);
            }

            //start gw
            for (int k = 0; k < startButtons.Length; k++)
                startButtons[k].Enabled = false;
            fO.labelStatus.Text = "gw2 is running";
            gw2pro = new Process();
            gw2pro.StartInfo.FileName = ConfigGet("path");
            gw2pro.StartInfo.Arguments = ConfigGet("cmd");
            if (ConfigGet("autosave") == "True")
            {
                gw2pro.EnableRaisingEvents = true;
                gw2pro.Exited += new EventHandler(onGW2Exit);
                index = i;
            }
            try { gw2pro.Start(); }
            catch (Exception e) { MessageBox.Show(e.Message); }
            

        }

        private void onGW2Exit(object sender, System.EventArgs e)
        {
            Thread.Sleep(2000);
            fO.saveLocalDat(index);
            for (int i=0; i<startButtons.Length;i++)
                startButtons[i].Enabled = true;
        }

        // get ID from accessibleName, all accessible names used in this program are plain numbers
        // used to access all buttons or textboxes with only one fuction assigned
        // the ID is used as index in the lists of all labels/boxes
        private int getID(object sender) {
            Button button = sender as Button;
            int id = Convert.ToInt32(button.AccessibleName);
            if (id >= 0 && id <= 100)
                return id;
            else
                return 0;
        }
        private void setLastlogin(int ID, DateTime date)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            DateTime dateUTC = date.ToUniversalTime();
            now = now.AddHours(0 - now.Hour);
            now = now.AddMinutes(0 - now.Minute);
            now = now.AddSeconds(0 - now.Second);
                
            double days = now.Subtract(dateUTC).TotalDays;
            double daysi = Math.Floor(days) + 1;
            if (daysi < 1)
            {
                lastloginlabels[ID].Text = "today";
                if (lastStarted == ID)
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
            lastloginsign[ID].Visible = true;
        }
        private void setLastlogin(int ID, string date)
        {
            if (date == null || date.Length < 8)
            {
                lastloginlabels[ID].Text = "";
                return;
            }
            setLastlogin(ID, Convert.ToDateTime(date));
        }
        
        private Rectangle getScreen()
        {
            return Screen.FromControl(this).Bounds;
        }
        //////////////////////
        // App.config methods
        internal void configLoad()
        {
            for (int i = 0; i < 10; i++)
            {
                if (ConfigGet("acc." + i.ToString() + ".name") != "" && ConfigGet("acc." + i.ToString() + ".lastlogin") != "")
                {
                    fO.setEntry(i,
                        ConfigGet("acc." + i.ToString() + ".name"),
                        ConfigGet("acc." + i.ToString() + ".useshader"),
                        ConfigGet("acc." + i.ToString() + ".p2p"));
                    setLastlogin(i, ConfigGet("acc." + i.ToString() + ".lastlogin"));
                    accountlabels[i].Text = ConfigGet("acc." + i.ToString() + ".name");
                }
                else
                if (File.Exists(Path.Combine(fO.path, pid + "#" + i.ToString() + "Local.dat")))
                {
                    lastloginlabels[i].Text = "unknown";
                    fO.setEntry(i, "acc#" + i.ToString(), "false","false");
                    lastloginsign[i].ForeColor = Color.Red;
                    lastloginsign[i].Visible = true;
                    lastloginsign[i].Checked = false;
                }
                else
                {
                    lastloginlabels[i].Text = "";
                    lastloginsign[i].ForeColor = Color.Gray;
                    lastloginsign[i].Checked = false;
                    lastloginsign[i].Visible = false;
                }

            }
            fO.setPath(ConfigGet("path"), 0);
            fO.setCmd(ConfigGet("cmd"));
            fO.setPath(ConfigGet("pathUnlocker"), 1);
            fO.setCheckbox(10, ConfigGet("snap"));
            fO.setCheckbox(11, ConfigGet("savePos"));
            fO.setCheckbox(12, ConfigGet("autosave"));
            if (fO.checkBoxSavePos.Checked) {
                string x = ConfigGet("WindowPosX");
                string y = ConfigGet("WindowPosY");
                if (x != null && y != null && x.Length>0 && y.Length>0 && Regex.IsMatch(x, @"^\d+$") && Regex.IsMatch(y, @"^\d+$"))
                {
                    int locX = int.Parse(x);
                    int locY = int.Parse(y);
                    if (locX>=0 && locY >= 0 && locX < 2000000 && locY < 2000000)
                        Location = new Point(locX, locY);
                }
                    
            }
        }
        internal void configPut(string key, string value)
        {
            if (pid != "")
                key = pid + "-" + key;
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
            ConfigurationManager.AppSettings.Set(key, value);
            config.Save(ConfigurationSaveMode.Minimal);
        }
        internal string ConfigGet(string key)
        {
            string ret = "";
            try {
                if (pid != "")
                    key = pid + "-" + key;
                ret = ConfigurationManager.AppSettings.Get(key);
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
            string date = System.DateTime.Now.ToString();
            configPut("acc." + ID.ToString() + ".lastlogin", date);
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
            int notLoggedIn = 0;
            for (int i = 0; i < 10; i++)
            {
                if (fO.listChecboxes2[i].Checked && 
                    lastloginsign[i].Visible &&
                    (lastloginsign[i].ForeColor == Color.Red || lastloginsign[i].ForeColor == Color.Goldenrod))
                    notLoggedIn++;

            }
            if (notLoggedIn > 0)
            {
                warn.Show();
                warn.labelCount.Text = notLoggedIn.ToString();
            }
            else
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
            for (int k = 0; k < startButtons.Length; k++)
                startButtons[k].Enabled = true;
        }
        
        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void buttonInfo_Click(object sender, EventArgs e)
        {
            fI.label1Version.Text = this.Version;
            if (fI.Visible)
                fI.Hide();
            else
            {
                if (fI.IsDisposed)
                    fI = new info();
                fI.Show();
            }
        }

        /////////////////
        // window events
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        
        private void Form1_Move(object sender, EventArgs e)
        {
            if (fO.checkBoxSnap.Checked) {
                int finalPosX = Location.X;
                int finalPosY = Location.Y;
                bool changed = false;
                Rectangle screen = getScreen();
                if (Location.X < screen.Left + snapDist)
                {
                    changed = true;
                    finalPosX = screen.Left;
                } else
                if (Location.X + Size.Width > screen.Right - snapDist)
                {
                    changed = true;
                    finalPosX = screen.Right - Size.Width;
                }

                if (Location.Y < screen.Top + snapDist)
                {
                    changed = true;
                    finalPosY = screen.Top;
                } else
                if (Location.Y + Size.Height > screen.Bottom - snapDist)
                {
                    changed = true;
                    finalPosY = screen.Bottom - Size.Height;
                }

                if (changed && !screen.IsEmpty)
                {
                    this.Location = new Point(finalPosX, finalPosY);
                    ReleaseCapture();
                    //this.Location = new Point(finalPosX, finalPosY);
                    SetCapture(Handle);
                }
            }
            //sets the location for the options window, moves them together
            Point p = this.PointToScreen(new Point(this.Width, this.ClientRectangle.Y));
            this.fO.Location = p;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (fO.checkBoxSavePos.Checked)
            {
                configPut("WindowPosX", Location.X.ToString());
                configPut("WindowPosY", Location.Y.ToString());
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            configLoad();
        }
    }
}
