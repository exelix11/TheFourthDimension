using ModelViewer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Xml;
using System.Windows.Input;
using System.IO;
using LibEveryFileExplorer.Files.SimpleFileSystem;
using ExtensionMethods;

namespace The4Dimension
{
    public partial class Form1 : Form
    {
        public UserControl1 render = new UserControl1();
        public Dictionary<string, string> LevelNameNum = new Dictionary<string, string>(); //WX-X, stageName
        int APP_VER = Int32.Parse(Application.ProductVersion.Replace(".", ""));
        string LoadedFile = "";
        bool IsFocus = true;

        public Form1(string FileLoad = "")
        {
            try
            {
                InitializeComponent();
                #region StageList 
                string[] lines = Properties.Resources.AllStageList.Split(Environment.NewLine[0]);
                int nextIndex = -1;
                for (int i = 1; i < 3; i++)
                {
                    for (int y = 1; y < 6; y++)
                    {
                        LevelNameNum.Add("W " + i.ToString() + "-" + y.ToString(), lines[++nextIndex].Trim());
                    }
                }
                for (int i = 3; i < 8; i++)
                {
                    for (int y = 1; y < 7; y++)
                    {
                        LevelNameNum.Add("W " + i.ToString() + "-" + y.ToString(), lines[++nextIndex].Trim());
                    }
                }
                for (int y = 1; y < 10; y++)
                {
                    LevelNameNum.Add("W 8-" + y.ToString(), lines[++nextIndex].Trim());
                }
                for (int y = 1; y < 6; y++)
                {
                    LevelNameNum.Add("W S1-" + y.ToString(), lines[++nextIndex].Trim());
                }
                for (int i = 10; i < 17; i++)
                {
                    for (int y = 1; y < 7; y++)
                    {
                        LevelNameNum.Add("W S" + (i - 8).ToString() + "-" + y.ToString(), lines[++nextIndex].Trim());
                    }
                }
                LevelNameNum.Add("W S8-Championship", lines[++nextIndex].Trim());
                #endregion

                KeyPreview = true;
                elementHost1.Child = render;
                render.MouseLeftButtonDown += render_LeftClick;
                render.MouseMove += render_MouseMove;
                render.MouseLeftButtonDown += render_MouseLeftButtonDown;
                render.MouseLeftButtonUp += render_MouseLeftButtonUp;
                render.KeyDown += render_KeyDown;
                render.KeyUp += render_KeyUP;

                Focus();
                if (FileLoad != "")
                {
                    LoadedFile = FileLoad;
                }
            }
            catch (Exception ex)
            {
                string err = "There was an error in the application.\r\n" +
                    "________________________________________\r\n" +
                    ex.Message + "\r\n\r\n" + ex.StackTrace + "\r\n";
                File.WriteAllText("Error.log", err);
                MessageBox.Show("There was an error in the application:\r\n" + ex.Message);
                MessageBox.Show("A log of the error was saved in the same folder of this application.");
            }
        }

        public Dictionary<string, byte[]> SzsFiles = null;
        Dictionary<string, AllInfoSection> AllInfos = new Dictionary<string, AllInfoSection>();
        public List<Rail> AllRailInfos = new List<Rail>();
        Dictionary<string, int> higestID = new Dictionary<string, int>();
        Dictionary<string, string> ModelResolver = new Dictionary<string, string>(); //Converts names like BlockBrickCoins to BlockBrick.obj, will be replaced with an object database
        public DataSet ObjDatabase = new DataSet("ObjDB");
        public List<string> ObjDatabaseNames = new List<string>(); //For quickly getting the right index in the database
        public Dictionary<string, string> CreatorClassNameTable = new Dictionary<string, string>();
        public CustomStack<UndoAction> Undo = new CustomStack<UndoAction>();
        public static List<ClipBoardItem> clipboard = new List<ClipBoardItem>();
        public static Encoding DefEnc = Encoding.GetEncoding("Shift-JIS");

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("models"))
            {
                if (MessageBox.Show("You must convert every model from the game before you can use the editor, convert now ? (you need to have the extracted ROMFS of the game on your pc)", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Select the ROMFS folder (This folder should contain ObjectData, SystemData, StageData, etc...)\r\nKeep this folder in the same path, from there will be loaded the levels, some files needed for editing the levels and the bgm.\r\nThe content of the folder won't be modified unless you save edited levels there");
                    ModelDumper dlg = new ModelDumper();
                    dlg.ShowDialog();
                    if (!Directory.Exists("models")) Application.Exit();
                    Properties.Settings.Default.Save();
                }
                else Application.Exit();
            }
            LoadObjectDatabase();
            LoadCreatorClassNameTable();
            if (LoadedFile != "") LoadFile(LoadedFile);
            else SetUiLock(false, false);
            gameROMFSPathToolStripMenuItem.Text = "Game ROMFS path: " + Properties.Settings.Default.GamePath;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Title = "Open a level file";
            opn.Filter = "Supported formats (.szs, .byml, .xml)|*.szs; *.byml; *.xml|Every file|*.*";
            if (opn.ShowDialog() == DialogResult.OK)
            {
                LoadFile(opn.FileName);
            }
        }

