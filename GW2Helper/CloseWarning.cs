using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GW2Helper
{
    public partial class CloseWarning : Form
    {
        internal MainWindow thatParentForm { get; set; }
        public CloseWarning()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
        }

        public CloseWarning(int id)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();

        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (thatParentForm != null) {
                Hide();
                thatParentForm.Close();
            }
        }
    }
}