using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;

[GlobalClass]
public partial class MeleeAttackBehavior : Node, IAttackBehavior
{
    public void OnEquip(Weapon weapon, Node owner)
    {
    }

    public void OnUnequip()
    {
    }

    public bool ExecuteAttack(double force)
    {
        return false;
    }
}