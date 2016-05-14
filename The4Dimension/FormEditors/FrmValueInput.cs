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
    public partial class FrmValueInput : Form
    {
        public object Res = null;

        public FrmValueInput()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
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
