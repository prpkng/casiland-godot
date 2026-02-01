using System.Collections.Generic;
using Casiland.Systems.ProceduralGen.Algorithms;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlacePropsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => $"Placing props";
    private void PlaceDoors()
    {
        
    }
    
    public override void Perform()
    {
        PlaceDoors();
    }
}