using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The bank-$A9 enemy-instruction state needed by Mother Brain's locomotion and posture
/// animations.
/// </summary>
/// <remarks>
/// Mother Brain's body movement is not performed by her AI function. The AI installs one of
/// the ordinary enemy instruction lists at `$A9:9730-$9972`; bank `$A0:C26A` later advances
/// that bytecode and its bank-$A9 opcodes mutate body position, pose, BG2 alignment, footsteps,
/// and the displayed extended spritemap. Keeping that second processing stage explicit is
/// essential: replacing a requested walk with a direct X adjustment would skip roughly ninety
/// visible animation frames and would change when the AI observes pose zero again.
///
/// This class deliberately admits only the command family used by the translated walk,
/// crouch, lean, and stand-up lists. Encountering any other command throws with its ROM
/// address, making future animation work an inspectable extension rather than silently
/// treating unknown bytecode as a frame record.
/// </remarks>
public sealed class MotherBrainBodyAnimationState
{

    /// <summary>Mother Brain body enemy-slot X position.</summary>
    public ushort XPosition { get; set; }

    /// <summary>Mother Brain body enemy-slot Y position.</summary>
    public ushort YPosition { get; set; }

    /// <summary>
    /// Native body pose word. The admitted programs publish zero standing, one walking,
    /// two transitioning between postures, three crouching, and six leaning down.
    /// </summary>
    public ushort Pose { get; set; }

    /// <summary>Phase/form word consulted by the native footstep sound gate.</summary>
    public ushort Form { get; set; }

    /// <summary>Current bank-$A9 enemy instruction pointer.</summary>
    public ushort InstructionPointer { get; private set; }

    /// <summary>Post-decremented enemy instruction timer.</summary>
    public ushort InstructionTimer { get; private set; }

    /// <summary>True after common enemy instruction `$812F` pins the program counter.</summary>
    public bool Sleeping { get; private set; }

    /// <summary>
    /// Equivalent of `$A9:C42D`: install a body list, make it eligible on the next enemy
    /// instruction-processing stage, and clear the prior sleep state.
    /// </summary>
    public void SetInstructionList(ushort pointer)
    {
        InstructionPointer = pointer;
        InstructionTimer = 1;
        Sleeping = false;
    }
}
