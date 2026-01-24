namespace Casiland.Entities.Weapons.Bullets;

using Godot;

[GlobalClass]
public partial class BulletMovement : Node
{
    [Export] private double MovementSpeed = 100;
    [Export] private bool EnableWallBounce = true;
    /// <summary>
    /// The bullet lifetime in seconds
    /// </summary>
    [Export] private double Lifetime = 3.0;
    /// <summary>
    /// The amount of seconds to increment the lifetime by on each bounce (usually negative)
    /// </summary>
    [Export] private double BounceLifetimeIncr = -1;

    private CharacterBody2D _bulletBody;
    private double _lifetimeCounter;

    public override void _Ready()
    {
        _bulletBody = GetParent<CharacterBody2D>();
        _lifetimeCounter = Lifetime;
    }

    private void Bounce(KinematicCollision2D collision)
    {
        var normal = collision.GetNormal();
        var direction = _bulletBody.Velocity.Normalized();
        var reflected = direction.Bounce(normal);
        _bulletBody.Rotation = Mathf.Atan2(reflected.Y, reflected.X);

        _lifetimeCounter += BounceLifetimeIncr;
    }

    public override void _PhysicsProcess(double delta)
    {
        _lifetimeCounter -= delta;

        _bulletBody.Velocity = _bulletBody.Transform.X * (float)MovementSpeed;
        var collision = _bulletBody.MoveAndCollide(_bulletBody.Velocity * (float)delta);

        if (collision != null && EnableWallBounce)
            Bounce(collision);
        else if (collision != null && !EnableWallBounce)
            _lifetimeCounter = 0;

        if (_lifetimeCounter <= 0)
            _bulletBody.QueueFree();
    }
}