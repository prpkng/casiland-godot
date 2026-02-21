namespace Casiland.Systems.RoomDesign;

using Casiland.Systems.ProceduralGen;
using Godot;

[GlobalClass]
public  partial class RoomTypeConstraint : RoomPropConstraint {
    [Export] public RoomTypes RoomType { get; set; } = RoomTypes.NormalRoom;

    public override bool CheckRoomFollowsConstraint(ProceduralRoom room)
    {
        return room.RoomType == RoomType;
    }
}