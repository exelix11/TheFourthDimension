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

namespace The4Dimension
{
    public partial class Form1 : Form
    {
        UserControl1 render = new UserControl1();
        string LoadedFile = "";
        public Form1(string FileLoad = "")
        {
            InitializeComponent();
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

        Dictionary<string, byte[]> SzsFiles = null;
        Dictionary<string, AllInfoSection> AllInfos = new Dictionary<string, AllInfoSection>();
        List<Rail> AllRailInfos = new List<Rail>();
        Dictionary<string, int> higestID = new Dictionary<string, int>();
        Dictionary<string, string> ModelResolver = new Dictionary<string, string>(); //Converts names like BlockBrickCoins to BlockBrick.obj
        Dictionary<string, string> CreatorClassNameTable = new Dictionary<string, string>();
        CustomStack<UndoAction> Undo = new CustomStack<UndoAction>();
        public static List<ClipBoardItem> clipboard = new List<ClipBoardItem>();

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists("models"))
            {
                if (MessageBox.Show("You must convert every model from the game before you can use the editor, convert now ? (you need to have the extracted ROMFS of the game on your pc)", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    MessageBox.Show("Select the ObjectData folder inside the ROMFS");
                    ModelDumper dlg = new ModelDumper();
                    dlg.ShowDialog();
                    if (!Directory.Exists("models")) Application.Exit();
                }
                else Application.Exit();
            }
            LoadModelResolver();
            LoadCreatorClassNameTable();
            if (LoadedFile != "") LoadFile(LoadedFile);
            else SetUiLock(false,false);
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

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Add("AllRailInfos");
            higestID.Add("AllRailInfos", 0);
            comboBox1.SelectedIndex = 0;
            SetUiLock(false,true);
        }

        void SetUiLock(bool SZS,bool Lock)
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
        }

