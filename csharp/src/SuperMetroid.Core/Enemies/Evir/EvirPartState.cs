using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// What each of an Evir's three parts keeps (#2063): the side it faces and its animation. The game builds each creature
/// from three consecutive enemies, a body (<see cref="EvirBodyState"/>), its arms (<see cref="EvirArmsState"/>) and a
/// reusable aimed spit (<see cref="EvirSpitState"/>), each with the state of its own part.
/// </summary>
public abstract partial class EvirPartState
{
    /// <summary>Closes the hierarchy to the three parts declared here.</summary>
    private protected EvirPartState()
    {
    }

    /// <summary>The side the part faces: the body's own, which the arms and spit copy when they position themselves.</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>The animation the part plays and where it stands in it (#1600), which its initialization starts.</summary>
    internal AnimationPlayback<EvirSegment> Playback { get; set; }

    /// <summary>
    /// Whether the part still shows the animation its initialization started (#2145). Its first request starts the
    /// requested animation afresh even when it is that same animation, so the body's and arms' first update and the spit's
    /// first launch restart the frame timer; a body that has not yet updated reads as facing left to its spit's
    /// regeneration.
    /// </summary>
    internal bool ShowsSpawnAnimation { get; set; }
}
