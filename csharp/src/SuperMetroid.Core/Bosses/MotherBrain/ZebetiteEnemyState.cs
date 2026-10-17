using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Debugger-facing view of the six common variables used by Tourian's Zebetite barrier.
/// Every native word remains in its physical <see cref="RoomEnemySlot"/> so a watch window
/// can be compared directly with <c>$0FA8-$0FB2</c> and the cartridge's linked-slot indices.
/// </summary>
public sealed class ZebetiteEnemyState
{
    /// <summary>Physical enemy slot retaining this Zebetite half's native variable words and linked-slot index.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Projects a single Zebetite half's debugger view over its live enemy slot.</summary>
    internal ZebetiteEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native Zebetite.function in variable A ($0FA8 plus this half's slot offset): the bank-$A6 indirect main-AI entry, independent for each physical half.</summary>
    public ZebetiteAiFunction Function
    {
        get => (ZebetiteAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Palette-cycle counter in variable C. Native code deliberately stores the shared counter
    /// in physical slot zero even after the first barrier generation has vacated that slot.
    /// </summary>
    public int PaletteCycle
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = checked((ushort)value);
    }

    /// <summary>Barrier generation 0 through 4 in variable D, formed from persistent events five/four/three; four means every barrier is destroyed.</summary>
    public int Generation
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = checked((ushort)value);
    }

    /// <summary>Variable F, including the sign bit that requests a linked second half.</summary>
    public ZebetiteGenerationFlags GenerationFlags
    {
        get => (ZebetiteGenerationFlags)_slot.VariableF;
        internal set => _slot.VariableF = value.ToNativeWord();
    }

    /// <summary>Where this half's health program resumes (#641).</summary>
    internal AnimationProgramCursor<ZebetiteProgram> Cursor { get; set; }

    /// <summary>The linked half's native <c>$40</c>-byte slot index, kept in parameter two.</summary>
    public int LinkedNativeIndex
    {
        get => _slot.Parameter2;
        internal set => _slot.Parameter2 = checked((ushort)value);
    }

    /// <summary>Nonzero native init0/parameter one identifies the linked bottom half; it skips primary palette cycling and dies without publishing the next generation's events or respawn.</summary>
    public bool IsSecondaryHalf => _slot.Parameter1 != 0;
}
