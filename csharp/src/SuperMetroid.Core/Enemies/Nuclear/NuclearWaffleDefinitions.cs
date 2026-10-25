namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Compiled cartridge definitions for Puromi/Nuclear Waffle.</summary>
internal static class NuclearWaffleDefinitions
{

    /// <summary>Sound effect $5E in library two, queued when a joint makes its rising turn.</summary>
    internal static readonly SoundEffectId TurnSoundEffect = Library2Sound.SidehopperLanding;

    /// <summary>$A6:95F6 endpoint pairs mirror around angle $140.</summary>
    private const int SweepCenter = 0x140;
    /// <summary>$A6:95F6 endpoints extend $50 to either side of the common center.</summary>
    private const int SweepRadius = 0x50;
    /// <summary>$A6:9606 turn thresholds extend $40 to either side of the center.</summary>
    private const int TurnRadius = 0x40;
    /// <summary>$A6:95FE separates same-kind links by 24 angle units, with interleaved links halfway between.</summary>
    private const int SegmentPitch = 24;
    /// <summary>Derives one of the two mirrored sweeps, including endpoints, link spacing, and turn thresholds.</summary>
    internal static NuclearWaffleSweepDefinition Sweep(int direction)
    {
        if (direction >= 2)
            throw new InvalidDataException($"Nuclear Waffle direction {direction} is outside the two native sweeps.");
        int sign = 1 - direction * 2;
        return new(SweepCenter + sign * SweepRadius, SweepCenter - sign * SweepRadius,
            (short)(-sign * SegmentPitch), (short)(-sign * SegmentPitch / 2),
            SweepCenter + sign * TurnRadius, SweepCenter - sign * TurnRadius);
    }
}
