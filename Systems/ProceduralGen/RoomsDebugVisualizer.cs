using System.Collections.Generic;
using Godot;

namespace Casiland.Systems.ProceduralGen;

public partial class RoomsDebugVisualizer : Node2D
{
    private static readonly Color GenRoomsColor = new("#ffafaf", 0.4f);
    private static readonly Color GenRoomsBorder = new("#dd5f5f", 0.6f);
    private static readonly Color MainRoomsColor = new("#afffaf", 0.4f);
    private static readonly Color MainRoomsBorder = new("#5fdd5f", 0.6f);
    private static readonly Color OtherRoomsColor = new("#afafff", 0.3f);
    private static readonly Color OtherRoomsBorder = new("#5f5fdd", 0.3f);
    private static readonly Color CorridorRoomsColor = new("#ffffaf", 0.15f);
    private static readonly Color CorridorRoomsBorder = new("#dddd5f", 0.3f);
    private static readonly Color TriangleLineColor = new("#6fff6f", 0.5f);

    private static readonly Color MstLineColor = Colors.Yellow;

    
    [Export] private ProceduralRoomGenerator _generator;

    private Font _font;
    private int _fontSize = 24;

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

    private void DrawArrow(Vector2 from, Vector2 to, Color color, float width = 1f, float arrowSize = 10f)
    {
        DrawLine(from, to, color, width);
        
        Vector2 direction = (to - from).Normalized();
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
        
        Vector2 arrowPoint1 = to - direction * arrowSize + perpendicular * arrowSize * 0.5f;
        Vector2 arrowPoint2 = to - direction * arrowSize - perpendicular * arrowSize * 0.5f;
        
        DrawLine(to, arrowPoint1, color, width);
        DrawLine(to, arrowPoint2, color, width);
    }

    public override void _Draw()
    {
        var state = _generator._currentState;
        if (state == null) return;

        foreach (var room in state.GeneratedRooms ?? [])
        {
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), GenRoomsColor, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), GenRoomsBorder, false, 1);
        }	

        foreach (var  room in state.CorridorRooms ?? []) {
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), CorridorRoomsColor, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), GenRoomsBorder, false, 1);
            DrawString(_font, room.Center * GridSize, state.CorridorRooms.IndexOf(room).ToString(), HorizontalAlignment.Center, -1, 
                _fontSize, Colors.White);
        }
		
	
        foreach (var  edge in state.CorridorLines?? []) {
            DrawLine(edge.From * GridSize, edge.To * GridSize, MstLineColor);
        }
	
        foreach (var  edge in state.MinimumSpanningTree?? []) {
            DrawLine(edge.From * GridSize, edge.To * GridSize, TriangleLineColor);
        }
		
        foreach (var  room in state.MainRooms ?? []) {

            var (color, border) = room.RoomType switch
            {
                RoomTypes.StartRoom => (Colors.Green, Colors.Lime),
                RoomTypes.BossRoom => (Colors.IndianRed, Colors.OrangeRed),
                _ => (MainRoomsColor, MainRoomsBorder)
            };

            // foreach (var conn in room.Connections)
            //     DrawLine(room.Center * GridSize, conn.Center * GridSize, MstLineColor);

            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), color, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), border, false, 1);
            DrawString(_font, room.Center * GridSize, $"ID: {room.Index}", HorizontalAlignment.Center, -1, 
            _fontSize, Colors.White);
            DrawString(_font, room.Center * GridSize + Vector2.Down*24, $"StartDepth: {room.StartDistance}", HorizontalAlignment.Center, -1, 
            _fontSize, Colors.White);
            DrawString(_font, room.Center * GridSize + Vector2.Down*48, $"BossDepth: {room.BossDistance}", HorizontalAlignment.Center, -1, 
            _fontSize, Colors.White);
            DrawString(_font, room.Center * GridSize + Vector2.Down*64, $"Bias: {room.ProgressBias}", HorizontalAlignment.Center, -1, 
            _fontSize, Colors.White);

        }
		

        foreach (var  room in state.OtherRooms ?? []) {
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), OtherRoomsColor/4, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), OtherRoomsBorder/4, false, 1);
        }

        DrawString(_font, new Vector2(10, 20), $"Seed: {state.Rng.Seed}", HorizontalAlignment.Left, -1, 
            _fontSize, Colors.White);

        List<ProceduralRoom> rooms = [..(state.MainRooms ?? []), ..(state.CorridorRooms ?? [])];
        // Draw arrows
        foreach (var room in rooms)
        {
            foreach (var dir in room.ConnectionDirections)
            {
                var vec = dir switch
                {
                    Directions.Up => Vector2.Up,
                    Directions.Down => Vector2.Down,
                    Directions.Left => Vector2.Left,
                    Directions.Right => Vector2.Right,
                    _ => throw new System.NotImplementedException(),
                };

                var end = room.Center + room.Size / 2f * vec + vec * 4;
                var start = room.Center + room.Size / 2f * vec;
                DrawArrow(start * GridSize, end * GridSize, Colors.IndianRed, 2);
            }
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