using System.Collections.Generic;
using Casiland.Systems.ProceduralGen.Algorithms;
using Fractural.Tasks;
using Godot;

namespace Casiland.Systems.ProceduralGen.Steps;

public class PlaceRoomsStep(GenerationState state, ProceduralGenerationSettings settings) : GenerationStep(state, settings)
{
    public override string StateDescription => $"Generating {State.GeneratedRooms?.Count} rooms";
    
    private Vector2 RandomPointAroundCircle(int radius)
    {
        float angle = State.Rng.RandfRange(0, 2 * Mathf.Pi);
        return (Vector2.FromAngle(angle) * State.Rng.RandfRange(0, radius)).Round();
    }


    public List<ProceduralRoom> GenerateRooms(int count)
    {
        var rooms = new List<ProceduralRoom>();

        var parkMiller = new ParkMillerRng(State.Rng.RandiRange(0, 32767)); // Change this later

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = RandomPointAroundCircle(Settings.StartCellsRadius);

            Vector2 size = Vector2.Zero;
            const int maxIter = 100;

            for (int iter = 0; iter < maxIter; iter++)
            {
                float baseSize = parkMiller.NextFloat(Settings.MinBaseRoomSize, Settings.MaxBaseRoomSize);
                size = ProceduralGeometry.AspectWiseRandomSize(
                    State.Rng,
                    Settings.BaseRoomAspect,
                    Settings.MaxRoomAspectDeviation,
                    baseSize,
                    Settings.MaxRoomSizeDeviation
                );

                if (Mathf.Abs(size.Aspect() - 1f) < Settings.MaxRoomAspectDeviation)
                    break;
            }

            rooms.Add(new ProceduralRoom(pos, size));
        }

        return rooms;
    }


    /* ============================
     * Separate Rooms
     * ============================ */

    public void SeparateRooms(List<ProceduralRoom> rooms)
    {
        const int maxIter = 150;
        int iteration = 0;
        int size = rooms.Count;

        while (iteration < maxIter)
        {
            bool hadOverlap = false;

            for (int i = 0; i < size - 1; i++)
            {
                var room = rooms[i];
                for (int j = i + 1; j < size; j++)
                {
                    var other = rooms[j];

                    var overlap = room.Rect.Intersection(other.Rect);
                    if (!overlap.HasArea()) continue;

                    hadOverlap = true;

                    Vector2 dir = (room.Rect.GetCenter() - other.Rect.GetCenter()).Normalized();
                    float length = overlap.Size.Length();

                    room.Rect.Position += dir * (length / 2);
                    other.Rect.Position -= dir * (length / 2);
                }
            }

            if (!hadOverlap)
                break;

            iteration++;
        }
    }


    /* ============================
     * Pack Rooms Toward Center
     * ============================ */

    public void PackRooms(List<ProceduralRoom> rooms, int minimumSpace = 0)
    {
        // Custom sorting
        rooms.Sort((a, b) =>
        {
            float da = a.Rect.GetCenter().Length();
            float db = b.Rect.GetCenter().Length();
            return da.CompareTo(db);
        });

        foreach (var room in rooms)
        {
            // PACK Y
            float dir = Mathf.Sign(room.Rect.GetCenter().Y) * 2;
            while (Mathf.Abs(room.Rect.GetCenter().Y) > 2)
            {
                room.Rect.Position += new Vector2(0, -dir);

                bool hit = false;
                foreach (var other in rooms)
                {
                    if (other == room) continue;
                    if (!room.Rect.Intersects(other.Rect)) continue;

                    room.Rect.Position += new Vector2(0, dir + dir * minimumSpace);
                    hit = true;
                }
                if (hit) break;
            }

            // PACK X
            dir = Mathf.Sign(room.Rect.GetCenter().X) * 2;
            while (Mathf.Abs(room.Rect.GetCenter().X) > 2)
            {
                room.Rect.Position += new Vector2(-dir, 0);

                bool hit = false;
                foreach (var other in rooms)
                {
                    if (other == room) continue;
                    if (!room.Rect.Intersects(other.Rect)) continue;

                    room.Rect.Position += new Vector2(dir + dir * minimumSpace, 0);
                    hit = true;
                }
                if (hit) break;
            }
        }
    }

    
    public override async GDTask Perform()
    {
        var rooms = GenerateRooms(Settings.StartCellCount);

        foreach (var room in rooms)
            room.Rect = room.Rect.Grow(5); // Shrink rooms to ensure corridors have at least one tile of length
        
        SeparateRooms(rooms);
        
        PackRooms(rooms, 0);

        foreach (var room in rooms)
            room.Rect = room.Rect.Grow(-5); // Shrink rooms to ensure corridors have at least one tile of length

            
        State.GeneratedRooms = rooms;
        
    }
}