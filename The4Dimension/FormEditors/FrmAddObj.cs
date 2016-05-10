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
    public partial class FrmAddObj : Form
    {
        public LevelObj Value { get; set; }
        string LayerName;
        public FrmAddObj(Dictionary<string,string> CCNT, string text)
        {
            InitializeComponent();
            LayerName = text;
            if (LayerName == "StartInfo") { comboBox1.Text = "Mario"; comboBox1.Enabled = false; }
            else if (LayerName == "AreaObjInfo")
            {
                foreach (string s in CCNT.Keys.ToArray())
                {
                    if (s.ToLower().EndsWith("area") && !s.ToLower().Contains("camera")) comboBox1.Items.Add(s);
                }
            }
            else if (LayerName == "CameraAreaObjInfo")
            {
                foreach (string s in CCNT.Keys.ToArray())
                    if (s.ToLower().EndsWith("area") && s.ToLower().Contains("camera")) comboBox1.Items.Add(s);
            }
            else if (LayerName == "ObjInfo")
            {
                foreach (string s in CCNT.Keys.ToArray())
                    if (!s.ToLower().EndsWith("area")) comboBox1.Items.Add(s);
            }
            else if (LayerName == "StartEventObjInfo")
            {
                foreach (string s in CCNT.Keys.ToArray())
                    if (s.ToLower().StartsWith("startevent")) comboBox1.Items.Add(s);
            }
            else comboBox1.Items.AddRange(CCNT.Keys.ToArray());
            Value = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LevelObj obj = new LevelObj();
            if (LayerName != "StartInfo") obj.Prop.Add("Arg", new int[1] { -1 });
            if (LayerName != "StartInfo") obj.Prop.Add("l_id", new Node("0", "D1")); else obj.Prop.Add("MarioNo", new Node("0", "D1"));
            obj.Prop.Add("LayerName", new Node("共通", "A0"));
            obj.Prop.Add("name", new Node(comboBox1.Text, "A0"));
            obj.Prop.Add("dir_x", new Node("0", "D2"));
            obj.Prop.Add("dir_y", new Node("0", "D2"));
            obj.Prop.Add("dir_z", new Node("0", "D2"));
            obj.Prop.Add("pos_x", new Node("0", "D2"));
            obj.Prop.Add("pos_y", new Node("0", "D2"));
            obj.Prop.Add("pos_z", new Node("0", "D2"));
            obj.Prop.Add("scale_x", new Node("1", "D2"));
            obj.Prop.Add("scale_y", new Node("1", "D2"));
            obj.Prop.Add("scale_z", new Node("1", "D2"));
            Value = obj;
            this.Close();
        }

        private void FrmAddObj_Load(object sender, EventArgs e)
        {

        }
    }
}
