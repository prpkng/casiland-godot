using System.Collections.Generic;
using System.Linq;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PickRoomsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
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

        if (_mainRooms.Any(other => room.Position.DistanceTo(other.Position) < Settings.MinRoomDistance))
            return false;

        return room.Rect.Size.X > widthThresh && room.Rect.Size.Y > heightThresh;
    }
    
    public void CalculateMainRooms(List<ProceduralRoom> rooms)
    {
        rooms.Sort((a, b) => a.Rect.Area.CompareTo(b.Rect.Area));

        _mainRooms.Clear();
        _otherRooms.Clear();

        float widthThresh = Settings.RoomWidthThreshold;
        float heightThresh = Settings.RoomHeightThreshold;

        const int maxIter = 50;
        int iter = 0;

        float roomCount = State.Rng.RandfRange(Settings.MinRoomCount, Settings.MaxRoomCount);

        while (_mainRooms.Count < roomCount && iter < maxIter)
        {
            foreach (var room in rooms.Where(room => _mainRooms.Count < Settings.MaxRoomCount &&
                                                     CheckValidMainRoom(room, widthThresh, heightThresh)))
            {
                _mainRooms.Add(room);
            }

            widthThresh -= 1;
            heightThresh -= 1;

            iter++;
        }

        foreach (var room in rooms)
        {
            if (!_mainRooms.Contains(room))
                _otherRooms.Add(room);
        }

        State.GeneratedRooms.Clear();
    }
    
    public override void Perform()
    {
        CalculateMainRooms(State.GeneratedRooms);

        State.MainRooms = _mainRooms;
        State.OtherRooms = _otherRooms;
    }
}