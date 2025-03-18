using FFMpegCore;
using WebCam.Interfaces;
using WebCam.Services;
using WebCam.Settings;

GlobalFFOptions.Configure(new FFOptions()
{
    LogLevel = FFMpegCore.Enums.FFMpegLogLevel.Info,    
    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
    BinaryFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg")
});

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<List<CaptureParameters>>(builder.Configuration.GetSection(nameof(CaptureParameters)));

// Add services to the container.
builder.Services.AddSingleton<ICaptureService, CaptureService>();
builder.Services.AddHostedService<StartCaptureService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddWindowsService();

builder.Logging.AddLog4Net("log4net.config");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.UseExceptionHandler("/error");
app.MapControllers();

app.Run();
