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
    
    private static bool RuleMatches(AutoTileRule rule, TileMapLayer layer, Vector2I pos)
    {
        var tileCoords = layer.GetCellAtlasCoords(pos);
        int tileId = layer.GetCellSourceId(pos);
        if (tileId != rule.SourceTileSourceId || tileCoords != rule.SourceTileCoords) return false;

        for (int i = 0; i < rule.Neighbours.Count; i++)
        {
            var type = rule.Neighbours[i];
            if (type == NeighbourType.Ignore) continue;

            var neighbourPos = pos + NeighborOffsets[i];
            var neighbourCoords = layer.GetCellAtlasCoords(neighbourPos);
            int neighbourId = layer.GetCellSourceId(neighbourPos);

            bool isMatch = (neighbourId == rule.SourceTileSourceId && neighbourCoords == rule.SourceTileCoords);
            bool isEmpty = neighbourId == -1; // Empty cell = -1

            switch (type)
            {
                case NeighbourType.Match when !isMatch:
                case NeighbourType.Nothing when !isEmpty:
                case NeighbourType.Other when isMatch:    
                    return false;
            }
        }

        return true;
    }
    
    public static void PerformAutoTile(TileMapLayer layer, Godot.Collections.Array<AutoTileRule> rules)
    {
        Dictionary<Vector2I, (int, Vector2I)> output = [];
        var used = layer.GetUsedCells();

        foreach (var pos in used)
        {
            (int, Vector2I)? chosenTile = null;
            
            foreach (var rule in rules)
                if (RuleMatches(rule, layer, pos))
                    chosenTile = (rule.TargetTileSourceId, rule.TargetTileCoords);

            if (!chosenTile.HasValue) continue;
            output[pos] = chosenTile.Value;
        }

        foreach (var (pos, (id, tile)) in output)
            layer.SetCell(pos, id, tile);
    }
}