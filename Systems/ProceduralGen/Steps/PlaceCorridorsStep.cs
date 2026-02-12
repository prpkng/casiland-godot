using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Casiland.Common;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;
using Serilog;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlaceCorridorsStep(GenerationState state, ProceduralGenerationSettings settings)
    : GenerationStep(state, settings)
{
    public override string StateDescription => $"Placing {State.CorridorLines?.Count} corridor lines and {State.CorridorRooms?.Count} corridor rooms";
    
    private static readonly Dictionary<Vector2, RoomNeighborDirection> VecToDirDict = new()
    {
        { new Vector2(1, -1), RoomNeighborDirection.Up },
        { new Vector2(-1, -1), RoomNeighborDirection.Up },
        { Vector2.Up, RoomNeighborDirection.Up },
        { new Vector2(1, 1), RoomNeighborDirection.Down },
        { new Vector2(-1, 1), RoomNeighborDirection.Down },
        { Vector2.Down, RoomNeighborDirection.Down },
        { Vector2.Left, RoomNeighborDirection.Left },
        { Vector2.Right, RoomNeighborDirection.Right }
    };

    #region === CREATE CORRIDOR LINES SUBSTEP ===

    private bool CheckCorridorCollision(ProceduralRoom fromRoom, ProceduralRoom toRoom, CorridorShape shape)
    {
        var lines = shape.ComputeLines();
        var allRooms = State.AllRooms.Except([fromRoom, toRoom]).ToArray();
        return lines.Any(line => allRooms.Any(r => ProceduralGeometry.EdgeIntersectsRect(line, r.Rect.Grow(5))));
    }
    
    private bool CheckIfCornerTooSteep(ProceduralRoom fromRoom, ProceduralRoom toRoom, Vector2 fromDir, Vector2 toDir)
    {

        var fromBorder = fromRoom.Rect.CastTowardsPerimeter(fromDir);
        var toBorder = toRoom.Rect.CastTowardsPerimeter(-toDir);
        var dcVector = toBorder - fromBorder;

        float[] arr = [Mathf.Abs(dcVector.X), Mathf.Abs(dcVector.Y)];
        float highest = arr.Max();
        float lowest = arr.Min();
        
        return lowest < highest / Settings.MaxCornerSizeDifference;
    }

    
    private bool TryCreateDirectCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom, Vector2 dir, 
        Vector2 axis, out DirectCorridorShape directCorridor)
    {
        directCorridor = null;
        var overlap = ProceduralGeometry.GetRectOverlapOnAxis(fromRoom.Rect, toRoom.Rect, axis);
        float overlapLengthOnAxis = (overlap.Size * (Vector2.One - axis)).Abs().Sum();
        float fromRoomSizeOnAxis = (fromRoom.Size * axis).Abs().Sum();
        float toRoomSizeOnAxis = (toRoom.Size * axis).Abs().Sum();

        if (overlapLengthOnAxis <= Settings.MinimumDirectCorridorOverlapLength &&
            (overlapLengthOnAxis <= fromRoomSizeOnAxis / 2f || overlapLengthOnAxis <= toRoomSizeOnAxis / 2f))
            return false;
        
        directCorridor = new DirectCorridorShape(axis, fromRoom, toRoom);
        if (fromRoom.Neighbors[directCorridor.FromDirection].Count == 0)
            return true;
        
        return false;
    }

    private bool TryCreateCornerCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom, Vector2 dir,
        out CornerCorridorShape cornerCorridor)
    {
        dir.X += Mathf.Epsilon;
        dir.Y += Mathf.Epsilon;
        var possibleDirections = new List<Vector2>
            {
                Vector2.Right * Mathf.Sign(dir.X),
                Vector2.Down * Mathf.Sign(dir.Y)
            };

        cornerCorridor = new CornerCorridorShape(fromRoom, toRoom, possibleDirections[0]);

        var axis = Vector2.Right;
        var overlap = ProceduralGeometry.GetRectOverlapOnAxis(fromRoom.Rect, toRoom.Rect, axis);
        float overlapLengthOnX = (overlap.Size * (Vector2.One - axis)).Abs().Sum();
        axis = Vector2.Down;
        overlap = ProceduralGeometry.GetRectOverlapOnAxis(fromRoom.Rect, toRoom.Rect, axis);
        float overlapLengthOnY = (overlap.Size * (Vector2.One - axis)).Abs().Sum();

        if (toRoom.Center.Y > fromRoom.Center.Y)
            cornerCorridor.EntranceCornerBias = 0f;
        else
            cornerCorridor.EntranceCornerBias = 1f;

        if (fromRoom.Center.X > toRoom.Center.X)
            cornerCorridor.ExitCornerBias = 1f;
        else
            cornerCorridor.ExitCornerBias = 0f;

        if (overlapLengthOnX > Settings.CorridorTileWidth/2f && overlapLengthOnX < Settings.CorridorTileWidth)
            cornerCorridor.EntranceCornerBias = 0.5f;
        else if (overlapLengthOnX > Settings.CorridorTileWidth)
            cornerCorridor.EntranceCornerBias = 1-cornerCorridor.EntranceCornerBias;
        
        if (overlapLengthOnY > Settings.CorridorTileWidth/2f && overlapLengthOnY < Settings.CorridorTileWidth)
            cornerCorridor.ExitCornerBias = 0.5f;
        else if (overlapLengthOnY > Settings.CorridorTileWidth)
            cornerCorridor.ExitCornerBias = 1-cornerCorridor.ExitCornerBias;

        if (!CheckCorridorCollision(fromRoom, toRoom, cornerCorridor) &&
            !CheckIfCornerTooSteep(fromRoom, toRoom, possibleDirections[0], possibleDirections[1]) &&
            fromRoom.Neighbors[VecToDirDict[possibleDirections[0]]].Count == 0 &&
            toRoom.Neighbors[VecToDirDict[-possibleDirections[1]]].Count == 0)
            return true;

        // Switch to vertical

        cornerCorridor.CornerDirection = possibleDirections[1];



        if (fromRoom.Center.X > toRoom.Center.X)
            cornerCorridor.EntranceCornerBias = 0f;
        else
            cornerCorridor.EntranceCornerBias = 1f;

        if (toRoom.Center.Y > fromRoom.Center.Y)
            cornerCorridor.ExitCornerBias = 0f;
        else
            cornerCorridor.ExitCornerBias = 1f;

            
        if (overlapLengthOnY > Settings.CorridorTileWidth/2f && overlapLengthOnY < Settings.CorridorTileWidth)
            cornerCorridor.EntranceCornerBias = 0.5f;
        else if (overlapLengthOnY > Settings.CorridorTileWidth)
            cornerCorridor.EntranceCornerBias = 1-cornerCorridor.EntranceCornerBias;
        
        if (overlapLengthOnX > Settings.CorridorTileWidth/2f && overlapLengthOnX < Settings.CorridorTileWidth)
            cornerCorridor.ExitCornerBias = 0.5f;
        else if (overlapLengthOnX > Settings.CorridorTileWidth)
            cornerCorridor.ExitCornerBias = 1-cornerCorridor.ExitCornerBias;

        if (!CheckCorridorCollision(fromRoom, toRoom, cornerCorridor) &&
            !CheckIfCornerTooSteep(fromRoom, toRoom, possibleDirections[1], possibleDirections[0]) &&
            fromRoom.Neighbors[VecToDirDict[possibleDirections[1]]].Count == 0 &&
            toRoom.Neighbors[VecToDirDict[-possibleDirections[0]]].Count == 0)
            return true;
        return false;
    }


    private bool TryCreateStepCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom, Vector2 dir,
        Vector2 axis, out StepCorridorShape stepCorridor)
    {
        bool horizontal = Mathf.Abs(dir.X) > Mathf.Abs(dir.Y);

        var dirOnAxis = (dir * axis);
        float stepBias = State.Rng.RandfRange(0.45f, 0.55f);

        stepCorridor = new StepCorridorShape(fromRoom, toRoom, axis, stepBias);

        if (fromRoom.Neighbors[VecToDirDict[dirOnAxis.Sign4Way()]].Count == 0)
            return true;
        
        // Solve for multiple connections on the same side

        var nb = fromRoom.Neighbors[VecToDirDict[dirOnAxis.Sign4Way()]].First();
        var nbDest = nb.endpoint == 0 ? nb.shape.ToRoom : nb.shape.FromRoom;
        float destPos = horizontal ? nbDest.Center.Y : nbDest.Center.X;
        float toPos = horizontal ? toRoom.Center.Y : toRoom.Center.X;
        int side = toPos > destPos ? 1 : 0;

        float roomMin = horizontal ? fromRoom.Rect.Position.Y : fromRoom.Rect.Position.X;
        float roomMax = horizontal ? fromRoom.Rect.End.Y : fromRoom.Rect.End.X;

        ref var endpointPos = ref (nb.endpoint == 0 ? ref nb.shape.FromPos : ref nb.shape.ToPos);
        float sizeOnAxis = horizontal ? fromRoom.Size.Y : fromRoom.Size.X;
        int increment = Mathf.Clamp(
            Mathf.RoundToInt((sizeOnAxis - Settings.CorridorTileWidth * 2) / 2f),
            Settings.CorridorTileWidth,
            (int)(sizeOnAxis / 2f + Settings.CorridorTileWidth / 1.75f)
        );

        if (horizontal)
        {
            endpointPos.Y = side == 1 ? roomMin + increment : roomMax - increment;
            stepCorridor.FromPos.Y = side == 0 ? roomMin + increment : roomMax - increment;
        }
        else
        {
            endpointPos.X = side == 1 ? roomMin + increment : roomMax - increment;
            stepCorridor.FromPos.X = side == 0 ? roomMin + increment : roomMax - increment;
        }

        return true;
    }

    private async GDTask CreateCorridorBetween(ProceduralRoom fromRoom, ProceduralRoom toRoom)
    {
        fromRoom.CorridorLines ??= [];
        toRoom.CorridorLines ??= [];

        var dir = toRoom.Center - fromRoom.Center;
        bool horizontal = Mathf.Abs(dir.X) > Mathf.Abs(dir.Y);

        var axis = horizontal ? Vector2.Right : Vector2.Down;

        CorridorShape corridorShape;
        if (TryCreateDirectCorridor(fromRoom, toRoom, dir, axis, out var directCorridor))
            corridorShape = directCorridor;
        else if (TryCreateCornerCorridor(fromRoom, toRoom, dir, out var cornerCorridor))
            corridorShape = cornerCorridor;

        else if (TryCreateStepCorridor(fromRoom, toRoom, dir, axis, out var stepCorridor))
            corridorShape = stepCorridor;
        else
        {
            Log.Error("Failed to create corridor between room {From} -> {To}", fromRoom.Id, toRoom.Id);
            return;
        }

        fromRoom.Neighbors[corridorShape.FromDirection].Add((corridorShape, 0));
        toRoom.Neighbors[corridorShape.ToDirection].Add((corridorShape, 1));

        fromRoom.ConnectionDirections.Add(corridorShape.FromDirection);
        toRoom.ConnectionDirections.Add(corridorShape.ToDirection);

    }
    private async GDTask CreateCorridorLines()
    {
        var visited = new HashSet<string>();
        State.CorridorLines.Clear();

        foreach (var fromRoom in State.AllRooms)
        foreach (var toRoom in fromRoom.Connections)
        {
            string key = new[] { fromRoom.Id, toRoom.Id }.Order().ToArray().Join("-");
            if (!visited.Add(key)) continue;
            
            Log.Information("Creating corridor {From} -> {To}",  fromRoom.Id, toRoom.Id);
            await CreateCorridorBetween(fromRoom, toRoom);
        }
        
        visited.Clear();
        foreach (var fromRoom in State.AllRooms)
        foreach (var (shape, endpoint) in fromRoom.Neighbors.Values.SelectMany(l => l))
        {
            var toRoom = endpoint == 0 ? shape.ToRoom : shape.FromRoom;
            string key = new[] { fromRoom.Id, toRoom.Id }.Order().ToArray().Join("-");
            if (!visited.Add(key)) continue;
            
            
            var lines = shape.ComputeLines();

            State.CorridorLines.AddRange(lines);
            State.CorridorLineGroups.Add(lines);
            fromRoom.CorridorLines.Add(lines[0]);
            toRoom.CorridorLines.Add(lines[^1]);
        }
        

    }
    
    #endregion
    
    public override async GDTask Perform()
    {
        State.AllRooms = [.. State.MainRooms, .. State.CorridorRooms];
        State.AllRooms = State.AllRooms.OrderBy(room => room.ProgressBias).ToList();

        foreach (var room in State.AllRooms)
        {
            room.Rect.Position = room.Rect.Position.Round();
            room.Rect.Size = (room.Rect.Size / 2f).Round() * 2f;
        }
        
        await CreateCorridorLines();
    }
}