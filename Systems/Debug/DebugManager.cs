namespace Casiland.Systems.Debug;

using Fractural.Tasks;
using Godot;

public partial class DebugManager : Node {
    public static DebugManager Instance;

    public static async GDTask WaitForDebugStep()
    {
        await Instance.ToSignal(Instance, SignalName.DebugStepPressed);
    }

    [Signal]
    public delegate void DebugStepPressedEventHandler();




    public override void _Ready() {
        Instance = this;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("debug_step"))
            EmitSignalDebugStepPressed();
    }
    

}