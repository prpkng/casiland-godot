using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;


[GlobalClass]

public partial class WeaponStats : Resource
{
    [Export] public string Name;

    [Export] public double BaseDamage;
    /// <summary>
    /// The weapon attacking rate in attacks / second
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