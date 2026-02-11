using System;
using System.Collections.Generic;
using Casiland.AutoTileEx.Api.Data;
using Godot;

namespace Casiland.AutoTileEx.Api;

public static class AutoTileImplementation
{

    /// Offsets in clockwise order: N, NE, E, SE, S, SW, W, NW
    private static readonly Vector2I[] NeighborOffsets =
    [
        new Vector2I(0, -1),  // N
        new Vector2I(1, -1),  // NE
        new Vector2I(1, 0),   // E
        new Vector2I(1, 1),   // SE
        new Vector2I(0, 1),   // S
        new Vector2I(-1, 1),  // SW
        new Vector2I(-1, 0),  // W
        new Vector2I(-1, -1)  // NW
    ];
    
    //TODO continue from here, finish bitmask implementation
    
    private static void BuildMasks(
        TileMapLayer map,
        Vector2I anchor,
        int radius,
        TileInfo sourceTile,
        ulong[] same,
        ulong[] empty,
        ulong[] other)
    {
        int size = radius * 2 + 1;
        int index = 0;

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                Vector2I p = anchor + new Vector2I(x, y);
                var tile = new TileInfo(map.GetCellAtlasCoords(p), map.GetCellSourceId(p));

                int chunk = index / 64;
                int bit   = index % 64;
                ulong flag = 1UL << bit;

                if (!tile.IsValid())
                    empty[chunk] |= flag;
                else if (tile == sourceTile)
                    same[chunk] |= flag;
                else
                    other[chunk] |= flag;

                index++;
            }
        }
    }
    
    private static bool MatchRule(
        Vector2I pos,
        ulong[] same,
        ulong[] empty,
        ulong[] other,
        AutoTileRule rule)
    {
        if (rule.ModuloX > 1 && (pos.X + rule.ModuloXOffset) % rule.ModuloX != 0)
            return false;
        if (rule.ModuloY > 1 && (pos.Y + rule.ModuloYOffset) % rule.ModuloY != 0)
            return false;
        for (int i = 0; i < rule.ArrayChunkCount; i++)
        {
            if ((same[i] & rule.RequiredMask[i]) != rule.RequiredMask[i])
                return false;

            if ((empty[i] & rule.EmptyMask[i]) != rule.EmptyMask[i])
                return false;

            if ((other[i] & rule.OtherMask[i]) != rule.OtherMask[i])
                return false;
        }
        return true;
    }


    public static void PerformAutoTile(TileMapLayer layer, AutoTileRuleSet rules) => PerformAutoTile(layer, layer, rules);

    public static void PerformAutoTile(TileMapLayer source, TileMapLayer dest, AutoTileRuleSet rules)
    {
        Dictionary<Vector2I, (int, Vector2I)> output = [];
        var used = source.GetUsedCells();

        foreach (var pos in used)
        {
            (int, Vector2I)? chosenTile = null;
            ulong[] requiredMask = new ulong[3];
            ulong[] emptyMask = new ulong[3];
            ulong[] otherMask = new ulong[3];
            BuildMasks(source, pos, AutoTileRule.Radius, new TileInfo(rules.SourceTileCoords, rules.SourceTileSourceId), requiredMask, emptyMask, otherMask);
            foreach (var rule in rules.Rules)
            {
                if (!MatchRule(pos, requiredMask, emptyMask, otherMask, rule))
                    continue;

                chosenTile = (rule.TargetTileSourceId, rule.TargetTileCoords);
            }

            if (!chosenTile.HasValue) continue;
            output[pos] = chosenTile.Value;
        }

        foreach (var (pos, (id, tile)) in output)
            dest.SetCell(pos, id, tile);
    }
}