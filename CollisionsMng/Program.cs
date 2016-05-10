using MarioKart.MK7;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollisionsMng
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Title = "CollisionsMng";
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("-----3D Land collision importer by exelix11-----");
            Console.WriteLine("------------------Version 1.0-------------------");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Thanks Gericom for Every File Explorer's KCL importer");
            Console.WriteLine("");
            try
            {
                if (args.Length == 0 || args.Length > 2) { WriteUsage(); Console.ForegroundColor = ConsoleColor.White; return; }
                string FileName = args[0];
                if (args.Length == 1)
                {
                    MakeKCLandPA(FileName);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                else
                {
                    if (args[1].ToLower() == "toobj")
                    {
                        KCL k = new KCL(File.ReadAllBytes(FileName));
                        k.Convert(0, FileName + ".obj");
                        Console.WriteLine("DONE !");
                        Console.ForegroundColor = ConsoleColor.White;
                        return;
                    }
                    else if (args[1].ToLower() == "viewdata")
                    {
                        Console.WriteLine(Pa_format.LoadFile(File.ReadAllBytes(FileName)).ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                        return;
                    }
                    else { WriteUsage(); Console.ForegroundColor = ConsoleColor.White; return; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error: ");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("");
                Console.ReadKey();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void MakeKCLandPA(string input)
        {
            try
            {
                Console.WriteLine("Creating KCL...");
                KCL k = new KCL();
                int matCount = k.CreateFromFile(File.ReadAllBytes(input));
                Console.WriteLine("Writing KCL...");
                File.WriteAllBytes(input + ".kcl", k.Write());
                Console.WriteLine("Creating PA...");
                Pa_format pa = new Pa_format();
                for (int i = 0; i < matCount; i++) pa.entries.Add(0);
                Console.WriteLine("Writing PA...");
                File.WriteAllBytes(input + ".pa", pa.MakeFile());
                Console.WriteLine("DONE !");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error: ");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("");
                Console.ReadKey();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void WriteUsage()
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Usage:");
            Console.WriteLine("CollisionsMng *File name* : ");
            Console.WriteLine("             Converts an obj to Kcl and Pa");
            Console.WriteLine("CollisionsMng *File name* ToObj :");
            Console.WriteLine("             Converts a kcl to obj");
            Console.WriteLine("CollisionsMng *File name* ViewData :");
            Console.WriteLine("             Displays materials flags from a Pa file");
            Console.WriteLine("Parametrers are not case sensitive");
            Console.WriteLine("------------------------------------------------");
            Console.ReadKey();
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
