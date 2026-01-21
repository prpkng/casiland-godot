using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;


//TODO!
public partial class GunFireBehavior : Node, IAttackBehavior
{
    public void OnEquip(Weapon weapon, Node owner)
    {
        throw new System.NotImplementedException();
    }

    public void OnUnequip()
    {
        throw new System.NotImplementedException();
    }

    public bool ExecuteAttack(float force)
    {
        throw new System.NotImplementedException();
    }
}