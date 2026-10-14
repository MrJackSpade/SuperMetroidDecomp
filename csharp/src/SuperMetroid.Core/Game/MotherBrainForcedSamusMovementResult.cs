namespace SuperMetroid.Core.Game;

/// <summary>One-call witness for Mother Brain's forced Samus coordinate helpers.</summary>
public readonly record struct MotherBrainForcedSamusMovementResult(
    FixedPosition<RoomPixels> Before,
    bool NativeCarry);
