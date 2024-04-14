using System.Buffers.Binary;
using ZeroAllocationOpenWire;

var handler = new ConnectionHandler();
await handler.ConnectAsync();

await handler.WriteMessage(new ReadOnlyMemory<byte>());

var r = "";

Console.ReadLine();