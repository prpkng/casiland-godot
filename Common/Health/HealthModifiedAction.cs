namespace Casiland.Common.Health;

using System;
using Godot;

[GlobalClass]
public partial class HealthModifiedAction(HealthAction action, HealthModifier modifier) : Resource, ICloneable {
    [Export] public HealthAction Action = action;
    [Export] public HealthModifier Modifier = modifier;

    public HealthAffect Affect => Modifier.ConvertAffect != HealthAffect.None
        ? Modifier.ConvertAffect
        : Action.Affect;
    
    public HealthActionType ActionType => Modifier.ConvertActionType != HealthActionType.None
        ? Modifier.ConvertActionType
        : Action.ActionType;

    public int TotalAmount => Mathf.RoundToInt((Amount + PreIncrement) * Multiplier) + PostIncrement;
    public int Amount => Action.Amount;
    public int PreIncrement => Modifier.PreIncrement;
    public int PostIncrement => Modifier.PostIncrement;
    public float Multiplier => Modifier.Multiplier;

    public object Clone()
        => new HealthModifiedAction((HealthAction)Action.Clone(), (HealthModifier)Modifier.Clone());
    public override string ToString()
        => string.Format("HealthModifiedAction<action={0} modifier={1}>", Action.ToString(), Modifier.ToString());
}