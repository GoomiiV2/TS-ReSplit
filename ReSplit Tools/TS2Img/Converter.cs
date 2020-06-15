using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS2Img
{
    public class Converter
    {
        // Convert a ts2 raw image from the ps2 version to a png
        public static void TS2ImgToPNG(string InFile, string OutDir)
        {
            var imgInBytes = File.ReadAllBytes(InFile);
            var imgIn      = new TS2.Texture(imgInBytes);
            var imgOut     = new Bitmap(imgIn.Width, imgIn.Height);

            int idx = 0;
            for (int y = 0; y < imgIn.Height; y++)
            {
                for (int x = 0; x < imgIn.Width; x++)
                {
                    var paletteColor = imgIn.Palettle[imgIn.Pixels[idx++]];
                    byte[] rgba      = BitConverter.GetBytes(paletteColor);
                    var color        = Color.FromArgb(rgba[3], rgba[0], rgba[1], rgba[2]);
                    imgOut.SetPixel(x, y, color); // yes locking bits would be faster but this should be fine for now, and the images are small
                }
            }

            var outPath = Path.Combine(OutDir, $"{Path.GetFileNameWithoutExtension(InFile)}.png");
            imgOut.Save(outPath, ImageFormat.Png);
        }
    }
}
