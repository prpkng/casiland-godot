using Casiland.Common.Interaction;
using Casiland.Common.Movement;
using Casiland.Entities.Weapons.WeaponSystem;
using Godot;

namespace Casiland.Entities.Player;

[GlobalClass]
public partial class Player : CharacterBody2D, IAimProvider
{
    [Export] public TopDownMovement Movement;
    [Export] public Sprite2D Sprite;

    [Export] public PackedScene Weapon;
    [Export] public WeaponManager WeaponManager;
    [Export] public Interactor Interactor;


    public override void _Ready()
    {
        WeaponManager.AddWeapon(Weapon);
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

        var aimDir = GetAimDirection();
        if (aimDir.X != 0)
            Sprite.FlipH = aimDir.X < 0;

        
        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            GlobalPosition = GetGlobalMousePosition();
        }

        
        // Input Forwarding
        if (Input.IsActionJustPressed("fire"))
            WeaponManager.PerformPrimary();
        
        if (Input.IsActionJustReleased("fire"))
            WeaponManager.StopPrimary(); 

        if (Input.IsActionJustPressed("interact"))
            Interactor.InteractPressed();
        if (input.Length() > 0)
            Interactor.Rotation = input.Angle();

    }
} 