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
        public Elias Elias;
        public XmlNode Node;

        public bool IsIcon;
        public bool IsShadow;
        public bool IsMemberAlias;
        public bool FlipX;
        public bool EmergencyFix;

        public string FlashAssetName;
        public string ShockwaveAssetName;

        public string FlashSourceAliasName;
        public string ShockwaveSourceAliasName;

        public int[] FlashRectanglePoint;
        public int[] ShockwaveRectanglePoint;

        public EliasAsset(Elias elias, XmlNode node, bool EmergencyFix)
        {
            this.Elias = elias;
            this.Node = node;
            this.EmergencyFix = EmergencyFix;
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

            if (!IsIcon && !IsShadow && AssetUtil.ConvertName(elias, this, Node.Attributes.GetNamedItem("name").InnerText) == null)
            {
                this.ShockwaveAssetName = null;
                this.IsShadow = false;
                this.IsIcon = false;
                return;
            }
        }

        public void Parse()
        {
            ParseAssetNames();
            ParseRecPointNames();
        }

        public void TryIcon()
        {
            if (!this.IsIcon)
                return;

            var iconName = this.Elias.Sprite + "_small";
            var icon = ImageAssetUtil.SolveFile(Elias.OUTPUT_PATH, "_icon_", false);

            if (icon == null)
                icon = ImageAssetUtil.SolveFile(Elias.OUTPUT_PATH, Node.Attributes.GetNamedItem("name").InnerText, false);

            if (icon == null)
            {
                var symbol = ImageAssetUtil.SolveSymbolReference(Elias, Node.Attributes.GetNamedItem("name").InnerText);

                if (symbol != null)
                    icon = symbol.Item2;
            }

            if (icon != null)
            {
                /* Console.ForegroundColor = ConsoleColor.Yellow;
                 Console.Write("Writing icon: ");
                 Console.ResetColor();
                 Console.WriteLine(iconName);
                 */
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
                    ShockwaveAssetName = AssetUtil.ConvertName(Elias, this, attribute.InnerText);// IsShadow ? AssetUtil.ConvertFlashShadow(Elias, attribute.InnerText) : AssetUtil.ConvertFlashName(Elias, attribute.InnerText, Elias.X, Elias.Y);
                }

                if (attribute.Name == "source")
                {
                    IsMemberAlias = true;
                    FlashSourceAliasName = attribute.InnerText;
                    ShockwaveSourceAliasName = AssetUtil.ConvertName(Elias, this, attribute.InnerText);// IsShadow ? AssetUtil.ConvertFlashShadow(Elias, attribute.InnerText) : AssetUtil.ConvertFlashName(Elias, attribute.InnerText, Elias.X, Elias.Y);
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

            if (FlipX)
            {
                var flashFile = ImageAssetUtil.SolveFile(Elias.OUTPUT_PATH, FlashAssetName);

                if (flashFile != null)
                {
                    var bitmap = Bitmap.FromFile(flashFile);
                    x = bitmap.Width - x;
                    bitmap.Dispose();
                }
            }

            FlashRectanglePoint = new int[] { x, y };

            if (Elias.IsWallItem)
            {
                ShockwaveRectanglePoint = new int[] { x, y };
            }
            else
            {
                if (Elias.IsSmallFurni)
                {
                    if (Elias.IsDownscaled)
                    {
                        x = x - 32;
                        ShockwaveRectanglePoint = new int[] { x / 2, y / 2 };
                        //ShockwaveRectanglePoint = new int[] { (int)Math.Round((double)x / 2), (int)Math.Round((double)y / 2) };
                        return;
                    }

                    ShockwaveRectanglePoint = new int[] { x - 16, y };
                }
                else
                {
                    ShockwaveRectanglePoint = new int[] { x - 32, y };
                }
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
                    ShockwaveAssetName = AssetUtil.ConvertName(Elias, this, attribute.InnerText);

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
                                IsMemberAlias = true;
                                FlashSourceAliasName = symbolFileName;
                                ShockwaveSourceAliasName = AssetUtil.ConvertName(Elias, this, FlashSourceAliasName);
                            }
                            else
                            {
                                // Copy it over because different regpoints
                                File.Copy(symbolReference, Path.Combine(Elias.OUTPUT_PATH, "images", FlashAssetName + ".png"));
                            }
                        }
                        else
                        {
                            if (!IsMemberAlias)
                            {
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
                            IsMemberAlias = true;
                            FlashSourceAliasName = symbolFileName;
                            ShockwaveSourceAliasName = AssetUtil.ConvertName(Elias, this, FlashSourceAliasName);
                        }
                        else
                        {
                            // Copy it over because different regpoints
                            File.Copy(symbolReference, Path.Combine(Elias.OUTPUT_PATH, "images", FlashSourceAliasName + ".png"));
                        }
                    }
                    else
                    {
                        if (!IsMemberAlias)
                        {
                            Bitmap bmp = new Bitmap(1, 1);
                            bmp.Save(Path.Combine(Elias.OUTPUT_PATH, "images", FlashSourceAliasName + ".png"), ImageFormat.Png);
                            bmp.Dispose();
                        }
                    }
                }
                else
                {
                    var integrity = Elias.Assets.FirstOrDefault(asset => asset.FlashAssetName == FlashSourceAliasName && asset.FlashSourceAliasName == null);

                    if (integrity != null && (EmergencyFix || Elias.IsWallItem || ((integrity.FlashRectanglePoint[0] != this.FlashRectanglePoint[0]) || (integrity.FlashRectanglePoint[1] != this.FlashRectanglePoint[1]))))
                    {

                        FlashSourceAliasName = null;
                        ShockwaveSourceAliasName = null;
                        IsMemberAlias = false;

                        var newPath = Path.Combine(Elias.OUTPUT_PATH, "images", FlashAssetName + ".png");

                        if (File.Exists(newPath))
                            File.Delete(newPath);

                        File.Copy(flashFile, newPath);

                        if (this.IsInverted())
                        {
                            this.FlipX = true;

                            var bitmap1 = (Bitmap)Bitmap.FromFile(newPath);
                            bitmap1.RotateFlip(RotateFlipType.Rotate180FlipY);
                            bitmap1.Save(newPath);
                            bitmap1.Dispose();
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
            var newPath = Path.Combine(Elias.IMAGE_PATH, ShockwaveAssetName + ".png");

            if (File.Exists(newPath))
                File.Delete(newPath);

            File.Copy(sourceImage, newPath);
        }

        public void WriteRegPointData()
        {
            if (IsIcon)
                return;

            if (IsShadow)
                return;


            if (!string.IsNullOrEmpty(FlashSourceAliasName))
                return;

            if (this.FlipX)
            {
                ParseRecPointNames();
            }

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

                // Fix for "already exists"
                if (File.Exists(Path.Combine(Elias.IMAGE_PATH, ShockwaveAssetName + ".png")))
                    File.Delete(Path.Combine(Elias.IMAGE_PATH, ShockwaveAssetName + ".png"));

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