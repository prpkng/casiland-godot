namespace Casiland.Systems.RoomDesign.Rooms;


using Godot;

[GlobalClass]
public partial class RoomPropCollection : Resource {
    
    [Export] public int Probability = 100;
    [Export] public PackedScene PropsScene;
}