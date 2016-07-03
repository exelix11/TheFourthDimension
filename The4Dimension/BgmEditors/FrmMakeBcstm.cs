using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;

namespace The4Dimension.BgmEditors
{
    public partial class FrmMakeBcstm : Form
    {
        public FrmMakeBcstm()
        {
            InitializeComponent();
        }

        private void FrmMakeBcstm_Load(object sender, EventArgs e)
        {
            if (Directory.Exists("Temp")) Directory.Delete("Temp", true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Filter = "Ogg files|*.ogg";
            opn.Multiselect = true;
            if (opn.ShowDialog() == DialogResult.OK) listBox1.Items.AddRange(opn.FileNames);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1) listBox1.Items.Remove(listBox1.SelectedItem);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0)
            {
                MessageBox.Show("Add at least a file to the list");
                return;
            }
            if (Directory.Exists("Temp")) Directory.Delete("Temp", true);
            label3.Text = "Status: extracting tools...";
            Refresh();
            if (!File.Exists("dspadpcm23.zip"))
            {
                MessageBox.Show("To convert to bcstm you need dspadpcm from the wii sdk, this file is pretty illegal, that's why it isn't included in the editor,you can download it, but i'm not liable if the Regginator comes to your house to catch you");
                if (MessageBox.Show("Do you want to download it ?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {                    
                    try
                    {
                        WebClient w = new WebClient();
                        w.DownloadFile("http://hcs64.com/files/dspadpcm23.zip", "dspadpcm23.zip");
                        w.Dispose();
                        MessageBox.Show("The file was downloaded as dspadpcm23.zip in this program's directory, don't delete it since it will be needed for every conversion !");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was an error, the file wasn't downloaded: \r\n" + ex.Message);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Without that file you can't use the converter :(");
                    return;
                }
            }
            Directory.CreateDirectory("Temp");
            File.WriteAllBytes(@"Temp\tools.zip", Properties.Resources.BCSTMtools);
            ZipFile.ExtractToDirectory(@"Temp\tools.zip", @"Temp");
            File.Delete(@"Temp\tools.zip");
            ZipFile.ExtractToDirectory(@"dspadpcm23.zip", @"Temp");
            List<string> failed = new List<string>();
            string ConverterPath = "\"" + Path.GetFullPath(@"Temp\") + "\"";
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                label3.Text = "Status: converting file " + (i + 1).ToString() + " of " + listBox1.Items.Count.ToString() + "...";
                Refresh();
                if (File.Exists(@"Temp\snd.ogg")) File.Delete(@"Temp\snd.ogg");
                File.Copy(listBox1.Items[i].ToString(), @"Temp\snd.ogg");
                Process p = new Process();
                ProcessStartInfo s = new ProcessStartInfo("cmd.exe", "/c cd " + ConverterPath +" && converter.bat");
                s.UseShellExecute = false;
                if (checkBox1.Checked) s.CreateNoWindow = true;
                p.StartInfo = s;
                p.Start();
                p.WaitForExit();
                if (File.Exists(@"Temp\snd.ogg.bcstm"))
                {
                    string path = Path.GetDirectoryName(listBox1.Items[i].ToString());
                    File.Copy(@"Temp\snd.ogg.bcstm", path + "\\" + Path.GetFileNameWithoutExtension(listBox1.Items[i].ToString()) + ".bcstm");
                    File.Delete(@"Temp\snd.ogg.bcstm");
                }
                else failed.Add(listBox1.Items[i].ToString());
            }
            if (failed.Count == 0) MessageBox.Show("Done !");
            else MessageBox.Show("some files weren't converted: \r\n" + string.Join("\r\n", failed.ToArray()), "warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Directory.Delete("Temp", true);
            label3.Text = "Status: Done ";
        }
    }
}