using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace The4Dimension.FormEditors
{
    public partial class FrmXmlEditor : Form
    {
        public String XmlRes = null;
        bool IsConverter = false;
        Style IntStyle = new TextStyle(Brushes.Blue, null, FontStyle.Bold);
        Style FloatStyle = new TextStyle(Brushes.Green, null, FontStyle.Bold);
        Style StringStyle = new TextStyle(Brushes.Red, null, FontStyle.Bold);
        Style PropNameWordStyle = new TextStyle(Brushes.DarkOrange, null, FontStyle.Bold);
        Style KeyWordStyle = new TextStyle(Brushes.DarkBlue, null, FontStyle.Bold);
        Style C1Style = new TextStyle(Brushes.Silver, null, FontStyle.Bold);
        int Pos = 0;

        public FrmXmlEditor(string Xml, string Name, bool converter, int _pos = 0)
        {
            InitializeComponent();
            fastColoredTextBox1.Text = Xml;
            this.Text = "Xml editor: " + Name;
            IsConverter = converter;
            Pos = _pos;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (IsConverter)
            {
                SaveFileDialog s = new SaveFileDialog();
                s.Filter = ".xml file|*.xml|.byml file|*.byml";
                if (s.ShowDialog() == DialogResult.OK)
                {
                    if (Path.GetFileName(s.FileName).EndsWith(".xml")) File.WriteAllText(s.FileName, fastColoredTextBox1.Text, Encoding.GetEncoding(932));
                    else File.WriteAllBytes(s.FileName, BymlConverter.GetByml(fastColoredTextBox1.Text));
                }
            }
            else XmlRes = fastColoredTextBox1.Text;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            XmlRes = null;
            this.Close();
        }

        private void FCTB_TextChangeDelayed(object sender, TextChangedEventArgs e)
        {
            fastColoredTextBox1.Range.ClearStyle(IntStyle, FloatStyle, StringStyle, PropNameWordStyle, KeyWordStyle, C1Style);
            fastColoredTextBox1.Range.SetStyle(IntStyle, @"\b(D1|/D1|d1|/d1)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            fastColoredTextBox1.Range.SetStyle(FloatStyle, @"\b(D2|/D2|d2|/d2)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            fastColoredTextBox1.Range.SetStyle(StringStyle, @"\b(A0|/A0|a0|/a0)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            fastColoredTextBox1.Range.SetStyle(PropNameWordStyle, @"\b(Name|StringValue|version|encoding|Value)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            fastColoredTextBox1.Range.SetStyle(KeyWordStyle, @"\b(BymlFormatVersion|isBigEndian|Root|/Root|shift_jis|xml|True|False|D0|d0)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            fastColoredTextBox1.Range.SetStyle(C1Style, @"\b(C0|C1|/C0|/C1|c1|c0|/c1|/c0)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private void FCTB_ToolTipNeeded(object sender, ToolTipNeededEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.HoveredWord))
            {
                var range = new Range(sender as FastColoredTextBox, e.Place, e.Place);
                string hoveredWord = range.GetFragment("[^\n]").Text;
                Regex pattern = new Regex("[<>/=]");
                string input = pattern.Replace(hoveredWord.Trim().ToLower(), "");
                System.Diagnostics.Debug.Print(input);
                string toolTipText = "";
                string toolTipTitle = "";
                if (input.StartsWith("c1"))
                {
                    toolTipTitle = "C1 type:";
                    toolTipText = "This is a dictionary node, contains other nodes";
                }
                else if (input.StartsWith("c0"))
                {
                    toolTipTitle = "C0 type:";
                    toolTipText = "This is an array node, contains other nodes, without the name property";
                }
                else if(input.StartsWith("a0"))
                {
                    toolTipTitle = "A0 type:";
                    toolTipText = "This is a string value, the text in the \"StringValue\" section will be used as normal text";
                }
                else if(input.StartsWith("d1"))
                {
                    toolTipTitle = "D1 type:";
                    toolTipText = "This is a number,the text in the \"StringValue\" section will be used as an integer number";
                }
                else if (input.StartsWith("d2"))
                {
                    toolTipTitle = "D2 type:";
                    toolTipText = "This is a number,the text in the \"StringValue\" section will be used as a decimal number";
                }
                else if (input.StartsWith("d0"))
                {
                    toolTipTitle = "D0 type:";
                    toolTipText = "This is a boolean value,the text in the \"StringValue\" section can be only True or False";
                }
                else if(input.StartsWith("name"))
                {
                    toolTipTitle = "Name property";
                    toolTipText = "This is the name of this value";
                }
                else if(input.StartsWith("stringvalue"))
                {
                    toolTipTitle = "StringValue property";
                    toolTipText = "This is the value of this node as a string, when changing this respect the node type,to get info about types place the mouse on the type name (D1, A0 etc..)";
                }
                else return;
                e.ToolTipTitle = toolTipTitle;
                e.ToolTipText = toolTipText;
            }
        }

        private void FrmXmlEditor_Load(object sender, EventArgs e)
        {
            if (Pos != 0)
            {
                fastColoredTextBox1.SelectionStart = Pos + 1;
                fastColoredTextBox1.BookmarkColor = Color.Red;
                fastColoredTextBox1.BookmarkLine(fastColoredTextBox1.Selection.Start.iLine);
                fastColoredTextBox1.Navigate(fastColoredTextBox1.Selection.Start.iLine);
                fastColoredTextBox1.DoCaretVisible();
            }
        }
    }
}
