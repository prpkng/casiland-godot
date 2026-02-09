namespace Casiland.Common.Interaction;

using Godot;
using Serilog;

[GlobalClass]
public partial class Interactor : Area2D
{
    public Interactor() { CollisionLayer = 1 << 9; CollisionMask = 1 << 9; }

    public override void _Ready()
    {
        AreaEntered += area =>
        {
            if (area is Interactable interactable)
                interactable.EmitSignal(Interactable.SignalName.BeginHover, this);
        };

        AreaExited += area =>
        {
            if (area is Interactable interactable)
                interactable.EmitSignal(Interactable.SignalName.EndHover, this);
        };
    }



    public void InteractPressed()
    {
        foreach (var body in GetOverlappingAreas())
        {
            if (body is not Interactable interactable)
                continue;

            Log.Debug("{this} is interacting with {interactable}", Owner.Name, interactable.Owner.Name);
            interactable.Interact(this);
        }
    }
}