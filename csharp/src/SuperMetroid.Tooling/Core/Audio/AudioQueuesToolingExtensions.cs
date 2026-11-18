using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.Core.Audio;

/// <summary>Creates sound and music queues for hosts that run a runtime without a frontend session (#2057).</summary>
internal static class AudioQueuesToolingExtensions
{
    /// <summary>Creates queues that stand alone.</summary>
    extension(AudioQueues)
    {
        /// <summary>
        /// Queues over their own sound request handshakes, created for every library as the session's composition root
        /// creates the session's, for a runtime or system that runs outside a session: no door disables its sounds, no
        /// attract demo drives it and no power bomb's explosion turns its sounds away, as nothing plays what its queues
        /// release.
        /// </summary>
        internal static AudioQueues Standalone() =>
            new(new SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>(SoundEffectLibraries.InQueueOrder), new NoGameplaySoundState());
    }

    /// <summary>The gameplay states of queues that stand alone: no door disables sounds, no attract demo plays and no power bomb explodes.</summary>
    private sealed class NoGameplaySoundState : IGameplaySoundState
    {
        /// <inheritdoc/>
        public bool DoorTransitionDisablesSounds => false;

        /// <inheritdoc/>
        public bool AttractDemoPlaying => false;

        /// <inheritdoc/>
        public bool PowerBombExploding => false;
    }
}
