namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Whether a cinematic actor plays on (#2560).</summary>
internal enum CinematicActorPlay
{
    /// <summary>The actor's animation plays on as its frames' holds run out.</summary>
    Playing,
    /// <summary>The actor rests for good on its frame until its owner starts another animation.</summary>
    Resting,
    /// <summary>The actor is deleted and shows nothing.</summary>
    Deleted,
}
