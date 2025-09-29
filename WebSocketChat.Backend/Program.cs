using WebSocketChat.Backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebSocketChatServices(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("PublicApi", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("PublicApi");

app.ConfigureWebSocketChat();

app.MapControllers();

app.Run();
