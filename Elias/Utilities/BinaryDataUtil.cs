using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Elias.Utilities
{
    public class BinaryDataUtil
    {
        public static XmlDocument SolveFile(string outputDirectory, string fileNameContains)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(outputDirectory, "binaryData"), "*"))
            {
                if (Path.GetFileNameWithoutExtension(file).Contains(fileNameContains))
                {
                    var text = File.ReadAllText(file);

                    if (text.Contains("\n<?xml"))
                    {
                        text = text.Replace("\n<?xml", "<?xml");
                        File.WriteAllText(file, text);
                    }

                    if (text.Contains("<graphics>"))
                    {
                        text = text.Replace("<graphics>", "");
                        text = text.Replace("</graphics>", "");
                        File.WriteAllText(file, text);
                    }

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(file);

                    return xmlDoc;
                }
            }

            return null;
        }

        public static void ReplaceFiles(string directory, string search, string newWord)
        {
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file) != ".data" && Path.GetExtension(file) != ".props")
                {
                    ReplaceWord(file, search, newWord);
                }
            }
        }

        public static void ReplaceData(string directory, string search, string newWord)
        {
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file) == ".data")
                {
                    ReplaceWord(file, search, newWord);
                }
            }
        }

        public static void ReplaceProps(string directory, string search, string newWord)
        {
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file) == ".props")
                {
                    ReplaceWord(file, search, newWord);
                }
            }
        }

        public static void ReplaceWord(string file, string find, string replaceWith)
        {
            string fileName = Path.GetFileName(file);
            string newFileName = fileName.Replace(find, replaceWith);
            string newFilePath = file.Replace(fileName, newFileName);

            File.Move(file, newFilePath);

            if (Path.GetExtension(file) == ".bin" || Path.GetExtension(file) == ".txt" || Path.GetExtension(file) == ".xml" || Path.GetExtension(file) == ".props" || Path.GetExtension(file) == ".data" || Path.GetExtension(file) == ".index")
            {
                string content = File.ReadAllText(newFilePath);
                content = content.Replace(find, replaceWith);
                File.WriteAllText(newFilePath, content);
            }
        }

        internal static void DownscaleImages(string directory)
        {
            foreach (string file in Directory.GetFiles(directory, "*.png", SearchOption.AllDirectories))
                DownscaleImage(file);
        }

        private static void DownscaleImage(string path)
        {
            if (path.Contains("_icon_"))
                return;

            string tempPath = Path.GetFileName(path);

            using (Bitmap image = new Bitmap(path))
            {
                int newWidth = image.Width > 1 ? image.Width / 2 : 1;
                int newHeight = image.Height > 1 ? image.Height / 2 : 1;

                Bitmap newImage = new Bitmap(newWidth, newHeight);
                using (Graphics gr = Graphics.FromImage(newImage))
                {
                    //gr.SmoothingMode = SmoothingMode.HighQuality;
                    //gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    //gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.Default;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight));
                }


                newImage.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            if (File.Exists(path))
                File.Delete(path);

            File.Move(tempPath, path);
        }
    }
}
