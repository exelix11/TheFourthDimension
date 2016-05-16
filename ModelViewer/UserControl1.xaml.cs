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
    /// <summary>
    /// Logica di interazione per UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {
        Dictionary<string, Model3D> ImportedModels = new Dictionary<string, Model3D>();
        Dictionary<string, List<ModelVisual3D>> Models = new Dictionary<string, List<ModelVisual3D>>();
        public Dictionary<string, List<Vector3D>> Positions = new Dictionary<string, List<Vector3D>>();
        ModelImporter Importer = new ModelImporter();
        
        public UserControl1()
        {
            InitializeComponent();
        }

        public void AddKey(string Type)
        {
            if (!Models.ContainsKey(Type)) Models.Add(Type, new List<ModelVisual3D>());
            if (!Positions.ContainsKey(Type)) Positions.Add(Type, new List<Vector3D>());
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
            Models[Type].RemoveAt(index);
            Positions[Type].RemoveAt(index);
            ModelViewer.UpdateLayout();
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
            Vector3D pos = Positions[Type][index];
            ModelViewer.Camera.LookAt(new Point3D(pos.X, pos.Y, pos.Z), 1000);
        }

        public Vector3D Drag(object[] DragArgs, System.Windows.Input.MouseEventArgs e, bool round100)
        {
            Point p = e.GetPosition(ModelViewer);
            Vector3D v = (Vector3D)DragArgs[2];
            Point3D? pos = ModelViewer.Viewport.UnProject(p, new Point3D(v.X,v.Y,v.Z), ModelViewer.Camera.LookDirection);
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
            Point p = e.GetPosition(ModelViewer);
            object[] res = new object[3] { null, null, null };
            ModelVisual3D result = GetHitResult(p);
            if (result == null) return res;
            foreach (string k in Models.Keys)
            {
                if (Models[k].Contains(result))
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
            HitTestResult result = VisualTreeHelper.HitTest(ModelViewer, location);
            if (result != null && result.VisualHit is ModelVisual3D)
            {
                ModelVisual3D visual = (ModelVisual3D)result.VisualHit;
                return visual;
            }

            return null;
        }
    }
}
