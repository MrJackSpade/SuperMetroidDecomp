using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// The state of one Dragon body or cosmetic wing. The body's motion timer and its three-shot counter are separate
/// countdowns here, since every phase starts its own before reading it.
/// </summary>
public sealed class DragonEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<DragonSegment> Playback { get; set; }

    /// <summary>Whether this is a body's cosmetic wing rather than the body.</summary>
    public bool IsWing { get; internal set; }

    /// <summary>Whether the body last turned to face left, toward a Samus on its left; the fireball spawns on that side.</summary>
    public bool FacingLeft { get; internal set; }

    /// <summary>The body's countdown while waiting, rising, or sinking.</summary>
    public int FunctionTimer { get; internal set; }

    /// <summary>The body's shots left while attacking; initialized to three before the first volley.</summary>
    public int AttackCounter { get; internal set; }

    /// <summary>The current behaviour.</summary>
    public DragonAction? Action { get; internal set; }

    /// <summary>The animation main AI asks for.</summary>
    public DragonAnimationSelector RequestedAnimation { get; internal set; }

    /// <summary>The animation already installed; a request for it installs nothing unless <see cref="ReinstallRequested"/>.</summary>
    public DragonAnimationSelector InstalledAnimation { get; internal set; }

    /// <summary>
    /// Whether the next install restarts the requested animation even when it is already installed: each fireball
    /// restarts the attack, which spaces the three fireballs by a whole attack animation.
    /// </summary>
    public bool ReinstallRequested { get; internal set; }

    /// <summary>Set by the attack animation's finishing cue.</summary>
    public bool AnimationFinished { get; internal set; }
}
