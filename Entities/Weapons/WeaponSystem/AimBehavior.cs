using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;
using Serilog;

namespace Casiland.Entities.Weapons.WeaponSystem;

[GlobalClass]
public partial class AimBehavior : Node, IWeaponBehavior
{
    [Export] public float RotationSpeed = 30f;
    [Export] public bool FlipSprite = true;

    /// <summary> Subtle push into shoulder </summary>
    [Export] public float BackOffset = 0f;

    private Weapon _weapon;
    private Node2D _owner;
    private AnimatedSprite2D _sprite;


    public void OnEquip(Weapon weapon, Node owner)
    {
        _weapon = weapon;
        _owner = owner as Node2D;

        _sprite = weapon.GetNode<AnimatedSprite2D>("Sprite2D");
    }

    public void OnUnequip()
    {
    }

    public override void _Process(double delta)
    {
        if (_weapon == null || _owner == null)
            return;


        Vector2 aimDir = _weapon.GetAimDirection();
        float targetRot = aimDir.Angle();

        _weapon.Rotation = Mathf.LerpAngle(
            _weapon.Rotation,
            targetRot,
            (float)delta * RotationSpeed
        );

        if (FlipSprite && _sprite != null)
        {
            bool facingRight = aimDir.X > 0;
            _sprite.FlipV = !facingRight;
        }

        // Optional visual offset
        _weapon.Position = -aimDir * BackOffset;
    }

}