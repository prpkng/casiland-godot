using System;
using System.Collections.Generic;
using System.Linq;
using Casiland.Entities.World.Dungeons.Doors;
using Casiland.Systems.Debug;
using Casiland.Systems.ProceduralGen.Algorithms;
using Casiland.Systems.RoomDesign;
using Casiland.Systems.RoomDesign.Rooms;
using Fractural.Tasks;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlacePropsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    private int _gridSize;
    public override string StateDescription => $"Placing props";

    private void SetupRoomNodes()
    {
        foreach (var room in State.AllRooms)
        {
            var node = new Node2D()
            {
                Name = $"Room {room.Id}",
                YSortEnabled = true
            };

            State.Payload.PropsGroup.AddChild(node);
            room.RoomNode = node;


            node.GlobalPosition = room.Rect.Position * 16;
        }
    }
    private void PlaceDoors()
    {

        foreach (var room in State.AllRooms)
            foreach (var (dir, nb) in room.Neighbors)
                foreach (var (corridor, endpoint) in nb)
                {
                    var direction = dir.ToVector2();
                    var point = endpoint == 0 ? corridor.FromPos : corridor.ToPos;
                    var door = Settings.DoorScene.Instantiate<DoubleDoors>();
                    door.Name = $"Door {Enum.GetName(dir)}";
                    room.RoomNode.AddChild(door);
                    door.GlobalPosition = (point + direction / 2f) * _gridSize;
                    door.GlobalPosition += direction.Orthogonal().Abs() * _gridSize;
                    door.Rotation = (-direction).Angle();

                    // Fix the door alignment when connecting down (because of tileset illusion)
                    if (dir == RoomNeighborDirection.Down)
                        door.GlobalPosition += Vector2.Down * _gridSize * 2;

                }
    }

    private RoomPropCollection GetRoomPropCollection(ProceduralRoom room)
    {
        //TODO implement constraints here!
        return Settings.PropCollections.First();
    }

    private async GDTask PlacePropContainer(Control control, PropContainer propContainer, ProceduralRoom room)
    {
        var prop = propContainer.Prop;
        propContainer.PerformSizing(prop);

        Vector2 pos = Vector2.Zero;

        {
            if (prop is Node2D nd) pos = nd.GlobalPosition;
            else if (prop is Control ctrl) pos = ctrl.GlobalPosition;
        }

        propContainer.RemoveChild(prop);
        room.RoomNode.AddChild(prop);
        {
            if (prop is Node2D nd) nd.Position = pos.RoundToInt();
            else if (prop is Control ctrl) ctrl.Position = pos.RoundToInt();
        }
    }

    private async GDTask PlaceProps()
    {
        foreach (var room in State.AllRooms)
        {
            var props = GetRoomPropCollection(room);

            var control = props.PropsScene.Instantiate<Control>();
            State.Payload.PropsGroup.AddChild(control);

            await GDTask.Yield();
            control.Size = room.Size * _gridSize;
            control.Position = Vector2.Zero;
            control.ForceUpdateTransform();
            await GDTask.Yield();

            foreach (var c in control.FindChildren("*"))
            {
                if (c is PropContainer propContainer) await PlacePropContainer(control, propContainer, room);
            }
            State.Payload.PropsGroup.RemoveChild(control);
            control.QueueFree();
        }
    }

    public override async GDTask Perform()
    {
        _gridSize = State.Payload.AutoTileLayer.TileSet.TileSize.X;
        SetupRoomNodes();

        PlaceDoors();

        await PlaceProps();

    }
}