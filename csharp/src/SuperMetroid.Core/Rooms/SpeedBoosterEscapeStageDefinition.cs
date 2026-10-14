namespace SuperMetroid.Core.Rooms;

/// <summary>One physical lava stage used by the Speed Booster escape controller.</summary>
internal readonly record struct SpeedBoosterEscapeStageDefinition(
    ushort TargetSamusX,
    ushort MaximumFxY,
    ushort PackedYVelocity);
