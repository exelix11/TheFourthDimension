using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionsMng
{
    class Pa_format
    {
        public static uint[] knownWallTypes = new uint[] {0,1,2,3,4,5 };
        public static uint[] knownGroundTypes = new uint[] { 0, 1, 2, 3, 4, 5, 6 };
        public static uint[] knownSoundTypes = new uint[] { 0, 1, 2, 3, 4, 5, 6,7,8,9,10,11,12,13,14,15,16 };

        public UInt32 NumFields = 5;
        public UInt32 EntrySize = 4;
        public List<Pa_Field> Fields = new List<Pa_Field>();
        public List<UInt32> entries = new List<uint>();

        public Pa_format(bool SetFields)
        {
            if (SetFields) for (int i = 0; i < NumFields; i++) Fields.Add(Pa_Field.GetDefaultField(i));
        }

        public byte[] MakeFile()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bin = new BinaryWriter(mem);
            bin.Write((UInt32)entries.Count);
            bin.Write(NumFields);
            bin.Write(new byte[] { 0x4C,0,0,0}); //This is always the entries offset
            bin.Write(EntrySize);
            for (int i = 0; i < NumFields; i++) bin.Write(Fields[i].GetSection());
            foreach (UInt32 entry in entries) bin.Write(entry);
            while ((bin.BaseStream.Position % 16) != 0) bin.Write((byte)0x40); //Padding
            return mem.ToArray();
        }

        public static Pa_format LoadFile(byte[] input)
        {
            BinaryReader bin = new BinaryReader(new MemoryStream(input));
            Pa_format Res = new Pa_format(false);
            UInt32 intCount = bin.ReadUInt32();
            Res.NumFields = bin.ReadUInt32();
            UInt32 DataOffset = bin.ReadUInt32();
            Res.EntrySize = bin.ReadUInt32();
            for (int i = 0; i < Res.NumFields; i++) Res.Fields.Add(Pa_Field.FromData(bin.ReadBytes(12)));
            bin.BaseStream.Position = DataOffset;
            if (Res.EntrySize != 4) throw new Exception("Format not supported : EntrySize != 4");
            for (int i = 0; i < intCount; i++) Res.entries.Add(bin.ReadUInt32());
            return Res;
        }

        public override string ToString()
        {
            string Res = "";
            Res += "Pa file:\r\n";
            Res += "Number of fields: " + NumFields.ToString() + "\r\nFields list:";
            foreach (Pa_Field f in Fields) Res += f.ToString() + "\r\n";
            Res += "Number of entries: " + entries.Count.ToString();
            Res += "\r\nEntries list:\r\n";
            List<byte> ShiftVals = new List<byte>();
            List<UInt16> Offsets = new List<ushort>();
            List<uint> Bitmasks = new List<uint>();
            foreach (Pa_Field f in Fields)
            {
                ShiftVals.Add(f.Shift);
                Offsets.Add(f.offsetInEntry);
                Bitmasks.Add(f.Bitmask);
            }
            foreach (UInt32 n in entries)
            {
                string entryText = " -" + Pa_Field.GetString(BitConverter.GetBytes(n)) + ": ";
                for (int i = 0; i < Fields.Count; i++)
                {
                    uint r = n & Bitmasks[i];
                    entryText += " " + (r >> ShiftVals[i]).ToString() + " ";
                }
                entryText += "\r\n";
                Res += entryText;
            }
            return Res;
        }

        public string ToString(bool Unknown)
        {
            string Res = "";
            Res += "Pa file:\r\n";
            Res += "Number of fields: " + NumFields.ToString() + "\r\nFields list:";
            foreach (Pa_Field f in Fields) Res += f.ToString() + "\r\n";
            Res += "Number of entries: " + entries.Count.ToString();
            Res += "\r\nUnknown entries list:\r\n";
            List<byte> ShiftVals = new List<byte>();
            List<UInt16> Offsets = new List<ushort>();
            List<uint> Bitmasks = new List<uint>();
            int count = 0;
            foreach (Pa_Field f in Fields)
            {
                ShiftVals.Add(f.Shift);
                Offsets.Add(f.offsetInEntry);
                Bitmasks.Add(f.Bitmask);
            }
            foreach (UInt32 n in entries)
            {
                string entryText = " -" + Pa_Field.GetString(BitConverter.GetBytes(n)) + ": ";
                bool toAdd = false;
                List<uint> values = new List<uint>();
                for (int i = 0; i < Fields.Count; i++)
                {
                    uint r = n & Bitmasks[i];
                    uint finalVal = r >> ShiftVals[i];
                    values.Add(finalVal);
                    if (i == 0 && !knownSoundTypes.Contains(finalVal)) toAdd = true;                    
                    else if (i == 1 && !knownGroundTypes.Contains(finalVal)) toAdd = true;
                    else if (i == 2 && finalVal != 0) toAdd = true;
                    else if (i == 3 && !knownWallTypes.Contains(finalVal)) toAdd = true;
                    else if (i == 4 && finalVal != 0) toAdd = true;
                }
                if (toAdd)
                {
                    foreach(uint v in values) entryText += " " + v.ToString() + " ";
                    entryText += "\r\n";
                    Res += entryText;
                    count++;
                }
            }
            Res += "\r\nUnknown entries count: " + count.ToString();
            return Res;
        }

    }

    class Pa_Field
    {
        public byte[] Name;
        public uint Bitmask;
        public UInt16 offsetInEntry;
        public byte Shift;
        public byte Type;

        public override string ToString()
        {
            string Res = "";
            string name;
            if (Name.SequenceEqual(new byte[] { 0x3D, 0xCB, 0x60, 0x62 })) name = "Sound_code"; //Known hashes
            else if (Name.SequenceEqual(new byte[] { 0x60, 0xC6, 0x5B, 0x1B })) name = "Floor_code";
            //*Unknown hash*
            else if (Name.SequenceEqual(new byte[] { 0x22, 0x83, 0x69, 0xCE })) name = "Wall_code";
            else if (Name.SequenceEqual(new byte[] { 0xCB, 0xCB, 0x06, 0xB5 })) name = "Camera_throught";
            else name = "(Unknown hash) " + GetString(Name);
            Res += "-Field Name: " + name + "\r\n";
            Res += " |Field bitmask: " + GetString(BitConverter.GetBytes(Bitmask)) + "\r\n";
            Res += " |Offset in entry: " + offsetInEntry.ToString() + "\r\n";
            Res += " |Shift value: " + Shift.ToString() + "\r\n";
            Res += " |Type: " + Type.ToString();
            return Res;
        }

        public byte[] GetSection()
        {
            MemoryStream mem = new MemoryStream();
            BinaryWriter bin = new BinaryWriter(mem);
            bin.Write(Name);
            bin.Write(Bitmask);
            bin.Write(offsetInEntry);
            bin.Write(Shift);
            bin.Write(Type);
            return mem.ToArray();
        }

        public static Pa_Field FromData(byte[] Data)
        {
            Pa_Field Res = new Pa_Field();
            BinaryReader bin = new BinaryReader(new MemoryStream(Data));
            Res.Name = bin.ReadBytes(4);
            Res.Bitmask = bin.ReadUInt32();
            Res.offsetInEntry = bin.ReadUInt16();
            Res.Shift = bin.ReadByte();
            Res.Type = bin.ReadByte();
            return Res;
        }

        public static Pa_Field GetDefaultField(int index)
        {
            Pa_Field res = new Pa_Field();
            switch (index)
            {
                case 0:
                    res.Name = new byte[] { 0x3D, 0xCB, 0x60, 0x62 };
                    res.Bitmask = BitConverter.ToUInt32(new byte[] { 0x7F, 0x00, 0x00, 0x00 },0);
                    res.offsetInEntry = 0;
                    res.Shift = 0;
                    res.Type = 0;
                    return res;
                case 1:
                    res.Name = new byte[] { 0x60, 0xC6, 0x5B, 0x1B };
                    res.Bitmask = BitConverter.ToUInt32(new byte[] { 0x80, 0x1F, 0x00, 0x00 }, 0);
                    res.offsetInEntry = 0;
                    res.Shift = 7;
                    res.Type = 0;
                    return res;
                case 2:
                    res.Name = new byte[] { 0xC6, 0x48, 0x15, 0x03 };
                    res.Bitmask = BitConverter.ToUInt32(new byte[] { 0x00, 0xE0, 0x07, 0x00 }, 0);
                    res.offsetInEntry = 0;
                    res.Shift = 0xD;
                    res.Type = 0;
                    return res;
                case 3:
                    res.Name = new byte[] { 0x22, 0x83, 0x69, 0xCE };
                    res.Bitmask = BitConverter.ToUInt32(new byte[] { 0x00, 0x00, 0x78, 0x00 }, 0);
                    res.offsetInEntry = 0;
                    res.Shift = 0x13;
                    res.Type = 0;
                    return res;
                case 4:
                    res.Name = new byte[] { 0xCB, 0xCB, 0x06, 0xB5 };
                    res.Bitmask = BitConverter.ToUInt32(new byte[] { 0x00, 0x00, 0x80, 0x00 }, 0);
                    res.offsetInEntry = 0;
                    res.Shift = 0x17;
                    res.Type = 0;
                    return res;
            }
            throw new Exception("index not valid");
        }

        public static string GetString(byte[] Data)
        {
            string Value = "";
            for (int ii = 0; ii < Data.Length; ii++) Value += " 0x" + GetHexString(Data[ii]);
            return Value;
        }

        public static string GetHexString(int num)
        {
            return num.ToString("X");
        }

        public static string GetHexString(uint num)
        {
            return num.ToString("X");
        }
    }
}
