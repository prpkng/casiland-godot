using System;
using System.Collections.Generic;
using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;

public partial class Weapon : Node2D
{
    [Export] public WeaponData Data;
    
    public Marker2D Muzzle { get; private set; }
    
    
    private Node _owner;

    private AimBehavior _aimBehavior;
    private IAttackBehavior _attackBehavior;
    private ITriggerBehavior _triggerBehavior;
    private List<IWeaponBehavior> _allBehaviors = [];

    
    #region === LIFECYCLE ===
    
    public override void _Ready()
    {
        base._Ready();
        Muzzle = GetNode<Marker2D>("Muzzle");
        BuildFromData();
    }
    
    public void OnEquip(Node owner)
    {
        _owner = owner;

        foreach (var b in _allBehaviors)
            b.OnEquip(this, owner);
    }

    public void OnUnequip()
    {
        foreach (var b in _allBehaviors)
            b.OnUnequip();

        _owner = null;
    }
    
    #endregion === LIFECYCLE ===
    
    #region === INPUT ===

    public void PrimaryPressed()
    {
        _triggerBehavior?.Press();
    }

    public void PrimaryReleased()
    {
        _triggerBehavior?.Release();
    }
    
    /// <summary>
    /// Override this with a function that returns the direction the player is aiming
    /// </summary>
    /// <remarks>
    /// This should NOT have side effects. The default value is a function that returns the direction
    /// from the gun origin to the mouse. It's only meant to be used for debugging.
    /// </remarks>
    public Func<Vector2> GetAimDirection { get; set; }
    
    #endregion === INPUT ===
    

    /// <summary>
    /// Attack execution logic (called by <see cref="ITriggerBehavior"/>s )
    /// </summary>
    /// <param name="power">The multiplier for the attack power (strength and damage)</param>
    public void ExecuteAttack(double power = 1.0)
    {
        if (!_attackBehavior?.ExecuteAttack(power) ?? false) return;
        
        // TODO Effects
        // Recoil kick...
        // Camera kick...
        // Muzzle flash...
    }


    /// <summary>
    /// Main weapon creation factory
    /// </summary>
    /// <remarks>
    /// Creates and adds <see cref="IWeaponBehavior"/>s to the node depending on the <see cref="WeaponData"/> contents.
    /// </remarks>
    private void BuildFromData()
    {
        ClearBehaviors();
        
        // Every weapon has an Aim system
        AddBehavior(new AimBehavior());

        switch (Data.AttackType)
        {
            case AttackType.Melee: 
                _attackBehavior = AddBehavior(new MeleeAttackBehavior());
                break;
            case AttackType.Ranged: 
                _attackBehavior = AddBehavior(new GunFireBehavior());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        };
        switch (Data.TriggerType)
        {
            case TriggerType.SemiAuto: 
                _triggerBehavior = AddBehavior(new SemiAutoTriggerBehavior());
                break;
            case TriggerType.Auto: 
                _triggerBehavior = AddBehavior(new FullAutoTriggerBehavior());
                break;
            case TriggerType.Charge: 
                _triggerBehavior = AddBehavior(new ChargeTriggerBehavior());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        };
        
        //TODO Visual effects:
        
        // if (Data.HasRecoil)
        //     AddChild(new RecoilBehavior { Strength = Data.Recoil });
        //
        // if (Data.Spread > 0)
        //     AddChild(new SpreadBehavior { MaxSpread = Data.Spread });
        //
        // if (Data.HasMuzzleFlash)
        //     AddChild(new MuzzleFlashBehavior());
        
    }

    private T AddBehavior<T>(T behavior) where T : Node, IWeaponBehavior
    {
        AddChild(behavior);
        _allBehaviors.Add(behavior);
        return behavior;
    }

    private void ClearBehaviors()
    {
        foreach (var child in GetChildren())
        {
            if (child is not IWeaponBehavior) continue;
            
            child.QueueFree();
        }
        
        _allBehaviors.Clear();
    }
}