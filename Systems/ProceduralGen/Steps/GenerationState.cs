using System.Collections.Generic;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class GenerationState
{
    public RandomNumberGenerator Rng;

    public List<ProceduralRoom> GeneratedRooms = [];

    public List<ProceduralRoom> MainRooms = [];
    public List<ProceduralRoom> OtherRooms = [];
    public List<ProceduralRoom> CorridorRooms = [];

    public List<LineSegment> MinimumSpanningTree = [];
    public List<LineSegment> CorridorLines = [];

    public Dictionary<Vector2I, ProceduralRoom> PointToRoom = [];


}