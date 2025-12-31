using Godot;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.GodotConsole;

namespace Casiland.CasilandUtils;

[Tool]
public partial class StartSerilog : EditorScript
{
	public override void _Run()
	{
		var logger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.WriteTo.GodotConsole(templateSelector: logEvent => logEvent.Level switch
			{
				LogEventLevel.Verbose => "[color=#949494]{Timestamp:HH:mm:ss} [VRB] {Message}[/color]",
				LogEventLevel.Debug => "[color=#DADADA]{Timestamp:HH:mm:ss} [DBG] {Message}[/color]",
				LogEventLevel.Information => "[color=#AFD7AF]{Timestamp:HH:mm:ss} [INF] {Message}[/color]",
				LogEventLevel.Warning => "[color=#FFAF87]{Timestamp:HH:mm:ss} [WRN] {Message}[/color]",
				LogEventLevel.Error => "[color=#FF6B9D]{Timestamp:HH:mm:ss} [ERR] {Message}{Exception}[/color]",
				LogEventLevel.Fatal => "[color=#FF005F][b]{Timestamp:HH:mm:ss} [FTL] {Message}{Exception}[/b][/color]",
				_ => "{Timestamp:HH:mm:ss} [{Level:u3}] {Message}"
			})
			.CreateLogger();
		Log.Logger = logger;

		Log.Information("Initialized Serilog");
	}
}