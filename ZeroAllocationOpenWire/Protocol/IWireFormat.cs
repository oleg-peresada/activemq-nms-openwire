using System.Buffers.Binary;
using System.Collections.ObjectModel;

namespace ZeroAllocationOpenWire.Protocol;

public readonly struct WireFormatProperty(byte type, string name)
{
    public byte Type { get; } = type;
    public string Name { get; } = name;
}

public interface IWireFormat
{
    byte Type { get; }
    List<WireFormatProperty> Properties { get; }

    ReadOnlyMemory<byte> Marshal();
}

public struct WireFormatInfo : IWireFormat
{
    public const byte NULL = 0;
    public const byte BOOLEAN_TYPE = 1;
    public const byte BYTE_TYPE = 2;
    public const byte CHAR_TYPE = 3;
    public const byte SHORT_TYPE = 4;
    public const byte INTEGER_TYPE = 5;
    public const byte LONG_TYPE = 6;
    public const byte DOUBLE_TYPE = 7;
    public const byte FLOAT_TYPE = 8;
    public const byte STRING_TYPE = 9;
    public const byte BYTE_ARRAY_TYPE = 10;
    public const byte MAP_TYPE = 11;
    public const byte LIST_TYPE = 12;
    public const byte BIG_STRING_TYPE = 13;
    
    public byte Type => 1;
    public List<WireFormatProperty> Properties { get; set; }

    public ReadOnlyMemory<byte> Marshal()
    {
        
        Properties = new List<WireFormatProperty>();
        Properties.Add(new WireFormatProperty(1, ""));

        var count = Properties.Count;
        BinaryWriter memoryStream = new BinaryWriter(new MemoryStream());
        
        memoryStream.Write(count);
        foreach (var prop in Properties)
        {
            memoryStream.Write(prop.Name);
        }

        //BinaryPrimitives.TryWriteDoubleLittleEndian()
        return new ReadOnlyMemory<byte>();
    }
}