namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One cue of a catalog of Mother Brain's brain animations (#2554): a <see cref="MotherBrainHeadCue"/> every brain animation
/// raises to the same effect on her encounter, or one of the catalog's own <typeparamref name="TOwn"/> cues, which act on what
/// only that catalog's playback carries and continue the play in that catalog.
/// </summary>
/// <typeparam name="TOwn">The cues only the catalog's animations raise.</typeparam>
internal abstract record MotherBrainBrainCue<TOwn>
    where TOwn : struct, Enum
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainBrainCue()
    {
    }

    /// <summary>A cue every brain animation raises to the same effect on her encounter.</summary>
    /// <param name="Cue">What the cue does.</param>
    internal sealed record Shared(MotherBrainHeadCue Cue) : MotherBrainBrainCue<TOwn>;

    /// <summary>A cue only the catalog's animations raise.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Own(TOwn Kind) : MotherBrainBrainCue<TOwn>;
}
