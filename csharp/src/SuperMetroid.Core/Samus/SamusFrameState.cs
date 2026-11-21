using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// Samus and her state across frames: the actor, movement mode, prospective poses, colours and load appearance. What one
/// frame's movement, grapple, X-ray and death steps did is that frame's own (<see cref="SamusControlFrame"/>).
/// </summary>
internal sealed partial class SamusFrameState : IRoomSamusAccess
{
    /// <summary>Samus's colours, which outlive each Samus (#1405).</summary>
    public SamusColours Colours { get; }

    /// <summary>Samus's frame state, her colours in their starting shades.</summary>
    internal SamusFrameState() => Colours = new SamusColours();

    /// <summary>The saved-game load appearance, before ordinary Samus drawing resumes.</summary>
    internal UpdateCountdown LoadAppearance { get; set; } = UpdateCountdown.Ended();

    /// <summary>Whether Samus's saved-game load appearance is still running.</summary>
    internal bool LoadAppearanceActive => LoadAppearance.IsRunning;

    /// <summary>Starts the saved-game load appearance with its length.</summary>
    internal void BeginLoadAppearance(int frames) => LoadAppearance = UpdateCountdown.For(frames);

    /// <summary>Counts one load-appearance update; returns whether that was its last.</summary>
    internal bool AdvanceLoadAppearance() => LoadAppearance.StepEnds();

    /// <summary>
    /// Normal-gameplay Samus rendering state. Null means the current scenario has not
    /// explicitly introduced Samus; the Landing Site cinematic does not do so by itself.
    /// </summary>
    public SamusState? Samus { get; internal set; }

    /// <summary>Samus once the scenario has introduced her; gameplay phases that move, draw or damage her require it.</summary>
    public SamusState IntroducedSamus => Samus ??
        throw new InvalidOperationException("Samus has not been introduced; this gameplay phase requires her actor.");

    /// <summary>
    /// Samus as the room's colour effects see her each time one reads her (#2391); an effect reading her before the scenario
    /// introduces her fails there.
    /// </summary>
    IRoomSamus IRoomSamusAccess.Current => IntroducedSamus.RoomView;

    /// <summary>
    /// The input transition the latched held and newly pressed actions selected, which the pose-transition phase applies
    /// after movement and animation, or none when no transition matched.
    /// </summary>
    public SamusPoseTransition? ProspectiveSamusPose { get; internal set; }

    /// <summary>
    /// Block-only result of the wall check after movement. This is separate from the input
    /// transition because a killed running speed can create a ran-into-wall pose even when
    /// no input transition matched, and a one-pixel probe can replace a running target.
    /// </summary>
    public SamusPoseId? ProspectiveSamusWallCollisionPose { get; internal set; }

    /// <summary>
    /// No-button fallback selected when the pose handling cancels the grapple. This is
    /// separate from <see cref="SamusFrameState.ProspectiveSamusPose"/> because no input transition wins.
    /// </summary>
    public SamusPoseId? ProspectiveSamusFallbackPose { get; internal set; }

    /// <summary>
    /// Whether Samus's input selects poses and her pose movement and pose transitions run: closed while a room load brings
    /// her in and while a scripted scene holds her still, opened when control returns to her. It is the frame's gate, apart
    /// from Samus's own control lock (#2560): a door's load opens it with her lock untouched, a station's and the Ceres
    /// start's close it with no lock of their own, and the gunship's landing and the saved game's fanfare open it on a later
    /// update than the one that unlocks her, so the two are not one lock state.
    /// </summary>
    public bool GroundedSamusMovementEnabled { get; internal set; }

    /// <summary>
    /// Snapshot of the previous movement type, used by X-ray stability and post-draw spin audio.
    /// The runtime updates it after each completed gameplay frame.
    /// </summary>
    public SamusMovementType PreviousMovementTypeForXray { get; internal set; }

    /// <summary>
    /// Forgets the pending pose change: the input transition the input phase selected and the grapple cancel's fallback
    /// pose, so the pose-transition phase applies neither.
    /// </summary>
    internal void ClearProspectivePoses()
    {
        ProspectiveSamusPose = null;
        ProspectiveSamusFallbackPose = null;
    }
}
