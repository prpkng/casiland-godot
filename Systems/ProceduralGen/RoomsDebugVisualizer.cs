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

    private void DrawText(Vector2 pos, string text) =>
        DrawString(_font, pos, text, HorizontalAlignment.Center, -1, 
            _fontSize, Colors.White);

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
            DrawText(room.Center * GridSize, $"ID: {room.Id}");
        }
		
	
        foreach (var  edge in state.CorridorLines?? []) {
            DrawLine(edge.From * GridSize, edge.To * GridSize, MstLineColor);
        }
	
        foreach (var  edge in state.MinimumSpanningTree?? [])
        {
            var center = edge.FromF + edge.FromF.DirectionTo(edge.ToF) * edge.EuclideanLength / 2f;
            center *= GridSize;
            DrawText(center + Vector2.Up * 12f, $"Ec. Length: {edge.EuclideanLength}");
            DrawText(center + Vector2.Down * 12f, $"Ar. Length: {edge.ArithmeticLength}");
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
            DrawText(room.Center * GridSize, $"ID: {room.Id}");
            DrawText(room.Center * GridSize + Vector2.Down*24, $"StartDepth: {room.StartDistance}");
            DrawText(room.Center * GridSize + Vector2.Down*48, $"BossDepth: {room.BossDistance}");
            DrawText(room.Center * GridSize + Vector2.Down*64, $"Bias: {room.ProgressBias}");

        }
		

        foreach (var  room in state.OtherRooms ?? []) {
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), OtherRoomsColor/4, true);
            DrawRect(new Rect2(room.Rect.Position * GridSize, room.Size * GridSize), OtherRoomsBorder/4, false, 1);
        }

        DrawText(new Vector2(10, 20), $"Seed: {state.Rng.Seed}");

        List<ProceduralRoom> rooms = [..(state.MainRooms ?? []), ..(state.CorridorRooms ?? [])];
        // Draw arrows
        foreach (var room in rooms)
        foreach (var (dir, connections) in room.Neighbors)
        foreach (var (corridor, endpoint) in connections)
        {
            var directionVector = dir switch
            {
                RoomNeighborDirection.Up => Vector2.Up,
                RoomNeighborDirection.Down => Vector2.Down,
                RoomNeighborDirection.Left => Vector2.Left,
                RoomNeighborDirection.Right => Vector2.Right,
                _ => throw new System.ArgumentOutOfRangeException(),
            };

            var endpointRoom = endpoint == 0 ? corridor.FromRoom : corridor.ToRoom;
            
            
            

            var end = endpointRoom.Rect.CastTowardsPerimeter(directionVector) + directionVector * 4;
            var start = endpointRoom.Rect.CastTowardsPerimeter(directionVector);
            DrawArrow(start * GridSize, end * GridSize, Colors.IndianRed, 2);
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