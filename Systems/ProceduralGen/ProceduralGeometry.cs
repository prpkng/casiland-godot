using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

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
    
    public static void BresenhamLine(Vector2I start, Vector2I end, Array<Vector2I> points)
    {
        int x0 = start.X;
        int y0 = start.Y;
        int x1 = end.X;
        int y1 = end.Y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            points.Add(new Vector2I(x0, y0));

            if (x0 == x1 && y0 == y1)
                break;

            int e2 = err * 2;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

    }
    public static Array<Vector2I> BresenhamLine(Vector2I start, Vector2I end)
    {
        var points = new Array<Vector2I>();
        BresenhamLine(start, end, points);
        return points;
    }

    /// <summary>
    /// Generates a Bresenham line with custom width.
    /// Width >= 1. Width = 1 = normal single-pixel line.
    /// If useManhattan = false → square thickness (Chebyshev)
    /// If useManhattan = true  → diamond thickness (Manhattan)
    /// </summary>
    public static Array<Vector2I> BresenhamLineWidth(Vector2I start, Vector2I end, int width, bool useManhattan = false)
    {
        var points = new Array<Vector2I>();
        BresenhamLineWidth(points, start, end, width, useManhattan);
        return points;
    }
    
    /// <summary>
    /// Generates a Bresenham line with custom width.
    /// Width >= 1. Width = 1 = normal single-pixel line.
    /// If useManhattan = false → square thickness (Chebyshev)
    /// If useManhattan = true  → diamond thickness (Manhattan)
    /// </summary>
    public static void BresenhamLineWidth(Array<Vector2I> points, Vector2I start, Vector2I end, int width, bool useManhattan = false)
    {
        if (width < 1)
            width = 1;

        var result = new HashSet<Vector2I>();
        var centerLine = BresenhamLine(start, end);

        int radius = width / 2;

        foreach (var p in centerLine)
        {
            // Expand thickness around center line tile
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    Vector2I tile = new Vector2I(p.X + dx, p.Y + dy);

                    if (useManhattan)
                    {
                        // Diamond / manhattan
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) <= radius)
                            result.Add(tile);
                    }
                    else
                    {
                        // Square / chebyshev
                        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= radius)
                            result.Add(tile);
                    }
                }
            }
        }

        points.AddRange(result);
    }
}
