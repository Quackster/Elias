using System;
using EliasLibrary;

namespace Elias.Utilities
{
    class AssetUtil
    {
        public static string ConvertFlashName(EliasLibrary.Elias elias, string name, int x, int y)
        {
            var fileName = name;
            fileName = fileName.Replace("_64_", "_");
            fileName = fileName.Replace("_32_", "_");
            fileName = fileName.Replace(elias.Sprite + "_", "");

            string[] data = fileName.Split('_');
            string layerLetter = data[0];
            string rotate = data[1];
            string frame = data[2];

            string newName = (elias.IsSmallFurni ? "s_" : "") + elias.Sprite + "_" + layerLetter + "_0_" + x + "_" + y + "_" + rotate + "_" + frame;
            return newName;
        }

        

        public static string NextRotation(string name, int rotation)
        {
            var fileName = name;
            string[] data = fileName.Split('_');

            return fileName.Replace(data[data.Length - 2] + "_" + data[data.Length - 1], rotation + "_" + data[data.Length - 1]);
        }

        public static string ConvertFlashFileName(EliasLibrary.Elias elias, string flashSourceAliasName, int x, int y)
        {
            int index = flashSourceAliasName.IndexOf('_');
            return ConvertFlashName(elias, flashSourceAliasName.Substring(index + 1), x, y);
        }
    }
}
