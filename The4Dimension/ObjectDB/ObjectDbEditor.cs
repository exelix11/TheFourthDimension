using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        List<string[]> objs = new List<string[]>();
        List<string> Names = new List<string>();
        DataSet database;
        bool changing = false;

        public ObjectDbEditor(DataSet db)
        {
            InitializeComponent();
            database = db;
        }

        private void ObjectDbView_Load(object sender, EventArgs e)
        {
            foreach (DataRow row in database.Tables[1].Rows)
            {
                string[] t = new string[5];
                for (int i = 0; i < 5; i++) t[i] = (string)row.ItemArray[i];
                objs.Add(t);
                Names.Add(objs[objs.Count - 1][0]);
                listBox1.Items.Add(Names[Names.Count - 1]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (int i in listBox1.SelectedIndices)
            {
                int actualIndex = Names.IndexOf(listBox1.Items[i].ToString());
                objs.RemoveAt(actualIndex);
                Names.RemoveAt(actualIndex);
                listBox1.Items.RemoveAt(i);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            objs.Add(new string[5] { "NewObj", "", "", "", "" });
            Names.Add("NewObj");
            listBox1.ClearSelected();
            listBox1.SelectedIndex = listBox1.Items.Add("NewObj");
        }        

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndices.Count > 1 || listBox1.SelectedIndex == -1)
            {
                groupBox1.Enabled = false;
            }
            else
            {
                changing = true;
                groupBox1.Enabled = true;
                int actualIndex = Names.IndexOf(listBox1.SelectedItem.ToString());
                textBox1.Text = objs[actualIndex][0];
                textBox2.Text = objs[actualIndex][1];
                textBox3.Text = objs[actualIndex][2];
                textBox4.Text = objs[actualIndex][3];
                textBox5.Text = objs[actualIndex][4];
                changing = false;
            }
        }        

        private void button3_Click(object sender, EventArgs e)
        {
            database.Tables[0].Rows[0][0] = Int32.Parse(Application.ProductVersion.Replace(".", ""));
            database.Tables.RemoveAt(1);
            DataTable tb = new DataTable("Objects");
            tb.Columns.Add("InGameName");
            tb.Columns.Add("ModelName");
            tb.Columns.Add("ShortDescription");
            tb.Columns.Add("LongDescription");
            tb.Columns.Add("Author");
            foreach (string[] obj in objs)
            {
                tb.Rows.Add(obj[0], obj[1], obj[2], obj[3], obj[4]);
            }
            database.Tables.Add(tb);
            File.WriteAllText("ObjectsDb.xml", database.GetXml());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (changing) return;
            int actualIndex = Names.IndexOf(listBox1.SelectedItem.ToString());
            objs[actualIndex][0] = textBox1.Text;
            objs[actualIndex][1] = textBox2.Text;
            objs[actualIndex][2] = textBox3.Text;
            objs[actualIndex][3] = textBox4.Text;
            objs[actualIndex][4] = textBox5.Text;
            int index = listBox1.SelectedIndex;
            listBox1.ClearSelected();
            Names[actualIndex] = textBox1.Text;
            listBox1.Items[index] = textBox1.Text;
        }
    }
}
