using WebSocketChat.Backend.Services;

namespace WebSocketChat.Backend.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebSocketChatServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register WebSocket connection manager
        services.AddSingleton<WebSocketConnectionManager>();
        
        // Configure CORS for frontend communication
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:8080")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
        
        return services;
    }
}
