using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Casiland.Common.SpriteStacking;

[Tool]
[GlobalClass]
public partial class StackedSprite : Node2D
{
    [Export] public Texture2D StackTexture;

    [Export] public int HFrames { get; set; } = 1;
    [Export] public int VFrames { get; set; } = 1;

    [Export] public float Spacing { get; set; } = 4f;
    [Export] public Array<Sprite2D> StackSprites;

    [Export] private Node2D _root;
    
    [ExportToolButton("Regenerate Stack")]
    public Callable RegenerateStackCallable => Callable.From(this.RegenerateStack);
    
    public override void _Ready()
    {
    }

    private void RegenerateStack()
    {
        foreach (var c in GetChildren()) c.QueueFree();

        var node = new Node();
        AddChild(node);
        node.Owner = GetTree().GetEditedSceneRoot();
        _root = new Node2D();
        node.AddChild(_root);
        _root.Owner = GetTree().GetEditedSceneRoot();

        StackSprites = [];
        for (int i = 0; i < HFrames * VFrames; i++)
        {
            var sprite = new Sprite2D
            {
                Hframes = HFrames,
                Vframes = VFrames,
                Frame = i,
                Position = new Vector2(0, i * Spacing),
                Texture = StackTexture
            };
            _root.AddChild(sprite);
            StackSprites.Add(sprite);
            sprite.Owner = GetTree().GetEditedSceneRoot();
        }
    }

    public override void _Process(double delta)
    {
        _root.GlobalPosition = GlobalPosition;
        
        if (StackTexture == null || HFrames <= 0 || VFrames <= 0) return;

        for (int i = 0; i < StackSprites.Count; i++)
        {
            var sprite = StackSprites[i];
            if (Engine.IsEditorHint())
            {
                sprite.Hframes = HFrames;
                sprite.Vframes = VFrames;
                sprite.Frame = i;
                sprite.Position = new Vector2(0, i * Spacing);
            }

            sprite.Rotation = GlobalRotation;
        }
    }
}