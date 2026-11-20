namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// A sound-effect note packet, which has no opcode: its first byte is the note's instrument, which no opcode may share,
/// then its other arguments. A note never ends the program.
/// </summary>
/// <param name="ArgumentCount">The argument count, the instrument included.</param>
internal sealed record SoundEffectNoteShape(int ArgumentCount) : SoundEffectInstructionShape(ArgumentCount, Terminal: false);
