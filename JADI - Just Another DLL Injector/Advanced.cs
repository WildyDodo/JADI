using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JADI___Just_Another_DLL_Injector
{
    public partial class Advanced : Form
    {
        public int InjDelay;
        public bool hide;
        public bool closeInjector;
        public Advanced()
        {
            InitializeComponent();
        }

        private void Advanced_Load(object sender, EventArgs e)
        {
            Text = Application.ProductName + " - Advanced settings | v" + Application.ProductVersion + " [x" + (Environment.Is64BitProcess ? "64" : "86") + "]";
            Icon = new Form1().Icon;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            InjDelay = Convert.ToInt32(numericUpDown1.Value);
            hide = checkBox1.Checked;
            closeInjector = checkBox2.Checked;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
