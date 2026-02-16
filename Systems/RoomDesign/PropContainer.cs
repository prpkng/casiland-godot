namespace Casiland.Systems.RoomDesign;

using System.Linq;
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
    public Node Prop;

    public override void _EnterTree()
    {
        if (!Engine.IsEditorHint())
            PerformSizing(Prop);
    }

    private void OnPropSceneChanged()
    {
        foreach (var c in GetChildren(true).Except(GetChildren())) c.Free();

        Prop = PropScene.Instantiate(PackedScene.GenEditState.Instance);
	    AddChild(Prop, false, InternalMode.Back);

    }

    public Vector2 GetPropPosition(Node node)
    {
        var rect = GetGlobalRect();
        
        return FillMode switch
        {
            PropFillMode.Centered when node is Node2D => rect.GetCenter(),
            PropFillMode.Fill when node is Node2D => rect.GetCenter(),
            PropFillMode.Centered when node is Control => rect.GetCenter(),
            PropFillMode.Fill when node is Control => rect.Position,
            _ => throw new System.NotImplementedException(),
        };
    }

    public void PerformSizing(Node node)
    {
        var rect = GetGlobalRect();
        
        switch (FillMode)
        {
            case PropFillMode.Centered when node is Node2D node2D:
                node2D.GlobalPosition = rect.GetCenter();
                break;
            case PropFillMode.Fill when node is Node2D node2D:
                node2D.GlobalPosition = rect.GetCenter();
                node2D.Set("size", rect.Size);
                break;

            case PropFillMode.Centered when node is Control ctrl:
                ctrl.GlobalPosition = rect.GetCenter();
                break;
            case PropFillMode.Fill when node is Control ctrl:
                ctrl.GlobalPosition = rect.Position;
                ctrl.Size = rect.Size;
                break;

        }
    }

    public override void _Process(double delta)
    {
        // if (Prop == null || !Engine.IsEditorHint()) return;
        
        PerformSizing(Prop);
    }

}