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

namespace The4Dimension.BgmEditors
{
    public partial class FrmBgmMain : Form
    {
        Dictionary<string, byte[]> SzsFiles = new Dictionary<string, byte[]>();
        Dictionary<string, string> LevelsNum = new Dictionary<string, string>(); //WX-X, LevelName
        Dictionary<string, string> Levels = new Dictionary<string, string>(); //LevelName, Music
        List<string> Music = new List<string>();

        public FrmBgmMain(Dictionary<string, string> NametoNum)
        {
            InitializeComponent();
            LevelsNum = NametoNum;
        }

        private void FrmBgmMain_Load(object sender, EventArgs e)
        {
            CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
            NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
            SzsArch = new NDS.NitroSystem.FND.NARC(y.Decompress(File.ReadAllBytes(@"BgmTable.szs")));
            foreach (LibEveryFileExplorer.Files.SimpleFileSystem.SFSFile file in SzsArch.ToFileSystem().Files) SzsFiles.Add(file.FileName, file.Data);

            string ConvertedXml = BymlConverter.GetXml(SzsFiles["StageDefaultBgmList.byml"]);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(ConvertedXml);
            XmlNode nodes = xml.SelectSingleNode("/Root/C1/C0");
            foreach (XmlNode no in nodes.ChildNodes)
            {
                int scenario = 0;
                string name = "";
                string music = "";
                foreach (XmlNode n in no.ChildNodes)
                {
                    if (n.Attributes["Name"].Value == "Scenario") scenario = Int32.Parse(n.Attributes["StringValue"].Value);
                    else if (n.Attributes["Name"].Value == "StageName") name = n.Attributes["StringValue"].Value;
                    else if (n.Attributes["Name"].Value == "BgmLabel") music = n.Attributes["StringValue"].Value;
                }
                name = name + "Map" + scenario.ToString() + ".szs";
                Levels.Add(name, music);
                if (!Music.Contains(music)) Music.Add(music);
            }

            foreach (string k in LevelsNum.Keys.ToArray()) listBox1.Items.Add(k + " (" + LevelsNum[k] + ")");
            comboBox1.Items.AddRange(Music.ToArray());
            listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                comboBox1.Enabled = false;
                return;
            }
            comboBox1.Enabled = true;
            comboBox1.Text = Levels[LevelsNum.Values.ToArray()[listBox1.SelectedIndex]];
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                comboBox1.Enabled = false;
                return;
            }
            else
            {
                Levels[LevelsNum.Values.ToArray()[listBox1.SelectedIndex]] = comboBox1.Text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (var stream = new MemoryStream())
            {
                using (var xr = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, Encoding = Form1.DefEnc }))
                {
                    xr.WriteStartDocument();
                    xr.WriteStartElement("Root");
                    xr.WriteStartElement("isBigEndian");
                    xr.WriteAttributeString("Value", "False");
                    xr.WriteEndElement();
                    xr.WriteStartElement("BymlFormatVersion");
                    xr.WriteAttributeString("Value", ((uint)1).ToString());
                    xr.WriteEndElement();
                    xr.WriteStartElement("C1");
                    xr.WriteStartElement("C0");
                    xr.WriteAttributeString("Name", "StageDefaultBgmList");
                    foreach (string k in Levels.Keys.ToArray())
                    {
                        xr.WriteStartElement("C1");
                        xr.WriteStartElement("A0");
                        xr.WriteAttributeString("Name", "BgmLabel");
                        xr.WriteAttributeString("StringValue", Levels[k]);
                        xr.WriteEndElement();
                        xr.WriteStartElement("D1");
                        xr.WriteAttributeString("Name", "Scenario");
                        xr.WriteAttributeString("StringValue", k.Substring(k.Length - 5,1));
                        xr.WriteEndElement();
                        xr.WriteStartElement("A0");
                        xr.WriteAttributeString("Name", "StageName");
                        xr.WriteAttributeString("StringValue", k.Substring(0, k.Length - 8));
                        xr.WriteEndElement();
                        xr.WriteEndElement();
                    }
                    xr.WriteEndElement();
                    xr.WriteEndElement();
                    xr.WriteEndElement();
                    xr.Close();
                }
                    Clipboard.SetText(Form1.DefEnc.GetString(stream.ToArray()));
                SzsFiles["StageDefaultBgmList.byml"] = BymlConverter.GetByml(Form1.DefEnc.GetString(stream.ToArray()));
            }
            CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
            NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
            SFSDirectory dir = new SFSDirectory("", true);
            for (int i = 0; i < SzsFiles.Count; i++)
            {
                SFSFile file = new SFSFile(i, SzsFiles.Keys.ToArray()[i], dir);
                file.Data = SzsFiles.Values.ToArray()[i];
                dir.Files.Add(file);
            }
            SzsArch.FromFileSystem(dir);
            File.WriteAllBytes(@"BgmTable.szs", y.Compress(SzsArch.Write()));
            MessageBox.Show("Done, file was saved as BgmTable.szs in this program folder");
            this.Close();
        }
    }
}
