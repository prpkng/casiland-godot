namespace Casiland.Entities.World.Dungeons.Doors;

using System.Threading.Tasks;
using Casiland.Common;
using Casiland.Common.Interaction;
using Godot;

public partial class DoubleDoors : Node2D {
    [Export] public Interactable Interactable;
    [Export] public Node2D LeftDoor;
    [Export] public Node2D RightDoor;

    private const float StartAngle = -Mathf.Pi / 2;
    private float _leftLastAngle = StartAngle;
    private float _rightLastAngle = StartAngle;

    private bool isOpened = false;
    private Tween _lastTween;

    override public void _Ready()
    {
        Interactable.Interacted += _OnInteract;
    }

    private void _OnInteract(Interactor interactor)
    {
        var direction = interactor.GlobalTransform.Y;
        int side = direction.Dot(Transform.Y) < 0 ? 1 : -1;
        ToggleDoor(side);
    }

    public async void ToggleDoor(int direction)
    {
        _lastTween?.Kill();

        isOpened = !isOpened;
        
        LeftDoor.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = isOpened;
        RightDoor.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = isOpened;
        LeftDoor.ResetPhysicsInterpolation();
        RightDoor.ResetPhysicsInterpolation();


        if (isOpened) OpenDoor(direction);
        else CloseDoor();
    }

    private void CloseDoor()
    {
        var tween = GetTree().CreateTween()
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
        _lastTween = tween;
        float curLeft = _leftLastAngle;
        float curRight = _rightLastAngle;
        _leftLastAngle = StartAngle;
        _rightLastAngle = StartAngle;
        tween.TweenMethod(Callable.From<float>(f =>
        {
            LeftDoor.Rotation = Mathf.LerpAngle(curLeft, StartAngle, f);
            RightDoor.Rotation = Mathf.LerpAngle(curRight, StartAngle, f);
        }), 0f, 1f, 0.8);

    }
    public void OpenDoor(int direction)
    {
        var tween = GetTree().CreateTween()
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
        _lastTween = tween;
        float curLeft = _leftLastAngle;
        float curRight = _rightLastAngle;
        float targetLeft = StartAngle + -Mathf.Pi / 2 * direction;
        float targetRight = StartAngle + Mathf.Pi / 2 * direction;
        _leftLastAngle = targetLeft;
        _rightLastAngle = targetRight;
        tween.TweenMethod(Callable.From<float>(f =>
        {
            LeftDoor.Rotation = Mathf.LerpAngle(curLeft, targetLeft, f);
            RightDoor.Rotation = Mathf.LerpAngle(curRight, targetRight, f);
        }), 0f, 1f, 0.8);
        
    }

}