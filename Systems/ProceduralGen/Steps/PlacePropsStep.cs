using System;
using System.Collections.Generic;
using Casiland.Entities.World.Dungeons.Doors;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlacePropsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    private int _gridSize;
    public override string StateDescription => $"Placing props";
    private void PlaceDoors()
    {
        
        foreach (var room in State.AllRooms) 
        foreach (var (dir, nb) in room.Neighbors)
        foreach (var (corridor, endpoint) in nb)
        {
            var direction = dir.ToVector2();
            var point = endpoint == 0 ? corridor.FromPos : corridor.ToPos;
            var door = Settings.DoorScene.Instantiate<DoubleDoors>();
            State.Payload.PropsGroup.AddChild(door);
            door.Position = (point + direction/2f) * _gridSize;
            door.Position += direction.Orthogonal().Abs() * _gridSize;
            door.Rotation = (-direction).Angle();

            // Fix the door alignment when connecting down (because of tileset illusion)
            if (dir == RoomNeighborDirection.Down)
                door.Position += Vector2.Down * _gridSize * 2;
            
        }
    }
    
    public override async GDTask Perform()
    {
        _gridSize = State.Payload.AutoTileLayer.TileSet.TileSize.X;
        PlaceDoors();
    }
}