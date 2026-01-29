namespace Casiland.Entities.Enemies.Testing;

using Casiland.Common.Health;
using Godot;

public partial class DummyProgressBar : ProgressBar {
    [Export] public Health health;

    public override void _Ready()
    {
        health.Connect(Health.SignalName.HealthChanged, Callable.From<int>((newHealth) =>
        {
            Value = health.PercentHundred;
        }));
    }

}