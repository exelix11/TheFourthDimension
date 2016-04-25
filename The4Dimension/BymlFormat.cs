using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BymlFormat
{
    public class BymlFile
    {
        public Header header;
        public StringTable NodeNames;
        public StringTable StringRes;
        public GenericNode RootNode;

        public BymlFile(byte[] data)
        {
            header = new Header(data);
            if (header.NodeNamesOffset != 0) NodeNames = new StringTable(header.NodeNamesOffset, data); else NodeNames = new StringTable(true);
            if (header.StringResOffset != 0) StringRes = new StringTable(header.StringResOffset, data); else StringRes = new StringTable(true);
            BinaryReader bin = new BinaryReader(new MemoryStream(data));
            bin.BaseStream.Position = header.RootOffset;
            byte rootNodeType = bin.ReadByte();
            bin.BaseStream.Position--;
            if (rootNodeType == 0xC0)
                RootNode = new ArrayNode(bin);
            else if (rootNodeType == 0xC1)
                RootNode = new DictionaryNode(bin);
            else
                throw new Exception(string.Format("Root node type {0} not supported", rootNodeType));
            //Debug.Print("NODE NAMES LENGHT: " + NodeNames.Strings.Count);
            //Debug.Print("STRING RES LENGHT: " + StringRes.Strings.Count);
        }

        public BymlFile()
        {
            header = new Header();
            NodeNames = new StringTable();
            StringRes = new StringTable();
            RootNode = new DictionaryNode();
        }

        public byte[] MakeFile()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bin = new BinaryWriter(mem);
            //Header
            bin.Write(new byte[] { 0x59, 0x42, 0x01, 0x00 });
            if (NodeNames != null) bin.Write((UInt32)0x10); else bin.Write((UInt32)0x00); //Offset for names table
            bin.Write((UInt32)0x00); //Temp offset for string resources
            bin.Write((UInt32)0x00); //Temp offset for the root node
            //StringTables
            WriteStringTable(NodeNames, bin, 0x04);
            while ((bin.BaseStream.Position % 4) != 0) bin.Write((byte)0x00); //Padding
            WriteStringTable(StringRes, bin, 0x08);
            //RootNode
            while ((bin.BaseStream.Position % 4) != 0) bin.Write((byte)0x00); //Padding
            //Debug.Print((bin.BaseStream.Position % 4).ToString());
            UInt32 RootNodeStart = (UInt32)bin.BaseStream.Position;
            bin.BaseStream.Position = 0x0C;
            bin.Write(RootNodeStart);
            bin.BaseStream.Position = RootNodeStart;
            WriteNodes(RootNode, bin);
            return mem.ToArray();
        }

        List<GenericNode> WrittenNodes = new List<GenericNode>();
        List<uint> WrittenNodesOffs = new List<uint>();      
        void WriteNodes(GenericNode Node, BinaryWriter bin)
        {
            List<GenericNode> NodesToWrite = new List<GenericNode>();
            List<uint> NodesToWriteOffs = new List<uint>();
            if (Node.NodeType == 0xC0)
            {
                bin.Write(Node.NodeType);
                bin.Write(HelpFunctions.GetUint24((uint)Node.SubNodes.Count));
                if (Node.SubNodes.Count != 0)
                {
                    foreach (GenericNode node in Node.SubNodes) bin.Write((byte)node.NodeType);
                    while ((bin.BaseStream.Position % 4) != 0) bin.Write((byte)0x00); //Padding
                    for (int i = 0; i < Node.SubNodes.Count; i++)
                    {
                        if (Node.SubNodes[i].NodeType == 0xC0 || Node.SubNodes[i].NodeType == 0xC1)
                        {
                            NodesToWrite.Add(Node.SubNodes[i]);
                            NodesToWriteOffs.Add((uint)bin.BaseStream.Position);
                            bin.Write(new byte[4]);
                        }
                        else bin.Write(Node.SubNodes[i].Value);
                    }
                }
            }
            else if (Node.NodeType == 0xC1)
            {
                bin.Write(Node.NodeType);
                bin.Write(HelpFunctions.GetUint24((uint)Node.SubNodes.Count));
                if (Node.SubNodes.Count != 0)
                {
                    for (int i = 0; i < Node.SubNodes.Count; i++)
                    {
                        bin.Write(HelpFunctions.GetUint24(Node.SubNodes[i].StringIndex));
                        bin.Write(Node.SubNodes[i].NodeType);
                        if (Node.SubNodes[i].NodeType == 0xC0 || Node.SubNodes[i].NodeType == 0xC1)
                        {
                            NodesToWrite.Add(Node.SubNodes[i]);
                            NodesToWriteOffs.Add((uint)bin.BaseStream.Position);
                            bin.Write(new byte[4]);
                        }
                        else bin.Write(Node.SubNodes[i].Value);
                    }
                }
            }
            else throw new Exception(String.Format("Node type {0} not supported", HelpFunctions.GetHexString(Node.NodeType)));
            for (int i = 0; i < NodesToWrite.Count; i++)
            {
                int ind = GetIndexInWrittenNodes(NodesToWrite[i]);
                //Debug.Print(ind +"   "+ NodesToWrite[i].StringIndex.ToString());
                if (ind == -1)
                {
                    WrittenNodes.Add(NodesToWrite[i]);
                    WrittenNodesOffs.Add((uint)bin.BaseStream.Position);
                    uint pos = (uint)bin.BaseStream.Position;
                    bin.BaseStream.Position = NodesToWriteOffs[i];
                    bin.Write(pos);
                    bin.BaseStream.Position = pos;
                    WriteNodes(NodesToWrite[i], bin);
                }
                else
                {
                    uint pos = (uint)bin.BaseStream.Position;
                    bin.BaseStream.Position = NodesToWriteOffs[i];
                    bin.Write(WrittenNodesOffs[ind]);
                    bin.BaseStream.Position = pos;
                }
            }
        }

        int GetIndexInWrittenNodes(GenericNode G)
        {
            for (int i = 0; i < WrittenNodes.Count; i++)
            {
                if (WrittenNodes[i].Equals(G)) return i;
            }
            return -1;
        }


        void debugData(BinaryWriter bin) //Prints last 16 bytes of the stream
        {
            BinaryReader rd = new BinaryReader(bin.BaseStream);
            rd.BaseStream.Position = rd.BaseStream.Length - 16;
            byte[] data = rd.ReadBytes(16);
            HelpFunctions.debugBytes(data);
        }

        void WriteStringTable(StringTable tbl, BinaryWriter bin, UInt32 UpdateOffset)
        {
            //Debug.Print(tbl.IsNull.ToString());
            if (tbl.IsNull)
            {
                UInt32 OldOffset = (UInt32)bin.BaseStream.Position;
                bin.BaseStream.Position = UpdateOffset;
                bin.Write((UInt32)0);
                bin.BaseStream.Position = OldOffset;
                return;
            }
            else
            {
                UInt32 TableOffset = (UInt32)bin.BaseStream.Position;
                bin.BaseStream.Position = UpdateOffset;
                bin.Write(TableOffset);
                bin.BaseStream.Position = TableOffset;
                bin.Write((byte)0xC2);
                bin.Write(HelpFunctions.GetUint24((uint)tbl.Strings.Count));
                UInt32 BaseOffList = (UInt32)bin.BaseStream.Position;
                for (int i = 0; i < tbl.Strings.Count + 1; i++) bin.Write((UInt32)0x00); //Space for offsets
                for (int i = 0; i < tbl.Strings.Count; i++)
                {
                    UInt32 StrOffset = (UInt32)bin.BaseStream.Position;
                    bin.Write(Encoding.Default.GetBytes(tbl.Strings[i]));
                    bin.Write((byte)0x00);
                    UInt32 OldOffset = (UInt32)bin.BaseStream.Position;
                    bin.BaseStream.Position = BaseOffList + i * 4;
                    bin.Write(StrOffset - TableOffset);
                    bin.BaseStream.Position = OldOffset;
                }
                UInt32 FinalOffset = (UInt32)bin.BaseStream.Position;
                bin.BaseStream.Position = BaseOffList + tbl.Strings.Count * 4;
                bin.Write(FinalOffset - TableOffset);
                bin.BaseStream.Position = FinalOffset;
            }
        }
    }

    public class Header
    {
        public UInt32 NodeNamesOffset;
        public UInt32 StringResOffset;
        public UInt32 RootOffset;

        public Header(byte[] data)
        {
            if (data[0] != 0x59 | data[1] != 0x42) throw new Exception("Wrong magic number");
            if (data[2] != 0x01 | data[3] != 0) throw new Exception("Wrong file version");
            BinaryReader bin = new BinaryReader(new MemoryStream(data));
            bin.BaseStream.Position = 4;
            NodeNamesOffset = bin.ReadUInt32();
            StringResOffset = bin.ReadUInt32();
            RootOffset = bin.ReadUInt32();
            //Debug.Print("NodeNamesOffset: " + NodeNamesOffset.ToString());
            //Debug.Print("StringResOffset: " + StringResOffset.ToString());
            //Debug.Print("RootOffset: " + RootOffset.ToString());
        }

        public Header()
        {
        }
    }

    public class StringTable
    {
        public UInt32[] StringOffsets;
        public List<string> Strings = new List<string>();
        public bool IsNull = false;

        string JapChars(byte[] input)
        {
            Encoding Enc = Encoding.GetEncoding(932);
            return Enc.GetString(input);
        }

        public StringTable(UInt32 offset, Byte[] data)
        {
            BinaryReader bin = new BinaryReader(new MemoryStream(data));
            bin.BaseStream.Position = offset;
            //Offset = offset;
            if (bin.ReadByte() != 0xC2) throw new Exception("Not a string table");
            List<byte> _count = new List<byte>();
            _count.AddRange(bin.ReadBytes(3));
            _count.Add(0x00);
            uint count = BitConverter.ToUInt32(_count.ToArray(), 0);
            List<UInt32> _stringOffsets = new List<uint>();
            for (int i = 0; i < count; i++) _stringOffsets.Add(bin.ReadUInt32() + offset); //The last offset is the end of the section
            StringOffsets = _stringOffsets.ToArray();
            for (int i = 0; i < count; i++)
            {
                bin.BaseStream.Position = _stringOffsets[i];
                Strings.Add(ReadNullTerminatedString(bin));
            }
        }

        public StringTable()
        {
        }

        public StringTable(bool _isNull)
        {
            IsNull = _isNull;
        }

        string ReadNullTerminatedString(BinaryReader bin)
        {
            List<byte> Data = new List<byte>();
            while (true)
            {
                byte[] r = bin.ReadBytes(1);
                if (r[0] == 0x00) break;
                else Data.Add(r[0]);
            }
            return JapChars(Data.ToArray());
        }

        /*string ReadNullTerminatedString(BinaryReader bin)
        {
            string res = "";
            while (true)
            {
                byte[] r = bin.ReadBytes(1);
                if (r[0] == 0x00) break;
                else res = res + Encoding.Default.GetString(r);
            }
            return res;
        }*/

        byte[] CreateNullTerminatedString(string str)
        {
            List<byte> res = new List<byte>();
            res.AddRange(Encoding.Default.GetBytes(str));
            res.Add(0x00);
            return res.ToArray();
        }
    }

    public class DictionaryNode : GenericNode
    {
        public DictionaryNode(BinaryReader bin)
        {
            //Offset = (UInt32)bin.BaseStream.Position;
            NodeType = bin.ReadByte();
            if (NodeType != 0xC1) throw new Exception("Not a dictionary node !");
            List<byte> _count = new List<byte>();
            _count.AddRange(bin.ReadBytes(3));
            _count.Add(0x00);
            uint count = BitConverter.ToUInt32(_count.ToArray(), 0);
            if (count == 0) return;
            for (int i = 0; i < count; i++)
            {
                ////Debug.Print("DICTIONARY SUBNODE AT: " + HelpFunctions.GetHexString((int)bin.BaseStream.Position));
                SubNodes.Add(new DictionarySubNode(bin));
            }
        }

        public DictionaryNode()
        {
        }

        public class DictionarySubNode : GenericNode
        {
            public DictionarySubNode(BinaryReader bin)
            {
                List<byte> _stringIndex = new List<byte>();
                _stringIndex.AddRange(bin.ReadBytes(3));
                _stringIndex.Add(0x00);
                StringIndex = BitConverter.ToUInt32(_stringIndex.ToArray(), 0);
                NodeType = bin.ReadByte();
                Value = bin.ReadBytes(4);
                if (NodeType == 0xC1)
                {
                    long OldPos = bin.BaseStream.Position;
                    bin.BaseStream.Position = BitConverter.ToUInt32(Value, 0);
                    ////Debug.Print(HelpFunctions.GetHexString(NodeType) + " node at: " + HelpFunctions.GetHexString((int)bin.BaseStream.Position));
                    DictionaryNode dict = new DictionaryNode(bin);
                    if (dict.SubNodes.Count != 0) SubNodes = dict.SubNodes;
                    bin.BaseStream.Position = OldPos;
                }
                else if (NodeType == 0xC0)
                {
                    long OldPos = bin.BaseStream.Position;
                    bin.BaseStream.Position = BitConverter.ToUInt32(Value, 0);
                    ////Debug.Print(HelpFunctions.GetHexString(NodeType) + " node at: " + HelpFunctions.GetHexString((int)bin.BaseStream.Position));
                    ArrayNode arr = new ArrayNode(bin);
                    if (arr.SubNodes.Count != 0) SubNodes = arr.SubNodes;
                    bin.BaseStream.Position = OldPos;
                }// else //Debug.Print(HelpFunctions.GetHexString(NodeType) + " node at: " + HelpFunctions.GetHexString((int)bin.BaseStream.Position));
            }
        }
    }

    public class ArrayNode : GenericNode
    {
        public ArrayNode(BinaryReader bin)
        {
            //Offset = (UInt32)bin.BaseStream.Position;
            NodeType = bin.ReadByte();
            if (NodeType != 0xC0) throw new Exception("Not an array node !");
            List<byte> _count = new List<byte>();
            _count.AddRange(bin.ReadBytes(3));
            _count.Add(0x00);
            uint Count = BitConverter.ToUInt32(_count.ToArray(), 0);
            if (Count == 0) return;
            byte[] NodeTypes;
            NodeTypes = bin.ReadBytes((int)Count);
            while ((bin.BaseStream.Position % 4) != 0) bin.ReadByte(); //Padding
            for (int i = 0; i < Count; i++)
            {
                ////Debug.Print("ARRAY SUBNODE AT: " + HelpFunctions.GetHexString((int)bin.BaseStream.Position));
                SubNodes.Add(new ArraySubNode(NodeTypes[i], bin));
            }
        }

        public class ArraySubNode : GenericNode
        {
            public ArraySubNode(byte type, BinaryReader bin)
            {
                NodeType = type;
                Value = bin.ReadBytes(4);
                if (NodeType == 0xC1)
                {
                    long OldPos = bin.BaseStream.Position;
                    bin.BaseStream.Position = BitConverter.ToUInt32(Value, 0);
                    ////Debug.Print(HelpFunctions.GetHexString(NodeType) + " node at: " + HelpFunctions.GetHexString((int)bin.BaseStream.Position));
                    DictionaryNode dict = new DictionaryNode(bin);
                    if (dict.SubNodes.Count != 0) SubNodes = dict.SubNodes;
                    bin.BaseStream.Position = OldPos;
                }
                else if (NodeType == 0xC0)
                {
                    long OldPos = bin.BaseStream.Position;
                    bin.BaseStream.Position = BitConverter.ToUInt32(Value, 0);
                    ////Debug.Print(HelpFunctions.GetHexString(NodeType) + " node at: " + HelpFunctions.GetHexString((int)bin.BaseStream.Position));
                    ArrayNode Arr = new ArrayNode(bin);
                    if (Arr.SubNodes.Count != 0) SubNodes = Arr.SubNodes;
                    bin.BaseStream.Position = OldPos;
                }//else //Debug.Print(HelpFunctions.GetHexString(NodeType) + " node at: " + HelpFunctions.GetHexString((int)bin.BaseStream.Position));

            }
        }
    }

    public enum SubNodeValTypes
    {
        String = (byte)0xA0,
        Empty = (byte)0xA1,
        Int = (byte)0xD1,
        Single = (byte)0xD2,
        Other,
    }

    public class GenericNode
    {
        public string Name;
        public string StringValue;
        public byte NodeType;
        public byte[] Value; //if NodeType = 0xC0 or 0xC1 this is empty
        public UInt32 StringIndex = 0xFFFFFFFF;
        public List<GenericNode> SubNodes = new List<GenericNode>(); //This is empty if NodeType != 0xC0 or 0xC1

        public GenericNode() { }

        string JapChars(string input)
        {
            byte[] bytes = Encoding.Default.GetBytes(input);
            Encoding Enc = Encoding.GetEncoding(932);
            return Enc.GetString(bytes);
        }

        public GenericNode(string _Name, string _StringValue, byte _type)
        {
            Name = _Name;
            StringValue = _StringValue;
            switch (_type)
            {
                case 0xC0:
                    NodeType = 0xC0;
                    break;
                case 0xC1:
                    NodeType = 0xC1;
                    break;
                case 0xA0:
                    StringValue = JapChars(_StringValue);
                    NodeType = 0xA0;
                    break;
                case 0xA1:
                    NodeType = 0xA1;
                    break;
                case 0xD1:
                    NodeType = 0xD1;
                    Int32 tmp = Convert.ToInt32(_StringValue);
                    Value = BitConverter.GetBytes(tmp);
                    break;
                case 0xD2:
                    NodeType = 0xD2;
                    Single Stmp = Convert.ToSingle(_StringValue);
                    Value = BitConverter.GetBytes(Stmp);
                    break;
                default:
                    NodeType = _type;
                    Value = StringToByteArray(_StringValue.Trim().ToUpper());
                    break;
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            return val - (val < 58 ? 48 : 55);
        }

        public bool Equals(GenericNode obj)
        {
            if (obj.StringIndex != StringIndex) return false;
            if (obj.NodeType != NodeType) return false;
            if (NodeType == 0xC0 || NodeType == 0xC1)
            {
                if (SubNodes.Count != obj.SubNodes.Count) return false;
                for (int i = 0; i < SubNodes.Count; i++) if (!SubNodes[i].Equals(obj.SubNodes[i]))
                        return false;
                //Debug.Print("Node equal for items");
            }
            else
            {
                if (!obj.Value.SequenceEqual(Value)) return false;
                //Debug.Print("Node equal for value");
            }
            return true;
        }
    }

    public static class HelpFunctions
    {
        public static byte[] GetUint24(uint Val)
        {
            List<byte> Res = new List<byte>();
            Res.AddRange(BitConverter.GetBytes((UInt32)(Val)));
            Res.RemoveAt(3);
            //debugBytes(Res.ToArray());
            return Res.ToArray();
        }

        public static void debugBytes(byte[] robe)
        {
            string Value = "";
            for (int ii = 0; ii < robe.Length; ii++) Value += " 0x" + GetHexString(robe[ii]);
            //Debug.Print(Value);
            return;
        }

        public static string GetHexString(int num)
        {
            return num.ToString("X");
        }
    }
}