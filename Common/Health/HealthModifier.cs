namespace Casiland.Common.Health;

using System;
using Godot;

[GlobalClass]
public partial class HealthModifier(
    HealthAffect affect = HealthAffect.None,
    int preIncrement = 0,
    float multiplier = 1.0f,
    int postIncrement = 0
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

    [Export] public HealthAffect Affect = affect;

    public HealthModifier() : this(HealthAffect.None, 0, 1.0f, 0) { }

    public object Clone()
        => new HealthModifier(Affect, PreIncrement, Multiplier, PostIncrement);

    public override string ToString()
        => string.Format("HealthModifier<affect={0} inc={1} mult={2} postInc={3}>", 
            Enum.GetName(Affect), PreIncrement, Multiplier, PostIncrement);
}