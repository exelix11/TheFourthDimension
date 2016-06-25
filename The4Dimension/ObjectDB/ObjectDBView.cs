using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace The4Dimension.ObjectDB
{
    public partial class ObjectDBView : Form
    {
        string[] objEntry;
        public ObjectDBView(object[] obj)
        {
            InitializeComponent();
            objEntry = new string[5];
            for (int i = 0; i < 5; i++) objEntry[i] = (string)obj[i];
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox1.Checked;
        }

        private void ObjectDBView_Load(object sender, EventArgs e)
        {
            label1.Text = (objEntry[3].Trim() == "" ? objEntry[2] : objEntry[3]) + Environment.NewLine + Environment.NewLine + "Model name: " + objEntry[1] + Environment.NewLine + Environment.NewLine + "Found by: " + objEntry[4];
            this.Text = objEntry[0];
        }
    }
}
