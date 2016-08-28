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
        ObjectDb database;

        public ObjectDbEditor(ObjectDb db)
        {
            InitializeComponent();
            database = db;
        }

        private void ObjectDbEditor_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(database.Categories.Values.ToArray());            
        }

        void UpdateResults()
        {
            int Category;
            if (comboBox1.Text == "All") Category = -1;
            else Category = database.Categories.Keys.ToArray()[database.Categories.Values.ToList().IndexOf(comboBox1.Text)];
            List<string> Results = new List<string>();

        }
    }
}
