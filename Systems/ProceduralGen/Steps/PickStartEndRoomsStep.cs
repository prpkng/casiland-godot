using System;
using System.Collections.Generic;
using System.Linq;
using Casiland.Common;
using Casiland.Entities.World.Dungeons.Doors;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;
using Serilog;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PickStartEndRoomsStep(GenerationState state, ProceduralGenerationSettings settings)
    : GenerationStep(state, settings)
{
    public override string StateDescription => "picking room types";

    private Dictionary<ProceduralRoom, int> _mainDepths;

    public void CalculateMainDepths()
    {
        _mainDepths = [];

        var passed = new HashSet<ProceduralRoom>();

        var leafRooms = State.MainRooms
            .Where(r => r.Connections.Count == 1).ToList();

        Queue<ProceduralRoom> rooms = [];
        foreach (var room in leafRooms)
        {
            passed.Add(room);
            rooms.Enqueue(room);
            _mainDepths[room] = 1;
        }

        while (rooms.Count > 0)
        {
            var room = rooms.Dequeue();
            passed.Add(room);
            foreach (var conn in room.Connections)
            {
                if (passed.Contains(conn)) continue;
                _mainDepths[conn] = _mainDepths[room] + 1;
                rooms.Enqueue(conn);
            }
        }
    }

    public void PickStartRoom()
    {
        try
        {
            var leafRooms = State.MainRooms
                .OrderByDescending(r => _mainDepths[r])
                .Take(4)
                .ToList();
            var startRoom = leafRooms.PickRandom(State.Rng);
            startRoom.RoomType = RoomTypes.StartRoom;
        }
        catch (KeyNotFoundException e)
        {
            Log.Error("{Err}", e.Message);
        }

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
            State.MainRooms[i].Index = i;
    }

    public void SortMst()
    {
        State.MinimumSpanningTree =
        [
            .. State.MinimumSpanningTree.OrderBy(line =>
                {
                    var meanBias = (State.PointToRoom[line.From].ProgressBias +
                                    State.PointToRoom[line.To].ProgressBias) / 2f;
                    return meanBias;
                }
            )
        ];
    }


    public override async GDTask Perform()
    {
        CalculateMainDepths();

        PickStartRoom();

        // AddLoops();

        PopulateStartDepth();

        PickBossRoom();

        PopulateBossDepth();

        PopulateRoomsBias();


        SortMainRoomsByBias();

        SortMst();

        State.AllRooms = [.. State.MainRooms, .. State.InBetweenRooms];
        State.AllRooms = State.AllRooms.OrderBy(room => room.ProgressBias).ToList();
    }
}