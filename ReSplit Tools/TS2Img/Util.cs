using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS2Img
{
    public class Util
    {
        public static void CreateDebugTexture(TS2.Texture Tex, string OutPath)
        {
            var font            = System.Drawing.SystemFonts.DefaultFont;
            var palTileSize     = 16;
            var palLabelSpacing = 5;
            var palLabelSize    = GetTextSize("[IDX] RRR, GGG, BBB, AAA", font);
            var idxLabelSize    = GetTextSize(" IDX ", font);
            var idxLabelPadding = 5;

            var paletteColums     = 2;
            var sideListItemWidth = (palLabelSpacing + palTileSize + palLabelSpacing + palLabelSize.Width + palLabelSpacing);
            var sideListWidth     = sideListItemWidth * paletteColums;
            var sideLabelHeight   = (palLabelSpacing *2 + (palLabelSpacing + Math.Max(palLabelSize.Height, palTileSize)) * Tex.Palettle.Length) / paletteColums;

            var idxGridTileWidth  = idxLabelSize.Width + (idxLabelPadding * 2);
            var idxGridTileHeight = idxLabelSize.Height + (idxLabelPadding * 2);
            var idxGridTileSize   = (int)Math.Max(idxGridTileWidth, idxGridTileHeight);

            var idxGridWidth  = Tex.Width * idxGridTileSize;
            var idxGridHeight = Tex.Height * idxGridTileSize;

            var width  = (int)(sideListWidth + idxGridWidth);
            var height = (int)Math.Max(sideLabelHeight, idxGridHeight);

            // Now make the debug image
            using (Image img = new Bitmap(width, height))
            {
                using (Graphics draw = Graphics.FromImage(img))
                {
                    Brush bgBrush  = new SolidBrush(Color.White);
                    Brush txtBrush = new SolidBrush(Color.Black);
                    draw.FillRectangle(bgBrush, new Rectangle(0, 0, width, height));

                    // Draw the side palette info
                    for (int i = 0; i < Tex.Palettle.Length; i++)
                    {
                        var colNum    = (int)Math.Floor((double)(i / (Tex.Palettle.Length / paletteColums)));
                        var tileWidth = (int)(palLabelSpacing + palTileSize + palLabelSpacing);
                        var x         = (int)(palLabelSpacing + (sideListItemWidth * colNum));
                        var h         = Math.Max(palLabelSize.Height, palTileSize);
                        var y         = (int)(palLabelSpacing + (palLabelSpacing + h) * (i - ((Tex.Palettle.Length / paletteColums) * colNum)));

                        //byte[] rgba = BitConverter.GetBytes(Tex.Palettle[i]);

                        var palColor = Tex.Palettle[i];
                        var rgba = GetRGBA(palColor);
                        var r = rgba[0];
                        var g = rgba[1];
                        var b = rgba[2];

                        var brush   = new SolidBrush(Color.FromArgb(255, r, g, b));

                        draw.FillRectangle(brush, new Rectangle(x, y, palTileSize, palTileSize));
                        draw.DrawString($"[{i:000}] {rgba[0]:000}, {rgba[1]:000}, {rgba[2]:000}, {rgba[3]:000}", font, txtBrush, new Point(x + tileWidth, y));
                    }

                    // Draw the scaled upk texture with the pixel indeies marked
                    var pixelIdx = 0;
                    for (int y = 0; y < Tex.Height; y++)
                    {
                        for (int x = 0; x < Tex.Width; x++)
                        {
                            var posX = sideListWidth + (x * idxGridTileSize);
                            var posY = y * idxGridTileSize;

                            var palIdx     = Tex.Pixels[pixelIdx++];
                            var palColor   = Tex.Palettle[palIdx];
                            //byte[] rgba    = BitConverter.GetBytes(palColor);

                            /*var r = rgba[0];
                            var g = rgba[1];
                            var b = rgba[2];*/

                            var rgba = GetRGBA(palColor);
                            var r = rgba[0];
                            var g = rgba[1];
                            var b = rgba[2];

                            /*if (palIdx >= 16 && palIdx <= 18)
                            {
                                //r = (byte)(r + 91);
                                b = (byte)(b + 10);
                                //g = (byte)(g + 91);
                            }*/

                            var pixelBrush = new SolidBrush(Color.FromArgb(255, r, g, b));

                            draw.FillRectangle(pixelBrush, new RectangleF(posX, posY, idxGridTileSize, idxGridTileSize));
                            draw.DrawString($" {palIdx} ", font, txtBrush, new PointF(posX + idxLabelPadding, posY + idxLabelPadding));
                        }
                    }
                }

                img.Save(OutPath);
            }
        }

        public static byte[] GetRGBA(uint Color)
        {
            //Color = Color << 1;
            var r = (byte)((Color /*| 0x80*/) >> 0);
            var g = (byte)((Color /*| 0x8000*/) >> 8);
            var b = (byte)((Color /*| 0x800000*/) >> 16);

            return new byte[] { r, g, b, 255 };
        }

        public static SizeF GetTextSize(string Text, Font Font)
        {
            using (Image img = new Bitmap(1, 1))
            {
                using (Graphics drawing = Graphics.FromImage(img))
                {
                    SizeF textSize = drawing.MeasureString(Text, Font);
                    return textSize;
                }
            }
        }
    }
}
