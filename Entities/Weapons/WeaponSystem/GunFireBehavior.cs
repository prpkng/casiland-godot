using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;


[GlobalClass]
//TODO!
public partial class GunFireBehavior : Node, IAttackBehavior
{
    [Export] public PackedScene BulletScene;
    [Export] public Marker2D Muzzle;


    public void OnEquip(Weapon weapon, Node owner)
    {
    }

    public void OnUnequip()
    {
    }

    public bool ExecuteAttack(double force)
    {
        var bullet = BulletScene.Instantiate<CharacterBody2D>();
        GetTree().Root.AddChild(bullet);
        bullet.GlobalPosition = Muzzle.GlobalPosition;
        bullet.Rotation = Muzzle.GlobalRotation;
        bullet.ResetPhysicsInterpolation();

        return true;
    }
}