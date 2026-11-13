namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Dachora's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The segment a branch continues at.</param>
internal readonly record struct EscapeDachoraCue(EscapeDachoraCueKind Kind, EscapeDachoraSegment Target = default);
