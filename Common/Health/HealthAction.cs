using System;
using Godot;

namespace Casiland.Common.Health;

/// <summary>
/// These are the keys used to add <see cref="HealthModifier"/>>s to <see cref="Health"/> components
/// </summary>
/// <remarks>
/// Modify this enum to add new types of health modifiers, such as poison, fire, lightning, potion, food, etc.
/// </remarks>
[Serializable]
public enum HealthActionType
{
    None,
    /// <summary>
    /// Basic healing from medical supplies
    /// </summary>
    Medicine,
    /// <summary>
    /// Physical damage, such as from bullets, melee, explosions, etc.
    /// </summary>
    Kinetic
}


[GlobalClass]
public partial class HealthAction(
    HealthAffect affect = HealthAffect.Damage,
    HealthActionType actionType = HealthActionType.Kinetic,
    int amount = 1
    ) : Resource, ICloneable
{
    [Export] public HealthAffect Affect = affect;
    [Export] public HealthActionType ActionType = actionType;
    [Export] public int Amount = amount;

    public object Clone() => new HealthAction(Affect, ActionType, Amount);

    public override string ToString() 
        => string.Format("HealthAction<affect={0} type={1} amount={2}>", Enum.GetName(Affect), Enum.GetName(ActionType), Amount);
}