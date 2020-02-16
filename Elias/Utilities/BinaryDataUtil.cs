using System;
using System.Collections.Generic;
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
                ReplaceWord(file, search, newWord);
        }

        public static void ReplaceWord(string file, string find, string replaceWith)
        {
            string fileName = Path.GetFileName(file);
            string newFileName = fileName.Replace(find, replaceWith);
            string newFilePath = file.Replace(fileName, newFileName);

            File.Move(file, newFilePath);

            if (Path.GetExtension(file) == ".bin" || Path.GetExtension(file) == ".xml")
            {
                string content = File.ReadAllText(newFilePath);
                content = content.Replace(find, replaceWith);
                File.WriteAllText(newFilePath, content);
            }
        }
    }
}
