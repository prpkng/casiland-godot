using Godot;

namespace Casiland.Systems.ProceduralGen.Algorithms;

public abstract class CorridorShape
{
    public ProceduralRoom FromRoom;
    public ProceduralRoom ToRoom;
    public Vector2 FromPos;
    public Vector2 ToPos;
    
    public abstract LineSegment[] ComputeLines();
    public abstract RoomNeighborDirection FromDirection { get; }
    public abstract RoomNeighborDirection ToDirection { get; }
}

public class DirectCorridorShape : CorridorShape
{
    public Vector2 Axis; 
    
    public DirectCorridorShape(Vector2 axis, 
        ProceduralRoom fromRoom, ProceduralRoom toRoom)
    {
        Axis = axis;
        FromRoom = fromRoom;
        ToRoom = toRoom;
        Recalculate();
    }

    private void Recalculate()
    {
        var inv = Vector2.One - Axis;
        var overlap = ProceduralGeometry.GetRectOverlapOnAxis(FromRoom.Rect, ToRoom.Rect, Axis);
        var baseDir = (ToRoom.Center - FromRoom.Center).Sign4Way();
        FromPos = (FromRoom.Rect.CastTowardsPerimeter(baseDir) * Axis + overlap.GetCenter() * inv).RoundToInt();
        ToPos = (ToRoom.Rect.CastTowardsPerimeter(-baseDir) * Axis + overlap.GetCenter() * inv).RoundToInt();
    }

    public override LineSegment[] ComputeLines() => [new(FromPos, ToPos)];

    public override RoomNeighborDirection FromDirection => 
        DirectionConversions.VecToDirDict[FromPos.DirectionTo(ToPos).Sign4Way()];
    public override RoomNeighborDirection ToDirection =>
        DirectionConversions.VecToDirDict[ToPos.DirectionTo(FromPos).Sign4Way()];
}

public class CornerCorridorShape : CorridorShape
{
    private Vector2 _cornerDirection;

    public Vector2 CornerDirection
    {
        get => _cornerDirection;
        set
        {
            _cornerDirection = value;
            RecomputeOrigins();
        }
    }
    
    public CornerCorridorShape(ProceduralRoom from, ProceduralRoom to, Vector2 cornerDirection)
    {
        FromRoom = from;
        ToRoom = to;
        FromPos = from.Center;
        ToPos = to.Center;
        CornerDirection = cornerDirection;
        RecomputeOrigins();
    }

    private void RecomputeOrigins()
    {
        FromPos = FromRoom.Center;
        ToPos = ToRoom.Center;
        var vector = ToPos - FromRoom.Center;
        var corner = FromPos + vector * CornerDirection.Abs();
        
        FromPos = FromRoom.Rect.CastTowardsPerimeter(FromPos.DirectionTo(corner));
        ToPos = ToRoom.Rect.CastTowardsPerimeter(ToPos.DirectionTo(corner));
    }
    
    public override LineSegment[] ComputeLines()
    {
        var vector = ToPos - FromPos;
        var corner = FromPos + vector * CornerDirection.Abs();
        return [new LineSegment(FromPos, corner), new LineSegment(corner, ToPos)];
    }
    public override RoomNeighborDirection FromDirection => 
        DirectionConversions.VecToDirDict[CornerDirection.Sign4Way()];

    public override RoomNeighborDirection ToDirection
    {
        get
        {
            var vector = ToPos - FromPos;
            var corner = FromPos + vector * CornerDirection.Abs();
            return DirectionConversions.VecToDirDict[(corner - ToPos).Sign4Way()];
        }
    }
}

public class StepCorridorShape : CorridorShape
{
    public Vector2 StepAxis; // Either Vector2.Right or Vector2.Down
    public float Bias;

    public StepCorridorShape(ProceduralRoom from, ProceduralRoom to, Vector2 stepAxis, float bias)
    {
        FromRoom = from;
        ToRoom = to;
        FromPos = from.Center;
        ToPos = to.Center;
        StepAxis = stepAxis;
        Bias = bias;
        RecomputeOrigins();
    }

    private void RecomputeOrigins()
    {
        FromPos = FromRoom.Center;
        ToPos = ToRoom.Center;
        var dir = ToPos - FromPos;

        var corner1 = FromPos + dir * StepAxis * Bias;
        var corner2 = ToPos - dir * StepAxis * (1f - Bias);
        
        FromPos = FromRoom.Rect.CastTowardsPerimeter((corner1 - FromPos).Sign4Way());
        ToPos = ToRoom.Rect.CastTowardsPerimeter((corner2 - ToPos).Sign4Way());
    }

    public override LineSegment[] ComputeLines()
    {
        var dir = ToPos - FromPos;

        var corner1 = FromPos + dir * StepAxis * Bias;
        var corner2 = ToPos - dir * StepAxis * (1f - Bias);

        return [new LineSegment(FromPos, corner1), new LineSegment(corner1, corner2), new LineSegment(corner2, ToPos)];

    }

    public override RoomNeighborDirection FromDirection
    {
        get
        {
            var dir = ToPos - FromPos;

            var corner1 = FromPos + dir * StepAxis * Bias;
            return DirectionConversions.VecToDirDict[(corner1 - FromPos).Sign4Way()];
        }
    }

    public override RoomNeighborDirection ToDirection
    {
        get
        {
            var dir = ToPos - FromPos;
            
            var corner2 = ToPos - dir * StepAxis * (1f - Bias);
            return DirectionConversions.VecToDirDict[(corner2 - ToPos).Sign4Way()];
        }
    }
}