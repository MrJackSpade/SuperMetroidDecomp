namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>A sound-effect opcode, its argument count and whether it ends the program.</summary>
/// <param name="Opcode">The opcode, or null for a note packet, whose first byte is the note's instrument rather than an opcode.</param>
/// <param name="ArgumentCount">The argument count.</param>
/// <param name="Terminal">The terminal.</param>
internal readonly record struct SoundEffectOpcodeShape(int? Opcode, int ArgumentCount, bool Terminal);
