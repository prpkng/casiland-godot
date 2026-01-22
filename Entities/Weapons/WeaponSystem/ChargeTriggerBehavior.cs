using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;

//TODO!
public partial class ChargeTriggerBehavior : Node, ITriggerBehavior
{
    private double _charge;
    private Weapon _weapon;

    public override void _Ready()
    {
        SetProcess(false);
    }

    public void OnEquip(Weapon weapon, Node owner)
    {
        _weapon = weapon;
    }

    public void OnUnequip()
    {
    }

    public void Press()
    {
        _charge = 0;
        SetProcess(true);
    }


    public override void _Process(double delta)
    {
        _charge = Mathf.Min(_charge + (float)delta, _weapon.Data.ChargeTime);
    }
    public void Release()
    {
        _weapon.ExecuteAttack(_charge / _weapon.Data.ChargeTime);
        SetProcess(false);
    }
}