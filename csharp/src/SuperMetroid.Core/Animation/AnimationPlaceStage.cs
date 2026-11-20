namespace SuperMetroid.Core.Animation;

/// <summary>Where in its phase an animation stands (<see cref="AnimationPlace"/>).</summary>
internal enum AnimationPlaceStage
{
    /// <summary>The play plays the phase; a place no one has set stands here, at the start of its phase.</summary>
    Playing,

    /// <summary>The play has not entered the phase yet.</summary>
    BeforePhase,

    /// <summary>The animation holds in the phase: its play is done for good and it rests on its last frame.</summary>
    Holding,
}
