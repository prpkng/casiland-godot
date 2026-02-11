using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;


[GlobalClass]
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

        //! COMPLETELY TEMPORARY
        //!TODO Replace this by a dedicated system in a Game Manager class or smth
        var root = GetTree().CurrentScene.GetNode("CanvasLayer/SubViewportContainer/SubViewport");
        root.AddChild(bullet);
        bullet.GlobalPosition = Muzzle.GlobalPosition;
        bullet.Rotation = Muzzle.GlobalRotation;
        bullet.ResetPhysicsInterpolation();

        return true;
    }
}