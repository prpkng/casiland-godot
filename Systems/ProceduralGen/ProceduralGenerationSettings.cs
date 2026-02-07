using Casiland.AutoTileEx.Api.Data;
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

    [Export] public int MinBaseRoomSize = 22;
    [Export] public int MaxBaseRoomSize = 26;
    [Export] public int MaxRoomSizeDeviation = 5;
 
    [Export] public float BaseRoomAspect = 1.2f;
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
    /// When creating corner corridors, checks if each side of the corner isn't N times bigger than the other one. (being N this variable)
    /// </summary>
    /// <remarks>
    /// If X is N times bigger than Y (and vice versa), creates an S-shaped corridor instead. 
    /// </remarks>
    [Export] public float MaxCornerSizeDifference = 2.4f;

    /// <summary>
    /// Every time the main room distances exceed a multiple of this number, a new room is added to the corridor.
    /// </summary>
    [Export] public int InBetweenRoomsDenominator = 26;

    /// <summary>
    /// The maximum distance that a corridor line can pass by (in both axes) from the center of its room. This ensures no corridor line 
    /// is too near the room boundaries. Tweak this number along with the room boundaries
    /// </summary>
    [Export] public int CorridorMaxDistanceToCenter = 6;


    [ExportGroup("Corridor Props")]
    [Export] public PackedScene DoorScene;

    
    [ExportGroup("Tile Settings")]
    [Export] public AutoTileRuleSet AutoTileRuleSet;

    [Export] public int CorridorTileWidth = 6;
}