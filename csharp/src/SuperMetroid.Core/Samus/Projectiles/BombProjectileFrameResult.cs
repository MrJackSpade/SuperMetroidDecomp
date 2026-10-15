namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One frame's debugger-visible bomb lifecycle transitions.</summary>
/// <param name="PublishedBombJumpDirection">Overlap direction written this pass: zero for no publication, one for left, two for straight up, or three for right; later overlapping slots overwrite earlier ones.</param>
/// <param name="BeamChargeConsumed">Whether spread admission or rejected placement consumed/cancelled beam charge; runtime clears ordinary charge state and restores Samus's suit colors when true.</param>
/// <param name="SoundRequests">Ordered sound requests from this bomb pass, including per-call queue limits and suppression witnesses; the usual result owns a newly created array, while the default result has null.</param>
public readonly record struct BombProjectileFrameResult(
    byte PublishedBombJumpDirection,
    bool BeamChargeConsumed = false,
    IReadOnlyList<SamusSoundRequest>? SoundRequests = null);
