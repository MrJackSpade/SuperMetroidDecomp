namespace SuperMetroid.Core.Audio;

/// <summary>Stable JSON operation names for decoded SPC music content.</summary>
public static class AudioMusicInstructionOperations
{
    /// <summary>Case-sensitive terminator shared by track and channel definitions: a zero track word or zero channel opcode, with no operands or following instructions.</summary>
    public const string End = "end";
    /// <summary>Channel operation for pitched-note opcodes <c>$80-$C7</c>; uses the instruction's timing prefix and has no argument bytes.</summary>
    public const string Note = "note";
    /// <summary>Channel opcode <c>$C8</c>, continuing the previous note without a fresh key-on; accepts an optional timing prefix and no argument bytes.</summary>
    public const string Tie = "tie";
    /// <summary>Channel opcode <c>$C9</c>, a rest rather than a pitched or percussion note; accepts an optional timing prefix and no argument bytes.</summary>
    public const string Rest = "rest";
    /// <summary>Channel operation for percussion opcodes <c>$CA-$DF</c>, selecting an instrument relative to the driver's percussion base; timing remains in the prefix and no argument bytes follow.</summary>
    public const string Percussion = "percussion";
    /// <summary>Track-level phrase routing: <c>Value</c> is an APU RAM pointer at least <c>$0100</c> to eight channel-program pointers, and <c>Target</c> must be null.</summary>
    public const string PlayPhrase = "playPhrase";
    /// <summary>Track control word <c>$0080</c> enables the driver's fast-forward mode; <c>Value</c> must be that word and <c>Target</c> must be null.</summary>
    public const string FastForwardOn = "fastForwardOn";
    /// <summary>Track control word <c>$0081</c> disables the driver's fast-forward mode; <c>Value</c> must be that word and <c>Target</c> must be null.</summary>
    public const string FastForwardOff = "fastForwardOff";
    /// <summary>Track repeat record with a nonzero low-byte count in <c>Value</c> (excluding <c>$80/$81</c>) and a required APU RAM track target in <c>Target</c>; distinct from a channel pattern-call effect.</summary>
    public const string Repeat = "repeat";

    /// <summary>Maps driver effect identities to stable camel-case manifest operations.</summary>
    private static readonly Dictionary<SpcMusicEffect, string> effectOperations =
        Enum.GetValues<SpcMusicEffect>().Distinct().ToDictionary(effect => effect, effect =>
        {
            string name = effect.ToString();
            return char.ToLowerInvariant(name[0]) + name[1..];
        });

    /// <summary>Uses stable camel-case operation names for SPC effects, including unknown numeric values.</summary>
    internal static string ForEffect(SpcMusicEffect effect)
    {
        if (effectOperations.TryGetValue(effect, out string? operation))
            return operation;
        string name = effect.ToString();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
