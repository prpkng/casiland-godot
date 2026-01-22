using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;

public enum AttackType
{
    Melee,
    Ranged
}

public enum TriggerType
{
    SemiAuto,
    Auto,
    Charge
}

public partial class WeaponData : Resource
{
    [Export] public string Name;

    [Export] public TriggerType TriggerType;
    [Export] public AttackType AttackType;

    [Export] public double Damage;
    /// <summary>
    /// The weapon firing rate in shots / second
    /// </summary>
    [Export] public double FireRate;
    [Export] public double Range;
    
    // Optional Modifiers;
    [ExportGroup("Gun Modifiers")] 
    
    [Export] public double Spread;
    [Export] public double Recoil;
    [Export] public double ChargeTime;
    
    [ExportGroup("Visual / FX")]
    
    [Export] public bool HasRecoil;

    // [Export] public bool HasMuzzleFlash;

}