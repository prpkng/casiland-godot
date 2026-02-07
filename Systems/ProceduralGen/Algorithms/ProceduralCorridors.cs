using Godot;

namespace Casiland.Systems.ProceduralGen.Algorithms;

public abstract class CorridorShape
{
    public Vector2I From;
    public Vector2I To;
    
    public abstract LineSegment[] ComputeLines();
    public abstract RoomNeighborDirection FromDirection { get; }
    public abstract RoomNeighborDirection ToDirection { get; }
}

public class DirectCorridorShape : CorridorShape
{
    public Vector2 Axis; 
    public ProceduralRoom FromRoom;
    public ProceduralRoom ToRoom;
    
    public DirectCorridorShape(Vector2 axis, 
        ProceduralRoom fromRoom, ProceduralRoom toRoom)
    {
        Axis = axis;
        FromRoom = fromRoom;
        ToRoom = toRoom;
    }

    public override LineSegment[] ComputeLines()
    {
        var inv = Vector2.One - Axis;
        var overlap = ProceduralGeometry.GetRectOverlapOnAxis(FromRoom.Rect, ToRoom.Rect, Axis);
        From = (FromRoom.Center * Axis + overlap.GetCenter() * inv).RoundToInt();
        To = (ToRoom.Center * Axis + overlap.GetCenter() * inv).RoundToInt();

        return [new LineSegment(From, To)];
    }

    public override RoomNeighborDirection FromDirection => 
        DirectionConversions.VecToDirDict[FromRoom.Center.DirectionTo(ToRoom.Center).Sign4Way()];
    public override RoomNeighborDirection ToDirection =>
        DirectionConversions.VecToDirDict[ToRoom.Center.DirectionTo(FromRoom.Center).Sign4Way()];
}

public class CornerCorridorShape : CorridorShape
{
    public Vector2 CornerDirection;
    
    public CornerCorridorShape(Vector2I from, Vector2I to, Vector2 cornerDirection)
    {
        From = from;
        To = to;
        CornerDirection = cornerDirection;
    }
    
    public override LineSegment[] ComputeLines()
    {
        var vector = To - From;
        var corner = From + vector * CornerDirection.Abs();
        return [new LineSegment(From, corner), new LineSegment(corner, To)];
    }
    public override RoomNeighborDirection FromDirection => 
        DirectionConversions.VecToDirDict[CornerDirection.Sign4Way()];

    public override RoomNeighborDirection ToDirection
    {
        get
        {
            var vector = To - From;
            var corner = From + vector * CornerDirection.Abs();
            return DirectionConversions.VecToDirDict[(corner - To).Sign4Way()];
        }
    }
}

public class StepCorridorShape : CorridorShape
{
    public Vector2 StepAxis; // Either Vector2.Right or Vector2.Down
    public float Bias;

    public StepCorridorShape(Vector2I from, Vector2I to, Vector2 stepAxis, float bias)
    {
        From = from;
        To = to;
        StepAxis = stepAxis;
        Bias = bias;
    }

    public override LineSegment[] ComputeLines()
    {
        var dir = To - From;

        var corner1 = From + dir * StepAxis * Bias;
        var corner2 = To - dir * StepAxis * (1f - Bias);

        return [new LineSegment(From, corner1), new LineSegment(corner1, corner2), new LineSegment(corner2, To)];

    }

    public override RoomNeighborDirection FromDirection
    {
        get
        {
            var dir = To - From;

            var corner1 = From + dir * StepAxis * Bias;
            return DirectionConversions.VecToDirDict[(corner1 - From).Sign4Way()];
        }
    }

    public override RoomNeighborDirection ToDirection
    {
        get
        {
            var dir = To - From;
            
            var corner2 = To - dir * StepAxis * (1f - Bias);
            return DirectionConversions.VecToDirDict[(corner2 - To).Sign4Way()];
        }
    }
}