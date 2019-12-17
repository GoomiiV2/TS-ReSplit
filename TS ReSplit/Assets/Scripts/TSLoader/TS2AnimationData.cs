using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TS2AnimationData
{
    public static SkelData HumanSkel { get { return Skeletons[(int)SkelationType.Human]; } }

    public enum SkelationType
    {
        Bespoke = -2, // Model will pass in mesh section ids to be given a bone
        None    = -1,
        Human   = 0
    };

    public static readonly SkelData[] Skeletons = new SkelData[]
    {
        // Human
        new SkelData()
        {
            BindPosePath = @"ts2/pak/fe_anim.pak/anim/data/ts2/human_19_bindpose.raw",
            Names        = new string[]
            {
                "Hips",                 // 0
                "Waist",                // 1
                "Neck",                 // 2
                "Head",                 // 3
                "Right Shoulder 1",     // 4
                "Right Shoulder 2",     // 5
                "Right Elbow",          // 6
                "Right Wrist",          // 7
                "Left Shoulder 1",      // 8
                "Left Shoulder 2",      // 9
                "Left Elbow",           // 10
                "Left Wrist",           // 11
                "Right Hip",            // 12
                "Right Knee",           // 13
                "Right Foot",           // 14
                "Left Hip",             // 15
                "Left Knee",            // 16
                "Left Foot",            // 17
            },

            /*Names        = new string[]
            {
                "Hips",                 // 0
                "Waist",                // 1
                "Neck",                 // 2
                "Head",                 // 3
                "rblade",     // 4
                "rshoulder",     // 5
                "relbow",          // 6
                "rwrist",          // 7
                "lblade",      // 8
                "lshoulder",      // 9
                "lelbow",           // 10
                "lwrist",           // 11
                "rhip",            // 12
                "rknee",           // 13
                "rheel",           // 14
                "lhip",             // 15
                "lknee",            // 16
                "lheel",            // 17
            },*/

            // TODO: calualte this
            BonePaths = new string[]
            {
                "Root/Hips",
                "Root/Hips/Waist",
                "Root/Hips/Waist/Neck",
                "Root/Hips/Waist/Neck/Head",
                "Root/Hips/Waist/Right Shoulder 1",
                "Root/Hips/Waist/Right Shoulder 1/Right Shoulder 2",
                "Root/Hips/Waist/Right Shoulder 1/Right Shoulder 2/Right Elbow",
                "Root/Hips/Waist/Right Shoulder 1/Right Shoulder 2/Right Elbow/Right Wrist",
                "Root/Hips/Waist/Left Shoulder 1",
                "Root/Hips/Waist/Left Shoulder 1/Left Shoulder 2",
                "Root/Hips/Waist/Left Shoulder 1/Left Shoulder 2/Left Elbow",
                "Root/Hips/Waist/Left Shoulder 1/Left Shoulder 2/Left Elbow/Left Wrist",
                "Root/Right Hip",
                "Root/Right Hip/Right Knee",
                "Root/Right Hip/Right Knee/Right Foot",
                "Root/Left Hip",
                "Root/Left Hip/Left Knee",
                "Root/Left Hip/Left Knee/Left Foot"
            },

            BoneMap = new short[][]
            {
                new short[] { 0, 12, 15 },  // Root
                new short[] { 1 },          // Hips
                new short[] { 2, 4, 8 },    // Waist
                new short[] { 3 },          // Neck
                new short[] { },            // Head
                new short[] { 5 },          // Right Shoulder 1
                new short[] { 6 },          // Right Shoulder 2
                new short[] { 7 },          // Right Elbow
                new short[] { },            // Right Wrist
                new short[] { 9 },          // Left Shoulder 1
                new short[] { 10 },         // Left Shoulder 2
                new short[] { 11 },         // Left Elbow
                new short[] { },            // Left Wrist
                new short[] { 13 },         // Right Hip
                new short[] { 14 },         // Right Knee
                new short[] { },            // Right Foot
                new short[] { 16 },         // Left Hip
                new short[] { 17 },         // Left Knee
                new short[] { },            // Left Foot
            }
        }
    };

    public struct SkelData
    {
        // bone index to names to make things nicer to debug and work with :>
        public string[]     Names;
        public short[][]    BoneMap;
        public string       BindPosePath;
        public string[]     BonePaths;

        /*private short[][]   _childToParentBoneMap;
        public short[][]    ChildToParentMap
        {
            get
            {
                if (_childToParentBoneMap != null) { return _childToParentBoneMap; }

                var map = new List<short>[BoneMap.Length];
                for (short i = 0; i < BoneMap.Length; i++)
                {
                    var childs = BoneMap[i];
                    foreach (var child in childs)
                    {
                        var idx = child + 1;
                        if (map[idx] == null) { map[idx] = new List<short>(); }
                        map[idx].Add(i);
                    }
                }
            }
        } */

        public (string Name, short[] Children) GetNameAndChildren(int Idx)
        {
            var data = (Names[Idx], BoneMap[Idx]);
            return data;
        }
    }
}
