using System.Collections.Generic;
using System.Linq;
using Casiland.AutoTileEx.Api;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlaceRoomTilesStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    private const int TilesSrcId = 1;
    private const int CollisionsSrcId = 0;
    private static readonly Vector2I CollisionsTileCoord = new Vector2I(0, 0);
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
        State.Payload.AutoTileLayer.Clear();

        foreach (var rect in State.AllRooms.Select(room => new Rect2I((Vector2I)room.Rect.Position, (Vector2I)room.Rect.Size)))
        {
            FillTilemap(rect.Grow(15), State.Payload.AutoTileLayer, TilesSrcId, SolidTileCoord);
        }
        
        foreach (var line in State.CorridorLines)
        {
            State.Payload.AutoTileLayer.SetCells(
                ProceduralGeometry.BresenhamLineWidth(line.From, line.To, 20), TilesSrcId,
                SolidTileCoord);
        }
    }

    private void CreateRoomFloors()
    {
        foreach (var rect in State.AllRooms.Select(room => new Rect2I((Vector2I)room.Rect.Position, (Vector2I)room.Rect.Size)))
        {
            // Fill with solid tiles before filling with empty tiles to create an outline of tiles around the room
            // FillTilemap(rect.Grow(1), State.Payload.AutoTileLayer, TilesSrcId, SolidTileCoord);
            FillTilemap(rect, State.Payload.AutoTileLayer, TilesSrcId, FloorTileCoord);
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
                State.Payload.AutoTileLayer, 
                line.From + negativeDir, 
                line.To + dir, 
                Settings.CorridorTileWidth,
                TilesSrcId,
                FloorTileCoord
            );
        }
    }


    /// <summary>
    /// Places tiles around the corridor connections assuming a 2x2 door will be placed on the center of the entrance
    /// </summary>
    /// <remarks>
    /// <para>This code assumes the top-down tileset style with no "uncollidable tiles". This means that the bottom part of the tileset 
    /// is expected to be messed up with perspective, therefore "single tile walls" are not supported and every tile must be 
    /// at least 3 tiles tall</para>
    /// <para>For that to work, we are checking here if the connection is vertical, and if so we create 2 more tiles in the corridor direction
    /// for the sake of the illusion</para>
    /// </remarks>
    private void FillDoorWalls()
    {
        foreach (var room in State.AllRooms)
        foreach (var (dir, nb) in room.Neighbors)
        foreach ((var corridor, int endpoint) in nb)
        {
            var direction = dir.ToVector2();
            var point = endpoint == 0 ? corridor.FromPos : corridor.ToPos;
            if (direction.X < 0 || direction.Y < 0) point += direction.Sign();
            // point += direction * .5f;
            var perpendicular = direction.Rotated(Mathf.Pi/2f).RoundToInt().Abs();
            int halfCorridorWidth = Settings.CorridorTileWidth/2;
            int fillingTilesCount = halfCorridorWidth - 1;
            int verticalFillCount = direction.Abs().Y > direction.Abs().X ? 3 : 1;


            for (int i = 0; i < fillingTilesCount; i++)
            {
                for (int j = 0; j < verticalFillCount; j++)
                {
                    
                    State.Payload.AutoTileLayer.SetCell(
                        point.RoundToInt() - perpendicular.Abs() * (halfCorridorWidth-(i+1)) + direction.Sign4Way().RoundToInt() * j, 
                        TilesSrcId, 
                        SolidTileCoord);
                    
                    State.Payload.AutoTileLayer.SetCell(
                        point.RoundToInt() + perpendicular.Abs() * (halfCorridorWidth - i) + direction.Sign4Way().RoundToInt() * j, 
                        TilesSrcId, 
                        SolidTileCoord);
                }
            }
            
        }
    }

    private void FillCollisionTiles()
    {
        foreach (var tile in State.Payload.AutoTileLayer.GetUsedCellsById(TilesSrcId, SolidTileCoord))
            State.Payload.CollisionsLayer.SetCell(tile, CollisionsSrcId, CollisionsTileCoord);
    }
    
    public override async GDTask Perform()
    {
        PlaceBlankTiles();

        CreateCorridorFloors();

        CreateRoomFloors();

                
        FillDoorWalls();

        FillCollisionTiles();
    }
}