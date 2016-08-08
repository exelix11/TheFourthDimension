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
using System.Xml;

namespace The4Dimension.ObjectDB
{
    public partial class GameObjectDumper : Form
    {
        string[] LevelList;
        List<FoundObj[]> result = new List<FoundObj[]>();
        List<string> Stages = new List<string>();
        Form1 own;
        public GameObjectDumper(string[] l, Form1 f)
        {
            InitializeComponent();
            LevelList = l;
            own = f;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "Xml file|*.xml";
            if (s.ShowDialog() != DialogResult.OK) return;
            LevelList = new string[listBox1.Items.Count];
            listBox1.Items.CopyTo(LevelList,0);
            progressBar1.Maximum = LevelList.Length;
            richTextBox1.Text = "LOG:\r\n";
            button1.Enabled = false;
            result = new List<FoundObj[]>();
            for (int i = 0; i < LevelList.Length; i++)
            {
                progressBar1.Value = i;
                label3.Text = "Processing " + LevelList[i];
                richTextBox1.Text += "Processing " + LevelList[i] + "\r\n";
                Refresh();
                if (File.Exists(Properties.Settings.Default.GamePath + "\\StageData\\" + own.LevelNameNum[LevelList[i]]))
                {
                    own.LoadFile(Properties.Settings.Default.GamePath + "\\StageData\\" + own.LevelNameNum[LevelList[i]]);
                    result.Add(ProcessLevel(LevelList[i]));
                    Stages.Add(LevelList[i]);
                    richTextBox1.Text += LevelList[i] + " Done !\r\n";
                }
                else richTextBox1.Text += "ERROR: Filed to load " + LevelList[i] + " : the file doesn't exist\r\n";
            }
            SaveAsXml(s.FileName);
            button1.Enabled = true;
            label3.Text = "Done";
            progressBar1.Value = 0;
        }

        void SaveAsXml(string name)
        {
            using (var stream = new MemoryStream())
            {
                using (var xr = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true}))
                {
                    xr.WriteStartDocument();
                    xr.WriteStartElement(textBox1.Text);
                    for(int i = 0; i < Stages.Count; i++)
                    {                        
                        xr.WriteStartElement(Stages[i].Replace(' ', '_'));
                        foreach (FoundObj o in result[i])
                        {
                            xr.WriteStartElement("Object");
                            xr.WriteStartElement("Args");
                            for (int ii = 0; ii < o.Args.Count; ii++) xr.WriteAttributeString("Arg" + ii.ToString(), o.Args[ii].ToString());
                            xr.WriteEndElement();
                            xr.WriteStartElement("Children_objects");
                            for (int ii = 0; ii < o.ChildrenObj.Count; ii++) xr.WriteAttributeString("ChildrenN" + ii.ToString(), o.ChildrenObj[ii]);
                            xr.WriteEndElement();
                            xr.WriteStartElement("Children_Areas");
                            for (int ii = 0; ii < o.ChildrenArea.Count; ii++) xr.WriteAttributeString("ChildrenN " + ii.ToString(), o.ChildrenArea[ii]);
                            xr.WriteEndElement();
                            xr.WriteStartElement("IDs_list");
                            xr.WriteAttributeString("l_id", string.Join("," ,o.Ids));
                            xr.WriteEndElement();
                            xr.WriteEndElement();
                        }
                        xr.WriteEndElement();
                    }
                    xr.WriteEndElement();
                    xr.WriteEndDocument();
                    xr.Close();
                }
                File.WriteAllBytes(name, stream.ToArray());
            }
        }

        FoundObj[] ProcessLevel(string CurrentLevel)
        {
            if (!own.AllInfos.ContainsKey(comboBox1.Text))
            {
                richTextBox1.Text += comboBox1.Text + " type not found in this level\r\n";
                return null;
            }
            AllInfoSection currentSection = own.AllInfos[comboBox1.Text];
            List<FoundObj> inCurrentLevel = new List<FoundObj>();
            foreach (LevelObj o in currentSection.Objs)
            {
                if (o.ToString() != textBox1.Text) continue;
                FoundObj obj = FoundObj.Parse(o);
                int i = inCurrentLevel.IndexOf(obj);
                if (i >= 0) inCurrentLevel[i].Ids.Add(obj.Ids[0]);
                else inCurrentLevel.Add(obj);
            }
            return inCurrentLevel.ToArray();
        }

        private void GameObjectDumper_Load(object sender, EventArgs e)
        {
            listBox1.Items.AddRange(LevelList);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int[] indexes = GetSelectedIndexes();
            if (indexes[0] == -1) return;
            foreach (int i in indexes)
            {
                listBox1.Items.RemoveAt(i);
            }
        }

        int[] GetSelectedIndexes()
        {
            if (listBox1.SelectedItems.Count == 0) return new int[] { -1 };
            int[] res = new int[listBox1.SelectedItems.Count];
            for (int i = 0; i < listBox1.SelectedItems.Count; i++) res[i] = listBox1.SelectedIndices[i];
            return res.Reverse().ToArray(); //From the last to the first
        }
    }

    class FoundObj : IEquatable<FoundObj>
    {
        public List<int> Args = new List<int>();
        public List<string> ChildrenObj = new List<string>();
        public List<string> ChildrenArea = new List<string>();
        public List<string> Ids = new List<string>();

        public bool Equals(FoundObj other)
        {
            if (other.Args.SequenceEqual(Args)
                && other.ChildrenObj.SequenceEqual(ChildrenObj)
                && other.ChildrenObj.SequenceEqual(ChildrenArea))
                return true;
            else return false;
        }

        public static FoundObj Parse(LevelObj o)
        {
            FoundObj res = new FoundObj();
            if (o.Prop.ContainsKey("l_id")) res.Ids.Add(((Node)o.Prop["l_id"]).StringValue);
            if (o.Prop.ContainsKey("Arg")) res.Args.AddRange((int[])o.Prop["Arg"]);
            if (o.Prop.ContainsKey("GenerateChildren"))
            {
                foreach (LevelObj ob in ((C0List)o.Prop["GenerateChildren"]).List)
                {
                    res.ChildrenObj.Add(ob.ToString());
                }
            }
            if (o.Prop.ContainsKey("AreaChildren"))
            {
                foreach (LevelObj ob in ((C0List)o.Prop["AreaChildren"]).List)
                {
                    res.ChildrenObj.Add(ob.ToString());
                }
            }
            return res;
        }
    }
}
