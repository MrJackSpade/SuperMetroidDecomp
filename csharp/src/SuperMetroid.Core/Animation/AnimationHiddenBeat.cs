namespace SuperMetroid.Core.Animation;

/// <summary>
/// Shows nothing for <paramref name="Duration"/> updates (#2558): the owner is hidden, holding no shown frame, until the
/// play shows its next frame. Only a family whose animations hide holds one (#2553): its segments are
/// <see cref="HidingAnimationSegments{TSegment, TFrame, TEvent}"/> or
/// <see cref="HidingUnpausedAnimationSegments{TSegment, TFrame, TEvent}"/>, so no other family's owner meets one.
/// </summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
/// <param name="Duration">The updates the owner stays hidden.</param>
internal sealed record AnimationHiddenBeat<TFrame, TEvent>(int Duration) : IHidingAnimationBeat<TFrame, TEvent>, IUnpausedHidingAnimationBeat<TFrame, TEvent>
    where TFrame : struct
    where TEvent : notnull
{
    /// <inheritdoc/>
    TFrame? IAnimationBeat<TFrame, TEvent>.ShownFrame => null;

    /// <inheritdoc/>
    TResult IHidingAnimationBeat<TFrame, TEvent>.Match<TPlay, TResult>(HidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
        Ensure.NotNull(cases).Hidden(this, ref play);

    /// <inheritdoc/>
    TResult IUnpausedHidingAnimationBeat<TFrame, TEvent>.Match<TPlay, TResult>(UnpausedHidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
        Ensure.NotNull(cases).Hidden(this, ref play);
}
