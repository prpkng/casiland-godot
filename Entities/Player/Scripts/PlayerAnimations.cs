using Godot;

namespace Casiland.Entities.Player;

public partial class PlayerAnimations : AnimationTree
{
    [Export] public Player Player;

    public override void _Process(double delta)
    {
        
        Set(
            "parameters/MovementBlendSpace/blend_position", 
            Mathf.Min(1, Player.Velocity.Length() / 10f)
            );        
    }
}