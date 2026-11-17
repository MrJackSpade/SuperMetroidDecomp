using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What every one of Kraid's parts holds: the encounter it is part of, the place it occupies (#2298) and the phase timer
/// its timed phases wait on; his animated parts hold their typed animation too (<see cref="KraidAnimatedPartState"/>). Each part kind keeps the phase it runs, of its own type (#2208), and its own state on top of
/// this: his body (<see cref="KraidBodyState"/>), his arm (<see cref="KraidArmState"/>), each lint
/// (<see cref="KraidLintState"/>), his foot (<see cref="KraidFootState"/>) and each fingernail
/// (<see cref="KraidNailState"/>).
/// </summary>
public abstract class KraidPartState
{
    /// <summary>Makes a part of <paramref name="kraid"/>'s encounter.</summary>
    /// <param name="kraid">The encounter the part is part of.</param>
    /// <param name="reference">Kraid's reference to the part, which it joins as it sets up.</param>
    private protected KraidPartState(KraidEnemyState kraid, KraidPartReference reference)
    {
        Kraid = kraid;
        Reference = Ensure.NotNull(reference);
    }

    /// <summary>The encounter the part is part of, which the part holds from its setup (#2218).</summary>
    internal KraidEnemyState Kraid { get; }

    /// <summary>Kraid's reference to the part, which it joins as it sets up.</summary>
    private KraidPartReference Reference { get; }

    /// <summary>The place the part occupies (#2298), which joined as it set up.</summary>
    /// <exception cref="InvalidOperationException">The part has not set up.</exception>
    internal RoomEnemyPlace Place => Reference.Place;

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

    /// <summary>The updates before the part's timed phase moves on to the phase it names.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>
    /// Counts the phase timer down one update, from a running timer only: whether it ran out on this update, which moves
    /// the timed phase on. A timer at zero stays there, and the phase waits on it.
    /// </summary>
    internal bool CountPhaseTimerDown() => PhaseTimer != 0 && --PhaseTimer == 0;
}
