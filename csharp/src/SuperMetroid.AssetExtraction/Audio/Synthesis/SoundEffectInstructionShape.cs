namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>How a sound-effect instruction is encoded: its argument count and whether it ends the program.</summary>
/// <param name="ArgumentCount">The argument count.</param>
/// <param name="Terminal">Whether the instruction ends the program.</param>
internal abstract partial record SoundEffectInstructionShape(int ArgumentCount, bool Terminal);
