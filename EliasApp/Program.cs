using EliasApp.Utilities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;

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

        public FurniItem()
        {
            this.Type = "";
            this.SpriteId = 0;
            this.FileName = "";
            this.Revision = "";
            this.Unknown = "";
            this.Length = 0;
            this.Width = 0;
            this.Colour = "";
            this.Name = "";
            this.Description = "";
        }

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
            var instance = Config.Instance;

            if (args.Length == 0)
            {
                Console.WriteLine("No arguments supplied");

                if (!Config.Instance.GetBoolean("close.when.finished"))
                    Console.Read();

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

                    if (!instance.GetBoolean("close.when.finished"))
                        Console.Read();

                    return;
                }

                var furnidataPath = instance.GetString("furnidata.path");
                var ffdecPath = instance.GetString("ffdec.path");
                var directorPath = instance.GetString("elias.cct.converter.app");

                if (!File.Exists(furnidataPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Furnidata doesn't exist, check your paths!");
                    Console.ResetColor();

                    if (!instance.GetBoolean("close.when.finished"))
                        Console.Read();

                    return;
                }

                if (!File.Exists(ffdecPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("FFDEC doesn't exist, check your paths!");
                    Console.ResetColor();

                    if (!instance.GetBoolean("close.when.finished"))
                        Console.Read();

                    return;
                }

                if (!File.Exists(directorPath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Director doesn't exist, check your paths!");
                    Console.ResetColor();

                    if (!instance.GetBoolean("close.when.finished"))
                        Console.Read();

                    return;
                }

                Console.WriteLine("Elias by Quackster");
                Console.WriteLine("");
                Console.WriteLine("Thanks to:");
                Console.WriteLine("- Sefhriloff");
                Console.WriteLine("");
                Console.WriteLine("Written in Feburary 2020");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Reading furnidata supplied...");
                Console.ResetColor();

                var furnidataExtension = Path.GetExtension(furnidataPath);

                if (furnidataExtension == ".xml")
                {
                    ParseFurnidataXML(furnidataPath);
                }
                else
                {
                    var officialFileContents = File.ReadAllText(furnidataPath);
                    officialFileContents = officialFileContents.Replace("]]\n[[", "],[");
                    var officialFurnidataList = JsonConvert.DeserializeObject<List<string[]>>(officialFileContents);

                    foreach (var stringArray in officialFurnidataList)
                    {
                        ItemList.Add(new FurniItem(stringArray));
                    }
                }

                var outputPath = Path.Combine(Path.GetDirectoryName(directorPath), "temp");
                var cctPath = instance.GetString("output.path");

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

                        ConvertFile(file, ffdecPath, outputPath, directorPath, cctPath);
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

                    ConvertFile(file, ffdecPath, outputPath, directorPath, cctPath);
                }

                Console.WriteLine("Done!");
            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
                ErrorLogging(ex, "[no furni]");
            }

            if (!Config.Instance.GetBoolean("close.when.finished"))
                Console.Read();
        }

        private static void ParseFurnidataXML(string furnidataPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(furnidataPath);

            var floorTypes = xmlDoc.SelectNodes("//furnidata/roomitemtypes/furnitype");
            var floorItems = floorTypes.Count;

            for (int i = 0; i < floorItems; i++)
            {
                var itemNode = floorTypes.Item(i);

                if (itemNode.Attributes.GetNamedItem("classname") == null)
                    continue;

                var className = itemNode.Attributes.GetNamedItem("classname").InnerText;

                var length = itemNode.ChildNodes.Item(2).InnerText;
                var width = itemNode.ChildNodes.Item(3).InnerText;

                FurniItem furniItem = new FurniItem();
                furniItem.Length = int.Parse(length);
                furniItem.Width = int.Parse(width);
                furniItem.Type = "S";
                furniItem.FileName = className;
                ItemList.Add(furniItem);
            }

            var wallTypes = xmlDoc.SelectNodes("//furnidata/wallitemtypes/furnitype");
            var wallItems = wallTypes.Count;

            for (int i = 0; i < wallItems; i++)
            {
                var itemNode = wallTypes.Item(i);

                if (itemNode.Attributes.GetNamedItem("classname") == null)
                    continue;

                var className = itemNode.Attributes.GetNamedItem("classname").InnerText;

                FurniItem furniItem = new FurniItem();
                furniItem.Type = "I";
                furniItem.FileName = className;
                ItemList.Add(furniItem);
            }
        }

        private static void ConvertFile(string file, string ffdecPath, string outputPath, string directorPath, string cctPath)
        {
            var sprite = Path.GetFileNameWithoutExtension(file);
            var furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite);

            int X = 1;
            int Y = 1;
            bool isWallItem = false;

            if (furniItem == null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("No furnidata entry found for item: ");
                Console.ResetColor();
                Console.WriteLine(sprite);

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Assuming item is a 1x1 floor item.");
                Console.ResetColor();
            }
            else
            {
                X = furniItem.Length;
                Y = furniItem.Width;
                isWallItem = furniItem.Type.ToUpper() == "I";
            }

            try
            {
                var elias = new EliasLibrary.Elias(isWallItem, sprite, file, X, Y, ffdecPath, outputPath, directorPath,
                    Config.Instance.GetBoolean("generate.small.modern.furni"), 
                    Config.Instance.GetBoolean("generate.small.furni"));

                SaveFiles(elias.Parse(), outputPath, cctPath);
            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
                ErrorLogging(ex, sprite);
            }
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

                if (Config.Instance.GetBoolean("save.as.cst"))
                {
                    castFilePath = castFilePath.Replace(".cct", ".cst");/*.ToCharArray());
                    castFilePath = castFilePath + ".cst";*/
                }

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
