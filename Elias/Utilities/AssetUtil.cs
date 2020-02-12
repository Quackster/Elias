
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elias.Utilities
{
    class AssetUtil
    {
        public static string ConvertFlashName(string name, int x, int y)
        {
            var fileName = name;
            fileName = fileName.Replace("_64_", "_");
            fileName = fileName.Replace("_32_", "_");

            string[] data = fileName.Split('_');
            string spriteName = data[0];
            string layerLetter = data[1];
            string rotate = data[2];
            string frame = data[3];

            return spriteName + "_" + layerLetter + "_0_" + x + "_" + y + "_" + rotate + "_" + frame;
        }
    }
}
