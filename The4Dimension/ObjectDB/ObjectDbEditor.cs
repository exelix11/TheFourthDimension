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
                listBox1.Items.Add(objs[objs.Count - 1][0]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            foreach (int i in listBox1.SelectedIndices)
            {
                objs.RemoveAt(i);
                listBox1.Items.RemoveAt(i);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            objs.Add(new string[5] { "NewObj", "", "", "", "" });
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
                textBox1.Text = objs[listBox1.SelectedIndex][0];
                textBox2.Text = objs[listBox1.SelectedIndex][1];
                RichtextBox3.Text = objs[listBox1.SelectedIndex][2];
                RichtextBox4.Text = objs[listBox1.SelectedIndex][3];
                textBox5.Text = objs[listBox1.SelectedIndex][4];
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
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (changing) return;
            objs[listBox1.SelectedIndex][0] = textBox1.Text;
            objs[listBox1.SelectedIndex][1] = textBox2.Text;
            objs[listBox1.SelectedIndex][2] = RichtextBox3.Text;
            objs[listBox1.SelectedIndex][3] = RichtextBox4.Text;
            objs[listBox1.SelectedIndex][4] = textBox5.Text;
            int index = listBox1.SelectedIndex;
            listBox1.ClearSelected();
            listBox1.Items[index] = textBox1.Text;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("For names like BlockBrick*, the * means the editor will load the same model for every object with the name starting with BlockBrick, this entry description won't be shown.");
        }
    }
}
