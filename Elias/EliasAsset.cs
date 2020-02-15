using Elias.Utilities;
using System;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace EliasLibrary
{
    public class EliasAsset
    {
        private Elias Elias;
        private XmlNode Node;

        public bool IsIcon;
        public bool IsShadow;
        public bool IsMemberAlias;

        public string FlashAssetName;
        public string ShockwaveAssetName;

        public string FlashSourceAliasName;
        public string ShockwaveSourceAliasName;

        public int[] FlashRectanglePoint;
        public int[] ShockwaveRectanglePoint;

        public EliasAsset(Elias elias, XmlNode node)
        {
            this.Elias = elias;
            this.Node = node;
            this.FlashRectanglePoint = new int[2];
            this.ShockwaveRectanglePoint = new int[2];

            for (int i = 0; i < node.Attributes.Count; i++)
            {
                var attribute = node.Attributes.Item(i);

                if (attribute == null)
                {
                    continue;
                }

                if (attribute.InnerText.Contains("_icon_"))
                {
                    IsIcon = true;
                    break;
                }

                if (attribute.InnerText.Contains("_sd_"))
                {
                    IsShadow = true;
                    break;
                }
            }

            ParseAssetNames();
            ParseRecPointNames();
        }

        public void TryIcon()
        {
            if (!this.IsIcon)
                return;

            var iconName = this.Elias.Sprite + "_small";
            var icon = ImageAssetUtil.SolveFile(Elias.OUTPUT_PATH, "_icon_", false);

            if (icon != null)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Writing icon: ");
                Console.ResetColor();
                Console.WriteLine(iconName);

                File.WriteAllText(Path.Combine(Elias.IMAGE_PATH, iconName + ".txt"), ShockwaveRectanglePoint[0] + "," + ShockwaveRectanglePoint[1]);

                var iconDestination = Path.Combine(Elias.IMAGE_PATH, iconName + ".png");

                if (File.Exists(iconDestination))
                    File.Delete(iconDestination);

                File.Copy(icon, iconDestination);
            }
        }

        public void ParseAssetNames()
        {
            if (IsIcon)
                return;

            for (int i = 0; i < Node.Attributes.Count; i++)
            {
                var attribute = Node.Attributes.Item(i);

                if (attribute == null)
                {
                    continue;
                }

                if (attribute.Name == "name")
                {
                    FlashAssetName = attribute.InnerText;
                    ShockwaveAssetName = AssetUtil.ConvertName(Elias, attribute.InnerText, IsShadow);// IsShadow ? AssetUtil.ConvertFlashShadow(Elias, attribute.InnerText) : AssetUtil.ConvertFlashName(Elias, attribute.InnerText, Elias.X, Elias.Y);
                }

                if (attribute.Name == "source")
                {
                    IsMemberAlias = true;
                    FlashSourceAliasName = attribute.InnerText;
                    ShockwaveSourceAliasName = AssetUtil.ConvertName(Elias, attribute.InnerText, IsShadow);// IsShadow ? AssetUtil.ConvertFlashShadow(Elias, attribute.InnerText) : AssetUtil.ConvertFlashName(Elias, attribute.InnerText, Elias.X, Elias.Y);
                }
            }
        }

        public void ParseRecPointNames()
        {
            int x = -1;
            int y = -1;

            for (int i = 0; i < Node.Attributes.Count; i++)
            {
                var attribute = Node.Attributes.Item(i);

                if (attribute == null)
                {
                    continue;
                }

                if (attribute.Name == "x")
                    x = int.Parse(attribute.InnerText);

                if (attribute.Name == "y")
                    y = int.Parse(attribute.InnerText);
            }

            if (x == -1 || y == -1)
            {
                FlashRectanglePoint = new int[] { 0, 0 };
                ShockwaveRectanglePoint = new int[] { 0, 0 };
                return;
            }

            FlashRectanglePoint = new int[] { x, y };

            if (Elias.IsSmallFurni)
            {
                ShockwaveRectanglePoint = new int[] { x - 16, y };
            }
            else
            {
                ShockwaveRectanglePoint = new int[] { x - 32, y };
            }
        }

        public void WriteAssets()
        {
            if (IsIcon)
                return;

            if (IsShadow)
                return;

            for (int i = 0; i < Node.Attributes.Count; i++)
            {
                var attribute = Node.Attributes.Item(i);

                if (attribute == null)
                {
                    continue;
                }

                if (attribute.Name == "name")
                {
                    FlashAssetName = attribute.InnerText;
                    ShockwaveAssetName = AssetUtil.ConvertName(Elias, attribute.InnerText, IsShadow);

                    var flashFile = ImageAssetUtil.SolveFile(Elias.OUTPUT_PATH, FlashAssetName);

                    if (flashFile == null)
                    {
                        var symbolData = ImageAssetUtil.SolveSymbolReference(Elias, FlashAssetName);

                        if (symbolData != null)
                        {
                            var symbolID = symbolData.Item1;
                            var symbolReference = symbolData.Item2;
                            var symbolFileName = Path.GetFileNameWithoutExtension(Path.GetFileName(symbolReference).Replace(symbolID + "_" + Elias.Sprite + "_", ""));

                            var symbolAsset = Elias.Assets.FirstOrDefault(asset => asset.FlashAssetName == symbolFileName && FlashRectanglePoint[0] == asset.FlashRectanglePoint[0]
                                && FlashRectanglePoint[1] == asset.FlashRectanglePoint[1]);

                           if (symbolAsset != null)
                            {
                                Console.WriteLine("Cloned: " + symbolFileName + " => " + FlashAssetName);

                                IsMemberAlias = true;
                                FlashSourceAliasName = symbolFileName;
                                ShockwaveSourceAliasName = AssetUtil.ConvertName(Elias, FlashSourceAliasName, IsShadow);
                            }
                            else
                            {
                                // Copy it over because different regpoints
                                Console.WriteLine("Copied: " + symbolFileName + " => " + FlashAssetName);
                                File.Copy(symbolReference, Path.Combine(Elias.OUTPUT_PATH, "images", FlashAssetName + ".png"));
                            }
                        }
                        else
                        {
                            if (!IsMemberAlias)
                            {
                                Console.WriteLine("Create blank sprite for: " + FlashAssetName);

                                Bitmap bmp = new Bitmap(1, 1);
                                bmp.Save(Path.Combine(Elias.OUTPUT_PATH, "images", FlashAssetName + ".png"), ImageFormat.Png);
                            }
                        }
                    }
                }
            }
        }

        public void WriteFlippedAssets()
        {
            if (IsIcon)
                return;

            if (IsShadow)
                return;

            if (FlashSourceAliasName != null)
            {
                var flashFile = ImageAssetUtil.SolveFile(Elias.OUTPUT_PATH, FlashSourceAliasName);

                if (flashFile == null)
                {
                    var symbolData = ImageAssetUtil.SolveSymbolReference(Elias, FlashSourceAliasName);

                    if (symbolData != null)
                    {
                        var symbolID = symbolData.Item1;
                        var symbolReference = symbolData.Item2;
                        var symbolFileName = Path.GetFileNameWithoutExtension(Path.GetFileName(symbolReference).Replace(symbolID + "_" + Elias.Sprite + "_", ""));

                        var symbolAsset = Elias.Assets.FirstOrDefault(asset => asset.FlashSourceAliasName == symbolFileName && FlashRectanglePoint[0] == asset.FlashRectanglePoint[0]
                            && FlashRectanglePoint[1] == asset.FlashRectanglePoint[1]);

                        if (symbolAsset != null)
                        {
                            Console.WriteLine("FP Cloned: " + symbolFileName + " => " + FlashSourceAliasName);

                            IsMemberAlias = true;
                            FlashSourceAliasName = symbolFileName;
                            ShockwaveSourceAliasName = AssetUtil.ConvertName(Elias, FlashSourceAliasName, IsShadow);
                        }
                        else
                        {
                            // Copy it over because different regpoints
                            Console.WriteLine("FP Copied: " + symbolFileName + " => " + FlashSourceAliasName);
                            File.Copy(symbolReference, Path.Combine(Elias.OUTPUT_PATH, "images", FlashSourceAliasName + ".png"));
                        }
                    }
                    else
                    {
                        if (!IsMemberAlias)
                        {
                            Console.WriteLine("Create blank sprite for: " + FlashSourceAliasName);

                            Bitmap bmp = new Bitmap(1, 1);
                            bmp.Save(Path.Combine(Elias.OUTPUT_PATH, "images", FlashSourceAliasName + ".png"), ImageFormat.Png);
                        }
                    }
                }
            }
        }

        public void WriteImageNames()
        {
            if (IsIcon)
                return;

            if (IsShadow)
                return;

            if (!string.IsNullOrEmpty(FlashSourceAliasName))
                return;

            var sourceImage = ImageAssetUtil.SolveFile(Elias.OUTPUT_PATH, FlashAssetName);
            
            File.Copy(sourceImage, Path.Combine(Elias.IMAGE_PATH, ShockwaveAssetName + ".png"));
        }

        public void WriteRegPointData()
        {
            if (IsIcon)
                return;

            if (IsShadow)
                return;


            if (!string.IsNullOrEmpty(FlashSourceAliasName))
                return;

            File.WriteAllText(Path.Combine(Elias.IMAGE_PATH, ShockwaveAssetName + ".txt"), ShockwaveRectanglePoint[0] + "," + ShockwaveRectanglePoint[1]);
        }

        public void TryShadow()
        {
            if (!IsShadow)
                return;

            var flashFile = ImageAssetUtil.SolveFile(Elias.OUTPUT_PATH, FlashAssetName);

            if (flashFile != null)
            {
                this.ShockwaveAssetName = AssetUtil.ConvertFlashShadow(this.Elias, this.FlashAssetName);

                File.Copy(flashFile, Path.Combine(Elias.IMAGE_PATH, ShockwaveAssetName + ".png"));
                File.WriteAllText(Path.Combine(Elias.IMAGE_PATH, ShockwaveAssetName + ".txt"), ShockwaveRectanglePoint[0] + "," + ShockwaveRectanglePoint[1]);
            }
        }

        public bool IsInverted()
        {
            return Node.Attributes.GetNamedItem("flipH") != null && Node.Attributes.GetNamedItem("flipH").InnerText == "1";
        }
    }
}