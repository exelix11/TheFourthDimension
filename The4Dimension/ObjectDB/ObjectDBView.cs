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
        ObjectDb.ObjectDbEntry entry;
        public ObjectDBView(ObjectDb.ObjectDbEntry _entry, string n)
        {
            InitializeComponent();
            entry = _entry;
            this.Text = n;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = checkBox1.Checked;
        }

        private void ObjectDBView_Load(object sender, EventArgs e)
        {

            label1.Text = "";
            if (entry.Complete == 0) label1.Text += "(This object is not fully known)\r\n";
            label1.Text += entry.name + ":\r\n" + entry.notes;
            if (entry.Fields.Count > 0)
            {
                label1.Text += "\r\n\r\nArgs:";
                for (int i = 0; i < entry.Fields.Count; i++)
                {
                    label1.Text += "\r\nArg[" + entry.Fields[i].id + "] Name: " + entry.Fields[i].name + "  Type: " + entry.Fields[i].type + "\r\n  Notes:" + entry.Fields[i].notes + "\r\n  Values:" + entry.Fields[i].values;
                }
            }
            label1.Text += "\r\n\r\nFiles:" + entry.files;
        }
    }
}
