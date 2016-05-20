using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using LibEveryFileExplorer.Collections;
using System.IO;
using LibEveryFileExplorer._3D;
using CommonFiles;
using LibEveryFileExplorer.IO;
using LibEveryFileExplorer.Math;
using static MarioKart.MK7.KCL;

namespace MarioKart.MK7 //Thanks Gericom for this
{
	public class KCL
	{
		public KCL()
		{
			Header = new MK7KCLHeader();
		}
		public KCL(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new MK7KCLHeader(er);
				er.BaseStream.Position = Header.VerticesOffset;
				uint nr = (Header.NormalsOffset - Header.VerticesOffset) / 0xC;
				Vertices = new Vector3[nr];
				for (int i = 0; i < nr; i++) Vertices[i] = er.ReadVector3();

				er.BaseStream.Position = Header.NormalsOffset;
				nr = (Header.PlanesOffset - Header.NormalsOffset) / 0xC;
				Normals = new Vector3[nr];
				for (int i = 0; i < nr; i++) Normals[i] = er.ReadVector3();

				er.BaseStream.Position = Header.PlanesOffset;
				nr = (Header.OctreeOffset - Header.PlanesOffset) / 0x10;
				Planes = new KCLPlane[nr];
				for (int i = 0; i < nr; i++) Planes[i] = new KCLPlane(er);

				er.BaseStream.Position = Header.OctreeOffset;
				int nodes = (int)(
					((~Header.XMask >> (int)Header.CoordShift) + 1) *
					((~Header.YMask >> (int)Header.CoordShift) + 1) *
					((~Header.ZMask >> (int)Header.CoordShift) + 1));
				Octree = new KCLOctree(er, nodes);
			}
			finally
			{
				er.Close();
			}
		}

