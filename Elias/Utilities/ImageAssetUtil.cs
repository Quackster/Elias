
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliasLibrary;

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

        public static Tuple<int, string> SolveSymbolReference(EliasLibrary.Elias elias, string flashAssetName)
        {
            foreach (var symbols in elias.Symbols)
            {
                foreach (var symbol in symbols.Value)
                {
                    if (symbol.EndsWith(flashAssetName))
                    {
                        return Tuple.Create<int, string>(symbols.Key, SolveSymbolImage(elias, symbols.Key));
                    }
                }
            }

            return null;
        }

        private static string SolveSymbolImage(EliasLibrary.Elias elias, int key)
        {
            foreach (var file in Directory.GetFiles(Path.Combine(elias.OUTPUT_PATH, "images"), "*"))
            {
                if (Path.GetFileNameWithoutExtension(file).StartsWith(key + "_"))
                {
                    return file;
                }
            }

            return null;
        }
    }
}
