using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Where her brain's animation stands after a play to its next picture, and the picture it then shows (#2554).</summary>
/// <typeparam name="TSegment">The catalog's segments.</typeparam>
/// <param name="Playback">Where the animation stands.</param>
/// <param name="Shown">The picture the brain shows.</param>
internal readonly record struct MotherBrainBrainPlayed<TSegment>(AnimationPlayback<TSegment> Playback, EnemyFrameId Shown)
    where TSegment : struct;
