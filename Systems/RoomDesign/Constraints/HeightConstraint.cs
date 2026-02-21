namespace Casiland.Systems.RoomDesign;

using Casiland.Systems.ProceduralGen;
using Godot;

[GlobalClass]
public  partial class HeightConstraint : RoomPropConstraint {
    [Export] public int MinHeight { get; set; } = -1;
    [Export] public int MaxHeight { get; set; } = -1;
    [Export] public int ExactHeight { get; set; } = -1;

    public override bool CheckRoomFollowsConstraint(ProceduralRoom room)
    {
        if (MinHeight != -1 && room.Size.X < MinHeight) return false;
        if (MaxHeight != -1 && room.Size.X > MaxHeight) return false;
        if (ExactHeight != -1 && room.Size.X != ExactHeight) return false;
        return true;
    }
}