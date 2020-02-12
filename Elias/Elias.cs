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

        public string CAST_PATH
        {
            get { return Path.Combine(OUTPUT_PATH, "cast_data"); }
        }

        public string IMAGE_PATH
        {
            get { return Path.Combine(CAST_PATH, "images"); }
        }

        private List<EliasAsset> Assets;

        public Elias(string sprite, bool IsSmallFurni, string fileName, int X, int Y, string FFDEC_PATH, string OUTPUT_PATH)
        {
            this.Sprite = sprite;
            this.IsSmallFurni = IsSmallFurni;
            this.FullFileName = fileName;
            this.X = X;
            this.Y = Y;
            this.FileDirectory = new FileInfo(this.FullFileName).DirectoryName;
            this.FFDEC_PATH = FFDEC_PATH;
            this.OUTPUT_PATH = OUTPUT_PATH;
            this.Assets = new List<EliasAsset>();
        }

        public void Parse()
        {
            this.TryCleanup();
            this.ExtractAssets();
            this.GenerateAliases();
            this.CreateMemberalias();

            File.WriteAllText(Path.Combine(OUTPUT_PATH, "sprite.name"), this.Sprite);
        }

        private void TryCleanup()
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

        private void ExtractAssets()
        {
            var p = new Process();
            p.StartInfo.FileName = "java";
            p.StartInfo.Arguments = string.Format("-jar \"" + FFDEC_PATH + "\" -export \"binaryData,image\" \"{0}\" \"{1}\"", OUTPUT_PATH, this.FullFileName);
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

                eliasAlias.ParseAssetNames();

                if (eliasAlias.ShockwaveAssetName == null)
                {
                    continue;
                }

                eliasAlias.WriteImageNames();
                eliasAlias.ParseRecPointNames();
                eliasAlias.WriteRegPointData();

                Assets.Add(eliasAlias);
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
                    SafetyCheckAsset(eliasAsset.ShockwaveSourceAliasName);

                    stringBuilder.Append(eliasAsset.ShockwaveAssetName);
                    stringBuilder.Append("=");
                    stringBuilder.Append(eliasAsset.ShockwaveSourceAliasName);
                    stringBuilder.Append("*");
                    stringBuilder.Append("\r");
                }
            }

            File.WriteAllText(Path.Combine(CAST_PATH, "memberalias.index"), stringBuilder.ToString());
        }

        /// <summary>
        /// Create blank PNG if not exists
        /// </summary>
        /// <param name="shockwaveSourceAliasName"></param>
        private void SafetyCheckAsset(string shockwaveSourceAliasName)
        {
            if (!File.Exists(Path.Combine(IMAGE_PATH, shockwaveSourceAliasName + ".png")))
            {
                Bitmap bmp = new Bitmap(1, 1);
                bmp.Save(shockwaveSourceAliasName + ".png", ImageFormat.Png);
                File.WriteAllText(Path.Combine(IMAGE_PATH, shockwaveSourceAliasName + ".txt"), "0,0");
            }
        }
    }
}