        private void openFromLevelNameWXXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.GamePath.Trim() == "")
            {
                MessageBox.Show("To use this function you must set the game ROMFS path");
                return;
            }
            else if (!Directory.Exists(Properties.Settings.Default.GamePath + "\\StageData"))
            {
                MessageBox.Show("Folder " + Properties.Settings.Default.GamePath + "\\StageData Not found!\r\nProbably your Romfs dump is incomplete or was modified");
                return;
            }
            FrmLevNameOpen f = new FrmLevNameOpen(LevelNameNum);
            f.ShowDialog();
            if (f.res == null) return;
            else
            {
                if (File.Exists(Properties.Settings.Default.GamePath + "\\StageData\\" + LevelNameNum[f.res])) LoadFile(Properties.Settings.Default.GamePath + "\\StageData\\" + LevelNameNum[f.res]);
                else MessageBox.Show(Properties.Settings.Default.GamePath + "\\StageData\\" + LevelNameNum[f.res] + " Not found!\r\nProbably your Romfs dump is incomplete or was modified");
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Add("AllRailInfos");
            higestID.Add("AllRailInfos", 0);
            render.AddKey("AllRailInfos");
            comboBox1.SelectedIndex = 0;
            SetUiLock(false, true);
        }

        void SetUiLock(bool SZS, bool Lock)
        {
            splitContainer1.Enabled = Lock;
            elementHost1.Enabled = Lock;
            saveAsXmlToolStripMenuItem.Enabled = Lock;
            saveAsBymlToolStripMenuItem1.Enabled = Lock;
            UndoMenu.Enabled = Lock;
            findToolStripMenuItem.Enabled = Lock;
            if (!SZS)
            {
                OtherLevelDataMenu.Enabled = false;
                saveAsSZSToolStripMenuItem.Enabled = false;
            }
            else
            {
                OtherLevelDataMenu.Enabled = Lock;
                saveAsSZSToolStripMenuItem.Enabled = Lock;
            }
            newToolStripMenuItem.Enabled = !Lock;
            openToolStripMenuItem.Enabled = !Lock;
            openFromLevelNameWXXToolStripMenuItem.Enabled = !Lock;
        }

        #region FileLoading
        void LoadFile(string FilePath) //Checks the file type and then loads the file
        {
            if (Path.GetExtension(FilePath).ToLower() == ".xml")
            {
                LoadedFile = FilePath;
                SetUiLock(false, true);
                OpenFile(File.ReadAllText(FilePath, DefEnc));
            }
            else if (Path.GetExtension(FilePath).ToLower() == ".byml")
            {
                LoadedFile = FilePath;
                SetUiLock(false, true);
                OpenFile(BymlConverter.GetXml(FilePath));
            }
            else if (Path.GetExtension(FilePath).ToLower() == ".szs")
            {
                LoadedFile = FilePath;
                SetUiLock(true, true);
                OtherLevelDataMenu.DropDownItems.Clear();
                SzsFiles = new Dictionary<string, byte[]>();
                CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
                NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
                SzsArch = new NDS.NitroSystem.FND.NARC(y.Decompress(File.ReadAllBytes(FilePath)));
                int index = 0;
                List<ToolStripMenuItem> OtherFiles = new List<ToolStripMenuItem>();
                byte[] StageData = null;
                foreach (SFSFile f in SzsArch.ToFileSystem().Files)
                {
                    if (f.FileName.ToLower() == "stagedata.byml") StageData = f.Data;
                    else
                    {
                        ToolStripMenuItem btn = new ToolStripMenuItem();
                        btn.Name = "LoadFile" + index.ToString();
                        btn.Text = f.FileName;
                        btn.Click += LoadFileList_click;
                        OtherFiles.Add(btn);
                        SzsFiles.Add(f.FileName, f.Data);
                    }
                    index++;
                }
                OtherLevelDataMenu.DropDownItems.AddRange(OtherFiles.ToArray());
                if (StageData != null)
                {
                    Debug.Print("Size : " + (StageData.Length / 1024).ToString());
                    OpenFile(BymlConverter.GetXml(StageData));
                }
                else
                {
                    MessageBox.Show("StageData.byml not found in the file !");
                    SzsFiles = null;
                    SetUiLock(false, false);
                }
            }
            else
            {
                MessageBox.Show("File type not supported !");
                SetUiLock(false, false);
            }
        }

        private void LoadFileList_click(object sender, EventArgs e)
        {
            string name = ((ToolStripMenuItem)sender).Text;
            FormEditors.FrmXmlEditor frm = new FormEditors.FrmXmlEditor(BymlConverter.GetXml(SzsFiles[name]), name, false);
            frm.ShowDialog();
            if (frm.XmlRes != null) SzsFiles[name] = BymlConverter.GetByml(frm.XmlRes);
        }

        public void LoadObjectDatabase()
        {
            ObjDatabase.Clear();
            ObjDatabaseNames.Clear();
            if (!File.Exists(@"ObjectsDb.xml"))
            {
                DataTable info = new DataTable("Infos");
                info.Columns.Add("AppVer");
                info.Rows.Add(APP_VER);
                DataTable tb = new DataTable("Objects");
                tb.Columns.Add("InGameName");
                tb.Columns.Add("ModelName");
                tb.Columns.Add("ShortDescription");
                tb.Columns.Add("LongDescription");
                tb.Columns.Add("Author");
                ObjDatabase.Tables.Add(info);
                ObjDatabase.Tables.Add(tb);
                MessageBox.Show("The object database wasn't found, some objects may not appear, and you won't be able to get informations about how to use objects, you can download the database from ???"); //TODO: objectdb updater/downloader
                return;
            } else
            {
                XmlReader xml = XmlReader.Create(@"ObjectsDb.xml");
                ObjDatabase.ReadXml(xml);
                if (Int32.Parse(((string)ObjDatabase.Tables[0].Rows[0][0]).Replace(".", "")) > APP_VER)
                {
                    MessageBox.Show("This object database was made with a newer version of the editor, some data may not work in this version.\r\nYou should update your editor to the latest version");
                }
                for (int i = 0; i < ObjDatabase.Tables[1].Rows.Count; i++)
                {
                    string tmpName = (string)ObjDatabase.Tables[1].Rows[i][0];
                    ObjDatabaseNames.Add(tmpName);
                }
            }
        }

        public void LoadCreatorClassNameTable()
        {
            CreatorClassNameTable.Clear();
            if (!File.Exists(@"CreatorClassNameTable.szs"))
            {
                if (Properties.Settings.Default.GamePath.Trim() != "" && File.Exists(Properties.Settings.Default.GamePath + "\\SystemData\\CreatorClassNameTable.szs"))
                {
                    File.Copy(Properties.Settings.Default.GamePath + "\\SystemData\\CreatorClassNameTable.szs", @"CreatorClassNameTable.szs");
                }
                else
                {
                    if (Properties.Settings.Default.GamePath.Trim() == "") MessageBox.Show("to add objects to the game, and get list of every objects in the editor you need CreatorClassNameTable.szs in the same folder as this program, this file is placed inside GameRomFS:SystemData\\CreatorClassNameTable.szs");
                    else MessageBox.Show(Properties.Settings.Default.GamePath + "\\SystemData\\CreatorClassNameTable.szs not found.\r\nProbably your Romfs dump is incomplete or was modified.\r\nWithout this file you can only duplicate or delete objects.");
                    //Btn_AddObj.Enabled = false;
                    creatorClassNameTableEditorToolStripMenuItem.Enabled = false;
                    return;
                }
            }
            CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
            NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
            SzsArch = new NDS.NitroSystem.FND.NARC(y.Decompress(File.ReadAllBytes(@"CreatorClassNameTable.szs")));
            string ConvertedCCN = BymlConverter.GetXml(SzsArch.ToFileSystem().Files[0].Data);
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(ConvertedCCN);
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
        }

        void OpenFile(string XmlText)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(XmlText);
            XmlNode n = xml.SelectSingleNode("/Root/C1/C1");
            if (n.Attributes["Name"].Value == "AllInfos") ProcessAllInfos(n.ChildNodes); else throw new Exception("Not The AllInfos node !");
            n = xml.SelectNodes("/Root/C1/C1")[1];
            if (n.Attributes["Name"].Value == "AllRailInfos") ProcessRailInfos(n.ChildNodes); else throw new Exception("Not The AllRailInfos node !");
            comboBox1.Items.AddRange(AllInfos.Keys.ToArray());
            comboBox1.Items.Add("AllRailInfos");
            render.AddKey("AllRailInfos");
            LoadRailsModels(AllRailInfos);
            /*xml.Load(System.IO.Path.GetDirectoryName(file) + "\\PreLoadFileList1.xml");
            n = xml.SelectSingleNode("/Root/C1");
            foreach (XmlNode subnode in n.ChildNodes)
            {
                if (subnode.ChildNodes.Count == 2 && subnode.ChildNodes[1].Attributes["StringValue"].Value == "Archive")
                {
                    string FileName = subnode.ChildNodes[0].Attributes["StringValue"].Value.Split('/')[1];
                    FileName = FileName.Substring(0, FileName.Length - 4);
                    Models.Add(FileName.ToLower(), "models\\" + FileName + ".obj");
                }
            } Reading the file list doesn't seem to be useful for now
            */
            foreach (string k in AllInfos.Keys.ToArray())
            {
                render.AddKey(k);
                if (k == "AreaObjInfo") LoadModels(AllInfos[k].Objs, k, "models\\UnkYellow.obj");
                else if (k == "CameraAreaInfo") LoadModels(AllInfos[k].Objs, k, "models\\UnkGreen.obj");
                else LoadModels(AllInfos[k].Objs, k);
            }
            if (AllInfos.ContainsKey("AreaObjInfo")) HideLayer("AreaObjInfo");
            if (AllInfos.ContainsKey("CameraAreaInfo")) HideLayer("CameraAreaInfo");
            checkBox1.Checked = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            comboBox1.Text = comboBox1.Items[0].ToString();
        }

        void LoadRailsModels(List<Rail> source)
        {
            foreach (Rail r in source)
            {
                render.addRail(r.GetPointArray(), 5, -1);
            }
        }

        void LoadRailsModels(Rail source, int at = -1)
        {
            List<Point3D> points = new List<Point3D>();
            foreach (Rail.Point p in source.Points) points.Add(new Point3D(p.X, -p.Z, p.Y));
            render.addRail(points.ToArray(), 5, at);
        }

        void LoadModels(List<LevelObj> Source, string Type, string PlaceHolderMod = "models\\UnkBlue.obj", int at = -1)
        {
            for (int i = 0; i < Source.Count; i++)
            {
                string Path = GetModelname(((Node)Source[i].Prop["name"]).StringValue.ToLower());
                if (!System.IO.File.Exists(Path)) Path = PlaceHolderMod;
                Single X, Y, Z, ScaleX, ScaleY, ScaleZ, RotX, RotY, RotZ;
                X = Single.Parse(((Node)Source[i].Prop["pos_x"]).StringValue);
                Y = Single.Parse(((Node)Source[i].Prop["pos_y"]).StringValue);
                Z = Single.Parse(((Node)Source[i].Prop["pos_z"]).StringValue);
                ScaleX = Single.Parse(((Node)Source[i].Prop["scale_x"]).StringValue);
                ScaleY = Single.Parse(((Node)Source[i].Prop["scale_y"]).StringValue);
                ScaleZ = Single.Parse(((Node)Source[i].Prop["scale_z"]).StringValue);
                RotX = Single.Parse(((Node)Source[i].Prop["dir_x"]).StringValue);
                RotY = Single.Parse(((Node)Source[i].Prop["dir_y"]).StringValue);
                RotZ = Single.Parse(((Node)Source[i].Prop["dir_z"]).StringValue);
                render.addModel(Path, Type, new Vector3D(X, -Z, Y), new Vector3D(ScaleX, ScaleZ, ScaleY), RotX, -RotZ, RotY, at);
            }
        }

        public void AddChildrenModels(C0List tmp, bool area)
        {
            if (tmp.List.Count > 0)
            {
                List<string> modelsPaths = new List<string>();
                List<Vector3D> Pos = new List<Vector3D>();
                List<Vector3D> Rot = new List<Vector3D>();
                List<Vector3D> Scale = new List<Vector3D>();
                foreach (LevelObj o in tmp.List)
                {
                    string Path;
                    if (area) Path = "models\\UnkYellow.obj"; else
                    {
                        Path = GetModelname(((Node)o.Prop["name"]).StringValue.ToLower());
                        if (!System.IO.File.Exists(Path)) Path = "models\\UnkRed.obj";
                    }
                    Single X, Y, Z, ScaleX, ScaleY, ScaleZ, RotX, RotY, RotZ;
                    X = Single.Parse(((Node)o.Prop["pos_x"]).StringValue);
                    Y = Single.Parse(((Node)o.Prop["pos_y"]).StringValue);
                    Z = Single.Parse(((Node)o.Prop["pos_z"]).StringValue);
                    ScaleX = Single.Parse(((Node)o.Prop["scale_x"]).StringValue);
                    ScaleY = Single.Parse(((Node)o.Prop["scale_y"]).StringValue);
                    ScaleZ = Single.Parse(((Node)o.Prop["scale_z"]).StringValue);
                    RotX = Single.Parse(((Node)o.Prop["dir_x"]).StringValue);
                    RotY = Single.Parse(((Node)o.Prop["dir_y"]).StringValue);
                    RotZ = Single.Parse(((Node)o.Prop["dir_z"]).StringValue);
                    Pos.Add(new Vector3D(X, -Z, Y));
                    Rot.Add(new Vector3D(RotX, -RotZ, RotY));
                    Scale.Add(new Vector3D(ScaleX, ScaleZ, ScaleY));
                    modelsPaths.Add(Path);
                }
                render.AddTmpObjects(Pos, Scale, Rot, modelsPaths, area ? "TmpAreaChildrenObjs" : "TmpChildrenObjs");
            }
        }

        string GetModelname(string ObjName)
        {
            foreach (DataRow row in ObjDatabase.Tables[1].Rows) 
            {
                if (((string)row.ItemArray[0]).EndsWith("*"))
                {
                    string name = ((string)row.ItemArray[0]);
                    name = name.Substring(0, name.Length - 1);
                    if (ObjName.ToLower().StartsWith(name.ToLower())) return "models\\" + (string)row.ItemArray[1];
                }
                else
                if (ObjName.Trim() != "" && ObjName.ToLower() == ((string)row.ItemArray[0]).ToLower())
                {
                    if ((string)row.ItemArray[1] != "") return "models\\" + (string)row.ItemArray[1];
                }
            }
            return "models\\" + ObjName + ".obj";
        }

        void ProcessAllInfos(XmlNodeList xml)
        {
            for (int i = 0; i < xml.Count; i++)
            {
                ProcessAllOBJECTS(xml[i].ChildNodes, xml[i].Attributes["Name"].Value);
            }
        }

        void ProcessRailInfos(XmlNodeList xml)
        {
            if (!higestID.ContainsKey("AllRailInfos")) higestID.Add("AllRailInfos", 0);
            for (int i = 0; i < xml.Count; i++)
            {
                foreach (XmlNode node in xml[i].ChildNodes) AllRailInfos.Add(LoadRail(node.ChildNodes, "AllRailInfos"));
            }
        }

        void ProcessAllOBJECTS(XmlNodeList xml, string Type)
        {
            if (!AllInfos.ContainsKey(Type)) AllInfos.Add(Type, new AllInfoSection());
            foreach (XmlNode N in xml) AllInfos[Type].Objs.Add(LoadOBJECT(N.ChildNodes, Type));
        }

        Rail LoadRail(XmlNodeList xml, string Type)
        {
            if (!higestID.ContainsKey(Type)) higestID.Add(Type, 0);
            Rail Ret = new Rail();
            List<int> Args = new List<int>();
            for (int i = 0; i < xml.Count; i++)
            {
                XmlNode xNode = xml[i];
                if (xNode.NodeType == XmlNodeType.Element)
                {
                    if (xNode.Attributes["Name"].Value.StartsWith("Arg")) Args.Add(Int32.Parse(xNode.Attributes["StringValue"].Value));
                    else if (xNode.Attributes["Name"].Value == "LayerName") Ret.LayerName = xNode.Attributes["StringValue"].Value;
                    else if (xNode.Attributes["Name"].Value == "closed") Ret._closed = xNode.Attributes["StringValue"].Value;
                    else if (xNode.Attributes["Name"].Value == "l_id") Ret.l_id = Int32.Parse(xNode.Attributes["StringValue"].Value);
                    else if (xNode.Attributes["Name"].Value == "name") Ret.Name = xNode.Attributes["StringValue"].Value;
                    else if (xNode.Attributes["Name"].Value == "no") Ret.no = Int32.Parse(xNode.Attributes["StringValue"].Value);
                    else if (xNode.Attributes["Name"].Value == "type") Ret.Type = xNode.Attributes["StringValue"].Value;
                    else if (xNode.Attributes["Name"].Value == "Points")
                    {
                        XmlNodeList PointsList = xNode.ChildNodes;
                        foreach (XmlNode Points in PointsList)
                        {
                            Rail.Point P = new Rail.Point();
                            List<int> _Args = new List<int>();
                            List<Single> _X = new List<Single>();
                            List<Single> _Y = new List<Single>();
                            List<Single> _Z = new List<Single>();
                            foreach (XmlNode Point in Points.ChildNodes)
                            {
                                if (Point.Attributes["Name"].Value.StartsWith("Arg")) _Args.Add(Int32.Parse(Point.Attributes["StringValue"].Value));
                                if (Point.Attributes["Name"].Value.EndsWith("_x")) _X.Add(Single.Parse(Point.Attributes["StringValue"].Value));
                                if (Point.Attributes["Name"].Value.EndsWith("_y")) _Y.Add(Single.Parse(Point.Attributes["StringValue"].Value));
                                if (Point.Attributes["Name"].Value.EndsWith("_z")) _Z.Add(Single.Parse(Point.Attributes["StringValue"].Value));
                                if (Point.Attributes["Name"].Value == "id") P.ID = (Int32.Parse(Point.Attributes["StringValue"].Value));
                            }
                            P.Args = _Args;
                            P._X = _X;
                            P._Y = _Y;
                            P._Z = _Z;
                            Ret.Points.Add(P);
                        }
                    }
                    if (xNode.Attributes["Name"].Value == "l_id") if (Int32.Parse(xNode.Attributes["StringValue"].Value) > higestID[Type]) higestID[Type] = Int32.Parse(xNode.Attributes["StringValue"].Value);
                }
            }
            if (Args.Count != 0) Ret.Arg = Args;
            return Ret;
        }

        LevelObj LoadOBJECT(XmlNodeList xml, string Type)
        {
            if (!higestID.ContainsKey(Type)) higestID.Add(Type, 0);
            LevelObj Ret = new LevelObj();
            List<int> Args = new List<int>();
            for (int i = 0; i < xml.Count; i++)
            {
                XmlNode xNode = xml[i];
                if (xNode.NodeType == XmlNodeType.Element)
                {
                    if (xNode.Attributes["Name"].Value.StartsWith("Arg")) Args.Add(Int32.Parse(xNode.Attributes["StringValue"].Value));
                    else
                    {
                        if (xNode.Name == "C1")
                        {
                            if (xNode.Attributes["Name"].Value == "Rail") Ret.Prop.Add("Rail", LoadRail(xNode.ChildNodes, "AllRailInfos"));
                            else throw new Exception("C1 type not implemented :(");
                        }
                        else if (xNode.Name == "C0")
                        {
                            C0List c0Section = new C0List();
                            XmlNodeList objList = xNode.ChildNodes;
                            foreach (XmlNode Object in objList)
                            {
                                c0Section.List.Add(LoadOBJECT(Object.ChildNodes, Type));
                            }
                            Ret.Prop.Add(xNode.Attributes["Name"].Value, c0Section);
                        }
                        else
                            Ret.Prop.Add(xNode.Attributes["Name"].Value, new Node(xNode.Attributes["StringValue"].Value, xNode.Name));
                        if (xNode.Attributes["Name"].Value == "l_id") if (Int32.Parse(xNode.Attributes["StringValue"].Value) > higestID[Type]) higestID[Type] = Int32.Parse(xNode.Attributes["StringValue"].Value);
                    }
                }
            }
            if (Args.Count != 0) Ret.Prop.Add("Arg", Args.ToArray());
            return Ret;
        }
        #endregion

        #region ObjectsEvents

        #region Find
        private void objectByIdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormEditors.FrmSearchValInput f = new FormEditors.FrmSearchValInput();
            f.ShowDialog();
            if (f.Res == null) return;
            FindIndex(comboBox1.Text, "l_id", Convert.ToInt32(f.Res));
        }

        private void objectByCameraIdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormEditors.FrmSearchValInput f = new FormEditors.FrmSearchValInput();
            f.ShowDialog();
            if (f.Res == null) return;
            FindIndex(comboBox1.Text, "CameraId", Convert.ToInt32(f.Res));
        }

        private void Switch___FindClick(object sender, EventArgs e)
        {
            FormEditors.FrmSearchValInput f = new FormEditors.FrmSearchValInput();
            f.ShowDialog();
            if (f.Res == null) return;
            FindIndex(comboBox1.Text, ((ToolStripMenuItem)sender).Text, Convert.ToInt32(f.Res));
        }

        private void objectByViewIdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormEditors.FrmSearchValInput f = new FormEditors.FrmSearchValInput();
            f.ShowDialog();
            if (f.Res == null) return;
            FindIndex(comboBox1.Text, "ViewId", Convert.ToInt32(f.Res));
        }

        private void objectByRailNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormEditors.FrmSearchValInput f = new FormEditors.FrmSearchValInput(true);
            f.ShowDialog();
            if (f.Res == null || (string)f.Res == "") return;
            FindIndex(comboBox1.Text, "Rail", (string)f.Res);
        }
        #endregion

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_cameraCode.Visible = false;
            ObjectsListBox.Items.Clear();
            propertyGrid1.SelectedObject = null;
            if (!AllInfos.ContainsKey(comboBox1.Text))
            {
                if (comboBox1.Text == "AllRailInfos")
                {
                    checkBox1.Visible = false;
                    checkBox2.Visible = false;
                    for (int i = 0; i < AllRailInfos.Count; i++) ObjectsListBox.Items.Add(AllRailInfos[i].ToString());
                }
                ObjectsListBox.SelectionMode = SelectionMode.One;
                return;
            }
            else
            {
                ObjectsListBox.SelectionMode = SelectionMode.MultiExtended;
                if (comboBox1.Text == "AreaObjInfo" || comboBox1.Text == "CameraAreaInfo")
                {
                    checkBox1.Visible = true;
                    checkBox2.Visible = false;
                    if (AllInfos[comboBox1.Text].IsHidden) checkBox1.Checked = true; else checkBox1.Checked = false;
                }
                else
                {
                    checkBox1.Visible = false;
                    checkBox2.Visible = true;
                }
            }
            for (int i = 0; i < AllInfos[comboBox1.Text].Objs.Count; i++) ObjectsListBox.Items.Add(AllInfos[comboBox1.Text].Objs[i].ToString());
        }

        private void render_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control || RenderIsDragging) return;
            object[] indexes = render.GetOBJ(sender, e); //indexes[0] string, [1] int
            if (indexes[0] == null) return; //this means indexes[0] = -1
            if ((string)indexes[0] == "SelectedRail") return;
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf((string)indexes[0]);
            ObjectsListBox.ClearSelected();
            ObjectsListBox.SelectedIndex = (int)indexes[1];
        }

        bool RenderIsDragging = false;
        object[] DraggingArgs = null;
        Vector3D StartPos;

        private void render_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) //Render hotkeys
        {
            if (e.Key == Key.Z)
            {
                if (Undo.Count > 0) Undo.Pop().Undo();
                return;
            }
            if (comboBox1.Text == "AllRailInfos" || ObjectsListBox.SelectedIndex == -1) return;
            if (e.Key == Key.Space) render.CameraToObj(comboBox1.Text, ObjectsListBox.SelectedIndex);
            else if (e.Key == Key.OemPlus) { if (Btn_AddObj.Enabled == true) BtnAddObj_Click(null, null); } //Add obj
            else if (e.Key == Key.D) button2_Click(null, null); //Duplicate
            else if (e.Key == Key.Delete) button3_Click(null, null); //Delete obj
            else if (e.Key == Key.F) findToolStripMenuItem.ShowDropDown();
            else if (e.Key == Key.R) //Round selected object position to a multiple of 100
            {
                if (RenderIsDragging) return;
                string type = comboBox1.Text;
                int id = ObjectsListBox.SelectedIndex;
                ((Node)AllInfos[type].Objs[id].Prop["pos_x"]).StringValue = (Math.Round(Single.Parse(((Node)AllInfos[type].Objs[id].Prop["pos_x"]).StringValue) / 100d, 0) * 100).ToString();
                ((Node)AllInfos[type].Objs[id].Prop["pos_y"]).StringValue = (Math.Round(Single.Parse(((Node)AllInfos[type].Objs[id].Prop["pos_y"]).StringValue) / 100d, 0) * 100).ToString();
                ((Node)AllInfos[type].Objs[id].Prop["pos_z"]).StringValue = (Math.Round(Single.Parse(((Node)AllInfos[type].Objs[id].Prop["pos_z"]).StringValue) / 100d, 0) * 100).ToString();
                UpdateOBJPos(id, ref AllInfos[type].Objs, type);
                propertyGrid1.Refresh();
            }
            else return;
        }

        private void Listbox_keyDown(object sender, System.Windows.Forms.KeyEventArgs e) //Listbox hotkeys
        {
            if (e.KeyCode == Keys.Z && e.Control)
            {
                if (Undo.Count > 0) Undo.Pop().Undo();
                return;
            }
            if (comboBox1.Text == "AllRailInfos" || ObjectsListBox.SelectedIndex == -1) return;
            if (e.KeyCode == Keys.Space) render.CameraToObj(comboBox1.Text, ObjectsListBox.SelectedIndex);
            else if (e.KeyCode == Keys.Oemplus) { if (Btn_AddObj.Enabled == true) BtnAddObj_Click(null, null); } //Add obj
            else if (e.KeyCode == Keys.D && e.Control) button2_Click(null, null); //Duplicate
            else if (e.KeyCode == Keys.Delete) button3_Click(null, null); //Delete obj
            else if (e.KeyCode == Keys.F && e.Control) findToolStripMenuItem.ShowDropDown();
            else if (e.KeyCode == Keys.R && e.Control) //Round selected object position to a multiple of 100
            {
                if (RenderIsDragging) return;
                string type = comboBox1.Text;
                int id = ObjectsListBox.SelectedIndex;
                ((Node)AllInfos[type].Objs[id].Prop["pos_x"]).StringValue = (Math.Round(Single.Parse(((Node)AllInfos[type].Objs[id].Prop["pos_x"]).StringValue) / 100d, 0) * 100).ToString();
                ((Node)AllInfos[type].Objs[id].Prop["pos_y"]).StringValue = (Math.Round(Single.Parse(((Node)AllInfos[type].Objs[id].Prop["pos_y"]).StringValue) / 100d, 0) * 100).ToString();
                ((Node)AllInfos[type].Objs[id].Prop["pos_z"]).StringValue = (Math.Round(Single.Parse(((Node)AllInfos[type].Objs[id].Prop["pos_z"]).StringValue) / 100d, 0) * 100).ToString();
                UpdateOBJPos(id, ref AllInfos[type].Objs, type);
                propertyGrid1.Refresh();
            }
            else return;
            e.SuppressKeyPress = true;
        }


        private void render_KeyUP(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                if (DraggingArgs != null) endDragging();
                RenderIsDragging = false;
                DraggingArgs = null;
                propertyGrid1.Refresh();
            }
        }

        void UpdateRailpos(int id, Point3D[] Points)
        {
            render.UpdateRailpos(id, Points);
            if (comboBox1.SelectedItem.ToString() == "AllRailInfos" && ObjectsListBox.SelectedIndex != -1) render.SelectRail(AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
        }

        private void render_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed || (ModifierKeys & Keys.Control) != Keys.Control || !RenderIsDragging) { RenderIsDragging = false; return; }
            Vector3D NewPos = render.Drag(DraggingArgs, e, ((ModifierKeys & Keys.Alt) == Keys.Alt) ? true : false);
            if (NewPos == null) return;
            if ((string)DraggingArgs[0] == "SelectedRail")
            {
                AllRailInfos[ObjectsListBox.SelectedIndex].Points[(int)DraggingArgs[1]].X = (float)NewPos.X;
                AllRailInfos[ObjectsListBox.SelectedIndex].Points[(int)DraggingArgs[1]].Y = (float)NewPos.Z;
                AllRailInfos[ObjectsListBox.SelectedIndex].Points[(int)DraggingArgs[1]].Z = -(float)NewPos.Y;
                UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
            }
            else
            {
                ((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_x"]).StringValue = NewPos.X.ToString();
                ((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_y"]).StringValue = NewPos.Z.ToString();
                ((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_z"]).StringValue = (-NewPos.Y).ToString();
                UpdateOBJPos((int)DraggingArgs[1], ref AllInfos[(string)DraggingArgs[0]].Objs, (string)DraggingArgs[0]);
            }
            DraggingArgs[2] = NewPos;
        }

        void endDragging()
        {
            if (DraggingArgs[0] == null || DraggingArgs[1] == null || DraggingArgs[2] == null) return;
            if ((string)DraggingArgs[0] == "SelectedRail")
            {
                Action<string, int, Vector3D> act;
                act = (string type, int id, Vector3D pos) =>
                {
                    AllRailInfos[int.Parse(type)].Points[id].X = (float)pos.X;
                    AllRailInfos[int.Parse(type)].Points[id].Y = (float)pos.Z;
                    AllRailInfos[int.Parse(type)].Points[id].Z = -(float)pos.Y;
                    UpdateRailpos(int.Parse(type), AllRailInfos[int.Parse(type)].GetPointArray());
                    propertyGrid1.Refresh();
                };
                Undo.Push(new UndoAction("Moved " + ObjectsListBox.SelectedItem.ToString() + "'s point[" + DraggingArgs[1].ToString() + "] : ", ObjectsListBox.SelectedIndex.ToString(), (int)DraggingArgs[1], StartPos, act));
            }
            else
            {
                Action<string, int, Vector3D> act;
                act = (string type, int id, Vector3D pos) =>
                {
                    ((Node)AllInfos[type].Objs[id].Prop["pos_x"]).StringValue = pos.X.ToString(); //These values were stored directly
                    ((Node)AllInfos[type].Objs[id].Prop["pos_y"]).StringValue = pos.Y.ToString();
                    ((Node)AllInfos[type].Objs[id].Prop["pos_z"]).StringValue = pos.Z.ToString();
                    UpdateOBJPos(id, ref AllInfos[type].Objs, type);
                    propertyGrid1.Refresh();
                };
                Undo.Push(new UndoAction("Moved object : " + AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].ToString(), (string)DraggingArgs[0], (int)DraggingArgs[1], StartPos, act));
            }
        }

        private void render_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DraggingArgs != null) endDragging();
            RenderIsDragging = false;
            DraggingArgs = null;
            propertyGrid1.Refresh();
        }

        private void render_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != Keys.Control || RenderIsDragging) return;
            RenderIsDragging = true;
            DraggingArgs = render.GetOBJ(sender, e);
            if (DraggingArgs[0] == null) { RenderIsDragging = false; return; }
            if ((string)DraggingArgs[0] == "SelectedRail")
            {
                /*
                RenderIsDragging = false;
                DraggingArgs = null;
                return;
                */
                StartPos = AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray()[(int)DraggingArgs[1]].ToVect();
                return;
            }
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf((string)DraggingArgs[0]);
            ObjectsListBox.ClearSelected();
            ObjectsListBox.SelectedIndex = (int)DraggingArgs[1];
            StartPos = new Vector3D(float.Parse(((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_x"]).StringValue),
               float.Parse(((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_y"]).StringValue),
               float.Parse(((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_z"]).StringValue));
        }

        #region Hiding layers
        void HideLayer(string layerName)
        {
            AllInfos[layerName].IsHidden = true;
            render.HideGroup(layerName);
        }

        void ShowLayer(string layerName)
        {
            AllInfos[layerName].IsHidden = false;
            for (int i = 0; i < AllInfos[layerName].Objs.Count; i++) UpdateOBJPos(i, ref AllInfos[layerName].Objs, layerName);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
                ShowLayer(comboBox1.Text);
            else HideLayer(comboBox1.Text);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            render.CleanTmpObjects();
            if (checkBox2.Checked && AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop.ContainsKey("GenerateChildren"))
            {
                AddChildrenModels((C0List)AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"], false);
            }

            if (checkBox2.Checked && AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop.ContainsKey("AreaChildren"))
            {
                AddChildrenModels((C0List)AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop["AreaChildren"], true);
            }
        }
        #endregion

        int AreaObjOldSelection = -1;
        int CameraAreaOldSelection = -1;

        private void ObjectsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_cameraCode.Visible = false;
            render.CleanTmpObjects();
            render.UnselectRail();
            lblDescription.Text = "";
            lblDescription.Tag = -1;
            if (ObjectsListBox.SelectedIndex < 0) return;
            if (ObjectsListBox.SelectedItems.Count > 1)
            {
                Btn_CopyObjs.Visible = true;
                Btn_Duplicate.Visible = false;
                btn_delObj.Text = "Delete objects";
                propertyGrid1.SelectedObject = null;
                return;
            }
            else
            {
                Btn_CopyObjs.Visible = false;
                Btn_Duplicate.Visible = true;
                btn_delObj.Text = "Delete object";
                UpdateHint();
            }
            if (comboBox1.Text == "AreaObjInfo")
            {
                propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop);
                if (!RenderIsDragging) render.CameraToObj(comboBox1.Text, ObjectsListBox.SelectedIndex);
                if (AllInfos[comboBox1.Text].IsHidden)
                {
                    if (AreaObjOldSelection != -1 && AreaObjOldSelection < AllInfos["AreaObjInfo"].Objs.Count) render.ChangeTransform(comboBox1.Text, AreaObjOldSelection, render.Positions[comboBox1.Text][AreaObjOldSelection], new Vector3D(0, 0, 0), 0, 0, 0);
                    UpdateOBJPos(ObjectsListBox.SelectedIndex, ref AllInfos[comboBox1.Text].Objs, comboBox1.Text);
                }
                AreaObjOldSelection = ObjectsListBox.SelectedIndex;
                return;
            }
            else if (comboBox1.Text == "CameraAreaInfo")
            {
                propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop);
                if (!RenderIsDragging) render.CameraToObj(comboBox1.Text, ObjectsListBox.SelectedIndex);
                if (AllInfos[comboBox1.Text].IsHidden)
                {
                    if (CameraAreaOldSelection != -1 && AreaObjOldSelection < AllInfos["CameraAreaInfo"].Objs.Count) render.ChangeTransform(comboBox1.Text, CameraAreaOldSelection, render.Positions[comboBox1.Text][CameraAreaOldSelection], new Vector3D(0, 0, 0), 0, 0, 0);
                    UpdateOBJPos(ObjectsListBox.SelectedIndex, ref AllInfos[comboBox1.Text].Objs, comboBox1.Text);
                }
                CameraAreaOldSelection = ObjectsListBox.SelectedIndex;
                if (AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].ToString() == "CameraArea") btn_cameraCode.Visible = true;
                return;
            }
            else if (comboBox1.Text == "AllRailInfos")
            {
                propertyGrid1.SelectedObject = AllRailInfos[ObjectsListBox.SelectedIndex];
                UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
                render.SelectRail(AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
                if (!RenderIsDragging) render.CameraToObj(comboBox1.Text, ObjectsListBox.SelectedIndex);
            }
            else
            {
                propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop);
                if (!RenderIsDragging) render.CameraToObj(comboBox1.Text, ObjectsListBox.SelectedIndex);
                if (checkBox2.Checked && AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop.ContainsKey("GenerateChildren"))
                {
                    AddChildrenModels((C0List)AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"], false);
                }
                if (checkBox2.Checked && AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop.ContainsKey("AreaChildren"))
                {
                    AddChildrenModels((C0List)AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop["AreaChildren"], true);
                }
            }
        }

        void UpdateHint()
        {
            if (comboBox1.Text == "AllRailInfos")
            {
                lblDescription.Text = "";
                lblDescription.Tag = -1;
                return;
            }
            int index = ObjDatabaseNames.IndexOf(ObjectsListBox.SelectedItem.ToString());
            if (index == -1)
            {
                lblDescription.Text = "This object is not in the database";
                lblDescription.Tag = -1;
            }
            else
            {
                lblDescription.Text = (string)ObjDatabase.Tables[1].Rows[index][2];
                lblDescription.Tag = index;
            }
        }

        private void bymlXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Filter = "Byml File|*.Byml|*.*|*.*";
            SaveFileDialog sav = new SaveFileDialog();
            sav.Filter = "Xml file|*.xml";
            if (opn.ShowDialog() == DialogResult.OK && sav.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sav.FileName, BymlConverter.GetXml(File.ReadAllBytes(opn.FileName)), DefEnc);
            }
        }

        private void xmlBymlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Filter = "Xml file|*.xml|*.*|*.*";
            SaveFileDialog sav = new SaveFileDialog();
            sav.Filter = "Byml File|*.Byml";
            if (opn.ShowDialog() == DialogResult.OK && sav.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sav.FileName, BymlConverter.GetByml(File.ReadAllText(opn.FileName, DefEnc)));
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            FrmCredits c = new FrmCredits();
            c.ShowDialog();
        }

        private void tipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("- To quickly add children to an object, paste other objects inside that object\r\n" +
                "- To paste an object to the list right click on the list to delesect every object and paste the object\r\n" +
                "- To quickly make rails, add some non-exsisting objects (like a random name) they will be showed as blue cubes, place them in the positions of the points of the rail, copy their position, you can have up to 10 items in the clipboard, add a new rail and paste the positions in the points\r\n" +
                "- Objects with rails need a copy of the rail in the object itself, every time you edit the rail you must copy and paste it inside the object\r\n" +
                "- To set the camera angle, add a @CameraPositionHelper and use it to get the right angle (every object starting with @ will be deleted before saving)");
        }

        private void hotkeysListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hotkeys list:\r\n" +
                " Ctrl + Z : Undo\r\n" +
                " Space : Focus the camera on the selected object\r\n" +
                " Ctrl + D : Duplicate selected object\r\n" +
                " + : Add a new object\r\n" +
                " Del : Delete selected object\r\n" +
                " Ctrl + R : Round the selected object position to a multiple of 100 (like Ctrl + alt + drag, but without dragging)\r\n" +
                " Ctrl + F : Open the search menu\r\n" +
                "In the 3D view:\r\n" +
                " Ctrl + drag : Move object\r\n" +
                " Ctrl + Alt + drag : Move object snapping every 100 units\r\n" +
                " -Every other combination without having to press Ctrl\r\n");
        }

        private void Undo_loading(object sender, EventArgs e)
        {
            UndoMenu.DropDownItems.Clear();
            List<ToolStripMenuItem> Items = new List<ToolStripMenuItem>();
            int count = 0;
            foreach (UndoAction act in Undo.ToArray().Reverse())
            {
                ToolStripMenuItem btn = new ToolStripMenuItem();
                btn.Name = "Undo" + count.ToString();
                btn.Text = act.ToString();
                btn.Click += UndoListItem_Click;
                btn.MouseEnter += UndoListItem_MouseEnter;
                Items.Add(btn);
                count++;
            }
            UndoMenu.DropDownItems.AddRange(Items.ToArray());
        }

        private void UndoListItem_MouseEnter(object sender, EventArgs e)
        {
            string SenderName = ((ToolStripMenuItem)sender).Name;
            int index = int.Parse(SenderName.Substring("Undo".Length));
            for (int i = 0; i < UndoMenu.DropDownItems.Count; i++)
            {
                if (i < index) UndoMenu.DropDownItems[i].BackColor = Color.LightBlue;
                else UndoMenu.DropDownItems[i].BackColor = SystemColors.Control;
            }
        }

        private void UndoListItem_Click(object sender, EventArgs e)
        {
            string SenderName = ((ToolStripMenuItem)sender).Name;
            int index = int.Parse(SenderName.Substring("Undo".Length));
            for (int i = 0; i <= index; i++)
            {
                Undo.Pop().Undo();
            }
            UndoMenu.HideDropDown();
        }

        private void gbatempThreadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://gbatemp.net/threads/wip-the-fourth-dimension-a-super-mario-3d-land-level-editor.424001/");
        }

        private void deleteEveryObjectInTheListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Action<string, int, object> action;
            if (comboBox1.Text == "AllRailInfos")
            {
                Rail[] tmp = AllRailInfos.ToArray();
                action = (string type, int at, object rail) =>
                {
                    Rail[] rails = (Rail[])rail;
                    foreach (Rail r in rails) AddRail(r, -1, true);
                };
                Undo.Push(new UndoAction("Removed every rail", "AllRailInfos", -1, tmp, action));
            }
            else
            {
                LevelObj[] tmp = AllInfos[comboBox1.Text].Objs.ToArray();
                action = (string type, int at, object data) =>
                {
                    LevelObj[] objs = (LevelObj[])data;
                    foreach (LevelObj o in objs) AddObj(o, ref AllInfos[type].Objs, type, false, -1, true);
                };
                Undo.Push(new UndoAction("Removed every object in " + comboBox1.Text, comboBox1.Text, -1, tmp, action));
            }
            while (ObjectsListBox.Items.Count != 0) { ObjectsListBox.ClearSelected(); ObjectsListBox.SelectedIndex = 0; DelSelectedObj(true); }
        }

        private void Form1_closing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void creatorClassNameTableEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormEditors.FrmCCNTEdit f = new FormEditors.FrmCCNTEdit(CreatorClassNameTable, this);
            f.ShowDialog();
            LoadCreatorClassNameTable();
        }

        private void changeToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog fld = new FolderBrowserDialog();
            if (fld.ShowDialog() != DialogResult.OK) return;
            Properties.Settings.Default.GamePath = fld.SelectedPath;
            gameROMFSPathToolStripMenuItem.Text = "Game ROMFS path: " + Properties.Settings.Default.GamePath;
            if (File.Exists(@"BgmTable.szs"))
            {
                var res = MessageBox.Show("There is already a BgmTable.szs file in this program's folder, do you want to replace it with a new one from the game path you just selected ? (Choose no if you edited the BGMs or else you will lose your changes)", "Warning", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    if (File.Exists(Properties.Settings.Default.GamePath + "\\SoundData\\BgmTable.szs"))
                    {
                        File.Delete(@"BgmTable.szs");
                        File.Copy(Properties.Settings.Default.GamePath + "\\SoundData\\BgmTable.szs", @"BgmTable.szs");
                    }
                    else MessageBox.Show(Properties.Settings.Default.GamePath + "\\SoundData\\BgmTable.szs not found !\r\nThe file wasn't replaced");
                }
            }
            if (File.Exists(@"CreatorClassNameTable.szs"))
            {
                var res = MessageBox.Show("There is already a CreatorClassNameTable.szs file in this program's folder, do you want to replace it with a new one from the game path you just selected ? (Choose no if you edited the game objects or else you will lose your changes)", "Warning", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    if (File.Exists(Properties.Settings.Default.GamePath + "\\SystemData\\CreatorClassNameTable.szs"))
                    {
                        File.Delete(@"CreatorClassNameTable.szs");
                        File.Copy(Properties.Settings.Default.GamePath + "\\SystemData\\CreatorClassNameTable.szs", @"CreatorClassNameTable.szs");
                    }
                    else MessageBox.Show(Properties.Settings.Default.GamePath + "\\SystemData\\CreatorClassNameTable.szs not found !\r\nThe file wasn't replaced");
                }
            }
        }

        private void stagesBgmEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.GamePath.Trim() == "" && !File.Exists(@"BgmTable.szs"))
            {
                MessageBox.Show("You must set the game Romfs path first !");
                return;
            }
            else if (!File.Exists(Properties.Settings.Default.GamePath + "\\SoundData\\BgmTable.szs") && !File.Exists(@"BgmTable.szs"))
            {
                MessageBox.Show(Properties.Settings.Default.GamePath + "\\SoundData\\BgmTable.szs not found !\r\nProbably your Romfs dump is incomplete or was modified.");
                return;
            }
            else if (File.Exists(Properties.Settings.Default.GamePath + "\\SoundData\\BgmTable.szs") && !File.Exists(@"BgmTable.szs"))
            {
                File.Copy(Properties.Settings.Default.GamePath + "\\SoundData\\BgmTable.szs", @"BgmTable.szs");
            }
            BgmEditors.FrmBgmMain f = new BgmEditors.FrmBgmMain(LevelNameNum);
            f.Show();
        }

        private void From_Activated(object sender, EventArgs e) //Resume sorting
        {
            IsFocus = true;
            render.SetSortFrequency(0.5);
        }

        private void Form_Deactivate(object sender, EventArgs e) //Stop sorting
        {
            IsFocus = false;
            render.SetSortFrequency(0);
        }

        private void ListBox_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ObjectsListBox.ClearSelected();
                ObjectsListBox.SelectedIndex = -1;
                propertyGrid1.SelectedObject = null;
            }
        }

        private void modelImporterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FormEditors.FrmObjImport().ShowDialog();
        }

        private void objectsDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ObjectDbEditor d = new ObjectDbEditor(ObjDatabase);
            d.ShowDialog();
            LoadObjectDatabase();
        }

        private void lblDescription_Click(object sender, EventArgs e)
        {
            if (lblDescription.Tag.ToString() != "-1") new ObjectDB.ObjectDBView(ObjDatabase.Tables[1].Rows[(int)lblDescription.Tag].ItemArray).Show();
        }
        #endregion

        #region LevelEditing

        private void CameraCode_click(object sender, EventArgs e)
        {
            int cameraId = int.Parse(((Node)AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop["CameraId"]).StringValue);
            if (cameraId < 0)
            {
                MessageBox.Show("CameraId can't be less than 0 !");
                return;
            }
            if (SzsFiles == null)
            {
                MessageBox.Show("To use this function you must load a level from an szs file");
                return;
            }
            if (!SzsFiles.ContainsKey("CameraParam.byml"))
            {
                MessageBox.Show("This level doesn't contain the CameraParam file, a generic CameraParam will be generated");
                string TmpCameraParam = Properties.Resources.GenericCameraParam;
                SzsFiles.Add("CameraParam.byml", BymlConverter.GetByml(TmpCameraParam));
                FormEditors.FrmAddCameraSettings f = new FormEditors.FrmAddCameraSettings(TmpCameraParam, cameraId, this);
                f.ShowDialog();
            }
            else
            {
                string CameraParam = BymlConverter.GetXml(SzsFiles["CameraParam.byml"]);
                if (!CameraParam.Contains("<C0 Name=\"CameraParams\">"))
                {
                    DialogResult r = MessageBox.Show("The CameraParam.byml from this szs can't be used, do you want to generate a new CameraParam ?", "CameraParam.byml", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                    if (r == DialogResult.Yes)
                    {
                        string TmpCameraParam = Properties.Resources.GenericCameraParam;
                        FormEditors.FrmAddCameraSettings f = new FormEditors.FrmAddCameraSettings(TmpCameraParam, cameraId, this);
                        f.ShowDialog();
                    }
                    else return;
                }
                else
                {
                    FormEditors.FrmAddCameraSettings f = new FormEditors.FrmAddCameraSettings(CameraParam, cameraId, this);
                    f.ShowDialog();
                }
            }
        }

        private void AddType_click(object sender, EventArgs e) //Addtype
        {
            FormEditors.FrmStringInput f = new FormEditors.FrmStringInput();
            f.ShowDialog();
            string[] internalNames = new string[] { "TmpChildrenObjs", "SelectedRail", "TmpAreaChildrenObjs" };
            if (f.Result == null) return;
            else if (f.Result.Trim() == "") return;
            else if (AllInfos.ContainsKey(f.Result)) MessageBox.Show("This type is already in use");
            else if (internalNames.Contains(f.Result)) MessageBox.Show("This type name is reserved");
            else
            {
                comboBox1.Items.Add(f.Result);
                render.AddKey(f.Result);
                AllInfos.Add(f.Result, new AllInfoSection());
                higestID.Add(f.Result, 0);
            }
        }

        private void propertyGridChange(object s, PropertyValueChangedEventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) { MessageBox.Show("No object selected in the list"); return; }
            if (comboBox1.Text == "AllRailInfos")
            {
                UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
                Action<string, int, string, object> act;
                act = (string type, int id, string propName, object value) =>
                {
                    AllRailInfos[id][propName] = value;
                    propertyGrid1.Refresh();
                    UpdateRailpos(id, ((Rail)value).GetPointArray());
                };
                Undo.Push(new UndoAction("Changed value: " + e.ChangedItem.Label + " of rail: " + AllRailInfos[ObjectsListBox.SelectedIndex].ToString(), comboBox1.Text, ObjectsListBox.SelectedIndex, e.ChangedItem.Label, e.OldValue, act));
                ObjectsListBox.Items[ObjectsListBox.SelectedIndex] = AllRailInfos[ObjectsListBox.SelectedIndex].ToString();
                return;
            }
            else
            {
                string name = e.ChangedItem.Parent.Value is Node ? e.ChangedItem.Parent.Label : e.ChangedItem.Label;
                if (name == "name" || name == "l_id") MessageBox.Show("You shouldn't mess up with the name or the l_id property of the objects, you should add a new object instead and copy the position from this object to the new one.\r\nYou can undo this action from the undo button", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpdateOBJPos(ObjectsListBox.SelectedIndex, ref AllInfos[comboBox1.Text].Objs, comboBox1.Text);
                Action<string, int, string, object> action;
                action = (string type, int id, string propName, object value) =>
                {
                    if (AllInfos[type].Objs[id].Prop[propName] is Node) ((Node)AllInfos[type].Objs[id].Prop[propName]).StringValue = value.ToString();
                    else
                        AllInfos[type].Objs[id].Prop[propName] = value;
                    propertyGrid1.Refresh();
                    UpdateOBJPos(id, ref AllInfos[type].Objs, type);
                };
                Undo.Push(new UndoAction("Changed value: " + name + " of object: " + AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].ToString(), comboBox1.Text, ObjectsListBox.SelectedIndex, name, e.OldValue, action));
                ObjectsListBox.Items[ObjectsListBox.SelectedIndex] = AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].ToString();
            }
        }

        public void UpdateSelectedRailView()
        {
            UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
        }

        public void RailPointsChanged(List<Rail.Point> OldPoints)
        {
            UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
            Action<string, int, string, object> act;
            act = (string type, int id, string propName, object value) =>
            {
                AllRailInfos[id].Points = (List<Rail.Point>)value;
                propertyGrid1.Refresh();
                UpdateRailpos(id, AllRailInfos[id].GetPointArray());
            };
            Undo.Push(new UndoAction("Changed points of rail: " + AllRailInfos[ObjectsListBox.SelectedIndex].ToString(), comboBox1.Text, ObjectsListBox.SelectedIndex, "", OldPoints, act));
        }

        public void C0ListChanged(C0List OldList, int Hash)
        {
            string lbl = propertyGrid1.SelectedGridItem.Label;
            if (((C0List)AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop[lbl]).GetHashCode() != Hash) return; //every time a FrmC0ListEdit form is closed triggers this, even for C0Lists inside an object from another C0List
            if (checkBox2.Checked && (lbl == "GenerateChildren" || lbl == "AreaChildren"))
                AddChildrenModels((C0List)AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop[lbl], lbl == "AreaChildren");
            Action<string, int, string, object> act;
            act = (string type, int id, string propName, object value) =>
            {
                AllInfos[type].Objs[id].Prop[propName] = value;
                propertyGrid1.Refresh();
                if (checkBox2.Checked && (propName == "GenerateChildren" || propName == "AreaChildren")) AddChildrenModels((C0List)AllInfos[type].Objs[id].Prop[propName], propName == "AreaChildren");
            };
            Undo.Push(new UndoAction("Changed " + lbl + " of object: " + AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].ToString(), comboBox1.Text, ObjectsListBox.SelectedIndex, lbl, OldList, act));
        }

        public int GetObjectGenChidCount(string type, int index)
        {
            if (!AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop.ContainsKey("GenerateChildren")) return 0;
            else return ((C0List)AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List.Count;
        }

        public void UpdateOBJPos(int id, ref List<LevelObj> Source, string Type)
        {
            Single X, Y, Z, ScaleX, ScaleY, ScaleZ, RotX, RotY, RotZ;
            X = Single.Parse(((Node)Source[id].Prop["pos_x"]).StringValue);
            Y = Single.Parse(((Node)Source[id].Prop["pos_y"]).StringValue);
            Z = Single.Parse(((Node)Source[id].Prop["pos_z"]).StringValue);
            ScaleX = Single.Parse(((Node)Source[id].Prop["scale_x"]).StringValue);
            ScaleY = Single.Parse(((Node)Source[id].Prop["scale_y"]).StringValue);
            ScaleZ = Single.Parse(((Node)Source[id].Prop["scale_z"]).StringValue);
            RotX = Single.Parse(((Node)Source[id].Prop["dir_x"]).StringValue);
            RotY = Single.Parse(((Node)Source[id].Prop["dir_y"]).StringValue);
            RotZ = Single.Parse(((Node)Source[id].Prop["dir_z"]).StringValue);
            render.ChangeTransform(Type, id, new Vector3D(X, -Z, Y), new Vector3D(ScaleX, ScaleZ, ScaleY), RotX, -RotZ, RotY);
        }

        private void button5_Click(object sender, EventArgs e) //Remove values
        {
            if (comboBox1.Text == "AllRailInfos") { MessageBox.Show("You can't remove properties from rails"); return; }
            if (propertyGrid1.SelectedObject == null) return;
            if (!AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop.ContainsKey(propertyGrid1.SelectedGridItem.Label)) return;
            if (comboBox1.Text == "CameraAreaInfo" && propertyGrid1.SelectedGridItem.Label == "CameraId")
            {
                MessageBox.Show("You can't remove this value from a camera");
                return;
            }
            if (propertyGrid1.SelectedGridItem.Label.Contains("dir") || propertyGrid1.SelectedGridItem.Label.Contains("pos") || propertyGrid1.SelectedGridItem.Label.Contains("scale") || propertyGrid1.SelectedGridItem.Label.Contains("id") || propertyGrid1.SelectedGridItem.Label.ToLower().Contains("name"))
            {
                MessageBox.Show("You can't remove this value");
                return;
            }
            Action<string, int, string, object> action;
            action = (string type, int at, string propName, object property) =>
            {
                AllInfos[type].Objs[at].Prop.Add(propName, property);
                propertyGrid1.Refresh();
                propertyGrid1.Update();
            };
            object prop = AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop[propertyGrid1.SelectedGridItem.Label];
            Undo.Push(new UndoAction("Removed property: " + propertyGrid1.SelectedGridItem.Label, comboBox1.Text, ObjectsListBox.SelectedIndex, propertyGrid1.SelectedGridItem.Label, prop, action));
            AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop.Remove(propertyGrid1.SelectedGridItem.Label);
            propertyGrid1.Refresh();
            propertyGrid1.Update();
        }

        private void button4_Click(object sender, EventArgs e)//Add Value
        {
            if (comboBox1.Text == "AllRailInfos") { MessageBox.Show("You can't remove properties from rails"); return; }
            if (propertyGrid1.SelectedObject == null) return;
            FrmAddValue v = new FrmAddValue(AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex]);
            v.ShowDialog();
            if (v.resName != null && v.resName != "") AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop.Add(v.resName, v.result); else return;
            Action<string, int, object> action;
            action = (string type, int at, object propName) =>
            {
                AllInfos[type].Objs[at].Prop.Remove((string)propName);
                propertyGrid1.Refresh();
                propertyGrid1.Update();
            };
            Undo.Push(new UndoAction("Added property: " + v.resName, comboBox1.Text, ObjectsListBox.SelectedIndex, v.resName, action));
            propertyGrid1.Refresh();
        }

        private void button3_Click(object sender, EventArgs e) //Remove objects
        {
            DelSelectedObj();
        }

        void DelSelectedObj(bool NoUndo = false)
        {
            if (ObjectsListBox.SelectedIndex == -1) return;
            if (comboBox1.Text == "AllRailInfos")
            {
                if (!NoUndo)
                {
                    Rail tmp = AllRailInfos[ObjectsListBox.SelectedIndex].Clone();
                    Action<string, int, object> action;
                    action = (string type, int at, object rail) =>
                    {
                        AddRail((Rail)rail, at, true);
                    };
                    Undo.Push(new UndoAction("Removed rail: " + tmp.ToString(), "AllRailInfos", ObjectsListBox.SelectedIndex, tmp, action));
                }
                render.RemoveModel(comboBox1.Text, ObjectsListBox.SelectedIndex);
                AllRailInfos.RemoveAt(ObjectsListBox.SelectedIndex);
                ObjectsListBox.Items.RemoveAt(ObjectsListBox.SelectedIndex);
            }
            else
            {
                int[] indexes = GetSelectedIndexes();
                if (!NoUndo)
                {
                    List<LevelObj> tmp = new List<LevelObj>();
                    for (int i = 0; i < indexes.Length; i++) tmp.Add(AllInfos[comboBox1.Text].Objs[indexes[i]].Clone());
                    Action<string, int[], object> action;
                    action = (string type, int[] at, object obj) =>
                    {
                        LevelObj[] t = ((LevelObj[])obj).Reverse().ToArray();
                        int[] index = at.Reverse().ToArray();
                        for (int i = 0; i < t.Length; i++) AddObj(t[i], ref AllInfos[type].Objs, type, false, index[i], true);
                    };
                    string name = (indexes.Length == 1) ? "Removed object: " + tmp[0].ToString() : "Removed " + indexes.Length.ToString() + " objects";
                    Undo.Push(new UndoAction(name, comboBox1.Text, indexes, tmp.ToArray(), action));
                }
                foreach (int i in indexes)
                {
                    render.RemoveModel(comboBox1.Text, i);
                    AllInfos[comboBox1.Text].Objs.RemoveAt(i);
                    ObjectsListBox.Items.RemoveAt(i);
                }
            }
            propertyGrid1.SelectedObject = null;
            propertyGrid1.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)//Duplicating objects
        {
            if (ObjectsListBox.SelectedIndex == -1) return;
            if (comboBox1.Text == "AllRailInfos")
            {
                Rail tmp = new Rail();
                tmp = AllRailInfos[ObjectsListBox.SelectedIndex].Clone();
                AddRail(tmp);
            }
            else
            {
                AddObj(AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex], ref AllInfos[comboBox1.Text].Objs, comboBox1.Text);
            }
        }

        void AddRail(Rail r, int at = -1, bool IsUndo = false)
        {
            higestID["AllRailInfos"]++;
            r.l_id = higestID["AllRailInfos"];
            LoadRailsModels(r, at);
            if (at == -1) AllRailInfos.Add(r); else AllRailInfos.Insert(at, r);
            if (at == -1) ObjectsListBox.Items.Add(r.ToString()); else ObjectsListBox.Items.Insert(at, r.ToString());
            ObjectsListBox.SetSelected(at == -1 ? ObjectsListBox.Items.Count - 1 : at, true);
            if (!IsUndo)
            {
                Action<int, string> action;
                action = (int index, string type) =>
                {
                    AllRailInfos.RemoveAt(index);
                    ObjectsListBox.Items.RemoveAt(index);
                };
                Undo.Push(new UndoAction("Added rail: " + r.ToString(), "AllRailInfos", ObjectsListBox.Items.Count - 1, action));
            }
        }

        void AddObj(LevelObj inobj, ref List<LevelObj> list, string name, bool clone = true, int at = -1, bool IsUndo = false)
        {
            if (!higestID.ContainsKey(name)) higestID.Add(name, 0);
            higestID[name]++;
            LevelObj obj = new LevelObj();
            if (clone) obj = inobj.Clone(); else obj = inobj;
            if (obj.Prop.ContainsKey("l_id")) obj.Prop["l_id"] = new Node(higestID[name].ToString(), "D1");
            if (inobj.ToString() == "CameraArea") obj.Prop["CameraId"] = new Node(higestID[name].ToString(), "D1");
            if (at == -1) list.Add(obj); else list.Insert(at, obj);
            if (at == -1) ObjectsListBox.Items.Add(obj.ToString()); else ObjectsListBox.Items.Insert(at, obj.ToString());
            List<LevelObj> tmp = new List<LevelObj>();
            tmp.Add(obj);
            if (name == "AreaObjInfo") LoadModels(tmp, name, "models\\UnkYellow.obj", at);
            else if (name == "CameraAreaInfo") LoadModels(tmp, name, "models\\UnkGreen.obj", at);
            else LoadModels(tmp, name, "models\\UnkBlue.obj", at);
            ObjectsListBox.ClearSelected();
            ObjectsListBox.SetSelected(at == -1 ? ObjectsListBox.Items.Count - 1 : at, true);
            if (!IsUndo)
            {
                Action<int, string> action;
                action = (int index, string type) =>
                {
                    render.RemoveModel(type, index);
                    AllInfos[type].Objs.RemoveAt(index);
                    ObjectsListBox.Items.RemoveAt(index);
                };
                Undo.Push(new UndoAction("added object: " + obj.ToString(), comboBox1.Text, ObjectsListBox.Items.Count - 1, action));
            }
        }

        private void BtnAddObj_Click(object sender, EventArgs e)//Add new object
        {
            if (comboBox1.Text == "AllRailInfos")
            {
                AddRail(new Rail(true));
            }
            else
            {
                FrmAddObj frm = new FrmAddObj(CreatorClassNameTable.Keys.ToArray(), ObjDatabaseNames.ToArray(), comboBox1.Text);
                frm.ShowDialog();
                if (frm.Value == null) return;
                AddObj(frm.Value, ref AllInfos[comboBox1.Text].Objs, comboBox1.Text);
            }
        }

        private void pasteValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0)
            {
                if (clipboard[clipboard.Count - 1].Type != ClipBoardItem.ClipboardType.FullObject) return;
            }
            if (comboBox1.Text == "AllRailInfos" && !(clipboard[clipboard.Count - 1].Type == ClipBoardItem.ClipboardType.Rail || clipboard[clipboard.Count - 1].Type == ClipBoardItem.ClipboardType.IntArray)) return;
            PasteValue(ObjectsListBox.SelectedIndex, comboBox1.Text, clipboard[clipboard.Count - 1]);
            ClipBoardMenu.Close();
        }

        int[] GetSelectedIndexes()
        {
            if (ObjectsListBox.SelectedItems.Count == 0) return new int[] { -1 };
            int[] res = new int[ObjectsListBox.SelectedItems.Count];
            for (int i = 0; i < ObjectsListBox.SelectedItems.Count; i++) res[i] = ObjectsListBox.SelectedIndices[i];
            return res.Reverse().ToArray(); //From the last to the first
        }

        private void copyPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(GetSelectedIndexes(), comboBox1.Text, "pos_");
        }

        private void copyRotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(GetSelectedIndexes(), comboBox1.Text, "dir_");
        }

        private void copyScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(GetSelectedIndexes(), comboBox1.Text, "scale_");
        }

        private void ClipBoardMenu_CopyArgs_Click(object sender, EventArgs e)
        {
            //if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(GetSelectedIndexes(), comboBox1.Text, "Arg");
        }

        private void ClipBoardMenu_CopyFull_Click(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) return;
            int[] indexes = GetSelectedIndexes();
            if (indexes.Length == 1) CopyValue(indexes, comboBox1.Text, "Full");
            else CopyValue(indexes, comboBox1.Text, "FullArray");
        }

        private void Btn_CopyObjs_Click(object sender, EventArgs e)
        {
            int[] indexes = GetSelectedIndexes();
            if (indexes.Length > 1) CopyValue(indexes, comboBox1.Text, "FullArray");
        }

        void CopyValue(int[] indexes, string type, string value)
        {
            ClipBoardItem cl = new ClipBoardItem();
            int index = indexes[0];
            if (value == "pos_" || value == "dir_" || value == "scale_")
            {
                if (value == "pos_") cl.Type = ClipBoardItem.ClipboardType.Position;
                else if (value == "dir_") cl.Type = ClipBoardItem.ClipboardType.Rotation;
                else cl.Type = ClipBoardItem.ClipboardType.Scale;
                if (AllInfos[type].Objs[index].Prop.ContainsKey(value + "x") && AllInfos[type].Objs[index].Prop.ContainsKey(value + "y") && AllInfos[type].Objs[index].Prop.ContainsKey(value + "z"))
                {
                    cl.X = Single.Parse(((Node)AllInfos[type].Objs[index].Prop[value + "x"]).StringValue);
                    cl.Y = Single.Parse(((Node)AllInfos[type].Objs[index].Prop[value + "y"]).StringValue);
                    cl.Z = Single.Parse(((Node)AllInfos[type].Objs[index].Prop[value + "z"]).StringValue);
                }
                else MessageBox.Show("You can't copy this value from this object");
            }
            else if (value == "Arg")
            {
                if (comboBox1.Text == "AllRailInfos")
                {
                    cl.Type = ClipBoardItem.ClipboardType.IntArray;
                    cl.Args = AllRailInfos[index].Arg.ToArray();
                }
                else
                {
                    cl.Type = ClipBoardItem.ClipboardType.IntArray;
                    if (AllInfos[type].Objs[index].Prop.ContainsKey("Arg"))
                    {
                        cl.Args = (int[])((int[])AllInfos[type].Objs[index].Prop["Arg"]).Clone(); //This looks strange but (int[])AllInfos[type].Objs[index].Prop["Arg"] doesn't work
                    }
                    else MessageBox.Show("You can't copy this value from this object");
                }
            }
            else if (value == "Full")
            {
                if (comboBox1.Text == "AllRailInfos")
                {
                    cl.Type = ClipBoardItem.ClipboardType.Rail;
                    cl.Rail = AllRailInfos[index].Clone();
                }
                else
                {
                    cl.Type = ClipBoardItem.ClipboardType.FullObject;
                    cl.Objs = new LevelObj[] { AllInfos[type].Objs[index].Clone() };
                }
            }
            else if (value == "FullArray")
            {
                if (comboBox1.Text == "AllRailInfos")
                {
                    MessageBox.Show("Multi-Rail copy not implemented");
                }
                else
                {
                    cl.Type = ClipBoardItem.ClipboardType.ObjectArray;
                    List<LevelObj> l = new List<LevelObj>();
                    foreach (int i in indexes) l.Add(AllInfos[type].Objs[i].Clone());
                    cl.Objs = l.ToArray();
                }
            }
            clipboard.Add(cl);
            if (clipboard.Count > 10) clipboard.RemoveAt(0);
            ClipBoardMenu_Paste.DropDownItems.Clear();
            List<ToolStripMenuItem> Items = new List<ToolStripMenuItem>();
            for (int i = 0; i < clipboard.Count; i++)
            {
                ToolStripMenuItem btn = new ToolStripMenuItem();
                btn.Name = "ClipboardN" + i.ToString();
                btn.Text = clipboard[i].ToString();
                btn.Click += QuickClipboardItem_Click;
                Items.Add(btn);
            }
            Items.Reverse();
            ClipBoardMenu_Paste.DropDownItems.AddRange(Items.ToArray());
        }


        private void ClipBoardMenu_Opening(object sender, CancelEventArgs e)
        {
            if (GetSelectedIndexes().Length > 1)
            {
                ClipBoardMenu_CopyPos.Enabled = false;
                ClipBoardMenu_CopyRot.Enabled = false;
                ClipBoardMenu_CopyScale.Enabled = false;
                ClipBoardMenu_CopyArgs.Enabled = false;
                ClipBoardMenu_CopyFull.Text = "Copy objects";
                ClipBoardMenu_Paste.Enabled = false;
            }
            else
            {
                ClipBoardMenu_CopyArgs.Enabled = true;
                ClipBoardMenu_CopyFull.Text = "Copy full object";
                ClipBoardMenu_Paste.Enabled = true;
                if (comboBox1.Text == "AllRailInfos")
                {
                    ClipBoardMenu_CopyPos.Enabled = false;
                    ClipBoardMenu_CopyRot.Enabled = false;
                    ClipBoardMenu_CopyScale.Enabled = false;
                }
                else
                {
                    ClipBoardMenu_CopyPos.Enabled = true;
                    ClipBoardMenu_CopyRot.Enabled = true;
                    ClipBoardMenu_CopyScale.Enabled = true;
                }
                ClipBoardMenu_Paste.DropDownItems.Clear();
                List<ToolStripMenuItem> Items = new List<ToolStripMenuItem>();
                for (int i = 0; i < clipboard.Count; i++)
                {
                    ToolStripMenuItem btn = new ToolStripMenuItem();
                    btn.Name = "ClipboardN" + i.ToString();
                    btn.Text = clipboard[i].ToString(ObjectsListBox.SelectedIndex);
                    btn.Click += QuickClipboardItem_Click;
                    Items.Add(btn);
                }
                Items.Reverse();
                ClipBoardMenu_Paste.DropDownItems.AddRange(Items.ToArray());
            }
        }

        private void QuickClipboardItem_Click(object sender, EventArgs e)
        {
            string SenderName = ((ToolStripMenuItem)sender).Name;
            int index = int.Parse(SenderName.Substring("ClipboardN".Length));
            if (ObjectsListBox.SelectedIndex < 0)
            {
                if (clipboard[index].Type != ClipBoardItem.ClipboardType.FullObject) return;
            }
            if (comboBox1.Text == "AllRailInfos" && !(clipboard[index].Type == ClipBoardItem.ClipboardType.Rail || clipboard[index].Type == ClipBoardItem.ClipboardType.IntArray)) return;
            PasteValue(ObjectsListBox.SelectedIndex, comboBox1.Text, clipboard[index]);
        }

        void PasteValue(int index, string type, ClipBoardItem itm)
        {
            if (index >= 0)
            {
                Action<string, int, object> act;
                act = (string _type, int id, object Inobj) =>
                {
                    if (_type == "AllRailInfos")
                    {
                        AllRailInfos[id] = ((Rail)Inobj);
                    }
                    else
                    {
                        AllInfos[_type].Objs[id] = ((LevelObj)Inobj);
                        UpdateOBJPos(id, ref AllInfos[_type].Objs, _type);
                    }
                };
                object obj = type == "AllRailInfos" ? (object)AllRailInfos[index].Clone() : AllInfos[type].Objs[index].Clone();
                Undo.Push(new UndoAction("Pasted value to object " + obj.ToString(), type, index, obj, act));
            }
            if (itm.Type == ClipBoardItem.ClipboardType.Position)
            {
                if (AllInfos[type].Objs[index].Prop.ContainsKey("pos_x")) ((Node)AllInfos[type].Objs[index].Prop["pos_x"]).StringValue = itm.X.ToString();
                if (AllInfos[type].Objs[index].Prop.ContainsKey("pos_y")) ((Node)AllInfos[type].Objs[index].Prop["pos_y"]).StringValue = itm.Y.ToString();
                if (AllInfos[type].Objs[index].Prop.ContainsKey("pos_z")) ((Node)AllInfos[type].Objs[index].Prop["pos_z"]).StringValue = itm.Z.ToString();
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.Rotation)
            {
                if (AllInfos[type].Objs[index].Prop.ContainsKey("dir_x")) ((Node)AllInfos[type].Objs[index].Prop["dir_x"]).StringValue = itm.X.ToString();
                if (AllInfos[type].Objs[index].Prop.ContainsKey("dir_y")) ((Node)AllInfos[type].Objs[index].Prop["dir_y"]).StringValue = itm.Y.ToString();
                if (AllInfos[type].Objs[index].Prop.ContainsKey("dir_z")) ((Node)AllInfos[type].Objs[index].Prop["dir_z"]).StringValue = itm.Z.ToString();
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.Scale)
            {
                if (AllInfos[type].Objs[index].Prop.ContainsKey("scale_x")) ((Node)AllInfos[type].Objs[index].Prop["scale_x"]).StringValue = itm.X.ToString();
                if (AllInfos[type].Objs[index].Prop.ContainsKey("scale_y")) ((Node)AllInfos[type].Objs[index].Prop["scale_y"]).StringValue = itm.Y.ToString();
                if (AllInfos[type].Objs[index].Prop.ContainsKey("scale_z")) ((Node)AllInfos[type].Objs[index].Prop["scale_z"]).StringValue = itm.Z.ToString();
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.IntArray)
            {
                if (type == "AllRailInfos")
                {
                    AllRailInfos[index].Arg = itm.Args.ToList();
                    return;
                }
                else
                {
                    if (AllInfos[type].Objs[index].Prop.ContainsKey("Arg")) AllInfos[type].Objs[index].Prop["Arg"] = itm.Args.Clone();
                    else AllInfos[type].Objs[index].Prop.Add("Arg", itm.Args.Clone());
                }
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.Rail)
            {
                if (type == "AllRailInfos")
                {
                    int id = AllRailInfos[index].l_id;
                    int no = AllRailInfos[index].no;
                    string name = AllRailInfos[index].Name;
                    AllRailInfos[index] = itm.Rail.Clone();
                    AllRailInfos[index].l_id = id;
                    AllRailInfos[index].no = no;
                    AllRailInfos[index].Name = name;
                    ObjectsListBox_SelectedIndexChanged(null, null);
                    return;
                }
                else
                {
                    if (AllInfos[type].Objs[index].Prop.ContainsKey("Rail")) AllInfos[type].Objs[index].Prop["Rail"] = itm.Rail.Clone();
                    else AllInfos[type].Objs[index].Prop.Add("Rail", itm.Rail.Clone());
                }
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.FullObject)
            {
                if (index < 0 || propertyGrid1.SelectedObject == null) AddObj(itm.Objs[0], ref AllInfos[type].Objs, type, true);
                else
                {
                    if (!AllInfos[type].Objs[index].Prop.ContainsKey("GenerateChildren")) AllInfos[type].Objs[index].Prop.Add("GenerateChildren", new C0List());
                    ((C0List)AllInfos[type].Objs[index].Prop["GenerateChildren"]).List.Add(itm.Objs[0].Clone());
                }
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.ObjectArray)
            {
                if (index < 0 || propertyGrid1.SelectedObject == null) foreach (LevelObj o in itm.Objs) AddObj(o, ref AllInfos[type].Objs, type, true);
                else
                {
                    if (!AllInfos[type].Objs[index].Prop.ContainsKey("GenerateChildren")) AllInfos[type].Objs[index].Prop.Add("GenerateChildren", new C0List());
                    foreach (LevelObj o in itm.Objs) ((C0List)AllInfos[type].Objs[index].Prop["GenerateChildren"]).List.Add(o.Clone());
                }
            }
            propertyGrid1.Refresh();
            if (index >= 0) UpdateOBJPos(index, ref AllInfos[type].Objs, comboBox1.Text);
        }

        void FindIndex(string Type, string PropertyName, string Value)
        {
            Form ToClose = Application.OpenForms["FrmSearchResults"];
            if (ToClose != null) ToClose.Close();
            List<string> HitsNames = new List<string>();
            List<int> HitsIndexes = new List<int>();
            if (Type == "AllRailInfos")
            {
                MessageBox.Show("You can't search here");
            }
            else
            {
                for (int i = 0; i < AllInfos[Type].Objs.Count; i++)
                {
                    if (AllInfos[Type].Objs[i].Prop.ContainsKey(PropertyName) && AllInfos[Type].Objs[i].Prop[PropertyName] is Rail)
                    {
                        if (((Rail)AllInfos[Type].Objs[i].Prop[PropertyName]).Name.ToLower() == Value.ToLower()) { HitsNames.Add(AllInfos[Type].Objs[i].ToString()); HitsIndexes.Add(i); }
                    }
                    if (AllInfos[Type].Objs[i].Prop.ContainsKey("GenerateChildren"))
                    {
                        C0List children = (C0List)AllInfos[Type].Objs[i].Prop["GenerateChildren"];
                        for (int ii = 0; ii < children.List.Count; ii++)
                        {
                            if (children.List[ii].Prop.ContainsKey(PropertyName) && children.List[ii].Prop[PropertyName] is Rail)
                            {
                                if (((Rail)children.List[ii].Prop[PropertyName]).Name.ToLower() == Value.ToLower()) { HitsNames.Add(children.List[ii].ToString() + " In GenerateChildren[" + ii.ToString() + "]"); HitsIndexes.Add(i); }
                            }
                        }
                    }
                }
            }
            if (HitsIndexes.Count == 0) { MessageBox.Show("Not found"); return; }
            else
            {
                FormEditors.FrmSearchResults f = new FormEditors.FrmSearchResults(Type, HitsNames, HitsIndexes, this);
                f.Text = "Search Results for: " + PropertyName + " = " + Value.ToString() + "  in " + Type;
                f.Show();
            }
        }

        void FindIndex(string Type, string PropertyName, int Value)
        {
            Form ToClose = Application.OpenForms["FrmSearchResults"];
            if (ToClose != null) ToClose.Close();
            List<string> HitsNames = new List<string>();
            List<int> HitsIndexes = new List<int>();
            if (Type == "AllRailInfos")
            {
                if (PropertyName == "l_id")
                {
                    for (int i = 0; i < AllRailInfos.Count; i++)
                    {
                        if (AllRailInfos[i].l_id == Value) { HitsNames.Add(AllRailInfos[i].ToString()); HitsIndexes.Add(i); }
                    }
                }
            }
            else
            {
                for (int i = 0; i < AllInfos[Type].Objs.Count; i++)
                {
                    if (AllInfos[Type].Objs[i].Prop.ContainsKey(PropertyName) && AllInfos[Type].Objs[i].Prop[PropertyName] is Node && ((Node)AllInfos[Type].Objs[i].Prop[PropertyName]).NodeType == Node.NodeTypes.Int)
                    {
                        if (((Node)AllInfos[Type].Objs[i].Prop[PropertyName]).StringValue == Value.ToString()) { HitsNames.Add(AllInfos[Type].Objs[i].ToString()); HitsIndexes.Add(i); }
                    }
                    if (AllInfos[Type].Objs[i].Prop.ContainsKey("GenerateChildren"))
                    {
                        C0List children = (C0List)AllInfos[Type].Objs[i].Prop["GenerateChildren"];
                        for (int ii = 0; ii < children.List.Count; ii++)
                        {
                            if (children.List[ii].Prop.ContainsKey(PropertyName) && children.List[ii].Prop[PropertyName] is Node && ((Node)children.List[ii].Prop[PropertyName]).NodeType == Node.NodeTypes.Int)
                            {
                                if (((Node)children.List[ii].Prop[PropertyName]).StringValue == Value.ToString()) { HitsNames.Add(children.List[ii].ToString() + " In GenerateChildren[" + ii.ToString() + "]"); HitsIndexes.Add(i); }
                            }
                        }
                    }
                }
            }
            if (HitsIndexes.Count == 0) { MessageBox.Show("Not found"); return; }
            else
            {
                FormEditors.FrmSearchResults f = new FormEditors.FrmSearchResults(Type, HitsNames, HitsIndexes, this);
                f.Text = "Search Results for: " + PropertyName + " = " + Value.ToString() + "  in " + Type;
                f.Show();
            }
        }

        public void SetSelectedObj(string Type, int Index)
        {
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Type);
            ObjectsListBox.ClearSelected();
            ObjectsListBox.SelectedIndex = Index;
        }
        #endregion

        #region Save
        private void saveAsBymlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
            sav.FileName = Path.GetFileNameWithoutExtension(LoadedFile);
            sav.Filter = "Szs file|*.Szs";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
                NDS.NitroSystem.FND.NARC SzsArch = new NDS.NitroSystem.FND.NARC();
                SFSDirectory dir = new SFSDirectory("", true);
                for (int i = 0; i < SzsFiles.Count; i++)
                {
                    SFSFile file = new SFSFile(i, SzsFiles.Keys.ToArray()[i], dir);
                    file.Data = SzsFiles.Values.ToArray()[i];
                    dir.Files.Add(file);
                }
                SFSFile StgData = new SFSFile(SzsFiles.Count, "StageData.byml", dir);
                StgData.Data = BymlConverter.GetByml(MakeXML());
                dir.Files.Add(StgData);
                SzsArch.FromFileSystem(dir);
                File.WriteAllBytes(sav.FileName, y.Compress(SzsArch.Write()));
                MessageBox.Show("Done !");
            }
        }

        private void saveAsXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
            sav.FileName = Path.GetFileNameWithoutExtension(LoadedFile);
            sav.Filter = "Xml file|*.xml";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sav.FileName, MakeXML(), DefEnc);
                MessageBox.Show("Done !");
            }
        }

        private void saveAsBymlToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
            sav.FileName = Path.GetFileNameWithoutExtension(LoadedFile);
            sav.Filter = "Byml file|*.byml";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sav.FileName, BymlConverter.GetByml(MakeXML()));
                MessageBox.Show("Done !");
            }
        }

        string MakeXML()
        {
            using (var stream = new MemoryStream())
            {
                using (var xr = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true, Encoding = DefEnc }))
                {
                    xr.WriteStartDocument();
                    xr.WriteStartElement("Root");
                    xr.WriteStartElement("isBigEndian");
                    xr.WriteAttributeString("Value", "False");
                    xr.WriteEndElement();
                    xr.WriteStartElement("BymlFormatVersion");
                    xr.WriteAttributeString("Value", ((uint)1).ToString());
                    xr.WriteEndElement(); 
                    xr.WriteStartElement("C1"); //Byml Root
                    xr.WriteStartElement("C1");
                    xr.WriteAttributeString("Name", "AllInfos");
                    List<string> keys = AllInfos.Keys.ToList();
                    keys.Sort(StringComparer.Ordinal);
                    foreach (string k in keys) if (AllInfos[k].Objs.Count != 0) WriteOBJInfoSection(xr, k, AllInfos[k].Objs);
                    xr.WriteEndElement();
                    xr.WriteStartElement("C1");
                    xr.WriteAttributeString("Name", "AllRailInfos");
                    if (AllRailInfos.Count != 0)
                    {
                        xr.WriteStartElement("C0");
                        xr.WriteAttributeString("Name", "RailInfo");
                        foreach (Rail r in AllRailInfos)
                        {
                            xr.WriteStartElement("C1");
                            WriteRail(xr, r);
                            xr.WriteEndElement();
                        }
                        xr.WriteEndElement();
                    }
                    xr.WriteEndElement();
                    xr.WriteStartElement("C0");
                    xr.WriteAttributeString("Name", "LayerInfos");
                    WriteLayerInfos(xr);
                    xr.WriteEndElement();
                    xr.WriteEndElement();
                    xr.WriteEndElement();
                    xr.Close();
                }

                return (DefEnc.GetString(stream.ToArray()));
            }
        }

        void WriteLayerInfos(XmlWriter xr)
        {
            //string[] LayerNames = new string[5] { "共通", "共通サブ", "シナリオ1", "シナリオ1＆2", "シナリオ1＆3" }; //PlaceHolders
            List<string> LayerNames = new List<string>();
            Dictionary<string, Dictionary<string, List<LevelObj>>> _AllInfos = new Dictionary<string, Dictionary<string, List<LevelObj>>>();
            List<string> keys = AllInfos.Keys.ToList();
            keys.Sort(StringComparer.Ordinal);
            foreach (string k in keys)
            {
                if (AllInfos[k].Objs.Count != 0)
                {
                    _AllInfos.Add(k, new Dictionary<string, List<LevelObj>>());
                    ProcessLayerNames(ref AllInfos[k].Objs, _AllInfos[k], ref LayerNames);
                }
            }
            for (int i = 0; i < LayerNames.Count; i++)
            {
                xr.WriteStartElement("C1");
                xr.WriteStartElement("C1");
                xr.WriteAttributeString("Name", "Infos");
                foreach (string k in _AllInfos.Keys)
                {
                    if (_AllInfos[k].ContainsKey(LayerNames[i])) WriteOBJInfoSection(xr, k, _AllInfos[k][LayerNames[i]]);
                }
                xr.WriteEndElement();
                xr.WriteStartElement("A0");
                xr.WriteAttributeString("Name", "LayerName");
                xr.WriteAttributeString("StringValue", GetEnglishName(LayerNames[i]));
                xr.WriteEndElement();
                xr.WriteEndElement();
            }
        }

        string GetEnglishName(string Name)
        {
            if (Name == "共通") return "Common";
            else if (Name == "共通サブ") return "CommonSub";
            else if (Name.StartsWith("シナリオ"))
            {
                if (Name.Length == "シナリオ1".Length) return "Scenario" + Name.Substring("シナリオ".Length, 1);
                else return "Scenario" + Name.Substring("シナリオ".Length, 1) + "And" + Name.Substring("シナリオ1＆".Length, 1);
            }
            else throw new Exception("Unsupported name !");
        }

        void ProcessLayerNames(ref List<LevelObj> list, Dictionary<string, List<LevelObj>> Dict, ref List<string> AllLayerNames)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string layerName = ((Node)list[i].Prop["LayerName"]).StringValue;
                if (Dict.ContainsKey(layerName)) Dict[layerName].Add(list[i]);
                else { Dict.Add(layerName, new List<LevelObj>()); Dict[layerName].Add(list[i]); }
                if (!AllLayerNames.Contains(layerName)) AllLayerNames.Add(layerName);
            }
            if (AllLayerNames.Count > 5) throw new Exception("Too many layer names !");
        }

        void WriteOBJInfoSection(XmlWriter xr, string name, List<LevelObj> list, string startelement = "C0")
        {
            xr.WriteStartElement(startelement);
            xr.WriteAttributeString("Name", name);
            foreach (LevelObj obj in list) WriteOBJ(xr, obj);
            xr.WriteEndElement();
        }

        void WriteOBJ(XmlWriter xr, LevelObj obj)
        {
            if (obj.ToString().StartsWith("@")) return; //for @CameraPositionHelper
            xr.WriteStartElement("C1");
            List<string> Keys = obj.Prop.Keys.ToList();
            Keys.Sort(StringComparer.Ordinal);
            foreach (string Key in Keys)
            {
                Object node = obj.Prop[Key];
                if (node is int[]) //Args
                {
                    int[] tmp = ((int[])node);
                    for (int i = 0; i < tmp.Length; i++)
                    {
                        xr.WriteStartElement("D1");
                        xr.WriteAttributeString("Name", Key + i.ToString());
                        xr.WriteAttributeString("StringValue", tmp[i].ToString());
                        xr.WriteEndElement();
                    }
                }
                else if (node is C0List)
                {
                    C0List tmp = (C0List)node;
                    xr.WriteStartElement("C0");
                    xr.WriteAttributeString("Name", Key);
                    foreach (LevelObj o in tmp.List) WriteOBJ(xr, o);
                    xr.WriteEndElement();
                }
                else if (node is Rail)
                {
                    Rail tmp = (Rail)node;
                    xr.WriteStartElement("C1");
                    xr.WriteAttributeString("Name", Key);
                    WriteRail(xr, tmp);
                    xr.WriteEndElement();
                }
                else
                {
                    Node tmp = (Node)node;
                    string startelement = tmp._StringNodeType;
                    if (tmp.NodeType == Node.NodeTypes.Empty) startelement = "A1";
                    else if (tmp.NodeType == Node.NodeTypes.String) startelement = "A0";
                    else if (tmp.NodeType == Node.NodeTypes.Int) startelement = "D1";
                    else if (tmp.NodeType == Node.NodeTypes.Single) startelement = "D2";
                    xr.WriteStartElement(startelement);
                    xr.WriteAttributeString("Name", Key);
                    xr.WriteAttributeString("StringValue", tmp.StringValue);
                    xr.WriteEndElement();
                }
            }
            xr.WriteEndElement();
        }

        void WriteRail(XmlWriter xr, Rail r)
        {
            for (int i = 0; i < r.Arg.Count; i++)
            {
                xr.WriteStartElement("D1");
                xr.WriteAttributeString("Name", "Arg" + i.ToString());
                xr.WriteAttributeString("StringValue", r.Arg[i].ToString());
                xr.WriteEndElement();
            }
            xr.WriteStartElement("A0");
            xr.WriteAttributeString("Name", "LayerName");
            xr.WriteAttributeString("StringValue", r.LayerName);
            xr.WriteEndElement();
            xr.WriteStartElement("A0");
            xr.WriteAttributeString("Name", "MultiFileName");
            xr.WriteAttributeString("StringValue", "StageData_tool");
            xr.WriteEndElement();
            xr.WriteStartElement("C0");
            xr.WriteAttributeString("Name", "Points");
            foreach (Rail.Point p in r.Points) writePoint(xr, p);
            xr.WriteEndElement();
            xr.WriteStartElement("A0");
            xr.WriteAttributeString("Name", "closed");
            xr.WriteAttributeString("StringValue", r._closed);
            xr.WriteEndElement();
            xr.WriteStartElement("D1");
            xr.WriteAttributeString("Name", "l_id");
            xr.WriteAttributeString("StringValue", r.l_id.ToString());
            xr.WriteEndElement();
            xr.WriteStartElement("A0");
            xr.WriteAttributeString("Name", "name");
            xr.WriteAttributeString("StringValue", r.Name);
            xr.WriteEndElement();
            xr.WriteStartElement("D1");
            xr.WriteAttributeString("Name", "no");
            xr.WriteAttributeString("StringValue", r.no.ToString());
            xr.WriteEndElement();
            xr.WriteStartElement("D1");
            xr.WriteAttributeString("Name", "num_pnt");
            xr.WriteAttributeString("StringValue", r.Points.Count.ToString());
            xr.WriteEndElement();
            xr.WriteStartElement("A0");
            xr.WriteAttributeString("Name", "type");
            xr.WriteAttributeString("StringValue", r.Type);
            xr.WriteEndElement();
        }

        void writePoint(XmlWriter xr, Rail.Point p)
        {
            xr.WriteStartElement("C1");
            for (int i = 0; i < p.Args.Count; i++)
            {
                xr.WriteStartElement("D1");
                xr.WriteAttributeString("Name", "Arg" + i.ToString());
                xr.WriteAttributeString("StringValue", p.Args[i].ToString());
                xr.WriteEndElement();
            }
            xr.WriteStartElement("D1");
            xr.WriteAttributeString("Name", "id");
            xr.WriteAttributeString("StringValue", p.ID.ToString());
            xr.WriteEndElement();
            for (int i = 0; i < p._X.Count; i++)
            {
                xr.WriteStartElement("D2");
                xr.WriteAttributeString("Name", "pnt" + i.ToString() + "_x");
                xr.WriteAttributeString("StringValue", p._X[i].ToString());
                xr.WriteEndElement();
                xr.WriteStartElement("D2");
                xr.WriteAttributeString("Name", "pnt" + i.ToString() + "_y");
                xr.WriteAttributeString("StringValue", p._Y[i].ToString());
                xr.WriteEndElement();
                xr.WriteStartElement("D2");
                xr.WriteAttributeString("Name", "pnt" + i.ToString() + "_z");
                xr.WriteAttributeString("StringValue", p._Z[i].ToString());
                xr.WriteEndElement();
            }
            xr.WriteEndElement();
        }

        #endregion

        private void oggToBcstmConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new BgmEditors.FrmMakeBcstm().ShowDialog();
        }
    }
}