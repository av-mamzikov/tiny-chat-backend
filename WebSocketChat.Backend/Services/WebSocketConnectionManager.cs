using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;

namespace WebSocketChat.Backend.Services;

public class WebSocketConnectionManager : IDisposable
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
    private readonly CancellationTokenSource _shutdownTokenSource = new();
    private readonly ConcurrentQueue<string> _recentMessages = new();
    private const int MaxRecentMessages = 50;

    public async Task HandleClientAsync(WebSocket webSocket, CancellationToken cancellationToken = default)
    {
        var clientId = Guid.NewGuid();
        _clients[clientId] = webSocket;
        
        // Combine the provided cancellation token with our shutdown token
        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _shutdownTokenSource.Token);
        var combinedToken = combinedTokenSource.Token;

        // Send recent chat history to the newly connected client
        await SendBacklogAsync(webSocket, combinedToken);

        var buffer = new byte[1024 * 4];
        try
        {
            while (webSocket.State == WebSocketState.Open && !combinedToken.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), combinedToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await BroadcastMessageAsync(message, clientId, combinedToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        finally
        {
            _clients.TryRemove(clientId, out _);
            if (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            }
        }
    }

    private async Task BroadcastMessageAsync(string message, Guid senderId, CancellationToken cancellationToken = default)
    {
        // Store message in bounded recent history
        _recentMessages.Enqueue(message);
        while (_recentMessages.Count > MaxRecentMessages && _recentMessages.TryDequeue(out _)) { }

        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tasks = _clients.Where(c => c.Value.State == WebSocketState.Open)
            .Select(async c =>
            {
                try
                {
                    await c.Value.SendAsync(
                        new ArraySegment<byte>(messageBytes),
                        WebSocketMessageType.Text,
                        true,
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown
                }
                catch
                {
                    // Optionally handle/log failed sends
                }
            });
        await Task.WhenAll(tasks);
    }

    private async Task SendBacklogAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        // snapshot the queue to avoid sending partial while iterating
        foreach (var msg in _recentMessages.ToArray())
        {
            if (socket.State != WebSocketState.Open || cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await SendTextAsync(socket, msg, cancellationToken);
        }
    }

    private static Task SendTextAsync(WebSocket socket, string message, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        return socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
    }

    public void Dispose()
    {
        _shutdownTokenSource.Cancel();
        
        // Close all remaining connections
        var closeTasks = _clients.Values
            .Where(ws => ws.State == WebSocketState.Open)
            .Select(async ws =>
            {
                try
                {
                    await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
                }
                catch
                {
                    // Ignore errors during cleanup
                }
            });
        
        try
        {
            Task.WaitAll(closeTasks.ToArray(), TimeSpan.FromSeconds(5));
        }
        catch
        {
            // Ignore timeout or other errors during shutdown
        }
        
        _shutdownTokenSource.Dispose();
    }
}
