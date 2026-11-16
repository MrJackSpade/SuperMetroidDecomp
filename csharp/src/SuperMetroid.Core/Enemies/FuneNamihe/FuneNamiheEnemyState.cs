using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// What a Fune and a Namihe, the two species that share this actor's animations and fireball, each keep (#2063): the
/// animation, whether it is spitting, its facing and its fireballs' launch speed. A Fune's state is a
/// <see cref="FuneEnemyState"/>, which waits on a cooldown; a Namihe's a <see cref="NamiheEnemyState"/>, which waits for
/// Samus.
/// </summary>
public abstract class FuneNamiheEnemyState
{
    /// <summary>Closes the hierarchy to the two species declared here.</summary>
    private protected FuneNamiheEnemyState()
    {
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<FuneNamiheSegment> Playback { get; set; }

    /// <summary>Whether the spitting animation is selected rather than the idle one; waking selects it and the spit's finishing cue deselects it.</summary>
    public bool ShowsActiveAnimation { get; internal set; }

    /// <summary>Whether the right-facing animations are selected, from its spawn facing.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>The speed, in pixels per update, at which its fireballs fly away from it, from its spawn settings.</summary>
    public SignedEightEight LaunchSpeed { get; internal set; }

    /// <summary>The pixels the spitter's mouth, from which its fireballs leave, sits below its position in the shared art.</summary>
    internal abstract int MouthDrop { get; }

    /// <summary>The fireball the species spits.</summary>
    internal abstract RoomEnemyProjectileKind Fireball { get; }

    /// <summary>The animation the species, activity and facing select.</summary>
    internal abstract FuneNamiheSegment Animation { get; }

    /// <summary>Ends the spit, as its animation's finishing cue does: back to the idle animation and the species' wait.</summary>
    internal abstract void FinishSpit();
}
