namespace SuperMetroid.Core.Audio;

/// <summary>What a music request does when the queue has no free slot.</summary>
internal enum MusicQueueOverflow
{
    /// <summary>The request is dropped.</summary>
    SkipWhenFull,

    /// <summary>The request is written regardless, overtaking the read position.</summary>
    Overwrite,
}
