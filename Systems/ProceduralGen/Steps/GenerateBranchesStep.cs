using System.Collections.Generic;
using System.Linq;
using Casiland.Common;
using Fractural.Tasks;
using Godot;
using Serilog;

namespace Casiland.Systems.ProceduralGen.Steps;

public class GenerateBranchesStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => "generating branches";

    private bool RandomPercentage(float percentage, RandomNumberGenerator rng)
    {
        return rng.RandfRange(0, 100f) < percentage;
    }

    private bool CheckRoomOverlap(ProceduralRoom room)
    {
        var rect = room.Rect.Grow(5);
        if (((ProceduralRoom[])[..State.MainRooms, ..State.InBetweenRooms])
                    .Any(other => other.Rect.Intersects(rect)))
            return true;
        // if (State.MinimumSpanningTree.Any(line =>
        //         ProceduralGeometry.EdgeIntersectsRect(line, rect)))
        //     return true;
        return false;
    }


    public ProceduralRoom CreateLeafRoom(ProceduralRoom from)
    {
        var possibleDirections = new List<Vector2>
        {
            Vector2.Right,
            Vector2.Left,
            Vector2.Up,
            Vector2.Down
        };
        foreach (var conn in from.Connections)
            possibleDirections.Remove(from.Center.DirectionTo(conn.Center).Sign4Way());
        
        foreach (var dir in possibleDirections) 
        {
            float baseSize = State.Rng.RandfRange(Settings.MinBaseRoomSize, Settings.MaxBaseRoomSize);
            var size = ProceduralGeometry.AspectWiseRandomSize(
                State.Rng,
                Settings.BaseRoomAspect,
                Settings.MaxRoomAspectDeviation,
                baseSize,
                Settings.MaxRoomSizeDeviation
            );

            float fromSizeInDir = (from.Size * dir.Abs()).Sum();
            float destSizeInDir = (size * dir.Abs()).Sum();
            int distance = Mathf.RoundToInt(
                fromSizeInDir / 2f + destSizeInDir / 2f +
                State.Rng.RandiRange(Settings.BranchedRoomMinDistance,
                    Settings.BranchedRoomMaxDistance));
            var pos = from.Center + dir * distance;
            var room = new ProceduralRoom(pos - size/2f, size);
            if (CheckRoomOverlap(room))
                continue;
            return room;
        }

        return null;
    }

    private void AddLeafRoom(ProceduralRoom fromRoom, ProceduralRoom leafRoom)
    {
        leafRoom.IsLeafGeneratedRoom = true;
        fromRoom.Connections.Add(leafRoom);
        leafRoom.Connections.Add(fromRoom);

        var fromPos = State.PointToRoom.First(r => r.Value == fromRoom).Key;

        var lineSegment = new LineSegment(fromPos, leafRoom.Center);
        State.PointToRoom[lineSegment.To] = leafRoom;
        State.MinimumSpanningTree.Add(lineSegment);
        State.MainRooms.Add(leafRoom);
    }
    
    
    private int CalculateBranchCount(ProceduralRoom room, float multiplier)
    {
        switch (room.Connections.Count)
        {
            // Leaf room
            case 1
                when RandomPercentage(Settings.LeafBranchPercentage * multiplier, State.Rng):
                return State.Rng.RandiRange(1, Settings.LeafMaxBranchCount);
            // Direct room
            case 2
                when RandomPercentage(Settings.DirectBranchPercentage * multiplier, State.Rng):
                return State.Rng.RandiRange(1, Settings.DirectMaxBranchCount);
        }

        return 0;
    }

    private void GenerateBranchRooms()
    {
        var visited = new HashSet<ProceduralRoom>();

        for (int iter = 0; iter < Settings.BranchingIterationCount; iter++)
        {
            float chanceMultiplier =
                Mathf.Pow(Settings.BranchingIterationChanceMultiplier, iter - 1);

            foreach (var room in State.MainRooms.ToArray())
            {
                int branchCount = CalculateBranchCount(room, chanceMultiplier);
                if (branchCount <= 0) continue;
                
                // if (!visited.Add(room)) continue;

                for (int j = 0; j < branchCount; j++)
                {
                    var leafRoom = CreateLeafRoom(room);
                    if (leafRoom == null)
                    {
                        Log.Error("Failed to create leaf room from room {Id}", room.Id);
                        continue;
                    }
                    
                    AddLeafRoom(room, leafRoom);
                    if (State.MainRooms.Count > Settings.AbsoluteMaxRoomCount) return;
                }
            }
        }
    }
    
    
    public override async GDTask Perform()
    {
        GenerateBranchRooms();
    }
}