namespace Casiland.Common.Interaction;

using Godot;
using Serilog;

[GlobalClass]
public partial class Interactor : Area2D
{
    public Interactor() { CollisionLayer = 1 << 9; CollisionMask = 1 << 9; }
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