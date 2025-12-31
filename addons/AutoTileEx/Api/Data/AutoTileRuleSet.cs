using Godot;
using Godot.Collections;
using Serilog;

namespace Casiland.AutoTileEx.Api.Data;

[Tool]
[GlobalClass]
public partial class AutoTileRuleSet : Resource
{
    [Export] public TileSet tileSet;
    [Export] public Array<AutoTileRule> Rules { get; set; } = [];

    [ExportToolButton("Recompute All Rule Masks")]
    public Callable RecomputeRuleMasks => new(this, MethodName.RecomputeAllRuleMasks);
    
    public void RecomputeAllRuleMasks()
    {
        Log.Information("Recomputing all rule masks...");
        foreach (var rule in Rules)
            rule.RecomputeMasks();
    }
    
}