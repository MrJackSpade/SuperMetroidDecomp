namespace SuperMetroid.Core.Game;

/// <summary>Visible head poses selected from Botwoon's eight angular octants.</summary>
internal readonly record struct BotwoonHeadInstructionDefinition(
    ushort MovementInstruction,
    ushort SpitInstruction);

/// <summary>Compiled head, spit, body, and tail instruction selectors for Botwoon.</summary>
internal static class BotwoonInstructionDefinitions
{
    /// <summary>
    /// <c>InstList_Botwoon_Hide</c> at <c>$B3:9389</c>. The trailing eight entries of
    /// <c>InstListPointers_Botwoon</c> at <c>$B3:947B-$B3:948A</c> all select this list.
    /// </summary>
    internal const ushort HiddenHeadInstruction = 0x9389;

    /// <summary>
    /// The visible half of <c>InstListPointers_Botwoon</c> at <c>$B3:946B-$B3:947A</c>
    /// paired with <c>InstListPointers_Botwoon_spit</c> at <c>$B3:948B-$B3:949A</c>.
    /// Entries run clockwise from up-facing-right through up-left.
    /// </summary>
    private static readonly BotwoonHeadInstructionDefinition[] HeadInstructions =
    [
        new(0x9381, 0x941f),
        new(0x9379, 0x940f),
        new(0x9371, 0x93ff),
        new(0x9369, 0x93ef),
        new(0x9361, 0x93df),
        new(0x9351, 0x93bf),
        new(0x9349, 0x93af),
        new(0x9341, 0x939f),
    ];

    /// <summary>
    /// <c>BotwoonsBodyTail_InstListPointers</c> at <c>$86:E9F1-$86:EA30</c>.
    /// Its four eight-entry regions are visible body, hidden body, visible tail, and
    /// hidden tail. Callers retain the native even byte offset in projectile variable zero.
    /// </summary>
    private static readonly ushort[] BodyInstructions =
    [
        0xe8af, 0xe89b, 0xe887, 0xe873, 0xe85f, 0xe837, 0xe823, 0xe80f,
        0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3,
        0xe8c3, 0xe8ed, 0xe8e7, 0xe8e1, 0xe8db, 0xe8d5, 0xe8cf, 0xe8c9,
        0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3, 0xe8f3,
    ];

    /// <summary>Returns the visible head instruction selected by a cartridge angle.</summary>
    internal static ushort HeadMovementInstruction(byte angle) =>
        HeadInstructions[angle >> 5].MovementInstruction;

    /// <summary>
    /// Returns the nearest-octant spit instruction selected after the native 16-step bias.
    /// </summary>
    internal static ushort HeadSpitInstruction(byte angle) =>
        HeadInstructions[unchecked((byte)(angle + 16)) >> 5].SpitInstruction;

    /// <summary>Returns one visible head/spit pair by its zero-based angular octant.</summary>
    internal static BotwoonHeadInstructionDefinition HeadForOctant(int octant)
    {
        if ((uint)octant >= HeadInstructions.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(octant), octant, "Botwoon head octant must be zero through seven.");
        }

        return HeadInstructions[octant];
    }

    /// <summary>Returns the body/tail instruction at one native even byte offset.</summary>
    internal static ushort BodyInstruction(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset >= BodyInstructions.Length * 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset), byteOffset,
                "Botwoon body instruction offset must be even and zero through 62.");
        }

        return BodyInstructions[byteOffset >> 1];
    }
}
