namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Gameplay cues of the Shitroid's programs.</summary>
internal enum ShitroidCue
{
    /// <summary>
    /// The remorse step samples the random word: with bit 15 clear the remorse pulse starts over; with it set the
    /// Shitroid cries and runs its quickening and slowing pulse.
    /// </summary>
    MaybeRemorseCry,
}
