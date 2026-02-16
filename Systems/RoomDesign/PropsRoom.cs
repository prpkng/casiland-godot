using Godot;
using System;

namespace Casiland.Systems.RoomDesign;

[Tool]
public partial class PropsRoom : ReferenceRect
{
    [Export] public int GridSize = 16;
    [Export] public Texture2D BackgroundSprite;

    private Font _font;
    private int _fontSize;

    public override void _Ready()
    {
        base._Ready();
        _font = ThemeDB.GetFallbackFont();
        _fontSize = ThemeDB.GetFallbackFontSize();
    }


    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) QueueRedraw();
    }



    private void DrawText(Vector2 pos, string text, int? fontSize = null)
    {
        var size = fontSize ?? _fontSize;
        DrawString(_font, pos + Vector2.Left * 200f + Vector2.Up * size/2f, text, HorizontalAlignment.Center, 400f,
            size, Colors.White);
    }


    public override void _Draw()
    {
        var rect = GetGlobalRect();
        if (BackgroundSprite != null)
            DrawTextureRect(BackgroundSprite, rect, true);

        var top = rect.GetCenter() + rect.Size / 2f * Vector2.Up;
        DrawText(top, $"{Size / GridSize}", _fontSize/4);        


        foreach (var child in GetChildren())
        {
            if (child is not Control c) continue;
            rect = c.GetGlobalRect();
            top = rect.GetCenter() + rect.Size / 2f * Vector2.Up;
            DrawText(top, $"{c.Size / GridSize}", _fontSize/4);  
        }
    }

}
