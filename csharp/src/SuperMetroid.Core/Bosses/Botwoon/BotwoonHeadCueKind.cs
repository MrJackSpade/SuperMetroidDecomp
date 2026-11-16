namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>What a signalling cue of Botwoon's head animations does: queue the spit sound or mark the spit frame.</summary>
internal enum BotwoonHeadCueKind
{
    /// <summary>Queues the spit sound.</summary>
    QueueSpitSound,
    /// <summary>Marks the frame the spit volley leaves the mouth.</summary>
    ReachSpitFrame,
}
