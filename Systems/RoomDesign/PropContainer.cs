namespace Casiland.Systems.RoomDesign;


using Godot;


[System.Serializable]
public enum PropFillMode
{
    Centered,
    Fill
}

[GlobalClass]
[Tool]
public partial class PropContainer : Control {
    
    [Export] public PackedScene PropScene
    {
        get => _propScene;
        set
        {
            _propScene = value;
            OnPropSceneChanged();
        }
    }

    [Export] public PropFillMode FillMode = PropFillMode.Centered;

    private PackedScene _propScene;
    private Node _child;

    private void OnPropSceneChanged()
    {
        _child?.Free();

        _child = PropScene.Instantiate(PackedScene.GenEditState.Instance);
	    AddChild(_child, false, InternalMode.Back);

    }


    public override void _Process(double delta)
    {
        if (_child == null || !Engine.IsEditorHint()) return;
        
        var rect = GetGlobalRect();
        
        switch (FillMode)
        {
            case PropFillMode.Centered when _child is Node2D node2D:
                node2D.GlobalPosition = rect.GetCenter();
                break;
            case PropFillMode.Fill when _child is Node2D node2D:
                node2D.GlobalPosition = rect.GetCenter();
                break;

            case PropFillMode.Centered when _child is Control ctrl:
                ctrl.GlobalPosition = rect.GetCenter();
                break;
            case PropFillMode.Fill when _child is Control ctrl:
                ctrl.GlobalPosition = rect.Position;
                ctrl.Size = rect.Size;
                break;

        }
    }

}