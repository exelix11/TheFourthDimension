using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace The4Dimension.FormEditors
{
    public partial class FrmAddCameraSettings : Form
    {
        string XmlFile;
        int CameraId;
        int TextInsertIndex = -1;
        Form1 owner;
        public FrmAddCameraSettings(string xml, int camId, Form1 own)
        {
            InitializeComponent();
            XmlFile = xml;
            CameraId = camId;
            label1.Text = "CameraId: " + camId.ToString();
            owner = own;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FrmAddCameraSettings_Load(object sender, EventArgs e)
        {
            TextInsertIndex = XmlFile.IndexOf("<C0 Name=\"CameraParams\">");
            if (TextInsertIndex == -1)
            {
                MessageBox.Show("Failed to get CameraParams node position !");
                this.Close();
            }
            TextInsertIndex += "<C0 Name=\"CameraParams\">".Length;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string str = "\r\n";
            str += "<C1>\r\n";
            str += "<D2 Name=\"AngleH\" StringValue=\"" + numericUpDown3.Value.ToString() + "\" />\r\n";
            str += "<D2 Name=\"AngleV\" StringValue=\"" + numericUpDown2.Value.ToString() + "\" />\r\n";
            str += "<A0 Name=\"Category\" StringValue=\"Map\" />\r\n<A0 Name=\"Class\" StringValue=\"Parallel\" />\r\n";
            str += "<D2 Name=\"Distance\" StringValue=\"" + numericUpDown4.Value.ToString() + "\" />\r\n";
            str += "<D1 Name=\"UserGroupId\" StringValue=\"" + CameraId.ToString() + "\" />\r\n";
            str += "<A0 Name=\"UserName\" StringValue=\"CameraArea\" />\r\n</C1>\r\n";
            XmlFile = XmlFile.Insert(TextInsertIndex,str);
            owner.SzsFiles["CameraParam.byml"] = BymlConverter.GetByml(XmlFile);
            this.Close();
        }
    }
}