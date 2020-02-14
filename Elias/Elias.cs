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

        public Elias(string sprite, bool IsSmallFurni, string fileName, int X, int Y, string FFDEC_PATH, string OUTPUT_PATH, string DIRECTOR_PATH)
        {
            this.Sprite = sprite;
            this.IsSmallFurni = IsSmallFurni;
            this.FullFileName = fileName;
            this.X = X;
            this.Y = Y;
            this.FileDirectory = new FileInfo(this.FullFileName).DirectoryName;
            this.FFDEC_PATH = FFDEC_PATH;
            this.OUTPUT_PATH = OUTPUT_PATH;
            this.DIRECTOR_PATH = DIRECTOR_PATH;
            this.Assets = new List<EliasAsset>();
            this.Symbols = new Dictionary<int, List<string>>();
        }

        public void Parse()
        {
            this.TryCleanup();
            this.ExtractAssets();
            this.RunSwfmill();
            this.GenerateAliases();
            this.CreateMemberalias();
            this.GenerateProps();
            this.GenerateAssetIndex();
            this.GenerateAnimations();
            this.RunEliasDirector();
        }

        private void TryCleanup()
        {
            try
            {
                if (Directory.Exists(this.OUTPUT_PATH))
                    Directory.Delete(this.OUTPUT_PATH, true);

                Directory.CreateDirectory(this.OUTPUT_PATH);

                if (Directory.Exists(this.CAST_PATH))
                    Directory.Delete(this.CAST_PATH, true);

                Directory.CreateDirectory(this.CAST_PATH);

                if (Directory.Exists(this.IMAGE_PATH))
                    Directory.Delete(this.IMAGE_PATH, true);

                Directory.CreateDirectory(this.IMAGE_PATH);
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
            p.StartInfo.FileName = "java";
            p.StartInfo.Arguments = string.Format("-jar \"" + FFDEC_PATH + "\" -export \"binaryData,image\" \"{0}\" \"{1}\"", OUTPUT_PATH, this.FullFileName);
            p.Start();
            p.WaitForExit();
        }

        private void RunSwfmill()
        {
            var p = new Process();
            p.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "swfmill\\swfmill.exe");
            p.StartInfo.Arguments = "swf2xml \"" + this.FullFileName + "\" \"" + Path.Combine(OUTPUT_PATH, Sprite + ".xml") + "\"";
            Console.WriteLine(p.StartInfo.Arguments);
            p.Start();
            p.WaitForExit();

            ReadSymbolClass();
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

        private void RunEliasDirector()
        {
            try
            {
                //Directory.Delete(Path.Combine(OUTPUT_PATH, "images"), true);
                //Directory.Delete(Path.Combine(OUTPUT_PATH, "binaryData"), true);
            }
            catch { }

            var p = new Process();
            p.StartInfo.WorkingDirectory = new FileInfo(DIRECTOR_PATH).DirectoryName;
            p.StartInfo.FileName = DIRECTOR_PATH;
            p.Start();
            p.WaitForExit();
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

                if (eliasAlias.ShockwaveAssetName == null)
                {
                    continue;
                }

                Assets.Add(eliasAlias);
            }

            foreach (var eliasAlias in Assets)
            {
                eliasAlias.WriteAssets();
                eliasAlias.WriteImageNames();
                eliasAlias.WriteRegPointData();
            }
        }

        private void CreateMemberalias()
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var eliasAsset in Assets)
            {
                if (eliasAsset.IsShadow)
                    continue;

                if (eliasAsset.IsIcon)
                    continue;

                if (eliasAsset.ShockwaveSourceAliasName != null)
                {
                    stringBuilder.Append(eliasAsset.ShockwaveAssetName);
                    stringBuilder.Append("=");
                    stringBuilder.Append(eliasAsset.ShockwaveSourceAliasName);

                    if (eliasAsset.IsFlipped)
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

            List<string> sections = new List<string>();

            if (xmlData == null)
            {
                return;
            }

            var visualisation = xmlData.SelectSingleNode("//visualizationData/graphics/visualization/layers");

            if (visualisation == null)
            {
                visualisation = xmlData.SelectSingleNode("//visualizationData/visualization/layers");
            }

            for (int i = 0; i < visualisation.ChildNodes.Count; i++)
            {
                var node = visualisation.ChildNodes.Item(i);

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
                    secondSection += "#zshift: [" + node.Attributes.GetNamedItem("z").InnerText + "], ";
                }

                if (node.Attributes.GetNamedItem("alpha") != null)
                {
                    double alphaValue = double.Parse(node.Attributes.GetNamedItem("alpha").InnerText);
                    double newValue = (double)((alphaValue / 255) * 100);
                    secondSection += "#blend: " + (int)newValue + ", ";
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

            File.WriteAllText(Path.Combine(CAST_PATH, ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + ".props"), sections.Count > 0 ? "[" + string.Join(", ", sections) + "]" : "");
        }

        private void GenerateAnimations()
        {
            char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToLower().ToCharArray();
            var xmlData = BinaryDataUtil.SolveFile(this.OUTPUT_PATH, "visualization");

            var animations = new List<int>();
            var sections = new Dictionary<string, EliasAnimation>();

            if (xmlData == null)
            {
                return;
            }
      
            var frames = xmlData.SelectNodes("//visualizationData/visualization[@size='" + (IsSmallFurni ? "32" : "64") + "']/animations/animation/animationLayer/frameSequence/frame");

            for (int i = 0; i < frames.Count; i++)
            {
                var frame = frames.Item(i);

                var animationLayer = frame.ParentNode.ParentNode;
                var animationLetter = Convert.ToString(alphabet[int.Parse(animationLayer.Attributes.GetNamedItem("id").InnerText)]);

                var animation = frame.ParentNode.ParentNode.ParentNode;
                var animationId = int.Parse(animation.Attributes.GetNamedItem("id").InnerText);

                if (!animations.Contains(animationId))
                {
                    animations.Add(animationId);
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

            var states = "";

            foreach (int id in animations)
                states += (id + 1) + ",";

            StringBuilder stringBuilder = new StringBuilder();

            if (animations.Count > 0)
            {
                stringBuilder.Append("[\r");
                stringBuilder.Append("states:[" + states.TrimEnd(",".ToCharArray()) + "],\r");
                stringBuilder.Append("layers:[\r");

                int e = 0;
                foreach (var animation in sections)
                {
                    if (animation.Value.States.Count == 0)
                    {
                        continue;
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
            File.WriteAllText(Path.Combine(CAST_PATH, "asset.index"), 
                "[#id: \"" + ((this.IsSmallFurni ? "s_" : "") + this.Sprite) + "\", #classes: [\"Active Object Class\",  \"Active Object Extension Class\"]]");
        }


    }
}
