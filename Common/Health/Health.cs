namespace Casiland.Common.Health;

using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Serilog;
using UnityHFSM;

[Serializable]
public enum HealthAffect
{
    None,
    Damage,
    Heal
}

[GlobalClass]
[Icon("uid://bqctdy6y6vrni")]
[Tool]
public partial class Health : Node {
    private const int DefaultMax = 100;

    /// <summary>Emitted after damage is applied.</summary>
    [Signal] public delegate void DamagedEventHandler(Node entity, HealthActionType type, int amount, int preIncrement, float multiplier, int postIncrement, int applied);
    
    /// <summary>Emitted after damage is applied when death has occurred.</summary>
    [Signal] public delegate void DiedEventHandler(Node entity);
    
    /// <summary>Emitted after healing is applied.</summary>
    [Signal] public delegate void HealedEventHandler(Node entity, HealthActionType type, int amount, int preIncrement, float multiplier, int postIncrement, int applied);
    
    /// <summary>Emitted after healing is applied when dead.</summary>
    [Signal] public delegate void RevivedEventHandler(Node entity);
    
    /// <summary>Emitted after damage or healing is applied.</summary>
    [Signal] public delegate void ActionAppliedEventHandler(HealthModifiedAction action, int applied);
    
    /// <summary>Emitted when damaged and entity had full health.</summary>
    [Signal] public delegate void FirstHitEventHandler(Node entity);

    /// <summary>Emitted when trying to damage an entity that is not damageable.</summary>
    [Signal] public delegate void NotDamageableEventHandler(Node entity);
    
    /// <summary>Emitted when damaging and current health is already zero.</summary>
    [Signal] public delegate void AlreadyDeadEventHandler(Node entity);
    
    /// <summary>Emitted when trying to apply enough damage to kill and they cannot be.</summary>
    [Signal] public delegate void NotKillableEventHandler(Node entity);
    
    /// <summary>Emitted when trying to heal and entity is not healable.</summary>
    [Signal] public delegate void NotHealableEventHandler(Node entity);
    
    /// <summary>Emitted when entity is healed and health is now full.</summary>
    [Signal] public delegate void FullEventHandler(Node entity);
    
    /// <summary>Emitted when healing and current health is already full.</summary>
    [Signal] public delegate void AlreadyFullEventHandler(Node entity);
    
    /// <summary>Emitted when trying to heal a dead entity that is not revivable.</summary>
    [Signal] public delegate void NotRevivableEventHandler(Node entity);


    [Export] public int Current
    {
        get => _current;
        set => _current = Mathf.Clamp(value, 0, Max);
    }

    [Export] public int Max
    {
        get => _max;
        set {
            var oldMax = _max;
            _max = Mathf.Max(value, 1);
            if (Engine.IsEditorHint() && _current == oldMax)
                _current = _max;
            else
                _current = Mathf.Min(_current, _max);

        }
    }

    [ExportGroup("Conditions")]
    [Export] public bool Damageable = true;
    [Export] public bool Healable = true;
    [Export] public bool Killable = true;
    [Export] public bool Revivable = true;

    [ExportGroup("Advanced")]
    [Export] public Node Entity
    {
        get => _entity ??= Owner;
        set => _entity = value;
    }
    [Export] public Godot.Collections.Dictionary<HealthActionType, HealthModifier> Modifiers { get; set; } = [];

    /// <summary>
    /// Enable verbose log events for this health component
    /// </summary>
    [Export] public bool Verbose = false;

    #region === Properties === 

    public bool IsDead => Current <= 0 && Killable;
    public bool IsAlive => !IsDead;
    public bool IsFull => Current >= Max;
    public float Percent => Mathf.Clamp((float)Current / Max, 0f, 1f);


    #endregion

    private int _max = DefaultMax;
    private int _current = DefaultMax;
    private Node _entity;

    #region === INTERFACE ===

    /// <summary>
    /// Applies enough damage to kill. Ignores modifiers.
    /// </summary>
    public void Kill(HealthActionType actionType = HealthActionType.None) {

    }

    /// <summary>
    /// Applies enough heal to fill the health to maximum. Ignores modifiers.
    /// </summary>
    public void Fill(HealthActionType actionType = HealthActionType.None) {
    }

    public void ApplyAllActions(IEnumerable<HealthAction> actions)
    {
        if (actions == null) return;

        foreach (var action in actions)
            ApplyAction(action);
    }

    public void ApplyAction(HealthAction action)
    {
        if (action == null) return;

        var modifier = GetModifier(action.ActionType) ?? new HealthModifier();
        ApplyModifiedAction(new HealthModifiedAction(action, modifier));
    }

    public void ApplyAllModifiedActions(IEnumerable<HealthModifiedAction> actions)
    {
        if (actions == null) return;

        foreach (var action in actions)
            ApplyModifiedAction(action);
    }

    public void ApplyModifiedAction(HealthModifiedAction action)
    {
        if (action == null) return;

        switch (action.Affect)
        {
            case HealthAffect.Damage:
                _Damage(action);
                break;  
            case HealthAffect.Heal:
                _Heal(action);
                break;  
            case HealthAffect.None:
                break;
            default:
                Log.Error("{Affect} affect not implemented.", Enum.GetName(action.Affect));
                break;
        }
    }

