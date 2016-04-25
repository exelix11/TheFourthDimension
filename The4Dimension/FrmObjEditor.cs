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
    public partial class FrmObjEditor : Form
    {
        public LevelObj Value { get; set; }

        public FrmObjEditor(Dictionary<string, object> Lev)
        {
            InitializeComponent();
            Value = new LevelObj();
            Value.Prop = Lev;
        }

        private void FrmObjEditor_Load(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(Value.Prop);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Value.Prop.Remove(propertyGrid1.SelectedGridItem.Label);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FrmAddValue v = new FrmAddValue(Value);
            v.ShowDialog();
            if (v.resName != null && v.resName != "") Value.Prop.Add(v.resName, v.result);
            propertyGrid1.Refresh();
        }
    }
}
