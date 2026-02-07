using System.Collections.Generic;
using System.Linq;
using Casiland.AutoTileEx.Api;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlaceRoomTilesStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    private static readonly int TilesSrcId = 1;
    private static readonly Vector2I SolidTileCoord = new(0, 0);
    private static readonly Vector2I FloorTileCoord = new(1, 0);

    public override string StateDescription => "Placing room tiles";
    
    private static void FillTilemap(Rect2I rect, TileMapLayer tl, int srcId, Vector2I tileCoord)
    {
        for (int y = rect.Position.Y; y < rect.End.Y; ++y)
            for (int x = rect.Position.X; x < rect.End.X; ++x)
                tl.SetCell(new Vector2I(x, y), srcId, tileCoord);
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
    
    

    private void PlaceBlankTiles()
    {
        State.TilemapLayer.Clear();

        foreach (var rect in State.AllRooms.Select(room => new Rect2I((Vector2I)room.Rect.Position, (Vector2I)room.Rect.Size)))
        {
            FillTilemap(rect.Grow(15), State.TilemapLayer, TilesSrcId, SolidTileCoord);
        }
        
        foreach (var line in State.CorridorLines)
        {
            State.TilemapLayer.SetCells(
                ProceduralGeometry.BresenhamLineWidth(line.From, line.To, 20), TilesSrcId,
                SolidTileCoord);
        }
    }

    private void CreateRoomFloors()
    {
        foreach (var rect in State.AllRooms.Select(room => new Rect2I((Vector2I)room.Rect.Position, (Vector2I)room.Rect.Size)))
        {
            FillTilemap(rect, State.TilemapLayer, TilesSrcId, FloorTileCoord);
        }
    }

    private void CreateCorridorFloors()
    {
        foreach (var line in State.CorridorLines)
        {
            State.TilemapLayer.SetCells(
                ProceduralGeometry.BresenhamLineWidth(line.From, line.To, Settings.CorridorTileWidth), TilesSrcId,
                FloorTileCoord);
        }
    }
    
    public override async GDTask Perform()
    {
        PlaceBlankTiles();

        CreateRoomFloors();

        CreateCorridorFloors();
    }
}