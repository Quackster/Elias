using EliasApp.Utilities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using EliasLibrary;

namespace EliasApp
{
    public class FurniItem
    {
        public string Alias;
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
        private static EliasLogging Logging;
        private static List<FurniItem> ItemList = new List<FurniItem>();

        private static List<string> QueuedFurniture = new List<string>();
        private static Dictionary<string, FurniItem> AliasFurniture = new Dictionary<string, FurniItem>();
        private static List<string> ConvertedFurniture = new List<string>();

        static void Main(string[] args)
        {
            UpdateTile();
            Logging = new EliasLogging();

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
                    Logging.Log(ConsoleColor.Red, "Furnidata doesn't exist, check your paths!");

                    if (!instance.GetBoolean("close.when.finished"))
                        Console.Read();

                    return;
                }

                if (!File.Exists(ffdecPath))
                {
                    Logging.Log(ConsoleColor.Red, "FFDEC doesn't exist, check your paths!");

                    if (!instance.GetBoolean("close.when.finished"))
                        Console.Read();

                    return;
                }

                if (!File.Exists(directorPath))
                {
                    Logging.Log(ConsoleColor.Red, "Director doesn't exist, check your paths!");

                    if (!instance.GetBoolean("close.when.finished"))
                        Console.Read();

                    return;
                }

                Logging.Log(ConsoleColor.Gray, "prjElias");
                Logging.Log(ConsoleColor.Gray, "");
                Logging.Log(ConsoleColor.Gray, "THE FREE AND OPEN SOURCE .SWF TO .CCT CONVERTER");
                Logging.Log(ConsoleColor.Gray, "COPYRIGHT (C) 2020 - QUACKSTER");
                Logging.Log(ConsoleColor.Gray, "");
                Logging.Log(ConsoleColor.Gray, "Shoutout to:");
                Logging.Log(ConsoleColor.Gray, "- Sefhriloff");
                Logging.Log(ConsoleColor.Gray, "");
                Logging.Log(ConsoleColor.Gray, "Written in Feburary 2020");

                Logging.Log(ConsoleColor.Yellow, "Reading furnidata supplied...");

                var furnidataExtension = Path.GetExtension(furnidataPath);

                if (furnidataExtension == ".xml")
                {
                    ParseFurnidataXML(furnidataPath);
                }
                else
                {
                    var fileContents = File.ReadAllText(furnidataPath);
                    fileContents = fileContents.Substring(1, fileContents.Length - 2);

                    string[] chunks = Regex.Split(fileContents, "\n\r{1,}|\n{1,}|\r{1,}", RegexOptions.Multiline);
                    foreach (string chunk in chunks)
                    {
                        MatchCollection collection = Regex.Matches(chunk, @"\[+?((.)*?)\]");

                        foreach (Match item in collection)
                        {
                            string itemData = item.Value;

                            try
                            {
                                List<string> splitted = new List<string>();

                                itemData = itemData.Substring(1, itemData.Length - 2);
                                itemData = itemData.Replace("\",\"", "\"|\"");

                                string[] splitData = itemData.Split('|').Select(x => x.Length > 1 ? x.Substring(1, x.Length - 2) : string.Empty).ToArray();
                                ItemList.Add(new FurniItem(splitData));
                            }
                            catch (Exception ex)
                            {
                                return;
                            }
                        }
                    }
                }

                var cctPath = instance.GetString("output.path");

                if (commandArguments.ContainsKey("-directory"))
                {
                    var directory = commandArguments["-directory"];
                    Logging.Log(ConsoleColor.Green, "Reading directory: " + directory);

                    foreach (var file in Directory.GetFiles(directory, "*.swf"))
                    {
                        QueuedFurniture.Add(file);
                        //ConvertFile(file, ffdecPath, outputPath, directorPath, cctPath);
                    }
                }
                else if (commandArguments.ContainsKey("-cct"))
                {
                    var file = commandArguments["-cct"];
                    QueuedFurniture.Add(file);
                    //ConvertFile(file, ffdecPath, outputPath, directorPath, cctPath);
                }

                foreach (var file in QueuedFurniture.ToArray())
                {
                    var sprite = Path.GetFileNameWithoutExtension(file);
                    var furniItem = ResolveFurni(sprite);

                    if (furniItem == null)
                    {
                        if (furniItem == null)
                        {
                            Console.WriteLine("Failed to find class data for " + sprite + ", type the furni class to base this furni from instead.");
                            string template = Console.ReadLine();
                            furniItem = ItemList.FirstOrDefault(item => item.FileName == template);

                            furniItem = new FurniItem(furniItem.RawData);
                            furniItem.Alias = template;
                            furniItem.FileName = sprite;

                            AliasFurniture[sprite] = furniItem;
                        }
                        else
                        {

                            //if (furniItem == null)
                            //{
                            //    fur new FurniItem()
                            //    {
                            //        Type = "s",
                            //        SpriteId = 0,
                            //        FileName = sprite,
                            //        Revision = "",
                            //        Unknown = "",
                            //        Length = 1,
                            //        Width = 1,
                            //        Colour = "",
                            //        Name = "",
                            //        Description = "",
                            //    };
                            //}

                            Logging.Log(ConsoleColor.Red, "No furnidata entry found for item: " + sprite);
                            QueuedFurniture.Remove(file);
                        }
                    }
                }

