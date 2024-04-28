using Apache.NMS.ActiveMQ.Commands;
using Apache.NMS.ActiveMQ.OpenWire;
using Apache.NMS.ActiveMQ.Util;
using ZeroAllocationOpenWire;

byte[] magic =
[
    'A'&0xFF,
    'c'&0xFF,
    't'&0xFF,
    'i'&0xFF,
    'v'&0xFF,
    'e'&0xFF,
    'M'&0xFF,
    'Q'&0xFF
];

var handler = new ConnectionHandler();
await handler.ConnectAsync();

var mem = new MemoryStream();
await using var writer = new BinaryWriter(mem);

var ci = new ConnectionInfo();
ci.Password = "artemis";
ci.UserName = "artemis";
ci.ClientId = new IdGenerator().GenerateId();
ci.ConnectionId = new ConnectionId();
ci.CommandId = 1;
ci.ConnectionId.Value = new IdGenerator().GenerateId();
ci.FaultTolerant = true;
ci.ResponseRequired = true;

var st = ci.ToString();

var wire = new OpenWireFormat();

var command = new WireFormatInfo
{
    TightEncodingEnabled = false
};

command.CacheEnabled = false;
command.StackTraceEnabled = false;
command.TcpNoDelayEnabled = true;
command.SizePrefixDisabled = false;
command.TightEncodingEnabled = false;
command.MaxInactivityDuration = 30000;
command.MaxInactivityDurationInitialDelay = 10000;
command.CacheSize = 0;
command.Version = 12;

wire.Marshal(command, writer);

// writer.Write(1); //type
// writer.Write(magic, 0, 8);
// writer.Write(10); //version


writer.Flush();
//writer.Seek(0, SeekOrigin.Begin);
await handler.WriteMessage(new ReadOnlyMemory<byte>(mem.ToArray()));


// var mem1 = new MemoryStream();
// await using var writer1 = new BinaryWriter(mem1);
// wire.Marshal(ci, writer1);
// await handler.WriteMessage(new ReadOnlyMemory<byte>(mem1.ToArray()));

// wire.Marshal(command, writer);
// writer.Flush();
// await handler.WriteMessage(new ReadOnlyMemory<byte>(mem.ToArray()));

var r = "";

Console.ReadLine();