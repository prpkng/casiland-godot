using Casiland.Common.Movement;
using Casiland.Entities.Weapons.WeaponSystem;
using Godot;

namespace Casiland.Entities.Player;

[GlobalClass]
public partial class Player : CharacterBody2D, IAimProvider
{
    [Export] public TopDownMovement Movement;
    [Export] public Sprite2D Sprite;

    [Export] public WeaponData WeaponData;


    public override void _Ready()
    {
        GetNode<WeaponManager>("WeaponManager").EquipWeapon(WeaponData);
    }



    public Vector2 GetAimDirection()
    {
        return GlobalPosition.DirectionTo(GetGlobalMousePosition());
    }


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