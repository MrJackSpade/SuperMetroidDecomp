namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>A sound-effect command: its opcode byte, then its arguments.</summary>
/// <param name="Opcode">The opcode.</param>
/// <param name="ArgumentCount">The argument count.</param>
/// <param name="Terminal">Whether the command ends the program.</param>
internal sealed record SoundEffectOpcodeShape(int Opcode, int ArgumentCount, bool Terminal)
    : SoundEffectInstructionShape(ArgumentCount, Terminal);
