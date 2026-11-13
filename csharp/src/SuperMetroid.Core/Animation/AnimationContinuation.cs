namespace SuperMetroid.Core.Animation;

/// <summary>Where an owner's reaction to an event continues its play (#1600): another animation, or the same, at a place.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Animation">The animation the play continues in.</param>
/// <param name="Place">The place it continues at.</param>
internal readonly record struct AnimationContinuation<TSegment>(Animation<TSegment> Animation, AnimationPlace Place)
    where TSegment : struct;
