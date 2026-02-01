using System.Collections.Generic;
using System.Linq;
using Casiland.Common;
using Casiland.Systems.ProceduralGen.Algorithms;
using Godot;
using TriangleNet;
using TriangleNet.Geometry;

namespace Casiland.Systems.ProceduralGen.Steps;

public class GenerateConnectionsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => "Generating room connections";

    public List<LineSegment> CalculateDelaunayConnections(List<ProceduralRoom> rooms)
    {
        var triangulator = new TriangleNet.Meshing.Algorithm.Dwyer();
        var points = rooms.Select(r => new Vertex(r.Rect.GetCenter().X, r.Rect.GetCenter().Y));
        var mesh = triangulator.Triangulate(points.ToArray(), new Configuration());
        
        
        var segments = new List<LineSegment>();
        var verts = mesh.Vertices.ToArray();
        foreach (var edge in mesh.Edges)
        {

            var from = VecFromVert(verts[edge.P0]);
            var to = VecFromVert(verts[edge.P1]);
            segments.Add(new LineSegment(from, to));
        }

        return segments;

        Vector2 VecFromVert(Vertex vert) => new((float)vert.X, (float)vert.Y);
    }


    /* ============================
     * Minimal Spanning Tree
     * ============================ */

    public void ConstructMst(List<LineSegment> delaunayConnections)
    {
        State.MinimumSpanningTree.Clear();

        var mst = KruskalMST.ComputeMST(delaunayConnections);
        State.MinimumSpanningTree = mst;
    }


    /* ============================
     * Fill Room Connections
     * ============================ */

    public void FillRoomConnections()
    {
        State.PointToRoom =  new Dictionary<Vector2I, ProceduralRoom>();
        foreach (var room in State.MainRooms)
        {
            foreach (var e in State.MinimumSpanningTree.Where(e => room.Position.DistanceTo(e.From) <= 2))
            {
                State.PointToRoom[e.From] = room;

                foreach (var other in State.MainRooms.Where(other => other.Position.DistanceTo(e.To) <= 2))
                {
                    State.PointToRoom[e.To] = other;

                    room.AddConnection(other);
                    other.AddConnection(room);
                }
            }
        }
    }


    /* ============================
     * Add Loops
     * ============================ */

    public void AddLoops()
    {
        var leafRooms = State.MainRooms
            .Where(r => r.GetConnectionsCount() == 1)
            .ToList();

        if (leafRooms.Count == 0) return;

        var startRoom = leafRooms.PickRandom();
        leafRooms.Remove(startRoom);

        var bossRoom = leafRooms.PickRandom();
        leafRooms.Remove(bossRoom);

        var potential = new List<ProceduralRoom>(State.MainRooms);
        potential.Remove(startRoom);
        potential.Remove(bossRoom);

        for (int i = 0; i < Settings.LoopCount; i++)
        {
            if (potential.Count < 2) break;

            var room = potential.PickRandom();
            potential.Remove(room);

            bool valid = false;
            ProceduralRoom other = null;

            while (!valid && potential.Count > 0)
            {
                other = potential.PickRandom();
                potential.Remove(other);

                valid = !room.HasConnection(other) &&
                        room.Position.DistanceTo(other.Position) <= Settings.MinRoomDistance;
            }

            if (!valid) break;
            
            room.AddConnection(other);
            other.AddConnection(room);
        }
    }

    public override void Perform()
    {
        var connections = CalculateDelaunayConnections(State.MainRooms);
        ConstructMst(connections);
        FillRoomConnections();
        AddLoops();
    }
}