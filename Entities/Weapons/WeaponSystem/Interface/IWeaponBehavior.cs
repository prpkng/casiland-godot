using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem.Interface;

public interface IWeaponBehavior
{
    public void OnEquip(Weapon weapon, Node owner);
    public void OnUnequip();
}