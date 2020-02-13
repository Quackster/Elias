﻿namespace Elias.Utilities
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

            return (elias.IsSmallFurni ? "s_" : "") + elias.Sprite + "_" + layerLetter + "_0_" + x + "_" + y + "_" + rotate + "_" + frame;
        }
    }
}
