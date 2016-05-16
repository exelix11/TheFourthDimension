using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (Args.Length == 1)
            {
                string Cont = System.IO.File.ReadAllText(Args[0]);
                if (Cont.StartsWith("<?xml")) System.IO.File.WriteAllBytes(Args[0] + ".byml", BymlConverter.GetByml(Cont));
                else if (Cont.StartsWith("YB")) System.IO.File.WriteAllText(Args[0] + ".xml", BymlConverter.GetXml(Args[0]), Encoding.GetEncoding(932));
                else Application.Run(new Form1(Args[0].Trim()));
            }        
            else Application.Run(new Form1());
        }
    }
}
