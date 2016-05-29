using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace The4Dimension.FormEditors
{
    public partial class FrmSearchValInput : Form
    {
        public object Res = null;
        bool IsString;

        public FrmSearchValInput(bool str = false)
        {
            InitializeComponent();
            IsString = str;
            if (str) textBox1.Visible = true; else numericUpDown1.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (IsString) Res = textBox1.Text.Trim(); else
            Res = numericUpDown1.Value;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Res = null;
            this.Close();
        }
    }
}
