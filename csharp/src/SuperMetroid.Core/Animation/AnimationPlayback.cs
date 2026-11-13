namespace SuperMetroid.Core.Animation;

/// <summary>An owner's playing animation (#1600): the animation, and where it stands in it.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <param name="Animation">The animation playing.</param>
/// <param name="Place">Where it stands.</param>
internal readonly record struct AnimationPlayback<TSegment>(Animation<TSegment> Animation, AnimationPlace Place)
    where TSegment : struct;
