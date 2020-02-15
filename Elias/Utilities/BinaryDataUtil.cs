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
    }
}
