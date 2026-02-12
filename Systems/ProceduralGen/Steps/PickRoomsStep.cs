using System.Collections.Generic;
using System.Linq;
using Fractural.Tasks;
using Serilog;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PickMainRoomsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    private List<ProceduralRoom> _mainRooms = [];
    private List<ProceduralRoom> _otherRooms = [];

    public override string StateDescription => $"Picking {_mainRooms?.Count} main rooms and {_otherRooms?.Count} other rooms";


    private bool CheckValidMainRoom(
        ProceduralRoom room,
        float widthThresh,
        float heightThresh)
    {
        if (_mainRooms.Contains(room))
            return false;

        if (_mainRooms.Any(other => room.Center.DistanceTo(other.Center) < Settings.MinRoomDistance))
            return false;

        return room.Rect.Size.X > widthThresh && room.Rect.Size.Y > heightThresh;
    }

    public void CalculateMainRooms(List<ProceduralRoom> rooms)
    {
        // Sort rooms by AREA
        rooms.Sort((a, b) => a.Rect.Area.CompareTo(b.Rect.Area));

        _mainRooms.Clear();
        _otherRooms.Clear();

        float widthThresh = Settings.RoomWidthThreshold;
        float heightThresh = Settings.RoomHeightThreshold;

        const int maxIter = 50;
        int iterIndex = 0;

        float mainRoomsCount = State.Rng.RandfRange(Settings.MinRoomCount, Settings.MaxRoomCount);

        while (_mainRooms.Count < mainRoomsCount && iterIndex < maxIter)
        {
            foreach (var room in rooms.Where(room => _mainRooms.Count < Settings.MaxRoomCount &&
                                                     CheckValidMainRoom(room, widthThresh, heightThresh)))
            {
                room.Index = _mainRooms.Count;
                _mainRooms.Add(room);
            }

            widthThresh -= 1;
            heightThresh -= 1;

            iterIndex++;
        }
        
        if (_mainRooms.Count < mainRoomsCount)
            Log.Error("Could not create the desired amount of main rooms {Idx}\n" +
                      "Perhaps something gone wrong or the MAX_ITER={MaxIter} is too low?", mainRoomsCount, maxIter);

        foreach (var room in rooms.Except(_mainRooms))
        {
            room.Index = _otherRooms.Count;
            _otherRooms.Add(room);
        }
        
        // Clear the old Generated Rooms array
        State.GeneratedRooms.Clear();
    }

    public override async GDTask Perform()
    {
        CalculateMainRooms(State.GeneratedRooms);

        State.MainRooms = _mainRooms;
        State.OtherRooms = _otherRooms;
    }
}