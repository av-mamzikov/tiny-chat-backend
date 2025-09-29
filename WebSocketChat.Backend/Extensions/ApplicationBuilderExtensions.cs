using WebSocketChat.Backend.Services;

namespace WebSocketChat.Backend.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication ConfigureWebSocketChat(this WebApplication app)
    {
        // Configure WebSocket options
        var webSocketOptions = new WebSocketOptions 
        { 
            KeepAliveInterval = TimeSpan.FromMinutes(2) 
        };
        app.UseWebSockets(webSocketOptions);

        // Map WebSocket endpoint
        app.Map("/ws", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var manager = context.RequestServices.GetRequiredService<WebSocketConnectionManager>();
                await manager.HandleClientAsync(webSocket, context.RequestAborted);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        });

        return app;
    }
}
