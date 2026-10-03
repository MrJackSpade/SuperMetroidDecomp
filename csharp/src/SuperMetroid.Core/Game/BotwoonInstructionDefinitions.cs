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
    internal const ushort HiddenHeadInstruction =
        BotwoonInstructionProgramDefinitions.Hidden;

    /// <summary>
    /// Computes <c>InstListPointers_Botwoon</c> at $B3:946B-$947A from the clockwise
    /// octant and eight-byte movement-program layout.
    /// </summary>
    internal static ushort HeadMovementInstruction(byte angle) =>
        HeadForOctant(angle >> 5).MovementInstruction;

    /// <summary>
    /// Returns the nearest-octant spit instruction selected after the native 16-step bias.
    /// Replaces <c>InstListPointers_Botwoon_spit</c> at $B3:948B-$949A.
    /// </summary>
    internal static ushort HeadSpitInstruction(byte angle) =>
        HeadForOctant(unchecked((byte)(angle + 16)) >> 5).SpitInstruction;

    /// <summary>Returns one visible head/spit pair by its zero-based angular octant.</summary>
    internal static BotwoonHeadInstructionDefinition HeadForOctant(int octant)
    {
        if ((uint)octant >= 8)
        {
            throw new ArgumentOutOfRangeException(
                nameof(octant), octant, "Botwoon head octant must be zero through seven.");
        }

        int direction = PhysicalDirection(octant);
        return new(
            (ushort)(BotwoonInstructionProgramDefinitions.MovingUpLeft + 8 * direction),
            (ushort)(BotwoonInstructionProgramDefinitions.SpittingUpLeft + 16 * direction));
    }

    /// <summary>
    /// Computes <c>BotwoonsBodyTail_InstListPointers</c> at $86:E9F1-$EA30.
    /// Four eight-entry regions select visible body, hidden body, visible tail and
    /// hidden tail; visible program records occupy twenty and six bytes respectively.
    /// </summary>
    internal static ushort BodyInstruction(ushort byteOffset)
    {
        if ((byteOffset & 1) != 0 || byteOffset >= 64)
        {
            throw new ArgumentOutOfRangeException(
                nameof(byteOffset), byteOffset,
                "Botwoon body instruction offset must be even and zero through 62.");
        }

        // Each sixteen-entry region has eight visible and eight hidden directions.
        int selector = byteOffset >> 1;
        if ((selector & 8) != 0) return BotwoonProjectileInstructionProgramDefinitions.Hidden;
        int octant = selector & 7;
        if ((selector & 16) != 0)
            return (ushort)(BotwoonProjectileInstructionProgramDefinitions.TailUpFacingRight + 6 * ((8 - octant) & 7));
        return (ushort)(BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft + 20 * PhysicalDirection(octant));
    }
    // Head and body programs run counterclockwise from up-left. Their physical
    // layout includes an unused down-facing-left slot between down-left and down.
    private static int PhysicalDirection(int octant) => 8 - octant - (octant >= 5 ? 1 : 0);
}
