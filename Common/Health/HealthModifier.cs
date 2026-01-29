namespace Casiland.Common.Health;

using System;
using Godot;

[GlobalClass]
public partial class HealthModifier(
    int preIncrement = 0,
    float multiplier = 1.0f,
    int postIncrement = 0,
    HealthAffect convertAffect = HealthAffect.None,
    HealthActionType convertActionType = HealthActionType.None
    ) : Resource, ICloneable
{
    /// <summary>
    /// The amount of health to add or subtract before applying the multiplier
    /// </summary>
    [Export] public int PreIncrement = preIncrement;
    /// <summary>
    /// The multiplier to apply to the total amount after pre-increment
    /// </summary>
    [Export] public float Multiplier = multiplier;
    /// <summary>
    /// The amount of health to add or subtract after applying the multiplier
    /// </summary>
    [Export] public int PostIncrement = postIncrement;

    [Export] public HealthAffect ConvertAffect = convertAffect;
    [Export] public HealthActionType ConvertActionType = convertActionType;

    public object Clone()
        => new HealthModifier(PreIncrement, Multiplier, PostIncrement, ConvertAffect, ConvertActionType);

    public override string ToString()
        => string.Format("HealthModifier<inc={0} mult={1} postInc={2} convertAffect={3} convertType={4}>", 
            PreIncrement, Multiplier, PostIncrement, Enum.GetName(ConvertAffect), Enum.GetName(ConvertActionType));
}