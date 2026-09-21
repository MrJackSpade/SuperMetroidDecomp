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
    /// The visible half of <c>InstListPointers_Botwoon</c> at <c>$B3:946B-$B3:947A</c>
    /// paired with <c>InstListPointers_Botwoon_spit</c> at <c>$B3:948B-$B3:949A</c>.
    /// Entries run clockwise from up-facing-right through up-left.
    /// </summary>
    private static readonly BotwoonHeadInstructionDefinition[] HeadInstructions =
    [
        new(BotwoonInstructionProgramDefinitions.MovingUp,
            BotwoonInstructionProgramDefinitions.SpittingUp),
        new(BotwoonInstructionProgramDefinitions.MovingUpRight,
            BotwoonInstructionProgramDefinitions.SpittingUpRight),
        new(BotwoonInstructionProgramDefinitions.MovingRight,
            BotwoonInstructionProgramDefinitions.SpittingRight),
        new(BotwoonInstructionProgramDefinitions.MovingDownRight,
            BotwoonInstructionProgramDefinitions.SpittingDownRight),
        new(BotwoonInstructionProgramDefinitions.MovingDown,
            BotwoonInstructionProgramDefinitions.SpittingDown),
        new(BotwoonInstructionProgramDefinitions.MovingDownLeft,
            BotwoonInstructionProgramDefinitions.SpittingDownLeft),
        new(BotwoonInstructionProgramDefinitions.MovingLeft,
            BotwoonInstructionProgramDefinitions.SpittingLeft),
        new(BotwoonInstructionProgramDefinitions.MovingUpLeft,
            BotwoonInstructionProgramDefinitions.SpittingUpLeft),
    ];

    /// <summary>
    /// <c>BotwoonsBodyTail_InstListPointers</c> at <c>$86:E9F1-$86:EA30</c>.
    /// Its four eight-entry regions are visible body, hidden body, visible tail, and
    /// hidden tail. Callers retain the native even byte offset in projectile variable zero.
    /// </summary>
    private static readonly ushort[] BodyInstructions =
    [
        BotwoonProjectileInstructionProgramDefinitions.BodyUpFacingRight,
        BotwoonProjectileInstructionProgramDefinitions.BodyUpRight,
        BotwoonProjectileInstructionProgramDefinitions.BodyRight,
        BotwoonProjectileInstructionProgramDefinitions.BodyDownRight,
        BotwoonProjectileInstructionProgramDefinitions.BodyDownFacingRight,
        BotwoonProjectileInstructionProgramDefinitions.BodyDownLeft,
        BotwoonProjectileInstructionProgramDefinitions.BodyLeft,
        BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.TailUpFacingRight,
        BotwoonProjectileInstructionProgramDefinitions.TailUpRight,
        BotwoonProjectileInstructionProgramDefinitions.TailRight,
        BotwoonProjectileInstructionProgramDefinitions.TailDownRight,
        BotwoonProjectileInstructionProgramDefinitions.TailDown,
        BotwoonProjectileInstructionProgramDefinitions.TailDownLeft,
        BotwoonProjectileInstructionProgramDefinitions.TailLeft,
        BotwoonProjectileInstructionProgramDefinitions.TailUpLeft,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
        BotwoonProjectileInstructionProgramDefinitions.Hidden,
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
