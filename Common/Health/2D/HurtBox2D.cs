namespace Casiland.Common.Health;

using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Serilog;

[Tool]
[GlobalClass]
[Icon("uid://dhbeivdkmtds1")]
public partial class HurtBox2D : Area2D
{
    public HurtBox2D()
    {
        Monitoring = false;
    }

    private Health _health;

    [Export]
    public Health Health
    {
        get => _health;
        set
        {
            _health = value;
            if (Engine.IsEditorHint())
                UpdateConfigurationWarnings();
        }
    }

    [Export] public Dictionary<HealthActionType, HealthModifier> Modifiers = [];

    public void ApplyAllActions(HealthAction[] actions)
    {
        if (_health == null)
        {
            Log.Error("{this} is missing a 'Health' component", this);
            return;
        }

        var modifiedActions = actions.Select(value =>
        {
            var modifier = Modifiers.TryGetValue(value.ActionType, out HealthModifier mod)
                ? mod : _health.GetModifier(value.ActionType);
            return new HealthModifiedAction(value, modifier);
        });

        Health.ApplyAllModifiedActions(modifiedActions.ToArray());
    }


    public override string[] _GetConfigurationWarnings()
    {
       if (Health == null)
           return ["This node requires an assigned 'Health' component."];

        return base._GetConfigurationWarnings();
    }
}