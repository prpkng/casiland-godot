namespace Casiland.Common.Interaction;

using Godot;
using Serilog;

[GlobalClass]
public partial class Interactable : Area2D
{
    [Signal]
    public delegate void InteractedEventHandler(Interactor interactor);

    public void Interact(Interactor interactor)
    {
        Log.Debug("{this} received an interaction from {interactor}", Owner.Name, interactor.Owner.Name);
        EmitSignalInteracted(interactor);
        OnInteracted(interactor);
    }

    protected virtual void OnInteracted(Interactor interactor) {}
}