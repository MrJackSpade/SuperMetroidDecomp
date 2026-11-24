namespace Engine.Core;

/// <summary>
/// How readily a sound that is asked for gives way when sounds are already waiting to be heard. Where nothing ever
/// waits it changes nothing; a game whose sounds wait their turn says how many may wait before each gives way.
/// </summary>
public enum SoundPriority
{
    /// <summary>Heard only when nothing is waiting.</summary>
    WhenIdle,

    /// <summary>Gives way soon.</summary>
    Low,

    /// <summary>The usual priority.</summary>
    Normal,

    /// <summary>Gives way late.</summary>
    High,

    /// <summary>Gives way only when no more can wait at all.</summary>
    Essential,
}
