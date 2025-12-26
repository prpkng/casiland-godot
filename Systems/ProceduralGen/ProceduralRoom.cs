using System.Collections.Generic;
using Godot;

namespace Casiland.Systems.ProceduralGen;

public class ProceduralRoom(Vector2 pos, Vector2 size)
{
    public Rect2 Rect = new(pos, size);

    public Vector2 Position => Rect.GetCenter();
    public Vector2 Size => Rect.Size;

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
}