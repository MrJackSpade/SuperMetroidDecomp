namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Dachora's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The program a branch continues at.</param>
internal readonly record struct EscapeDachoraCue(EscapeDachoraCueKind Kind, EscapeDachoraProgram Target = default);
