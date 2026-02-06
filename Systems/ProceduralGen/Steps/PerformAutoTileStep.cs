using Casiland.AutoTileEx.Api;
using Fractural.Tasks;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PerformAutoTileStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => "Performing auto-tiling";
    public override async GDTask Perform()
    {
        AutoTileImplementation.PerformAutoTile(State.TilemapLayer, Settings.AutoTileRuleSet);
    }
}