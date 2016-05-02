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
using LibEveryFileExplorer.Files.SimpleFileSystem;
using _3DS.NintendoWare.GFX;
using CommonFiles;
using The4Dimension.Ohana;

namespace The4Dimension
{
    public partial class ModelDumper : Form
    {
        string ObjDataPath;
        bool UseEFE = true;
        public ModelDumper()
        {
            InitializeComponent();
        }

        private void ModelDumper_Load(object sender, EventArgs e)
        {
             FolderBrowserDialog fld = new FolderBrowserDialog();
            if (fld.ShowDialog() != DialogResult.OK) this.Close();
            ObjDataPath = fld.SelectedPath;
            Directory.CreateDirectory("models");
            File.WriteAllBytes(@"models\baseModels.zip", Properties.Resources.BaseModels);
            ZipFile.ExtractToDirectory(@"models\baseModels.zip", @"models");
            File.Delete(@"models\baseModels.zip");
            Directory.CreateDirectory(@"models\Tex");
            progressBar1.Maximum = Directory.GetFiles(ObjDataPath).Length;
            if (MessageBox.Show("Do you want to convert the models with Ohana3DS ?\r\n" +
                "Converting with Ohana3DS will produce better quality models, the older method was kept only for compatibility, if you have problems with Ohana3DS use Every File Explorers(by clicking No)\r\n", "Model conversion",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    UseEFE = false;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker Sender = (BackgroundWorker)sender;
            int Total, actual;
            var files = Directory.GetFiles(ObjDataPath);
            Total = files.Length;
            for (int i = 0; i < files.Length; i++)
            {
                actual = i;
                Sender.ReportProgress(actual);
                if (files[i].EndsWith(".szs"))
                {
                    CommonCompressors.YAZ0 y = new CommonCompressors.YAZ0();
                    NDS.NitroSystem.FND.NARC f = new NDS.NitroSystem.FND.NARC(y.Decompress(File.ReadAllBytes(files[i])));
                    foreach (SFSFile file in f.ToFileSystem().Files)
                    {
                        if (file.FileName.Contains(".bcmdl"))
                        {
                            try
                            {
                                string Name = Application.StartupPath + @"\models\" + file.FileName.Remove(file.FileName.Length - 6, 6) + ".obj";
                                if (UseEFE)
                                {
                                    ConvertEFE(file.Data, Name);
                                }
                                else
                                {
                                    if (!Path.GetFileNameWithoutExtension(Name).ToLower().StartsWith("demo") && Path.GetFileNameWithoutExtension(Name).ToLower() != "karon")
                                    {
                                        RenderBase.OModelGroup mdl = new Ohana.CGFX().load(new MemoryStream(file.Data));
                                        new Ohana.OBJ().export(mdl, Name, 0);
                                    }
                                    else if (Path.GetFileNameWithoutExtension(Name).ToLower() == "karon") ConvertEFE(file.Data, Name);
                                }
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
        }

        void ConvertEFE(byte[] Data, string Name)
        {
            _3DS.NintendoWare.GFX.CGFX mod = new _3DS.NintendoWare.GFX.CGFX(Data);
            CommonFiles.OBJ o = mod.Data.Models[0].ToOBJ();
            o.MTLPath = Path.GetFileNameWithoutExtension(Name) + ".mtl";
            MTL m = mod.Data.Models[0].ToMTL("Tex");
            byte[] d = o.Write();
            byte[] d2 = m.Write();
            File.Create(Name).Close();
            File.WriteAllBytes(Name, d);
            File.Create(Path.ChangeExtension(Name, "mtl")).Close();
            File.WriteAllBytes(Path.ChangeExtension(Name, "mtl"), d2);
            Directory.CreateDirectory(Path.GetDirectoryName(Name) + "\\Tex");
            foreach (var v in mod.Data.Textures)
            {
                if (!(v is ImageTextureCtr)) continue;
                ((ImageTextureCtr)v).GetBitmap().Save(Path.GetDirectoryName(Name) + "\\Tex\\" + v.Name + ".png");
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label2.Text = string.Format("Converting file {0} of {1}...",e.ProgressPercentage,progressBar1.Maximum);
        }

        private void backgroundWorker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Done !");
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            System.Threading.Thread.Sleep(300);
            if (Directory.Exists(@"models\Tex")) Directory.Delete(@"models\Tex", true);
            Directory.Delete("models", true);
            MessageBox.Show("You must have every model from the game before you can use the editor");
            Application.Exit();
        }
    }
}
