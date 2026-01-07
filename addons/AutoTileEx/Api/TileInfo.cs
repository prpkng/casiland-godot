using Godot;

namespace Casiland.AutoTileEx.Api;

public readonly record struct TileInfo(Vector2I Pos, int Source)
{
    public bool IsValid() => Pos != -Vector2I.One && Source != -1;
    public bool Equals(TileInfo? other)
    {
        return other?.Pos == Pos && other?.Source == Source;
    }
}