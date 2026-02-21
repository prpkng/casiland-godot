namespace Casiland.Systems.RoomDesign;

using System.Linq;
using Casiland.Systems.ProceduralGen;
using Godot;
using Godot.Collections;

[GlobalClass]
public  partial class RoomExitDirectionsConstraint : RoomPropConstraint {
    [Export] public Array<RoomNeighborDirection> Directions { get; set; } = [];

    public override bool CheckRoomFollowsConstraint(ProceduralRoom room)
    {
        return Directions.All(dir => {
            var hasExitInDir = room.Neighbors.Any(nb => nb.Key == dir);
            return hasExitInDir;
        });
    }
}