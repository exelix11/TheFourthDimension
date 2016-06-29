using BymlFormat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace The4Dimension
{
    class BymlConverter
    {
        BymlFile LoadedFile;

        public static string GetXml(byte[] Data)
        {
            BymlConverter app = new BymlConverter();
            app.LoadedFile = new BymlFile(Data);            
            return app.exportToXml(app.LoadedFile);
        }

        public static byte[] GetByml(string XmlText)
        {
            BymlConverter app = new BymlConverter();
            app.LoadedFile = app.ImportFromXml(XmlText);
            return app.LoadedFile.MakeFile(app.LoadedFile.header.BigEndian);
        }

        public static byte[] GetBymlFrom(string XmlText)
        {
            BymlConverter app = new BymlConverter();
            app.LoadedFile = app.ImportFromXml(XmlText);
            return app.LoadedFile.MakeFile(app.LoadedFile.header.BigEndian);
        }

        public static string GetXml(string Path)
        {
            return GetXml(File.ReadAllBytes(Path));
        }

        public static string GetHexString(int num)
        {
            return num.ToString("X");
        }

        #region exportXML        
        string exportToXml(BymlFile File)
        {
            CustomStringWriter str = new CustomStringWriter(Encoding.GetEncoding(932));
            XmlTextWriter xr;
            xr = new XmlTextWriter(str);
            xr.Formatting = System.Xml.Formatting.Indented;
            xr.WriteStartDocument();
            xr.WriteStartElement("Root");
            xr.WriteStartElement("isBigEndian");
            xr.WriteAttributeString("Value", File.header.BigEndian.ToString());
            xr.WriteEndElement();
            xr.WriteStartElement("BymlFormatVersion");
            xr.WriteAttributeString("Value", File.header.Version.ToString());
            xr.WriteEndElement();
            GenericNode RootNode = File.RootNode;
            xr.WriteStartElement(RootNode.NodeType == (byte)0xC1 ? "C1" : "C0");
            saveNode(RootNode.SubNodes.ToArray(), xr);
            xr.WriteEndElement();
            xr.WriteEndElement();
            xr.Close();
            return str.ToString();
        }

        void saveNode(GenericNode[] tnc, XmlTextWriter xr)
        {
            for (int i = 0; i < tnc.Length; i++)
            {
                if (tnc[i].NodeType == 0xC1 || tnc[i].NodeType == 0xC0)
                {
                    xr.WriteStartElement(GetHexString(tnc[i].NodeType));
                    if (tnc[i].StringIndex != 0xFFFFFFFF) xr.WriteAttributeString("Name", LoadedFile.NodeNames.Strings[(int)tnc[i].StringIndex]);
                    saveNode(tnc[i].SubNodes.ToArray(), xr);
                    xr.WriteEndElement();
                }
                else
                {
                    xr.WriteStartElement(GetHexString(tnc[i].NodeType));
                    if (tnc[i].StringIndex != 0xFFFFFFFF) xr.WriteAttributeString("Name", LoadedFile.NodeNames.Strings[(int)tnc[i].StringIndex]);
                    xr.WriteAttributeString("StringValue", NodeValueToString(tnc[i]));
                    xr.WriteEndElement();
                }
            }
        }

        string NodeValueToString(GenericNode Node)
        {
            switch (Node.NodeType)
            {
                case (byte)SubNodeValTypes.Int:
                    return BitConverter.ToInt32(Node.Value, 0).ToString();
                case (byte)SubNodeValTypes.Single:
                    return BitConverter.ToSingle(Node.Value, 0).ToString();
                case (byte)SubNodeValTypes.String:
                    int val = (int)BitConverter.ToUInt32(Node.Value, 0);
                    if (val >= LoadedFile.StringRes.Strings.Count) return "Missing string ?";
                    return LoadedFile.StringRes.Strings[val];
                    /*Encoding enc = Encoding.GetEncoding(932);
                    byte[] Data = enc.GetBytes(LoadedFile.StringRes.Strings[(int)BitConverter.ToUInt32(Node.Value, 0)]);
                    return Encoding.Default.GetString(Data);*/
                case (byte)SubNodeValTypes.Empty:
                    return "";
                case (byte)SubNodeValTypes.Boolen:
                    if (Node.Value[0] == 0x01) return "True"; else return "False";
                default:
                    string Value = "";
                    for (int ii = 0; ii < Node.Value.Length; ii++)
                    {
                        string v = GetHexString(Node.Value[ii]);
                        if (v.Length == 1) v = "0" + v;
                        Value += v;
                    };
                    return Value;
            }
        }
        #endregion

        #region ImportXML
        BymlFile ImportFromXml(string file)
        {
            BymlFile ret = new BymlFile();
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(file);
            XmlNode n = xml.SelectSingleNode("/Root/isBigEndian");
            ret.header = new Header();
            ret.header.BigEndian = Convert.ToBoolean(n.Attributes["Value"].Value);
            n = xml.SelectSingleNode("/Root/BymlFormatVersion");
            if (n == null) ret.header.Version = 1; else ret.header.Version = UInt16.Parse(n.Attributes["Value"].Value);
            n = xml.SelectSingleNode("/Root");
            ret.RootNode.NodeType = n.LastChild.Name == "C1" ? (byte)0xC1 : (byte)0xC0;
            ret.RootNode.SubNodes.AddRange(XmlToNode(n.LastChild.ChildNodes));
            ProcessStrings(ref ret, ret.RootNode);
            List<string> tmp = new List<string>();

            for (int i = 0; i < ret.StringRes.Strings.Count; i++) ret.StringRes.Strings[i] = StringToJapChar(ret.StringRes.Strings[i]); //Some tricks to get the right order, i HATE text encondings
            for (int i = 0; i < ret.NodeNames.Strings.Count; i++) ret.NodeNames.Strings[i] = StringToJapChar(ret.NodeNames.Strings[i]);
            ret.StringRes.Strings.Sort(StringComparer.Ordinal);
            ret.NodeNames.Strings.Sort(StringComparer.Ordinal);
            if (ret.StringRes.Strings.Count == 0) ret.StringRes.IsNull = true;
            if (ret.NodeNames.Strings.Count == 0) ret.NodeNames.IsNull = true;
            for (int i = 0; i < ret.StringRes.Strings.Count; i++) ret.StringRes.Strings[i] = StringFromXml(ret.StringRes.Strings[i]);
            for (int i = 0; i < ret.NodeNames.Strings.Count; i++) ret.NodeNames.Strings[i] = StringFromXml(ret.NodeNames.Strings[i]);
            SetStringsIndexes(ref ret, ret.RootNode);
            return ret;
        }

        string StringFromXml(string input)
        {
            Encoding enc = Encoding.GetEncoding(932);
            byte[] Data = enc.GetBytes(input);
            return Encoding.Default.GetString(Data);
        }

        string StringToJapChar(string input)
        {
            Encoding enc = Encoding.GetEncoding(932);
            byte[] Data = Encoding.Default.GetBytes(input);
            return enc.GetString(Data);
        }

        GenericNode[] XmlToNode(XmlNodeList xml)
        {
            List<GenericNode> ret = new List<GenericNode>();
            for (int i = 0; i < xml.Count; i++)
            {
                XmlNode xNode = xml[i];
                if (xNode.NodeType == XmlNodeType.Element)
                {
                    switch (xNode.Name.ToUpper().Trim())
                    {
                        case "C0":
                            if (xNode.Attributes.Count != 0) ret.Add(new GenericNode(xNode.Attributes["Name"].Value, null, 0xC0)); else ret.Add(new GenericNode(null, null, 0xC0));
                            ret[i].SubNodes.AddRange(XmlToNode(xNode.ChildNodes));
                            break;
                        case "C1":
                            if (xNode.Attributes.Count != 0) ret.Add(new GenericNode(xNode.Attributes["Name"].Value, null, 0xC1)); else ret.Add(new GenericNode(null, null, 0xC1));
                            ret[i].SubNodes.AddRange(XmlToNode(xNode.ChildNodes));
                            break;
                        case "A0":
                            ret.Add(new GenericNode(xNode.Attributes["Name"] != null ? xNode.Attributes["Name"].Value : "", StringFromXml(xNode.Attributes["StringValue"].Value), 0xA0));
                            break;
                        case "A1":
                            ret.Add(new GenericNode(xNode.Attributes["Name"] != null ? xNode.Attributes["Name"].Value : "", xNode.Attributes["StringValue"].Value, 0xA1));
                            break;
                        case "D1":
                            ret.Add(new GenericNode(xNode.Attributes["Name"] != null ? xNode.Attributes["Name"].Value : "", xNode.Attributes["StringValue"].Value, 0xD1));
                            break;
                        case "D2":
                            ret.Add(new GenericNode(xNode.Attributes["Name"] != null ? xNode.Attributes["Name"].Value : "", xNode.Attributes["StringValue"].Value, 0xD2));
                            break;
                        case "D0":
                            ret.Add(new GenericNode(xNode.Attributes["Name"] != null ? xNode.Attributes["Name"].Value : "", xNode.Attributes["StringValue"].Value, 0xD0));
                            break;
                        default:
                            if (xNode.LocalName.Trim().Length == 2)
                            {
                                ret.Add(new GenericNode(xNode.Attributes["Name"] != null ? xNode.Attributes["Name"].Value : "", xNode.Attributes["StringValue"].Value, Convert.ToByte("0x" + xNode.LocalName.Trim(), 16)));
                            }
                            break;
                    }
                }
            }
            return ret.ToArray();
        }

        void ProcessStrings(ref BymlFile File, GenericNode Node)
        {
            for (int i = 0; i < Node.SubNodes.Count; i++)
            {
                if (Node.SubNodes[i].Name != null) if (!File.NodeNames.Strings.Contains(Node.SubNodes[i].Name)) File.NodeNames.Strings.Add(Node.SubNodes[i].Name);
                if (Node.SubNodes[i].NodeType == (byte)SubNodeValTypes.String) { if (!File.StringRes.Strings.Contains(StringFromXml(Node.SubNodes[i].StringValue))) { File.StringRes.Strings.Add(StringFromXml(Node.SubNodes[i].StringValue)); } }
                if (Node.SubNodes[i].NodeType == 0xC0 || Node.SubNodes[i].NodeType == 0xC1) ProcessStrings(ref File, Node.SubNodes[i]);
            }
        }

        void SetStringsIndexes(ref BymlFile File, GenericNode Node)
        {
            for (int i = 0; i < Node.SubNodes.Count; i++)
            {
                if (Node.SubNodes[i].Name != null)
                {
                    Node.SubNodes[i].StringIndex = (uint)File.NodeNames.Strings.IndexOf(Node.SubNodes[i].Name);
                    Node.SubNodes[i].Name = null;
                }
                else Node.SubNodes[i].StringIndex = 0;
                if (Node.SubNodes[i].NodeType == (byte)SubNodeValTypes.String)
                {
                    if (Node.SubNodes[i].StringValue != null && Node.SubNodes[i].StringValue.Trim() != "") Node.SubNodes[i].Value = BitConverter.GetBytes((UInt32)File.StringRes.Strings.IndexOf(StringFromXml(Node.SubNodes[i].StringValue))); else Node.SubNodes[i].Value = new byte[4];
                }
                if (Node.SubNodes[i].NodeType == 0xC0 || Node.SubNodes[i].NodeType == 0xC1) SetStringsIndexes(ref File, Node.SubNodes[i]);
            }
        }
        #endregion
    }
}
