using Casiland.AutoTileEx.Api;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PerformAutoTileStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => "Performing auto-tiling";
    public override void Perform()
    {
        AutoTileImplementation.PerformAutoTile(State.TilemapLayer, Settings.AutoTileRuleSet);
    }
}