namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// What one music channel command is (#2548), decoded once from its opcode: the end of the program, a pitched note, a tie,
/// a rest, a percussion note, or an effect, which its opcode names (<see cref="SpcMusicEffect"/>). The installed manifest
/// spells a command by its kind's key in <see cref="AudioMusicInstructionKindKeys"/>, an effect by the effect's own key.
/// </summary>
public enum AudioMusicInstructionKind
{
    /// <summary>The zero opcode ending a channel program, with no operands and no command after it.</summary>
    End,

    /// <summary>A pitched note, opcodes <c>$80-$C7</c>, timed by its prefix and with no argument bytes.</summary>
    Note,

    /// <summary>Opcode <c>$C8</c>, continuing the previous note without a fresh key-on; timed by its prefix, with no argument bytes.</summary>
    Tie,

    /// <summary>Opcode <c>$C9</c>, a rest rather than a pitched or percussion note; timed by its prefix, with no argument bytes.</summary>
    Rest,

    /// <summary>A percussion note, opcodes <c>$CA-$DF</c>, selecting an instrument relative to the driver's percussion base; timed by its prefix, with no argument bytes.</summary>
    Percussion,

    /// <summary>An effect command, which its opcode names, with that effect's argument bytes.</summary>
    Effect,
}
