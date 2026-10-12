using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>The three native function words stored in Zebetite variable A.</summary>
public enum ZebetiteAiFunction : ushort
{
    /// <summary>$A6:FC41, Function_Zebetite_SpawnBottomZebetiteIfNeeded: primary setup allocates a linked half when the generation flag is negative, links native slot indices, then enters the door wait in the same update.</summary>
    SpawnLinkedHalf = 0xfc41,
    /// <summary>$A6:FC5B, Function_Zebetite_WaitForDoorTransitionToFinish: remains inert while shared WRAM $0795 is nonzero, then installs and executes the active function immediately.</summary>
    WaitForDoorTransition = 0xfc5b,
    /// <summary>$A6:FC67, Function_Zebetite_Active: updates palette/health artwork, regenerates one health per call up to 1000, or handles death with primary-owned generation progression.</summary>
    Active = 0xfc67,
}

/// <summary>
/// Debugger-facing view of the six common variables used by Tourian's Zebetite barrier.
/// Every native word remains in its physical <see cref="RoomEnemySlot"/> so a watch window
/// can be compared directly with <c>$0FA8-$0FB2</c> and the cartridge's linked-slot indices.
/// </summary>
public sealed class ZebetiteEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal ZebetiteEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native Zebetite.function in variable A ($0FA8 plus this half's slot offset): the bank-$A6 indirect main-AI entry, independent for each physical half.</summary>
    public ZebetiteAiFunction Function
    {
        get => (ZebetiteAiFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Variable C. Native code deliberately stores the palette-cycle counter in physical
    /// slot zero even after the first barrier generation has vacated that slot.
    /// </summary>
    public ushort PaletteCycle
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable D, the binary value formed from persistent events five/four/three.</summary>
    public ushort Generation
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Variable F, including the sign bit that requests a linked second half.</summary>
    public ushort GenerationFlags
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Parameter two contains the linked half's native <c>$40</c>-byte slot index.</summary>
    public ushort LinkedNativeIndex
    {
        get => _slot.Parameter2;
        internal set => _slot.Parameter2 = value;
    }

    /// <summary>Nonzero native init0/parameter one identifies the linked bottom half; it skips primary palette cycling and dies without publishing the next generation's events or respawn.</summary>
    public bool IsSecondaryHalf => _slot.Parameter1 != 0;
}
