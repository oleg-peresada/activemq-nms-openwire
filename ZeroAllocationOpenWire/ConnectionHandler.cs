using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;

namespace ZeroAllocationOpenWire;

internal sealed class ConnectionHandler : IAsyncDisposable
{
    private readonly ChannelWriter<ReadOnlyMemory<byte>> _channelWriter;
    private readonly ChannelReader<ReadOnlyMemory<byte>> _channelReader;

    private PipeWriter? _pipeWriter;
    private PipeReader? _pipeReader;

    private Task? _backgroundLoop;
    
    public ConnectionHandler()
    {
        var channel = Channel.CreateBounded<ReadOnlyMemory<byte>>(
            new BoundedChannelOptions(128)
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = false
            });

        _channelWriter = channel.Writer;
        _channelReader = channel.Reader;
    }

    public async Task ConnectAsync()
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(IPAddress.Parse("127.0.0.1"), 61616);

        var stream = new NetworkStream(socket, FileAccess.ReadWrite);
        
        _pipeWriter = PipeWriter.Create(stream, new StreamPipeWriterOptions());
        _pipeReader = PipeReader.Create(stream, new StreamPipeReaderOptions());
        
        _backgroundLoop = Task.Run(LoopAsync);
    }

    private Task LoopAsync()
    {
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}