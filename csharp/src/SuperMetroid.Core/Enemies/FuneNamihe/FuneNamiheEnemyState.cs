using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>
/// What a Fune and a Namihe, the two species that share this actor's animations and fireball, each keep (#2063): the
/// animation and whether it is spitting. Each faces and launches its fireballs as its spawn settings say, which it reads
/// where it uses them (#2476). A Fune's state is a
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

    /// <summary>What both kinds' placements tell them: their facing and their fireball's speed (#2554).</summary>
    internal abstract FuneNamiheSpawnSettings SharedSettings { get; }

    /// <summary>The pixels the spitter's mouth, from which its fireballs leave, sits below its position in the shared art.</summary>
    internal abstract int MouthDrop { get; }

    /// <summary>The fireball the species spits.</summary>
    internal abstract RoomEnemyProjectileKind Fireball { get; }

    /// <summary>The animation the species and activity select facing <paramref name="facing"/>, its spawn settings' facing.</summary>
    internal abstract FuneNamiheSegment Animation(HorizontalDirection facing);

    /// <summary>Ends the spit, as its animation's finishing cue does: back to the idle animation and the species' wait.</summary>
    internal abstract void FinishSpit();
}
