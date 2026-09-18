namespace SuperMetroid.Core.Game;

/// <summary>One authored Puromi/Nuclear Waffle sweep geometry definition.</summary>
internal readonly record struct NuclearWaffleSweepDefinition(
    ushort StartAngle,
    ushort EndAngle,
    short SegmentSpacing,
    short InterleavedSegmentOffset,
    ushort SecondTurnThreshold,
    ushort FirstTurnThreshold);

/// <summary>Compiled cartridge definitions for Puromi/Nuclear Waffle.</summary>
internal static class NuclearWaffleDefinitions
{
    /// <summary>Enemy definition $E0BF (Puromi) in bank $A6.</summary>
    internal const ushort EnemyDefinition = 0xe0bf;

    /// <summary>$A6:9490, Puromi's initial animation instruction list.</summary>
    internal const ushort InitialInstructionList = 0x9490;

    /// <summary>Sound effect $5E in library two, queued when a joint turns.</summary>
    internal const ushort TurnSoundEffect = 0x005e;

    /// <summary>
    /// $A6:95F6/$A6:95FE/$A6:9606: paired sweep endpoints, articulated-link
    /// spacing, and orientation thresholds for the two population directions.
    /// </summary>
    private static readonly NuclearWaffleSweepDefinition[] Sweeps =
    [
        new(0x0190, 0x00f0, -24, -12, 0x0180, 0x0100),
        new(0x00f0, 0x0190,  24,  12, 0x0100, 0x0180),
    ];

    internal static NuclearWaffleSweepDefinition Sweep(byte direction)
    {
        if (direction >= Sweeps.Length)
        {
            throw new InvalidDataException(
                $"Nuclear Waffle direction {direction} is outside the two authored sweeps.");
        }

        return Sweeps[direction];
    }
}
