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
    public partial class FrmC0ListEdit : Form
    {
        public C0List Value { get; set; }
        public C0List OldValue;
        Form1 owner;

        public FrmC0ListEdit(C0List list)
        {
            InitializeComponent();
            Value = list;
            OldValue = Value.Clone();
            this.Text = "Edit C0List";
        }

        private void FrmC0ListEdit_Load(object sender, EventArgs e)
        {
            owner = (Form1)Application.OpenForms["Form1"];
            foreach (LevelObj o in Value.List) listBox1.Items.Add(o.ToString());
            if (listBox1.Items.Count > 0) listBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FrmAddObj frm = new FrmAddObj(owner.CreatorClassNameTable, "");
            frm.ShowDialog();
            if (frm.Value == null) return;
            Value.List.Add(frm.Value);
            listBox1.Items.Add(frm.Value.ToString());
            if (owner.propertyGrid1.SelectedGridItem.Label == "GenerateChildren") owner.AddChildrenModels(Value);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            Value.List.RemoveAt(listBox1.SelectedIndex);
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            if (owner.propertyGrid1.SelectedGridItem.Label == "GenerateChildren") owner.render.RemoveModel("TmpChildrenObjs", listBox1.SelectedIndex);
        }

        private void ListBox_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            FrmObjEditor f = new FrmObjEditor(Value.List[listBox1.SelectedIndex].Prop);
            f.ShowDialog();
            Value.List[listBox1.SelectedIndex] = f.Value;
            listBox1.Items[listBox1.SelectedIndex] = f.Value.ToString();
            List<LevelObj> tmp = Value.List;
            owner.UpdateOBJPos(listBox1.SelectedIndex, ref tmp, "TmpChildrenObjs");
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            owner.C0ListChanged(OldValue);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            if (owner.propertyGrid1.SelectedGridItem.Label == "GenerateChildren") owner.render.CameraToObj("TmpChildrenObjs", listBox1.SelectedIndex);
        }
    }
}
