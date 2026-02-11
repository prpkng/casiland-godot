using Godot;

namespace Casiland.AutoTileEx.Editor;

[Tool]
public partial class RightClickableButton : Button
{
    [Signal]
    public delegate void RightClickedEventHandler();
    [Signal]
    public delegate void LeftClickedEventHandler();

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: true })
            EmitSignalRightClicked();
        else if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
            EmitSignalLeftClicked();
    }
}