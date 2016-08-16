using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.IO;
using System.Xml;

namespace The4Dimension
{

    class CustomStringWriter : System.IO.StringWriter
    {
        private readonly Encoding encoding;

        public CustomStringWriter(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }

    public class CustomStack<T>
    {
        private List<T> items = new List<T>();
        public int MaxItems = 50;

        public int Count
        { get { return items.Count(); } }

        public void Remove(int index)
        {
            items.RemoveAt(index);
        }

        public void Push(T item)
        {
            items.Add(item);
            if (items.Count > MaxItems)
            {
                for (int i = MaxItems; i < items.Count; i++) Remove(0);
            }
        }

        public T Pop()
        {
            if (items.Count > 0)
            {
                T tmp = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return tmp;
            }
            else return default(T);
        }

        public T[] ToArray()
        {
            return items.ToArray();
        }
    }

    public class UndoAction
    {
        public string actionName;
        public string type;
        public int index;
        public int[] indexes;
        public Action<int, string> Action = null;
        private Action<string, int, object> ObjAddAction = null;
        private Action<string, int[], object> ObjMultiAddAction = null;
        object objToAdd = null;
        string propName = null;
        private Action<string, int, string, object> PropAddAction = null;
        private Action<string, int, Vector3D> MoveAction = null;
        private Action<string, int, int, Vector3D> UndoChildrenMoveAction = null;

        public void Undo()
        {
            Form1 form1 = (Form1)Application.OpenForms[0]; //There is always one instance of this form
            form1.comboBox1.Text = type;
            if (form1.ObjectsListBox.SelectedIndex == index && PropAddAction == null) form1.ObjectsListBox.SelectedIndex = -1;
            if (Action != null) Action.Invoke(index, type);
            else if (ObjAddAction != null) ObjAddAction.Invoke(type, index, objToAdd);
            else if (PropAddAction != null) PropAddAction.Invoke(type, index, propName, objToAdd);
            else if (ObjMultiAddAction != null) ObjMultiAddAction.Invoke(type, indexes, objToAdd);
            else if (UndoChildrenMoveAction != null) UndoChildrenMoveAction.Invoke(type, indexes[0], indexes[1], (Vector3D)objToAdd);
            else MoveAction.Invoke(type, index, (Vector3D)objToAdd);
            if (form1.ObjectsListBox.Items.Count > index) form1.ObjectsListBox.SelectedIndex = index;
        }

        public override string ToString()
        {
            return actionName;
        }

        public UndoAction(string name, string _type, int _index, Action<int, string> Act)
        {
            actionName = name;
            type = _type;
            index = _index;
            Action = Act;
        }

        public UndoAction(string name, string _type, int _index, Vector3D vec, Action<string, int, Vector3D> Act)
        {
            actionName = name;
            type = _type;
            index = _index;
            objToAdd = vec;
            MoveAction = Act;
        }

        public UndoAction(string name, string _type, int _index, object rail, Action<string, int, object> action)
        {
            actionName = name;
            type = _type;
            index = _index;
            objToAdd = rail;
            ObjAddAction = action;
        }

        public UndoAction(string name, string _type, int[] _index, object[] rail, Action<string, int[], object> action)
        {
            actionName = name;
            type = _type;
            indexes = _index;
            objToAdd = rail;
            ObjMultiAddAction = action;
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

        public UndoAction(string name, string _type, int[] _index, Vector3D vec, Action<string, int, int, Vector3D> action) //Childrenobjectmove
        {
            actionName = name;
            type = _type;
            indexes = _index;
            objToAdd = vec;
            UndoChildrenMoveAction = action;
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
            Rail = 6,
            ObjectArray = 7
        }

        public Single X = 0;
        public Single Y = 0;
        public Single Z = 0;
        public int[] Args = null;
        public ClipboardType Type = 0;
        public Rail Rail = null;
        public LevelObj[] Objs = null;

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
                    return "Object - " + Objs[0].ToString();
                case ClipboardType.ObjectArray:
                    return "Object[" + Objs.Length.ToString() + "]";
                default:
                    return "Not set";
            }
        }

        public string ToString(int ObjectAsChildren)
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
                    if (ObjectAsChildren < 0) return "Object - " + Objs[0].ToString();
                    else
                        return "Paste object as children - " + Objs[0].ToString();
                case ClipboardType.ObjectArray:
                    return "Object[" + Objs.Length.ToString() + "]";
                default:
                    return "Not set";
            }
        }
    }

    public class AllInfoSection
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

    /*New object database format (whitehole-like), not ready yet
    public class ObjectDb
    {
        public Dictionary<int, string> Categories = new Dictionary<int, string>();
        public Dictionary<string, ObjectDbEntry> Entries = new Dictionary<string, ObjectDbEntry>();
        public int timestamp;

        public static ObjectDb FromXml(string xml)
        {
            ObjectDb res = new ObjectDb();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode n = doc.SelectSingleNode("/database");
            res.timestamp = int.Parse(n.Attributes["timestamp"].InnerText);
            foreach (XmlNode node in n.ChildNodes)
            {
                if (node.Name == "categories")
                {
                    foreach (XmlNode subn in node.ChildNodes)
                    {
                        res.Categories.Add(int.Parse(subn.Attributes["id"].InnerText), subn.InnerText);
                    }
                }
                else if (node.Name == "object")
                {
                    res.Entries.Add(node.Attributes["id"].InnerText, ObjectDbEntry.FromXml(node.ChildNodes));
                }                
            }
            return res;
        }

        public class ObjectDbEntry
        {
            public string name, notes, files;
            public int Known, Complete, Category;
            public List<ObjectDbField> Fields = new List<ObjectDbField>();

            public static ObjectDbEntry FromXml(XmlNodeList nodes)
            {
                ObjectDbEntry res = new ObjectDbEntry();
                foreach (XmlNode n in nodes)
                {
                    switch (n.Name)
                    {
                        case "name":
                            res.name = n.InnerText;
                            break;
                        case "flags":
                            res.Known = int.Parse(n.Attributes["known"].InnerText);
                            res.Complete = int.Parse(n.Attributes["complete"].InnerText);
                            break;
                        case "category":
                            res.Category = int.Parse(n.Attributes["id"].InnerText);
                            break;
                        case "notes":
                            res.notes = n.InnerText;
                            break;
                        case "files":
                            res.files = n.InnerText;
                            break;
                        case "field":
                            res.Fields.Add(ObjectDbField.FromXml(n));
                            break;
                    }
                }
                return res;
            }

            public void Serialize()
            {

            }
        }

        public class ObjectDbField
        {
            public int id;
            public string type = "int";
            public string name, values, notes;

            public void Serialize()
            {

            }

            public static ObjectDbField FromXml(XmlNode n)
            {
                ObjectDbField res = new ObjectDbField();
                res.id = int.Parse(n.Attributes["id"].InnerText);
                res.type = n.Attributes["type"].InnerText;
                res.name = n.Attributes["name"].InnerText;
                res.values = n.Attributes["values"].InnerText;
                res.notes = n.Attributes["notes"].InnerText;
                return res;
            }
        }
    }*/
}

namespace ExtensionMethods
{
    static class Extensions
    {
        public static Vector3D ToVect(this Point3D p)
        {
            return new Vector3D(p.X, p.Y, p.Z);
        }
    }
}
