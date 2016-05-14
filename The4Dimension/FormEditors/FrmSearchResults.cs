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
    public partial class FrmSearchResults : Form
    {
        Form1 owner;
        string Type;
        List<string> HitNames;
        List<int> HitIndexes;
        public FrmSearchResults(string _Type, List<string> res_names,  List<int> res_indexes, Form1 _owner)
        {
            InitializeComponent();
            owner = _owner;
            Type = _Type;
            HitNames = res_names;
            HitIndexes = res_indexes;
            listBox1.Items.AddRange(HitNames.ToArray());
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            owner.SetSelectedObj(Type, HitIndexes[listBox1.SelectedIndex]);
            owner.Focus();
        }
    }
}
