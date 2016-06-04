using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace The4Dimension
{
    public partial class FrmLevNameOpen : Form
    {
        public string res = null;
        public Dictionary<string, string> LevelNameNum = new Dictionary<string, string>();

        public FrmLevNameOpen(Dictionary<string, string> s )
        {
            InitializeComponent();
            LevelNameNum = s;
            listBox1.Items.AddRange(s.Keys.ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            res = null;
            this.Close();
        }

        private void ListBox_DoubleClick(object sender, MouseEventArgs e)
        {
            res = listBox1.SelectedItem.ToString();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            res = listBox1.SelectedItem.ToString();
            this.Close();
        }
    }
}
