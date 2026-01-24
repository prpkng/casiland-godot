using System;
using System.Collections.Generic;
using System.Linq;
using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;
using Serilog;

namespace Casiland.Entities.Weapons.WeaponSystem;

[GlobalClass]
public partial class Weapon : Node2D
{
    [Export] public WeaponStats Stats;
    
    
    private Node _owner;

    private AimBehavior _aimBehavior;
    private IAttackBehavior _attackBehavior;
    private ITriggerBehavior _triggerBehavior;
    private List<IWeaponBehavior> _allBehaviors = [];

    
    #region === LIFECYCLE ===
    
    public override void _Ready()
    {
        GatherBehaviors();
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
    /// Creates and adds <see cref="IWeaponBehavior"/>s to the node depending on the <see cref="WeaponStats"/> contents.
    /// </remarks>
    private void GatherBehaviors()
    {
        _allBehaviors = [.. GetChildren().OfType<IWeaponBehavior>()];

        if (_allBehaviors.Count == 0)
        {
            Log.Error("Weapon has no behaviors attached!");
            return;
        }

        try {
            _aimBehavior = _allBehaviors.OfType<AimBehavior>().Single();
            _attackBehavior = _allBehaviors.OfType<IAttackBehavior>().Single();
            _triggerBehavior = _allBehaviors.OfType<ITriggerBehavior>().Single();
        } catch (InvalidOperationException e) {
            Log.Error("Weapon is missing a required behavior\n Any weapon must have exactly one AIM, ATTACK, and TRIGGER behavior.", e.Message);
        }
    }
}