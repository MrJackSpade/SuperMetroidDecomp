namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One frame's debugger-visible bomb lifecycle transitions.</summary>
/// <param name="PublishedBombJumpDirection">Launch direction an overlapping bomb published this pass, or none; later overlapping slots overwrite earlier ones.</param>
/// <param name="BeamChargeConsumed">Whether spread admission or rejected placement consumed/cancelled beam charge; runtime clears ordinary charge state and restores Samus's suit colors when true.</param>
/// <param name="SoundRequests">Ordered sound requests from this bomb pass, including per-call queue limits and suppression witnesses; the usual result owns a newly created array, while the default result has null.</param>
public readonly record struct BombProjectileFrameResult(
    SamusBombJumpDirection PublishedBombJumpDirection,
    bool BeamChargeConsumed = false,
    IReadOnlyList<SoundRequest>? SoundRequests = null);
