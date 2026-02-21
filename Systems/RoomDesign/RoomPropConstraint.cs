namespace Casiland.Systems.RoomDesign;

using Casiland.Systems.ProceduralGen;
using Godot;

[GlobalClass]
public abstract partial class RoomPropConstraint : Resource {
    public abstract bool CheckRoomFollowsConstraint(ProceduralRoom room);
}