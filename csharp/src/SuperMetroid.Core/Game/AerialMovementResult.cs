namespace SuperMetroid.Core.Game;

/// <summary>Both collision scans and collision state produced by one aerial frame.</summary>
public readonly record struct AerialMovementResult(
    bool Landed,
    bool HitCeiling,
    bool WallJumpTriggered = false,
    bool WallContact = false,
    ushort WallDistance = 0);
