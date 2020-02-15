using System;
using EliasLibrary;

namespace Elias.Utilities
{
    class AssetUtil
    {
        public static string ConvertFlashName(EliasLibrary.Elias elias, string name)
        {
            var fileName = name;
            fileName = fileName.Replace("_64_", "_");
            fileName = fileName.Replace("_32_", "_");
            fileName = fileName.Replace(elias.Sprite + "_", "");

            string[] data = fileName.Split('_');
            string layerLetter = data[0];
            string rotate = data[1];
            string frame = data[2];

            string newName = (elias.IsSmallFurni ? "s_" : "") + elias.Sprite + "_" + layerLetter + "_0_" + elias.X + "_" + elias.Y + "_" + rotate + "_" + frame;
            return newName;
        }

        public static string ConvertFlashShadow(EliasLibrary.Elias elias, string name)
        {
            var fileName = name;
            fileName = fileName.Replace("_64_", "_");
            fileName = fileName.Replace("_32_", "_");
            fileName = fileName.Replace(elias.Sprite + "_", "");

            string[] data = fileName.Split('_');
            string layerLetter = data[0];
            string rotate = data[1];

            string newName = (elias.IsSmallFurni ? "s_" : "") + elias.Sprite + "_" + layerLetter + "_" + rotate;
            return newName;
        }

        public static string ConvertFlashWallName(EliasLibrary.Elias elias, string name)
        {
            var fileName = name;
            fileName = fileName.Replace("_64_", "_");
            fileName = fileName.Replace("_32_", "_");
            fileName = fileName.Replace(elias.Sprite + "_", "");

            string[] data = fileName.Split('_');
            string newName = null;

            if (data[1] == "mask")
            {
                newName += (elias.IsSmallFurni ? "s_" : "");
                newName += data[0] == "2" ? "leftwall" : "rightwall";
                newName += " " + elias.Sprite;
                newName += "_";
                newName += data[1];
                return newName;
            }

            newName += (elias.IsSmallFurni ? "s_" : "");
            newName += data[1] == "2" ? "leftwall" : "rightwall";
            newName += " " + elias.Sprite;
            newName += "_";
            newName += data[0];
            newName += "_";
            newName += data[2];

            return newName;
        }

        public static string ConvertName(EliasLibrary.Elias elias, string name, bool isShadow)
        {
            if (elias.IsWallItem)
            {
                return ConvertFlashWallName(elias, name);
            }

            if (isShadow)
            {
                return ConvertFlashShadow(elias, name);
            }

            return ConvertFlashName(elias, name);
        }
    }
}
