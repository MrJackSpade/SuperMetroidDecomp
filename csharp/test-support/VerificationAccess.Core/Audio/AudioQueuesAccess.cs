namespace SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="AudioQueues"/> queues, which production reads only to dispatch.</summary>
internal static class AudioQueuesAccess
{
    /// <summary>Exposes music timing and each sound library's waiting requests and handshake.</summary>
    extension(AudioQueues self)
    {
        /// <summary>The requests waiting in <paramref name="library"/>'s queue, oldest first; the next its handler asks for is the first.</summary>
        internal IReadOnlyList<SoundEffectId> WaitingSounds(SoundEffectLibrary library) => self.SoundQueues[library].Waiting;

        /// <summary>
        /// The music handler's wait (the fetched request and the calls left before it is dispatched, or "fetch" when its
        /// next call fetches), the request the queue would hand it next, and whether the queue holds any request.
        /// </summary>
        internal string MusicQueueForVerification() =>
            (self.PendingMusic is PendingMusicDispatch pending
                ? $"wait={pending.Request}@{pending.CallsUntilDispatch:X}"
                : "wait=fetch") +
            (self.Music.NextToFetch is QueuedMusic next ? $" next={next.Request}@{next.Delay.Updates:X}" : " next=none") +
            $" queued={self.HasQueuedMusic}";

        /// <summary>One sound library's waiting requests, oldest first, and its handler's handshake phase.</summary>
        /// <param name="library">The library read.</param>
        internal SoundQueueState SoundQueueForVerification(SoundEffectLibrary library) =>
            new([.. self.WaitingSounds(library)], HandshakeOf(self, library).Phase);

        /// <summary>
        /// Marks <paramref name="library"/>'s requests now, so that <c>SoundsQueuedSinceForVerification</c> can
        /// later name the requests queued since.
        /// </summary>
        /// <param name="library">The library marked.</param>
        internal SoundRequestsMark MarkSoundRequestsForVerification(SoundEffectLibrary library) =>
            new(library, self.SoundHandshakes.Awaited(library), HandshakeOf(self, library).Phase, [.. self.WaitingSounds(library)]);

        /// <summary>
        /// The requests queued for <paramref name="mark"/>'s library since the mark, oldest first, across at most one
        /// update's sound-handler calls (two at most, in a message box's update).
        /// </summary>
        /// <remarks>
        /// The handler takes requests from the front of the queue and new requests join its back, so every request not
        /// yet withdrawn (the one the handler holds, then those waiting) keeps its order, and the ones queued since the
        /// mark follow the ones the mark saw. Within one update the handler withdraws at most the request it held at the
        /// mark: it does when it then holds none (its hold ran out or a music change's downtime dropped it) or when it
        /// was holding an accepted request and has moved past holding.
        /// </remarks>
        /// <param name="mark">A mark of the library taken earlier in the same update or the one before.</param>
        /// <exception cref="InvalidOperationException">
        /// The requests now held and waiting do not continue the marked ones, so more than one update passed since the mark.
        /// </exception>
        internal IReadOnlyList<SoundEffectId> SoundsQueuedSinceForVerification(SoundRequestsMark mark)
        {
            SoundRequestsMark now = self.MarkSoundRequestsForVerification(mark.Library);
            bool markedRequestWithdrawn = mark.Held is not null &&
                (now.Held is null ||
                 (mark.Phase == SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase.HoldingBeforeWithdrawal &&
                  now.Phase != SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Phase.HoldingBeforeWithdrawal));
            var before = mark.HeldThenWaiting;
            var after = now.HeldThenWaiting;
            int kept = before.Length - (markedRequestWithdrawn ? 1 : 0);
            if (after.Length < kept || !after.Take(kept).SequenceEqual(before.Skip(before.Length - kept)))
            {
                throw new InvalidOperationException(
                    $"{mark.Library}'s requests [{string.Join(",", after)}] do not continue the marked [{string.Join(",", before)}].");
            }
            return [.. after.Skip(kept)];
        }

    }

    /// <summary>The handshake <paramref name="library"/>'s handler runs in <paramref name="queues"/>.</summary>
    private static SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>.Handshake HandshakeOf(AudioQueues queues, SoundEffectLibrary library) =>
        queues.SoundHandshakes.Handshakes[SoundEffectLibraries.ToQueueIndex(library)];

    /// <summary>Creates queues for a check that runs them without a game session.</summary>
    extension(AudioQueues)
    {
        /// <summary>
        /// Queues over their own sound request handshakes, created for every library as the session's composition root
        /// creates the session's; for a check whose queues stand alone, with no demo playing and no power bomb exploding.
        /// </summary>
        internal static AudioQueues Standalone() => Standalone(new CheckGameplaySoundState());

        /// <summary>
        /// Standalone queues (see <see cref="Standalone()"/>) that test every sound request against <paramref name="gameplay"/>,
        /// whose demo and power bomb a check sets.
        /// </summary>
        internal static AudioQueues Standalone(IGameplaySoundState gameplay) =>
            new(new SoundRequestHandshakes<SoundEffectLibrary, SoundEffectId>(SoundEffectLibraries.InQueueOrder), gameplay);
    }
}
