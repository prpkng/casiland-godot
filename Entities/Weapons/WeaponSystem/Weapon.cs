using System;
using System.Collections.Generic;
using Casiland.Entities.Weapons.WeaponSystem.Interface;
using Godot;

namespace Casiland.Entities.Weapons.WeaponSystem;

public partial class Weapon : Node2D
{
    [Export] public WeaponData Data;


    private AimBehavior _aimBehavior;
    private IAttackBehavior _attackBehavior;
    private ITriggerBehavior _triggerBehavior;
    private List<IWeaponBehavior> _allBehaviors = [];

    public override void _Ready()
    {
        base._Ready();
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