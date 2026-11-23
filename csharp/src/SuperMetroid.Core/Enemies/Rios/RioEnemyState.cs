using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>The state of one Rio.</summary>
public sealed partial class RioEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal RioEnemyState(RioAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<RioSegment> Playback { get; set; }

    /// <summary>The sideways dive velocity, kept while the Rio homes in on Samus and taken back as it climbs.</summary>
    public SignedEightEight SavedHorizontalVelocity { get; internal set; }

    /// <summary>The behaviour that runs after the main AI's RNG advance.</summary>
    public RioAction Action { get; internal set; }

    /// <summary>The Rio's velocity each update, positive right and down.</summary>
    public PixelVelocity<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The finish of its swoop's opening or its cooldown animation (#2562), from the animation's end until its next
    /// step acts on it (#2450): a wall or floor can turn the dive into its rebound before the opening the dive waits
    /// for has finished, or on the update after; the opening's finish then carries through the rebound and, as the Rio
    /// lands, ends its wait for the cooldown animation at once.
    /// </summary>
    public RioAnimationFinish Finish { get; } = new();
}
