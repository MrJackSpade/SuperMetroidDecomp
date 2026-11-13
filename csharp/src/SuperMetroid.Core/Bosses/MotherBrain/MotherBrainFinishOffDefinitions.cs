namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// How Mother Brain judges Samus's energy when deciding to finish her off in the second phase: she stands up to fire the
/// final beam once Samus's energy is within a few onion ring hits, and keeps to onion rings rather than her bomb once it
/// is within one bomb hit, each threshold padded by the same margin.
/// </summary>
internal static class MotherBrainFinishOffDefinitions
{
    /// <summary>
    /// The onion ring hits, as Samus's suit divides them, that Samus's energy, less <see cref="EnergyMargin"/>, must be
    /// within for Mother Brain to stand up and finish her off.
    /// </summary>
    internal const int StandUpOnionRingHits = 4;

    /// <summary>The energy each finish-off threshold allows above the hits it counts.</summary>
    internal const int EnergyMargin = 20;
}
