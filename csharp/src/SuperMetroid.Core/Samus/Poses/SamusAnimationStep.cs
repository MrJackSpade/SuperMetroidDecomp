using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What one Samus animation step reads (#633): the controller input, the frame counter, the shared random generator, the prospective input pose, whether a demo supplies the pose input, and whether a boss is present.</summary>
public sealed record SamusAnimationStep
{
    /// <summary>The actions held this update.</summary>
    public GameplayAction ControllerInput { get; init; } = GameplayAction.None;

    /// <summary>
    /// The frame clock's value for this step, 0 through 65535: the clock wraps back to 0 after 65535 and paces
    /// liquid effects, so every value of its cycle is carried; other values are rejected.
    /// </summary>
    public int FrameCounter
    {
        get;
        init => field = Ensure.BetweenInclusive(0, Compatibility.FrameClock.CounterPeriod - 1, value);
    }

    /// <summary>The session's random-number generator, which Samus's liquid effects draw air bubbles from.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>The pose this update's input selected, if any: a jump it selected survives a turn's last frame.</summary>
    public SamusPoseId? ProspectiveInputPose { get; init; } = null;

    /// <summary>Whether a demo, not the player, drives the pose input; a demo's turn never arms the automatic jump.</summary>
    public bool DemoPoseInput { get; init; } = false;

    /// <summary>
    /// The ground her running footsteps go by: the room's, or a scene's own where no room has loaded (#2554).
    /// </summary>
    public required SamusGround Ground { get; init; }

    /// <summary>Whether the room has a boss, which suppresses the running-footstep sound.</summary>
    public bool BossPresent { get; init; } = false;
}
