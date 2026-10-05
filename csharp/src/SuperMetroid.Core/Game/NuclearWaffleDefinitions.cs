namespace SuperMetroid.Core.Game;

/// <summary>One Puromi/Nuclear Waffle sweep geometry definition.</summary>
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
    internal const ushort InitialInstructionList =
        NuclearWaffleInstructionProgramDefinitions.BodyLoop;

    /// <summary>Sound effect $5E in library two, queued when a joint turns.</summary>
    internal const ushort TurnSoundEffect = 0x005e;

    /// <summary>$A6:95F6 endpoint pairs mirror around angle $140.</summary>
    private const int SweepCenter = 0x140;
    /// <summary>$A6:95F6 endpoints extend $50 to either side of the common center.</summary>
    private const int SweepRadius = 0x50;
    /// <summary>$A6:9606 turn thresholds extend $40 to either side of the center.</summary>
    private const int TurnRadius = 0x40;
    /// <summary>$A6:95FE separates same-kind links by 24 angle units, with interleaved links halfway between.</summary>
    private const int SegmentPitch = 24;
    internal static NuclearWaffleSweepDefinition Sweep(byte direction)
    {
        if (direction >= 2)
            throw new InvalidDataException($"Nuclear Waffle direction {direction} is outside the two native sweeps.");
        int sign = 1 - direction * 2;
        return new((ushort)(SweepCenter + sign * SweepRadius), (ushort)(SweepCenter - sign * SweepRadius),
            (short)(-sign * SegmentPitch), (short)(-sign * SegmentPitch / 2),
            (ushort)(SweepCenter + sign * TurnRadius), (ushort)(SweepCenter - sign * TurnRadius));
    }
}
