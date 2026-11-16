using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// What a Dragon body and its cosmetic wing both keep: the animation each shows, which the body's AI starts for both.
/// The body keeps the rest of its own state (<see cref="DragonEnemyState"/>); the wing keeps nothing more
/// (<see cref="DragonWingState"/>).
/// </summary>
public abstract class DragonPartState
{
    /// <summary>Only the body and the wing are Dragon parts.</summary>
    private protected DragonPartState()
    {
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DragonSegment> Playback { get; set; }
}
