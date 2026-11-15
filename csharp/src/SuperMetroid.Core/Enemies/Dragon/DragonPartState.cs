using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// What a Dragon body and its cosmetic wing both keep: the animation each shows, which the body's AI requests for both.
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

    /// <summary>The animation main AI asks for.</summary>
    internal DragonSegment RequestedAnimation { get; set; }

    /// <summary>The animation already installed; a request for it installs nothing unless <see cref="ReinstallRequested"/>.</summary>
    internal DragonSegment InstalledAnimation { get; set; }

    /// <summary>
    /// Whether the next install restarts the requested animation even when it is already installed: each fireball
    /// restarts the attack, which spaces the three fireballs by a whole attack animation.
    /// </summary>
    public bool ReinstallRequested { get; internal set; }
}
