namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Gameplay cues published by the head animation.</summary>
internal enum BotwoonHeadCue
{
    /// <summary>Sets the head's collision radius for its facing.</summary>
    ApplyFacingRadius,
    /// <summary>Queues the spit sound.</summary>
    QueueSpitSound,
    /// <summary>Marks the frame the spit volley leaves the mouth.</summary>
    ReachSpitFrame,
}
