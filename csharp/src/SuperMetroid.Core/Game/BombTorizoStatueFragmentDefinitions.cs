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

    /// <summary>Returns the definition selected by an even native parameter.
    /// The initializer at $86:A764 reads program pointers at $86:A7AB, X offsets at
    /// $86:A7CB, and eight wrapping Y/velocity/acceleration rows at $86:A7EB/A7FB/A80B.</summary>
    internal static BombTorizoStatueFragmentDefinition ForParameter(ushort parameter)
    {
        if ((parameter & 1) != 0 || parameter >= BombTorizoStatueInstructionProgramDefinitions.ProgramCount * 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter),
                $"Bomb Torizo statue parameter ${parameter:X4} is outside " +
                "the sixteen-entry even parameter table.");
        }

        int index = parameter >> 1;
        // A three-by-three grid of sixteen-pixel cells, omitting the first corner.
        // The second eight-fragment set mirrors X about the middle column (x = 8).
        int cell = (index & 7) + 1;
        int x = 16 * (cell % 3) - 8;
        if (index >= 8) x = 16 - x;
        return new(
            BombTorizoStatueInstructionProgramDefinitions.Program(index),
            (short)x,
            (short)(16 * (cell / 3) - 8),
            YVelocity: 0x0100,
            Acceleration: 0x0010);
    }
}
