using System.Collections.Generic;
using System.Linq;
using Casiland.AutoTileEx.Api;
using Casiland.Systems.ProceduralGen.Algorithms;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlaceRoomTilesStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    private static readonly int TilesSrcId = 1;
    private static readonly Vector2I SolidTileCoord = new(0, 0);
    private static readonly Vector2I FloorTileCoord = new(1, 0);

    public override string StateDescription => "Placing room tiles";
    
    private static void FillWithPoints(Rect2I rect, ref List<Vector2I> points)
    {
        for (int y = rect.Position.Y; y < rect.End.Y; ++y)
            for (int x = rect.Position.X; x < rect.End.X; ++x)
                points.Add(new Vector2I(x, y));
    }
    private Rect2I ComputeRoomBounds()
    {
        int roomMinX = (int)State.MainRooms.Min(room => room.Rect.Position.X);
        int roomMinY = (int)State.MainRooms.Min(room => room.Rect.Position.Y);
        int roomMaxX = (int)State.MainRooms.Max(room => room.Rect.End.X);
        int roomMaxY = (int)State.MainRooms.Max(room => room.Rect.End.Y);

        var boundsRect = new Rect2I(roomMinX,
            roomMinY,
            roomMaxX - roomMinX,
            roomMaxY - roomMinY).Grow(20);
        return boundsRect;
    }

    private async void PlaceBlankTiles()
    {
        State.TilemapLayer.Clear();
        await Engine.GetMainLoop().ToSignal(Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame);

        var cells = new List<Vector2I>();
        FillWithPoints(ComputeRoomBounds(), ref cells);
        //
        // foreach (var room in State.AllRooms)
        // {
        //     var rect = new Rect2I((Vector2I)room.Rect.Position, (Vector2I)room.Rect.Size);//.Grow(5);
        //     FillWithPoints(rect, ref cells);
        // }

        var tilemap = State.TilemapLayer;
        int c = 0;
        foreach (var pos in cells)
        {
            c++;
            if (c>1000)
            {
                await Engine.GetMainLoop()
                    .ToSignal(Engine.GetMainLoop(), SceneTree.SignalName.ProcessFrame);
                c = 0;
            }
            tilemap.SetCell(pos, 1, new Vector2I(0, 0));
        }
    }

    private void CreateRoomFloors()
    {
        var cells = new List<Vector2I>();
        foreach (var room in State.AllRooms)
        {
            cells.Clear();
            var rect = new Rect2I((Vector2I)room.Rect.Position, (Vector2I)room.Size);
            FillWithPoints(rect, ref cells);

            foreach (var pos in cells)
                State.TilemapLayer.SetCell(pos, TilesSrcId, FloorTileCoord);
        }
    }

    private void CreateCorridorFloors()
    {
        var cells = new List<Vector2I>();
        foreach (var line in State.CorridorLines)
        {
            cells.AddRange(ProceduralGeometry.BresenhamLineWidth(line.From, line.To, 5));
        }
        foreach (var pos in cells)
            State.TilemapLayer.SetCell(pos, TilesSrcId, FloorTileCoord);
    }
    
    public override void Perform()
    {
        PlaceBlankTiles();

        // CreateRoomFloors();

        // CreateCorridorFloors();
    }
}