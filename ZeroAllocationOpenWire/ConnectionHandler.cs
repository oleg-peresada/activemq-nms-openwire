using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Apache.NMS.ActiveMQ.OpenWire;

namespace ZeroAllocationOpenWire;

internal sealed class ConnectionHandler : IAsyncDisposable
{
    private readonly ChannelWriter<ReadOnlyMemory<byte>> _channelWriter;
    private readonly ChannelReader<ReadOnlyMemory<byte>> _channelReader;

    private PipeWriter? _pipeWriter;
    private PipeReader? _pipeReader;

    private Task? _backgroundLoop;
    private Task? _mainLoop;

    private volatile bool _closed;
    
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

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(IPAddress.Parse("127.0.0.1"), 61616, cancellationToken);

        var stream = new NetworkStream(socket);
        
        stream.ReadTimeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
        stream.WriteTimeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;
        
        _pipeWriter = PipeWriter.Create(stream);
        _pipeReader = PipeReader.Create(stream);
        
        _mainLoop = Task.Run(LoopReadAsync, cancellationToken);
        _backgroundLoop = Task.Run(LoopAsync, cancellationToken);
    }

    private async Task LoopReadAsync()
    {
        if (_pipeReader == null)
        {
            return;
        }

        while (!_closed)
        {
            var result  = await _pipeReader.ReadAsync();
            if (result.IsCompleted)
            {
                _closed = true;
                return;
            }

            var buffer = result.Buffer;
            var arr = buffer.ToArray();
            
            var wire = new OpenWireFormat();
            using BinaryReader reader = new(new MemoryStream(arr));

            var r = wire.Unmarshal(reader);
            _pipeReader.AdvanceTo(buffer.End);
        }
    }

    private async ValueTask LoopAsync()
    {
        try
        {
            if (_pipeWriter is null)
            {
                throw new InvalidOperationException("Pipe writer closed");
            }

            while (await _channelReader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (_channelReader.TryRead(out ReadOnlyMemory<byte> message))
                {
                    await _pipeWriter.WriteAsync(message).ConfigureAwait(false);
                }

                await _pipeWriter.FlushAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            //todo log
            throw;
        }
    }

    public ValueTask WriteMessage(ReadOnlyMemory<byte> message)
    {
        return _channelWriter.WriteAsync(message);
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}