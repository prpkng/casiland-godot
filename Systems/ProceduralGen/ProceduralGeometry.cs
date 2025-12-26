using System.Linq;
using Godot;

namespace Casiland.Systems.ProceduralGen;

public struct LineSegment(Vector2 a, Vector2 b)
{
    public Vector2I From = (Vector2I)a;
    public Vector2I To = (Vector2I)b;
}

public static class ProceduralGeometry
{
    public static Vector2 FindLineIntersection(Vector2 p1A, Vector2 p1B, Vector2 p2A, Vector2 p2B)
    {
        float denom =
            (p1A.X - p1B.X) * (p2A.Y - p2B.Y) -
            (p1A.Y - p1B.Y) * (p2A.X - p2B.X);

        if (Mathf.Abs(denom) < 0.0001f)
            return Vector2.Zero;

        float t =
            ((p1A.X - p2A.X) * (p2A.Y - p2B.Y) -
             (p1A.Y - p2A.Y) * (p2A.X - p2B.X)) / denom;

        float u =
            -((p1A.X - p1B.X) * (p1A.Y - p2A.Y) -
              (p1A.Y - p1B.Y) * (p1A.X - p2A.X)) / denom;

        if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            return p1A + t * (p1B - p1A);

        return Vector2.Zero;
    }


    public static bool EdgeIntersectsRect(Vector2 a, Vector2 b, Rect2 rect)
    {
        var tl = rect.Position;
        var tr = rect.Position + new Vector2(rect.Size.X, 0);
        var br = rect.Position + rect.Size;
        var bl = rect.Position + new Vector2(0, rect.Size.Y);

        bool[] lines =
        {
            FindLineIntersection(a, b, tl, tr) != Vector2.Zero,
            FindLineIntersection(a, b, bl, br) != Vector2.Zero,
            FindLineIntersection(a, b, tr, br) != Vector2.Zero,
            FindLineIntersection(a, b, tl, bl) != Vector2.Zero
        };

        return lines.Any(x => x);
    }
}
