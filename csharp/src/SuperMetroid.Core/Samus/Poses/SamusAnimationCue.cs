namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The cue that closes a run of delays in a Samus animation: where the animation goes next, or which
/// transitional pose it publishes. Each kind is its own record; the animation acts on the kind.
/// </summary>
internal abstract record SamusAnimationCue : SamusPoseAnimationStep
{
    /// <summary>
    /// Whether the animation can go on from the cue to the frame after it, the first frame of the next segment: a hold, the
    /// breathing's low-energy continuation and the drained fall's handover do. Every other cue moves the animation itself
    /// or ends it, and a frame after one of them is no step of the animation
    /// (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
    /// </summary>
    internal virtual bool ContinuesToNextSegment => false;
}
