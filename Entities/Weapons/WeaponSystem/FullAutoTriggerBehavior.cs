using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;

//TODO!
public partial class FullAutoTriggerBehavior : Node, ITriggerBehavior
{
    private double _fireRateCounter;
    private bool _isHoldingFire;
    private Weapon _weapon;
    
    public void OnEquip(Weapon weapon, Node owner)
    {
        _weapon = weapon;
    }

    public void OnUnequip()
    {
    }

    private void ExecuteAttack()
    {
        _weapon.ExecuteAttack();
        _fireRateCounter = _weapon.Data.FireRate;
    }

    public void Press()
    {
        _isHoldingFire = true;
        if (_fireRateCounter <= 0) ExecuteAttack();
    }

    public void Release()
    {
        _isHoldingFire = false;
    }

    public override void _Process(double delta)
    {
        _fireRateCounter -= delta;
        if (_fireRateCounter <= 0) ExecuteAttack();
    }
}