using SuperMetroid.Core.Compatibility;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// Requests a check makes through a runtime's own sound requests ahead of the request it checks (#2602): a sound's
/// priority is how full its library's queue may already be for the queue to admit it, so the check fills the queue with
/// requests made the way the game makes them and reads which of the competing requests the queue lets in.
/// </summary>
internal static class CompetingSoundRequests
{
    /// <summary>Fills a runtime's sound queues ahead of a checked request.</summary>
    extension(GameplaySoundRequests self)
    {
        /// <summary>
        /// Requests <paramref name="filler"/> at essential priority, which a queue below its most always admits, until the
        /// filler's library's queue holds <paramref name="waiting"/> waiting requests, as requests made earlier in the
        /// update would.
        /// </summary>
        /// <param name="filler">A sound of the checked request's library that the check does not look for.</param>
        /// <param name="waiting">How many requests the queue is to hold.</param>
        /// <exception cref="InvalidOperationException">
        /// The queue already holds more than <paramref name="waiting"/> requests, or a filler request did not join it.
        /// </exception>
        internal void FillQueueAhead(SoundEffectId filler, int waiting)
        {
            AudioQueues queues = self.QueuesReached;
            int held = queues.WaitingSounds(filler.Library).Count;
            if (held > waiting)
                throw new InvalidOperationException($"{filler.Library}'s queue already holds {held} requests, more than the {waiting} the check needs.");
            while (held < waiting)
            {
                SoundQueueAdmission admission = self.QueueSound(filler, SoundRequestPriority.Essential);
                int now = queues.WaitingSounds(filler.Library).Count;
                if (!admission.IsAdmitted || now != held + 1)
                    throw new InvalidOperationException($"The filler {filler} did not join {filler.Library}'s queue holding {held} requests.");
                held = now;
            }
        }
    }
}
