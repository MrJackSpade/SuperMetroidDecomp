using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>
/// The state of one Skultera. It reads its speed, its wave's height and its wave's step from its spawn settings where it
/// uses them (#2450).
/// </summary>
public sealed class SkulteraEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/> toward <paramref name="heading"/>.</summary>
    internal SkulteraEnemyState(SkulteraAction action, HorizontalDirection heading)
    {
        Action = action;
        Heading = heading;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<SkulteraSegment> Playback { get; set; }

    /// <summary>The swimming or turning behaviour main AI runs.</summary>
    public SkulteraAction Action { get; internal set; }

    /// <summary>The way the Skultera swims, or turns to swim (#2165).</summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>The vertical oscillation's phase, carried on around the circle.</summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>
    /// Oscillation offset retained from the preceding movement attempt, in pixels (at most the radius either way): each
    /// update bobs the Skultera by its new offset's difference from this one (#2512).
    /// </summary>
    public int PreviousYOffset { get; internal set; }

    /// <summary>
    /// Advances the oscillation phase by the wave's step of <paramref name="settings"/>, around the circle: forward while
    /// the Skultera swims the way it started, backward once a turn has brought it the other way (#2450).
    /// </summary>
    internal void AdvanceAngle(SkulteraSpawnSettings settings) =>
        Angle = Angle.TurnedBySteps(Heading == Ensure.NotNull(settings).StartDirection ? settings.AngleDelta : -settings.AngleDelta);
}
