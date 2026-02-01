using System.Collections.Generic;
using System.Linq;
using Casiland.Common;
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

    private LineSegment CreateDirectCorridor(LineSegment edge, Vector2 axis, Rect2 overlap)
    {
        var inv = Vector2.One - axis;
        var from = (Vector2)edge.From * axis + overlap.GetCenter() * inv;
        var dest = (Vector2)edge.To * axis + overlap.GetCenter() * inv;

        return new LineSegment(from, dest);
    }

    private LineSegment[] CreateSShapedCorridor(LineSegment edge, Vector2 axis, Vector2 dir)
    {
        var from = edge.From;
        var to = edge.To;
        var inv = Vector2.One - axis.Abs();

        float bias = State.Rng.RandfRange(0.45f, 0.55f);

        var c1 = from + dir * axis * bias;
        var c2 = from + (dir * axis * bias + dir * inv);

        return [new LineSegment(from, c1), new LineSegment(c1, c2), new LineSegment(c2, to)];
    }

    private LineSegment[] CreateCornerCorridor(Rect2 fromRoom, LineSegment edge, Vector2 axis, Vector2 dir)
    {
        var dirs = new List<Vector2>
        {
            Vector2.Right * Mathf.Abs(Mathf.Sign(dir.X)),
            Vector2.Down * Mathf.Abs(Mathf.Sign(dir.Y))
        };

        dirs.Sort((a, b) =>
        {
            var ap = fromRoom.GetCenter() + a * (fromRoom.Size / 2f);
            var bp = fromRoom.GetCenter() + b * (fromRoom.Size / 2f);
            return ap.DistanceTo(edge.To) < bp.DistanceTo(edge.To) ? -1 : 1;
        });

        var selected = dirs[0];
        var corner = edge.From + dir * selected;

        return [new LineSegment(edge.From, corner), new LineSegment(corner, edge.To)];
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
                var line = CreateDirectCorridor(edge, axis, overlap);
                fromRoom.CorridorLines.Add(line);
                toRoom.CorridorLines.Add(line);
                State.CorridorLines.Add(line);
                continue;
            }

            if (length <= 0)
            {
                length = (fromRoom.Rect.GetCenter() * axis)
                    .DistanceTo(toRoom.Rect.GetCenter() * axis);
            }

            LineSegment[] lines;

            if (length <= Settings.MaximumCornerCorridorOverlapLength)
                lines = CreateCornerCorridor(fromRoom.Rect, edge, axis, dir);
            else
                lines = CreateSShapedCorridor(edge, axis, dir);

            State.CorridorLines.AddRange(lines);
            fromRoom.CorridorLines.Add(lines[0]);
            toRoom.CorridorLines.Add(lines[^1]);
            
        }
    }


    public void CreateCorridors()
    {
        State.CorridorRooms.Clear();

        foreach (var edge in State.CorridorLines)
        {
            foreach (var room in State.OtherRooms)
            {
                if (!State.CorridorRooms.Contains(room) &&
                    ProceduralGeometry.EdgeIntersectsRect(edge.From, edge.To, room.Rect))
                {
                    State.CorridorRooms.Add(room);
                }
            }
        }

        State.AllRooms = [.. State.MainRooms, .. State.CorridorRooms];
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
    private void FixCorridorRoomsPlacement()
    {
        foreach (var corridor in State.CorridorRooms)
        foreach (var intersectingLine in
                 State.CorridorLines.Where(line => ProceduralGeometry.EdgeIntersectsRect(line, corridor.Rect)))
        {
            var dir = ((Vector2)intersectingLine.From).DirectionTo(intersectingLine.To).Sign().Abs();
            var invertedDir = Vector2.One - dir;

            var lineMediumPoint = intersectingLine.From * invertedDir + corridor.Position * dir;
            var axisSize = (corridor.Size / 2 * invertedDir).Sum();

            float distanceToMediumPoint = corridor.Position.DistanceTo(lineMediumPoint);
            if (distanceToMediumPoint < axisSize - Settings.MinimumDirectCorridorOverlapLength)
                continue;

            float deviationAmount = distanceToMediumPoint - axisSize + Settings.MinimumDirectCorridorOverlapLength;
            var lastPos = corridor.Rect.Position;
            
            corridor.Rect.Position += corridor.Position.DirectionTo(lineMediumPoint) * deviationAmount;
            bool intersects = false;
            var rect = corridor.Rect.Grow(2);
            foreach (var room in State.CorridorRooms)
            {
                if (room == corridor) continue;
                if (!room.Rect.Intersects(rect)) continue;
                intersects = true;
                break;
            }

            if (intersects) // Roll back if we hit something
                corridor.Rect.Position = lastPos;
            
            Log.Verbose("> Found room {Idx} too far away from the line!", State.CorridorRooms.IndexOf(corridor));
        }
    }

    public override void Perform()
    {
        CreateCorridorLines();
        CreateCorridors();
        EnsureCorridorsNotEmpty();

        FixCorridorRoomsPlacement();

        FixOverlappingCorridorLines();
    }
}