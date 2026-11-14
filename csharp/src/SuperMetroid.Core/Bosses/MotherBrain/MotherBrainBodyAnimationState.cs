namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The animation state needed by Mother Brain's locomotion and posture
/// animations.
/// </summary>
/// <remarks>
/// Mother Brain's body movement is not performed by her AI phase. The AI installs one of
/// the body segments (<see cref="MotherBrainBodySegment"/>); the enemy animation stage later advances
/// that animation and its cues mutate body position, pose, the background's alignment, footsteps,
/// and the displayed frame. Keeping that second processing stage explicit is
/// essential: replacing a requested walk with a direct X adjustment would skip roughly ninety
/// visible animation frames and would change when the AI observes her standing pose again.
/// </remarks>
public sealed class MotherBrainBodyAnimationState
{

    /// <summary>Mother Brain body enemy-slot whole-pixel room position.</summary>
    public Position16<RoomPixels> Position { get; set; }

    /// <summary>
    /// Body pose. The admitted animations publish standing, walking, the
    /// posture transition, crouched and leaning down.
    /// </summary>
    public MotherBrainBodyPose Pose { get; set; }

    /// <summary>The encounter stage, consulted by shot reactions and the footstep sound gate.</summary>
    public MotherBrainForm Form { get; set; }

    /// <summary>The body animation most recently requested, or null before the first request.</summary>
    public MotherBrainBodySegment? Animation { get; private set; }

    /// <summary>
    /// Requests a body animation; the physical body slot starts it on the next enemy
    /// animation stage.
    /// </summary>
    public void SetAnimation(MotherBrainBodySegment segment) => Animation = segment;
}
