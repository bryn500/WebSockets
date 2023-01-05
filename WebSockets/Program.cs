using WebSockets.AppStart;
using WebSockets.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddSingleton<TimerBackgroundService>();
builder.Services.AddHostedService<BackgroundServiceStarter<TimerBackgroundService>>();

var app = builder.Build();

var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(20)    
};

app.UseWebSockets(webSocketOptions);

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapHub<TimerHub>("/timerHub");

app.Run();