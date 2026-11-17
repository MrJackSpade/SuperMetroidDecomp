namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One-update witness for Mother Brain's forced Samus movement.</summary>
public readonly record struct MotherBrainForcedSamusMovementResult(
    FixedPosition<RoomPixels> Before,
    bool ReachedVerticalBoundary);
