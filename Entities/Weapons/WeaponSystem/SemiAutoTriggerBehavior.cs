using Casiland.Common;
using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;

[GlobalClass]
//TODO!
public partial class SemiAutoTriggerBehavior : Node, ITriggerBehavior
{
    private Weapon _weapon;
    private Countdown _fireRateCountdown;
    
    public void OnEquip(Weapon weapon, Node owner)
    {
        _weapon = weapon;
    }

    public void OnUnequip()
    {
    }

    public void Press()
    {
        if (!_fireRateCountdown.IsFinished) return;
        
        _weapon.ExecuteAttack();
        _fireRateCountdown.SetCountdown(1.0 / _weapon.Data.FireRate);
    }

    public void Release()
    {
    }
}