        #region FileLoading
        void LoadFile(string FilePath) //Checks the file type and then loads the file
        {
            if (Path.GetExtension(FilePath).ToLower() == ".xml")
            {
                SetUiLock(false, true);
                OpenFile(File.ReadAllText(FilePath, Encoding.GetEncoding(932)));
            }
            else if (Path.GetExtension(FilePath).ToLower() == ".byml")
            {
                SetUiLock(false, true);
                OpenFile(BymlConverter.GetXml(FilePath));
            }
            else if (Path.GetExtension(FilePath).ToLower() == ".szs")
            {
                SetUiLock(true, true);
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
            FormEditors.FrmXmlEditor frm = new FormEditors.FrmXmlEditor(BymlConverter.GetXml(SzsFiles[name]));
            frm.ShowDialog();
            if (frm.XmlRes != null) SzsFiles[name] = BymlConverter.GetByml(frm.XmlRes);
        }

        void LoadModelResolver()
        {
            string[] Text = File.ReadAllLines(@"models\ModelResolver.inf");
            if (Text[0] != "[ModelResolver]") return;
            foreach (string Line in Text)
            {
                if (!Line.StartsWith(";") && !Line.StartsWith("["))
                {
                    string[] Sections = Line.Split(';');
                    ModelResolver.Add(Sections[0].Substring(0, Sections[0].Length - 1), Sections[1]);
                }
            }
        }

        void LoadCreatorClassNameTable()
        {
            if (!File.Exists(@"CreatorClassNameTable.byml"))
            {
                MessageBox.Show("to add new objects you need CreatorClassNameTable.byml in the same folder as this program, this file is placed inside GameRomFS:SystemData\\CreatorClassNameTable.szs\r\nWithout this file you can only duplicate or delete objects.");
                BtnAddObj.Enabled = false;
                return;
            }
            string ConvertedCCN = BymlConverter.GetXml(File.ReadAllBytes(@"CreatorClassNameTable.byml"));
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

        string GetModelname(string ObjName)
        {
            foreach (string key in ModelResolver.Keys.ToArray())
            {
                if (ObjName.StartsWith(key.ToLower())) return "models\\" + ModelResolver[key];
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
                            P.X = _X;
                            P.Y = _Y;
                            P.Z = _Z;
                            Ret.Points.Add(P);
                        }
                    }
                    if (xNode.Attributes["Name"].Value == "l_id") if (Int32.Parse(xNode.Attributes["StringValue"].Value) > higestID[Type]) higestID[Type] = Int32.Parse(xNode.Attributes["StringValue"].Value);
                }
            }
            if (Args.Count != 0) Ret.Args = Args;
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
                    if (xNode.Attributes["Name"].Value.StartsWith("Arg")) Args.Add(Int32.Parse(xNode.Attributes["StringValue"].Value)); else
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
        #endregion

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ObjectsListBox.Items.Clear();
            propertyGrid1.SelectedObject = null;
            if (!AllInfos.ContainsKey(comboBox1.Text))
            {
                if (comboBox1.Text == "AllRailInfos")
                {
                    checkBox1.Visible = false;
                    for (int i = 0; i < AllRailInfos.Count; i++) ObjectsListBox.Items.Add(AllRailInfos[i].ToString());
                }
                else propertyGrid1.SelectedObject = null;
                return;
            }
            else
            {
                if (comboBox1.Text == "AreaObjInfo" || comboBox1.Text == "CameraAreaInfo")
                {
                    checkBox1.Visible = true;
                    if (AllInfos[comboBox1.Text].IsHidden) checkBox1.Checked = true; else checkBox1.Checked = false;
                }
                else checkBox1.Visible = false;
            }
            for (int i = 0; i < AllInfos[comboBox1.Text].Objs.Count; i++) ObjectsListBox.Items.Add(AllInfos[comboBox1.Text].Objs[i].ToString());
        }

        private void render_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control || RenderIsDragging) return;
            object[] indexes = render.GetOBJ(sender, e); //indexes[0] string, [1] int
            if (indexes[0] == null) return; //this means indexes[0] = -1
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf((string)indexes[0]);
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
            else if (e.Key == Key.OemPlus) { if (BtnAddObj.Enabled == true) BtnAddObj_Click(null, null); } //Add obj
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
            else if (e.KeyCode == Keys.Oemplus) { if (BtnAddObj.Enabled == true) BtnAddObj_Click(null, null); } //Add obj
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

        private void render_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed || (ModifierKeys & Keys.Control) != Keys.Control || !RenderIsDragging) { RenderIsDragging = false; return; }
            Vector3D NewPos = render.Drag(DraggingArgs, e, ((ModifierKeys & Keys.Alt) == Keys.Alt) ? true : false);
            if (NewPos == null) return;
            ((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_x"]).StringValue = NewPos.X.ToString();
            ((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_y"]).StringValue = NewPos.Z.ToString();
            ((Node)AllInfos[(string)DraggingArgs[0]].Objs[(int)DraggingArgs[1]].Prop["pos_z"]).StringValue = (-NewPos.Y).ToString();
            UpdateOBJPos((int)DraggingArgs[1], ref AllInfos[(string)DraggingArgs[0]].Objs, (string)DraggingArgs[0]);
            DraggingArgs[2] = NewPos;
        }

        void endDragging()
        {
            if (DraggingArgs[0] == null || DraggingArgs[1] == null || DraggingArgs[2] == null) return;
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
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf((string)DraggingArgs[0]);
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

        #endregion

        int AreaObjOldSelection = -1;
        int CameraAreaOldSelection = -1;

        private void ObjectsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) return;
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
                return;
            }
            else if (comboBox1.Text == "AllRailInfos") propertyGrid1.SelectedObject = AllRailInfos[ObjectsListBox.SelectedIndex];
            else
            {
                propertyGrid1.SelectedObject = new DictionaryPropertyGridAdapter(AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Prop);
                if (!RenderIsDragging) render.CameraToObj(comboBox1.Text, ObjectsListBox.SelectedIndex);
            }
        }
        #endregion

        #region LevelEditing

        private void button1_Click(object sender, EventArgs e) //Addtype
        {
            FormEditors.FrmStringInput f = new FormEditors.FrmStringInput();
            f.ShowDialog();
            if (f.Result == null) return;
            else if (f.Result.Trim() == "") return;
            else if (AllInfos.ContainsKey(f.Result)) MessageBox.Show("This type is already in use");
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
                Action<string, int, string, object> act;
                act = (string type, int id, string propName, object value) =>
                {
                    AllRailInfos[id][propName] = value;
                    propertyGrid1.Refresh();
                };
                Undo.Push(new UndoAction("Changed value: " + propertyGrid1.SelectedGridItem.Label + " of rail: " + AllRailInfos[ObjectsListBox.SelectedIndex].ToString(), comboBox1.Text, ObjectsListBox.SelectedIndex, propertyGrid1.SelectedGridItem.Label, e.OldValue, act));
                return;
            }
            else
            {
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
                string name = e.ChangedItem.Parent.Value is Node ? e.ChangedItem.Parent.Label : e.ChangedItem.Label;
                Undo.Push(new UndoAction("Changed value: " + name + " of object: " + AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].ToString(), comboBox1.Text, ObjectsListBox.SelectedIndex, name, e.OldValue, action));
            }
        }

        void UpdateOBJPos(int id, ref List<LevelObj> Source, string Type)
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
                AllRailInfos.RemoveAt(ObjectsListBox.SelectedIndex);
                ObjectsListBox.Items.RemoveAt(ObjectsListBox.SelectedIndex);
            }
            else
            {
                if (!NoUndo)
                {
                    LevelObj tmp = AllInfos[comboBox1.Text].Objs[ObjectsListBox.SelectedIndex].Clone();
                    Action<string, int, object> action;
                    action = (string type, int at, object obj) =>
                    {
                        AddObj((LevelObj)tmp, ref AllInfos[type].Objs, type, false, at, true);
                    };
                    Undo.Push(new UndoAction("Removed object: " + tmp.ToString(), comboBox1.Text, ObjectsListBox.SelectedIndex, tmp, action));
                }
                render.RemoveModel(comboBox1.Text, ObjectsListBox.SelectedIndex);
                AllInfos[comboBox1.Text].Objs.RemoveAt(ObjectsListBox.SelectedIndex);
                ObjectsListBox.Items.RemoveAt(ObjectsListBox.SelectedIndex);
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
            if (at == -1) AllRailInfos.Add(r); else AllRailInfos.Insert(at, r);
            if (comboBox1.Text == "AllRailInfos")
            {
                if (at == -1) ObjectsListBox.Items.Add(r.ToString()); else ObjectsListBox.Items.Insert(at, r.ToString());
                ObjectsListBox.SetSelected(at == -1 ? ObjectsListBox.Items.Count - 1 : at, true);
            }
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
            if (at == -1) list.Add(obj); else list.Insert(at, obj);
            if (at == -1) ObjectsListBox.Items.Add(obj.ToString()); else ObjectsListBox.Items.Insert(at, obj.ToString());
            List<LevelObj> tmp = new List<LevelObj>();
            tmp.Add(obj);
            if (name == "AreaObjInfo") LoadModels(tmp, name, "models\\UnkYellow.obj", at);
            else if (name == "CameraAreaInfo") LoadModels(tmp, name, "models\\UnkGreen.obj", at);
            else LoadModels(tmp, name, "models\\UnkBlue.obj", at);
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
                AddRail(new Rail());
            }
            else
            {
                FrmAddObj frm = new FrmAddObj(CreatorClassNameTable, comboBox1.Text);
                frm.ShowDialog();
                if (frm.Value == null) return;
                AddObj(frm.Value, ref AllInfos[comboBox1.Text].Objs, comboBox1.Text);
            }
        }

        private void pasteValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) return;
            if (comboBox1.Text == "AllRailInfos" && clipboard[clipboard.Count - 1].Type != ClipBoardItem.ClipboardType.Rail) return;
            PasteValue(ObjectsListBox.SelectedIndex, comboBox1.Text, clipboard[clipboard.Count - 1]);
            ClipBoardMenu.Close();
        }

        private void copyPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(ObjectsListBox.SelectedIndex, comboBox1.Text, "pos_");
        }

        private void copyRotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(ObjectsListBox.SelectedIndex, comboBox1.Text, "dir_");
        }

        private void copyScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(ObjectsListBox.SelectedIndex, comboBox1.Text, "scale_");
        }

        private void ClipBoardMenu_CopyArgs_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "AllRailInfos") return;
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(ObjectsListBox.SelectedIndex, comboBox1.Text, "Arg");
        }

        private void ClipBoardMenu_CopyFull_Click(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) return;
            CopyValue(ObjectsListBox.SelectedIndex, comboBox1.Text, "Full");
        }

        void CopyValue(int index, string type, string value)
        {
            ClipBoardItem cl = new ClipBoardItem();
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
                cl.Type = ClipBoardItem.ClipboardType.IntArray;
                if (AllInfos[type].Objs[index].Prop.ContainsKey("Arg"))
                {
                    cl.Args = (int[])((int[])AllInfos[type].Objs[index].Prop["Arg"]).Clone(); //This looks strange but (int[])AllInfos[type].Objs[index].Prop["Arg"] doesn't work
                }
                else MessageBox.Show("You can't copy this value from this object");
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
                    cl.Obj = AllInfos[type].Objs[index].Clone();
                }
            }
            clipboard.Add(cl);
            if (clipboard.Count > 5) clipboard.RemoveAt(0);
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

        private void QuickClipboardItem_Click(object sender, EventArgs e)
        {
            if (ObjectsListBox.SelectedIndex < 0) return;
            string SenderName = ((ToolStripMenuItem)sender).Name;
            int index = int.Parse(SenderName.Substring("ClipboardN".Length));
            if (comboBox1.Text == "AllRailInfos" && clipboard[index].Type != ClipBoardItem.ClipboardType.Rail) return;
            PasteValue(ObjectsListBox.SelectedIndex, comboBox1.Text, clipboard[index]);
        }

        void PasteValue(int index, string type, ClipBoardItem itm)
        {
            Action<string, int, object> act;
            act = (string _type, int id, object Inobj) =>
            {
                if (_type == "AllRailInfos")
                {
                    AllRailInfos[id] = ((Rail)Inobj).Clone();
                }
                else
                {
                    AllInfos[_type].Objs[id] = ((LevelObj)Inobj).Clone();
                    UpdateOBJPos(id, ref AllInfos[_type].Objs, _type);
                }
            };
            object obj = type == "AllRailInfos" ? (object)AllRailInfos[index] : AllInfos[type].Objs[index];
            Undo.Push(new UndoAction("Pasted value to object " + obj.ToString(), type, index, obj, act));
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
                if (AllInfos[type].Objs[index].Prop.ContainsKey("Arg")) AllInfos[type].Objs[index].Prop["Arg"] = itm.Args.Clone();
                else AllInfos[type].Objs[index].Prop.Add("Arg", itm.Args.Clone());
            }
            else if (itm.Type == ClipBoardItem.ClipboardType.Rail)
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
            else if (itm.Type == ClipBoardItem.ClipboardType.FullObject)
            {
                Node name, id;
                name = null;
                id = null;
                if (AllInfos[type].Objs[index].Prop.ContainsKey("name")) name = ((Node)AllInfos[type].Objs[index].Prop["name"]).Clone();
                if (AllInfos[type].Objs[index].Prop.ContainsKey("l_id")) id = ((Node)AllInfos[type].Objs[index].Prop["l_id"]).Clone();
                AllInfos[type].Objs[index] = itm.Obj.Clone();
                if (AllInfos[type].Objs[index].Prop.ContainsKey("name")) AllInfos[type].Objs[index].Prop["name"] = name;
                if (AllInfos[type].Objs[index].Prop.ContainsKey("l_id")) AllInfos[type].Objs[index].Prop["l_id"] = id;
            }
            propertyGrid1.Refresh();
            UpdateOBJPos(index, ref AllInfos[type].Objs, comboBox1.Text);
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
                    Debug.Print(AllInfos[Type].Objs[i].Prop.ContainsKey(PropertyName).ToString());
                    if (AllInfos[Type].Objs[i].Prop.ContainsKey(PropertyName) && AllInfos[Type].Objs[i].Prop[PropertyName] is Node && ((Node)AllInfos[Type].Objs[i].Prop[PropertyName]).NodeType == Node.NodeTypes.Int)
                    {
                        if (((Node)AllInfos[Type].Objs[i].Prop[PropertyName]).StringValue == Value.ToString()) { HitsNames.Add(AllInfos[Type].Objs[i].ToString()); HitsIndexes.Add(i); }
                    }
                }
            }
            if (HitsIndexes.Count == 0) { MessageBox.Show("Not found"); return; }
            else
            {
                FormEditors.FrmSearchResults f = new FormEditors.FrmSearchResults(Type, HitsNames, HitsIndexes,this);
                f.Text = "Search Results for: " + PropertyName + " = " + Value.ToString() + "  in " + Type;
                f.Show();
            }
        }

        public void SetSelectedObj(string Type, int Index)
        {
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Type);
            ObjectsListBox.SelectedIndex = Index;
        }
        #endregion

        #region Save
        private void saveAsBymlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
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
            sav.Filter = "Xml file|*.xml";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sav.FileName, MakeXML());
                MessageBox.Show("Done !");
            }
        }

        private void saveAsBymlToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sav = new SaveFileDialog();
            sav.Filter = "Byml file|*.byml";
            if (sav.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sav.FileName, BymlConverter.GetByml(MakeXML()));
                MessageBox.Show("Done !");
            }
        }

        string MakeXML()
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
            xr.WriteStartElement("C1"); //Byml Root
            xr.WriteStartElement("C1");
            xr.WriteAttributeString("Name", "AllInfos");
            foreach (string k in AllInfos.Keys) if (AllInfos[k].Objs.Count != 0) WriteOBJInfoSection(xr, k, AllInfos[k].Objs);
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
            return str.ToString();
        }

        void WriteLayerInfos(XmlTextWriter xr)
        {
            //string[] LayerNames = new string[5] { "共通", "共通サブ", "シナリオ1", "シナリオ1＆2", "シナリオ1＆3" }; //PlaceHolders
            List<string> LayerNames = new List<string>();
            Dictionary<string,Dictionary<string,List<LevelObj>>> _AllInfos = new Dictionary<string, Dictionary<string, List<LevelObj>>>();
            foreach (string k in AllInfos.Keys)
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
                else return "Scenario" + Name.Substring("シナリオ".Length , 1) + "And" + Name.Substring("シナリオ1＆".Length , 1);
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

        void WriteOBJInfoSection(XmlTextWriter xr,string name, List<LevelObj> list, string startelement = "C0")
        {
            xr.WriteStartElement(startelement);
            xr.WriteAttributeString("Name", name);
            foreach (LevelObj obj in list) WriteOBJ(xr, obj);
            xr.WriteEndElement();
        }

        void WriteOBJ(XmlTextWriter xr, LevelObj obj)
        {
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
                else if (node is C0List) //Usially GenerateChildren
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

        void WriteRail(XmlTextWriter xr, Rail r)
        {
            for (int i = 0; i < r.Args.Count; i++)
            {
                xr.WriteStartElement("D1");
                xr.WriteAttributeString("Name", "Arg" + i.ToString());
                xr.WriteAttributeString("StringValue", r.Args[i].ToString());
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

        void writePoint(XmlTextWriter xr, Rail.Point p)
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
            for (int i = 0; i < p.X.Count; i++)
            {
                xr.WriteStartElement("D2");
                xr.WriteAttributeString("Name", "pnt" + i.ToString()+"_x");
                xr.WriteAttributeString("StringValue", p.X[i].ToString());
                xr.WriteEndElement();
                xr.WriteStartElement("D2");
                xr.WriteAttributeString("Name", "pnt" + i.ToString() + "_y");
                xr.WriteAttributeString("StringValue", p.Y[i].ToString());
                xr.WriteEndElement();
                xr.WriteStartElement("D2");
                xr.WriteAttributeString("Name", "pnt" + i.ToString() + "_z");
                xr.WriteAttributeString("StringValue", p.Z[i].ToString());
                xr.WriteEndElement();
            }
            xr.WriteEndElement();
        }

        #endregion

        private void bymlXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Filter = "Byml File|*.Byml|*.*|*.*";
            SaveFileDialog sav = new SaveFileDialog();
            sav.Filter = "Xml file|*.xml";
            if (opn.ShowDialog() == DialogResult.OK && sav.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sav.FileName, BymlConverter.GetXml(File.ReadAllBytes(opn.FileName)), Encoding.GetEncoding(932));
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
                File.WriteAllBytes(sav.FileName, BymlConverter.GetByml(File.ReadAllText(opn.FileName, Encoding.GetEncoding(932))));
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
                " Space : Focus the camera on the selected object\r\n" +
                " Ctrl + D : Duplicate selected object\r\n" +
                " + : Add a new object\r\n" +
                " Del : Delete selected object\r\n" +
                " Ctrl + R : Round the selected object position to a multiple of 100 (like Ctrl + alt + drag, but without dragging)\r\n" +
                " Ctrl + F : Open the search menu\r\n" + 
                "In the 3D view:\r\n" +
                " Ctrl + drag : Move object\r\n" +
                " Ctrl + Alt + drag : Move object snapping every 100 units\r\n"+
                " -Every other combination without having to press Ctrl");
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
            while (ObjectsListBox.Items.Count != 0) { ObjectsListBox.SelectedIndex = 0; DelSelectedObj(true); }
        }
    }

    #region Other
    public class UndoAction
    {
        public string actionName;
        public string type;
        public int index;
        public Action<int,string> Action = null;
        private Action<string,int, object> ObjAddAction = null;
        object objToAdd = null;
        string propName = null;
        private Action<string, int, string, object> PropAddAction = null;
        private Action<string, int, Vector3D> MoveAction = null;

        public void Undo()
        {
            Form1 form1 = (Form1)Application.OpenForms[0]; //There is always one instance of this form
            form1.comboBox1.Text = type;
            if (form1.ObjectsListBox.SelectedIndex == index && PropAddAction == null) form1.ObjectsListBox.SelectedIndex = -1;
            if (Action != null) Action.Invoke(index, type);
            else if (ObjAddAction != null) ObjAddAction.Invoke(type, index, objToAdd);
            else if (PropAddAction != null) PropAddAction.Invoke(type, index, propName, objToAdd);
            else MoveAction.Invoke(type,index,(Vector3D)objToAdd);
            if (form1.ObjectsListBox.Items.Count > index) form1.ObjectsListBox.SelectedIndex = index;
        }

        public override string ToString()
        {
            return actionName;
        }

        public UndoAction(string name,string _type, int _index, Action<int, string> Act)
        {
            actionName = name;
            type = _type;
            index = _index;
            Action = Act;
        }

        public UndoAction(string name, string _type, int _index, Vector3D vec, Action<string,int, Vector3D> Act)
        {
            actionName = name;
            type = _type;
            index = _index;
            objToAdd = vec;
            MoveAction = Act;
        }

        public UndoAction(string name, string _type, int _index, object rail, Action<string,int, object> action)
        {
            actionName = name;
            type = _type;
            index = _index;
            objToAdd = rail;
            ObjAddAction = action;
        }

        public UndoAction(string name, string _type, int _index, string label, object prop, Action<string, int, string, object> action)
        {
            actionName = name;
            type = _type;
            index = _index;
            objToAdd = prop;
            propName = label;
            PropAddAction = action;
        }
    }

    public class ClipBoardItem
    {
        public enum ClipboardType
        {
            NotSet = 0,
            Position = 1,
            Rotation = 2,
            Scale = 3,
            IntArray = 4,
            FullObject = 5,
            Rail = 6
        }

        public Single X = 0;
        public Single Y = 0;
        public Single Z = 0;
        public int[] Args = null;
        public ClipboardType Type = 0;
        public Rail Rail = null;
        public LevelObj Obj = null;

        public override string ToString()
        {
            switch (Type)
            {
                case ClipboardType.Position:
                    return String.Format("Position - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.Rotation:
                    return String.Format("Rotation - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.Scale:
                    return String.Format("Scale - X:{0} Y:{1} Z:{2}", X.ToString(), Y.ToString(), Z.ToString());
                case ClipboardType.IntArray:
                    return "Args[]";
                case ClipboardType.Rail:
                    return "Rail - " + Rail.Name;
                case ClipboardType.FullObject:
                    return "Object - " + Obj.ToString();
                default:
                    return "Not set";
            }
        }
    }

    class AllInfoSection
    {
        public bool IsHidden = false;
        public List<LevelObj> Objs = new List<LevelObj>();
    }

    class DictionaryPropertyGridAdapter : ICustomTypeDescriptor
    {
        IDictionary _dictionary;

        public DictionaryPropertyGridAdapter(IDictionary d)
        {
            _dictionary = d;
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection System.ComponentModel.ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return _dictionary;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        PropertyDescriptorCollection
            System.ComponentModel.ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            ArrayList properties = new ArrayList();
            foreach (DictionaryEntry e in _dictionary)
            {
                properties.Add(new DictionaryPropertyDescriptor(_dictionary, e.Key));
            }

            PropertyDescriptor[] props =
                (PropertyDescriptor[])properties.ToArray(typeof(PropertyDescriptor));

            return new PropertyDescriptorCollection(props);
        }
    }

    class DictionaryPropertyDescriptor : PropertyDescriptor
    {
        IDictionary _dictionary;
        object _key;

        internal DictionaryPropertyDescriptor(IDictionary d, object key)
            : base(key.ToString(), null)
        {
            _dictionary = d;
            _key = key;
        }

        public override Type PropertyType
        {
            get { return _dictionary[_key].GetType(); }
        }

        public override void SetValue(object component, object value)
        {
            _dictionary[_key] = value;
        }

        public override object GetValue(object component)
        {
            return _dictionary[_key];
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void ResetValue(object component)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
    #endregion
}
