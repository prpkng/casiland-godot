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

    public void PickStartRoom()
    {
        var leafRooms = State.MainRooms
            .Where(r => r.GetConnectionsCount() == 1)
            .ToList();

        var startRoom = leafRooms.PickRandom(State.Rng);
        startRoom.RoomType = RoomTypes.StartRoom;
    }

    public void PickBossRoom()
    {
        var leafRooms = State.MainRooms
            .Where(r => r.GetConnectionsCount() == 1 && r.RoomType == RoomTypes.NormalRoom)
            .ToList();

        var bossRoom = leafRooms
            .OrderByDescending(r => r.StartDistance)
            .Take(2)
            .PickRandom(State.Rng);
        bossRoom.RoomType = RoomTypes.BossRoom;
    }


    /* ============================
     * Add Loops
     * ============================ */

    public void AddLoops()
    {
        var leafRooms = State.MainRooms
            .Where(r => r.GetConnectionsCount() == 1 && r.RoomType == RoomTypes.NormalRoom)
            .ToList();

        if (leafRooms.Count == 0) return;


        var potential = new List<ProceduralRoom>(State.MainRooms)
            .Where(r => r.RoomType == RoomTypes.NormalRoom)
            .ToList();

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
                        room.Center.DistanceTo(other.Center) <= Settings.MinRoomDistance;
            }

            if (!valid) break;

            room.AddConnection(other);
            other.AddConnection(room);
        }
    }

    public void PopulateStartDepth()
    {
        var startRoom = State.MainRooms.First(r => r.RoomType == RoomTypes.StartRoom);
        startRoom.StartDistance = 0;

        Queue<ProceduralRoom> rooms = [];
        rooms.Enqueue(startRoom);
        var passed = new HashSet<ProceduralRoom>();
        while (rooms.Count > 0)
        {
            var room = rooms.Dequeue();
            passed.Add(room);
            foreach (var conn in room.Connections)
            {
                if (passed.Contains(conn)) continue;
                conn.StartDistance = room.StartDistance + 1;
                rooms.Enqueue(conn);
            }
        }
    }

    public void PopulateBossDepth()
    {
        var bossRoom = State.MainRooms.First(r => r.RoomType == RoomTypes.BossRoom);
        bossRoom.BossDistance = 0;

        Queue<ProceduralRoom> rooms = [];
        rooms.Enqueue(bossRoom);
        var passed = new HashSet<ProceduralRoom>();
        while (rooms.Count > 0)
        {
            var room = rooms.Dequeue();
            passed.Add(room);
            foreach (var conn in room.Connections)
            {
                if (passed.Contains(conn)) continue;
                conn.BossDistance = room.BossDistance + 1;
                rooms.Enqueue(conn);
            }
        }
    }

    public void PopulateRoomsBias()
    {
        var bossRoom = State.MainRooms.First(r => r.RoomType == RoomTypes.BossRoom);

        foreach (var room in State.MainRooms)
        {
            var detourPenalty = room.StartDistance + room.BossDistance - bossRoom.StartDistance;
            room.ProgressBias = room.StartDistance + 5 * detourPenalty;
        }
    }

    public void SortMainRoomsByBias()
    {
        State.MainRooms = State.MainRooms.OrderBy(room => room.ProgressBias).ToList();
        for (int i = 0; i < State.MainRooms.Count; i++)
        {
            State.MainRooms[i].Index = i;
        }
    }
    public void SortMst()
    {
        State.MinimumSpanningTree = [.. State.MinimumSpanningTree.OrderBy(line => {
                var meanBias = (State.PointToRoom[line.From].ProgressBias + State.PointToRoom[line.To].ProgressBias) / 2f;
                return meanBias;
            }
        )];
    }


    public override async GDTask Perform()
    {
        var connections = CalculateDelaunayConnections(State.MainRooms);
        ConstructMst(connections);
        FillRoomConnections();
        
        UpdateMstLines();

        PickStartRoom();

        AddLoops();

        PopulateStartDepth();

        PickBossRoom();

        PopulateBossDepth();

        PopulateRoomsBias();


        SortMainRoomsByBias();

        SortMst();
    }
}