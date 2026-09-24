namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>
    /// Redirects the sole live PLM to a constructed bank-$84 program for interpreter tests.
    /// This is not a gameplay operation: the real setup still owns allocation, block
    /// position, restore word and BTS, while the test controls only the next instruction.
    /// Keeping synthetic opcodes at nonretail addresses avoids rewriting an immutable
    /// compiled cartridge program in a fake address space.
    /// </summary>
    internal void SetSoleInstructionPointerForVerification(ushort instructionPointer)
    {
        PlmSlot[] active = _slots.Where(slot => slot.Active).ToArray();
        if (active.Length != 1)
            throw new InvalidOperationException(
                "Instruction probe requires exactly one active PLM slot.");
        active[0].InstructionPointer = instructionPointer;
        active[0].InstructionTimer = 1;
    }
}
