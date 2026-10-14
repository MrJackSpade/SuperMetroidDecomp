namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Etecoons' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The program a branch continues at.</param>
/// <param name="Offset">The cue's signed pixel offset.</param>
/// <param name="PreInstruction">The cue's PreInstruction operand.</param>
internal readonly record struct EscapeEtecoonCue(EscapeEtecoonCueKind Kind, EscapeEtecoonProgram Target = default, short Offset = 0, EscapeEtecoonPreInstruction PreInstruction = default);
