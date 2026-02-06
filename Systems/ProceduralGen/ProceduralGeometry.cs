using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Casiland.Systems.ProceduralGen;

public record struct LineSegment
{
    public Vector2I From;
    public Vector2I To;

    public LineSegment(Vector2 from, Vector2 to)
    {
        From = (Vector2I)from;
        To = (Vector2I)to;
    }
    
    public void Deconstruct(out Vector2I from, out Vector2I to) {
        from = From;
        to = To;
    }
    public Vector2 GetNearestPoint(Vector2 point)
    {
        Vector2 segmentVec = To - From;
        float segmentLenSq = segmentVec.LengthSquared();
        
        if (segmentLenSq < 1e-8f)
            return From;
        
        float t = (point - From).Dot(segmentVec) / segmentLenSq;
        t = Mathf.Clamp(t, 0f, 1f);
        
        return From + t * segmentVec;
    }
    
}

public static class ProceduralGeometry
{

    public static void SetCells(this TileMapLayer tilemap, IEnumerable<Vector2I> tiles,
        int sourceId, Vector2I coords)
    {
        foreach (var tile in tiles)
            tilemap.SetCell(tile, sourceId, coords);
    }
    
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


    public static bool EdgeIntersectsRect(LineSegment line, Rect2 rect) => EdgeIntersectsRect(line.From, line.To, rect);
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
    public static List<Vector2I> BresenhamLineWidth(Vector2I start, Vector2I end, int width, bool useManhattan = false)
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

        return result.ToList();
    }
    public static List<LineSegment> GetVisibleSegments(LineSegment segment, IReadOnlyList<Rect2> rects)
    {
        var (a, b) = segment;
        Vector2 dir = b - a;
        float len = dir.Length();
        var result = new List<LineSegment>();

        if (len < 1e-8f)
            return result;

        dir /= len; // normalize

        // Collect covered t-intervals
        List<(float t0, float t1)> covered = new();
        foreach (var rect in rects)
        {
            if (RaySegmentAABBRange(a, dir, len, rect, out float t0, out float t1))
                covered.Add((t0, t1));
        }

        if (covered.Count == 0)
        {
            // Entire line visible
            result.Add(new LineSegment(a, b));
            return result;
        }

        // Merge covered intervals
        covered.Sort((x, y) => x.t0.CompareTo(y.t0));
        List<(float t0, float t1)> merged = new();

        float c0 = covered[0].t0;
        float c1 = covered[0].t1;
        for (int i = 1; i < covered.Count; i++)
        {
            var (t0, t1) = covered[i];
            if (t0 <= c1)
                c1 = Mathf.Max(c1, t1);
            else
            {
                merged.Add((c0, c1));
                c0 = t0;
                c1 = t1;
            }
        }
        merged.Add((c0, c1));

        // Now compute the EXPOSED segments = complement of merged intervals
        float previousEnd = 0f;

        foreach (var (t0, t1) in merged)
        {
            if (t0 > previousEnd)
            {
                Vector2 pA = a + dir * (previousEnd * len);
                Vector2 pB = a + dir * (t0 * len);
                result.Add(new LineSegment(pA, pB));
            }
            previousEnd = Mathf.Max(previousEnd, t1);
        }

        // Final tail part, if visible
        if (previousEnd < 1f)
        {
            Vector2 pA = a + dir * (previousEnd * len);
            Vector2 pB = b;
            result.Add(new LineSegment(pA, pB));
        }

        return result;
    }

    // Intersection helper: returns t-range where the line is inside the rectangle
    private static bool RaySegmentAABBRange(
        Vector2 origin, Vector2 dir, float length, Rect2 box, 
        out float tMin, out float tMax)
    {
        float t0 = 0f;
        float t1 = length;

        if (!SlabIntersect(origin.X, dir.X, box.Position.X, box.End.X, ref t0, ref t1) ||
            !SlabIntersect(origin.Y, dir.Y, box.Position.Y, box.End.Y, ref t0, ref t1))
        {
            tMin = tMax = 0;
            return false;
        }

        tMin = t0 / length;
        tMax = t1 / length;
        return tMax >= 0 && tMin <= 1;
    }

    private static bool SlabIntersect(float o, float d, float min, float max, ref float t0, ref float t1)
    {
        if (Mathf.Abs(d) < 1e-8f)
        {
            return o >= min && o <= max;
        }

        float invD = 1f / d;
        float tNear = (min - o) * invD;
        float tFar  = (max - o) * invD;

        if (tNear > tFar)
            (tNear, tFar) = (tFar, tNear);

        if (tNear > t1 || tFar < t0)
            return false;

        t0 = Mathf.Max(t0, tNear);
        t1 = Mathf.Min(t1, tFar);
        return true;
    }
    
}
