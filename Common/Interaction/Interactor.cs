namespace Casiland.Common.Interaction;

using Godot;

[GlobalClass]
public partial class Interactor : Area2D {
    
    public void InteractPressed()
    {
        foreach (var body in GetOverlappingBodies())
        {
            if (body is Interactable interactable)
                interactable.Interact(this);
        }
    }
}