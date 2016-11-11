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
            updateListbox();
            own = owner;
            if (listBox1.Items.Count > 0) listBox1.SelectedIndex = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFile(ref CCNT);
            own.LoadCreatorClassNameTable();
            this.Close();
        }

        public static void SaveFile(ref Dictionary<string, string> ccnt_r)
        {
            CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
            NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
            SFSDirectory dir = new SFSDirectory("", true);
            SFSFile StgData = new SFSFile(0, "CreatorClassNameTable.byml", dir);
            StgData.Data = BymlConverter.GetByml(MakeXML(ref ccnt_r));
            dir.Files.Add(StgData);
            SzsArch.FromFileSystem(dir);
            File.Delete("CreatorClassNameTable.szs");
            File.WriteAllBytes("CreatorClassNameTable.szs", y.Compress(SzsArch.Write()));
        }

        void LoadCCNT2(string stringxml)
        {
            CCNT2 = new Dictionary<string, string>();
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

        public static string MakeXML(ref Dictionary<string,string> ccnt_r)
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
            xr.WriteStartElement("BymlFormatVersion");
            xr.WriteAttributeString("Value", ((uint)1).ToString());
            xr.WriteEndElement();
            xr.WriteStartElement("C0"); //Byml Root
            foreach (string k in ccnt_r.Keys.ToArray())
            {
                xr.WriteStartElement("C1");
                xr.WriteStartElement("A0");
                xr.WriteAttributeString("Name", "ClassName");
                xr.WriteAttributeString("StringValue", ccnt_r[k]);
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
            label4.Text = "";
            int[] Selected = GetSelectedObjs(false);
            if (Selected[0] == -1) label4.Text = "";
            else if (Selected.Length > 30 ) label4.Text = "Too many objects selected";
            else
            {
                foreach (int i in Selected)
                    label4.Text += listBox1.Items[i].ToString() + " class: " + CCNT[listBox1.Items[i].ToString()] + "\r\n";
            }
        }

        void updateListbox()
        {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(CCNT.Keys.ToArray());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;            
            foreach (int i in GetSelectedObjs())
            {
                CCNT.Remove(listBox1.Items[i].ToString());
            }
            updateListbox();
        }

        int[] GetSelectedObjs(bool reverse = true)
        {
            if (listBox1.SelectedIndex == -1) return new int[1] { -1 };
            int[] selectedObjs = new int[listBox1.SelectedItems.Count];
            for (int i = 0; i < listBox1.SelectedItems.Count; i++) selectedObjs[i] = listBox1.SelectedIndices[i];
            if (reverse) return selectedObjs.Reverse().ToArray();
            else return selectedObjs;
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
            updateListbox();
            textBox1.Text = "";
            textBox2.Text = "";
        }

        private void FrmCCNTEdit_Load(object sender, EventArgs e)
        {
            //TestCCNTEdit + add multi file stuff
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
            else return;
            foreach (string k in CCNT2.Keys.ToArray())
            {
                if (!CCNT.ContainsKey(k))
                {
                    CCNT.Add(k, CCNT2[k]);
                }
            }
            updateListbox();
            MessageBox.Show("Done");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
            sav.Filter = "Xml file|*.xml";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                Dictionary<string, string> NewCCNT = new Dictionary<string, string>();
                foreach (int i in GetSelectedObjs(false)) NewCCNT.Add(listBox1.Items[i].ToString(), CCNT[listBox1.Items[i].ToString()]);
                File.WriteAllText(sav.FileName, MakeXML(ref NewCCNT), Form1.DefEnc);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Select at least one entry in the list");
                return;
            }
            FolderBrowserDialog d = new FolderBrowserDialog();
            if (d.ShowDialog() == DialogResult.OK)
            {
                if (d.SelectedPath == Directory.GetCurrentDirectory())
                {
                    MessageBox.Show("You can't use this program's directory, select another folder");
                    return;
                }
                if (new DirectoryInfo(d.SelectedPath).GetFiles().Length != 0)
                {
                    if (MessageBox.Show("This folder isn't empty, do you want to continue ?", "Warning", MessageBoxButtons.YesNo) == DialogResult.No) return;
                }
                File.Copy(System.Reflection.Assembly.GetEntryAssembly().Location, d.SelectedPath + "\\patcher.exe", true);
                File.Copy("3DS.dll", d.SelectedPath + "\\3DS.dll", true);
                File.Copy("CommonCompressors.dll", d.SelectedPath + "\\CommonCompressors.dll", true);
                File.Copy("CommonFiles.dll", d.SelectedPath + "\\CommonFiles.dll", true);
                File.Copy("LibEveryFileExplorer.dll", d.SelectedPath + "\\LibEveryFileExplorer.dll", true);
                File.Copy("NDS.dll", d.SelectedPath + "\\NDS.dll", true);
                Dictionary<string, string> NewCCNT = new Dictionary<string, string>();
                foreach (int i in GetSelectedObjs(false)) NewCCNT.Add(listBox1.Items[i].ToString(), CCNT[listBox1.Items[i].ToString()]);
                if (File.Exists(d.SelectedPath + "\\CCNTpatch.xml")) File.Delete(d.SelectedPath + "\\CCNTpatch.xml");
                File.WriteAllText(d.SelectedPath + "\\CCNTpatch.xml", MakeXML(ref NewCCNT), Form1.DefEnc);
                if (File.Exists(d.SelectedPath + "\\Patch script.bat")) File.Delete(d.SelectedPath + "\\Patch script.bat");
                File.WriteAllText(d.SelectedPath + "\\Patch script.bat", Properties.Resources.PatchScript);
                MessageBox.Show("Patch files for selected object(s) generated !");
            }
        }
    }
}
