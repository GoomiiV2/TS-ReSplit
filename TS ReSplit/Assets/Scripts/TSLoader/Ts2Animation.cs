using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace TS2
{
    public class Animation
    {
        public Header Head;
        public uint[] Ids;
        public BoneTrack[] BoneTracks;
        public RootFrame[] RootFrames;
        public RootFrame[] BindPose;
        public Frame[] Frames;
        public bool IsBindPose = true;

        const int ROOT_BONE_FLAG  = 8;
        const int CHILD_BONE_FLAG = 2;
        const int BIND_BONE_FLAG  = 0;

        public Animation(byte[] Data)
        {
            Load(Data);
        }

        public void Load(byte[] Data)
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(Data)))
            {
                Head = Header.Read(r);
                r.BaseStream.Seek(24, SeekOrigin.Current); // padding

                ReadIds(r);
                ReadBoneTracks(r);
                ReadFrames(r);
            }
        }

        private void ReadIds(BinaryReader R)
        {
            var bytesToRead = (int)(sizeof(uint) * Head.NumIds);
            var bytes       = R.ReadBytes(bytesToRead);

            Ids = new uint[Head.NumIds];
            Buffer.BlockCopy(bytes, 0, Ids, 0, bytes.Length);
        }

        private void ReadBoneTracks(BinaryReader R)
        {
            BoneTracks = new BoneTrack[Head.NumBones];

            for (int i = 0; i < BoneTracks.Length; i++)
            {
                var boneTrack = BoneTrack.Read(R);
                BoneTracks[i] = boneTrack;

                if (IsBindPose && boneTrack.Flags != BIND_BONE_FLAG)
                {
                    IsBindPose = false;
                }
            }
        }

        private void ReadFrames(BinaryReader R)
        {
            var frames = new List<Frame>();

            // This is kinda hacky yea, but not sure how else to load these bind poses correctly
            if (IsBindPose)
            {
                R.BaseStream.Seek(8, SeekOrigin.Current);
                BindPose = new RootFrame[Head.NumBones];
            }

            for (int i = 0; i < BoneTracks.Length; i++)
            {
                var boneTrack = BoneTracks[i];

                if (boneTrack.Flags == ROOT_BONE_FLAG)
                {
                    RootFrames = new RootFrame[Head.NumIds];
                    for (int eye = 0; eye < RootFrames.Length; eye++)
                    {
                        var keyFrame  = RootFrame.Read(R);
                        RootFrames[eye] = keyFrame;
                    }
                }
                else if (boneTrack.Flags == CHILD_BONE_FLAG)
                {
                    var frame = Frame.Read(R, (int)Head.NumIds);
                    frames.Add(frame);
                }
                else if (boneTrack.Flags == BIND_BONE_FLAG)
                {
                    var bindFrame = RootFrame.Read(R);
                    BindPose[i]   = bindFrame;
                }
                else
                {
                    Debug.WriteLine($"Bone track had an unknown flag: {boneTrack.Flags}");
                }
            }

            Frames = frames.ToArray();
        }

        // Internal types
        //===============================================

        public struct Header
        {
            public char[] Magic;
            public uint Version;

            public uint Unk1;
            public uint Unk2;
            public uint NumIds;
            public uint NumBones;

            public static Header Read(BinaryReader R)
            {
                var header = new Header()
                {
                    Magic   = R.ReadChars(5),
                    Version = R.ReadUInt32()
                };

                R.BaseStream.Seek(39, SeekOrigin.Current);

                header.Unk1     = R.ReadUInt32();
                header.Unk2     = R.ReadUInt32();
                header.NumIds   = R.ReadUInt32();
                header.NumBones = R.ReadUInt32();

                return header;
            }
        }

        public struct BoneTrack
        {
            public uint     Unk;
            public uint     Flags;
            public float    NumFrames;
            public uint     NumKeyframes;

            public static BoneTrack Read(BinaryReader R)
            {
                var boneTrack = new BoneTrack()
                {
                    Unk           = R.ReadUInt32(),
                    Flags         = R.ReadUInt32(),
                    NumFrames     = R.ReadSingle(),
                    NumKeyframes  = R.ReadUInt32()
                };

                R.BaseStream.Seek(16, SeekOrigin.Current);

                return boneTrack;
            }
        }

        public struct RootFrame
        {
            public float X;
            public float Y;
            public float Z;
            public Quaternion Rotation;

            public static RootFrame Read(BinaryReader R)
            {
                var rootFrame = new RootFrame()
                {
                    X        = R.ReadSingle(),
                    Y        = R.ReadSingle(),
                    Z        = R.ReadSingle(),
                    Rotation = Quaternion.Read(R)
                };

                return rootFrame;
            }
        }

        // TODO: name better, its more of a track than a frame
        public class Frame
        {
            public float X;
            public float Y;
            public float Z;
            public Quaternion[] Rotations;

            public static Frame Read(BinaryReader R, int NumFrames)
            {
                var frame = new Frame()
                {
                    X         = R.ReadSingle(),
                    Y         = R.ReadSingle(),
                    Z         = R.ReadSingle(),
                    Rotations = new Quaternion[NumFrames]
                };

                for (int i = 0; i < frame.Rotations.Length; i++)
                {
                    var rot            = Quaternion.Read(R);
                    frame.Rotations[i] = rot;
                }

                return frame;
            }
        }

        // Reads in 8 bytes and normlises them into 4 floats for a Quaternion
        // Thanks to FreakByte for help with this :>
        public struct Quaternion
        {
            const int SIZE                 = 8;
            const double NormalizeFraction = 2d / 65535d;

            public float X;
            public float Y;
            public float Z;
            public float W;

            public static Quaternion Read(BinaryReader R)
            {
                var bytes = R.ReadBytes(SIZE);

                var rot = new Quaternion()
                {
                    X = NormalizeTwoBytes(bytes[0], bytes[1]),
                    Y = NormalizeTwoBytes(bytes[2], bytes[3]),
                    Z = NormalizeTwoBytes(bytes[4], bytes[5]),
                    W = NormalizeTwoBytes(bytes[6], bytes[7])
                };

                return rot;
            }

            public static float NormalizeTwoBytes(byte a, byte b)
            {
                return (float)((a + (b << 8)) * NormalizeFraction) - 1f;
            }
        }
    }
}
