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

    /// <summary>
    /// Whether it is spitting, which selects its spitting animation rather than its idle one (#2432): waking starts the
    /// spit and the spit's finishing cue ends it.
    /// </summary>
    internal abstract bool IsSpitting { get; }

    /// <summary>The way it faces, which selects its animations, from its spawn facing (#2277).</summary>
    public HorizontalDirection Facing { get; internal set; }

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
