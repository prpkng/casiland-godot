using System.Collections.Generic;
using System.Linq;
using Casiland.Systems.ProceduralGen.Algorithms;
using Godot;

namespace Casiland.Systems.ProceduralGen;

public enum RoomNeighborDirection
{
    Up,
    Down,
    Left,
    Right
}

public static class DirectionConversions
{
    public static readonly Dictionary<Vector2, RoomNeighborDirection> VecToDirDict = new()
    {
        { new Vector2(1, -1), RoomNeighborDirection.Up },
        { new Vector2(-1, -1), RoomNeighborDirection.Up },
        { Vector2.Up, RoomNeighborDirection.Up },
        { new Vector2(1, 1), RoomNeighborDirection.Down },
        { new Vector2(-1, 1), RoomNeighborDirection.Down },
        { Vector2.Down, RoomNeighborDirection.Down },
        { Vector2.Left, RoomNeighborDirection.Left },
        { Vector2.Right, RoomNeighborDirection.Right }
    };
}

public enum RoomTypes
{
    NormalRoom,
    CorridorRoom,
    StartRoom,
    BossRoom
}

public static class SimpleRoomIdGenerator
{
    private static int _counter = 0;
    public static int NextId() => ++_counter;
    public static void Reset() => _counter = 0;
}

public class ProceduralRoom(Vector2 pos, Vector2 size)
{
    public Rect2 Rect = new(pos, size);

    public readonly int Id = SimpleRoomIdGenerator.NextId();
    public Vector2 Center => Rect.GetCenter();
    public Vector2 Size => Rect.Size;
    

    /// <summary>
    /// All rooms that this room is connected to via corridors.
    /// </summary>
    public readonly List<ProceduralRoom> Connections = [];
    public readonly List<RoomNeighborDirection> ConnectionDirections = [];

    public readonly Dictionary<RoomNeighborDirection, List<(CorridorShape shape, int endpoint)>> Neighbors =
        new()
        {
            { RoomNeighborDirection.Up, [] },
            { RoomNeighborDirection.Down, [] },
            { RoomNeighborDirection.Left, [] },
            { RoomNeighborDirection.Right, [] },
        };

    public RoomTypes RoomType = RoomTypes.NormalRoom;

    public void AddConnection(ProceduralRoom room)
    {
        Connections.Add(room);
    }

    public bool HasConnection(ProceduralRoom room)
    {
        return room.Connections.Contains(this) || Connections.Contains(room);
    }

    public int GetConnectionsCount() => Connections.Count;


    public List<Vector2> GetConnectionSides() =>
        Connections.Select(conn => Center.DirectionTo(conn.Center).Sign()).ToList();


    /// <summary>
    /// The list of corridor lines that make up the connections from this room to others.
    /// </summary>
    /// <remarks>
    /// <para>This only includes the direct connected lines, so S-shaped 
    /// and corner-shaped corridors subsequent lines are not included.</para>
    /// <para>This is populated during corridor placement, so using this
    /// before the corridor placement step is an expected null.</para>
    /// </remarks>
    public List<LineSegment> CorridorLines { get; set; } = null;

    public int Index;
    public int StartDistance;
    public int BossDistance;
    public int ProgressBias;
}