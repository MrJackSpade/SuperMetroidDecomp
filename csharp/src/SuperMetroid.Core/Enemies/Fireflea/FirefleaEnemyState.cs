using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// The state of a Fireflea: its animation and its flight, circling or bobbing, each with only the values that flight
/// keeps. Setup sets every value its flight reads.
/// </summary>
public sealed class FirefleaEnemyState
{
    /// <summary>Starts the state of a Fireflea flying <paramref name="flight"/>.</summary>
    internal FirefleaEnemyState(FirefleaFlightState flight) => Flight = Ensure.NotNull(flight);

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FirefleaSegment> Playback { get; set; }

    /// <summary>How the Fireflea flies, as its spawn settings chose: around a circle, or up and down between two rows.</summary>
    public FirefleaFlightState Flight { get; }
}
