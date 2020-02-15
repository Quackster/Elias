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
        public int X;
        public int Y;

        public string FullFileName;
        public string FileDirectory;

        public string FFDEC_PATH;
        public string OUTPUT_PATH;
        public string DIRECTOR_PATH;

        public Dictionary<int, List<string>> Symbols;
        public List<EliasAsset> Assets;

        public string CAST_PATH
        {
            get { return Path.Combine(OUTPUT_PATH, "cast_data"); }
        }

        public string IMAGE_PATH
        {
            get { return Path.Combine(CAST_PATH, "images"); }
        }

        public Elias(string sprite, string fileName, int X, int Y, string FFDEC_PATH, string OUTPUT_PATH, string DIRECTOR_PATH)
        {
            this.Sprite = sprite;
            this.IsWallItem = X == 0 && Y == 0;
            this.FullFileName = fileName;
            this.X = X == 0 ? 1 : X;
            this.Y = Y == 0 ? 1 : Y;
            this.FileDirectory = new FileInfo(this.FullFileName).DirectoryName;
            this.FFDEC_PATH = FFDEC_PATH;
            this.OUTPUT_PATH = OUTPUT_PATH;
            this.DIRECTOR_PATH = DIRECTOR_PATH;
            this.Assets = new List<EliasAsset>();
            this.Symbols = new Dictionary<int, List<string>>();
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
            this.RunEliasDirector();

            filesWritten.Add("hh_furni_xx_" + Sprite + ".cct");

            this.Assets.Clear();
            this.Symbols.Clear();

            this.IsSmallFurni = true;

            this.TryCleanup(true);
            this.ReadSymbolClass();
            this.GenerateAliases();

            if (this.Assets.Count(asset => asset.FlashAssetName != null && asset.FlashAssetName.Contains("_32_")) == 0)
                return filesWritten.ToArray();

            this.TryWriteIcon();
            this.GenerateShadows();
            this.CreateMemberalias();
            this.GenerateProps();
            this.GenerateAssetIndex();
            this.GenerateAnimations();
            this.RunEliasDirector();

            filesWritten.Add("hh_furni_xx_s_" + Sprite + ".cct");
            return filesWritten.ToArray();
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
            catch
            {

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
            p.StartInfo.WorkingDirectory = new FileInfo(DIRECTOR_PATH).DirectoryName;
            p.StartInfo.FileName = DIRECTOR_PATH;
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

        private void GenerateAliases()
        {
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
                {
                    continue;
                }

                if (IsSmallFurni && node.OuterXml.Contains("_64_"))
                {
                    continue;
                }

                if (!IsSmallFurni && node.OuterXml.Contains("_32_"))
                {
                    continue;
                }

                var eliasAlias = new EliasAsset(this, node);

                if (eliasAlias.ShockwaveAssetName == null && !eliasAlias.IsIcon && !eliasAlias.IsShadow)
                {
                    continue;
                }

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

            File.WriteAllText(Path.Combine(CAST_PATH, "memberalias.index"), stringBuilder.ToString());
        }

        private void GenerateProps()
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            var xmlData = BinaryDataUtil.SolveFile(this.OUTPUT_PATH, "visualization");

            Dictionary<int, int> shiftValues = new Dictionary<int, int>();
            List<string> sections = new List<string>();

            if (xmlData == null)
            {
                return;
            }

            var layers = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (IsSmallFurni ? "32" : "64") + "']/layers/layer");

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

                if (node.Attributes.GetNamedItem("z") == null && node.Attributes.GetNamedItem("alpha") == null)
                {
                    continue;
                }

                char letter = alphabet[int.Parse(node.Attributes.GetNamedItem("id").InnerText)];

                string firstSection = "\"" + letter + "\": [{0}]";
                string secondSection = "";

                if (node.Attributes.GetNamedItem("z") != null)
                {
                    int z = int.Parse(node.Attributes.GetNamedItem("z").InnerText);

                    if (!shiftValues.ContainsKey(z))
                        shiftValues.Add(z, z);
                    else
                        shiftValues[z] = shiftValues[z] - 1;

                    secondSection += "#zshift: [" + shiftValues[z] + "], ";
                }

                if (node.Attributes.GetNamedItem("alpha") != null && node.Attributes.GetNamedItem("ink") == null)//if (node.Attributes.GetNamedItem("alpha") != null)
                {
                    double alphaValue = double.Parse(node.Attributes.GetNamedItem("alpha").InnerText);
                    double newValue = (double)((alphaValue / 255) * 100);
                    secondSection += "#blend: " + (int)newValue + ", ";
                }

                if (node.Attributes.GetNamedItem("ink") != null)//if (node.Attributes.GetNamedItem("alpha") != null)
                {
                    secondSection += "#ink: 33, ";
                }

                if (secondSection.Length > 0)
                {
                    secondSection = secondSection.TrimEnd(", ".ToCharArray());
                }
                else
                {
                    secondSection = ":";
                }

                sections.Add(string.Format(firstSection, secondSection));
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
                sections.Add("\"" + zData.Key + "\": [#zshift: [" + zData.Value.Props + "]]" + ((j + 1 < directions.Count) ? ", " : ""));
                j++;
            }

            File.WriteAllText(Path.Combine(CAST_PATH, ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + ".props"), sections.Count > 0 ? "[" + string.Join(", ", sections) + "]" : "");
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
      
            var frames = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (IsSmallFurni ? "32" : "64") + "']/animations/animation/animationLayer/frameSequence/frame");
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
                File.WriteAllText(Path.Combine(CAST_PATH, "asset.index"), "[#id: \"" + ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + "\", #classes: [\"Item Object Class\", \"Item Object Extension Class\", \"Window Class\"]]");
            }
            else
            {
                File.WriteAllText(Path.Combine(CAST_PATH, "asset.index"), "[#id: \"" + ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + "\", #classes: [\"Active Object Class\",  \"Active Object Extension Class\"]]");
            }
        }
    }
}
