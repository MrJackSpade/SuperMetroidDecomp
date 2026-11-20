using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What every one of Kraid's parts holds: the encounter it is part of, the place it occupies (#2298) and the wait its
/// timed phases run; his animated parts hold their typed animation too (<see cref="KraidAnimatedPartState"/>). Each part kind keeps the phase it runs, of its own type (#2208), and its own state on top of
/// this: his body (<see cref="KraidBodyState"/>), his arm (<see cref="KraidArmState"/>), each lint
/// (<see cref="KraidLintState"/>), his foot (<see cref="KraidFootState"/>) and each fingernail
/// (<see cref="KraidNailState"/>).
/// </summary>
public abstract class KraidPartState
{
    /// <summary>Makes a part of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the part is part of.</param>
    /// <param name="place">The place the part occupies, which his body heads.</param>
    private protected KraidPartState(KraidEnemyState kraid, RoomEnemyPlace place)
    {
        Kraid = kraid;
        Place = Ensure.NotNull(place);
    }

    /// <summary>The encounter the part is part of, which the part holds from its setup (#2218).</summary>
    internal KraidEnemyState Kraid { get; }

    /// <summary>The place the part occupies (#2298, #2554).</summary>
    internal RoomEnemyPlace Place { get; }

    /// <summary>
    /// The pose the part's animation shows (#2210): its picture, where the picture sits and the depth it is drawn at; null
    /// before the animation shows one. The foot's collision moves with its pose's placement.
    /// </summary>
    internal KraidPartPose? Pose { get; private set; }

    /// <summary>Shows <paramref name="pose"/>: keeps it for the part's draw and collision, and returns the picture the part shows.</summary>
    /// <param name="pose">The pose the animation shows.</param>
    internal EnemyFrameId Show(KraidPartPose pose)
    {
        Pose = pose;
        return pose.Picture;
    }

    /// <summary>
    /// The wait the part's timed phase runs (#2538): the phase's start gives it its duration, and the part moves on to
    /// the phase it names on the wait's last update.
    /// </summary>
    public BossCountdown PhaseWait { get; } = new();
}
