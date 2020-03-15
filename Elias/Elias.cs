using Elias;
using Elias.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace EliasLibrary
{
    public class Elias
    {
        public string Sprite;
        public bool IsSmallFurni;
        public bool IsWallItem;
        public bool IsDownscaled;
        public int X;
        public int Y;

        public string FullFileName;
        public string FileDirectory;

        public string FFDEC_PATH;
        public string OUTPUT_PATH;
        public string DIRECTOR_PATH;
        public string DIRECTOR_PROGRAM;
        public bool GenerateSmallModernFurni;
        public bool GenerateSmallFurni;

        public Dictionary<int, List<string>> Symbols;
        public List<EliasAsset> Assets;

        public ILogging Logging;

        public string CAST_PATH
        {
            get { return Path.Combine(OUTPUT_PATH, "cast_data"); }
        }

        public string IMAGE_PATH
        {
            get { return Path.Combine(CAST_PATH, "images"); }
        }

        public Elias(bool IsWallItem, string sprite, string fileName, int X, int Y, string FFDEC_PATH, string DIRECTOR_PATH, bool generateSmallModernFurni, bool generateSmallFurni)
        {
            this.IsWallItem = IsWallItem;
            this.Sprite = sprite;
            this.FullFileName = fileName;
            this.X = X;
            this.Y = Y;
            this.FileDirectory = new FileInfo(this.FullFileName).DirectoryName;
            this.FFDEC_PATH = FFDEC_PATH;
            this.DIRECTOR_PATH = new FileInfo(DIRECTOR_PATH).DirectoryName;
            this.DIRECTOR_PROGRAM = new FileInfo(DIRECTOR_PATH).FullName;
            this.OUTPUT_PATH = Path.Combine(this.DIRECTOR_PATH, "temp");
            this.Assets = new List<EliasAsset>();
            this.Symbols = new Dictionary<int, List<string>>();

            this.GenerateSmallModernFurni = generateSmallModernFurni;
            this.GenerateSmallFurni = generateSmallFurni;
        }

        public string[] Parse()
        {
            List<string> filesWritten = new List<string>();

            this.IsSmallFurni = false;

            this.TryCleanup();
            this.ExtractAssets();
            this.RunSwfmill();

            this.ReadSymbolClass();
            this.GenerateAliases();
            this.TryWriteIcon();
            this.GenerateShadows();
            this.CreateMemberalias();
            this.GenerateProps();
            this.GenerateAssetIndex();
            this.GenerateAnimations();
            GenerateMissingImages();
            this.RunEliasDirector();

            filesWritten.Add("hh_furni_xx_" + Sprite + ".cct");

            if (!GenerateSmallFurni)
                return filesWritten.ToArray();

            this.IsSmallFurni = true;

            if (!this.ContainsSmallFurni())
            {
                this.Assets.Clear();
                this.Symbols.Clear();

                if (!GenerateSmallModernFurni)
                    return filesWritten.ToArray();

                this.Assets.Clear();
                this.Symbols.Clear();

                BinaryDataUtil.ReplaceFiles(this.OUTPUT_PATH, "_64_", "_32_");
                BinaryDataUtil.DownscaleImages(this.OUTPUT_PATH);

                TryCleanup(true);
                ReadSymbolClass();
                GenerateAliases(true);

                if (IsDownscaled)
                {
                    Logging.Log(ConsoleColor.Red, "WARNING: Downscaling small version for: " + Sprite);
                }

                TryWriteIcon();
                GenerateShadows();
                CreateMemberalias();
                GenerateProps();
                GenerateAssetIndex();
                GenerateAnimations();
                GenerateMissingImages();
                RunEliasDirector();
            }
            else
            {
                this.Assets.Clear();
                this.Symbols.Clear();

                TryCleanup(true);
                ReadSymbolClass();
                GenerateAliases();
                TryWriteIcon();
                GenerateShadows();
                CreateMemberalias();
                GenerateProps();
                GenerateAssetIndex();
                GenerateAnimations();
                GenerateMissingImages();
                RunEliasDirector();

                filesWritten.Add("hh_furni_xx_s_" + Sprite + ".cct");
            }
            return filesWritten.ToArray();
        }

        private void GenerateMissingImages()
        {
            if (this.IsWallItem)
            {
                if (this.Assets.Count(asset => asset.FlashAssetName != null && asset.FlashAssetName.Contains("_a_")) > 0 && this.Assets.Count(asset => asset.FlashAssetName != null && asset.FlashAssetName.Contains("_b_")) == 0)
                {
                    // Shift a->b and so forth
                    AssetUtil.CrudeReplace(this.CAST_PATH);

                    foreach (var file in Directory.GetFiles(this.IMAGE_PATH))
                    {
                        if (Path.GetExtension(file) != ".png" || Path.GetFileNameWithoutExtension(file).EndsWith("_small"))
                        {
                            continue;
                        }

                        string newFile = Path.Combine(this.IMAGE_PATH, Path.GetFileNameWithoutExtension(file).Replace("_b_", "_a_") + ".png");
                        string regPointFile = Path.Combine(this.IMAGE_PATH, Path.GetFileNameWithoutExtension(file).Replace("_b_", "_a_") + ".txt");

                        File.WriteAllText(regPointFile, "0,0");

                        Console.WriteLine("Generating: " + newFile);

                        Bitmap bmp = new Bitmap(1, 1);
                        bmp.Save(newFile, ImageFormat.Png);
                    }
                }
                else
                {
                    // Shift a->b and so forth
                    AssetUtil.CrudeReplace(this.CAST_PATH);

                    foreach (var file in Directory.GetFiles(this.IMAGE_PATH))
                    {
                        if (Path.GetExtension(file) != ".png" || Path.GetFileNameWithoutExtension(file).EndsWith("_small"))
                        {
                            continue;
                        }

                        string newFile = Path.Combine(this.IMAGE_PATH, Path.GetFileNameWithoutExtension(file).Replace("_b_", "_a_") + ".png");

                        if (!File.Exists(newFile))
                        {
                            string regPointFile = Path.Combine(this.IMAGE_PATH, Path.GetFileNameWithoutExtension(file).Replace("_b_", "_a_") + ".txt");

                            File.WriteAllText(regPointFile, "0,0");

                            Console.WriteLine("Generating: " + newFile);

                            Bitmap bmp = new Bitmap(1, 1);
                            bmp.Save(newFile, ImageFormat.Png);
                        }
                    }
                }

                var dataPath = Path.Combine(this.CAST_PATH, this.Sprite + ".data");
                var data = File.Exists(dataPath) ? File.ReadAllText(dataPath) : "";

                if (data.Length == 0)
                {
                    List<string> layers = new List<string>();

                  

                    foreach (var file in Directory.GetFiles(this.IMAGE_PATH))
                    {
                        if (Path.GetExtension(file) != ".png" || Path.GetFileNameWithoutExtension(file).EndsWith("_small"))
                        {
                            continue;
                        }

                        string layer = Path.GetFileNameWithoutExtension(file).Split('_')[Path.GetFileNameWithoutExtension(file).Split('_').Length - 2];
                        
                        if (!layers.Contains(layer))
                        {
                            layers.Add(layer);
                        }
                    }

                    StringBuilder output = new StringBuilder();
                    output.Append("[\r" +
                                    "states:[1],\r" +
                                    "layers:[ \r");

                    int k = 0;
                    foreach (string layer in layers)
                    {
                        output.Append(layer + ":[ [ frames:[ 0 ] ] ]");

                        if (layers.Count - 1 > k)
                        {
                            output.Append(", ");
                        }

                        k++;

                        output.Append("\r");
                    }

                    output.Append("]\r");
                    output.Append("]\r");

                    File.WriteAllText(dataPath, output.ToString());
                }
            }
        }

        private bool ContainsSmallFurni()
        {
            var xmlData = BinaryDataUtil.SolveFile(this.OUTPUT_PATH, "assets");

            if (xmlData == null)
            {
                return false;
            }

            var assets = xmlData.SelectSingleNode("//assets");

            for (int i = 0; i < assets.ChildNodes.Count; i++)
            {
                var node = assets.ChildNodes.Item(i);

                if (node == null)
                    continue;

                if (node.OuterXml.Contains("_32_"))
                    return true;
            }

            return false;
        }

        private void GenerateAliases(bool isDownscaled = false)
        {
            this.IsDownscaled = isDownscaled;
            var xmlData = BinaryDataUtil.SolveFile(this.OUTPUT_PATH, "assets");

            if (xmlData == null)
            {
                return;
            }

            var assets = xmlData.SelectSingleNode("//assets");

            for (int i = 0; i < assets.ChildNodes.Count; i++)
            {
                var node = assets.ChildNodes.Item(i);

                if (node == null)
                    continue;

                if (IsSmallFurni && node.OuterXml.Contains("_64_"))
                    continue;

                if (!IsSmallFurni && node.OuterXml.Contains("_32_"))
                    continue;

                var eliasAlias = new EliasAsset(this, node);
                eliasAlias.Parse();

                if (eliasAlias.ShockwaveAssetName == null && !eliasAlias.IsIcon && !eliasAlias.IsShadow)
                    continue;

                Assets.Add(eliasAlias);
            }

            foreach (var eliasAlias in Assets)
            {
                if (eliasAlias.IsIcon)
                    continue;

                eliasAlias.WriteAssets();
                eliasAlias.WriteFlippedAssets();
                eliasAlias.WriteImageNames();
                eliasAlias.WriteRegPointData();
            }

            this.SantiyCheckFrames();
        }

        private void TryCleanup(bool castPathOnly = false)
        {
            try
            {
                if (castPathOnly)
                {
                    if (Directory.Exists(this.CAST_PATH))
                        AssetUtil.DeleteDirectory(this.CAST_PATH);

                    Directory.CreateDirectory(this.CAST_PATH);

                    if (Directory.Exists(this.IMAGE_PATH))
                        AssetUtil.DeleteDirectory(this.IMAGE_PATH);

                    Directory.CreateDirectory(this.IMAGE_PATH);
                }
                else
                {
                    if (Directory.Exists(this.OUTPUT_PATH))
                        AssetUtil.DeleteDirectory(this.OUTPUT_PATH);

                    Directory.CreateDirectory(this.OUTPUT_PATH);

                    if (Directory.Exists(this.IMAGE_PATH))
                        AssetUtil.DeleteDirectory(this.IMAGE_PATH);

                    Directory.CreateDirectory(this.IMAGE_PATH);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
            }
            finally
            {
                File.WriteAllText(Path.Combine(CAST_PATH, "sprite.data"),
                    string.Format("{0}|{1}", this.Sprite, (this.IsSmallFurni ? "small" : "large")));
            }
        }

        private void ExtractAssets()
        {
            var p = new Process();
            p.StartInfo.FileName = FFDEC_PATH;
            p.StartInfo.Arguments = string.Format("-export \"binaryData,image\" \"{0}\" \"{1}\"", OUTPUT_PATH, this.FullFileName);
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
        }

        private void RunSwfmill()
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "swfmill\\swfmill.exe");
            p.StartInfo.Arguments = "swf2xml \"" + this.FullFileName + "\" \"" + Path.Combine(OUTPUT_PATH, Sprite + ".xml") + "\"";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
        }

        private void RunEliasDirector()
        {
            try
            {
                //AssetUtil.DeleteDirectory(Path.Combine(OUTPUT_PATH, "images"), true);
                //AssetUtil.DeleteDirectory(Path.Combine(OUTPUT_PATH, "binaryData"), true);
            }
            catch { }

            var p = new Process();
            p.StartInfo.WorkingDirectory = DIRECTOR_PATH;
            p.StartInfo.FileName = DIRECTOR_PROGRAM;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            p.WaitForExit();
        }

        private void ReadSymbolClass()
        {
            var xmlPath = Path.Combine(OUTPUT_PATH, Sprite + ".xml");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            var nodes = xmlDoc.SelectNodes("//swf/Header/tags/SymbolClass/symbols/Symbol");

            for (int i = 0; i < nodes.Count; i++)
            {
                var symbol = nodes.Item(i);

                if (symbol == null)
                {
                    continue;
                }

                int objectID = int.Parse(symbol.Attributes.GetNamedItem("objectID").InnerText);
                string name = symbol.Attributes.GetNamedItem("name").InnerText;

                if (!Symbols.ContainsKey(objectID))
                    Symbols.Add(objectID, new List<string>());

                Symbols[objectID].Add(name);
            }

        }

        private void SantiyCheckFrames()
        {               
            if (IsWallItem)
                    return;

            List<string> assetTypes = new List<string>();

            for (int i = 0; i < Assets.Count; i++)
            {
                var asset = Assets[i];

                if (asset.IsIcon)
                    continue;

                if (asset.IsShadow)
                    continue;

                string[] data = asset.ShockwaveAssetName.Replace((this.IsSmallFurni ? "s_" : "") + Sprite + "_", "").Split('_');
                string member = (this.IsSmallFurni ? "s_" : "") + Sprite + "_" + data[0] + "_" + data[1] + "_" + data[2] + "_" + data[3] + "_" + data[4];

                if (!assetTypes.Contains(member))
                    assetTypes.Add(member);
                else
                    continue;

                /*Console.WriteLine("-----------");
                Console.WriteLine("Finding: " + member + "_0");

                foreach (var a in Assets)
                {
                    Console.WriteLine(a.ShockwaveAssetName);
                }

                Console.WriteLine("-----------");*/

                    if (Assets.Count(f => f.ShockwaveAssetName == (member + "_0")) == 0)
                {
                    if (asset.IsMemberAlias)
                    {
                        string[] sourceData = asset.ShockwaveSourceAliasName.Replace(Sprite + "_", "").Split('_');
                        string sourceMember = Sprite + "_" + sourceData[0] + "_" + sourceData[1] + "_" + sourceData[2] + "_" + sourceData[3] + "_" + sourceData[4];

                        var newAsset = new EliasAsset(this, asset.Node);
                        newAsset.Parse();
                        newAsset.IsMemberAlias = true;
                        newAsset.ShockwaveAssetName = (member + "_0");
                        newAsset.ShockwaveSourceAliasName = (sourceMember + "_0");
                        Assets.Add(newAsset);

                        //Console.WriteLine("Added to memberalias: " + newAsset.ShockwaveAssetName + " => " + newAsset.ShockwaveSourceAliasName);
                    }
                    else
                    {
                        //Console.WriteLine("Added source: " + (member + "_0"));
      
                        Bitmap bmp = new Bitmap(1, 1);
                        bmp.Save(Path.Combine(IMAGE_PATH, (member + "_0") + ".png"), ImageFormat.Png);
                        File.WriteAllText(Path.Combine(IMAGE_PATH, (member + "_0") + ".txt"), "0,0");
                    }
                }
            }
        }

        private void GenerateShadows()
        {
            foreach (var asset in Assets)
            {
                if (!asset.IsShadow)
                    continue;

                asset.TryShadow();
            }
        }

        private void TryWriteIcon()
        {
            foreach (var asset in Assets)
            {
                if (!asset.IsIcon)
                    continue;

                asset.TryIcon();
            }
        }

        private void CreateMemberalias()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var eliasAsset in Assets)
            {
                if (eliasAsset.IsIcon)
                    continue;

                if (eliasAsset.IsShadow)
                    continue;

                if (eliasAsset.ShockwaveSourceAliasName != null && eliasAsset.IsInverted())
                {
                    stringBuilder.Append(eliasAsset.ShockwaveAssetName);
                    stringBuilder.Append("=");
                    stringBuilder.Append(eliasAsset.ShockwaveSourceAliasName);

                    if (eliasAsset.IsInverted())
                    {
                        stringBuilder.Append("*");
                    }

                    stringBuilder.Append("\r");
                }
            }

            foreach (var eliasAsset in Assets)
            {
                if (eliasAsset.IsIcon)
                    continue;

                if (eliasAsset.IsShadow)
                    continue;

                if (eliasAsset.ShockwaveSourceAliasName != null && !eliasAsset.IsInverted())
                {
                    stringBuilder.Append(eliasAsset.ShockwaveAssetName);
                    stringBuilder.Append("=");
                    stringBuilder.Append(eliasAsset.ShockwaveSourceAliasName);
                    stringBuilder.Append("\r");
                }
            }

            foreach (var eliasAsset in Assets)
            {
                if (eliasAsset.IsShadow)
                {
                    if (eliasAsset.ShockwaveSourceAliasName != null && eliasAsset.IsInverted())
                    {
                        stringBuilder.Append(eliasAsset.ShockwaveAssetName);
                        stringBuilder.Append("=");
                        stringBuilder.Append(eliasAsset.ShockwaveSourceAliasName);

                        if (eliasAsset.IsInverted())
                        {
                            stringBuilder.Append("*");
                        }

                        stringBuilder.Append("\r");
                    }
                }
            }

            foreach (var eliasAsset in Assets)
            {
                if (eliasAsset.IsShadow)
                {
                    if (eliasAsset.ShockwaveSourceAliasName != null && !eliasAsset.IsInverted())
                    {
                        stringBuilder.Append(eliasAsset.ShockwaveAssetName);
                        stringBuilder.Append("=");
                        stringBuilder.Append(eliasAsset.ShockwaveSourceAliasName);

                        if (eliasAsset.IsInverted())
                        {
                            stringBuilder.Append("*");
                        }

                        stringBuilder.Append("\r");
                    }
                }
            }

            File.WriteAllText(Path.Combine(CAST_PATH, "memberalias.index"), stringBuilder.ToString());
        }

        private void GenerateProps()
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            var xmlData = BinaryDataUtil.SolveFile(this.OUTPUT_PATH, "visualization");

            Dictionary<int, int> shiftValues = new Dictionary<int, int>();

            if (xmlData == null)
            {
                return;
            }

            XmlNodeList layers = null;
            string prefix = "";

            if (!IsDownscaled)
            {
                prefix = "//visualizationData/visualization[@size='" + (IsSmallFurni ? "32" : "64") + "']";
                layers = xmlData.SelectNodes(prefix + "/layers/layer");
            }
            else
            {
                prefix = "//visualizationData/visualization[@size='64']";
                layers = xmlData.SelectNodes(prefix + "/layers/layer");
            }

            Dictionary<string, string> layerData = new Dictionary<string, string>();

            for (int i = 0; i < layers.Count; i++)
            {
                var node = layers.Item(i);

                if (node == null)
                {
                    continue;
                }

                if (node.Name != "layer")
                {
                    continue;
                }

                if (node.Attributes.GetNamedItem("z") == null && node.Attributes.GetNamedItem("alpha") == null && node.Attributes.GetNamedItem("ink") == null)
                {
                    continue;
                }

                char letter = alphabet[int.Parse(node.Attributes.GetNamedItem("id").InnerText)];
                //string firstSection = "\"" + letter + "\": [{0}]";
                string secondSection = "";

                if (node.Attributes.GetNamedItem("z") != null)
                {
                    int z = int.Parse(node.Attributes.GetNamedItem("z").InnerText);

                    if (!shiftValues.ContainsKey(z))
                        shiftValues.Add(z, z);
                    else
                        shiftValues[z] = shiftValues[z] - 1;

                    if (xmlData.SelectNodes(prefix + "/directions/direction/layer[@id='" + node.Attributes.GetNamedItem("id").InnerText + "']").Count == 0)
                    {
                        secondSection += "#zshift: [" + shiftValues[z] + "], ";
                    }
                }

                if (node.Attributes.GetNamedItem("alpha") != null && node.Attributes.GetNamedItem("ink") == null)//if (node.Attributes.GetNamedItem("alpha") != null)
                {
                    double alphaValue = double.Parse(node.Attributes.GetNamedItem("alpha").InnerText);
                    double newValue = (double)((alphaValue / 255) * 100);
                    secondSection += "#blend: " + (int)newValue + ", ";
                }

                if (node.Attributes.GetNamedItem("ink") != null)//if (node.Attributes.GetNamedItem("alpha") != null)
                {
                    if (node.Attributes.GetNamedItem("ink").InnerText != "COPY")
                    {
                        secondSection += "#ink: 33, ";
                        secondSection += "#transparent: 1, "; // Don't allow click
                    }
                }

                if (layerData.ContainsKey(Convert.ToString(letter)))
                    layerData.Remove(Convert.ToString(letter));

                layerData.Add(Convert.ToString(letter), secondSection);
                //sections.Add(string.Format(firstSection, secondSection));
            }

            var directions = new Dictionary<string, EliasDirection>();
            var directionLayers = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (IsSmallFurni ? "32" : "64") + "']/directions/direction/layer");

            for (int i = 0; i < directionLayers.Count; i++)
            {
                var node = directionLayers.Item(i);

                if (node == null)
                {
                    continue;
                }

                if (node.Name != "layer")
                {
                    continue;
                }

                var layerNode = node;
                var directionNode = layerNode.ParentNode;

                if (layerNode.Attributes.GetNamedItem("id") == null ||
                    layerNode.Attributes.GetNamedItem("z") == null ||
                    directionNode.Attributes.GetNamedItem("id") == null)
                {
                    continue;
                }

                string letter = Convert.ToString(alphabet[int.Parse(layerNode.Attributes.GetNamedItem("id").InnerText)]);
                int z = int.Parse(layerNode.Attributes.GetNamedItem("z").InnerText);
                int direction = int.Parse(directionNode.Attributes.GetNamedItem("id").InnerText);

                if (!directions.ContainsKey(letter))
                {
                    directions.Add(letter, new EliasDirection());
                }

                directions[letter].Coords[direction] = z;
                directions[letter].Coords[direction + 1] = z;
            }

            var j = 0;
            foreach (var zData in directions)
            {
                string letter = zData.Key;

                if (!layerData.ContainsKey(letter))
                {
                    layerData.Add(letter, "");
                }

                layerData[letter] += "#zshift: [" + zData.Value.Props + "], ";
                //sections.Add("\"" + zData.Key + "\": [#zshift: [" + zData.Value.Props + "]]" + ((j + 1 < directions.Count) ? ", " : ""));
                j++;
            }

            StringBuilder output = new StringBuilder();

            if (layerData.Count > 0)
            {
                output.Append("[");

                int k = 0;
                foreach (var layer in layerData)
                {
                    output.Append("\"" + layer.Key + "\": ");
                    output.Append("[" + (layer.Value.Length >= 2 ? layer.Value.Substring(0, layer.Value.Length - 2) : ":") + "]");

                    if (layerData.Count - 1 > k)
                    {
                        output.Append(", ");
                    }

                    k++;
                }

                output.Append("]");
            }

            File.WriteAllText(Path.Combine(CAST_PATH, ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + ".props"), output.ToString());
            //File.WriteAllText(Path.Combine(CAST_PATH, ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + ".props"), sections.Count > 0 ? "[" + string.Join(", ", sections) + "]" : "");
        }

        private void GenerateAnimations()
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            var xmlData = BinaryDataUtil.SolveFile(this.OUTPUT_PATH, "visualization");

            var animations = 0;
            var sections = new SortedDictionary<string, EliasAnimation>();

            if (xmlData == null)
            {
                return;
            }

            XmlNodeList frames = null;

            if (!IsDownscaled)
            {
                frames = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (IsSmallFurni ? "32" : "64") + "']/animations/animation/animationLayer/frameSequence/frame");
            }
            else
            {
                frames = xmlData.SelectNodes("//visualizationData/visualization[@size='64']/animations/animation/animationLayer/frameSequence/frame");
            }

            int highestAnimationLayer = 0;

            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames.Item(i);

                var animationLayer = frame.ParentNode.ParentNode;
                int letterPosition = int.Parse(animationLayer.Attributes.GetNamedItem("id").InnerText);

                if (letterPosition < 0 || letterPosition > alphabet.Length)
                {
                    continue;
                }

                var animationLetter = Convert.ToString(alphabet[int.Parse(animationLayer.Attributes.GetNamedItem("id").InnerText)]);

                highestAnimationLayer = int.Parse(animationLayer.Attributes.GetNamedItem("id").InnerText) + 1;

                var animation = frame.ParentNode.ParentNode.ParentNode;
                var animationId = int.Parse(animation.Attributes.GetNamedItem("id").InnerText);

                var castAnimationId = animationId + 1;

                if (castAnimationId > animations)
                {
                    animations = castAnimationId;
                }

                if (!sections.ContainsKey(animationLetter))
                {
                    sections.Add(animationLetter, new EliasAnimation());//new Dictionary<int, List<string>>());
                }

                if (!sections[animationLetter].States.ContainsKey(animationId))
                {
                    var frameClass = new EliasFrame();
                    sections[animationLetter].States.Add(animationId, frameClass);

                    if (animationLayer.Attributes.GetNamedItem("loopCount") != null)
                        frameClass.Loop = int.Parse(animationLayer.Attributes.GetNamedItem("loopCount").InnerText);

                    if (animationLayer.Attributes.GetNamedItem("frameRepeat") != null)
                        frameClass.FramesPerSecond = int.Parse(animationLayer.Attributes.GetNamedItem("frameRepeat").InnerText);
                }

                sections[animationLetter].States[animationId].Frames.Add(frame.Attributes.GetNamedItem("id").InnerText);
            }

            for (int i = 0; i < highestAnimationLayer; i++)
            {
                string letter = Convert.ToString(alphabet[i]);

                if (!sections.ContainsKey(letter))
                {
                    var animation = new EliasAnimation();
                    sections.Add(letter, animation);

                    for (int j = 0; j < animations; j++)
                    {
                        if (!animation.States.ContainsKey(j))
                        {
                            var frame = new EliasFrame();
                            frame.Frames.Add("0");

                            animation.States.Add(j, frame);
                        }
                    }
                }
            }

            var states = "";

            for (int i = 0; i < animations; i++)
                states += (i + 1) + ",";

            StringBuilder stringBuilder = new StringBuilder();

            if (animations > 0)
            {
                stringBuilder.Append("[\r");
                stringBuilder.Append("states:[" + states.TrimEnd(",".ToCharArray()) + "],\r");
                stringBuilder.Append("layers:[\r");

                int e = 0;
                foreach (var animation in sections)
                {
                    while (animation.Value.States.Count != animations)
                    {
                        int nextKey = 0;

                        if (animation.Value.States.ContainsKey(nextKey))
                        {
                            while (animation.Value.States.ContainsKey(nextKey))
                            {
                                nextKey++;
                            }
                        }

                        animation.Value.States.Add(nextKey, new EliasFrame());
                        animation.Value.States[nextKey].Frames.Add("0");
                    }

                    stringBuilder.Append(animation.Key + ": [ ");

                    int i = 0;
                    foreach (var f in animation.Value.States)
                    {
                        // loop: 0, delay: 4, 
                        stringBuilder.Append("[ ");

                        if (f.Value.Loop != -1)
                        {
                            stringBuilder.Append("loop: " + f.Value.Loop + ", ");
                        }

                        if (f.Value.FramesPerSecond != -1)
                        {
                            stringBuilder.Append("delay: " + f.Value.FramesPerSecond + ", ");
                        }

                        stringBuilder.Append("frames:[ ");
                        stringBuilder.Append(string.Join(",", f.Value.Frames));

                        if (animation.Value.States.Count - 1 > i)
                        {
                            stringBuilder.Append(" ] ], ");
                        }
                        else
                        {
                            stringBuilder.Append(" ] ] ");
                        }

                        i++;
                    }

                    if (sections.Count - 1 > e)
                    {
                        stringBuilder.Append("],\r");
                    }
                    else
                    {
                        stringBuilder.Append("]\r");
                    }

                    e++;
                }

                stringBuilder.Append("]\r");
                stringBuilder.Append("]\r");
            }

            File.WriteAllText(Path.Combine(CAST_PATH, ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + ".data"), stringBuilder.ToString());
        }

        private void GenerateAssetIndex()
        {
            // [#id: "s_tv_flat", #classes: ["Active Object Class",  "Active Object Extension Class"]]

            if (IsWallItem)
            {
                // [#id: "window_diner", #classes: ["Item Object Class", "Item Object Extension Class", "Window Class"]]
                File.WriteAllText(Path.Combine(CAST_PATH, "asset.index"), "[#id: \"" + ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + "\", #classes: [\"Item Object Class\", \"Item Object Extension Class\"]]");
            }
            else
            {
                File.WriteAllText(Path.Combine(CAST_PATH, "asset.index"), "[#id: \"" + ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + "\", #classes: [\"Active Object Class\",  \"Active Object Extension Class\"]]");
            }
        }
    }
}
