using CommonFiles;
using MarioKart.MK7;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
                    MakeKCLandPA(FileName,false);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                else if (args[1].ToLower() == "zero")
                {
                    MakeKCLandPA(FileName, true);
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
                        Console.ReadLine();
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
                Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void MakeKCLandPA(string input, bool zero)
        {
            try
            {
                Console.WriteLine("Creating KCL...");
                KCL k = new KCL();
                List<String> Materials = k.CreateFromFile(File.ReadAllBytes(input));
                Console.WriteLine("Writing KCL...");
                File.WriteAllBytes(input + ".kcl", k.Write());
                Console.WriteLine("Creating PA...");
                Pa_format pa = new Pa_format(true);
                for (int i = 0; i < Materials.Count; i++)
                {
                    if (zero) pa.entries.Add(0);
                    else
                    {
                        Console.WriteLine("-Data for material :" + Materials[i]);
                        Console.Write("|Enter value for Sound_code [0]: ");
                        string tmp = Console.ReadLine();
                        uint SoundCode;
                        if (tmp.Trim() == "") SoundCode = 0; else SoundCode = uint.Parse(tmp);
                        Console.Write("|Enter value for Floor_code [0]: ");
                        tmp = Console.ReadLine();
                        uint FloorCode;
                        if (tmp.Trim() == "") FloorCode = 0; else FloorCode = uint.Parse(tmp);
                        Console.Write("|Enter value for Wall_code [0]: ");
                        tmp = Console.ReadLine();
                        uint WallCode;
                        if (tmp.Trim() == "") WallCode = 0; else WallCode = uint.Parse(tmp);
                        uint Unknown = 0;
                        uint CameraThrought = 0;
                        SoundCode = SoundCode << pa.Fields[0].Shift;
                        FloorCode = FloorCode << pa.Fields[1].Shift;
                        Unknown = Unknown << pa.Fields[2].Shift;
                        WallCode = WallCode << pa.Fields[3].Shift;
                        CameraThrought = CameraThrought << pa.Fields[4].Shift;

                        SoundCode &= pa.Fields[0].Bitmask;
                        FloorCode &= pa.Fields[1].Bitmask;
                        Unknown &= pa.Fields[2].Bitmask;
                        WallCode &= pa.Fields[3].Bitmask;
                        CameraThrought &= pa.Fields[4].Bitmask;

                        pa.entries.Add(SoundCode + FloorCode + Unknown + WallCode + CameraThrought);
                        Console.WriteLine("");
                    }
                }
                Console.WriteLine("Writing PA...");
                File.WriteAllBytes(input + ".pa", pa.MakeFile());
                Console.WriteLine("DONE !");
                Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error: ");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("");
                Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        static void WriteUsage()
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Usage:");
            Console.WriteLine("CollisionsMng *File name* [-zero]: ");
            Console.WriteLine("             Converts an obj to Kcl and Pa add -zero parametrer to set every flag to 0");
            Console.WriteLine("CollisionsMng *File name* ToObj :");
            Console.WriteLine("             Converts a kcl to obj");
            Console.WriteLine("CollisionsMng *File name* ViewData :");
            Console.WriteLine("             Displays materials flags from a Pa file");
            Console.WriteLine("Parametrers are not case sensitive");
            Console.WriteLine("------------------------------------------------");
            Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
