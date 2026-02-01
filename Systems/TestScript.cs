using System.Diagnostics;
using System.Linq;
using Casiland.AutoTileEx.Api;
using Casiland.AutoTileEx.Api.Data;
using Casiland.Common;
using Casiland.Systems.ProceduralGen;
using Casiland.Systems.ProceduralGen.Steps;
using Godot;
using Godot.Collections;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.GodotConsole;
using Serilog.Sinks.SystemConsole.Themes;
using Logger = Serilog.Core.Logger;

namespace Casiland.Systems;

public partial class TestScript : Button
{
    [Export] public ProceduralRoomGenerator generator;
    [Export] public ProceduralGenerationSettings settings;

    [Export] public TileMapLayer tilemap;
    [Export] public AutoTileRuleSet ruleSet;

    public override void _Ready()
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
    }


    public override async void _Pressed()
    {

        var state = new GenerationState
        {
            Rng = new RandomNumberGenerator
            {
                Seed = GD.Randi()
            },
            TilemapLayer = tilemap,
        };


        GD.Randomize();
        await generator.PerformGeneration(settings, state);
        var now = new Stopwatch();
        now.Restart();
    }
}