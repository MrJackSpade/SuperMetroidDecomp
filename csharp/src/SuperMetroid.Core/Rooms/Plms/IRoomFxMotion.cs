namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The room liquid's motion words room PLMs write and read (#1395). The room FX state implements it.</summary>
public interface IRoomFxMotion
{
    /// <summary>The liquid's base surface.</summary>
    FixedCoordinate BaseY { get; }

    /// <summary>The surface the liquid moves toward.</summary>
    Position16<RoomPixels> TargetSurface { get; }

    /// <summary>Sets the liquid's base surface, target surface, vertical speed and motion delay; each null value is left as it is.</summary>
    void SetLiquidMotion(
        int? baseYPosition = null,
        int? targetYPosition = null,
        int? packedYVelocity = null,
        int? timer = null);
}
