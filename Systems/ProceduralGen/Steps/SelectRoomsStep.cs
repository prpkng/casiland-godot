using System.Collections.Generic;
using System.Linq;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class SelectRoomStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => "Selecting rooms";
    
    private List<List<ProceduralRoom>> ComputeGroupRooms()
    {
        var groups = new List<List<ProceduralRoom>>();

        foreach (var room in State.AllRooms)
        {
            
        }


        return groups;
    }
    
    public override async GDTask Perform()
    {
        
        var groups = ComputeGroupRooms();
    }
}