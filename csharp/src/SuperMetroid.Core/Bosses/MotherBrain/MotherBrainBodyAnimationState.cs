namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The bank-$A9 enemy-instruction state needed by Mother Brain's locomotion and posture
/// animations.
/// </summary>
/// <remarks>
/// Mother Brain's body movement is not performed by her AI function. The AI installs one of
/// the body programs (<see cref="MotherBrainBodyProgram"/>); the enemy instruction stage later advances
/// that program and its cues mutate body position, pose, BG2 alignment, footsteps,
/// and the displayed extended spritemap. Keeping that second processing stage explicit is
/// essential: replacing a requested walk with a direct X adjustment would skip roughly ninety
/// visible animation frames and would change when the AI observes pose zero again.
/// </remarks>
public sealed class MotherBrainBodyAnimationState
{

    /// <summary>Mother Brain body enemy-slot whole-pixel room position.</summary>
    public Position16<RoomPixels> Position { get; set; }

    /// <summary>
    /// Native body pose word. The admitted programs publish standing, walking, the
    /// posture transition, crouched and leaning down.
    /// </summary>
    public MotherBrainBodyPose Pose { get; set; }

    /// <summary>Phase/form word consulted by the native footstep sound gate.</summary>
    public ushort Form { get; set; }

    /// <summary>The body program most recently requested, or null before the first request.</summary>
    public MotherBrainBodyProgram? Program { get; private set; }

    /// <summary>
    /// Equivalent of `$A9:C42D`: requests a body program; the physical body slot starts it on the next enemy
    /// instruction-processing stage.
    /// </summary>
    public void SetProgram(MotherBrainBodyProgram program) => Program = program;
}
