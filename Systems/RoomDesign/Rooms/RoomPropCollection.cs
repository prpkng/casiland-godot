namespace Casiland.Systems.RoomDesign.Rooms;


using Godot;
using Godot.Collections;


[GlobalClass]
public partial class RoomPropCollection : Resource {
    
    [Export] public int Probability = 100;
    [Export] public Array<RoomPropConstraint> Constraints = [];
    [Export] public PackedScene PropsScene;
}