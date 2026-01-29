namespace Casiland.Common.Health;

using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Collections;

[GlobalClass]
[Icon("uid://du8rjdipnoyji")]
public partial class HitBox2D : Area2D {
    
    /// <summary>Emitted when collision with HitBox2D detected.</summary>
    [Signal] public delegate void HitBoxEnteredEventHandler(HitBox2D hitBox);
    
    /// <summary>Emitted when collision with HurtBox2D detected.</summary>
    [Signal] public delegate void HurtBoxEnteredEventHandler(HurtBox2D hurtBox);
    
    /// <summary>Emitted after the action is applied to a HurtBox2D.</summary>
    [Signal] public delegate void ActionAppliedEventHandler(HurtBox2D hurtBox);
    
    /// <summary>Emitted when collision with Area2D that isn't HitBox2D or HurtBox2D. Can be used to detect things like environment.</summary>
    [Signal] public delegate void UnknownAreaEnteredEventHandler(Area2D area);

    /// <summary>
    /// Ignores collisions when true
    /// </summary>
    /// <remarks>
    /// <para>Set this to true after a collision is detected to avoid further collisions. </para>
    /// <para>It is recommended to set this to true before calling <see cref="Node.QueueFree"/> to avoid further collisions</para>
    /// </remarks>
    [Export] public bool IgnoreCollisions;

    [Export] public HealthAction BaseAction;

    [Export] public Array<HealthAction> ExtraActions = [];

    public override void _Ready() {
        Connect(Area2D.SignalName.AreaEntered, Callable.From<Area2D>(OnAreaEntered));
    }

    private void OnAreaEntered(Area2D area) {
        if (IgnoreCollisions)
            return;

        if (area is HitBox2D hitBox) {
            EmitSignal(SignalName.HitBoxEntered, hitBox);
            return;
        }
        
        if (area is not HurtBox2D hurtBox)
        {
            EmitSignalUnknownAreaEntered(area);
            return;
        }

        EmitSignalHurtBoxEntered(hurtBox);

        hurtBox.ApplyAllActions([BaseAction, .. ExtraActions]);
        EmitSignalActionApplied(hurtBox);
    }
}