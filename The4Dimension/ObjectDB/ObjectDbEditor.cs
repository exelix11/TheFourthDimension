using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace The4Dimension
{
    public partial class ObjectDbEditor : Form
    {
        ObjectDb database;

        public ObjectDbEditor(ObjectDb db)
        {
            InitializeComponent();
            database = db;
        }

        private void ObjectDbEditor_Load(object sender, EventArgs e)
        {
            listView1.MultiSelect = Debugger.IsAttached;
            button1.Visible = Debugger.IsAttached;
            textBox2.Visible = Debugger.IsAttached;
            comboBox1.Items.AddRange(database.Categories.Values.ToArray());
            comboBox1.Text = "All";
            listView1.View = View.Details;
            listView1.HeaderStyle = ColumnHeaderStyle.None;
            UpdateResults();
        }

        void UpdateResults()
        {
            int Category;
            if (comboBox1.Text == "All") Category = -1;
            else Category = database.Categories.Keys.ToArray()[database.Categories.Values.ToList().IndexOf(comboBox1.Text)];
            List<string> Results = new List<string>();
            if (textBox1.Text.Trim() == "") Results = database.Entries.Keys.ToList();
            else
            {
                foreach (string s in database.Entries.Keys.ToArray())
                {
                    if (s.ToLower().StartsWith(textBox1.Text.Trim().ToLower()) || s.ToLower() == textBox1.Text.Trim().ToLower()) Results.Add(s);
                }
            }
            if (Category != -1)
            {
                List<string> _Results = new List<string>();
                foreach (string s in Results)
                {
                    if (database.Entries[s].Category == Category) _Results.Add(s);
                }
                Results = _Results;
            }
            listView1.Items.Clear();
            foreach (string s in Results)
            {
                listView1.Items.Add(s);
                Color c = Color.Red;
                if (database.Entries[s].Known == 1) c = database.Entries[s].Complete == 0 ? Color.Orange : Color.Green;
                listView1.Items[listView1.Items.Count - 1].ForeColor = c;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateResults();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            UpdateResults();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label4.Text = "";
            if (listView1.SelectedItems.Count != 1) return;
            ObjectDb.ObjectDbEntry entry = database.Entries[listView1.SelectedItems[0].Text];
            if (entry.Known == 0)
            {
                label4.Text += "This object is not documented";
                return;
            }
            if (entry.Complete == 0) label4.Text += "(This object is not fully known)\r\n";
            label4.Text += entry.name + ":\r\n" + entry.notes;
            if (entry.Fields.Count > 0)
            {
                label4.Text += "\r\n\r\nArgs:";
                for (int i = 0; i < entry.Fields.Count; i++)
                {
                    label4.Text += "\r\nArg[" + entry.Fields[i].id + "] Name: " + entry.Fields[i].name + "  Type: " + entry.Fields[i].type + "\r\n  Notes:" + entry.Fields[i].notes + "\r\n  Values:" + entry.Fields[i].values;
                }
            }
            label4.Text += "\r\n\r\nFiles:" + entry.files + "\r\n\r\nObject category: " + database.Categories[entry.Category];
        }

        private void button1_Click(object sender, EventArgs e)
        {//this is for generating the switch for From1.GetModelname(string ObjName)
            string s = "";
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                s += "case \"" + listView1.SelectedItems[i].Text + "\":\r\n";
            }
            s += "return \"models\\" + textBox2.Text + "\";";
            Clipboard.SetText(s);
        }
    }
}
