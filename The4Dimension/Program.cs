using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace The4Dimension
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main(string[] Args)
        {
            System.IO.Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Args.Length > 1)
            {
                if (Args[0] == "convert")
                {
                    string Cont = System.IO.File.ReadAllText(Args[1]);
                    if (Cont.StartsWith("<?xml version=\"1.0\"")) System.IO.File.WriteAllBytes(Args[1] + ".byml", BymlConverter.GetByml(Args[1]));
                    else if (Cont.StartsWith("YB")) System.IO.File.WriteAllText(Args[1] + ".xml", BymlConverter.GetXml(Args[1]));
                }
                Application.Exit();
            }
            else if (Args.Length == 1)
            {
                Application.Run(new Form1(Args[0].Trim()));
            }        
            else Application.Run(new Form1());
        }
    }
}
