using Godot;

namespace Casiland.Systems.ProceduralGen;

public partial class ProceduralGenerationSettings : Resource
{
    [Export] public int StartCellCount = 50;
    [Export] public int StartCellsRadius = 64;

    [Export] public int MinRoomCount = 5;
    [Export] public int MaxRoomCount = 10;
    [Export] public int MinRoomDistance = 90;

    [Export] public int MinRoomWidth = 20;
    [Export] public int MaxRoomWidth = 40;
    [Export] public int MinRoomHeight = 20;
    [Export] public int MaxRoomHeight = 30;

    /// 'Aspect deviation' means how wide OR tall a room can be, in the range [0.0-1.0]
    [Export] public float MaxRoomAspectDeviation = 0.4f;

    [Export] public int RoomWidthThreshold = 45;
    [Export] public int RoomHeightThreshold = 25;

    [Export] public int LoopCount = 2;

    // The amount of pixels two rooms need to be overlapping on a single axis for a DIRECT corridor to be generated. [br]
    // If the requirements aren't met, an S-shaped or corner-shaped corridor will be generated instead.
    // This prevents the corridors from being generated TOO close from the edges
    [Export] public int MinimumDirectCorridorOverlapLength = 8;

    /// The maximum amount of pixels two rooms need to be apart on a single axis for a CORNER corridor to be generated.
    /// 
    /// If the requirements aren't met, an S-shaped will be generated instead.
    [Export] public int MaximumCornerCorridorOverlapLength = 128;
}