namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Compiled authored definitions for Puromi/Nuclear Waffle.</summary>
internal static class NuclearWaffleDefinitions
{

    /// <summary>The Sidehopper landing sound, queued when a joint makes its rising turn.</summary>
    internal static readonly SoundEffectId TurnSoundEffect = EnemySound.LowThud;

    /// <summary>The sweep endpoint pairs mirror around angle 320.</summary>
    private const int SweepCenter = 320;
    /// <summary>The sweep endpoints extend 80 to either side of the common center.</summary>
    private const int SweepRadius = 80;
    /// <summary>The turn thresholds extend 64 to either side of the center.</summary>
    private const int TurnRadius = 64;
    /// <summary>Same-kind links are separated by 24 angle units, with interleaved links halfway between.</summary>
    private const int SegmentPitch = 24;
    /// <summary>
    /// The backward sweep, with its endpoints, link spacing and turn thresholds: from the far side of the center back,
    /// its links trailing.
    /// </summary>
    internal static readonly NuclearWaffleSweepDefinition BackwardSweep =
        new(SweepCenter + SweepRadius, SweepCenter - SweepRadius, -SegmentPitch, -SegmentPitch / 2,
            SweepCenter + TurnRadius, SweepCenter - TurnRadius);

    /// <summary>The forward sweep: the mirror of <see cref="BackwardSweep"/>.</summary>
    internal static readonly NuclearWaffleSweepDefinition ForwardSweep =
        new(SweepCenter - SweepRadius, SweepCenter + SweepRadius, SegmentPitch, SegmentPitch / 2,
            SweepCenter - TurnRadius, SweepCenter + TurnRadius);
}
