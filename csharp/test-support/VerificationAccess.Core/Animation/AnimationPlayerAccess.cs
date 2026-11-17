namespace SuperMetroid.Core.Animation;

/// <summary>Verification access to <see cref="AnimationPlayer{TSegment, TFrame, TEvent}"/> state only its own updates write.</summary>
internal static class AnimationPlayerAccess
{
    /// <summary>Restores a player's place from a native snapshot's animation counters (#2359).</summary>
    extension<TSegment, TFrame, TEvent>(AnimationPlayer<TSegment, TFrame, TEvent> self)
        where TSegment : struct
        where TFrame : struct
        where TEvent : notnull
    {
        /// <summary>
        /// Puts the player on beat <paramref name="beat"/> of its animation's first phase, the frame that beat shows, with
        /// <paramref name="updatesLeft"/> updates of it left: where a native list stands once it has shown that beat's frame
        /// and counts its hold down.
        /// </summary>
        /// <exception cref="ArgumentException">The beat shows no frame.</exception>
        internal void ResumeOn(int beat, int updatesLeft)
        {
            AnimationSegments<TSegment, TFrame, TEvent> segments =
                PrivateState.Field<AnimationPlayer<TSegment, TFrame, TEvent>, AnimationSegments<TSegment, TFrame, TEvent>>(self, "segments");
            AnimationPhase<TSegment> phase = self.Playback.Animation.Phases[0];
            TFrame shown = segments.Beats(phase.Segment)[phase.FirstBeat + beat].ShownFrame
                ?? throw new ArgumentException($"Beat {beat} of {phase.Segment} shows no frame.", nameof(beat));
            PrivateState.Set(self, nameof(self.Playback), self.Playback with { Place = new AnimationPlace(0, beat + 1) });
            PrivateState.Set(self, nameof(self.Shown), (TFrame?)shown);
            PrivateState.Set(self, nameof(self.UpdatesLeft), updatesLeft);
        }
    }
}
