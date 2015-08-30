using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
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
    public partial class options : Form
    {

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();
        internal Form1 thatParentForm { get; set; }
        private Label[] listStatus;
        private TextBox[] listNamebox;
        private CheckBox[] listShader;
        internal String path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Guild Wars 2");
        
        public options()
        {
            InitializeComponent();
            this.AutoScaleMode = AutoScaleMode.Dpi;
            listStatus = new Label[] { labelStatus0, labelStatus1, labelStatus2, labelStatus3, labelStatus4,
                                     labelStatus5, labelStatus6, labelStatus7, labelStatus8, labelStatus9};
            listNamebox = new TextBox[] { textBoxname0, textBoxname1, textBoxname2, textBoxname3, textBoxname4,
                                          textBoxname5, textBoxname6, textBoxname7, textBoxname8, textBoxname9};
            listShader = new CheckBox[] {checkBox1, checkBox2, checkBox3, checkBox4, checkBox5,
                                         checkBox6, checkBox7, checkBox8, checkBox9, checkBox10};
            toolTip1.SetToolTip(textBoxCMD,"add additional command line arguments here like -maploadinfo, seperated by a space");
            toolTip1.SetToolTip(textBoxname0, "Could be anything e.g.the account name'name.1234' or simply an enumeration");
        }
        internal void setEntry(int id, string name, string shader)
        {
            if (File.Exists(Path.Combine(path, "#" + id.ToString() + "Local.dat"))){
                listStatus[id].Visible = true;
                if (name == null){
                    listNamebox[id].Text = "";
                    return;
                }
                else
                    listNamebox[id].Text = name;
                if (shader == "True")
                    listShader[id].Checked = true;
                else
                   listShader[id].Checked = false;
            }
            else
                listStatus[id].Visible = false;
        }
        internal void setPath(string path, int i)
        {
            if (i==0)
                textBoxPath.Text = path;
            if (i == 1)
                textBoxShaderPath.Text = path;
        }
        internal void setCmd(string cmd)
        {
            textBoxCMD.Text = cmd;
        }

        private int getID(object sender)
        {
            TextBox box = sender as TextBox;
            int id = Convert.ToInt32(box.AccessibleName);
            if (id >= 0 && id <= 100)
                return id;
            else
                return 0;
        }
        private int getID_CB(object sender)
        {
            CheckBox box = sender as CheckBox;
            int id = Convert.ToInt32(box.AccessibleName);
            if (id >= 0 && id <= 100)
                return id;
            else
                return 0;
        }
        private int getID_BN(object sender)
        {
            Button box = sender as Button;
            int id = Convert.ToInt32(box.AccessibleName);
            if (id >= 0 && id <= 100)
                return id;
            else
                return 0;
        }
        

        //button click events
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog searchDialog = new OpenFileDialog();
            searchDialog.Filter = "executeable|*.exe;*.bat";
            System.Windows.Forms.DialogResult dr = searchDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                //gw2.exe entry
                textBoxPath.Text = searchDialog.FileName;
                thatParentForm.configPut("path", searchDialog.FileName);

                //reshadeUnlocker.exe entry
                string gw2path = searchDialog.FileName.Substring(0, searchDialog.FileName.LastIndexOf("\\"));
                string unlocker=Path.Combine(gw2path, "ReshadeUnlocker.exe");
                if (File.Exists(unlocker))
                {
                    textBoxShaderPath.Text = unlocker;
                    thatParentForm.configPut("pathUnlocker", unlocker);
                }
                else
                {
                    unlocker = Path.Combine(Environment.CurrentDirectory, "ReshadeUnlocker.exe");
                    if (File.Exists(unlocker))
                    {
                        textBoxShaderPath.Text = unlocker;
                        thatParentForm.configPut("pathUnlocker", unlocker);
                    }
                }

            }
        }

        private void buttonSet0_Click(object sender, EventArgs e)
        {
            //start gw if gw is not running and index is ok 
            Process p = Process.GetProcessesByName("gw2").FirstOrDefault();
            if (p == null)
            {
                int id = getID_BN(sender);
                if (listNamebox[id].Text == "")
                {
                    thatParentForm.configPut("acc." + id.ToString() + ".file", thatParentForm.pid + "#" + id.ToString() + "Local.dat");
                    thatParentForm.configPut("acc." + id.ToString() + ".name", id.ToString());
                }
                else
                {
                    //if (System.Text.RegularExpressions.Regex.IsMatch(listNamebox[id].Text,@"^[\+-.\p{L}\p{N}]+$"))
                    thatParentForm.configPut("acc." + id.ToString() + ".file", thatParentForm.pid + "#" + id.ToString() + "Local.dat");
                    thatParentForm.configPut("acc." + id.ToString() + ".name", listNamebox[id].Text);
                }
                string target=Path.Combine(path, thatParentForm.pid + "#" + id.ToString() + "Local.dat");
                if (!File.Exists(Path.Combine(path, "Local.dat")))
                {
                    MessageBox.Show("No local.dat found, Start GW2 first!");
                    return;
                }

                File.Copy(Path.Combine(path, "Local.dat"), target,true);
                listStatus[id].Visible = true;
            }
            else
                MessageBox.Show("Close Guild Wars 2 first!");
        }
        
        private void shadersCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            int i = getID_CB(sender);
            thatParentForm.configPut("acc."+i.ToString() + ".useshader", ((CheckBox)sender).Checked.ToString());
        }

        private void textBoxname0_TextChanged(object sender, EventArgs e)
        {
            int id = getID(sender);
            thatParentForm.configPut("acc." + id.ToString() + ".name", listNamebox[id].Text);
        }

        private void textBoxPath_TextChanged(object sender, EventArgs e)
        {
            thatParentForm.configPut("path", textBoxPath.Text);
        }

        private void textBoxCMD_TextChanged(object sender, EventArgs e)
        {
            thatParentForm.configPut("cmd", textBoxCMD.Text);
        }

        private void ButtonStartGW_Click(object sender, EventArgs e)
        {
            //start gw if gw is not running and index is ok 
            Process p = Process.GetProcessesByName("gw2").FirstOrDefault();
            if (p == null)
            {
                //start gw
                string path = thatParentForm.ConfigGet("path");
                if (path != "") { 
                    Process gw2pro = new Process();
                    gw2pro.StartInfo.FileName = path; //insert file and path from config
                    if (thatParentForm.ConfigGet("cmd") != "")
                        gw2pro.StartInfo.Arguments += " " + thatParentForm.ConfigGet("cmd");
                    gw2pro.StartInfo.WorkingDirectory = path.Substring(0, path.LastIndexOf("\\"));
                    gw2pro.Start();
                }
            }
        }

        private void textBoxPID_TextChanged(object sender, EventArgs e)
        {
            thatParentForm.pid = textBoxPID.Text;
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            thatParentForm.hlp.Show();
        }

        private void buttonBrowseShader_Click(object sender, EventArgs e)
        {
            OpenFileDialog searchDialog = new OpenFileDialog();
            searchDialog.Filter = "executeable|*.exe;*.bat";
            System.Windows.Forms.DialogResult dr = searchDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                textBoxShaderPath.Text = searchDialog.FileName;
                thatParentForm.configPut("pathUnlocker", searchDialog.FileName);
            }
        }

        private void textBoxShaderPath_TextChanged(object sender, EventArgs e)
        {
            thatParentForm.configPut("pathUnlocker", textBoxShaderPath.Text);
        }
    }
}
