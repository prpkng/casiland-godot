using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Casiland.Systems.ProceduralGen;

public class ProceduralRoom(Vector2 pos, Vector2 size)
{
    public Rect2 Rect = new(pos, size);

    
    public Vector2 Position => Rect.GetCenter();
    public Vector2 Size => Rect.Size;

    /// <summary>
    /// All rooms that this room is connected to via corridors.
    /// </summary>
    public readonly List<ProceduralRoom> Connections = [];

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
        Connections.Select(conn => Position.DirectionTo(conn.Position).Sign()).ToList();


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
}