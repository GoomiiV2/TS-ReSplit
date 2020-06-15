using System;
using System.IO;
using System.Linq;
using System.Text;

public class BinaryReaderEndian : BinaryReader
{
    public bool IsLittleEndian = true;

    public BinaryReaderEndian(Stream input, bool IsLittleEndian = true) : base(input)
    {
        this.IsLittleEndian = IsLittleEndian;
    }

    public BinaryReaderEndian(Stream input, Encoding encoding, bool IsLittleEndian = true) : base(input, encoding)
    {
        this.IsLittleEndian = IsLittleEndian;
    }

    public BinaryReaderEndian(Stream input, Encoding encoding, bool leaveOpen, bool IsLittleEndian = true) : base(input, encoding, leaveOpen)
    {
        this.IsLittleEndian = IsLittleEndian;
    }

    public override double ReadDouble()
    {
        return IsLittleEndian ? base.ReadDouble() : BitConverter.ToDouble(ReverseArray(ReadBytes(sizeof(double))), 0);
    }

    public override short ReadInt16()
    {
        return IsLittleEndian ? base.ReadInt16() : BitConverter.ToInt16(ReverseArray(ReadBytes(sizeof(short))), 0);
    }

    public override int ReadInt32()
    {
        return IsLittleEndian ? base.ReadInt32() : BitConverter.ToInt32(ReverseArray(ReadBytes(sizeof(int))), 0);
    }

    public override long ReadInt64()
    {
        return IsLittleEndian ? base.ReadInt64() : BitConverter.ToInt64(ReverseArray(ReadBytes(sizeof(long))), 0);
    }

    public override float ReadSingle()
    {
        return IsLittleEndian ? base.ReadSingle() : BitConverter.ToSingle(ReverseArray(ReadBytes(sizeof(float))), 0);
    }

    public override ushort ReadUInt16()
    {
        return IsLittleEndian ? base.ReadUInt16() : BitConverter.ToUInt16(ReverseArray(ReadBytes(sizeof(ushort))), 0);
    }

    public override uint ReadUInt32()
    {
        return IsLittleEndian ? base.ReadUInt32() : BitConverter.ToUInt32(ReadBytes(sizeof(uint)), 0);
    }

    public override ulong ReadUInt64()
    {
        return IsLittleEndian ? base.ReadUInt64() : BitConverter.ToUInt64(ReadBytes(sizeof(long)), 0);
    }

    public float[] ReadSingles(int Count)
    {
        var floats = new float[Count];

        for (int i = 0; i < Count; i++)
        {
            floats[i] = ReadSingle();
        }

        return floats;
    }

    public int[] ReadInt32s(int Count)
    {
        var ints = new int[Count];

        for (int i = 0; i < Count; i++)
        {
            ints[i] = ReadInt32();
        }

        return ints;
    }

    // Good thing floats, ints and such are cleanly divisable by 2 :>
    private T[] ReverseArray<T>(T[] InArray)
    {
        for (int i = 0; i < InArray.Length / 2; i++)
        {
            var tmp                         = InArray[i];
            InArray[i]                      = InArray[InArray.Length - i - 1];
            InArray[InArray.Length - i - 1] = tmp;
        }

        return InArray;
    }
}
