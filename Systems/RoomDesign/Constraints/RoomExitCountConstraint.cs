namespace Casiland.Systems.RoomDesign;

using Casiland.Systems.ProceduralGen;
using Godot;

[GlobalClass]
public  partial class RoomExitCountConstraint : RoomPropConstraint {
    [Export] public int MinExits { get; set; } = -1;
    [Export] public int MaxExits { get; set; } = -1;
    [Export] public int ExactExits { get; set; } = -1;

    public override bool CheckRoomFollowsConstraint(ProceduralRoom room)
    {
        if (MinExits != -1 && room.Connections.Count < MinExits) return false;
        if (MaxExits != -1 && room.Connections.Count > MaxExits) return false;
        if (ExactExits != -1 && room.Connections.Count != ExactExits) return false;
        return true;
    }
}