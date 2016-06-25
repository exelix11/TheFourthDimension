using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModelViewer
{
    public partial class UserControl1 : UserControl
    {
        Dictionary<string, Model3D> ImportedModels = new Dictionary<string, Model3D>();
        Dictionary<string, List<ModelVisual3D>> Models = new Dictionary<string, List<ModelVisual3D>>();
        public Dictionary<string, List<Vector3D>> Positions = new Dictionary<string, List<Vector3D>>();
        ModelImporter Importer = new ModelImporter();
        SortingVisual3D ModelViewer = new SortingVisual3D();

        public UserControl1()
        {
            InitializeComponent();
            ModelViewer.SortingFrequency = 0.5;
            ModelView.Children.Add(ModelViewer);
            AddKey("TmpChildrenObjs");
            AddKey("SelectedRail");
        }

        public void Clean()
        {
            ImportedModels = null;
            Models = null;
            Positions = null;
            Importer = null;
            ModelView = null;
        }

        public void SetSortFrequency(double t)
        {
            ModelViewer.SortingFrequency = t;
        }

        public void AddKey(string Type)
        {
            if (!Models.ContainsKey(Type)) Models.Add(Type, new List<ModelVisual3D>());
            if (!Positions.ContainsKey(Type)) Positions.Add(Type, new List<Vector3D>());
        }

        public void AddTmpObjects(List<Vector3D> positions, List<Vector3D> scale, List<Vector3D> rot, List<string> Paths)
        {
            string type = "TmpChildrenObjs";
            if (Models[type].Count != 0) CleanTmpObjects();
            for (int i = 0; i < positions.Count; i++) addModel(Paths[i], type, positions[i], scale[i], (float)rot[i].X, (float)rot[i].Y, (float)rot[i].Z);
        }

        public void CleanTmpObjects()
        {
            string type = "TmpChildrenObjs";
            while (Models[type].Count != 0) RemoveModel(type, 0);
            ModelView.UpdateLayout();
        }

        public void addRail(Point3D[] Points, int Thickness = 5, int at = -1)
        {
            string Type = "AllRailInfos";
            LinesVisual3D l = new LinesVisual3D();
            if (at == -1) Models[Type].Add(l); else Models[Type].Insert(at, l);
            if (at == -1) Positions[Type].Add(Points[0].ToVector3D()); else Positions[Type].Insert(at, Points[0].ToVector3D());
            if (at == -1) ModelViewer.Children.Add(Models[Type][Models[Type].Count - 1]); else ModelViewer.Children.Insert(at, Models[Type][at]);
            if (Points.Length < 2) return;
            l.Color = Color.FromRgb(255, 0, 0);
            l.Thickness = Thickness;
            AddRailpoints(l, Points, Thickness);
        }

        public void SelectRail(Point3D[] Points)
        {
            UnselectRail(); 
            foreach (Point3D p in Points)
            {
                addModel(@"models\UnkRed.obj", "SelectedRail", p.ToVector3D(), new Vector3D(.5f, .5f, .5f), 0, 0, 0);
            }
        }

        public void UnselectRail()
        {
            string type = "SelectedRail";
            while (Models[type].Count != 0) RemoveModel(type, 0);
            ModelView.UpdateLayout();
        }

        public void AddRailpoints(LinesVisual3D l, Point3D[] Points, int Thickness)
        {
            Point3D oldPoint = Points[1];
            l.Points.Add(Points[0]);
            l.Points.Add(Points[1]);
            for (int i = 2; i < Points.Length; i++)
            {
                int chidIndex = l.Children.Count;
                l.Children.Add(new LinesVisual3D());
                ((LinesVisual3D)l.Children[chidIndex]).Color = Color.FromRgb(255, 255, 255);
                ((LinesVisual3D)l.Children[chidIndex]).Thickness = Thickness;
                ((LinesVisual3D)l.Children[chidIndex]).Points.Add(oldPoint);
                ((LinesVisual3D)l.Children[chidIndex]).Points.Add(Points[i]);
                oldPoint = Points[i];
            }
        }

        public void UpdateRailpos(int id, Point3D[] Points)
        {
            RemoveRailPoints(((LinesVisual3D)Models["AllRailInfos"][id]));
            if (Points.Length < 2) return;
            AddRailpoints((LinesVisual3D)Models["AllRailInfos"][id], Points, 5);
            Positions["AllRailInfos"][id] = Points[0].ToVector3D();
            ModelView.UpdateLayout();
        }

        public void addModel(string path, string Type, Vector3D pos, Vector3D scale, Single RotX, Single RotY, Single RotZ, int at = -1)
        {           
            if (at == -1) Models[Type].Add(new ModelVisual3D()); else Models[Type].Insert(at,new ModelVisual3D());
            if (at == -1) ModelViewer.Children.Add(Models[Type][Models[Type].Count-1]); else ModelViewer.Children.Insert(at,Models[Type][at]);
            Model3D Model;
            if (!ImportedModels.ContainsKey(path))
            {
                Model = Importer.Load(path);
                ImportedModels.Add(path, Model);
            }
            else Model = ImportedModels[path];
            Model.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 90));
            Models[Type][at == -1 ? Models[Type].Count - 1: at].Content = Model;
            Transform3DGroup t = new Transform3DGroup();
            t.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), RotX)));
            t.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), RotY)));
            t.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), RotZ)));
            t.Children.Add(new ScaleTransform3D(scale));
            t.Children.Add(new TranslateTransform3D(pos));
            if (at == -1) Positions[Type].Add(pos); else Positions[Type].Insert(at, pos);
            Models[Type][at == -1 ? Models[Type].Count - 1 : at].Transform = t;
        }

        public void RemoveModel(string Type, int index)
        {
            Models[Type][index].Content = null;
            if (Type == "AllRailInfos")
            {
                RemoveRailPoints(((LinesVisual3D)Models[Type][index]));
            }
            Models[Type].RemoveAt(index);
            Positions[Type].RemoveAt(index);
            ModelView.UpdateLayout();
        }

        public void RemoveRailPoints(LinesVisual3D rail)
        {
            foreach (LinesVisual3D r in rail.Children) RemoveRailPoints(r);
            rail.Points.Clear();
        }

        public void HideGroup(string Type)
        {
            for (int i = 0; i < Positions[Type].Count; i++)
            {
                ChangeTransform(Type, i, Positions[Type][i], new Vector3D(0, 0, 0), 0, 0, 0);
            }
        }

        public void CameraToObj(string Type, int index)
        {
            if (Positions[Type].Count <= index) return;
            Vector3D pos = Positions[Type][index];
            ModelView.Camera.LookAt(new Point3D(pos.X, pos.Y, pos.Z), 1000);
        }

        public void SetCameraDirection(int x, int y, int z)
        {
            ModelView.Camera.UpDirection = new Vector3D(x, y, z);
        }

        public Vector3D Drag(object[] DragArgs, System.Windows.Input.MouseEventArgs e, bool round100)
        {
            Point p = e.GetPosition(ModelView);
            Vector3D v = (Vector3D)DragArgs[2];
            Point3D? pos = ModelView.Viewport.UnProject(p, new Point3D(v.X,v.Y,v.Z), ModelView.Camera.LookDirection);
            if (pos.HasValue)
            {
                Vector3D vec = pos.Value.ToVector3D();
                if (round100)
                {
                    vec.X = Math.Round(vec.X / 100d, 0) * 100;
                    vec.Y = Math.Round(vec.Y / 100d, 0) * 100;
                    vec.Z = Math.Round(vec.Z / 100d, 0) * 100;
                    return vec;
                }
                else
                {
                    vec.X = Math.Round(vec.X, 3, MidpointRounding.AwayFromZero);
                    vec.Y = Math.Round(vec.Y, 3, MidpointRounding.AwayFromZero);
                    vec.Z = Math.Round(vec.Z, 3, MidpointRounding.AwayFromZero);
                    return vec;
                }
            }
            return pos.Value.ToVector3D();
        }

        public void ChangeTransform(string Type, int index, Vector3D pos, Vector3D scale, Single RotX, Single RotY, Single RotZ)
        {
            Transform3DGroup t = new Transform3DGroup();
            t.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), RotX)));
            t.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), RotY)));
            t.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), RotZ)));
            t.Children.Add(new ScaleTransform3D(scale));
            t.Children.Add(new TranslateTransform3D(pos));
            Positions[Type][index] = pos;
            Models[Type][index].Transform = t;
        }        

        public object[] GetOBJ(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(ModelView);
            object[] res = new object[3] { null, null, null };
            ModelVisual3D result = GetHitResult(p);
            if (result == null) return res;
            foreach (string k in Models.Keys)
            {
                if (k != "TmpChildrenObjs" && Models[k].Contains(result))
                {
                    res[0] = k;
                    res[1] = Models[k].IndexOf(result);
                    res[2] = Positions[k][(int)res[1]];
                    return res;
                }
            }
            return new object[3] { null, null, null };
        }

        ModelVisual3D GetHitResult(Point location)
        {
            HitTestResult result = VisualTreeHelper.HitTest(ModelView, location);
            if (result != null && result.VisualHit is ModelVisual3D)
            {
                ModelVisual3D visual = (ModelVisual3D)result.VisualHit;
                return visual;
            }

            return null;
        }
    }
}
