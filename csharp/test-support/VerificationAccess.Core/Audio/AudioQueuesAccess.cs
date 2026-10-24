namespace SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="AudioQueues"/> queues, which production reads only to dispatch.</summary>
internal static class AudioQueuesAccess
{
    /// <summary>Exposes music timing and per-library SFX queue positions from cartridge audio state.</summary>
    extension(AudioQueues self)
    {
        /// <summary>Music queue read/write indices, current timer and slot delays ($063B/$0639/$063F/$0629).</summary>
        internal string MusicQueueForVerification() =>
            $"{self.MusicReadPosition:X}/{self.MusicWritePosition:X} " +
            $"t={self.MusicTimer.RawValue:X} d=" +
            string.Join(",", self.MusicDelays.Select(delay => $"{delay.Updates:X}"));

        /// <summary>One entry of an SFX library's queue ($0643+): the sound queued there, or none.</summary>
        internal SoundEffectId? SoundQueueEntryForVerification(int queue, int index) =>
            self.SoundQueues[queue, index];

        /// <summary>Queue start/next indices and dispatcher state of one SFX library ($0643+/$0646+/$0649+).</summary>
        internal SoundQueueState SoundQueueForVerification(int queue) =>
            new SoundQueueState(self.SoundReadPositions[queue], self.SoundWritePositions[queue],
             self.SoundStates[queue]);
    }
}
