using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>The state of the Blue Brinstar face block.</summary>
public sealed class BlueBrinstarFaceBlockEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<BlueBrinstarFaceBlockProgram> Program { get; set; }

    /// <summary>
    /// Once set, the face has selected its directional animation and the main AI never performs the proximity test
    /// again.
    /// </summary>
    public bool Activated { get; internal set; }

    /// <summary>
    /// Samus's signed horizontal distance in room pixels from the face block, -32768 through 32767, written only after
    /// Samus passes the vertical range test; negative when Samus is to the left. The room-coordinate subtraction wraps
    /// across the room's wrapping axis (<see cref="CoordinateAxis.Difference"/>).
    /// </summary>
    public int LastHorizontalDelta { get; internal set; }

    /// <summary>
    /// Whether the face looks right (parameter two's bit zero). Activation needs Samus on the side opposite the one
    /// this names, so a face looking right wakes for Samus to its left.
    /// </summary>
    public bool FacesRight { get; internal set; }
}
