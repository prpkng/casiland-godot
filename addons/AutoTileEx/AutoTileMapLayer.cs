namespace Casiland.AutoTileEx;

using System.ComponentModel;
using Casiland.AutoTileEx.Api;
using Casiland.AutoTileEx.Api.Data;
using Godot;

[Tool]
[GlobalClass]
public partial class AutoTileMapLayer : TileMapLayer {

    [Export] public TileMapLayer SourceLayer;
    [Export] public AutoTileRuleSet RuleSet;

    [ExportToolButton("Compute Auto Tile")]
    public Callable RecalculateButton => Callable.From(this.RecalculateAutoTile);
    
    public void RecalculateAutoTile()
    {
        Clear();
        AutoTileImplementation.PerformAutoTile(SourceLayer, this, RuleSet);
    }
}