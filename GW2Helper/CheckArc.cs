using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Net;

namespace GW2Helper
{
    public partial class CheckArc : Form
    {
        private string d3d9dll, d3d9old, gw2dat, d3d9btdll, d3d9btold;
        public string path;
        internal MainWindow thatParentForm { get; set; }


        public CheckArc()
        {
            InitializeComponent();
        }
        //cancel button
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //restore button
        private void button5_Click(object sender, EventArgs e)
        {

            if (File.Exists(d3d9btold))
            {
                DateTime date = File.GetCreationTimeUtc(d3d9btold);
                if (File.Exists(d3d9btdll))
                {
                    File.Delete(d3d9btdll);
                }
                File.Move(d3d9btold, d3d9btdll);
                File.SetCreationTimeUtc(d3d9btdll, date);
            }
            if (File.Exists(d3d9old))
            {
                DateTime date = File.GetCreationTimeUtc(d3d9old);
                if (File.Exists(d3d9dll))
                {
                    File.Delete(d3d9dll);
                }
                File.Move(d3d9old, d3d9dll);
                File.SetCreationTimeUtc(d3d9dll, date);
            }
            refreshDateLocal();
        }

        //refresh button
        private void button4_Click(object sender, EventArgs e)
        {
            path = thatParentForm.getPath();
            if (!String.IsNullOrEmpty(path))
            {
                refreshDateLocal();
                refreshDateOnline();
            }
            else
                MessageBox.Show("GW2 Folder not found!");
        }
        //backup button
        private void button2_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(gw2dat)||!File.Exists(gw2dat))
            {
                MessageBox.Show("GW2 Folder not found");
                return;
            }
            if (File.Exists(d3d9btdll))
            {
                DateTime date = File.GetCreationTimeUtc(d3d9btdll);
                if (File.Exists(d3d9btold))
                {
                    File.Delete(d3d9btold);
                }
                File.Move(d3d9btdll, d3d9btold);
                File.SetCreationTimeUtc(d3d9btold, date);
            }
            if (File.Exists(d3d9dll))
            {
                DateTime date = File.GetCreationTimeUtc(d3d9dll);
                if (File.Exists(d3d9old))
                {
                    File.Delete(d3d9old);
                }
                File.Move(d3d9dll, d3d9old);
                File.SetCreationTimeUtc(d3d9old,date);
            }
            refreshDateLocal();
        }
        //download button
        private void button3_Click(object sender, EventArgs e)
        {

            if (File.Exists(d3d9dll))
                File.Delete(d3d9dll);

            if (File.Exists(d3d9btdll))
                File.Delete(d3d9btdll);
            String htmlCode, htmlCode2;

            using (WebClient client = new WebClient())
            {
                htmlCode = client.DownloadString("https://www.deltaconnected.com/arcdps/x64/");
                htmlCode2 = client.DownloadString("https://www.deltaconnected.com/arcdps/x64/buildtemplates/");
            }

            using (var client = new WebClient())
            {
                client.DownloadFile("https://www.deltaconnected.com/arcdps/x64/d3d9.dll", d3d9dll);
                client.DownloadFile("https://www.deltaconnected.com/arcdps/x64/buildtemplates/d3d9_arcdps_buildtemplates.dll", d3d9btdll);
            }

            if (File.Exists(d3d9dll) && !String.IsNullOrEmpty(htmlCode))
            {
                File.SetCreationTimeUtc(d3d9dll, getDate(htmlCode, "d3d9"));
            }
            if (File.Exists(d3d9btdll) && !String.IsNullOrEmpty(htmlCode2))
            {
                File.SetCreationTimeUtc(d3d9btdll, getDate(htmlCode2, "d3d9_arcdps_buildtemplates.dll"));
            }

            refreshDateLocal();
        }

        internal void refreshDateLocal()
        {
            if (String.IsNullOrEmpty(path))
            {
                MessageBox.Show("Select GW2 folder first!");
                return;
            }

            gw2dat = Path.Combine(path.Substring(0, path.LastIndexOf("\\")), "gw2.dat");
            d3d9dll = Path.Combine(path.Substring(0, path.LastIndexOf("\\")), "bin64\\d3d9.dll");
            d3d9old = Path.Combine(path.Substring(0, path.LastIndexOf("\\")), "bin64\\d3d9.old");
            d3d9btdll = Path.Combine(path.Substring(0, path.LastIndexOf("\\")), "bin64\\d3d9_arcdps_buildtemplates.dll");
            d3d9btold = Path.Combine(path.Substring(0, path.LastIndexOf("\\")), "bin64\\d3d9_arcdps_buildtemplates.old");

            if (File.Exists(d3d9dll))
                label_date_d3d9_local.Text = File.GetCreationTimeUtc(d3d9dll).ToShortDateString();
            else
                label_date_d3d9_local.Text = "-";
            if (File.Exists(d3d9old))
                label_date_d3d9_old.Text = File.GetCreationTimeUtc(d3d9old).ToShortDateString();
            else
                label_date_d3d9_old.Text = "-";

            if (File.Exists(d3d9btdll))
                label_date_bt_local.Text = File.GetCreationTimeUtc(d3d9btdll).ToShortDateString();
            else
                label_date_bt_local.Text = "-";
            if (File.Exists(d3d9btold))
                label_date_bt_old.Text = File.GetCreationTimeUtc(d3d9btold).ToShortDateString();
            else
                label_date_bt_old.Text = "-";
        }
        internal void refreshDateOnline()
        {
            String htmlCode, htmlCode2;

            using (WebClient client = new WebClient())
            {
                htmlCode = client.DownloadString("https://www.deltaconnected.com/arcdps/x64/");
                htmlCode2 = client.DownloadString("https://www.deltaconnected.com/arcdps/x64/buildtemplates/");
            }

            label_date_d3d9_online.Text = getDate(htmlCode,"d3d9").ToShortDateString();
            label_date_bt_online.Text = getDate(htmlCode2, "d3d9_arcdps_buildtemplates.dll").ToShortDateString();
            
        }
        private DateTime getDate(String htmlCode, String searchValue)
        {
            DateTime date = new DateTime();
            if (htmlCode != null)
            {
                string date_str = htmlCode.Substring(htmlCode.IndexOf(searchValue));
                date_str = date_str.Substring(date_str.IndexOf("2"), 10);
                DateTime.TryParse(Int32.Parse(date_str.Substring(8, 2)) + "."+Int32.Parse(date_str.Substring(5, 2)) + "."+Int32.Parse(date_str.Substring(2, 2)),out date);
            }

            return date;
        }
    }
}
