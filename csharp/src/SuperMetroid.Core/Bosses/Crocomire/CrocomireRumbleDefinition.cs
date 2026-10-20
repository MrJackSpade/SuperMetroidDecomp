namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One target in Crocomire's hidden-wall rumble schedule. Negative targets carry the
/// cooldown and approach delta loaded when their oscillation completes.
/// </summary>
[IndependentFlag("marks the rumble table's terminating record, a fact of the authored data")]
internal readonly record struct CrocomireRumbleDefinition(
    short TargetYOffset,
    ushort NextTargetOffset,
    ushort Cooldown,
    ushort Delta,
    bool IsTerminator);
