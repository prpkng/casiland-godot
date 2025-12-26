namespace Casiland.Systems.ProceduralGen.Algorithms;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class KruskalMST
{
    /// <summary>
    /// Computes the Minimum Spanning Tree from a collection of line segments.
    /// Each unique Vector2I point becomes a node in the graph.
    /// </summary>
    public static List<LineSegment> ComputeMST(IEnumerable<LineSegment> segments)
    {
        // Extract all unique vertices
        var vertexSet = new HashSet<Vector2I>();
        foreach (var seg in segments)
        {
            vertexSet.Add(seg.From);
            vertexSet.Add(seg.To);
        }

        // Map each unique point to an index
        var vertices = vertexSet.ToList();
        var indexMap = new Dictionary<Vector2I, int>();
        for (int i = 0; i < vertices.Count; i++)
            indexMap[vertices[i]] = i;

        // Convert segments to weighted edges
        var edges = segments
            .Select(s => new Edge(
                indexMap[s.From],
                indexMap[s.To],
                s))
            .OrderBy(e => e.Weight)
            .ToList();

        // Prepare Union-Find
        var uf = new UnionFind(vertices.Count);

        // Kruskal result list
        var mst = new List<LineSegment>();

        foreach (var edge in edges)
        {
            if (uf.Find(edge.A) != uf.Find(edge.B))
            {
                uf.Union(edge.A, edge.B);
                mst.Add(edge.Segment);
            }
        }

        return mst;
    }

    /// <summary>
    /// Internal weighted-edge representation
    /// </summary>
    private readonly struct Edge
    {
        public readonly int A;
        public readonly int B;
        public readonly float Weight;
        public readonly LineSegment Segment;

        public Edge(int a, int b, LineSegment segment)
        {
            A = a;
            B = b;
            Segment = segment;
            Weight = segmentWeight(segment);
        }

        private static float segmentWeight(LineSegment s)
        {
            return s.From.DistanceTo(s.To);
        }
    }

    /// <summary>
    /// Union-Find (Disjoint Set Union) for Kruskal
    /// </summary>
    private class UnionFind
    {
        private readonly int[] parent;
        private readonly int[] rank;

        public UnionFind(int size)
        {
            parent = new int[size];
            rank = new int[size];

            for (int i = 0; i < size; i++)
                parent[i] = i;
        }

        public int Find(int x)
        {
            while (x != parent[x])
            {
                parent[x] = parent[parent[x]]; // Path compression
                x = parent[x];
            }
            return x;
        }

        public void Union(int x, int y)
        {
            int rx = Find(x);
            int ry = Find(y);

            if (rx == ry)
                return;

            // Union by rank
            if (rank[rx] < rank[ry])
                parent[rx] = ry;
            else if (rank[rx] > rank[ry])
                parent[ry] = rx;
            else
            {
                parent[ry] = rx;
                rank[rx]++;
            }
        }
    }
}
