namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomBlockAnimations"/> members production does not use.</summary>
internal static class RoomBlockAnimationsAccess
{
    /// <summary>Names the animations the room effect started.</summary>
    extension(RoomBlockAnimations self)
    {
        /// <summary>The animations the room effect the entry door selected started, in the effect's order.</summary>
        internal IReadOnlyList<BlockAnimation> EffectAnimations => [.. self.effectAnimations.Select(player => player.Playback.PlayingSegment)];
    }
}
