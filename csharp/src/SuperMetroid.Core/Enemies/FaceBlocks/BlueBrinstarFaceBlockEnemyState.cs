using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// The state of the Blue Brinstar face block (<c>$A8:E8AE</c>). Its activation latch and last horizontal distance
/// are values the cartridge keeps in the enemy's common record, so a kill empties them with that record
/// (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class BlueBrinstarFaceBlockEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<BlueBrinstarFaceBlockProgram> Program { get; set; }

    /// <summary>
    /// Once set, the face has selected its directional animation and the main AI never performs the proximity test
    /// again.
    /// </summary>
    public bool Activated { get; internal set; }

    /// <summary>
    /// Samus's signed horizontal distance in room pixels from the face block, -32768 through 32767, written only after
    /// Samus passes the vertical range test; negative when Samus is to the left. The room-coordinate subtraction wraps
    /// at the word boundary, so the setter accepts either the signed distance or its wrapped word.
    /// </summary>
    public int LastHorizontalDelta
    {
        get;
        internal set => field = WordWrap.Signed(value);
    }

    /// <summary>
    /// Whether the face looks right (parameter two's bit zero). Activation needs Samus on the side opposite the one
    /// this names, so a face looking right wakes for Samus to its left.
    /// </summary>
    public bool FacesRight { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: not activated, no distance.</summary>
    internal void ClearWithRecord()
    {
        Activated = false;
        LastHorizontalDelta = 0;
        FacesRight = false;
    }
}
