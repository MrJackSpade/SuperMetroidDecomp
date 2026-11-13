namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room liquid's motion values room objects write and read (#1395). The room FX state implements it.</summary>
public interface IRoomFxMotion
{
    /// <summary>The liquid's base surface, or null when it has none (#1546).</summary>
    FixedCoordinate? BaseY { get; }

    /// <summary>The line the liquid moves toward, or null when it has none and never moves (#1546).</summary>
    int? TargetSurface { get; }

    /// <summary>Removes the liquid's target, so the liquid stops.</summary>
    void RemoveLiquidTarget();

    /// <summary>Sets the liquid's base surface, target surface, vertical speed and motion delay; each null value is left as it is.</summary>
    void SetLiquidMotion(
        int? baseYPosition = null,
        int? targetYPosition = null,
        SignedEightEight? yVelocity = null,
        int? timer = null);
}
