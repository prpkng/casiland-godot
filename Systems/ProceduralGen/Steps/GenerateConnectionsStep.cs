using System;
using System.Collections.Generic;
using System.Linq;
using Casiland.Common;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
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
        State.PointToRoom = new Dictionary<Vector2I, ProceduralRoom>();
        foreach (var room in State.MainRooms)
        {
            foreach (var e in State.MinimumSpanningTree.Where(e => room.Center.DistanceTo(e.From) <= 2))
            {
                State.PointToRoom[e.From] = room;

                foreach (var other in State.MainRooms.Where(other => other.Center.DistanceTo(e.To) <= 2))
                {
                    State.PointToRoom[e.To] = other;

                    room.AddConnection(other);
                    other.AddConnection(room);
                }
            }
        }
    }

    public void UpdateMstLines()
    {
        var oldPointToRoom = State.PointToRoom;
        State.PointToRoom = new Dictionary<Vector2I, ProceduralRoom>();

        for (int index = 0; index < State.MinimumSpanningTree.Count; index++)
        {
            var edge = State.MinimumSpanningTree[index];
            var fromRoom = oldPointToRoom[edge.From];
            var toRoom = oldPointToRoom[edge.To];
            var direction = edge.FromF.DirectionTo(edge.ToF);
            var newFrom = fromRoom.Rect.CastTowardsPerimeter(direction).RoundToInt();
            State.PointToRoom[newFrom] = fromRoom;
            var newTo = toRoom.Rect.CastTowardsPerimeter(-direction).RoundToInt();
            State.PointToRoom[newTo] = toRoom;
            
            edge.From = newFrom;
            edge.To = newTo;
            
            State.MinimumSpanningTree[index] = edge;
        }
    }

    

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

    public override async GDTask Perform()
    {
        var connections = CalculateDelaunayConnections(State.MainRooms);
        ConstructMst(connections);
        FillRoomConnections();
        
        UpdateMstLines();

        CreateInBetweenRooms();
    }
}