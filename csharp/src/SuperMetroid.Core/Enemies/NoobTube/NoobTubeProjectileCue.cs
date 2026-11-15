using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>One cue of the n00b tube's crack, shards and bubbles animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct NoobTubeProjectileCue(NoobTubeProjectileCueKind Kind);
