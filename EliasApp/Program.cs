using EliasApp.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

namespace EliasApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No arguments supplied");

#if DEBUG
            Console.Read();
#endif
                return;
            }

            Dictionary<string, string> commandArguments = new Dictionary<string, string>();

            try
            {
                try
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        string key = args[i];
                        string value = args[i + 1];

                        commandArguments.Add(key, value);
                        i++;
                    }
                }
                catch
                {
                    Console.WriteLine("Invalid argument parameters!");

#if DEBUG
                    Console.Read();
#endif
                    return;
                }

                var ffdecPath = Config.Instance.GetString("ffdec.path");
                var directorPath = Config.Instance.GetString("elias.cct.converter.app");
                var outputPath = Path.Combine(Path.GetDirectoryName(directorPath), "temp");
                var cctPath = Config.Instance.GetString("output.path");

                if (commandArguments.ContainsKey("-directory"))
                {
                    var directory = commandArguments["-directory"];

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Reading directory: ");
                    Console.ResetColor();
                    Console.WriteLine(directory);

                    foreach (var file in Directory.GetFiles(directory, "*.swf"))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("Creating CCT: ");
                        Console.ResetColor();
                        Console.WriteLine(Path.GetFileNameWithoutExtension(file));

                        int X = 1;
                        int Y = 1;

                        var elias = new EliasLibrary.Elias(Path.GetFileNameWithoutExtension(file), file, X, Y, ffdecPath, outputPath, directorPath);
                        SaveFiles(elias.Parse(), outputPath, cctPath);
                    }
                }
                else if (commandArguments.ContainsKey("-cct"))
                {
                    var file = commandArguments["-cct"];

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Reading file: ");
                    Console.ResetColor();
                    Console.WriteLine(file);

                    int X = 1;
                    int Y = 1;

                    var elias = new EliasLibrary.Elias(Path.GetFileNameWithoutExtension(file), file, X, Y, ffdecPath, outputPath, directorPath);
                    SaveFiles(elias.Parse(), outputPath, cctPath);
                }

                /*string fullFileName = args[0];
                string fileName = Path.GetFileName(fullFileName);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Reading file: ");
                Console.ResetColor();
                Console.WriteLine(args[0]);

                var fileExtension = Path.GetExtension(fileName);

                if (fileExtension != ".swf")
                {
                    WriteError("The input file is not a .swf file!");

#if DEBUG
                    Console.Read();
#endif
                    return;
                }

                var outputDirectory = Path.Combine(Environment.CurrentDirectory, "output");

                if (!Directory.Exists(CCT_PATH))
                {
                    Directory.CreateDirectory(CCT_PATH);
                }


                var elias = new EliasLibrary.Elias(Path.GetFileNameWithoutExtension(fileName), fullFileName, int.Parse(args[1]), int.Parse(args[2]),  FFDEC_PATH, OUTPUT_PATH, DIRECTOR_PATH);
                var filesWritten = elias.Parse();

                Console.WriteLine("Done!");

                foreach (var castFile in filesWritten)
                {
                    var newFilePath = Path.Combine(OUTPUT_PATH, castFile);
                    var castFilePath = Path.Combine(CCT_PATH, castFile);

                    if (File.Exists(castFilePath))
                        File.Delete(castFilePath);

                    File.Copy(newFilePath, castFilePath);
                }*/

                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
            }

#if DEBUG
            Console.Read();
#endif
        }

        private static void SaveFiles(IEnumerable<string> outputFiles, string outputPath, string cctPath)
        {
            foreach (var castFile in outputFiles)
            {
                var newFilePath = Path.Combine(outputPath, castFile);
                var castFilePath = Path.Combine(cctPath, castFile);

                if (File.Exists(castFilePath))
                    File.Delete(castFilePath);

                File.Copy(newFilePath, castFilePath);
            }
        }

        private static void WriteError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error occurred: ");
            Console.ResetColor();
            Console.WriteLine(errorMessage);
        }
    }
}
