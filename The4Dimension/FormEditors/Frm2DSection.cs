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
    public partial class Frm2DSection : Form
    {
        public Frm2DSection()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown6.Enabled = checkBox1.Checked;
            checkBox2.Enabled = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 f = (Form1)Application.OpenForms["Form1"];
            List<LevelObj> List = new List<LevelObj>();
            List.AddRange(Generate((int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value, false));
            if (checkBox1.Checked)
            {
                List.AddRange(Generate((int)numericUpDown3.Value, (int)numericUpDown4.Value, (int)numericUpDown5.Value + (int)numericUpDown6.Value,true));
                if (checkBox2.Checked)
                {
                    int X, Y, Z;
                    X = (int)numericUpDown3.Value;
                    Y = (int)(numericUpDown4.Value +( numericUpDown1.Value -1) * 1000 + 500);
                    Z = (int)numericUpDown5.Value + 500;
                    LevelObj BaseObj = new The4Dimension.LevelObj();
                    int HighestId = f.higestID["ObjInfo"];
                    BaseObj.Prop.Add("LayerName", new Node("共通", "A0"));
                    BaseObj.Prop.Add("name", new Node("TransparentWall", "A0"));
                    BaseObj.Prop.Add("dir_x", new Node("90", "D2"));
                    BaseObj.Prop.Add("dir_y", new Node("0", "D2"));
                    BaseObj.Prop.Add("dir_z", new Node("0", "D2"));
                    BaseObj.Prop.Add("pos_x", new Node(X.ToString(), "D2"));
                    BaseObj.Prop.Add("pos_y", new Node(Y.ToString(), "D2"));
                    BaseObj.Prop.Add("pos_z", new Node(Z.ToString(), "D2"));
                    BaseObj.Prop.Add("scale_x", new Node("1", "D2"));
                    BaseObj.Prop.Add("scale_y", new Node("1", "D2"));
                    BaseObj.Prop.Add("scale_z", new Node("1", "D2"));
                    BaseObj.Prop.Add("l_id", new Node("0", "D1"));
                    for (int ix = 0; ix < numericUpDown2.Value; ix++)
                    {
                            LevelObj tmpObj = BaseObj.Clone();
                            ((Node)tmpObj.Prop["pos_x"]).StringValue = (X + ix * 1000).ToString();
                            ((Node)tmpObj.Prop["l_id"]).StringValue = (++HighestId).ToString();
                            List.Add(tmpObj);
                    }
                    f.higestID["ObjInfo"] = HighestId;
                }
            }
            ClipBoardItem cl = new ClipBoardItem();
            cl.Type = ClipBoardItem.ClipboardType.ObjectArray;
            cl.Objs = List.ToArray();
            f.PasteValue(-1, "ObjInfo", cl);
            this.Close();
        }

        List<LevelObj> Generate(int X, int Y, int Z, bool rot)
        {
            List<LevelObj> List = new List<LevelObj>();
            LevelObj BaseObj = new The4Dimension.LevelObj();
            Form1 f = (Form1)Application.OpenForms["Form1"];
            int HighestId = f.higestID["ObjInfo"];
            BaseObj.Prop.Add("LayerName", new Node("共通", "A0"));
            BaseObj.Prop.Add("name", new Node("TransparentWall", "A0"));
            BaseObj.Prop.Add("dir_x", new Node("0" , "D2"));
            BaseObj.Prop.Add("dir_y", new Node(rot ? "180" : "0", "D2"));
            BaseObj.Prop.Add("dir_z", new Node("0", "D2"));
            BaseObj.Prop.Add("pos_x", new Node(X.ToString(), "D2"));
            BaseObj.Prop.Add("pos_y", new Node(Y.ToString(), "D2"));
            BaseObj.Prop.Add("pos_z", new Node(Z.ToString(), "D2"));
            BaseObj.Prop.Add("scale_x", new Node("1", "D2"));
            BaseObj.Prop.Add("scale_y", new Node("1", "D2"));
            BaseObj.Prop.Add("scale_z", new Node("1", "D2"));
            BaseObj.Prop.Add("l_id", new Node("0", "D1"));
            for (int ix = 0; ix < numericUpDown2.Value; ix++)
            {
                for (int iy = 0; iy < numericUpDown1.Value; iy++)
                {
                    LevelObj tmpObj = BaseObj.Clone();
                    ((Node)tmpObj.Prop["pos_x"]).StringValue = (X + ix * 1000).ToString();
                    ((Node)tmpObj.Prop["pos_y"]).StringValue = (Y + iy * 1000).ToString();
                    ((Node)tmpObj.Prop["l_id"]).StringValue = (++HighestId).ToString();
                    List.Add(tmpObj);
                }
            }
            f.higestID["ObjInfo"] = HighestId;
            return List;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
