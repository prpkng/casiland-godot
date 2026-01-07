using System.Diagnostics;
using System.Linq;
using Casiland.AutoTileEx.Api;
using Casiland.AutoTileEx.Api.Data;
using Casiland.Common;
using Casiland.Systems.ProceduralGen;
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

    private static void FillWithPoints(Rect2I rect, ref Array<Vector2I> points)
    {
        for (int y = rect.Position.Y; y < rect.End.Y; ++y)
        for (int x = rect.Position.X; x < rect.End.X; ++x)
            points.Add(new Vector2I(x, y));
    }

    public override async void _Pressed()
    {
        base._Pressed();
        GD.Randomize();
        await generator.PerformGeneration(settings, GD.Randi().ToString());
        var now = new Stopwatch();
        now.Restart();

        // return;
        
        int roomMinX = (int)generator.ResultingState.MainRooms.Min(room => room.Rect.Position.X);
        int roomMinY = (int)generator.ResultingState.MainRooms.Min(room => room.Rect.Position.Y);
        int roomMaxX = (int)generator.ResultingState.MainRooms.Max(room => room.Rect.End.X);
        int roomMaxY = (int)generator.ResultingState.MainRooms.Max(room => room.Rect.End.Y);

        var boundsRect = new Rect2I(roomMinX,
            roomMinY,
            roomMaxX - roomMinX,
            roomMaxY - roomMinY).Grow(20);

        Log.Debug("> Calculated bounds rect (took {Duration} ms)", now.Elapsed.TotalMilliseconds);
        now.Restart();
        
        tilemap.Clear();

        var cells = new Array<Vector2I>();

        FillWithPoints(boundsRect, ref cells);
        foreach (var pos in cells)
            tilemap.SetCell(pos, 1, new Vector2I(0, 0));


        Log.Debug("> Filled with empty tiles (took {Duration} ms)", now.Elapsed.TotalMilliseconds);
        now.Restart();

        foreach (var room in generator.ResultingState.MainRooms.Concat(generator.ResultingState.CorridorRooms))
        {
            cells.Clear();
            var rect = new Rect2I((Vector2I)room.Rect.Position, (Vector2I)room.Size);
            FillWithPoints(rect, ref cells);
            foreach (var pos in cells)
                tilemap.SetCell(pos, 1, new Vector2I(1, 0));
        }


        Log.Debug("> Filled with room tiles (took {Duration} ms)", now.Elapsed.TotalMilliseconds);
        now.Restart();

        cells.Clear();
        
        foreach (var line in generator.ResultingState.CorridorLines)
        {
            ProceduralGeometry.BresenhamLineWidth(cells, line.From, line.To, 5);
        }
        
        foreach (var pos in cells)
            tilemap.SetCell(pos, 1, new Vector2I(1, 0));
        //
        // betterTerrain.UpdateTerrainArea(boundsRect);
        
        Log.Debug("> Updated terrain area (took {Duration} ms)", now.Elapsed.TotalMilliseconds);
        now.Restart();

        AutoTileImplementation.PerformAutoTile(tilemap, ruleSet);
        
        Log.Debug("> Performed auto-tiling (took {Duration} ms)", now.Elapsed.TotalMilliseconds);

        now.Restart();
    }
}