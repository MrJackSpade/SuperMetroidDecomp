namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One cue of Mini-Kraid's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct FakeKraidCue(FakeKraidCueKind Kind);
