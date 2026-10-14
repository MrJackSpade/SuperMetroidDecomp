namespace SuperMetroid.Core.Game;

/// <summary>What one step of an <see cref="AnimationTimeline{TFrame, TCue}"/> does.</summary>
internal enum AnimationStepKind
{
    /// <summary>Shows a frame for a number of processing updates; the timeline yields until it expires.</summary>
    Show,
    /// <summary>Publishes a gameplay cue (sound, collision shape, attack timing) and continues at once.</summary>
    Cue,
    /// <summary>Keeps the last frame indefinitely: the timeline stays here and publishes nothing more.</summary>
    Hold,
    /// <summary>Continues from the first step: a looping animation.</summary>
    Loop,
}
