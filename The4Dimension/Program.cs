using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace The4Dimension
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Args.Length != 0)
            {
                if (Args[0].ToLower() == "ccntpatch")
                {
                    if (File.Exists(@"CreatorClassNameTable.szs") && File.Exists(@"CCNTpatch.xml")) PatchCCNT(Args);
                    else MessageBox.Show("To apply the patch you need both the CreatorClassNameTable.szs and CCNTpatch.xml in this folder !");
                }
                else
                {
                    string Cont = System.IO.File.ReadAllText(Args[0]);
                    if (Cont.StartsWith("<?xml") || Cont.StartsWith("YB") || Cont.StartsWith("BY"))
                    {
                        if (Cont.StartsWith("<?xml")) new FormEditors.FrmXmlEditor(File.ReadAllText(Args[0]), Args[0], true).ShowDialog();
                        else new FormEditors.FrmXmlEditor(BymlConverter.GetXml(Args[0]), Args[0], true).ShowDialog();
                    }
                    else RunApp(Args[0].Trim());
                }
            }
            else
            {
                RunApp();         
            }
        }

        static void RunApp(string arg = "")
        {
            Application.Run(new Form1(arg));
            return;
            /*if (File.Exists("SkipTutorial.txt"))
                Application.Run(new Form1(arg));//Just to be sure
            else if (Properties.Settings.Default.FirstStartup)
            {
                MessageBox.Show("Hey !");
                MessageBox.Show("Looks like this is the first start up of The Fourht Dimension on this pc");
                var res = MessageBox.Show("Would you like to read a tutorial ?\r\nThis will explain you what are the components of every level so you can understand how to make cameras and other objects work and make wonderful levels,the basics of this editor and some tricks to quickly make levels", "", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes) { }
                else
                {
                    MessageBox.Show("You can read the tutorial at any time from Help -> Tutorial");
                }
                Application.Run(new Form1(arg));
                return;
            }
            else
            {
                Application.Run(new Form1(arg));
            }*/
        }

        static void PatchCCNT(string[] Args)
        {
            string message = "";
            if (Args.Length > 1) for (int i = 1; i < Args.Length; i++) message += Args[i] + " ";
            CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
            NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
            SzsArch = new NDS.NitroSystem.FND.NARC(y.Decompress(File.ReadAllBytes(@"CreatorClassNameTable.szs")));
            string ConvertedCCN = BymlConverter.GetXml(SzsArch.ToFileSystem().Files[0].Data);
            Dictionary<string, string> ccnt = LoadCCNT(ConvertedCCN);
            Dictionary<string, string> ccnt2 = LoadCCNT(File.ReadAllText("CCNTpatch.xml", Form1.DefEnc));
            foreach (string k in ccnt2.Keys.ToArray())
            {
                if (!ccnt.ContainsKey(k))
                {
                    ccnt.Add(k, ccnt2[k]);
                }
            }
            FormEditors.FrmCCNTEdit.SaveFile(ref ccnt);
            MessageBox.Show(message + "\r\n\r\nThe4Dimension by Exelix11\r\nEvery File Explorer by Gericom");
        }

        static Dictionary<string,string> LoadCCNT(string inFile)
        {
            Dictionary<string, string> CreatorClassNameTable = new Dictionary<string, string>();            
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(inFile);
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
                CreatorClassNameTable.Add(ObjName, ClassName);
            }
            return CreatorClassNameTable;
        }
    }
}
