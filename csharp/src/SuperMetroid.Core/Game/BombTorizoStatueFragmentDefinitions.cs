namespace SuperMetroid.Core.Game;

/// <summary>One physical fragment emitted when Bomb Torizo's statue hand breaks.</summary>
internal readonly record struct BombTorizoStatueFragmentDefinition(
    ushort InstructionList,
    short XOffset,
    short YOffset,
    ushort YVelocity,
    ushort Acceleration);

/// <summary>Fixed projectile definitions for Bomb Torizo's breaking statue hand.</summary>
internal static class BombTorizoStatueFragmentDefinitions
{
    /// <summary>
    /// Room-graphics enemy-projectile definition <c>$86:A993</c> requested by PLM
    /// instruction <c>$84:D357</c> for every statue fragment.
    /// </summary>
    internal const ushort ProjectileDefinition = 0xa993;

    /// <summary>
    /// The sixteen instruction/X-offset selections at <c>$86:A7AB-$A7EA</c>, joined with
    /// the eight wrapping Y-offset, launch-speed, and acceleration rows at
    /// <c>$86:A7EB-$A81A</c>. Native parameters are even byte offsets <c>$00..$1E</c>.
    /// </summary>
    private static readonly BombTorizoStatueFragmentDefinition[] Definitions =
    [
        new(BombTorizoStatueInstructionProgramDefinitions.Program(0), 8, -8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(1), 24, -8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(2), -8, 8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(3), 8, 8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(4), 24, 8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(5), -8, 24, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(6), 8, 24, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(7), 24, 24, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(8), 8, -8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(9), -8, -8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(10), 24, 8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(11), 8, 8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(12), -8, 8, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(13), 24, 24, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(14), 8, 24, 0x0100, 0x0010),
        new(BombTorizoStatueInstructionProgramDefinitions.Program(15), -8, 24, 0x0100, 0x0010),
    ];

    /// <summary>Returns the definition selected by an even native parameter.</summary>
    internal static BombTorizoStatueFragmentDefinition ForParameter(ushort parameter)
    {
        if ((parameter & 1) != 0 || parameter >= Definitions.Length * 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter),
                $"Bomb Torizo statue parameter ${parameter:X4} is outside " +
                "the sixteen-entry even parameter table.");
        }

        return Definitions[parameter >> 1];
    }
}
