using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// What each of an Evir's three parts keeps (#2063): the side it faces and its animation. The game builds each creature
/// from three consecutive enemies, a body (<see cref="EvirBodyState"/>), its arms (<see cref="EvirArmsState"/>) and a
/// reusable aimed spit (<see cref="EvirSpitState"/>), each with the state of its own part.
/// </summary>
public abstract class EvirPartState
{
    /// <summary>Closes the hierarchy to the three parts declared here.</summary>
    private protected EvirPartState()
    {
    }

    /// <summary>The side the part faces: the body's own, which the arms and spit copy when they position themselves.</summary>
    public EvirFacing Facing { get; internal set; }

    /// <summary>The last installed animation, not its advancing cursor; none forces installation of the requested one.</summary>
    internal EvirSegment? InstalledAnimation { get; set; }

    /// <summary>The animation requested by the part's current facing or the spit's phase; a changed request restarts the frame timer at one.</summary>
    internal EvirSegment RequestedAnimation { get; set; }

    /// <summary>The animation the part plays and where it stands in it (#1600), which its initialization installs.</summary>
    internal AnimationPlayback<EvirSegment> Playback { get; set; }
}
