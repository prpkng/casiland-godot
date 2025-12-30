using Godot;

namespace Casiland.Systems.ProceduralGen;

public partial class ProceduralGenerationSettings : Resource
{
    [ExportGroup("Initial Generation Parameters")]
    [Export] public int StartCellCount = 50;
    [Export] public int StartCellsRadius = 64;

    [ExportGroup("Initial Room Constraints")]
    [Export] public int MinRoomCount = 5;
    [Export] public int MaxRoomCount = 10;
    [Export] public int MinRoomDistance = 90;

    [Export] public int MinRoomWidth = 20;
    [Export] public int MaxRoomWidth = 40;
    [Export] public int MinRoomHeight = 20;
    [Export] public int MaxRoomHeight = 30;

    /// <summary>'Aspect deviation' means how wide OR tall a room can be, in the range [0.0-1.0] </summary>
    [Export] public float MaxRoomAspectDeviation = 0.4f;

    [Export] public int RoomWidthThreshold = 45;
    [Export] public int RoomHeightThreshold = 25;
    
    [ExportGroup("Misc Connection Parameters")]
    
    [Export] public int LoopCount = 2;
    
    
    
    /// <summary>
    /// The amount of pixels two rooms need to be overlapping on a single axis for a DIRECT corridor to be generated.
    /// If the requirements aren't met, an S-shaped or corner-shaped corridor will be generated instead.
    /// This prevents the corridors from being generated TOO close from the edges
    /// </summary>
    [ExportGroup("Corridor Parameters")]
    [Export] public int MinimumDirectCorridorOverlapLength = 8;

    /// <summary>
    /// The maximum amount of pixels two rooms need to be apart on a single axis for a CORNER corridor to be generated.
    /// </summary>
    /// <remarks>
    /// If the requirements aren't met, an S-shaped will be generated instead.
    /// </remarks>
    [Export] public int MaximumCornerCorridorOverlapLength = 128;

    /// <summary>
    /// Defines the maximum length for a corridor before another room is generated in-between
    /// </summary>
    [Export] public int MaximumCorridorLength = 24;
}