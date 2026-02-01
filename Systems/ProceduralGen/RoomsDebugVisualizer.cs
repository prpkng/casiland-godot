using Godot;

namespace Casiland.Systems.ProceduralGen;

public partial class RoomsDebugVisualizer : Node2D
{
    private static readonly Color GenRoomsColor = new("#ffafaf", 0.4f);
    private static readonly Color GenRoomsBorder = new("#dd5f5f", 0.6f);
    private static readonly Color MainRoomsColor = new("#afffaf", 0.4f);
    private static readonly Color MainRoomsBorder = new("#5fdd5f", 0.6f);
    private static readonly Color OtherRoomsColor = new("#afafff", 0.15f);
    private static readonly Color OtherRoomsBorder = new("#5f5fdd", 0.3f);
    private static readonly Color CorridorRoomsColor = new("#ffffaf", 0.15f);
    private static readonly Color CorridorRoomsBorder = new("#dddd5f", 0.3f);
    private static readonly Color TriangleLineColor = new("#6fff6f", 0.5f);

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

    private const int GridSize = 8;

    public override void _Draw()
    {
        var state = _generator.ResultingState;
        if (state == null) return;

        foreach (var room in state.GeneratedRooms)
        {
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), GenRoomsColor, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), GenRoomsBorder, false, 1);
        }	

        foreach (var  room in state.CorridorRooms) {
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), CorridorRoomsColor, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), GenRoomsBorder, false, 1);
            DrawString(_font, room.Position, state.CorridorRooms.IndexOf(room).ToString(), HorizontalAlignment.Center, -1, 
                12, Colors.White);
        }
		
	
        foreach (var  edge in state.CorridorLines) {
            DrawLine(edge.From * GridSize, edge.To * GridSize, MstLineColor);
        }
		
        foreach (var  room in state.MainRooms) {
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), MainRoomsColor, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), MainRoomsBorder, false, 1);
            DrawString(_font, room.Rect.Position * GridSize, state.MainRooms.IndexOf(room).ToString(), HorizontalAlignment.Center, -1, 
            12, Colors.White);

        }
		

        foreach (var  room in state.OtherRooms) {
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), OtherRoomsColor/4, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), OtherRoomsBorder/4, false, 1);
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