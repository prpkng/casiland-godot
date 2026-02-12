using System.Collections.Generic;
using System.Linq;
using Fractural.Tasks;

namespace Casiland.Systems.ProceduralGen.Steps;

public class RecalculateDepthsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => "recalculating depths";
    

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

        foreach (var room in State.AllRooms)
        {
            var detourPenalty = room.StartDistance + room.BossDistance - bossRoom.StartDistance;
            room.ProgressBias = room.StartDistance + 5 * detourPenalty;
        }
    }

    public void SortRoomsByBias()
    {
        State.AllRooms = State.AllRooms.OrderBy(room => room.ProgressBias).ToList();
        for (int i = 0; i < State.AllRooms.Count; i++)
            State.AllRooms[i].Index = i;
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
        PopulateStartDepth();
        PopulateBossDepth();
        PopulateRoomsBias();
        SortRoomsByBias();
        SortMst();
    }

}