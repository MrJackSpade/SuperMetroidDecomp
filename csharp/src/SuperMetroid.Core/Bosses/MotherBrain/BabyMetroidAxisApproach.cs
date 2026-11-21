namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One update of the Baby Metroid's approach along one axis (#2557): whether it has arrived, and its speed along the axis after it.</summary>
/// <param name="Progress">Whether the Baby is still approaching the target along the axis or has arrived.</param>
/// <param name="Speed">The Baby's speed along the axis after the update.</param>
internal readonly partial record struct BabyMetroidAxisApproach(BabyMetroidAxisProgress Progress, SignedEightEight Speed)
{
    /// <summary>Whether the Baby has arrived at the target along the axis.</summary>
    internal bool Arrived => Progress == BabyMetroidAxisProgress.Arrived;
}
