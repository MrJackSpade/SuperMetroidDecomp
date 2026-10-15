namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One target in Crocomire's hidden-wall rumble schedule. Negative targets carry the
/// cooldown and approach delta loaded when their oscillation completes.
/// </summary>
internal readonly record struct CrocomireRumbleDefinition(
    short TargetYOffset,
    ushort NextTargetOffset,
    ushort Cooldown,
    ushort Delta,
    bool IsTerminator);
