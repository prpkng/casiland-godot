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

    [Export] public float Damage;
    [Export] public float FireRate;
    [Export] public float Range;
    
    // Optional Modifiers;
    [ExportGroup("Gun Modifiers")] 
    
    [Export] public float Spread;
    [Export] public float Recoil;
    [Export] public float ChargeTime;
    
    [ExportGroup("Visual / FX")]
    
    [Export] public bool HasRecoil;

    // [Export] public bool HasMuzzleFlash;

}