namespace Casiland.Entities.Weapons.WeaponSystem.Interface;

public interface IAttackBehavior : IWeaponBehavior
{
    public bool ExecuteAttack(double force);
}