namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One cue of the Magdollite's lava programs, with the operands its native command carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct MagdolliteLavaCue(MagdolliteLavaCueKind Kind);
