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

    [Export]
    public int Fps
    {
        get => _fps;
        set
        {
            _fps = value;
            _updateTime = 1.0 / _fps;
        }
    }
    private int _fps = -1;
    private double _updateTime = -1;

    [Export] private Node2D _root;
    
    [ExportToolButton("Regenerate Stack")]
    public Callable RegenerateStackCallable => Callable.From(this.RegenerateStack);
    
    public override void _Ready()
    {
    }

    private void RegenerateStack()
    {
        foreach (var c in GetChildren()) c.QueueFree();

        _root = new Node2D
        {
            Name = "Root"
        };
        AddChild(_root);
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
                Texture = StackTexture,
                Name = $"Sprite {i}"
            };
            _root.AddChild(sprite);
            StackSprites.Add(sprite);
            sprite.Owner = GetTree().GetEditedSceneRoot();
        }
    }

    private double _counter = 0f;
    private double LastRotation;

    public override void _Process(double delta)
    {
        _root.GlobalRotation = 0;
        _counter += delta;
        if (_root == null) return;
        if (_updateTime > 0 && _counter < _updateTime)
            return;

        _counter = 0;
        
        if (GlobalRotation == LastRotation) return;
        LastRotation = GlobalRotation;
        
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