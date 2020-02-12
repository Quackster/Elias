using Elias.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliasLibrary
{
    public class Elias
    {
        public bool smallCast;
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

        public Elias(bool smallCast, string fileName, int X, int Y, string FFDEC_PATH, string OUTPUT_PATH)
        {
            this.smallCast = smallCast;
            this.FullFileName = fileName;
            this.X = X;
            this.Y = Y;
            this.FileDirectory = new FileInfo(this.FullFileName).DirectoryName;
            this.FFDEC_PATH = FFDEC_PATH;
            this.OUTPUT_PATH = OUTPUT_PATH;
        }

        public void Parse()
        {
            this.TryCleanup();
            this.ExtractAssets();
            this.GenerateAliases();
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

                if (smallCast && node.OuterXml.Contains("_64_"))
                {
                    continue;
                }

                if (!smallCast && node.OuterXml.Contains("_32_"))
                {
                    continue;
                }

                var eliasAlias = new EliasAsset(this, node);

                eliasAlias.ParseAssetNames();
                eliasAlias.ParseRecPointNames();
                eliasAlias.WriteImageNames();
                eliasAlias.WriteRegPointData();
            }
        }
    }
}
