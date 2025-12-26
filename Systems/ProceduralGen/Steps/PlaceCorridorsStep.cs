using System.Collections.Generic;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlaceCorridorsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    
    private Rect2 CheckForDirectLine(Rect2 from, Rect2 to, Vector2 axis)
    {
        var inv = Vector2.One - axis.Abs();
        var r1 = new Rect2(from.Position * inv, from.Size);
        var r2 = new Rect2(to.Position * inv, to.Size);
        return r1.Intersection(r2);
    }

    private void CreateDirectCorridor(LineSegment edge, Vector2 axis, Rect2 overlap)
    {
        var inv = Vector2.One - axis;
        var from = (Vector2)edge.From * axis + overlap.GetCenter() * inv;
        var dest = (Vector2)edge.To * axis + overlap.GetCenter() * inv;

        State.CorridorLines.Add(new LineSegment(from, dest));
    }

    private void CreateSShapedCorridor(LineSegment edge, Vector2 axis, Vector2 dir)
    {
        var from = edge.From;
        var to = edge.To;
        var inv = Vector2.One - axis.Abs();

        float bias = State.Rng.RandfRange(0.45f, 0.55f);

        var c1 = from + dir * axis * bias;
        var c2 = from + (dir * axis * bias + dir * inv);

        State.CorridorLines.Add(new LineSegment(from, c1));
        State.CorridorLines.Add(new LineSegment(c1, c2));
        State.CorridorLines.Add(new LineSegment(c2, to));
    }

    private void CreateCornerCorridor(Rect2 fromRoom, LineSegment edge, Vector2 axis, Vector2 dir)
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

        State.CorridorLines.Add(new LineSegment(edge.From, corner));
        State.CorridorLines.Add(new LineSegment(corner, edge.To));
    }


    public void CreateCorridorLines()
    {
        State.CorridorLines.Clear();

        foreach (var edge in State.MinimumSpanningTree)
        {
            var fromRoom = State.PointToRoom[edge.From];
            var toRoom = State.PointToRoom[edge.To];

            var dir = edge.To - edge.From;
            bool horizontal = Mathf.Abs(dir.X) > Mathf.Abs(dir.Y);

            var axis = horizontal ? Vector2.Right : Vector2.Down;

            var overlap = CheckForDirectLine(fromRoom.Rect, toRoom.Rect, axis);
            float length = (overlap.Size * (Vector2.One - axis)).Abs().X +
                           (overlap.Size * (Vector2.One - axis)).Abs().Y;

            if (length > Settings.MinimumDirectCorridorOverlapLength)
            {
                CreateDirectCorridor(edge, axis, overlap);
                continue;
            }

            if (length <= 0)
            {
                length = (fromRoom.Rect.GetCenter() * axis)
                    .DistanceTo(toRoom.Rect.GetCenter() * axis);
            }

            if (length <= Settings.MaximumCornerCorridorOverlapLength)
                CreateCornerCorridor(fromRoom.Rect, edge, axis, dir);
            else
                CreateSShapedCorridor(edge, axis, dir);
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
    }

    public override void Perform()
    {
        CreateCorridorLines();
        CreateCorridors();
    }
}