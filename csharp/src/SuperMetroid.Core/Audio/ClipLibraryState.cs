namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound library's clip playback (#1305): the sound it plays, from which frame, whether it
/// still holds the library (before its logical end), and the library's priority and low-health mode.
/// </summary>
internal sealed partial class ClipLibraryState
{
    /// <summary>The sound the library plays and whether it still holds the library, or null while it plays none (#2560).</summary>
    internal ClipLibraryPlayback? Playback { get; private set; }

    /// <summary>Whether the library holds a high-priority sound, which admits only requests exempt from priority; otherwise any request replaces the sound.</summary>
    internal bool HighPriority { get; set; }

    /// <summary>Whether the room library is in low-health mode, which rejects every request except those exempt from it.</summary>
    internal bool InLowHealthMode { get; set; }

    /// <summary>Whether the library's priority or low-health mode turns away a request under <paramref name="policy"/>.</summary>
    internal bool Rejects(SoundEffectClipPolicy policy) =>
        (!policy.IgnoresLowHealthMode && InLowHealthMode) || (!policy.IgnoresPriority && HighPriority);

    /// <summary>
    /// Starts <paramref name="sound"/> at <paramref name="now"/> in place of the playing sound, whose
    /// priority it drops while that sound still held the library, then applies the request's
    /// priority and low-health effects.
    /// </summary>
    internal void Play(SoundEffectClipEntry sound, long now, SoundEffectClipPolicy policy)
    {
        if (Playback is HoldingClipPlayback)
            HighPriority = false;
        Playback = new HoldingClipPlayback(sound, now);
        HighPriority = PrioritySetBy[policy.Priority] ?? HighPriority;
        InLowHealthMode = LowHealthModeSetBy[policy.LowHealthMode] ?? InLowHealthMode;
    }

    /// <summary>The priority each priority effect gives the library, or null for one that keeps it.</summary>
    private static IdentityTable<SoundEffectPriorityEffect, bool?> PrioritySetBy => field ??= new(new Dictionary<SoundEffectPriorityEffect, bool?>
    {
        [SoundEffectPriorityEffect.Low] = false,
        [SoundEffectPriorityEffect.High] = true,
        [SoundEffectPriorityEffect.Unchanged] = null,
    });

    /// <summary>The low-health mode each low-health effect gives the library, or null for one that keeps it.</summary>
    private static IdentityTable<LowHealthModeEffect, bool?> LowHealthModeSetBy => field ??= new(new Dictionary<LowHealthModeEffect, bool?>
    {
        [LowHealthModeEffect.Unchanged] = null,
        [LowHealthModeEffect.Clear] = false,
        [LowHealthModeEffect.Enter] = true,
    });

    /// <summary>Ends the sound at a request check after its logical end, releasing the library and its priority.</summary>
    internal void EndIfFinished(long now)
    {
        if (Playback is HoldingClipPlayback { Sound.Shape: OneShotSoundEffectClip { ActiveFrames: long active } } holding &&
            now - holding.Start > active)
        {
            Playback = new EndedClipPlayback(holding.Sound, holding.Start);
            HighPriority = false;
        }
    }
}
