using LibEveryFileExplorer.Files.SimpleFileSystem;
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

namespace The4Dimension.FormEditors
{
    public partial class FrmCCNTEdit : Form
    {
        Dictionary<string, string> CCNT;
        Dictionary<string, string> CCNT2;
        Form1 own;
        public FrmCCNTEdit(Dictionary<string, string> CreatorClassNameTable, Form1 owner)
        {
            InitializeComponent();
            CCNT = CreatorClassNameTable;
            listBox1.Items.AddRange(CCNT.Keys.ToArray());
            own = owner;
            if (listBox1.Items.Count > 0) listBox1.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
            NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
            SFSDirectory dir = new SFSDirectory("", true);          
            SFSFile StgData = new SFSFile(0, "CreatorClassNameTable.byml", dir);
            StgData.Data = BymlConverter.GetByml(MakeXML());
            dir.Files.Add(StgData);
            SzsArch.FromFileSystem(dir);
            File.Delete("CreatorClassNameTable.szs");
            File.WriteAllBytes("CreatorClassNameTable.szs", y.Compress(SzsArch.Write()));
            own.LoadCreatorClassNameTable();
            this.Close();
        }

        void LoadCCNT2(string stringxml)
        {           
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(stringxml);
            XmlNode n = xml.SelectSingleNode("/Root/C0");
            foreach (XmlNode C1Block in n.ChildNodes)
            {
                string ClassName = "";
                string ObjName = "";
                foreach (XmlNode SubNode in C1Block.ChildNodes)
                {
                    if (SubNode.Attributes["Name"].Value == "ClassName")
                        ClassName = SubNode.Attributes["StringValue"].Value;
                    else ObjName = SubNode.Attributes["StringValue"].Value;
                }
                CCNT2.Add(ObjName, ClassName);
            }
        }

        string MakeXML()
        {
            CustomStringWriter str = new CustomStringWriter(Encoding.Default);
            XmlTextWriter xr;
            xr = new XmlTextWriter(str);
            xr.Formatting = System.Xml.Formatting.Indented;
            xr.WriteStartDocument();
            xr.WriteStartElement("Root");
            xr.WriteStartElement("isBigEndian");
            xr.WriteAttributeString("Value", "False");
            xr.WriteEndElement();
            xr.WriteStartElement("C0"); //Byml Root
            foreach (string k in CCNT.Keys.ToArray())
            {
                xr.WriteStartElement("C1");
                xr.WriteStartElement("A0");
                xr.WriteAttributeString("Name", "ClassName");
                xr.WriteAttributeString("StringValue", CCNT[k]);
                xr.WriteEndElement();
                xr.WriteStartElement("A0");
                xr.WriteAttributeString("Name", "ObjectName");
                xr.WriteAttributeString("StringValue", k);
                xr.WriteEndElement();
                xr.WriteEndElement();
            }
            xr.WriteEndElement();
            xr.WriteEndElement();
            xr.Close();
            return str.ToString();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) label4.Text = "";else
            label4.Text = "Selected object class: " + CCNT[listBox1.SelectedItem.ToString()];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            CCNT.Remove(listBox1.SelectedItem.ToString());
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim() == "" || textBox2.Text.Trim() == "" || textBox1.Text.Contains(" ")|| textBox2.Text.Contains(" "))
            {
                MessageBox.Show("Use a valid name and a valid class name");
                return;
            }
            if (CCNT.ContainsKey(textBox1.Text))
            {
                MessageBox.Show("This object is already in the list");
                return;
            }
            CCNT.Add(textBox1.Text, textBox2.Text);
            listBox1.Items.Add(textBox1.Text);
            textBox1.Text = "";
            textBox2.Text = "";
        }

        private void FrmCCNTEdit_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Title = "Open a file";
            opn.Filter = "Supported formats (.szs, .byml, .xml)|*.szs; *.byml; *.xml|Every file|*.*";
            if (opn.ShowDialog() == DialogResult.OK)
            {
                if (Path.GetExtension(opn.FileName).ToLower() == ".xml")
                {
                    LoadCCNT2(File.ReadAllText(opn.FileName));
                }
                else if (Path.GetExtension(opn.FileName).ToLower() == ".byml")
                {
                    LoadCCNT2(BymlConverter.GetXml(opn.FileName));
                }
                else if (Path.GetExtension(opn.FileName).ToLower() == ".szs")
                {
                    CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
                    NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
                    SzsArch = new NDS.NitroSystem.FND.NARC(y.Decompress(File.ReadAllBytes(opn.FileName)));
                    string ConvertedCCN = BymlConverter.GetXml(SzsArch.ToFileSystem().Files[0].Data);
                    LoadCCNT2(ConvertedCCN);
                }
                else
                {
                    MessageBox.Show("Unknown format !");
                    return;
                }
            }
            foreach (string k in CCNT2.Keys.ToArray())
            {
                if (!CCNT.ContainsKey(k))
                {
                    CCNT.Add(k, CCNT2[k]);
                    listBox1.Items.Add(k);
                }
            }
        }
    }
}
