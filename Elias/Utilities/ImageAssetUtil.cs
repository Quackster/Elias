
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elias.Utilities
{
    class ImageAssetUtil
    {
        public static string SolveFile(string outputDirectory, string fileNameContains)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(outputDirectory, "images"), "*"))
            {
                if (Path.GetFileNameWithoutExtension(file).EndsWith(fileNameContains))
                {
                    return file;
                }
            }

            return null;
        }
    }
}
