using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Media.Media3D;

namespace The4Dimension
{
    public class LevelObj : ICloneable
    {
        public Dictionary<string, object> Prop = new Dictionary<string, object>();
        public override string ToString()
        {
            if (Prop == null)
            {
                Prop = new Dictionary<string, object>();
                return "Unknown name LevelObj";
            }
            if (Prop.ContainsKey("name"))
                return Prop["name"].ToString().Substring("String : ".Length);
            else
            {
                if (Prop.ContainsKey("l_id")) return "LevelObj id: " + Prop["l_id"].ToString();
                else return "LevelObj";
            }
        }

        public LevelObj Clone()
        {
            LevelObj o = new LevelObj();
            foreach (string k in Prop.Keys.ToArray())
            {
                if (Prop[k] is ICloneable) o.Prop.Add(k, ((ICloneable)Prop[k]).Clone()); else throw new Exception("Type non cloneable");
            }
            return o;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        [Editor(typeof(LevelObjEditor), typeof(UITypeEditor))]
        [Description("This contains every property of this object")]
        public Dictionary<string, object> ObjectData
        {
            get { return Prop; }
            set { Prop = value; }
        }
    }

    class LevelObjEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            Dictionary<string, object> v = value as Dictionary<string, object>;
            if (svc != null && v != null)
            {
                using (FrmObjEditor form = new FrmObjEditor(v))
                {
                    form.ShowDialog();
                    v = form.Value.Prop;
                }
            }
            return v; // can also replace the wrapper object here
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    class C0List : ICloneable
    {
        List<LevelObj> l = new List<LevelObj>();
        public List<LevelObj> List
        {
            get { return l; }
            set { l = value; }
        }

        public C0List Clone()
        {
            C0List C = new C0List();
            foreach (LevelObj lev in l) C.l.Add(lev.Clone());
            return C;
        }

        public override string ToString()
        {
            return "C0 list";
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Rail : ICloneable
    {
        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
            set { this.GetType().GetProperty(propertyName).SetValue(this, value, null); }
        }

        public Point3D[] GetPointArray()
        {
            List<Point3D> points = new List<Point3D>();
            foreach (Rail.Point p in Points) points.Add(new Point3D(p.X, -p.Z, p.Y));
            return points.ToArray();
        }

        List<int> _Args = new List<int>();
        string _LayerName;
        internal List<Point> _points = new List<Point>();
        //Multi file name ?
        public string _closed;
        int _l_id;
        string _name;
        int _no;
        string _type;

        public Rail(bool Adding = false)
        {
            _LayerName = "共通";
            _closed = "FALSE";
            _name = "empty rail";
            _type = "Bezier";
            if (Adding) _points.Add(new Point());
            if (Adding) _points.Add(new Point(1));
        }

        public override string ToString()
        {
            return _name;
        }

        public string Name
        {
            set
            {
                _name = JapChars(value);
            }
            get { return _name; }
        }

        public string Type
        {
            set { _type = value; }
            get { return _type; }
        }

        public bool Closed
        {
            set { _closed = value == false ? "OPEN" : "CLOSE"; }
            get { return _closed == "OPEN" ? false : true; }
        }

        public int no
        {
            set { _no = value; }
            get { return _no; }
        }

        public int l_id
        {
            set { _l_id = value; }
            get { return _l_id; }
        }        

        public List<int> Arg
        {
            set { _Args = value; }
            get { return _Args; }
        }

        internal string JapChars(string input)
        {
            byte[] bytes = Encoding.Default.GetBytes(input);
            Encoding Enc = Encoding.GetEncoding(932);
            return input;//Enc.GetString(bytes);
        }

        public Rail Clone()
        {
            
            Rail R = new Rail();
            foreach (int i in _Args) R._Args.Add(i);
            R.LayerName = (string)_LayerName.Clone();
            foreach (Point p in _points) R._points.Add(p.Clone());
            R._closed = (string)_closed.Clone();
            R.l_id = _l_id;
            R.Name = (string)_name.Clone();
            R.no = _no;
            R.Type = (string)_type.Clone();
            return R;
         }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public string LayerName
        {
            set {
                _LayerName = JapChars(value);
            }
            get { return _LayerName; }
        }

        [Editor(typeof(RailPointEditor), typeof(UITypeEditor))]
        public List<Point> Points
        {
            set { _points = value; }
            get { return _points; }
        }

        // [TypeConverter(typeof(ExpandableObjectConverter))]
        public class Point : ICloneable
        {
            List<int> _Args = new List<int>();
            int _ID;
            public List<Single> _X = new List<Single>();
            public List<Single> _Y = new List<Single>();
            public List<Single> _Z = new List<Single>();

            public Point(int id = 0)
            {
                _ID = id;
            }

            public override string ToString()
            {
                return "Point ID: " + _ID.ToString();
            }

            public Point Clone()
            {
                Point N = new Point();
                foreach (int i in _Args) N._Args.Add(i);
                N.ID = _ID;
                foreach (int s in _X) N._X.Add(s);
                foreach (int s in _Y) N._Y.Add(s);
                foreach (int s in _Z) N._Z.Add(s);
                return N;
            }

            object ICloneable.Clone()
            {
                return Clone();
            }

            public List<int> Args
            {
                set { _Args = value; }
                get { return _Args; }
            }

            public int ID
            {
                set { _ID = value; }
                get { return _ID; }
            }

            public Single X
            {
                set { _X.Clear(); _X.Add(value); _X.Add(value); _X.Add(value); }
                get {
                    if (_X.Count == 0) X = 0;
                    return _X[0];
                }
            }

            public Single Y
            {
                set { _Y.Clear(); _Y.Add(value); _Y.Add(value); _Y.Add(value); }
                get {
                    if (_Y.Count == 0) Y = 0;
                    return _Y[0];
                }
            }

            public Single Z
            {
                set { _Z.Clear(); _Z.Add(value); _Z.Add(value); _Z.Add(value); }
                get {
                    if (_Z.Count == 0) Z = 0;
                    return _Z[0];
                }
            }
        }
    }

    class RailPointEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService svc = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            List<Rail.Point> v = value as List<Rail.Point>;
            if (svc != null && v != null)
            {
                using (FrmRailPointEditor form = new FrmRailPointEditor(v))
                {
                    form.ShowDialog();
                    v = form.Value;
                }
            }
            return v; // can also replace the wrapper object here
        }
    }


