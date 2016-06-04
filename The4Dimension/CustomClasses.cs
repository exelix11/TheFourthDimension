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
        public Action<int, string> Action = null;
        private Action<string, int, object> ObjAddAction = null;
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
                    if (ObjectAsChildren < 0) return "Object - " + Obj.ToString();
                    else
                        return "Paste object as children - " + Obj.ToString();
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
}
