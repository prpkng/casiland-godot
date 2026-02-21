namespace Casiland.Systems.RoomDesign;

using Casiland.Systems.ProceduralGen;
using Godot;

[GlobalClass]
public  partial class WidthConstraint : RoomPropConstraint {
    [Export] public int MinWidth { get; set; } = -1;
    [Export] public int MaxWidth { get; set; } = -1;
    [Export] public int ExactWidth { get; set; } = -1;

    public override bool CheckRoomFollowsConstraint(ProceduralRoom room)
    {
        if (MinWidth != -1 && room.Size.X < MinWidth) return false;
        if (MaxWidth != -1 && room.Size.X > MaxWidth) return false;
        if (ExactWidth != -1 && room.Size.X != ExactWidth) return false;
        return true;
    }
}