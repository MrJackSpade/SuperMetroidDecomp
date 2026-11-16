using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Dachora's animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Then">Where a branch continues the play when it is taken (#2061); none for a cue that runs on.</param>
internal readonly record struct EscapeDachoraCue(EscapeDachoraCueKind Kind, AnimationContinuation<EscapeDachoraSegment>? Then = null);
