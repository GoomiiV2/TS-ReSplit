using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.TSFramework
{
    public static class TSTextureUtils
    {
        public static Texture2D TS2TexToT2D(TS2.Texture Tex)
        {
            var t2d = new Texture2D(Tex.Width, Tex.Height);

            // Prob look into a more efficent way to do this for Unity
            for (int y = 0; y < Tex.Height; y++)
            {
                for (int x = 0; x < Tex.Width; x++)
                {
                    var idx      = Tex.Pixels[Tex.Width * y + x];
                    var palColor = Tex.Palettle[idx];
                    byte[] rgba  = BitConverter.GetBytes(palColor);
                    var color    = new Color32(rgba[0], rgba[1], rgba[2], rgba[3]);
                    t2d.SetPixel(x, y, color);
                }
            }

            t2d.Apply(true, false);
            return t2d;
        }

        public static string GetTexturePathFromID(uint ID)
        {
            string texPath = $"textures/{ID:0000}.raw";
            return texPath;
        }

        public static string[] GetTexturePathsForMats(TS2.MatInfo[] MatInfos)
        {
            var matPaths = new string[MatInfos.Length];
            for (int i = 0; i < MatInfos.Length; i++)
            {
                var matInfo = MatInfos[i];
                matPaths[i] = GetTexturePathFromID(matInfo.ID);
            }

            return matPaths;
        }
    }
}