    public void Damage(int amount, int preInc = 0, float multiplier = 1f, int postInc = 0)
    {
        var action = new HealthAction(HealthAffect.Damage, HealthActionType.Kinetic, amount);
        var modifier = new HealthModifier(preInc, multiplier, postInc);
        ApplyModifiedAction(new HealthModifiedAction(action, modifier));
    }

    public void Heal(int amount, int preInc = 0, float multiplier = 1f, int postInc = 0)
    {
        var action = new HealthAction(HealthAffect.Heal, HealthActionType.Medicine, amount);
        var modifier = new HealthModifier(preInc, multiplier, postInc);
        ApplyModifiedAction(new HealthModifiedAction(action, modifier));
    }

    #endregion

    /// <summary>
    /// Internal damaging logic, similar to <see cref="_Heal"/>
    /// </summary>
    private void _Damage(HealthModifiedAction action)
    {
        if (action.Affect != HealthAffect.Damage){
            Log.Error("Health action affect must be Damage to apply damage. {action}", action.ToString());
            return;
        }
        if (!Damageable) {
            LogDebug("Entity '{entity}' cannot be damaged.", Entity.Name);
            EmitSignalNotDamageable(Entity);
            return;
        }

        if (IsDead) {
            LogDebug("Entity '{entity}' is already dead, cannot apply more damage.", Entity.Name);
            EmitSignalAlreadyDead(Entity);
            return;
        }

        int appliedDamage = Mathf.Clamp(action.TotalAmount, 0, Current);
        if (appliedDamage >= Current && !Killable) {
            LogDebug("Entity '{entity}' is not killable, not applying damage that would kill it.", Entity.Name);
            EmitSignalNotKillable(Entity);
            appliedDamage = Mathf.Max(appliedDamage - 1, 0);
        }

        bool isFirstHit = IsFull && appliedDamage > 0;
        Current -= appliedDamage;
        LogDebug("Entity '{entity}' took {appliedDamage} damage (action affect: {affect}, type: {type}, amount: {amount}, preInc: {preInc}, mult: {mult}, postInc: {postInc}). New health: {current}/{max}.",
            Entity.Name, appliedDamage, action.Affect, action.ActionType, action.Amount, action.PreIncrement, action.Multiplier, action.PostIncrement, Current, Max);
        
        EmitSignalDamaged(Entity, action.ActionType, action.Amount, action.PreIncrement, action.Multiplier, action.PostIncrement, appliedDamage);
        EmitSignalActionApplied(action, appliedDamage);

        if (isFirstHit)
        {
            LogDebug("Entity '{entity}' took its first hit.", Entity.Name);
            EmitSignalFirstHit(Entity);
        }

        if (IsDead)
        {
            LogDebug("Entity '{entity}' has died.", Entity.Name);
            EmitSignalDied(Entity);
        }
    }

    /// <summary>
    /// Internal healing logic, similar to <see cref="_Damage"/>
    /// </summary>
    private void _Heal(HealthModifiedAction action)
    {
        if (action.Affect != HealthAffect.Heal){
            Log.Error("Health action affect must be Heal to apply healing. {action}", action.ToString());
            return;
        }
        if (!Healable) {
            LogDebug("Entity '{entity}' cannot be healed.", Entity.Name);
            EmitSignalNotHealable(Entity);
            return;
        }

        if (IsFull) {
            LogDebug("Entity '{entity}' is already at full health, cannot apply more healing.", Entity.Name);
            EmitSignalAlreadyFull(Entity);
            return;
        }

        if (IsDead && !Revivable) {
            LogDebug("Entity '{entity}' is dead and not revivable, cannot heal.", Entity.Name);
            EmitSignalNotRevivable(Entity);
            return;
        }

        int appliedHeal = Mathf.Clamp(action.TotalAmount, 0, Max - Current);
        bool notifyRevived = IsDead && appliedHeal > 0;

        Current += appliedHeal;
        LogDebug("Entity '{entity}' healed {appliedHeal} health (action affect: {affect}, type: {type}, amount: {amount}, preInc: {preInc}, mult: {mult}, postInc: {postInc}). New health: {current}/{max}.",
            Entity.Name, appliedHeal, action.Affect, action.ActionType, action.Amount, action.PreIncrement, action.Multiplier, action.PostIncrement, Current, Max);
        
        EmitSignalHealed(Entity, action.ActionType, action.Amount, action.PreIncrement, action.Multiplier, action.PostIncrement, appliedHeal);
        EmitSignalActionApplied(action, appliedHeal);

        if (IsFull)
        {
            LogDebug("Entity '{entity}' has reached full health.", Entity.Name);
            EmitSignalFull(Entity);
        }

        if (notifyRevived)
        {
            LogDebug("Entity '{entity}' has been revived.", Entity.Name);
            EmitSignalRevived(Entity);
        }
    }


    /// <summary>
    /// Gets the health modifier for the given action type. If none exists, means the entity is IMUNE to the modifier
    /// </summary>
    /// <returns>A modifier or null (imune)</returns>
    private HealthModifier GetModifier(HealthActionType actionType)
    {
        if (Modifiers.TryGetValue(actionType, out HealthModifier value))
            return value;
        // Do NOT return an error here, this is intended (same as ignoring the modifier)
        return null;
    }

    private void LogDebug(params object[] args) {
        if (!Verbose) return;
        Log.Debug("[Health] " + args[0], args[1..]);
    }


    public override void _Ready() {
        base._Ready();
    }

    public override void _Process(double delta) {
        base._Process(delta);
    }
}