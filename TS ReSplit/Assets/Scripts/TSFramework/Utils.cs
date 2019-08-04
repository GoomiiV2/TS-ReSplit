using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.TSFramework
{
    public static class Utils
    {
        public static Vector3 V3FromFloats(float[] Floats)
        {
            var vector = new Vector3(Floats[0], Floats[1], Floats[2]);
            return vector;
        }

        // Scan the give reader for a series of 3 floats that are in the given range, returns a list of all triples that match
        // Walks the file 4 bytes at a time
        public static List<Tuple<int,float[]>> ScanForVector3(BinaryReader R, float FloatMin, float FloatMax, int StartOffset = -1, int EndOffset = -1)
        {
            const int NUM_FLOATS = 3;
            var vec3s            = new List<Tuple<int, float[]>>();

            if (StartOffset != -1) { R.BaseStream.Seek(StartOffset, SeekOrigin.Begin); }

            for (int aye = 0; aye < 100000; aye++)
            {
                var pos       = (int)R.BaseStream.Position;
                var bytesLeft = R.BaseStream.Length - pos;
                var vec3      = new float[NUM_FLOATS];
                var canRead   = EndOffset != -1 ? ((EndOffset - pos) > vec3.Length) : (bytesLeft > vec3.Length);

                if (!canRead) { break; }

                var bytes = R.ReadBytes(sizeof(float) * NUM_FLOATS);

                Buffer.BlockCopy(bytes, 0, vec3, 0, bytes.Length);

                bool allInRange = true;
                for (int i = 0; i < NUM_FLOATS; i++)
                {
                    var f = vec3[i];
                    var asString = $"{f}";
                    if (f < FloatMin || f > FloatMax || asString.ToUpper().Contains("E"))
                    {
                        allInRange = false;
                    }
                }

                if (allInRange)
                {
                    vec3s.Add(Tuple.Create(pos, vec3));
                }
                else
                {
                    R.BaseStream.Seek(-8, SeekOrigin.Current);
                }
            }


            return vec3s;
        }

        public static Vector3 RadRotationToDeg(Vector3 Rot)
        {
            var newRotation = new Vector3()
            {
                x = Rot.x * Mathf.Rad2Deg,
                y = Rot.y * Mathf.Rad2Deg,
                z = Rot.z * Mathf.Rad2Deg
            };

            return newRotation;
        }

        public static Vector3 GetCenterOfPoints(List<Vector3> Positions)
        {
            var averagePos = new Vector3();

            foreach (var pos in Positions)
            {
                averagePos += pos;
            }

            averagePos = averagePos / Positions.Count;
            return averagePos;
        }

        public static Bounds CalcNavMeshBounds()
        {
            var navVerts = UnityEngine.AI.NavMesh.CalculateTriangulation().vertices;
            var bounds = new Bounds();
            for (int i = 0; i < navVerts.Length; i++)
            {
                var vert = navVerts[i];
                bounds.Encapsulate(vert);
            }

            return bounds;
        }

        public static Vector3 RandPointInBounds(Bounds bounds)
        {
            var vec = new Vector3()
            {
                x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                y = UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                z = UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
            };

            return vec;
        }

        // A simple display of the ReSplit name in the bottom right
        public static void ProjectLogo()
        {
            var width  = 60;
            var height = 40;
            var x      = Screen.width - width;
            var y      = Screen.height - height;
            var rect   = new Rect(Screen.width - width, Screen.height - height, width, height);

            var defaultFontSize     = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = 22;
            GUI.contentColor        = new Color32(0xDE, 0x31, 0x63, 0xFF);
            GUI.Label(new Rect(x - 30, y, width, height), "Re");
            GUI.contentColor = Color.white;
            GUI.Label(new Rect(x, y, width, height), "Split");

            GUI.skin.label.fontSize = defaultFontSize;
        }
    }
}
