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

    private Rect2 CheckForDirectLine(Rect2 from, Rect2 to, Vector2 axis)
    {
        var inv = Vector2.One - axis.Abs();
        var r1 = new Rect2(from.Position * inv, from.Size);
        var r2 = new Rect2(to.Position * inv, to.Size);
        return r1.Intersection(r2);
    }
    private static readonly Dictionary<Vector2, Directions> _vecToDirDict = new()
    {
        { new Vector2(1, -1), Directions.Up },
        { new Vector2(-1, -1), Directions.Up },
        { Vector2.Up, Directions.Up },
        { new Vector2(1, 1), Directions.Down },
        { new Vector2(-1, 1), Directions.Down },
        { Vector2.Down, Directions.Down },
        { Vector2.Left, Directions.Left },
        { Vector2.Right, Directions.Right }
    };


    private LineSegment CreateDirectCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom, Vector2 axis, Rect2 overlap)
    {
        var inv = Vector2.One - axis;
        var from = fromRoom.Center * axis + overlap.GetCenter() * inv;
        var dest = toRoom.Center * axis + overlap.GetCenter() * inv;

        var dir = (dest - from).Sign();
        fromRoom.ConnectionDirections.Add(_vecToDirDict[dir]);
        toRoom.ConnectionDirections.Add(_vecToDirDict[-dir]);

        return new LineSegment(from, dest);
    }

    private LineSegment[] CreateSShapedCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom, Vector2 axis, Vector2 dir)
    {
        var from = fromRoom.Center;
        var to = toRoom.Center;
        var inv = Vector2.One - axis.Abs();

        float bias = State.Rng.RandfRange(0.45f, 0.55f);

        var c1 = from + dir * axis * bias;
        var c2 = from + (dir * axis * bias + dir * inv);

        toRoom.ConnectionDirections.Add(_vecToDirDict[(c2 - from).Sign()]);
        toRoom.ConnectionDirections.Add(_vecToDirDict[(c1 - from).Sign()]);

        return [new LineSegment(from, c1), new LineSegment(c1, c2), new LineSegment(c2, to)];
    }
    private LineSegment[] CreateCornerCorridor(ProceduralRoom fromRoom, ProceduralRoom toRoom)
    {
        var dir = toRoom.Center - fromRoom.Center;
        var vecs = new List<Vector2>
        {
            Vector2.Right * Mathf.Sign(dir.X),
            Vector2.Down * Mathf.Sign(dir.Y)
        };

        var destDir = fromRoom.ConnectionDirections.Contains(_vecToDirDict[vecs[0]]) ? vecs[1] : vecs[0];

        var corner = fromRoom.Center + dir * destDir.Abs();
        fromRoom.ConnectionDirections.Add(_vecToDirDict[(corner - fromRoom.Center).Sign()]);
        toRoom.ConnectionDirections.Add(_vecToDirDict[(corner - toRoom.Center).Sign()]);


        return [new LineSegment(fromRoom.Center, corner), new LineSegment(corner, toRoom.Center)];
    }


    public void CreateCorridorLines()
    {
        State.CorridorLines.Clear();

        foreach (var edge in State.MinimumSpanningTree)
        {
            var fromRoom = State.PointToRoom[edge.From];
            var toRoom = State.PointToRoom[edge.To];

            fromRoom.CorridorLines ??= [];
            toRoom.CorridorLines ??= [];

            var dir = edge.To - edge.From;
            bool horizontal = Mathf.Abs(dir.X) > Mathf.Abs(dir.Y);

            var axis = horizontal ? Vector2.Right : Vector2.Down;

            var overlap = CheckForDirectLine(fromRoom.Rect, toRoom.Rect, axis);
            float length = (overlap.Size * (Vector2.One - axis)).Abs().X +
                           (overlap.Size * (Vector2.One - axis)).Abs().Y;

            if (length > Settings.MinimumDirectCorridorOverlapLength)
            {
                var line = CreateDirectCorridor(fromRoom, toRoom, axis, overlap);
                fromRoom.CorridorLines.Add(line);
                toRoom.CorridorLines.Add(line);
                State.CorridorLines.Add(line);
                State.CorridorLineGroups.Add([line]);
                continue;
            }

            if (length <= 0)
            {
                length = (fromRoom.Rect.GetCenter() * axis)
                    .DistanceTo(toRoom.Rect.GetCenter() * axis);
            }

            LineSegment[] lines;

            if (length <= Settings.MaximumCornerCorridorOverlapLength)
                lines = CreateCornerCorridor(fromRoom, toRoom);
            else
                lines = CreateSShapedCorridor(fromRoom, toRoom, axis, dir);

            State.CorridorLines.AddRange(lines);
            State.CorridorLineGroups.Add(lines);
            fromRoom.CorridorLines.Add(lines[0]);
            toRoom.CorridorLines.Add(lines[^1]);
            
        }
    }


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

                room.CorridorLines.AddRange(validEdges);

                State.CorridorRooms.Add(room);
            }
        }

    }

    private void SolveLongCorridor(LineSegment longCorridor)
    {
        Log.Verbose("> Found corridor with length {Len} which exceeds the maximum length!",
            longCorridor.From.DistanceTo(longCorridor.To));

        float width = Mathf.Lerp(Settings.MinRoomWidth, Settings.MaxRoomWidth, State.Rng.RandfRange(0.25f, .75f));
        float height = Mathf.Lerp(Settings.MinRoomHeight, Settings.MaxRoomHeight, State.Rng.RandfRange(0.25f, .75f));

        var from = (Vector2)longCorridor.From;
        var lineDir = from.DirectionTo(longCorridor.To);

        var size = new Vector2(width, height);
        var room = new ProceduralRoom(from.Lerp(longCorridor.To, 0.5f) - size / 2, size);
        State.CorridorRooms.Add(room);

        State.CorridorLines.Remove(longCorridor);

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
        CreateCorridorLines();
        SelectCorridorRooms();
        // EnsureCorridorsNotEmpty();

        await FixCorridorRoomsPlacement();

        // FixOverlappingCorridorLines();
        
        
        State.AllRooms = [.. State.MainRooms, .. State.CorridorRooms];


        SeparateRooms();
    }
}