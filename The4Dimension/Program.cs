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
                    if (Cont.StartsWith("<?xml")) System.IO.File.WriteAllBytes(Args[0] + ".byml", BymlConverter.GetByml(Cont));
                    else if (Cont.StartsWith("YB")) System.IO.File.WriteAllText(Args[0] + ".xml", BymlConverter.GetXml(Args[0]), Encoding.GetEncoding(932));
                    else Application.Run(new Form1(Args[0].Trim()));
                }
            }
            else
            {
                    Application.Run(new Form1());                
            }
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
