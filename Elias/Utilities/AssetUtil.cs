using System;
using System.Drawing;
using System.IO;
using EliasLibrary;

namespace Elias.Utilities
{
    class AssetUtil
    {
        public static void FlipAllMembers(EliasLibrary.Elias elias)
        {
            BinaryDataUtil.ReplaceFiles(elias.CAST_PATH, "leftwall", "testwall");
            BinaryDataUtil.ReplaceFiles(elias.CAST_PATH, "rightwall", "leftwall");
            BinaryDataUtil.ReplaceFiles(elias.CAST_PATH, "testwall", "rightwall");

            foreach (string file in Directory.GetFiles(elias.CAST_PATH, "*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file) != ".txt" || !Path.GetFileName(file).Contains("leftwall"))
                {
                    continue;
                }

                string picture = file.Replace(".txt", ".png");

                if (File.Exists(picture))
                {                    
                    string[] existingCoordinateData = File.ReadAllText(file).Split(',');
                    int X = int.Parse(existingCoordinateData[0]);

                    var bitmap1 = (Bitmap)Bitmap.FromFile(picture);
                    bitmap1.RotateFlip(RotateFlipType.Rotate180FlipY);
                    bitmap1.Save(picture);
                    X = bitmap1.Width - X;
                    bitmap1.Dispose();

                    File.WriteAllText(file, X + "," + existingCoordinateData[1]);
                }
            }
        }

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

        public static string ConvertName(EliasLibrary.Elias elias, EliasAsset asset, string name)
        {
            try
            {
                if (elias.IsWallItem)
                {
                    return ConvertFlashWallName(elias, name);
                }

                if (asset.IsShadow)
                {
                    return ConvertFlashShadow(elias, name);
                }

                if (asset.IsIcon)
                {
                    return elias.Sprite + "_small";
                }

                return ConvertFlashName(elias, name);
            }
            catch
            {
                return null;
            }
        }

        public static void DeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        internal static void CrudeReplace(string CAST_PATH)
        {
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_a_", "_bb_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_b_", "_cc_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_c_", "_dd_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_d_", "_ee_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_e_", "_ff_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_f_", "_gg_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_g_", "_hh_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_h_", "_ii_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_i_", "_jj_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_j_", "_kk_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_k_", "_ll_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_l_", "_mm_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_m_", "_nn_");

            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_bb_", "_b_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_cc_", "_c_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_dd_", "_d_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_ee_", "_e_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_ff_", "_f_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_gg_", "_g_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_hh_", "_h_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_ii_", "_i_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_jj_", "_j_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_kk_", "_k_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_ll_", "_l_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_mm_", "_m_");
            BinaryDataUtil.ReplaceFiles(CAST_PATH, "_nn_", "_n_");

            BinaryDataUtil.ReplaceData(CAST_PATH, "a:", "111:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "b:", "222:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "c:", "333:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "d:", "444:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "e:", "555:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "f:", "666:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "g:", "777:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "h:", "888:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "i:", "999:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "j:", "001:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "k:", "002:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "l:", "003:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "m:", "004:");

            BinaryDataUtil.ReplaceData(CAST_PATH, "111:", "b:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "222:", "c:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "333:", "d:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "444:", "e:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "555:", "f:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "666:", "g:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "777:", "h:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "888:", "i:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "999:", "j:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "001:", "k:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "002:", "l:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "003:", "m:");
            BinaryDataUtil.ReplaceData(CAST_PATH, "004:", "n:");

            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"a\":", "111:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"b\":", "222:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"c\":", "333:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"d\":", "444:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"e\":", "555:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"f\":", "666:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"g\":", "777:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"h\":", "888:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"i\":", "999:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"j\":", "001:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"k\":", "002:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"l\":", "003:");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "\"m\":", "004:");

            BinaryDataUtil.ReplaceProps(CAST_PATH, "111:", "\"b\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "222:", "\"c\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "333:", "\"d\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "444:", "\"e\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "555:", "\"f\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "666:", "\"g\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "777:", "\"h\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "888:", "\"i\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "999:", "\"j\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "001:", "\"k\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "002:", "\"l\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "003:", "\"m\":");
            BinaryDataUtil.ReplaceProps(CAST_PATH, "004:", "\"n\":");
        }
    }
}
