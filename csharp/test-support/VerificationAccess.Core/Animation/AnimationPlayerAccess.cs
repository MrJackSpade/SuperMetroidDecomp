namespace SuperMetroid.Core.Animation;

/// <summary>Verification access to <see cref="AnimationPlayer{TSegment, TFrame, TEvent}"/> state only its own updates write.</summary>
internal static class AnimationPlayerAccess
{
    /// <summary>The frames a player's animation shows.</summary>
    extension<TSegment, TFrame, TEvent>(AnimationPlayer<TSegment, TFrame, TEvent> self)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>The frames one play of the player's animation shows, in the order it shows them, from its start.</summary>
        internal IReadOnlyList<TFrame> PlayedFrames =>
        [
            .. PrivateState.Field<AnimationPlayer<TSegment, TFrame, TEvent>, AnimationSegments<TSegment, TFrame, TEvent>>(self, "segments")
                .PlayedBeats(self.Playback.Animation).Select(beat => beat.ShownFrame).OfType<TFrame>(),
        ];
    }
}
