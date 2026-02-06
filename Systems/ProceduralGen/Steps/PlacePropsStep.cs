using System.Collections.Generic;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlacePropsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => $"Placing props";
    private void PlaceDoors()
    {
        
    }
    
    public override async GDTask Perform()
    {
        PlaceDoors();
    }
}