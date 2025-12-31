using Godot;

namespace Casiland.AutoTileEx.Editor;

[Tool]
public partial class RightClickableButton : Button
{
    [Signal]
    public delegate void RightClickedEventHandler();

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Right, Pressed: false })
            EmitSignalRightClicked();
            
    }
}