using System;
using System.Collections.Generic;
using Casiland.Entities.World.Dungeons.Doors;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PickRoomTypesStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => "picking room types";

    public override async GDTask Perform()
    {
        
    }
}