namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One frame's bomb lifecycle transitions; the pass's sounds are already queued in its audio requests.</summary>
/// <param name="PublishedBombJumpDirection">Launch direction an overlapping bomb published this pass, or none; later overlapping slots overwrite earlier ones.</param>
/// <param name="BeamChargeConsumed">Whether spread admission or rejected placement consumed/cancelled beam charge; runtime clears ordinary charge state and restores Samus's suit colors when true.</param>
public readonly record struct BombProjectileFrameResult(
    SamusBombJumpDirection PublishedBombJumpDirection,
    bool BeamChargeConsumed = false);
