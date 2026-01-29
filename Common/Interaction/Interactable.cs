namespace Casiland.Common.Interaction;

using Godot;

[GlobalClass]
public partial class Interactable : Area2D
{
    [Signal]
    public delegate void InteractedEventHandler(Interactor interactor);

    public void Interact(Interactor interactor)
    {
        EmitSignalInteracted(interactor);
        OnInteracted(interactor);
    }

    protected virtual void OnInteracted(Interactor interactor) {}
}