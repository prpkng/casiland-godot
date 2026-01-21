namespace Casiland.Entities.Weapons.WeaponSystem.Interface;

public interface ITriggerBehavior : IWeaponBehavior
{
    public void Press();
    public void Release();
}