namespace Casiland.Common.Health;

using System;
using Godot;

[GlobalClass]
public partial class HealthModifiedAction(HealthAction action, HealthModifier modifier) : Resource {
    [Export] public HealthAction Action = action;
    [Export] public HealthModifier Modifier = modifier;

    public HealthAffect Affect => Modifier.Affect;
    
    public HealthActionType ActionType => Action.ActionType;

    public int TotalAmount => Mathf.RoundToInt((Amount + PreIncrement) * Multiplier) + PostIncrement;
    public int Amount => Action.Amount;
    public int PreIncrement => Modifier.PreIncrement;
    public int PostIncrement => Modifier.PostIncrement;
    public float Multiplier => Modifier.Multiplier;
    public override string ToString()
        => string.Format("HealthModifiedAction<action={0} modifier={1}>", Action.ToString(), Modifier.ToString());
}