﻿using ModelViewer;
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
using System.Net;
using The4Dimension.ObjectDB;
using The4Dimension.FormEditors;

namespace The4Dimension
{
    public partial class Form1 : Form
    {
        public static string ObjectDbLink = "https://raw.githubusercontent.com/exelix11/TheFourthDimension/master/ObjectsDb.xml";
        public UserControl1 render = new UserControl1();
        public Dictionary<string, string> LevelNameNum = new Dictionary<string, string>(); //WX-X, stageName
        int APP_VER = Int32.Parse(Application.ProductVersion.Replace(".", ""));
        string LoadedFile = "";
        bool AutoMoveCam = true;
        bool AddObjectOrigin = false;
        public static int ReleaseId = 11;

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
                        LevelNameNum.Add("W " + i.ToString() + "-" + y.ToString() + "  (" + lines[++nextIndex].Trim() + ")", lines[nextIndex].Trim());
                    }
                }
                for (int i = 3; i < 8; i++)
                {
                    for (int y = 1; y < 7; y++)
                    {
                        LevelNameNum.Add("W " + i.ToString() + "-" + y.ToString() + "  (" + lines[++nextIndex].Trim() + ")", lines[nextIndex].Trim());
                    }
                }
                for (int y = 1; y < 10; y++)
                {
                    LevelNameNum.Add("W 8-" + y.ToString() + "  (" + lines[++nextIndex].Trim() + ")", lines[nextIndex].Trim());
                }
                for (int y = 1; y < 6; y++)
                {
                    LevelNameNum.Add("W S1-" + y.ToString() + "  (" + lines[++nextIndex].Trim() + ")", lines[nextIndex].Trim());
                }
                for (int i = 10; i < 17; i++)
                {
                    for (int y = 1; y < 7; y++)
                    {
                        LevelNameNum.Add("W S" + (i - 8).ToString() + "-" + y.ToString() + "  (" + lines[++nextIndex].Trim() + ")", lines[nextIndex].Trim());
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
                render.CameraInertiaFactor = Properties.Settings.Default.CameraInertia;
                render.ShowFps = Properties.Settings.Default.ShowFps;
                render.ShowTriangleCount = Properties.Settings.Default.ShowTriCount;
                render.ShowDebugInfo = Properties.Settings.Default.ShowDbgInfo;
                render.CamMode = Properties.Settings.Default.CameraMode == 0 ? HelixToolkit.Wpf.CameraMode.Inspect : HelixToolkit.Wpf.CameraMode.WalkAround;
                render.ZoomSensitivity = Properties.Settings.Default.ZoomSen;
                render.RotationSensitivity = Properties.Settings.Default.RotSen;
                AutoMoveCam = Properties.Settings.Default.AutoMoveCam;
                AddObjectOrigin = Properties.Settings.Default.AddObjectOrigin;

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
        public Dictionary<string, AllInfoSection> AllInfos = new Dictionary<string, AllInfoSection>();
        List<Rail> AllRailInfos = new List<Rail>();
        public Dictionary<string, int> higestID = new Dictionary<string, int>();
        public Dictionary<string, string> CreatorClassNameTable = new Dictionary<string, string>();
        public CustomStack<UndoAction> Undo = new CustomStack<UndoAction>();
        public static List<ClipBoardItem> clipboard = new List<ClipBoardItem>();
        public static Encoding DefEnc = Encoding.GetEncoding("Shift-JIS");
        public ObjectDb ObjectDatabase = null;
        List<String> CustomModels = new List<string>();

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
                else
                {
                    Directory.CreateDirectory("models");
                    File.WriteAllBytes(@"models\baseModels.zip", Properties.Resources.BaseModels);
                    System.IO.Compression.ZipFile.ExtractToDirectory(@"models\baseModels.zip", @"models");
                    File.Delete(@"models\baseModels.zip");
                    Directory.CreateDirectory(@"models\Tex");
                    MessageBox.Show("You won't be able to see any model, to do the procedure delete the models folder");
                }
            }
            if (!Properties.Settings.Default.DownloadDb) LoadObjectDatabase();
            LoadCreatorClassNameTable();
            if (LoadedFile != "") LoadFile(LoadedFile);
            else SetUiLock(false);
            gameROMFSPathToolStripMenuItem.Text = "Game ROMFS path: " + Properties.Settings.Default.GamePath;
            if (Properties.Settings.Default.CheckUpdates || Properties.Settings.Default.DownloadDb)
            {
                StatusLbl.Visible = true;
                downloadLatestObjectDatabaseToolStripMenuItem.Enabled = true;
                StartupChecks.RunWorkerAsync();
            }
            if (Properties.Settings.Default.CheckUpdates)
            {
                StatusLbl.Text = "Checking updates";
                if (Properties.Settings.Default.DownloadDb) StatusLbl.Text += " and downloading database..."; else StatusLbl.Text += "...";
            }
            else if (Properties.Settings.Default.DownloadDb) StatusLbl.Text = "Downloading database...";
            if (Directory.Exists("CustomModels"))
            {
                foreach (string s in Directory.EnumerateFiles("CustomModels"))
                    if (s.EndsWith(".obj")) CustomModels.Add(Path.GetFileNameWithoutExtension(s));
            }
        }

        void UnloadLevel()
        {
            render.UnloadLevel();
            SzsFiles = null;

            C0ListEditingStack = new Stack<List<LevelObj>>();
            IsEditingC0List = false;
            SelectionIndex = new Stack<int>();
            InitialAllInfosSection = -1;
            C0EditingPanel.Visible = false;

            AllInfos = new Dictionary<string, AllInfoSection>();
            AllRailInfos = new List<Rail>();
            higestID = new Dictionary<string, int>();
            Undo = new CustomStack<UndoAction>();
            comboBox1.Items.Clear();
            ObjectsListBox.Items.Clear();
            propertyGrid1.SelectedObject = null;
            //if (MessageBox.Show("Keep clipboard ?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) clipboard = new List<ClipBoardItem>();
            LoadedFile = "";
            this.Text = LoadedFile == "" ? "The Fourth Dimension - by Exelix11" : "The Fourth Dimension - " + LoadedFile;
            SetUiLock(false);
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
            UnloadLevel();
            comboBox1.Items.Add("AllRailInfos");
            higestID.Add("AllRailInfos", 0);
            render.AddKey("AllRailInfos");
            comboBox1.SelectedIndex = 0;
            SetupSZS();
            SetUiLock(true);
            saveToolStripMenuItem.Enabled = false;
        }

        void SetUiLock(bool Lock)
        {
            splitContainer1.Enabled = Lock;
            saveAsXmlToolStripMenuItem.Enabled = Lock;
            saveAsBymlToolStripMenuItem1.Enabled = Lock;
            saveToolStripMenuItem.Enabled = Lock;
            generate2DSectionToolStripMenuItem.Enabled = Lock;
            UndoMenu.Enabled = Lock;
            findToolStripMenuItem.Enabled = Lock;
            label3.Text = "";
            if (Lock) ZoomCheckWarning.Start(); else ZoomCheckWarning.Stop();
            OtherLevelDataMenu.Enabled = Lock;
            saveAsSZSToolStripMenuItem.Enabled = Lock;
            generatePreloadFileListToolStripMenuItem.Enabled = Lock;
        }

        #region FileLoading
        public void LoadFile(string FilePath) //Checks the file type and then loads the file
        {
            UnloadLevel();
            if (Path.GetExtension(FilePath).ToLower() == ".xml")
            {
                LoadedFile = FilePath;
                SetUiLock(true);
                OpenFile(File.ReadAllText(FilePath, DefEnc));
                SetupSZS();
            }
            else if (Path.GetExtension(FilePath).ToLower() == ".byml")
            {
                LoadedFile = FilePath;
                SetUiLock(true);
                OpenFile(BymlConverter.GetXml(FilePath));
                SetupSZS();
            }
            else if (Path.GetExtension(FilePath).ToLower() == ".szs")
            {
                LoadedFile = FilePath;
                SetUiLock(true);
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
                    SetUiLock(false);
                }
            }
            else
            {
                LoadedFile = "";
                MessageBox.Show("File type not supported !");
                SetUiLock(false);
            }
            this.Text = LoadedFile == "" ? "The Fourth Dimension - by Exelix11" : "The Fourth Dimension - " + LoadedFile;
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
            ObjectDatabase = null;
            if (!File.Exists(@"objectdb.xml"))
            {
                MessageBox.Show("The object database wasn't found, some objects may not appear, and you won't be able to get informations about how to use objects, you can download the database from Help -> Download latest object database");
                return;
            } else
            {
                try { ObjectDatabase = ObjectDb.FromXml(File.ReadAllText(@"objectdb.xml")); }
                catch (Exception ex) { MessageBox.Show("Could't load the objects database: \r\n\r\n" + ex.Message); }
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
                if (k == "AreaObjInfo") LoadModels(AllInfos[k], k, "models\\UnkYellow.obj");
                else if (k == "CameraAreaInfo") LoadModels(AllInfos[k], k, "models\\UnkGreen.obj");
                else LoadModels(AllInfos[k], k);
            }
            if (AllInfos.ContainsKey("AreaObjInfo")) HideLayer("AreaObjInfo");
            if (AllInfos.ContainsKey("CameraAreaInfo")) HideLayer("CameraAreaInfo");
            checkBox1.Checked = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            if (comboBox1.Items.Contains("ObjInfo")) comboBox1.Text = "ObjInfo"; else comboBox1.Text = comboBox1.Items[0].ToString();
            if (ObjectsListBox.Items.Count > 0)
            {
                ObjectsListBox.SelectedIndex = 0;
                render.CameraToObj(CurrentAllInfosSectionName, 0);
            }
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
                string Path = GetModelname(Source[i].ToString());
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
                        Path = GetModelname(o.ToString());
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
            if (CustomModels.Contains(ObjName)) return "CustomModels\\" + ObjName + ".obj";
            else if (ObjectDatabase != null && ObjectDatabase.IdToModel.ContainsKey(ObjName)) return "models\\" + ObjectDatabase.IdToModel[ObjName] + ".obj";
            else return "models\\" + ObjName + ".obj";
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
            foreach (XmlNode N in xml) GetListByName(Type).Add(LoadOBJECT(N.ChildNodes, Type));
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
                    if (((AllInfoSection)CurrentAllInfosSection).IsHidden) checkBox1.Checked = true; else checkBox1.Checked = false;
                }
                else
                {
                    checkBox1.Visible = false;
                    checkBox2.Visible = true;
                }
            }
            for (int i = 0; i < CurrentAllInfosSection.Count; i++) ObjectsListBox.Items.Add(CurrentAllInfosSection[i].ToString());
        }

        private void render_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control || RenderIsDragging) return;
            object[] indexes = render.GetOBJ(sender, e); //indexes[0] string, [1] int
            if (indexes[0] == null) return; //this means indexes[0] = -1
            if ((string)indexes[0] == "SelectedRail" || (string)indexes[0] == "TmpChildrenObjs" || (string)indexes[0] == "TmpAreaChildrenObjs" || (IsEditingC0List && (string)indexes[0] != "C0EditingListObjs")) return;
            if ((ModifierKeys & Keys.Shift) == Keys.Shift && (string)indexes[0] == CurrentAllInfosSectionName)
            {
                ObjectsListBox.SelectedIndices.Add((int)indexes[1]);
            }
            else
            {
                if ((string)indexes[0] != "C0EditingListObjs") comboBox1.SelectedIndex = comboBox1.Items.IndexOf((string)indexes[0]);
                ObjectsListBox.ClearSelected();
                ObjectsListBox.SelectedIndex = (int)indexes[1];
            }
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
            else if (e.Key == Key.B && comboBox1.Text != "AllRailInfos")
            {
                if (IsEditingC0List) C0ListEditorGoBack();
            }
            if (ObjectsListBox.SelectedIndex == -1) return;
            if (comboBox1.Text == "AllRailInfos")
            {
                if (propertyGrid1.SelectedObject is Rail && e.Key == Key.N)
                {
                    Rail tmp = (Rail)propertyGrid1.SelectedObject;
                    tmp.Points.Add(tmp.Points[tmp.Points.Count - 1].Clone_increment());
                    propertyGrid1.SelectedObject = AllRailInfos[ObjectsListBox.SelectedIndex];
                    UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
                    render.SelectRail(AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
                }
            }
            if (e.Key == Key.Space) render.CameraToObj(CurrentAllInfosSectionName, ObjectsListBox.SelectedIndex);
            else if (e.Key == Key.OemPlus) { if (Btn_AddObj.Enabled == true) BtnAddObj_Click(null, null); } //Add obj
            else if (e.Key == Key.D && ObjectsListBox.SelectedItems.Count == 1) button2_Click(null, null); //Duplicate
            else if (e.Key == Key.Delete) button3_Click(null, null); //Delete obj
            else if (e.Key == Key.F) findToolStripMenuItem.ShowDropDown();
            else if (e.Key == Key.R && comboBox1.Text != "AllRailInfos") //Round selected object position to a multiple of 100
            {
                if (RenderIsDragging) return;
                string type = comboBox1.Text;
                int id = ObjectsListBox.SelectedIndex;
                ((Node)GetListByName(type)[id].Prop["pos_x"]).StringValue = (Math.Round(Single.Parse(((Node)GetListByName(type)[id].Prop["pos_x"]).StringValue) / 100d, 0) * 100).ToString();
                ((Node)GetListByName(type)[id].Prop["pos_y"]).StringValue = (Math.Round(Single.Parse(((Node)GetListByName(type)[id].Prop["pos_y"]).StringValue) / 100d, 0) * 100).ToString();
                ((Node)GetListByName(type)[id].Prop["pos_z"]).StringValue = (Math.Round(Single.Parse(((Node)GetListByName(type)[id].Prop["pos_z"]).StringValue) / 100d, 0) * 100).ToString();
                UpdateOBJPos(id, GetListByName(type), type);
                propertyGrid1.Refresh();
            }
            else if (e.Key == Key.C && comboBox1.Text != "AllRailInfos" && ObjectsListBox.SelectedItems.Count == 1)
            {
                if (CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop.ContainsKey("GenerateChildren"))
                {
                    EditC0List(CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"] as C0List);
                }
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
            else if (e.KeyCode == Keys.B && comboBox1.Text != "AllRailInfos")
            {
                if (IsEditingC0List) C0ListEditorGoBack();
            }
            if (ObjectsListBox.SelectedIndex == -1) return;
            if (e.KeyCode == Keys.Space) render.CameraToObj(CurrentAllInfosSectionName, ObjectsListBox.SelectedIndex);
            else if (e.KeyCode == Keys.D && e.Control && ObjectsListBox.SelectedItems.Count == 1) button2_Click(null, null); //Duplicate
            else if (e.KeyCode == Keys.Delete) button3_Click(null, null); //Delete obj
            else if (e.KeyCode == Keys.F && e.Control) findToolStripMenuItem.ShowDropDown();
            else if (comboBox1.Text != "AllRailInfos" && e.KeyCode == Keys.Oemplus) { if (Btn_AddObj.Enabled == true) BtnAddObj_Click(null, null); } //Add obj            
            else if (comboBox1.Text != "AllRailInfos" && e.KeyCode == Keys.R && e.Control) //Round selected object position to a multiple of 100
            {
                if (RenderIsDragging) return;
                string type = comboBox1.Text;
                int id = ObjectsListBox.SelectedIndex;
                ((Node)GetListByName(type)[id].Prop["pos_x"]).StringValue = (Math.Round(Single.Parse(((Node)GetListByName(type)[id].Prop["pos_x"]).StringValue) / 100d, 0) * 100).ToString();
                ((Node)GetListByName(type)[id].Prop["pos_y"]).StringValue = (Math.Round(Single.Parse(((Node)GetListByName(type)[id].Prop["pos_y"]).StringValue) / 100d, 0) * 100).ToString();
                ((Node)GetListByName(type)[id].Prop["pos_z"]).StringValue = (Math.Round(Single.Parse(((Node)GetListByName(type)[id].Prop["pos_z"]).StringValue) / 100d, 0) * 100).ToString();
                UpdateOBJPos(id, GetListByName(type), type);
                propertyGrid1.Refresh();
            }
            else if (e.KeyCode == Keys.C && comboBox1.Text != "AllRailInfos" && ObjectsListBox.SelectedItems.Count == 1)
            {
                if (CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop.ContainsKey("GenerateChildren"))
                {
                    EditC0List(CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"] as C0List);
                }
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
            int RoundTo = (ModifierKeys & Keys.Alt) == Keys.Alt ? 100 : ((ModifierKeys & Keys.Shift) == Keys.Shift ? 50 : 0);
            Vector3D NewPos = render.Drag(DraggingArgs, e, RoundTo);
            if (NewPos == null) return;
            if ((string)DraggingArgs[0] == "SelectedRail")
            {
                AllRailInfos[ObjectsListBox.SelectedIndex].Points[(int)DraggingArgs[1]].X = (float)NewPos.X;
                AllRailInfos[ObjectsListBox.SelectedIndex].Points[(int)DraggingArgs[1]].Y = (float)NewPos.Z;
                AllRailInfos[ObjectsListBox.SelectedIndex].Points[(int)DraggingArgs[1]].Z = -(float)NewPos.Y;
                UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
            }
            else if ((string)DraggingArgs[0] == "TmpChildrenObjs")
            {
                ((Node)((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List[(int)DraggingArgs[1]].Prop["pos_x"]).StringValue = NewPos.X.ToString();
                ((Node)((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List[(int)DraggingArgs[1]].Prop["pos_y"]).StringValue = NewPos.Z.ToString();
                ((Node)((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List[(int)DraggingArgs[1]].Prop["pos_z"]).StringValue = (-NewPos.Y).ToString();
                UpdateOBJPos((int)DraggingArgs[1], null, "TmpChildrenObjs");
            }
            else if ((string)DraggingArgs[0] != "AllRailInfos")
            {
                ((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_x"]).StringValue = NewPos.X.ToString();
                ((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_y"]).StringValue = NewPos.Z.ToString();
                ((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_z"]).StringValue = (-NewPos.Y).ToString();
                UpdateOBJPos((int)DraggingArgs[1], GetListByName((string)DraggingArgs[0]), (string)DraggingArgs[0]);
            }
            DraggingArgs[2] = NewPos;
        }

        void endDragging()
        {
            if (DraggingArgs[0] == null || DraggingArgs[1] == null || DraggingArgs[2] == null) return;
            if (IsEditingC0List && (string)DraggingArgs[0] != "C0EditingListObjs") return;
            if ((string)DraggingArgs[0] == "SelectedRail")
            {
                Action<object[]> act;
                act = (object[] args) =>
                {
                    AllRailInfos[(int)args[0]].Points[(int)args[1]].X = (float)((Vector3D)args[2]).X;
                    AllRailInfos[(int)args[0]].Points[(int)args[1]].Y = (float)((Vector3D)args[2]).Z;
                    AllRailInfos[(int)args[0]].Points[(int)args[1]].Z = -(float)((Vector3D)args[2]).Y;
                    UpdateRailpos((int)args[0], AllRailInfos[(int)args[0]].GetPointArray());
                    propertyGrid1.Refresh();
                };
                Undo.Push(new UndoAction("Moved " + ObjectsListBox.SelectedItem.ToString() + "'s point[" + DraggingArgs[1].ToString() + "] : ", new object[] { ObjectsListBox.SelectedIndex, (int)DraggingArgs[1], StartPos }, act));
            }
            else if ((string)DraggingArgs[0] == "TmpChildrenObjs")
            {
                Action<object[]> act;
                act = (object[] args) =>
                {
                    render.ClearSelection();
                    List<LevelObj> type = (List<LevelObj>)args[0];
                    int id = (int)args[1];
                    int idInList = (int)args[2];
                    Vector3D pos = (Vector3D)args[3];
                    SetComboboxToLevObjList(type);
                    ObjectsListBox.SelectedIndex = id;
                    ((Node)((C0List)type[id].Prop["GenerateChildren"]).List[idInList].Prop["pos_x"]).StringValue = pos.X.ToString();
                    ((Node)((C0List)type[id].Prop["GenerateChildren"]).List[idInList].Prop["pos_y"]).StringValue = pos.Y.ToString();
                    ((Node)((C0List)type[id].Prop["GenerateChildren"]).List[idInList].Prop["pos_z"]).StringValue = pos.Z.ToString();
                    propertyGrid1.Refresh();
                };
                Undo.Push(new UndoAction("Moved children object of: " + CurrentAllInfosSection[ObjectsListBox.SelectedIndex].ToString(), new object[] { CurrentAllInfosSection, ObjectsListBox.SelectedIndex, (int)DraggingArgs[1] , StartPos }, act));
            }
            else if ((string)DraggingArgs[0] != "AllRailInfos")
            {
                Action<object[]> act;
                act = (object[] args) =>
                {
                    render.ClearSelection();
                    List<LevelObj> type = (List<LevelObj>)args[0];
                    int id = (int)args[1];
                    Vector3D pos = (Vector3D)args[2];
                    ((Node)type[id].Prop["pos_x"]).StringValue = pos.X.ToString(); //These values were stored directly
                    ((Node)type[id].Prop["pos_y"]).StringValue = pos.Y.ToString();
                    ((Node)type[id].Prop["pos_z"]).StringValue = pos.Z.ToString();
                    string typename = (string)args[3];
                    if (typename != "C0EditingListObjs" || type.GetHashCode() == CurrentAllInfosSection.GetHashCode())
                    {
                        UpdateOBJPos(id, type, typename, true);
                    }
                    if (ObjectsListBox.SelectedIndex == id) RefreshTmpChildrenObjects();
                    propertyGrid1.Refresh();
                };
                Undo.Push(new UndoAction("Moved object : " + GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].ToString(), new object[] { GetListByName((string)DraggingArgs[0]), DraggingArgs[1], StartPos, DraggingArgs[0] }, act));
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
            if ((string)DraggingArgs[0] == "SelectedRail" && !IsEditingC0List)
            {
                StartPos = AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray()[(int)DraggingArgs[1]].ToVect();
                return;
            }
            else if ((string)DraggingArgs[0] == "TmpChildrenObjs")
            {
                StartPos = new Vector3D(
                     float.Parse(((Node)((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List[(int)DraggingArgs[1]].Prop["pos_x"]).StringValue),
                      float.Parse(((Node)((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List[(int)DraggingArgs[1]].Prop["pos_y"]).StringValue),
                      float.Parse(((Node)((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List[(int)DraggingArgs[1]].Prop["pos_z"]).StringValue));
            }
            else if ((string)DraggingArgs[0] == "C0EditingListObjs")
            {
                ObjectsListBox.ClearSelected();
                ObjectsListBox.SelectedIndex = (int)DraggingArgs[1];
                    StartPos = new Vector3D(float.Parse(((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_x"]).StringValue),
                       float.Parse(((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_y"]).StringValue),
                       float.Parse(((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_z"]).StringValue));
            }
            else if (!IsEditingC0List)
            {
                comboBox1.SelectedIndex = comboBox1.Items.IndexOf((string)DraggingArgs[0]);
                ObjectsListBox.ClearSelected();
                ObjectsListBox.SelectedIndex = (int)DraggingArgs[1];
                if ((string)DraggingArgs[0] != "AllRailInfos")
                    StartPos = new Vector3D(float.Parse(((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_x"]).StringValue),
                       float.Parse(((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_y"]).StringValue),
                       float.Parse(((Node)GetListByName((string)DraggingArgs[0])[(int)DraggingArgs[1]].Prop["pos_z"]).StringValue));
            }
            else { RenderIsDragging = false; return; }
        }

        private void ZoomCheckWarning_Tick(object sender, EventArgs e)
        {
            //Maybe not the best way to accomplish this, but it works...
            double d = Math.Abs(render.TooCloseCheck());
            if (d <= 0.1) label3.Text = "You are zooming in too much, the camera may glitch, zoom out to fix.";
            else if (d > 1000000)
            {
                if (ObjectsListBox.SelectedIndex == -1) label3.Text = "You are too far from the level, select an object and press space.";
                else label3.Text = "You are too far from the selected object, press space.";
            }
            else label3.Text = "";
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
            for (int i = 0; i < AllInfos[layerName].Count; i++) UpdateOBJPos(i, AllInfos[layerName], layerName);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox1.Checked)
                ShowLayer(comboBox1.Text);
            else HideLayer(comboBox1.Text);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            RefreshTmpChildrenObjects();
        }
        #endregion

        int AreaObjOldSelection = -1;
        int CameraAreaOldSelection = -1;

        private void ObjectsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_cameraCode.Visible = false;
            render.ClearTmpObjects();
            render.UnselectRail();
            render.ClearSelection();
            lblDescription.Text = "";
            lblDescription.Tag = -1;
            if (ObjectsListBox.SelectedIndex < 0) return;
            if (ObjectsListBox.SelectedItems.Count > 1)
            {
                Btn_CopyObjs.Visible = true;
                Btn_Duplicate.Visible = false;
                button4.Enabled = false;
                btn_delObj.Text = "Delete objects";
                if (comboBox1.Text != "AllRailInfos")
                {
                    object[] SelectedObjects = new object[ObjectsListBox.SelectedItems.Count];
                    int index = 0;
                    foreach (int i in ObjectsListBox.SelectedIndices) SelectedObjects[index++] = new DictionaryPropertyGridAdapter(CurrentAllInfosSection[i].Prop);
                    propertyGrid1.SelectedObjects = SelectedObjects;
                    render.SelectObjs(CurrentAllInfosSectionName, ObjectsListBox.SelectedIndices);
                }
                else propertyGrid1.SelectedObject = null;
                return;
            }
            else
            {
                Btn_CopyObjs.Visible = false;
                Btn_Duplicate.Visible = true;
                button4.Enabled = true;
                btn_delObj.Text = "Delete object";
                if (ObjectDatabase != null) UpdateHint();
            }

            if (comboBox1.Text == "AreaObjInfo")
            {
                propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop);
                if (((AllInfoSection)CurrentAllInfosSection).IsHidden)
                {
                    if (AreaObjOldSelection != -1 && AreaObjOldSelection < AllInfos["AreaObjInfo"].Count) render.ChangeTransform(comboBox1.Text, AreaObjOldSelection, render.Positions[comboBox1.Text][AreaObjOldSelection], new Vector3D(0, 0, 0), 0, 0, 0, false);
                    UpdateOBJPos(ObjectsListBox.SelectedIndex, CurrentAllInfosSection, comboBox1.Text);
                }
                AreaObjOldSelection = ObjectsListBox.SelectedIndex;
                return;
            }
            else if (comboBox1.Text == "CameraAreaInfo")
            {
                propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop);
                if (((AllInfoSection)CurrentAllInfosSection).IsHidden)
                {
                    if (CameraAreaOldSelection != -1 && AreaObjOldSelection < AllInfos["CameraAreaInfo"].Count) render.ChangeTransform(comboBox1.Text, CameraAreaOldSelection, render.Positions[comboBox1.Text][CameraAreaOldSelection], new Vector3D(0, 0, 0), 0, 0, 0, false);
                    UpdateOBJPos(ObjectsListBox.SelectedIndex, CurrentAllInfosSection, comboBox1.Text);
                }
                CameraAreaOldSelection = ObjectsListBox.SelectedIndex;
                if (CurrentAllInfosSection[ObjectsListBox.SelectedIndex].ToString() == "CameraArea") btn_cameraCode.Visible = true;
                return;
            }
            else if (comboBox1.Text == "AllRailInfos")
            {
                propertyGrid1.SelectedObject = AllRailInfos[ObjectsListBox.SelectedIndex];
                UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
                render.SelectRail(AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
             }
            else
            {
                render.SelectObjs(CurrentAllInfosSectionName, ObjectsListBox.SelectedIndices);
                propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop);
                RefreshTmpChildrenObjects();
            }
        }

        private void RefreshTmpChildrenObjects()
        {
            render.ClearTmpObjects();
            if (ObjectsListBox.SelectedIndex == -1) return;
            if (checkBox2.Checked && CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop.ContainsKey("GenerateChildren"))
            {
                AddChildrenModels((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"], false);
            }
            if (checkBox2.Checked && CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop.ContainsKey("AreaChildren"))
            {
                AddChildrenModels((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["AreaChildren"], true);
            }
        }

        private void ObjectsListBox_Doubleclick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) return;
            if (ObjectsListBox.SelectedItems.Count > 1) return;
            render.CameraToObj(CurrentAllInfosSectionName, ObjectsListBox.SelectedIndex);
        }

        void UpdateHint()
        {
            if (comboBox1.Text == "AllRailInfos")
            {
                lblDescription.Text = "";
                lblDescription.Tag = -1;
                return;
            }
            if (ObjectDatabase.Entries.ContainsKey(ObjectsListBox.SelectedItem.ToString()))
            {
                lblDescription.Text = ObjectDatabase.Entries[ObjectsListBox.SelectedItem.ToString()].notes;
                if (ObjectDatabase.Entries[ObjectsListBox.SelectedItem.ToString()].Known == 0)
                {
                    lblDescription.Text = "This object is not documented";
                    lblDescription.Tag = -1;
                }
                else
                {
                    if (ObjectDatabase.Entries[ObjectsListBox.SelectedItem.ToString()].Complete == 0)
                    {
                        lblDescription.Text += "\r\nThis object entry is not completed";
                    }
                    lblDescription.Tag = 1;
                    lblDescription.Text += "\r\n(Click for more)";
                }
            }
            else
            {
                lblDescription.Text = "This object is not in the database";
                lblDescription.Tag = -1;
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

        private void hotkeysListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hotkeys list:\r\n" +
                " Ctrl + Z : Undo\r\n" +
                " Space : Move the camera on the selected object\r\n" +
                " Ctrl + D : Duplicate selected object\r\n" +
                " + : Add a new object\r\n" +
                " Del : Delete selected object\r\n" +
                " Ctrl + R : Round the selected object position to a multiple of 100 (like Ctrl + alt + drag, but without dragging)\r\n" +
                " Ctrl + F : Open the search menu\r\n" +
                " C : If the selected object has a GenerateChildren C0List edit it\r\n" +
                " B : If you are editing a C0List go back\r\n\r\n" +
                "In the Objects list:\r\n" +
                " Click once on an object to select it\r\n" +
                " Double click an object to select it and move the camera to it\r\n" +
                " Left click to deselect every object\r\n"+
                "In the 3D view:\r\n" +
                " Ctrl + drag : Move object\r\n" +
                " Ctrl + Alt + drag : Move object snapping every 100 units\r\n" +
                " Ctrl + Shift + drag : Move object snapping every 50 units\r\n" +
                " (With a rail selected) N : Add a new point at the end of the rail\r\n" +
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
            render.SetSortFrequency(0.5);
        }

        private void Form_Deactivate(object sender, EventArgs e) //Stop sorting
        {
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
            if (ObjectDatabase == null)
            {
                MessageBox.Show("The database was not loaded, this is normal if it's being downloaded, wait until it's done");
                return;
            }
            Form f = Application.OpenForms["ObjectDbEditor"];
            if (f != null)
            {
                f.Focus();
                return;
            }
            ObjectDbEditor d = new ObjectDbEditor(ObjectDatabase);
            d.Show();
            //LoadObjectDatabase();
        }

        private void generate2DSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!AllInfos.ContainsKey("ObjInfo"))
            {
                MessageBox.Show("This level doesn't include the type ObjInfo, add it to use this function");
                return;
            }
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf("ObjInfo");
            Form f = Application.OpenForms["Frm2DSection"];
            if (f != null)
            {
                f.Focus();
                return;
            }
            Frm2DSection d = new Frm2DSection();
            d.Show();
        }

        private void lblDescription_Click(object sender, EventArgs e)
        {
            if (lblDescription.Tag.ToString() != "-1") new ObjectDB.ObjectDBView(ObjectDatabase.Entries[ObjectsListBox.SelectedItem.ToString()], ObjectsListBox.SelectedItem.ToString()).Show();
        }

        private void oggToBcstmConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new BgmEditors.FrmMakeBcstm().ShowDialog();
        }

        private void downloadLatestObjectDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("objectdb.xml"))
            {
                if (MessageBox.Show("An object database file already exists, if you download a new one it will be replaced, if you edited it your changes will be lost, do you want to continue ?", "Warning", MessageBoxButtons.YesNo) == DialogResult.No) return;
                if (File.Exists("objectdb.xml.bak")) File.Delete("objectdb.xml.bak");
                File.Copy("objectdb.xml", "objectdb.xml.bak");
                File.Delete("objectdb.xml");
            }
            try
            {
                new WebClient().DownloadFile(Properties.Settings.Default.DownloadDbLink == "" ? ObjectDbLink : Properties.Settings.Default.DownloadDbLink, "objectdb.xml");
                LoadObjectDatabase();
                File.Delete("objectdb.xml.bak");
                MessageBox.Show("Done !");
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error, the file wasn't downloaded: \r\n" + ex.Message);
                if (File.Exists("objectdb.xml.bak"))
                {
                    if (File.Exists("objectdb.xml")) File.Delete("objectdb.xml");
                    File.Copy("objectdb.xml.bak", "objectdb.xml");
                    File.Delete("objectdb.xml.bak");
                    MessageBox.Show("The backup was restored");
                }
                return;
            }
        }

        private void generatePreloadFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SzsFiles == null)
            {
                MessageBox.Show("Szs not loaded ?");
                return;
            }
            if (!SzsFiles.ContainsKey("PreLoadFileList1.byml"))
            {
                SzsFiles.Add("PreLoadFileList1.byml", new byte[] { 0x00 });
            }
            string PreLoadFileList = Properties.Resources.GenericPreloadList;
            string ObjsList = "";
            List<string> ProcessedItems = new List<string>();
            foreach (LevelObj o in AllInfos["ObjInfo"])
            {
                if (ProcessedItems.Contains(o.ToString())) continue;
                ProcessedItems.Add(o.ToString());
                if (ObjectDatabase.Entries.ContainsKey(o.ToString()) && ObjectDatabase.Entries[o.ToString()].files != "")
                {
                    foreach (string s in ObjectDatabase.Entries[o.ToString()].files.Split("\r\n"[0]))
                    {
                        if (s.StartsWith("/ObjectData") && s.EndsWith(".szs"))
                            ObjsList += "<C1>\r\n<A0 Name=\"Path\" StringValue=\"" + s + "\" />\r\n<A0 Name=\"Type\" StringValue=\"Archive\" />\r\n</C1>";
                    }
                }
            }
            PreLoadFileList = PreLoadFileList.Insert(252, ObjsList);
            if (SzsFiles.ContainsKey("PreLoadFileList2.byml"))
            {
                MessageBox.Show("This SZS contains more PreLoadFileList files, the result of the generation will be put in your clipboard as xml, open the file you want to replace and paste replacing everything, then click save (the layout of the text doesn't matter)");
                Clipboard.SetText(PreLoadFileList);
            }
            else
            {
                SzsFiles["PreLoadFileList1.byml"] = BymlConverter.GetByml(PreLoadFileList);
                MessageBox.Show("Done");
            }
        }
        #endregion

        #region LevelEditing
        bool IsEditingC0List = false;
        Stack<List<LevelObj>> C0ListEditingStack = new Stack<List<LevelObj>>();
        Stack<int> SelectionIndex = new Stack<int>();
        int InitialAllInfosSection = -1;
        List<LevelObj> dummyList = new List<LevelObj>();
        private List<LevelObj> CurrentAllInfosSection
        {
            get
            {
                if (IsEditingC0List) return C0ListEditingStack.Peek();
                else if (comboBox1.Text == "AllRailInfos") return dummyList;
                else return AllInfos[comboBox1.Text];
            }
        }

        private string CurrentAllInfosSectionName
        {
            get
            {
                if (IsEditingC0List) return "C0EditingListObjs";
                else return comboBox1.Text;
            }
        }

        public List<LevelObj> GetListByName(string name)
        {
                if (name == "C0EditingListObjs" || IsEditingC0List) return C0ListEditingStack.Peek();
                else return AllInfos[name];
        }

        void EditC0List(C0List list, bool addToStack = true) { if (list != null) EditC0List(list.List, addToStack); }
        public void EditC0List(List<LevelObj> list, bool addToStack = true)
        {
            if (addToStack && ObjectsListBox.SelectedItems.Count > 1)
            {
                MessageBox.Show("You can't edito more C0lists at once, edit only on one object then copy the children objects to the others");
                return;
            }           
            comboBox1.Text = "C0EditingListObjs";
            C0EditingPanel.Visible = true;
            IsEditingC0List = true;
            if (addToStack) {
                if (C0ListEditingStack.Count == 0) InitialAllInfosSection = comboBox1.SelectedIndex;
                C0ListEditingStack.Push(list);
                SelectionIndex.Push(ObjectsListBox.SelectedIndex);
            }
            ObjectsListBox.SelectedItems.Clear();
            render.ClearC0Objects();
            LoadModels(list, "C0EditingListObjs");
            ObjectsListBox.Items.Clear();
            if (!higestID.ContainsKey("C0EditingListObjs")) higestID.Add("C0EditingListObjs", 0); else higestID["C0EditingListObjs"] = 0;
            foreach (LevelObj o in list) if (o.Prop.ContainsKey("l_id")) if (Int32.Parse(((Node)o.Prop["l_id"]).StringValue) > higestID["C0EditingListObjs"]) higestID["C0EditingListObjs"] = Int32.Parse(((Node)o.Prop["l_id"]).StringValue);
            ObjectsListBox.Items.AddRange(list.ToArray());;
            propertyGrid1.SelectedObject = null;
        }

        private void C0ListEditorGoBack()
        {
            if (C0ListEditingStack.Count == 1)
            {
                render.ClearC0Objects();
                propertyGrid1.SelectedObject = null;
                IsEditingC0List = false;
                comboBox1.SelectedIndex = InitialAllInfosSection;
                comboBox1_SelectedIndexChanged(null,null);
                C0ListEditingStack.Pop();
                C0EditingPanel.Visible = false;
            }
            else
            {
                C0ListEditingStack.Pop();
                EditC0List(C0ListEditingStack.Peek(), false);
            }
            ObjectsListBox.SelectedIndex = SelectionIndex.Pop();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            C0ListEditorGoBack();
        }

        public void SetComboboxToLevObjList(List<LevelObj> list)
        {
            for (int i = 0; i < AllInfos.Keys.Count; i++) if (AllInfos.Values.ToArray()[i] == list) comboBox1.SelectedIndex = i;
        }

        private void CameraCode_click(object sender, EventArgs e)
        {
            int cameraId = int.Parse(((Node)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["CameraId"]).StringValue);
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
                    if (CameraParam.Contains("<D1 Name=\"UserGroupId\" StringValue=\"" + cameraId.ToString() + "\" />"))
                    {
                        FormEditors.FrmXmlEditor frm = new FormEditors.FrmXmlEditor(BymlConverter.GetXml(SzsFiles["CameraParam.byml"]), "CameraParam.byml", false, CameraParam.IndexOf("<D1 Name=\"UserGroupId\" StringValue=\"" + cameraId.ToString() + "\" />"));
                        frm.ShowDialog();
                        if (frm.XmlRes != null) SzsFiles["CameraParam.byml"] = BymlConverter.GetByml(frm.XmlRes);
                    }
                    else
                    {
                        FormEditors.FrmAddCameraSettings f = new FormEditors.FrmAddCameraSettings(CameraParam, cameraId, this);
                        f.ShowDialog();
                    }
                }
            }
        }

        private void AddType_click(object sender, EventArgs e) //Addtype
        {
            FormEditors.FrmStringInput f = new FormEditors.FrmStringInput();
            f.ShowDialog();
            string[] internalNames = new string[] {"C0EditingListObjs", "TmpChildrenObjs", "SelectedRail", "TmpAreaChildrenObjs" };
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

        bool warningShow = false;
        private void propertyGridChange(object s, PropertyValueChangedEventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) { MessageBox.Show("No object selected in the list"); return; }
            if (comboBox1.Text == "AllRailInfos")
            {
                UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
                Action<object[]> act;
                act = (object[] args) =>
                {
                    int id = (int)args[0];
                    string propName = (string)args[1];
                    object value = args[2];
                    AllRailInfos[id][propName] = value;
                    propertyGrid1.Refresh();
                    UpdateRailpos(id, ((Rail)value).GetPointArray());
                };
                Undo.Push(new UndoAction("Changed value: " + e.ChangedItem.Label + " of rail: " + AllRailInfos[ObjectsListBox.SelectedIndex].ToString(), new object[] { ObjectsListBox.SelectedIndex, e.ChangedItem.Label, e.OldValue }, act));
                ObjectsListBox.Items[ObjectsListBox.SelectedIndex] = AllRailInfos[ObjectsListBox.SelectedIndex].ToString();
                return;
            }
            else
            {
                string name = e.ChangedItem.Parent.Value is Node ? e.ChangedItem.Parent.Label : e.ChangedItem.Label;
                if (name == "name" || name == "l_id")
                {
                    if (!warningShow)
                    {
                        MessageBox.Show("You shouldn't mess up with the name or the l_id property of the objects, you should add a new object instead and copy the position from this object to the new one.\r\nYou can undo this action from the undo button", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        warningShow = true;
                    }
                    string path = GetModelname(CurrentAllInfosSection[ObjectsListBox.SelectedIndex].ToString());
                    if (!System.IO.File.Exists(path)) path = "models\\UnkBlue.obj";
                    if (name == "name") {
                      foreach(int i in ObjectsListBox.SelectedIndices)  render.ChangeModel(CurrentAllInfosSectionName, i, path);
                    }
                }
                UpdateOBJPos(ObjectsListBox.SelectedIndices,  CurrentAllInfosSection, CurrentAllInfosSectionName);
                if (ObjectsListBox.SelectedItems.Count == 1)
                {
                    Action<object[]> action;
                    action = (object[] args) =>
                    {
                        List<LevelObj> type = (List<LevelObj>)args[0];
                        render.ClearSelection();
                        string TypeName = (string)args[1];
                        int id = (int)args[2];
                        string propName = (string)args[3];
                        object value = args[4];
                        if (type[id].Prop[propName] is Node) ((Node)type[id].Prop[propName]).StringValue = value.ToString();
                        else
                            type[id].Prop[propName] = value;
                        propertyGrid1.Refresh();
                        if (type.GetHashCode() == CurrentAllInfosSection.GetHashCode()) UpdateOBJPos(id, type, TypeName,true);
                        if (propName == "name")
                        {
                            string path = GetModelname(type[id].ToString());
                            if (!System.IO.File.Exists(path)) path = "models\\UnkBlue.obj";
                            if (name == "name") render.ChangeModel(TypeName, id, path);
                            ObjectsListBox.Items[id] = type[id].ToString();
                        }
                    };
                    Undo.Push(new UndoAction("Changed value: " + name + " of object: " + CurrentAllInfosSection[ObjectsListBox.SelectedIndex].ToString(), new object[] { CurrentAllInfosSection, CurrentAllInfosSectionName, ObjectsListBox.SelectedIndex, name, e.OldValue }, action));
                }
                ObjectsListBox.Items[ObjectsListBox.SelectedIndex] = CurrentAllInfosSection[ObjectsListBox.SelectedIndex].ToString();
            }
        }

        public void UpdateSelectedRailView()
        {
            UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
        }

        public void RailPointsChanged(List<Rail.Point> OldPoints)
        {
            UpdateRailpos(ObjectsListBox.SelectedIndex, AllRailInfos[ObjectsListBox.SelectedIndex].GetPointArray());
            Action<object[]> act;
            act = (object[] args ) =>
            {
                int id = (int)args[0];
                AllRailInfos[id].Points = (List<Rail.Point>)args[1];
                propertyGrid1.Refresh();
                UpdateRailpos(id, AllRailInfos[id].GetPointArray());
            };
            Undo.Push(new UndoAction("Changed points of rail: " + AllRailInfos[ObjectsListBox.SelectedIndex].ToString(), new object[] { ObjectsListBox.SelectedIndex, OldPoints }, act));
        }

        public int GetObjectGenChidCount(string type, int index)
        {
            if (!CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop.ContainsKey("GenerateChildren")) return 0;
            else return ((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List.Count;
        }

        private void UpdateOBJPos(ListBox.SelectedIndexCollection selectedIndices, List<LevelObj> Source, string Type)
        {
            foreach (int i in selectedIndices) UpdateOBJPos(i, Source, Type);
        }

        public void UpdateOBJPos(int id, List<LevelObj> Source, string Type, bool isUndo = false)
        {
            if (Type == "TmpChildrenObjs")
            {
                Source = ((C0List)CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["GenerateChildren"]).List;
            }
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
            render.ChangeTransform(Type, id, new Vector3D(X, -Z, Y), new Vector3D(ScaleX, ScaleZ, ScaleY), RotX, -RotZ, RotY , (Type != "AreaObjInfo" && Type != "CameraAreaInfo" && !isUndo));
        }

        private void button5_Click(object sender, EventArgs e) //Remove values
        {
            if (comboBox1.Text == "AllRailInfos") { MessageBox.Show("You can't remove properties from rails"); return; }
            if (propertyGrid1.SelectedObject == null) return;
            if (!CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop.ContainsKey(propertyGrid1.SelectedGridItem.Label)) return;
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
            foreach (int i in ObjectsListBox.SelectedIndices)
            {
                Action<object[]> action;
                action = (object[] args) =>
                {
                    List<LevelObj> type = (List<LevelObj>)args[0];
                    int at = (int)args[1];
                    string propName = (string)args[2];
                    type[at].Prop.Add(propName, args[3]);
                    propertyGrid1.Refresh();
                    propertyGrid1.Update();
                };
                object prop = CurrentAllInfosSection[i].Prop[propertyGrid1.SelectedGridItem.Label];
                Undo.Push(new UndoAction("Removed property: " + propertyGrid1.SelectedGridItem.Label, new object[] { CurrentAllInfosSection, i, propertyGrid1.SelectedGridItem.Label, prop }, action));
                CurrentAllInfosSection[i].Prop.Remove(propertyGrid1.SelectedGridItem.Label);
            }
            propertyGrid1.Refresh();
            propertyGrid1.Update();
        }

        private void button4_Click(object sender, EventArgs e)//Add Value
        {
            if (comboBox1.Text == "AllRailInfos") { MessageBox.Show("You can't remove properties from rails"); return; }
            if (propertyGrid1.SelectedObject == null) return;
            FrmAddValue v = new FrmAddValue(CurrentAllInfosSection[ObjectsListBox.SelectedIndex]);
            v.ShowDialog();
            if (v.resName != null && v.resName != "") CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop.Add(v.resName, v.result); else return;
            Action<object[]> action;
            action = (object[] args) =>
            {
                List<LevelObj> type = (List<LevelObj>)args[0];
                int at = (int)args[1];
                type[at].Prop.Remove((string)args[2]);
                propertyGrid1.Refresh();
                propertyGrid1.Update();
            };
            Undo.Push(new UndoAction("Added property: " + v.resName,new object[] { CurrentAllInfosSection, ObjectsListBox.SelectedIndex, v.resName }, action));
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
                    Action<object[]> action;
                    action = (object[] args) =>
                    {
                        int at = (int)args[0];
                        AddRail((Rail)args[1], at, true);
                    };
                    Undo.Push(new UndoAction("Removed rail: " + tmp.ToString(), new object[] { ObjectsListBox.SelectedIndex, tmp }, action));
                }
                render.RemoveModel(CurrentAllInfosSectionName, ObjectsListBox.SelectedIndex);
                AllRailInfos.RemoveAt(ObjectsListBox.SelectedIndex);
                ObjectsListBox.Items.RemoveAt(ObjectsListBox.SelectedIndex);
            }
            else
            {
                int[] indexes = GetSelectedIndexes();
                if (!NoUndo)
                {
                    List<LevelObj> tmp = new List<LevelObj>();
                    for (int i = 0; i < indexes.Length; i++) tmp.Add(CurrentAllInfosSection[indexes[i]]);
                    Action<object[]> action;
                    action = (object[] args) =>
                    {
                        List<LevelObj> type = (List<LevelObj>)args[0];
                        int[] at = (int[])args[1];
                        LevelObj[] t = ((LevelObj[])args[2]).Reverse().ToArray();
                        int[] index = at.Reverse().ToArray();
                        for (int i = 0; i < t.Length; i++) AddObj(t[i], type, (string)args[3], false, index[i], type.GetHashCode());
                        RefreshTmpChildrenObjects();
                    };
                    string name = (indexes.Length == 1) ? "Removed object: " + tmp[0].ToString() : "Removed " + indexes.Length.ToString() + " objects";
                    Undo.Push(new UndoAction(name, new object[] {CurrentAllInfosSection, indexes, tmp.ToArray(), CurrentAllInfosSectionName }, action));
                }
                foreach (int i in indexes)
                {
                    render.RemoveModel(CurrentAllInfosSectionName, i);
                    CurrentAllInfosSection.RemoveAt(i);
                    ObjectsListBox.Items.RemoveAt(i);
                }
            }
            propertyGrid1.SelectedObject = null;
            propertyGrid1.Refresh();
        }

        private void button2_Click(object sender, EventArgs e)//Duplicating objects
        {
            if (ObjectsListBox.SelectedIndices.Count != 1) return;
            if (comboBox1.Text == "AllRailInfos")
            {
                Rail tmp = new Rail();
                tmp = AllRailInfos[ObjectsListBox.SelectedIndex].Clone();
                AddRail(tmp);
            }
            else
            {
                AddObj(CurrentAllInfosSection[ObjectsListBox.SelectedIndex],  CurrentAllInfosSection, CurrentAllInfosSectionName);
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
                Action<object[]> action;
                action = (object[] args) =>
                {
                    AllRailInfos.RemoveAt((int)args[0]);
                    ObjectsListBox.Items.RemoveAt((int)args[0]);
                };
                Undo.Push(new UndoAction("Added rail: " + r.ToString(), new object[] { ObjectsListBox.Items.Count - 1 }, action));
            }
        }

        void AddObj(LevelObj inobj, List<LevelObj> list, string name, bool clone = true, int at = -1, int UndoHash = -1)
        {
            if (!higestID.ContainsKey(name)) higestID.Add(name, 0);
            higestID[name]++;
            LevelObj obj = new LevelObj();
            if (clone) obj = inobj.Clone(); else obj = inobj;
            if (obj.Prop.ContainsKey("l_id")) obj.Prop["l_id"] = new Node(higestID[name].ToString(), "D1");
            if (inobj.ToString() == "CameraArea") obj.Prop["CameraId"] = new Node(higestID[name].ToString(), "D1");
            if (at == -1) list.Add(obj); else list.Insert(at, obj);
            if (UndoHash == -1 || CurrentAllInfosSection.GetHashCode() == UndoHash)
            {
                if (at == -1) ObjectsListBox.Items.Add(obj.ToString()); else ObjectsListBox.Items.Insert(at, obj.ToString());
                List<LevelObj> tmp = new List<LevelObj>();
                tmp.Add(obj);
                if (name == "AreaObjInfo") LoadModels(tmp, name, "models\\UnkYellow.obj", at);
                else if (name == "CameraAreaInfo") LoadModels(tmp, name, "models\\UnkGreen.obj", at);
                else LoadModels(tmp, name, "models\\UnkBlue.obj", at);
                ObjectsListBox.ClearSelected();
                ObjectsListBox.SetSelected(at == -1 ? ObjectsListBox.Items.Count - 1 : at, true);
            }
            if (UndoHash == -1)
            {
                Action<object[]> action;
                action = (object[] args) =>
                {
                    List<LevelObj> type = (List<LevelObj>)args[0];
                    int index = (int)args[1];
                    string typename = (string)args[2];
                    type.RemoveAt(index);
                    if (type.GetHashCode() == CurrentAllInfosSection.GetHashCode())
                    {
                        render.RemoveModel(typename, index);
                        ObjectsListBox.Items.RemoveAt(index);
                    }
                    render.ClearTmpObjects();
                };
                Undo.Push(new UndoAction("added object: " + obj.ToString(), new object[] { CurrentAllInfosSection, ObjectsListBox.Items.Count - 1, CurrentAllInfosSectionName }, action));
            }
        }  

        private void BtnAddObj_Click(object sender, EventArgs e)//Add new object
        {
            Vector3D pos = AddObjectOrigin ? new Vector3D(0,0,0) : render.GetPositionInView();
            if (comboBox1.Text == "AllRailInfos")
            {
                AddRail(new Rail(true, pos));
                if (AutoMoveCam) render.LookAt(pos);
            }
            else
            {
                FrmAddObj frm = new FrmAddObj(CreatorClassNameTable.Keys.ToArray(), ObjectDatabase, comboBox1.Text,pos );
                frm.ShowDialog();
                if (frm.Value == null) return;
                AddObj(frm.Value,  CurrentAllInfosSection, CurrentAllInfosSectionName);
                if (AutoMoveCam) render.LookAt(pos);
            }
        }

        private void pasteValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0)
            {
                if (clipboard[clipboard.Count - 1].Type != ClipBoardItem.ClipboardType.FullObject) return;
            }
            if (comboBox1.Text == "AllRailInfos" && !(clipboard[clipboard.Count - 1].Type == ClipBoardItem.ClipboardType.Rail || clipboard[clipboard.Count - 1].Type == ClipBoardItem.ClipboardType.IntArray)) return;
            PasteValue(ObjectsListBox.SelectedIndex, CurrentAllInfosSection,CurrentAllInfosSectionName, clipboard[clipboard.Count - 1]);
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
            CopyValue(GetSelectedIndexes(), CurrentAllInfosSectionName, "pos_");
        }

        private void copyRotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(GetSelectedIndexes(), CurrentAllInfosSectionName, "dir_");
        }

        private void copyScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(GetSelectedIndexes(), CurrentAllInfosSectionName, "scale_");
        }

        private void ClipBoardMenu_CopyArgs_Click(object sender, EventArgs e)
        {
            //if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(GetSelectedIndexes(), CurrentAllInfosSectionName, "Arg");
        }

        private void ClipBoardMenu_CopyFull_Click(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) return;
            int[] indexes = GetSelectedIndexes();
            if (indexes.Length == 1) CopyValue(indexes, CurrentAllInfosSectionName, "Full");
            else CopyValue(indexes, CurrentAllInfosSectionName, "FullArray");
        }

        private void Btn_CopyObjs_Click(object sender, EventArgs e)
        {
            int[] indexes = GetSelectedIndexes();
            if (indexes.Length > 1) CopyValue(indexes, CurrentAllInfosSectionName, "FullArray");
        }


        private void ClipBoardMenu_CopyRail_Click(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(GetSelectedIndexes(), CurrentAllInfosSectionName, "Rail");
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
                if (GetListByName(type)[index].Prop.ContainsKey(value + "x") && GetListByName(type)[index].Prop.ContainsKey(value + "y") && GetListByName(type)[index].Prop.ContainsKey(value + "z"))
                {
                    cl.X = Single.Parse(((Node)GetListByName(type)[index].Prop[value + "x"]).StringValue);
                    cl.Y = Single.Parse(((Node)GetListByName(type)[index].Prop[value + "y"]).StringValue);
                    cl.Z = Single.Parse(((Node)GetListByName(type)[index].Prop[value + "z"]).StringValue);
                }
                else MessageBox.Show("You can't copy this value from this object");
            }
            else if (value == "Arg")
            {
                if (type == "AllRailInfos")
                {
                    cl.Type = ClipBoardItem.ClipboardType.IntArray;
                    cl.Args = AllRailInfos[index].Arg.ToArray();
                }
                else
                {
                    cl.Type = ClipBoardItem.ClipboardType.IntArray;
                    if (GetListByName(type)[index].Prop.ContainsKey("Arg"))
                    {
                        cl.Args = (int[])((int[])GetListByName(type)[index].Prop["Arg"]).Clone(); //This looks strange but (int[])GetListByName(type)[index].Prop["Arg"] doesn't work
                    }
                    else MessageBox.Show("You can't copy this value from this object");
                }
            }
            else if (value == "Full")
            {
                if (type == "AllRailInfos")
                {
                    cl.Type = ClipBoardItem.ClipboardType.Rail;
                    cl.Rail = AllRailInfos[index].Clone();
                }
                else
                {
                    cl.Type = ClipBoardItem.ClipboardType.FullObject;
                    cl.Objs = new LevelObj[] { GetListByName(type)[index].Clone() };
                }
            }
            else if (value == "FullArray")
            {
                if (type == "AllRailInfos")
                {
                    MessageBox.Show("Multi-Rail copy not implemented");
                }
                else
                {
                    cl.Type = ClipBoardItem.ClipboardType.ObjectArray;
                    List<LevelObj> l = new List<LevelObj>();
                    foreach (int i in indexes) l.Add(GetListByName(type)[i].Clone());
                    cl.Objs = l.ToArray();
                }
            }
            else if (value == "Rail")
            {
                cl.Type = ClipBoardItem.ClipboardType.Rail;
                cl.Rail = ((Rail)GetListByName(type)[index].Prop["Rail"]).Clone();
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
                ClipBoardMenu_CopyRail.Visible = false;
            }
            else
            {
                ClipBoardMenu_CopyArgs.Enabled = true;
                ClipBoardMenu_Paste.Enabled = true;
                ClipBoardMenu_CopyRail.Visible = false;
                if (comboBox1.Text == "AllRailInfos")
                {
                    ClipBoardMenu_CopyPos.Enabled = false;
                    ClipBoardMenu_CopyRot.Enabled = false;
                    ClipBoardMenu_CopyScale.Enabled = false;
                    ClipBoardMenu_CopyFull.Text = "Copy rail";
                }
                else
                {
                    ClipBoardMenu_CopyFull.Text = "Copy full object";
                    ClipBoardMenu_CopyPos.Enabled = true;
                    ClipBoardMenu_CopyRot.Enabled = true;
                    ClipBoardMenu_CopyScale.Enabled = true;
                    if (ObjectsListBox.SelectedItems.Count == 1 && CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop.Keys.Contains("Rail") && CurrentAllInfosSection[ObjectsListBox.SelectedIndex].Prop["Rail"] is Rail)
                        ClipBoardMenu_CopyRail.Visible = true;
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
                if (clipboard[index].Type != ClipBoardItem.ClipboardType.FullObject && clipboard[index].Type != ClipBoardItem.ClipboardType.ObjectArray) return;
            }
            if (comboBox1.Text == "AllRailInfos" && !(clipboard[index].Type == ClipBoardItem.ClipboardType.Rail || clipboard[index].Type == ClipBoardItem.ClipboardType.IntArray)) return;
            PasteValue(ObjectsListBox.SelectedIndex, CurrentAllInfosSection,CurrentAllInfosSectionName, clipboard[index]);
        }

        public void PasteValue(int index, List<LevelObj> type, string TypeName, ClipBoardItem itm)
        {
            if (index >= 0)
            {
                Action<object[]> act;
                act = (object[] args) =>
                {
                    List<LevelObj> _type = (List<LevelObj>)args[0];
                    string _typeName = (string)args[1];
                    int id = (int)args[2];
                    object Inobj = args[3];
                    if (_typeName == "AllRailInfos")
                    {
                        AllRailInfos[id] = ((Rail)Inobj);
                    }
                    else
                    {
                        _type[id] = ((LevelObj)Inobj);
                        if (_type.GetHashCode() == CurrentAllInfosSection.GetHashCode()) UpdateOBJPos(id, _type, _typeName);
                    }
                };
                object obj = TypeName == "AllRailInfos" ? (object)AllRailInfos[index].Clone() : type[index].Clone();
                Undo.Push(new UndoAction("Pasted value to object " + obj.ToString(), new object[] { type, TypeName, index, obj }, act));
            }
            if (itm.Type == ClipBoardItem.ClipboardType.Position)
            {
                if (type[index].Prop.ContainsKey("pos_x")) ((Node)type[index].Prop["pos_x"]).StringValue = itm.X.ToString();
                if (type[index].Prop.ContainsKey("pos_y")) ((Node)type[index].Prop["pos_y"]).StringValue = itm.Y.ToString();
                if (type[index].Prop.ContainsKey("pos_z")) ((Node)type[index].Prop["pos_z"]).StringValue = itm.Z.ToString();
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.Rotation)
            {
                if (type[index].Prop.ContainsKey("dir_x")) (type[index].Prop["dir_x"] as Node).StringValue = itm.X.ToString();
                if (type[index].Prop.ContainsKey("dir_y")) ((Node)type[index].Prop["dir_y"]).StringValue = itm.Y.ToString();
                if (type[index].Prop.ContainsKey("dir_z")) ((Node)type[index].Prop["dir_z"]).StringValue = itm.Z.ToString();
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.Scale)
            {
                if (type[index].Prop.ContainsKey("scale_x")) ((Node)type[index].Prop["scale_x"]).StringValue = itm.X.ToString();
                if (type[index].Prop.ContainsKey("scale_y")) ((Node)type[index].Prop["scale_y"]).StringValue = itm.Y.ToString();
                if (type[index].Prop.ContainsKey("scale_z")) ((Node)type[index].Prop["scale_z"]).StringValue = itm.Z.ToString();
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.IntArray)
            {
                if (TypeName == "AllRailInfos")
                {
                    AllRailInfos[index].Arg = itm.Args.ToList();
                    return;
                }
                else
                {
                    if (type[index].Prop.ContainsKey("Arg")) type[index].Prop["Arg"] = itm.Args.Clone();
                    else type[index].Prop.Add("Arg", itm.Args.Clone());
                }
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.Rail)
            {
                if (TypeName == "AllRailInfos")
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
                    if (type[index].Prop.ContainsKey("Rail")) type[index].Prop["Rail"] = itm.Rail.Clone();
                    else type[index].Prop.Add("Rail", itm.Rail.Clone());
                }
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.FullObject)
            {
                if (index < 0 || propertyGrid1.SelectedObject == null) AddObj(itm.Objs[0],  type, TypeName, true);
                else
                {
                    string name = itm.Objs[0].ToString();
                    if (name == "ObjectChildArea" || name == "SwitchKeepOnArea" || name == "SwitchOnArea")
                    {
                        if (!type[index].Prop.ContainsKey("ChildrenArea")) type[index].Prop.Add("ChildrenArea", new C0List());
                        ((C0List)type[index].Prop["ChildrenArea"]).List.Add(itm.Objs[0].Clone());
                    }
                    else 
                    {
                        if (!type[index].Prop.ContainsKey("GenerateChildren")) type[index].Prop.Add("GenerateChildren", new C0List());
                        ((C0List)type[index].Prop["GenerateChildren"]).List.Add(itm.Objs[0].Clone());
                    }
                }
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.ObjectArray)
            {
                if (index < 0 || propertyGrid1.SelectedObject == null) foreach (LevelObj o in itm.Objs) AddObj(o,  type, TypeName, true);
                else
                {
                    if (index < 0 || propertyGrid1.SelectedObject == null) foreach (LevelObj o in itm.Objs) AddObj(o, type, TypeName, true);
                    foreach (LevelObj o in itm.Objs)
                    {
                        string name = o.ToString();
                        if (name == "ObjectChildArea" || name == "SwitchKeepOnArea" || name == "SwitchOnArea")
                        {
                            if (!type[index].Prop.ContainsKey("ChildrenArea")) type[index].Prop.Add("ChildrenArea", new C0List());
                            ((C0List)type[index].Prop["ChildrenArea"]).List.Add(o.Clone());
                        }
                        else
                        {
                            if (!type[index].Prop.ContainsKey("GenerateChildren")) type[index].Prop.Add("GenerateChildren", new C0List());
                            ((C0List)type[index].Prop["GenerateChildren"]).List.Add(o.Clone());
                        }
                    }
                }
            }
            propertyGrid1.Refresh();
            if (index >= 0) UpdateOBJPos(index,  type, CurrentAllInfosSectionName);
        }

        void FindIndex(string type, string PropertyName, string Value)
        {
            Form ToClose = Application.OpenForms["FrmSearchResults"];
            if (ToClose != null) ToClose.Close();
            List<string> HitsNames = new List<string>();
            List<int> HitsIndexes = new List<int>();
            if (type == "AllRailInfos")
            {
                MessageBox.Show("You can't search here");
            }
            else
            {
                for (int i = 0; i < GetListByName(type).Count; i++)
                {
                    if (GetListByName(type)[i].Prop.ContainsKey(PropertyName) && GetListByName(type)[i].Prop[PropertyName] is Rail)
                    {
                        if (((Rail)GetListByName(type)[i].Prop[PropertyName]).Name.ToLower() == Value.ToLower()) { HitsNames.Add(GetListByName(type)[i].ToString()); HitsIndexes.Add(i); }
                    }
                    if (GetListByName(type)[i].Prop.ContainsKey("GenerateChildren"))
                    {
                        C0List children = (C0List)GetListByName(type)[i].Prop["GenerateChildren"];
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
                FormEditors.FrmSearchResults f = new FormEditors.FrmSearchResults(type, HitsNames, HitsIndexes, this);
                f.Text = "Search Results for: " + PropertyName + " = " + Value.ToString() + "  in " + type;
                f.Show();
            }
        }

        void FindIndex(string type, string PropertyName, int Value)
        {
            Form ToClose = Application.OpenForms["FrmSearchResults"];
            if (ToClose != null) ToClose.Close();
            List<string> HitsNames = new List<string>();
            List<int> HitsIndexes = new List<int>();
            if (type == "AllRailInfos")
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
                for (int i = 0; i < GetListByName(type).Count; i++)
                {
                    if (GetListByName(type)[i].Prop.ContainsKey(PropertyName) && GetListByName(type)[i].Prop[PropertyName] is Node && ((Node)GetListByName(type)[i].Prop[PropertyName]).NodeType == Node.NodeTypes.Int)
                    {
                        if (((Node)GetListByName(type)[i].Prop[PropertyName]).StringValue == Value.ToString()) { HitsNames.Add(GetListByName(type)[i].ToString()); HitsIndexes.Add(i); }
                    }
                    if (GetListByName(type)[i].Prop.ContainsKey("GenerateChildren"))
                    {
                        C0List children = (C0List)GetListByName(type)[i].Prop["GenerateChildren"];
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
                FormEditors.FrmSearchResults f = new FormEditors.FrmSearchResults(type, HitsNames, HitsIndexes, this);
                f.Text = "Search Results for: " + PropertyName + " = " + Value.ToString() + "  in " +type;
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
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (LoadedFile.EndsWith(".szs", StringComparison.InvariantCultureIgnoreCase) && SzsFiles != null) SzsSave(LoadedFile);
            else XmlSave(LoadedFile, LoadedFile.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase));
        }

        void SetupSZS()
        {
            if (SzsFiles == null)
            {
                SzsFiles = new Dictionary<string, byte[]>();
                SzsFiles.Add("CameraParam.byml", BymlConverter.GetByml(Properties.Resources.GenericCameraParam));
                SzsFiles.Add("PreLoadFileList1.byml", BymlConverter.GetByml(Properties.Resources.GenericPreloadList));
                SzsFiles.Add("StageInfo1.byml", BymlConverter.GetByml(Properties.Resources.GenericStageInfo));
                for (int i = 0; i < SzsFiles.Keys.Count; i++)
                {
                    ToolStripMenuItem btn = new ToolStripMenuItem();
                    btn.Name = "LoadFile" + i.ToString();
                    btn.Text = SzsFiles.Keys.ToArray()[i];
                    btn.Click += LoadFileList_click;
                    OtherLevelDataMenu.DropDownItems.Add(btn);
                }
            }
        }

        void SzsSave(string filename)
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
            File.WriteAllBytes(filename, y.Compress(SzsArch.Write()));
            MessageBox.Show("Done !");
        }

        void XmlSave(string filename, bool BYML)
        {
            if (BYML) File.WriteAllBytes(filename, BymlConverter.GetByml(MakeXML()));
            else File.WriteAllText(filename, MakeXML(), DefEnc);
            MessageBox.Show("Done !");
        }

        private void saveAsSZSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
            sav.FileName = Path.GetFileNameWithoutExtension(LoadedFile);
            sav.Filter = "Szs file|*.Szs";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                SzsSave(sav.FileName);
                LoadedFile = sav.FileName;
                this.Text = LoadedFile == "" ? "The Fourth Dimension - by Exelix11" : "The Fourth Dimension - " + LoadedFile;
                saveToolStripMenuItem.Enabled = true;
            }
        }

        private void saveAsXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
            sav.FileName = Path.GetFileNameWithoutExtension(LoadedFile);
            sav.Filter = "Xml file|*.xml";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                XmlSave(sav.FileName, false);
                LoadedFile = sav.FileName;
                this.Text = LoadedFile == "" ? "The Fourth Dimension - by Exelix11" : "The Fourth Dimension - " + LoadedFile;
                saveToolStripMenuItem.Enabled = true;
            }
        }

        private void saveAsBymlToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
            sav.FileName = Path.GetFileNameWithoutExtension(LoadedFile);
            sav.Filter = "Byml file|*.byml";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                XmlSave(sav.FileName, true);
                LoadedFile = sav.FileName;
                this.Text = LoadedFile == "" ? "The Fourth Dimension - by Exelix11" : "The Fourth Dimension - " + LoadedFile;
                saveToolStripMenuItem.Enabled = true;
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
                    foreach (string k in keys) if (AllInfos[k].Count != 0) WriteOBJInfoSection(xr, k, AllInfos[k]);
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
                if (AllInfos[k].Count != 0)
                {
                    _AllInfos.Add(k, new Dictionary<string, List<LevelObj>>());
                    ProcessLayerNames( AllInfos[k], _AllInfos[k],  LayerNames);
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

        void ProcessLayerNames( List<LevelObj> list, Dictionary<string, List<LevelObj>> Dict,  List<string> AllLayerNames)
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

        #region Settings
        private void SettingsPanel_LostFocus(object sender, EventArgs e)
        {
            SettingsPanel.Visible = false;
            Properties.Settings.Default.CameraInertia = (double)CamInertiaUpDown.Value;
            render.CameraInertiaFactor = (double)CamInertiaUpDown.Value;
            Properties.Settings.Default.ShowFps = ChbFps.Checked;
            render.ShowFps = ChbFps.Checked;
            Properties.Settings.Default.ShowTriCount = ChbTriCount.Checked;
            render.ShowTriangleCount = ChbTriCount.Checked;
            Properties.Settings.Default.ShowDbgInfo = ChbDebugInfo.Checked;
            render.ShowDebugInfo = ChbDebugInfo.Checked;
            Properties.Settings.Default.CameraMode = cbCameraMode.SelectedIndex;
            render.CamMode = cbCameraMode.SelectedIndex == 0 ? HelixToolkit.Wpf.CameraMode.Inspect : HelixToolkit.Wpf.CameraMode.WalkAround;
            Properties.Settings.Default.ZoomSen = (double)ZoomSenUpDown.Value;
            render.ZoomSensitivity = (double)ZoomSenUpDown.Value;
            Properties.Settings.Default.RotSen = (double)RotSenUpDown.Value;
            render.RotationSensitivity = (double)RotSenUpDown.Value;
            Properties.Settings.Default.AutoMoveCam = ChbAddCameraMove.Checked;
            AutoMoveCam = ChbAddCameraMove.Checked;
            Properties.Settings.Default.CheckUpdates = ChbStartupUpdate.Checked;
            Properties.Settings.Default.DownloadDb = ChbStartupDb.Checked;
            Properties.Settings.Default.DownloadDbLink = tbUrl.Text;
            Properties.Settings.Default.AddObjectOrigin = chbAddObjectOrigin.Checked;
            AddObjectOrigin = chbAddObjectOrigin.Checked;
            Properties.Settings.Default.Save();
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SettingsPanel.Visible = true;
            CamInertiaUpDown.Value = (decimal)render.CameraInertiaFactor;
            ChbFps.Checked = render.ShowFps;
            ChbTriCount.Checked = render.ShowTriangleCount;
            ChbDebugInfo.Checked = render.ShowDebugInfo;
            cbCameraMode.SelectedIndex = render.CamMode == HelixToolkit.Wpf.CameraMode.Inspect ? 0 : 1;
            ZoomSenUpDown.Value = (decimal)render.ZoomSensitivity;
            RotSenUpDown.Value = (decimal)render.RotationSensitivity;
            ChbAddCameraMove.Checked = AutoMoveCam;
            ChbStartupUpdate.Checked = Properties.Settings.Default.CheckUpdates;
            ChbStartupDb.Checked = Properties.Settings.Default.DownloadDb;
            tbUrl.Text = Properties.Settings.Default.DownloadDbLink;
            chbAddObjectOrigin.Checked = AddObjectOrigin;
            SettingsPanel.Focus();
        }

        private void btn_url_Default_Click(object sender, EventArgs e)
        {
            tbUrl.Text = "http://neomariogalaxy.bplaced.net/objectdb/3dl_download.php";
        }
        #endregion

        private async void StartupChecks_DoWork(object sender, DoWorkEventArgs e)
        {
            string state = "";
            try
            {               
                if (Properties.Settings.Default.DownloadDb)
                {
                    state = "downloading object database";
                    if (File.Exists("objectdb.xml.bak")) File.Delete("objectdb.xml.bak");
                    if (File.Exists("objectdb.xml")) File.Move("objectdb.xml", "objectdb.xml.bak");
                    new WebClient().DownloadFile(Properties.Settings.Default.DownloadDbLink == "" ? ObjectDbLink : Properties.Settings.Default.DownloadDbLink, "objectdb.xml");
                    if (!File.Exists("objectdb.xml"))
                    {
                        MessageBox.Show("There was an error downloading the object database");
                        if (File.Exists("objectdb.xml.bak")) File.Move("objectdb.xml.bak", "objectdb.xml");
                    }
                }
                if (Properties.Settings.Default.CheckUpdates)
                {
                    state = "checking updates";
                    var githubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("TheFourthDimension"));
                    var ver = await githubClient.Repository.Release.GetAll("exelix11", "TheFourthDimension");
                    if (ver.Count > ReleaseId)
                    {
                        if (MessageBox.Show("There is a new version of the editor, do you want to open the github page ?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            System.Diagnostics.Process.Start("https://github.com/exelix11/TheFourthDimension/releases");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error" + state + ": " + ex.Message);
                if (File.Exists("objectdb.xml.bak") && !File.Exists("objectdb.xml")) File.Move("objectdb.xml.bak", "objectdb.xml");
            }
        }

        private void StartupChecks_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            StatusLbl.Text = "";
            StatusLbl.Visible = false;
            downloadLatestObjectDatabaseToolStripMenuItem.Enabled = true;
            if (ObjectDatabase == null) LoadObjectDatabase();
        }

        private void guideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/exelix11/TheFourthDimension/blob/master/guide.md");
        }
    }
}