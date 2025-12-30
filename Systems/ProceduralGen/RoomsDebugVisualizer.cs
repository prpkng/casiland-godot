using Godot;

namespace Casiland.Systems.ProceduralGen;

public partial class RoomsDebugVisualizer : Node2D
{
    private static readonly Color GenRoomsColor = new Color("#ffafaf", 0.4f);
    private static readonly Color GenRoomsBorder = new Color("#dd5f5f", 0.6f);
    private static readonly Color MainRoomsColor = new Color("#afffaf", 0.4f);
    private static readonly Color MainRoomsBorder = new Color("#5fdd5f", 0.6f);
    private static readonly Color OtherRoomsColor = new Color("#afafff", 0.15f);
    private static readonly Color OtherRoomsBorder = new Color("#5f5fdd", 0.3f);
    private static readonly Color CorridorRoomsColor = new Color("#ffffaf", 0.15f);
    private static readonly Color CorridorRoomsBorder = new Color("#dddd5f", 0.3f);
    private static readonly Color TriangleLineColor = new Color("#6fff6f", 0.5f);

    private static readonly Color MstLineColor = Colors.Yellow;

    [Export] private ProceduralRoomGenerator _generator;

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
        QueueRedraw();
        base._Process(delta);
    }

    public override void _Draw()
    {
        var state = _generator.ResultingState;
        if (state == null) return;

        foreach (var room in state.GeneratedRooms)
        {
            DrawRect(room.Rect, GenRoomsColor, true);
            DrawRect(room.Rect, GenRoomsBorder, false, 1);
        }	

        foreach (var  room in state.CorridorRooms) {
            DrawRect(room.Rect, CorridorRoomsColor, true);
            DrawRect(room.Rect, GenRoomsBorder, false, 1);
            DrawString(_font, room.Position, state.CorridorRooms.IndexOf(room).ToString(), HorizontalAlignment.Center, -1, 
                12, Colors.White);
        }
		
	
        foreach (var  edge in state.CorridorLines) {
            DrawLine(edge.From, edge.To, MstLineColor);
        }
		
        foreach (var  room in state.MainRooms) {
            DrawRect(room.Rect, MainRoomsColor, true);
            DrawRect(room.Rect, MainRoomsBorder, false, 1);
            DrawString(_font, room.Position, state.MainRooms.IndexOf(room).ToString(), HorizontalAlignment.Center, -1, 
            12, Colors.White);

        }
		

        foreach (var  room in state.OtherRooms) {
            DrawRect(room.Rect, OtherRoomsColor/4, true);
            DrawRect(room.Rect, OtherRoomsBorder/4, false, 1);
        }


        // if (state.StartRoom != null) {
        //     DrawRect(state.StartRoom.Rect, new Color("#ff0000", 0.8f), true);
        //     DrawRect(state.StartRoom.Rect, new Color("#ff0000", 1), false, 1);
        // }
        //
        // if (state.BossRoom != null) {
        //     DrawRect(state.BossRoom.Rect, new Color("#00ff00", 0.8f), true);
        //     DrawRect(state.BossRoom.Rect, new Color("#00ff00", 1), false, 1);
        // }
    }
}