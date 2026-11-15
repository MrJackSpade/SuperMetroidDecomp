namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room liquid as room objects read and move it (#1395, #1987). The room FX state implements it.</summary>
public interface IRoomFxMotion
{
    /// <summary>The liquid's base surface, or null when it has none (#1546).</summary>
    FixedCoordinate? BaseY { get; }

    /// <summary>The line the liquid moves toward, or null when it has none and never moves (#1546).</summary>
    int? TargetSurface { get; }

    /// <summary>Moves the liquid's surface to room line <paramref name="line"/>, keeping its fraction.</summary>
    void SetLiquidLevel(int line);

    /// <summary>Changes the speed the liquid moves toward its target at, leaving its target and its wait.</summary>
    void SetLiquidSpeed(SignedEightEight speed);

    /// <summary>Sets the liquid moving toward its target at <paramref name="speed"/> once it has waited <paramref name="delay"/> updates.</summary>
    void MoveLiquidAfter(int delay, SignedEightEight speed);

    /// <summary>Cuts the liquid's wait to its next update.</summary>
    void EndLiquidWaitNextUpdate();

    /// <summary>Halts the liquid where it stands: it loses its target and its speed, and its wait is cleared.</summary>
    void HaltLiquid();
}
