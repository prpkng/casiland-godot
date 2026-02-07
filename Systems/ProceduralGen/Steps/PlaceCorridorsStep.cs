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

    private ProceduralRoom CreateInBetweenRoom(Vector2 center)
    {
        float baseSize = Mathf.Lerp(Settings.MinBaseRoomSize, Settings.MaxBaseRoomSize, State.Rng.RandfRange(0f, 0.5f));
        var size = ProceduralGeometry.AspectWiseRandomSize(
            State.Rng,
            Settings.BaseRoomAspect,
            Settings.MaxRoomAspectDeviation,
            baseSize,
            Settings.MaxRoomSizeDeviation
        );
        return new ProceduralRoom(center - size / 2, size);
    }

    private bool CheckForRoomOverlap(ProceduralRoom room)
    {
        var rect = room.Rect.Grow(5);
        return ((ProceduralRoom[]) [..State.MainRooms, ..State.CorridorRooms])
            .Any(other => other.Rect.Intersects(rect));
    }
    
    ProceduralRoom TryCreateRoom(Vector2 center, Vector2 offset)
    {
        var candidates = new[]
        {
            center + offset,
            center - offset,
            center
        };

        return candidates.Select(CreateInBetweenRoom).FirstOrDefault(room => !CheckForRoomOverlap(room));
    }
    
    private void CreateInBetweenRooms()
    {
        foreach (var line in State.MinimumSpanningTree)
        {
            float len = line.EuclideanLength;
            int roomCount = Mathf.FloorToInt(len / Settings.InBetweenRoomsDenominator);
            if (roomCount < 1) continue;
            
            var fromRoom = State.PointToRoom[line.From];
            var toRoom = State.PointToRoom[line.To];
            
            fromRoom.Connections.Remove(toRoom);
            toRoom.Connections.Remove(fromRoom);
            var lastRoom = fromRoom;
            
            for (int i = 0; i < roomCount; i++)
            {
                float factor = (i + 1f) / (roomCount + 1f);
                var center = line.FromF.Lerp(line.ToF, factor);
                const float maxSpacing = 16f;
                var offset = line.Direction.Orthogonal() * State.Rng.RandfRange(-maxSpacing, maxSpacing);

                var room = TryCreateRoom(center, offset);
                if (room == null) continue;                
                
                room.ProgressBias = (fromRoom.ProgressBias + toRoom.ProgressBias) / 2;
                room.StartDistance = (fromRoom.StartDistance + toRoom.StartDistance) / 2;
                room.BossDistance = (fromRoom.BossDistance + toRoom.BossDistance) / 2;
                
                room.Connections.Add(lastRoom);
                lastRoom.Connections.Add(room);
                
                room.CorridorLines = [];
                State.CorridorRooms.Add(room);
                
                lastRoom = room;
            }
            lastRoom.AddConnection(toRoom);
            toRoom.AddConnection(lastRoom);

        }
    }

    #region === CREATE CORRIDOR LINES SUBSTEP ===
    
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
    
    private async GDTask CreateCorridorBetween(ProceduralRoom fromRoom, ProceduralRoom toRoom)
    {
        fromRoom.CorridorLines ??= [];
        toRoom.CorridorLines ??= [];

        var dir = toRoom.Center - fromRoom.Center;
        bool horizontal = Mathf.Abs(dir.X) > Mathf.Abs(dir.Y);

        var axis = horizontal ? Vector2.Right : Vector2.Down;

        var overlap = ProceduralGeometry.GetRectOverlapOnAxis(fromRoom.Rect, toRoom.Rect, axis);
        float overlapLengthOnAxis = (overlap.Size * (Vector2.One - axis)).Abs().Sum();
        float fromRoomSizeOnAxis = (fromRoom.Size * axis).Abs().Sum();
        float toRoomSizeOnAxis = (toRoom.Size * axis).Abs().Sum();

        var corridorShape = GenCorridorShape();
        fromRoom.Neighbors[corridorShape.FromDirection].Add((corridorShape, 0));
        toRoom.Neighbors[corridorShape.ToDirection].Add((corridorShape, 1));
        fromRoom.ConnectionDirections.Add(corridorShape.FromDirection);
        toRoom.ConnectionDirections.Add(corridorShape.ToDirection);

        var lines = corridorShape.ComputeLines();

        State.CorridorLines.AddRange(lines);
        State.CorridorLineGroups.Add(lines);
        fromRoom.CorridorLines.Add(lines[0]);
        toRoom.CorridorLines.Add(lines[^1]);
        await GDTask.Delay(200);
        return;

        CorridorShape GenCorridorShape()
        {
            if (overlapLengthOnAxis > Settings.MinimumDirectCorridorOverlapLength ||
                (overlapLengthOnAxis > fromRoomSizeOnAxis / 2f && overlapLengthOnAxis > toRoomSizeOnAxis / 2f))
            {
                var shape = new DirectCorridorShape(axis, fromRoom, toRoom);
                if (fromRoom.Neighbors[shape.FromDirection].Count == 0)
                    return shape;
            }
            
            var possibleDirections = new List<Vector2>
            {
                Vector2.Right * Mathf.Sign(dir.X),
                Vector2.Down * Mathf.Sign(dir.Y)
            };

            var cornerCorridor = new CornerCorridorShape(fromRoom, toRoom, possibleDirections[0]);
            if (!CheckIfCornerTooSteep(fromRoom, toRoom, possibleDirections[0], possibleDirections[1]) &&
                fromRoom.Neighbors[VecToDirDict[possibleDirections[0]]].Count == 0 && 
                toRoom.Neighbors[VecToDirDict[-possibleDirections[1]]].Count == 0)
                return cornerCorridor;
            cornerCorridor.CornerDirection = possibleDirections[1];
            if (!CheckIfCornerTooSteep(fromRoom, toRoom, possibleDirections[1], possibleDirections[0]) &&
                     fromRoom.Neighbors[VecToDirDict[possibleDirections[1]]].Count == 0 && 
                     toRoom.Neighbors[VecToDirDict[-possibleDirections[0]]].Count == 0)
                return cornerCorridor;

            float stepBias = State.Rng.RandfRange(0.45f, 0.55f);
            return new StepCorridorShape(fromRoom, toRoom, axis, stepBias);
        }
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
        
    }
    
    #endregion

    public void SelectCorridorRooms()
    {
        State.CorridorRooms.Clear();

        foreach (var edges in State.CorridorLineGroups)
        {
            foreach (var edge in edges)
            foreach (var room in State.OtherRooms)
            {
                if (!ProceduralGeometry.EdgeIntersectsRect(edge.From, edge.To, room.Rect)) continue;

                if (State.CorridorRooms.Contains(room)) return;

                room.CorridorLines ??= [];

                var validEdges = edges.Where(e => ProceduralGeometry.EdgeIntersectsRect(e, room.Rect.Grow(5))).ToArray();

                if (validEdges.Length == 1)
                {
                    var validEdge =  validEdges[0];
                    var dir = ((Vector2)validEdge.From).DirectionTo(validEdge.To).Abs();
                    bool horizontal = dir.X > dir.Y;
                    List<RoomNeighborDirection> directions = horizontal 
                        ? [RoomNeighborDirection.Left, RoomNeighborDirection.Right] : [RoomNeighborDirection.Up, RoomNeighborDirection.Down];
                    room.ConnectionDirections.AddRange(directions);
                }
                else
                {
                    foreach (var validEdge in validEdges)
                    {
                        Vector2[] points = [validEdge.From, validEdge.To];
                        foreach (var p in points)
                        {
                            if (room.Rect.HasPoint(p)) continue;
                            var vectDir = room.Center.DirectionTo(p);
                            bool horizontal = vectDir.X > vectDir.Y;
                            vectDir *= horizontal ? Vector2.Right : Vector2.Down;
                            var dir = VecToDirDict[vectDir.Sign()];
                            if (room.ConnectionDirections.Contains(dir)) continue;
                            room.ConnectionDirections.Add(dir);
                        }
                    }
                }

                room.CorridorLines.AddRange(validEdges);

                State.CorridorRooms.Add(room);
            }
        }

    }

    private void SolveLongCorridor(LineSegment longCorridor)
    {
        Log.Verbose("> Found corridor with length {Len} which exceeds the maximum length!",
            longCorridor.From.DistanceTo(longCorridor.To));


        var from = (Vector2)longCorridor.From;
        var lineDirAbs = from.DirectionTo(longCorridor.To).Abs();
        bool horizontal = lineDirAbs.X > lineDirAbs.Y;
        List<RoomNeighborDirection> directions = horizontal 
            ? [RoomNeighborDirection.Left, RoomNeighborDirection.Right] : [RoomNeighborDirection.Up, RoomNeighborDirection.Down];
        

        float baseSize = Mathf.Lerp(Settings.MinBaseRoomSize, Settings.MaxBaseRoomSize, State.Rng.RandfRange(0f, 0.5f));
        var size = ProceduralGeometry.AspectWiseRandomSize(
            State.Rng,
            Settings.BaseRoomAspect,
            Settings.MaxRoomAspectDeviation,
            baseSize,
            Settings.MaxRoomSizeDeviation
        );
        var room = new ProceduralRoom(from.Lerp(longCorridor.To, 0.5f) - size / 2, size);
        room.ConnectionDirections.AddRange(directions);
        room.CorridorLines = [longCorridor];
        State.CorridorRooms.Add(room);
    
        // State.CorridorLines.Remove(longCorridor);

        room.Rect.Position += Vector2.FromAngle(State.Rng.Randf() * Mathf.Pi * 2) * State.Rng.RandfRange(0, 4);
        Log.Verbose("> Added in-between room {Index}!", State.CorridorRooms.IndexOf(room));
    }

    // private void EnsureCorridorsNotEmpty()
    // {
    //     var obstacleRectangles = State.CorridorRooms.Concat(State.MainRooms).Select(room => room.Rect).ToList();
    //
    //     foreach (var line in State.CorridorLines.ToList())
    //     {
    //         var longCorridorLines = ProceduralGeometry.GetVisibleSegments(line, obstacleRectangles);
    //         longCorridorLines =
    //             longCorridorLines.Where(l => l.From.DistanceTo(l.To) > Settings.MaxCorridorLength).ToList();
    //         if (longCorridorLines.Count <= 0)
    //             continue;       
    //         foreach (var longCorridor in longCorridorLines)
    //             SolveLongCorridor(longCorridor);
    //     }
    // }

    private void FixOverlappingCorridorLines()
    {
        var obstacleRectangles = State.CorridorRooms.Concat(State.MainRooms).Select(room => room.Rect).ToList();
        foreach (var line in State.CorridorLines.ToList())
        {
            var corridorLines = ProceduralGeometry.GetVisibleSegments(line, obstacleRectangles);
            State.CorridorLines.Remove(line);
            State.CorridorLines.AddRange(corridorLines);
        }
    }


    // This step attempts to move corridor rooms that lay too far away from its corridor line
    private async Task FixCorridorRoomsPlacement()
    {
        foreach (var corridor in State.CorridorRooms)
        {
            var center = corridor.Rect.GetCenter();
            foreach (var line in corridor.CorridorLines) 
            {
                var nearestPoint = line.GetNearestPoint(center);
                if (center.DistanceTo(nearestPoint) <= Settings.CorridorMaxDistanceToCenter)
                    continue;
                
                var direction = center.DirectionTo(nearestPoint);
                while (center.DistanceTo(nearestPoint) > Settings.CorridorMaxDistanceToCenter)
                {
                    corridor.Rect.Position += direction;
                    center = corridor.Rect.GetCenter();
                    await GDTask.Delay(200);
                }

            }
            Log.Verbose("> Found room {Idx} too far away from the line!", State.CorridorRooms.IndexOf(corridor));
        }
    }


    public void SeparateRooms()
    {
        foreach (var room in State.AllRooms)
        {
            var others = State.AllRooms.Where(r => r != room && r.Center.DistanceTo(room.Center) < 300).ToArray();
            while (others.Any(r => r.Rect.Intersects(room.Rect)))
            {
                var intersecting = others.First(r => r.Rect.Intersects(room.Rect));
                var dir = (room.Rect.GetCenter() - intersecting.Rect.GetCenter()).Sign();
                if (dir == Vector2.Zero)
                    dir = Vector2.FromAngle(State.Rng.Randf() * Mathf.Pi * 2).Sign();

                room.Rect.Position += dir;
            }
        }
    }

    public override async GDTask Perform()
    {
        CreateInBetweenRooms();
        State.AllRooms = [.. State.MainRooms, .. State.CorridorRooms];
        State.AllRooms = State.AllRooms.OrderBy(room => room.ProgressBias).ToList();
        
        await CreateCorridorLines();
        // SelectCorridorRooms();
        // EnsureCorridorsNotEmpty();

        // await FixCorridorRoomsPlacement();

        // FixOverlappingCorridorLines();
        
        


        SeparateRooms();
    }
}