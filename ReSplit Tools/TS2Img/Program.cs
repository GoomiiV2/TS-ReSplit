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
    class Program
    {
        // TODO: Either fix up or just focus on the rust version
        static void Main(string[] args)
        {
            ConvertToTS2Img(args[0], args[1]);
        }

        static bool IsTS2Img()
        {
            return true;
        }

        // This is crude
        static void ConvertToTS2Img(string InImg, string OutImg)
        {
            const uint TS2_TEXID = 1;

            var srcImg  = new Bitmap(Bitmap.FromFile(InImg));
            Bitmap pImg = srcImg.Clone(new Rectangle(0, 0, srcImg.Width, srcImg.Height), PixelFormat.Format8bppIndexed);

            using (var w = new BinaryWriter(new FileStream(OutImg, FileMode.Create)))
            {
                // Header
                w.Write(TS2_TEXID);
                w.Write(1);
                w.Write(srcImg.Width);
                w.Write(srcImg.Height);

                // Palette
                for (int i = 0; i < 256; i++)
                {
                    if (i < pImg.Palette.Entries.Length)
                    {
                        w.Write((byte)Math.Min((byte)pImg.Palette.Entries[i].R, (byte)210));
                        w.Write((byte)Math.Min((byte)pImg.Palette.Entries[i].G, (byte)210));
                        w.Write((byte)Math.Min((byte)pImg.Palette.Entries[i].B, (byte)210));
                        w.Write((byte)0x7f);
                    }
                    else // pad with zeros
                    {
                        w.Write((byte)0);
                        w.Write((byte)0);
                        w.Write((byte)0);
                        w.Write((byte)0x7f);
                    }
                }

                // Indices
                var imgData = pImg.LockBits(new Rectangle(0, 0, srcImg.Width, srcImg.Height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
                for (int y = 0; y < srcImg.Height; y++)
                {
                    for (int x = 0; x < srcImg.Width; x++)
                    {
                        var idx = GetPixel(imgData, x, y);
                        w.Write(idx);
                    }
                }
                pImg.UnlockBits(imgData);
            }  
        }

        public static unsafe Byte GetPixel(BitmapData Img, int x, int y)
        {
            byte* p = (byte*)Img.Scan0.ToPointer();
            int offset = y * Img.Stride + x;
            return p[offset];
        }
    }
}
