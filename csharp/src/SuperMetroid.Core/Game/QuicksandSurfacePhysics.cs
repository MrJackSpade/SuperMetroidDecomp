namespace SuperMetroid.Core.Game;

/// <summary>One suit-dependent row of the fixed quicksand-surface physics table.</summary>
public readonly record struct QuicksandSurfacePhysics(
    ushort MovingDisplacement,
    ushort StationaryDisplacement,
    ushort UpwardSpeedLimit);
