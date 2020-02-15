using EliasApp.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace EliasApp
{
    public class FurniItem
    {
        public string Type;
        public int SpriteId;
        public string FileName;
        public string Revision;
        public string Unknown;
        public int Length;
        public int Width;
        public string Colour;
        public string Name;
        public string Description;
        public string[] RawData
        {
            get
            {
                return new string[] { Type, Convert.ToString(SpriteId), FileName, Revision, Unknown, Length == -1 ? "" : Convert.ToString(Length), Width == -1 ? "" : Convert.ToString(Width), Colour, Name, Description };
            }
        }

        public bool Ignore;

        public FurniItem(string[] data)
        {
            this.Type = data[0];
            this.SpriteId = int.Parse(data[1]);
            this.FileName = data[2];
            this.Revision = data[3];
            this.Unknown = data[4];
            try
            {
                this.Length = Convert.ToInt32(data[5]);
                this.Width = Convert.ToInt32(data[6]);
            }
            catch (Exception ex)
            {
                this.Length = -1;
                this.Width = -1;
            }

            this.Colour = data[7];
            this.Name = data[8];
            this.Description = data[9];
        }

        public FurniItem(int SpriteId)
        {
            this.SpriteId = SpriteId;
            this.Ignore = true;
        }
    }

    class Program
    {
        private static List<FurniItem> ItemList = new List<FurniItem>();

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

                var furnidataPath = Config.Instance.GetString("furnidata.path");
                var ffdecPath = Config.Instance.GetString("ffdec.path");
                var directorPath = Config.Instance.GetString("elias.cct.converter.app");

                if (!File.Exists(furnidataPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Furnidata doesn't exist, check your paths!");
                    Console.ResetColor();

#if DEBUG
                    Console.Read();
#endif
                    return;
                }

                if (!File.Exists(ffdecPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("FFDEC doesn't exist, check your paths!");
                    Console.ResetColor();

#if DEBUG
                    Console.Read();
#endif
                    return;
                }

                if (!File.Exists(directorPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Director doesn't exist, check your paths!");
                    Console.ResetColor();

#if DEBUG
                    Console.Read();
#endif
                    return;
                }

                var officialFileContents = File.ReadAllText(furnidataPath);
                officialFileContents = officialFileContents.Replace("]]\n[[", "],[");
                var officialFurnidataList = JsonConvert.DeserializeObject<List<string[]>>(officialFileContents);

                foreach (var stringArray in officialFurnidataList)
                {
                    ItemList.Add(new FurniItem(stringArray));
                }

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

                        var sprite = Path.GetFileNameWithoutExtension(file);

                        int X = 1;
                        int Y = 1;

                        try
                        {
                            var elias = new EliasLibrary.Elias(sprite, file, X, Y, ffdecPath, outputPath, directorPath);
                            SaveFiles(elias.Parse(), outputPath, cctPath);
                        }
                        catch (Exception ex)
                        {
                            WriteError(ex.ToString());
                            ErrorLogging(ex, sprite);
                        }
                    }
                }
                else if (commandArguments.ContainsKey("-cct"))
                {
                    var file = commandArguments["-cct"];
                    var sprite = Path.GetFileNameWithoutExtension(file);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Reading file: ");
                    Console.ResetColor();
                    Console.WriteLine(file);

                    int X = 1;
                    int Y = 1;

                    try
                    {
                        var elias = new EliasLibrary.Elias(sprite, file, X, Y, ffdecPath, outputPath, directorPath);
                        SaveFiles(elias.Parse(), outputPath, cctPath);
                    }
                    catch (Exception ex)
                    {
                        WriteError(ex.ToString());
                        ErrorLogging(ex, sprite);
                    }
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
                ErrorLogging(ex, "[no furni]");
            }

#if DEBUG
            Console.Read();
#endif
        }

        private static void SaveFiles(IEnumerable<string> outputFiles, string outputPath, string cctPath)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Saving files to disk: ");
            Console.ResetColor();
            Console.WriteLine(string.Join(", ", outputFiles));

            foreach (var castFile in outputFiles)
            {
                var newFilePath = Path.Combine(outputPath, castFile);
                var castFilePath = Path.Combine(cctPath, castFile);

                if (File.Exists(castFilePath))
                    File.Delete(castFilePath);

                File.Copy(newFilePath, castFilePath);
            }
        }

        public static void ErrorLogging(Exception ex, string furniSprite)
        {
            string strPath = @"error.log";

            if (!File.Exists(strPath))
                File.Create(strPath).Dispose();

            using (StreamWriter sw = File.AppendText(strPath))
            {
                sw.WriteLine("=============Error Logging ===========");
                sw.WriteLine(ex.GetType().FullName + " occurred when processing furni: " + furniSprite);
                sw.WriteLine("Date: " + DateTime.Now);
                sw.WriteLine("Error Message: " + ex.Message);
                sw.WriteLine("Stack Trace: " + ex.StackTrace);
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
