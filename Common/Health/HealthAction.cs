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
public partial class HealthAction() : Resource
{
    [Export] public HealthActionType ActionType = HealthActionType.Kinetic;
    [Export] public int Amount = 1;

    public override string ToString() 
        => string.Format("HealthAction<type={0} amount={1}>", Enum.GetName(ActionType), Amount);
}