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
            int halfWid = Mathf.FloorToInt(Settings.CorridorTileWidth / 2f);
            var dir = (line.ToF - line.FromF).Sign4Way().RoundToInt() * halfWid;
            var negativeDir = -dir;
            if (dir.X < 0 || dir.Y < 0) dir -= dir.Sign();
            if (negativeDir.X < 0 || negativeDir.Y < 0) negativeDir -= negativeDir.Sign();
            ProceduralGeometry.DrawLine(
                State.TilemapLayer, 
                line.From + negativeDir, 
                line.To + dir, 
                Settings.CorridorTileWidth,
                TilesSrcId,
                FloorTileCoord
            );
        }
    }

    private void PlaceDoorBorders()
    {
        foreach (var room in State.AllRooms)
        foreach (var (dir, nb) in room.Neighbors)
        foreach ((var corridor, int endpoint) in nb)
        {
            var direction = dir.ToVector2();
            var point = endpoint == 0 ? corridor.FromPos : corridor.ToPos;
            if (direction.X < 0 || direction.Y < 0) point += direction.Sign();
            // point += direction * .5f;
            var perpendicular = direction.Rotated(Mathf.Pi/2f).RoundToInt();
            float corridorWidth = Settings.CorridorTileWidth/2f;
            
            State.TilemapLayer.SetCell(
                point.RoundToInt() - perpendicular.Abs() * Mathf.FloorToInt(corridorWidth-1), 
                TilesSrcId, 
                SolidTileCoord);
            
            State.TilemapLayer.SetCell(
                point.RoundToInt() + perpendicular.Abs() * Mathf.CeilToInt(corridorWidth), 
                TilesSrcId, 
                SolidTileCoord);
            
            State.TilemapLayer.SetCell(
                point.RoundToInt() - perpendicular.Abs() * Mathf.FloorToInt(corridorWidth-2), 
                TilesSrcId, 
                SolidTileCoord);
            
            State.TilemapLayer.SetCell(
                point.RoundToInt() + perpendicular.Abs() * Mathf.CeilToInt(corridorWidth-1), 
                TilesSrcId, 
                SolidTileCoord);
        }
    }
    
    public override async GDTask Perform()
    {
        PlaceBlankTiles();

        CreateRoomFloors();

        CreateCorridorFloors();
                
        PlaceDoorBorders();
    }
}