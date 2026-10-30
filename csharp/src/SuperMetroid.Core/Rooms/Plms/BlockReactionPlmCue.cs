namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One block-reaction PLM cue.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Sound">The library-2 sound a queue cue plays; none for the behaviour write.</param>
internal readonly record struct BlockReactionPlmCue(BlockReactionPlmCueKind Kind, SoundEffectId? Sound)
{
    /// <summary>The sound a queue cue plays.</summary>
    /// <exception cref="InvalidOperationException">The cue carries no sound.</exception>
    internal SoundEffectId QueuedSound => Sound ?? throw new InvalidOperationException($"Block-reaction cue {Kind} carries no sound.");
}
