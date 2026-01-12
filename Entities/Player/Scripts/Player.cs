using Casiland.Common.Movement;
using Godot;

namespace Casiland.Entities.Player;

[GlobalClass]
public partial class Player : CharacterBody2D
{
    [Export] public TopDownMovement Movement;
    [Export] public Sprite2D Sprite;

    public override void _Process(double delta)
    {
        var input = new Vector2
        {
            X = Input.GetAxis("move_left", "move_right"),
            Y = Input.GetAxis("move_up", "move_down")
        }.Normalized();
        Movement.MovementInput = input;

        if (input.X != 0)
            Sprite.FlipH = input.X < 0;

    }
}