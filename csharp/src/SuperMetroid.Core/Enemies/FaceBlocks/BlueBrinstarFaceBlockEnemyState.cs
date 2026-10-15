using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>
/// Debugger-facing names for the two private words used by <c>$A8:E8AE</c>. The activation
/// latch and last horizontal delta remain in the ordinary enemy slot, so watches still map
/// directly to native variables A and B instead of drifting into a parallel object model.
/// </summary>
public sealed class BlueBrinstarFaceBlockEnemyState
{
    /// <summary>Native enemy slot whose variables A and B hold the face block's observable state.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds debugger-facing state to the original physical enemy slot.</summary>
    internal BlueBrinstarFaceBlockEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<BlueBrinstarFaceBlockProgram> Program { get; set; }

    /// <summary>
    /// Native variable A. Once set, the face has selected its directional animation and the
    /// main AI never performs the proximity test again.
    /// </summary>
    public bool Activated
    {
        get => _slot.VariableA != 0;
        internal set => _slot.VariableA = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Native variable B, written only after Samus passes the vertical range test. Retaining
    /// the wrapped 16-bit subtraction makes left-side deltas directly comparable to WRAM.
    /// </summary>
    public ushort LastHorizontalDelta
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }
}