    [TypeConverter(typeof(ExpandableObjectConverter))]
    class Node : ICloneable
    {
        string _StringValue;
        public string _StringNodeType; 
        NodeTypes _NodeType;        

        [Description("This is the value of this node as a string, change it respecting the type")]
        public string StringValue
        {
            set {
                if (_NodeType == NodeTypes.Int) Int32.Parse(value); //Crashes if the value is invalid
                else if (_NodeType == NodeTypes.Single) Single.Parse(value);
                _StringValue = value;
            }
            get { return _StringValue; }
        }
        [Description("The node type can't be changed, it tells what kind of data contains the node")]
        public NodeTypes NodeType
        {
            //set { _NodeType = value; }
            get { return _NodeType; }
        }

        public override string ToString()
        {
            string Prev = "";
            if (_NodeType == NodeTypes.Empty) Prev += "Empty";
            else if (_NodeType == NodeTypes.String) Prev += "String";
            else if (_NodeType == NodeTypes.Int) Prev += "Int";
            else if (_NodeType == NodeTypes.Single) Prev += "Single";
            else Prev += string.Format("Unk Type ({0})", _StringNodeType);
            Prev += " : ";
            Prev += _StringValue;
            return Prev;
        }

        public enum NodeTypes
        {
            String = 0xA0,
            Empty = 0xA1,
            Int = 0xD1,
            Single = 0xD2,         
            Other   
        }

        string JapChars(string input)
        {
            byte[] bytes = Encoding.Default.GetBytes(input);
            Encoding Enc = Encoding.GetEncoding(932);
            return input; //Enc.GetString(bytes);
        }

        public Node(string _stringValue, string _type)
        {
            _NodeType = NodeTypes.Other;
            if (_type == "A0") _NodeType = NodeTypes.String;
            else if (_type == "A1") _NodeType = NodeTypes.Empty;
            else if (_type == "D1") _NodeType = NodeTypes.Int;
            else if (_type == "D2") _NodeType = NodeTypes.Single;
            _StringNodeType = _type;
            ApplyValue(_stringValue, _NodeType);
        }

        void ApplyValue(string _stringValue, NodeTypes _type)
        {
            _StringValue = _stringValue;
            switch (_type)
            {
                case NodeTypes.String:
                    _StringValue = JapChars(_StringValue);
                    _NodeType = NodeTypes.String;
                    break;
                case NodeTypes.Empty:
                    _NodeType = NodeTypes.Empty;
                    break;
                default:
                    _NodeType = _type;
                    break;
            }
        }

        public Node Clone()
        {
            return new Node(this._StringValue, this._StringNodeType);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
    /*String = 0xA0,
      Empty = 0xA1,
      Int = 0xD1,
      Single = 0xD2,
      */

}
