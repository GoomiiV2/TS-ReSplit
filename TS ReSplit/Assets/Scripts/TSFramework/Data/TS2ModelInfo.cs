using System.Collections;
using System.Collections.Generic;
using static TS2AnimationData;

namespace TSData
{
    public class TS2ModelInfo
    {
        public string           Name;
        public string           Path;
        public Bonemap?         BoneToMehses = null; // bone id is the index to an array of mesh ids for it
        public SkelationType    SkelType;
        public BespokeBone[]    BespokeBones;
        public int[]            IngoreMeshes; // Idxs of sub meshes to not import

        public SkelData? Skeleton {
            get
            {
                if (SkelType < 0) { return null; }

                return Skeletons[(int)SkelType];
            }
        }
    }

    public struct Bonemap
    {
        public short[][] BoneToMehses; // bone id is the index to an array of mesh ids for it

        public static Bonemap Create(short[][] BoneMeshes)
        {
            var bmap = new Bonemap()
            {
                BoneToMehses = BoneMeshes
            };

            return bmap;
        }

        // Turn this bone map into a lookup where a mesh index retives the bone idx for it
        public Dictionary<int, int> ToLookup()
        {
            const int AVG_BONE_COUNT = 40;
            var meshToBoneMap        = new Dictionary<int, int>(AVG_BONE_COUNT);

            for (int i = 0; i < BoneToMehses.Length; i++)
            {
                var boneMeshes = BoneToMehses[i];
                if (boneMeshes != null)
                {
                    for (int aye = 0; aye < boneMeshes.Length; aye++)
                    {
                        var meshIdx = boneMeshes[aye];
                        meshToBoneMap.Add(meshIdx, i);
                    }
                }
            }

            return meshToBoneMap;
        }
    }

    public struct BespokeBone
    {
        public string Name;
        public short[] MeshIdxs;
    }

}
