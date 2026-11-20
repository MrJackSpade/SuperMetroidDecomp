using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's live brain animation (#2317, #2554): where it stands and the picture it shows. An animation that opens
/// with a cue shows the picture after that cue from its start, before its first draw plays the cue.
/// </summary>
/// <param name="Animation">The playing animation and where it stands.</param>
/// <param name="Shown">The picture the brain shows.</param>
internal sealed record MotherBrainBrainPlayback(AnimationPlayback<MotherBrainHeadSegment> Animation, EnemyFrameId Shown);
