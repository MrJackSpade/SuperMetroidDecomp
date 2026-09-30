namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    // Constructed interpreter fixtures are explicit, bounded instruction fragments,
    // never a general cartridge byte source. They are not production/save-state data.
    [NonSerialized]
    private Dictionary<ushort, byte>? verificationInstructionBytes;

    internal void SupplyInstructionFragmentForVerification(ushort pointer, ReadOnlySpan<byte> instructions)
    {
        if (instructions.Length is < 1 or > 256)
            throw new ArgumentOutOfRangeException(nameof(instructions), "A fixture instruction fragment must contain 1..256 bytes.");
        verificationInstructionBytes ??= [];
        for (int offset = 0; offset < instructions.Length; offset++)
            verificationInstructionBytes[unchecked((ushort)(pointer + offset))] = instructions[offset];
        if (verificationInstructionBytes.Count > 1024)
            throw new InvalidOperationException("PLM instruction fixtures may not become a bank-sized data image.");
    }

    private bool TryReadVerificationInstructionWord(ushort pointer, out ushort value)
    {
        if (verificationInstructionBytes is not null &&
            verificationInstructionBytes.TryGetValue(pointer, out byte low) &&
            verificationInstructionBytes.TryGetValue(unchecked((ushort)(pointer + 1)), out byte high))
        {
            value = (ushort)(low | high << 8);
            return true;
        }
        value = 0;
        return false;
    }

    private bool TryReadVerificationInstructionByte(ushort pointer, out byte value)
    {
        value = 0;
        return verificationInstructionBytes?.TryGetValue(pointer, out value) == true;
    }

    /// <summary>
    /// Redirects the sole live PLM to a constructed bank-$84 program for interpreter tests.
    /// This is not a gameplay operation: the real setup still owns allocation, block
    /// position, restore word and BTS, while the test controls only the next instruction.
    /// Keeping synthetic opcodes at nonretail addresses avoids rewriting an immutable
    /// compiled cartridge program in a fake address space.
    /// </summary>
    internal void SetSoleInstructionPointerForVerification(ushort instructionPointer,
        ReadOnlySpan<ushort> instructions = default)
    {
        PlmSlot[] active = _slots.Where(slot => slot.Active).ToArray();
        if (active.Length != 1)
            throw new InvalidOperationException(
                "Instruction probe requires exactly one active PLM slot.");
        if (!instructions.IsEmpty)
        {
            var bytes = new byte[instructions.Length * 2];
            for (int index = 0; index < instructions.Length; index++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(index * 2), instructions[index]);
            SupplyInstructionFragmentForVerification(instructionPointer, bytes);
        }
        active[0].InstructionPointer = instructionPointer;
        active[0].InstructionTimer = 1;
    }
}
