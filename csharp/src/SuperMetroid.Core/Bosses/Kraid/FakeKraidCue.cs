namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One cue of Mini-Kraid's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct FakeKraidCue(FakeKraidCueKind Kind);
