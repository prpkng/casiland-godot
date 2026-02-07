using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Casiland.Common;
using Fractural.Tasks;
using Godot;
using Serilog;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlaceCorridorsStep(GenerationState state, ProceduralGenerationSettings settings)
    : GenerationStep(state, settings)
{

    public override string StateDescription => $"Placing {State.CorridorLines?.Count} corridor lines and {State.CorridorRooms?.Count} corridor rooms";

    private static Rect2 GetRectOverlapOnAxis(Rect2 from, Rect2 to, Vector2 axis)
    {
        var inv = Vector2.One - axis.Abs();
        var r1 = new Rect2(from.Position * inv, from.Size);
        var r2 = new Rect2(to.Position * inv, to.Size);
        return r1.Intersection(r2);
    }
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
    
    #region === CORRIDOR CREATION ALGORITHMS 
    /// <summary>
    /// Tries to create a direct corridor between the two rooms and returns null if there is already another corridor in the way
    /// </summary>
    private LineSegment? CreateDirectCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom, Vector2 axis, Rect2 overlap)
    {
        var inv = Vector2.One - axis;
        var from = fromRoom.Center * axis + overlap.GetCenter() * inv;
        var dest = toRoom.Center * axis + overlap.GetCenter() * inv;

        var dir = (dest - from).Sign();

        if (fromRoom.ConnectionDirections.Contains(VecToDirDict[dir]))
            return null;

        fromRoom.ConnectionDirections.Add(VecToDirDict[dir]);
        toRoom.ConnectionDirections.Add(VecToDirDict[-dir]);

        return new LineSegment(from, dest);
    }
    
    /// <summary>
    /// Tries to create a corner-shaped corridor between the two given rooms and return null if the all possible corners are blocked
    /// </summary>
    private LineSegment[] CreateCornerCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom)
    {
        var dir = toRoom.Center - fromRoom.Center;
        var vecs = new List<Vector2>
        {
            Vector2.Right * Mathf.Sign(dir.X),
            Vector2.Down * Mathf.Sign(dir.Y)
        };

        Vector2 destDir;

        // Try first pattern
        if (!fromRoom.ConnectionDirections.Contains(VecToDirDict[vecs[0]]) && 
            !toRoom.ConnectionDirections.Contains(VecToDirDict[-vecs[1]]) &&
            Mathf.Abs(dir.X) > fromRoom.Size.X + 4)
            destDir = vecs[0];
        else if (
            !fromRoom.ConnectionDirections.Contains(VecToDirDict[vecs[1]]) && 
            !toRoom.ConnectionDirections.Contains(VecToDirDict[-vecs[0]]) &&
            Mathf.Abs(dir.Y) > fromRoom.Size.Y + 4)
            destDir = vecs[1];
        else 
            return null; // Fail to find a viable corner segment


        // var destDir = fromRoom.ConnectionDirections.Contains(_vecToDirDict[vecs[0]]) ? vecs[1] : vecs[0];

        var corner = fromRoom.Center + dir * destDir.Abs();
        fromRoom.ConnectionDirections.Add(VecToDirDict[(corner - fromRoom.Center).Sign()]);
        toRoom.ConnectionDirections.Add(VecToDirDict[(corner - toRoom.Center).Sign()]);


        return [new LineSegment(fromRoom.Center, corner), new LineSegment(corner, toRoom.Center)];
    }
    
    /// <summary>
    /// Creates an S-shaped corridor between the two given rooms
    /// </summary>
    private LineSegment[] CreateSShapedCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom, Vector2 axis)
    {
        var from = fromRoom.Center;
        var to = toRoom.Center;
        var dir = to - from;



        var inv = Vector2.One - axis.Abs();

        float bias = State.Rng.RandfRange(0.45f, 0.55f);

        var c1 = from + dir * axis * bias;
        var c2 = to - dir * axis * (1f-bias);

        fromRoom.ConnectionDirections.Add(VecToDirDict[(c1 - from).Sign()]);
        toRoom.ConnectionDirections.Add(VecToDirDict[(c2 - to).Sign()]);

        return [new LineSegment(from, c1), new LineSegment(c1, c2), new LineSegment(c2, to)];
    }
    
    #endregion
    

    public async GDTask CreateCorridorLines()
    {
        State.CorridorLines.Clear();

        foreach (var edge in State.MinimumSpanningTree)
        {
            var fromRoom = State.PointToRoom[edge.From];
            var toRoom = State.PointToRoom[edge.To];

            fromRoom.CorridorLines ??= [];
            toRoom.CorridorLines ??= [];

            var dir = fromRoom.Center - toRoom.Center;
            bool horizontal = Mathf.Abs(dir.X) > Mathf.Abs(dir.Y);

            var axis = horizontal ? Vector2.Right : Vector2.Down;

            var overlap = GetRectOverlapOnAxis(fromRoom.Rect, toRoom.Rect, axis);
            float length = (overlap.Size * (Vector2.One - axis)).Abs().Sum();

            LineSegment[] generateLines()
            {
                if (length > Settings.MinimumDirectCorridorOverlapLength)
                {
                    var line = CreateDirectCorridor(fromRoom, toRoom, axis, overlap);
                    if (line.HasValue) return [line.Value];
                }

                if (length <= Settings.MaximumCornerCorridorOverlapLength)
                {
                    var result = CreateCornerCorridor(fromRoom, toRoom);
                    if (result != null) return result;
                }

                return CreateSShapedCorridor(fromRoom, toRoom, axis);
            }

            var lines = generateLines();

            State.CorridorLines.AddRange(lines);
            State.CorridorLineGroups.Add(lines);
            fromRoom.CorridorLines.Add(lines[0]);
            toRoom.CorridorLines.Add(lines[^1]);
            await GDTask.Delay(200);
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

    private void EnsureCorridorsNotEmpty()
    {
        var obstacleRectangles = State.CorridorRooms.Concat(State.MainRooms).Select(room => room.Rect).ToList();

        foreach (var line in State.CorridorLines.ToList())
        {
            var longCorridorLines = ProceduralGeometry.GetVisibleSegments(line, obstacleRectangles);
            longCorridorLines =
                longCorridorLines.Where(l => l.From.DistanceTo(l.To) > Settings.MaximumCorridorLength).ToList();
            if (longCorridorLines.Count <= 0)
                continue;       
            foreach (var longCorridor in longCorridorLines)
                SolveLongCorridor(longCorridor);
        }
    }

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
        await CreateCorridorLines();
        SelectCorridorRooms();
        EnsureCorridorsNotEmpty();

        await FixCorridorRoomsPlacement();

        // FixOverlappingCorridorLines();
        
        
        State.AllRooms = [.. State.MainRooms, .. State.CorridorRooms];


        SeparateRooms();
    }
}