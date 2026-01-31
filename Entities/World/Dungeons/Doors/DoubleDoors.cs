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
        LeftDoor.Rotation = isOpened ? StartAngle + -Mathf.Pi / 2.2f * direction : StartAngle;
        RightDoor.Rotation = isOpened ? StartAngle + Mathf.Pi / 2.2f * direction : StartAngle;


        LeftDoor.ResetPhysicsInterpolation();
        RightDoor.ResetPhysicsInterpolation();


        isOpened = !isOpened;
        if (isOpened) OpenDoor(direction);
        else CloseDoor();
    }

    private void CloseDoor()
    {
        var tween = GetTree().CreateTween()
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
        _lastTween = tween;
        float curLeft = LeftDoor.Rotation;
        float curRight = RightDoor.Rotation;
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
        float curLeft = LeftDoor.Rotation;
        float curRight = RightDoor.Rotation;
        tween.TweenMethod(Callable.From<float>(f =>
        {
            LeftDoor.Rotation = Mathf.LerpAngle(curLeft, StartAngle + -Mathf.Pi / 2 * direction, f);
            RightDoor.Rotation = Mathf.LerpAngle(curRight, StartAngle + Mathf.Pi / 2 * direction, f);
        }), 0f, 1f, 0.8);
        
    }

}