                UpdateTile();

                foreach (var file in QueuedFurniture)
                {
                    var sprite = Path.GetFileNameWithoutExtension(file);

                    ConvertFile(file, ffdecPath, directorPath, cctPath);
                    UpdateTile();
                }

                Logging.Log(ConsoleColor.DarkGreen, "Done!");
            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
                ErrorLogging(ex, "[no furni]");
            }

            if (!Config.Instance.GetBoolean("close.when.finished"))
                Console.Read();
        }

        private static void UpdateTile()
        {
            Console.Title = string.Format("prjElias - Converted {0} out of {1} furniture", ConvertedFurniture.Count, QueuedFurniture.Count);
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

                var length = itemNode.SelectNodes("xdim")[0]?.InnerText ?? "1";
                var width = itemNode.SelectNodes("ydim")[0]?.InnerText ?? "1";



                FurniItem furniItem = new FurniItem();
                furniItem.Length = int.Parse(length);
                furniItem.Width = int.Parse(width);
                furniItem.Type = "S";
                furniItem.FileName = className;
                furniItem.Description = itemNode.SelectNodes("description")[0]?.InnerText ?? "";
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

        private static void ConvertFile(string file, string ffdecPath, string directorPath, string cctPath)
        {
            var sprite = Path.GetFileNameWithoutExtension(file);
            var furniItem = ResolveFurni(sprite) ?? AliasFurniture[sprite];

            int X = 1;
            int Y = 1;
            bool isWallItem = false;

            if (furniItem == null)
            {
                /*
                Logging.Log(ConsoleColor.Yellow, "No furnidata entry found for item: " + sprite);
                Logging.Log(ConsoleColor.DarkYellow, "Assuming item is a 1x1 floor item.");
                */
                //Logging.Log(ConsoleColor.Red, "No furnidata entry found for item: " + sprite);
                return;
            }
            else
            {
                X = furniItem.Length;
                Y = furniItem.Width;
                isWallItem = furniItem.Type.ToUpper() == "I";
            }

            try
            {
                Logging.Log(ConsoleColor.Blue, "Creating CCT: " + Path.GetFileNameWithoutExtension(file));

                var elias = new EliasLibrary.Elias(isWallItem, sprite, file, X, Y, ffdecPath, directorPath,
                    Config.Instance.GetBoolean("generate.small.modern.furni"),
                    Config.Instance.GetBoolean("generate.small.furni"));

                SaveFiles(elias.Parse(), elias.OUTPUT_PATH, cctPath);
                ConvertedFurniture.Add(file);
            }
            catch (Exception ex)
            {
                WriteError(ex.ToString());
                ErrorLogging(ex, sprite);
            }
        }

        private static FurniItem ResolveFurni(string sprite)
        {
            FurniItem furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite);

            if (furniItem == null && sprite.EndsWith("_cmp"))
                furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite.Remove(sprite.Length - "_cmp".Length));

            if (furniItem == null && sprite.EndsWith("cmp"))
                furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite.Remove(sprite.Length - "cmp".Length));

            if (furniItem == null && sprite.EndsWith("camp"))
                furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite.Remove(sprite.Length - "camp".Length));

            if (furniItem == null && sprite.EndsWith("_camp"))
                furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite.Remove(sprite.Length - "_camp".Length));

            if (furniItem == null && sprite.EndsWith("c"))
                furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite.Remove(sprite.Length - "c".Length));

            if (furniItem == null && sprite.EndsWith("_c"))
                furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite.Remove(sprite.Length - "_c".Length));

            if (furniItem == null && sprite.EndsWith("campaign"))
                furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite.Remove(sprite.Length - "campaign".Length));

            if (furniItem == null && sprite.EndsWith("_campaign"))
                furniItem = ItemList.FirstOrDefault(item => item.FileName == sprite.Remove(sprite.Length - "_campaign".Length));

            return furniItem;
        }

        private static void SaveFiles(IEnumerable<string> outputFiles, string outputPath, string cctPath)
        {
            Logging.Log(ConsoleColor.Cyan, "Saving files to disk: " + string.Join(", ", outputFiles));

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