		public byte[] Write()
		{
			MemoryStream m = new MemoryStream();
			EndianBinaryWriter er = new EndianBinaryWriter(m, Endianness.LittleEndian);
			Header.Write(er);
			long curpos = er.BaseStream.Position;
			er.BaseStream.Position = 0;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;
			foreach (Vector3 v in Vertices)
			{
				er.WriteVector3(v);
			}
			while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0);
			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 4;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;
			foreach (Vector3 v in Normals)
			{
				er.WriteVector3(v);
			}
			while ((er.BaseStream.Position % 4) != 0) er.Write((byte)0);
			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 8;
			er.Write((uint)(curpos - 0x10));
			er.BaseStream.Position = curpos;
			foreach (KCLPlane p in Planes) p.Write(er);
			curpos = er.BaseStream.Position;
			er.BaseStream.Position = 12;
			er.Write((uint)curpos);
			er.BaseStream.Position = curpos;
			Octree.Write(er);
			byte[] result = m.ToArray();
			er.Close();
			return result;
		}
        
		public bool Convert(int FilterIndex, string Path)
		{
			switch (FilterIndex)
			{
				case 0:
					OBJ o = ToOBJ();
					byte[] d = o.Write();
					File.Create(Path).Close();
					File.WriteAllBytes(Path, d);
					return true;
				default:
					return false;
			}
		}

        public List<String> CreateFromFile(byte[] Data) //Return mat count to create pa
        {
            OBJ o = new OBJ(Data);
            List<String> matnames = new List<string>();
            foreach (var v in o.Faces) if (!matnames.Contains(v.Material)) matnames.Add(v.Material);
            Dictionary<string, ushort> Mapping = new Dictionary<string, ushort>();
            Dictionary<string, bool> Colli = new Dictionary<string, bool>();
            for (ushort i = 0; i < matnames.Count;i++) Mapping.Add(matnames[i], i);
            foreach (string str in matnames) Colli.Add(str, true);
            List<Vector3> Vertex = new List<Vector3>();
            List<Vector3> Normals = new List<Vector3>();
            List<KCLPlane> planes = new List<KCLPlane>();
            List<Triangle> Triangles = new List<Triangle>();
            foreach (var v in o.Faces)
            {
                if (Colli[v.Material])
                {
                    Triangle t = new Triangle(o.Vertices[v.VertexIndieces[0]], o.Vertices[v.VertexIndieces[1]], o.Vertices[v.VertexIndieces[2]]);
                    Vector3 qq = (t.PointB - t.PointA).Cross(t.PointC - t.PointA);
                    if ((qq.X * qq.X + qq.Y * qq.Y + qq.Z * qq.Z) < 0.01) continue;
                    KCLPlane p = new KCLPlane();
                    p.CollisionType = Mapping[v.Material];
                    Vector3 a = (t.PointC - t.PointA).Cross(t.Normal);
                    a.Normalize();
                    a = -a;
                    Vector3 b = (t.PointB - t.PointA).Cross(t.Normal);
                    b.Normalize();
                    Vector3 c = (t.PointC - t.PointB).Cross(t.Normal);
                    c.Normalize();
                    p.Length = (t.PointC - t.PointA).Dot(c);
                    int q = ContainsVector3(t.PointA, Vertex);
                    if (q == -1) { p.VertexIndex = (ushort)Vertex.Count; Vertex.Add(t.PointA); }
                    else p.VertexIndex = (ushort)q;
                    q = ContainsVector3(t.Normal, Normals);
                    if (q == -1) { p.NormalIndex = (ushort)Normals.Count; Normals.Add(t.Normal); }
                    else p.NormalIndex = (ushort)q;
                    q = ContainsVector3(a, Normals);
                    if (q == -1) { p.NormalAIndex = (ushort)Normals.Count; Normals.Add(a); }
                    else p.NormalAIndex = (ushort)q;
                    q = ContainsVector3(b, Normals);
                    if (q == -1) { p.NormalBIndex = (ushort)Normals.Count; Normals.Add(b); }
                    else p.NormalBIndex = (ushort)q;
                    q = ContainsVector3(c, Normals);
                    if (q == -1) { p.NormalCIndex = (ushort)Normals.Count; Normals.Add(c); }
                    else p.NormalCIndex = (ushort)q;
                    planes.Add(p);
                    Triangles.Add(t);
                }
            }
            Vertices = Vertex.ToArray();
            this.Normals = Normals.ToArray();
            Planes = planes.ToArray();
            Header = new MK7KCLHeader();
            Octree = KCLOctree.FromTriangles(Triangles.ToArray(), Header, 2048, 128, 128, 50);
            return matnames;
        }

		private int ContainsVector3(Vector3 a, List<Vector3> b)
		{
			for (int i = 0; i < b.Count; i++)
			{
				if (b[i].X == a.X && b[i].Y == a.Y && b[i].Z == a.Z)
				{
					return i;
				}
			}
			return -1;
		}

		public MK7KCLHeader Header;
		public class MK7KCLHeader
		{
            #region HeaderData
            public UInt32 VerticesOffset;
            public UInt32 NormalsOffset;
            public UInt32 PlanesOffset;//-0x10
            public UInt32 OctreeOffset;
            public Single Unknown1;
            public Vector3 OctreeOrigin;
            public UInt32 XMask;
            public UInt32 YMask;
            public UInt32 ZMask;
            public UInt32 CoordShift;
            public UInt32 YShift;
            public UInt32 ZShift;
            public Single Unknown2;
            #endregion
            public MK7KCLHeader() { }
			public MK7KCLHeader(EndianBinaryReader er)
			{
				VerticesOffset = er.ReadUInt32();
				NormalsOffset = er.ReadUInt32();
				PlanesOffset = er.ReadUInt32() + 0x10;
				OctreeOffset = er.ReadUInt32();
				Unknown1 = er.ReadSingle();
				OctreeOrigin = er.ReadVector3();
				XMask = er.ReadUInt32();
				YMask = er.ReadUInt32();
				ZMask = er.ReadUInt32();
				CoordShift = er.ReadUInt32();
				YShift = er.ReadUInt32();
				ZShift = er.ReadUInt32();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(VerticesOffset);
				er.Write(NormalsOffset);
				er.Write((uint)(PlanesOffset - 0x10));
				er.Write(OctreeOffset);
				er.Write(Unknown1);
				er.WriteVector3(OctreeOrigin);
				er.Write(XMask);
				er.Write(YMask);
				er.Write(ZMask);
				er.Write(CoordShift);
				er.Write(YShift);
				er.Write(ZShift);
			}
		}

		public Vector3[] Vertices;
		public Vector3[] Normals;

		public KCLPlane[] Planes;
		public class KCLPlane
		{
			public KCLPlane() { }
			public KCLPlane(EndianBinaryReader er)
			{
				Length = er.ReadSingle();
				VertexIndex = er.ReadUInt16();
				NormalIndex = er.ReadUInt16();
				NormalAIndex = er.ReadUInt16();
				NormalBIndex = er.ReadUInt16();
				NormalCIndex = er.ReadUInt16();
				CollisionType = er.ReadUInt16();
			}
			public void Write(EndianBinaryWriter er)
			{
				er.Write(Length);
				er.Write(VertexIndex);
				er.Write(NormalIndex);
				er.Write(NormalAIndex);
				er.Write(NormalBIndex);
				er.Write(NormalCIndex);
				er.Write(CollisionType);
			}
			public Single Length;
			public UInt16 VertexIndex;
			public UInt16 NormalIndex;
			public UInt16 NormalAIndex;
			public UInt16 NormalBIndex;
			public UInt16 NormalCIndex;
			public UInt16 CollisionType;
		}

		public KCLOctree Octree;

		public Triangle GetTriangle(KCLPlane Plane)
		{
			Vector3 A = Vertices[Plane.VertexIndex];
			Vector3 CrossA = Normals[Plane.NormalAIndex].Cross(Normals[Plane.NormalIndex]);
			Vector3 CrossB = Normals[Plane.NormalBIndex].Cross(Normals[Plane.NormalIndex]);
			Vector3 B = A + CrossB * (Plane.Length / CrossB.Dot(Normals[Plane.NormalCIndex]));
			Vector3 C = A + CrossA * (Plane.Length / CrossA.Dot(Normals[Plane.NormalCIndex]));
			return new Triangle(A, B, C);
		}

		public OBJ ToOBJ()
		{
			OBJ o = new OBJ();
			int v = 0;
			foreach (var vv in Planes)
			{
				Triangle t = GetTriangle(vv);
				o.Vertices.Add(t.PointA);
				o.Vertices.Add(t.PointB);
				o.Vertices.Add(t.PointC);
				var f = new OBJ.OBJFace();
				f.Material = vv.CollisionType.ToString("X");
				f.VertexIndieces.Add(v);
				f.VertexIndieces.Add(v + 1);
				f.VertexIndieces.Add(v + 2);
				o.Faces.Add(f);
				v += 3;
			}
			return o;
		}
	}

    public class KCLOctree
    {
        public KCLOctree() { }
        public KCLOctree(EndianBinaryReader er, int NrNodes)
        {
            long baseoffset = er.BaseStream.Position;
            RootNodes = new KCLOctreeNode[NrNodes];
            for (int i = 0; i < NrNodes; i++)
            {
                RootNodes[i] = new KCLOctreeNode(er, baseoffset);
            }
        }

        public void Write(EndianBinaryWriter er)
        {
            long basepos = er.BaseStream.Position;
            Queue<uint> NodeBaseOffsets = new Queue<uint>();
            Queue<KCLOctreeNode> Nodes = new Queue<KCLOctreeNode>();
            foreach (var v in RootNodes)
            {
                NodeBaseOffsets.Enqueue(0);
                Nodes.Enqueue(v);
            }
            uint offs = (uint)(RootNodes.Length * 4);
            while (Nodes.Count > 0)
            {
                KCLOctreeNode n = Nodes.Dequeue();
                if (n.IsLeaf)
                {
                    NodeBaseOffsets.Dequeue();
                    er.Write((uint)0);
                }
                else
                {
                    n.DataOffset = offs - NodeBaseOffsets.Dequeue();
                    er.Write(n.DataOffset);
                    foreach (var v in n.SubNodes)
                    {
                        NodeBaseOffsets.Enqueue(offs);
                        Nodes.Enqueue(v);
                    }
                    offs += 8 * 4;
                }
            }
            foreach (var v in RootNodes)
            {
                NodeBaseOffsets.Enqueue(0);
                Nodes.Enqueue(v);
            }
            long leafstartpos = er.BaseStream.Position;
            uint relleafstartpos = offs;
            er.BaseStream.Position = basepos;
            offs = (uint)(RootNodes.Length * 4);
            while (Nodes.Count > 0)
            {
                KCLOctreeNode n = Nodes.Dequeue();
                if (n.IsLeaf)
                {
                    er.Write((uint)(0x80000000 | (relleafstartpos - NodeBaseOffsets.Dequeue() - 2)));
                    long curpos = er.BaseStream.Position;
                    er.BaseStream.Position = leafstartpos;
                    foreach (var v in n.Triangles)
                    {
                        er.Write((ushort)(v + 1));
                    }
                    er.Write((ushort)0);
                    relleafstartpos += (uint)(n.Triangles.Length * 2) + 2;
                    leafstartpos = er.BaseStream.Position;
                    er.BaseStream.Position = curpos;
                }
                else
                {
                    er.BaseStream.Position += 4;
                    NodeBaseOffsets.Dequeue();
                    foreach (var v in n.SubNodes)
                    {
                        NodeBaseOffsets.Enqueue(offs);
                        Nodes.Enqueue(v);
                    }
                    offs += 8 * 4;
                }
            }
        }

        public KCLOctreeNode[] RootNodes;

        public class KCLOctreeNode
        {
            public KCLOctreeNode() { }
            public KCLOctreeNode(EndianBinaryReader er, long BaseOffset)
            {
                DataOffset = er.ReadUInt32();
                IsLeaf = (DataOffset >> 31) == 1;
                DataOffset &= 0x7FFFFFFF;
                long curpos = er.BaseStream.Position;
                er.BaseStream.Position = BaseOffset + DataOffset;
                if (IsLeaf)
                {
                    er.BaseStream.Position += 2;//Skip starting zero
                    List<ushort> tris = new List<ushort>();
                    while (true)
                    {
                        ushort v = er.ReadUInt16();
                        if (v == 0) break;
                        tris.Add((ushort)(v - 1));
                    }
                    Triangles = tris.ToArray();
                }
                else
                {
                    SubNodes = new KCLOctreeNode[8];
                    for (int i = 0; i < 8; i++)
                    {
                        SubNodes[i] = new KCLOctreeNode(er, BaseOffset + DataOffset);
                    }
                }
                er.BaseStream.Position = curpos;
            }

            public UInt32 DataOffset;
            public Boolean IsLeaf;

            public KCLOctreeNode[] SubNodes;
            public ushort[] Triangles;

            public static KCLOctreeNode Generate(Dictionary<ushort, Triangle> Triangles, Vector3 Position, float BoxSize, int MaxTris, int MinSize)
            {
                KCLOctreeNode n = new KCLOctreeNode();
                //Pump this box a little up, to prevent glitches
                Vector3 midpos = Position + new Vector3(BoxSize / 2f, BoxSize / 2f, BoxSize / 2f);
                float newsize = BoxSize + 50;// 60;
                Vector3 newpos = midpos - new Vector3(newsize / 2f, newsize / 2f, newsize / 2f);
                Dictionary<ushort, Triangle> t = new Dictionary<ushort, Triangle>();
                foreach (var v in Triangles)
                {
                    if (tricube_overlap(v.Value, newpos, newsize)) t.Add(v.Key, v.Value);
                }
                if (BoxSize > MinSize && t.Count > MaxTris)
                {
                    n.IsLeaf = false;
                    float childsize = BoxSize / 2f;
                    n.SubNodes = new KCLOctreeNode[8];
                    int i = 0;
                    for (int z = 0; z < 2; z++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            for (int x = 0; x < 2; x++)
                            {
                                Vector3 pos = Position + childsize * new Vector3(x, y, z);
                                n.SubNodes[i] = KCLOctreeNode.Generate(t, pos, childsize, MaxTris, MinSize);
                                i++;
                            }
                        }
                    }
                }
                else
                {
                    n.IsLeaf = true;
                    n.Triangles = t.Keys.ToArray();
                }
                return n;
            }

            private static bool axis_test(float a1, float a2, float b1, float b2, float c1, float c2, float half)
            {
                float p = a1 * b1 + a2 * b2;
                float q = a1 * c1 + a2 * c2;
                float r = half * (Math.Abs(a1) + Math.Abs(a2));
                return Math.Min(p, q) > r || Math.Max(p, q) < -r;
            }
            //Based on this algorithm: http://jgt.akpeters.com/papers/AkenineMoller01/tribox.html
            private static bool tricube_overlap(Triangle t, Vector3 Position, float BoxSize)
            {
                float half = BoxSize / 2f;
                //Position is the min pos, so add half the box size
                Position += new Vector3(half, half, half);
                Vector3 v0 = t.PointA - Position;
                Vector3 v1 = t.PointB - Position;
                Vector3 v2 = t.PointC - Position;

                if (Math.Min(Math.Min(v0.X, v1.X), v2.X) > half || Math.Max(Math.Max(v0.X, v1.X), v2.X) < -half) return false;
                if (Math.Min(Math.Min(v0.Y, v1.Y), v2.Y) > half || Math.Max(Math.Max(v0.Y, v1.Y), v2.Y) < -half) return false;
                if (Math.Min(Math.Min(v0.Z, v1.Z), v2.Z) > half || Math.Max(Math.Max(v0.Z, v1.Z), v2.Z) < -half) return false;

                float d = t.Normal.Dot(v0);
                float r = half * (Math.Abs(t.Normal.X) + Math.Abs(t.Normal.Y) + Math.Abs(t.Normal.Z));
                if (d > r || d < -r) return false;

                Vector3 e = v1 - v0;
                if (axis_test(e.Z, -e.Y, v0.Y, v0.Z, v2.Y, v2.Z, half)) return false;
                if (axis_test(-e.Z, e.X, v0.X, v0.Z, v2.X, v2.Z, half)) return false;
                if (axis_test(e.Y, -e.X, v1.X, v1.Y, v2.X, v2.Y, half)) return false;

                e = v2 - v1;
                if (axis_test(e.Z, -e.Y, v0.Y, v0.Z, v2.Y, v2.Z, half)) return false;
                if (axis_test(-e.Z, e.X, v0.X, v0.Z, v2.X, v2.Z, half)) return false;
                if (axis_test(e.Y, -e.X, v0.X, v0.Y, v1.X, v1.Y, half)) return false;

                e = v0 - v2;
                if (axis_test(e.Z, -e.Y, v0.Y, v0.Z, v1.Y, v1.Z, half)) return false;
                if (axis_test(-e.Z, e.X, v0.X, v0.Z, v1.X, v1.Z, half)) return false;
                if (axis_test(e.Y, -e.X, v1.X, v1.Y, v2.X, v2.Y, half)) return false;
                return true;
            }
        }

        public static KCLOctree FromTriangles(Triangle[] Triangles, MK7KCLHeader Header, int MaxRootSize = 2048, int MinRootSize = 128, int MinCubeSize = 32, int MaxNrTris = 10)//35)
        {
            Header.Unknown1 = 30;
            Header.Unknown2 = 25;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Dictionary<ushort, Triangle> tt = new Dictionary<ushort, Triangle>();
            ushort index = 0;
            foreach (var t in Triangles)
            {
                if (t.PointA.X < min.X) min.X = t.PointA.X;
                if (t.PointA.Y < min.Y) min.Y = t.PointA.Y;
                if (t.PointA.Z < min.Z) min.Z = t.PointA.Z;
                if (t.PointA.X > max.X) max.X = t.PointA.X;
                if (t.PointA.Y > max.Y) max.Y = t.PointA.Y;
                if (t.PointA.Z > max.Z) max.Z = t.PointA.Z;

                if (t.PointB.X < min.X) min.X = t.PointB.X;
                if (t.PointB.Y < min.Y) min.Y = t.PointB.Y;
                if (t.PointB.Z < min.Z) min.Z = t.PointB.Z;
                if (t.PointB.X > max.X) max.X = t.PointB.X;
                if (t.PointB.Y > max.Y) max.Y = t.PointB.Y;
                if (t.PointB.Z > max.Z) max.Z = t.PointB.Z;

                if (t.PointC.X < min.X) min.X = t.PointC.X;
                if (t.PointC.Y < min.Y) min.Y = t.PointC.Y;
                if (t.PointC.Z < min.Z) min.Z = t.PointC.Z;
                if (t.PointC.X > max.X) max.X = t.PointC.X;
                if (t.PointC.Y > max.Y) max.Y = t.PointC.Y;
                if (t.PointC.Z > max.Z) max.Z = t.PointC.Z;
                tt.Add(index, t);
                index++;
            }
            //in real mkds, 25 is subtracted from the min pos
            min -= new Vector3(25, 25, 25);
            //TODO: after that, from some of the components (may be more than one) 30 is subtracted aswell => How do I know from which ones I have to do that?

            //Assume the same is done for max:
            max += new Vector3(25, 25, 25);
            //TODO: +30
            Header.OctreeOrigin = min;
            Vector3 size = max - min;
            float mincomp = Math.Min(Math.Min(size.X, size.Y), size.Z);
            int CoordShift = MathUtil.GetNearest2Power(mincomp);
            if (CoordShift > MathUtil.GetNearest2Power(MaxRootSize)) CoordShift = MathUtil.GetNearest2Power(MaxRootSize);
            //else if (CoordShift < Get2Power(MinRootSize)) CoordShift = Get2Power(MinRootSize);
            Header.CoordShift = (uint)CoordShift;
            int cubesize = 1 << CoordShift;
            int NrX = (1 << MathUtil.GetNearest2Power(size.X)) / cubesize;
            int NrY = (1 << MathUtil.GetNearest2Power(size.Y)) / cubesize;
            int NrZ = (1 << MathUtil.GetNearest2Power(size.Z)) / cubesize;
            if (NrX <= 0) NrX = 1;
            if (NrY <= 0) NrY = 1;
            if (NrZ <= 0) NrZ = 1;
            Header.YShift = (uint)(MathUtil.GetNearest2Power(size.X) - CoordShift);
            Header.ZShift = (uint)(MathUtil.GetNearest2Power(size.X) - CoordShift + MathUtil.GetNearest2Power(size.Y) - CoordShift);
            Header.XMask = 0xFFFFFFFF << MathUtil.GetNearest2Power(size.X);
            Header.YMask = 0xFFFFFFFF << MathUtil.GetNearest2Power(size.Y);
            Header.ZMask = 0xFFFFFFFF << MathUtil.GetNearest2Power(size.Z);

            KCLOctree k = new KCLOctree();
            k.RootNodes = new KCLOctreeNode[NrX * NrY * NrZ];
            int i = 0;
            for (int z = 0; z < NrZ; z++)
            {
                for (int y = 0; y < NrY; y++)
                {
                    for (int x = 0; x < NrX; x++)
                    {
                        Vector3 pos = min + ((float)cubesize) * new Vector3(x, y, z);
                        k.RootNodes[i] = KCLOctreeNode.Generate(tt, pos, cubesize, MaxNrTris, MinCubeSize);
                        i++;
                    }
                }
            }
            return k;
        }
    }
}
