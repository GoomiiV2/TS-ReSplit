using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TS2
{
    public class Pathing
    {
        public Header   Head;
        public Node[]   PathingNodes;
        public Link[]   PathingLinks;
        public Node[]   ExtraNodes;

        public Pathing( ) { }

        public Pathing(byte[] Data)
        {
            Load(Data);
        }

        public void Load(byte[] Data)
        {
            using (BinaryReader r = new BinaryReader(new MemoryStream(Data)))
            {
                Head = Header.Read(r);
                LoadNodes(r);
                LoadLinks(r);
                LoadExtraNodes(r);
            }
        }

        private void LoadNodes(BinaryReader R)
        {
            PathingNodes = new Node[Head.NumPathingNodes];
            for (int i = 0; i < PathingNodes.Length; i++)
            {
                var thing       = Node.Read(R);
                PathingNodes[i] = thing;
            }
        }

        public void LoadLinks(BinaryReader R)
        {
            PathingLinks = new Link[Head.NumPathingLinks];
            for (int i = 0; i < PathingLinks.Length; i++)
            {
                var link        = Link.Read(R);
                PathingLinks[i] = link;
            }
        }

        private void LoadExtraNodes(BinaryReader R)
        {
            var numExtraNodes = R.ReadUInt32();
            ExtraNodes        = new Node[numExtraNodes];
            for (int i = 0; i < ExtraNodes.Length; i++)
            {
                var thing     = Node.Read(R);
                ExtraNodes[i] = thing;
            }
        }

        public struct Header
        {
            public uint ID;
            public uint NumPathingNodes;
            public uint NumPathingLinks;
            public uint StartID;

            public static Header Read(BinaryReader R)
            {
                var header = new Header()
                {
                    ID                  = R.ReadUInt32(),
                    NumPathingNodes     = R.ReadUInt32(),
                    NumPathingLinks     = R.ReadUInt32()
                };

                return header;
            }
        }

        [System.Serializable]
        public struct Node
        {
            public const uint SIZE = 32;

            public uint ID;
            public uint UNK4;
            public uint UNK;
            public float[] Position;
            public uint UNK2;
            public uint UNK3;

            public static Node Read(BinaryReader R)
            {
                var thing      = new Node();
                thing.ID       = R.ReadUInt32();
                thing.UNK      = R.ReadUInt32();
                thing.UNK2 = R.ReadUInt32();

                // Position, array of 3 floats
                thing.Position = new float[3];
                var bytes      = R.ReadBytes(3 * 4);
                Buffer.BlockCopy(bytes, 0, thing.Position, 0, bytes.Length);

                thing.UNK3 = R.ReadUInt32();
                thing.UNK4 = R.ReadUInt32();

                return thing;
            }
        }

        public struct Link
        {
            public const uint SIZE = 32;

            public uint Flags;
            public uint ParentNodeID;
            public uint ChildNodeID;
            public uint UNK1;
            public uint UNK2;

            public static Link Read(BinaryReader R)
            {
                var linkle = new Link()
                {
                    Flags         = R.ReadUInt32(),
                    ParentNodeID  = R.ReadUInt32(),
                    ChildNodeID   = R.ReadUInt32(),
                    UNK1          = R.ReadUInt32(),
                    UNK2          = R.ReadUInt32()
                };

                return linkle;
            }
        }
    }
}
