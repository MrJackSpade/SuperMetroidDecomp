namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>A sound-effect opcode, its argument count and whether it ends the program.</summary>
/// <param name="Opcode">The opcode.</param>
/// <param name="ArgumentCount">The argument count.</param>
/// <param name="Terminal">The terminal.</param>
internal readonly record struct SoundEffectOpcodeShape(int Opcode, int ArgumentCount, bool Terminal);
