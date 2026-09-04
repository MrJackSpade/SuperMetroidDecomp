using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The simple looping bank-$87 animated-tile object spawned directly by a room-FX type.
/// </summary>
/// <remarks>
/// Lava, acid, and rain do not obtain their visible characters from the room tileset.
/// Their bank-$88 type initializer calls <c>SpawnAnimtiles</c> with one of three object
/// headers. Each header owns an instruction list, transfer size, and VRAM destination.
/// Keeping this as an instruction-list owner matters: copying one convenient frame would
/// make the first still image look plausible while discarding the cartridge's cadence and
/// preventing future replacement samples/graphics from following the same data path.
/// </remarks>
internal sealed class RoomFxAnimatedTilesState
{
    private ushort objectPointer;
    private ushort instructionPointer;
    private ushort instructionTimer;
    private ushort transferByteCount;
    private ushort encodedVramDestination;

    /// <summary>Whether a room-FX animated-tile object currently owns this slot.</summary>
    public bool IsActive { get; private set; }

    /// <summary>The most recent cartridge source copied during a handler pass.</summary>
    public int? LastSourceAddress { get; private set; }

    /// <summary>
    /// Replaces the previous room's object with the one spawned by the selected FX type.
    /// Non-animated types deliberately leave the owner empty.
    /// </summary>
    public void Load(ISnesAddressSpace bus, RoomFxType type)
    {
        ArgumentNullException.ThrowIfNull(bus);
        Reset();

        objectPointer = type switch
        {
            RoomFxType.Lava => AnimatedTileObjectPointers.Lava,
            RoomFxType.Acid => AnimatedTileObjectPointers.Acid,
            RoomFxType.Rain => AnimatedTileObjectPointers.Rain,
            _ => 0,
        };
        if (objectPointer == 0)
            return;

        instructionPointer = ReadWord(bus, objectPointer);
        transferByteCount = ReadWord(bus, unchecked((ushort)(objectPointer + 2)));
        encodedVramDestination = ReadWord(bus, unchecked((ushort)(objectPointer + 4)));
        instructionTimer = 1;
        IsActive = true;

        if (transferByteCount == 0)
        {
            throw new InvalidDataException(
                $"Room-FX animated-tile object $87:{objectPointer:X4} has no transfer bytes.");
        }
    }

    /// <summary>
    /// Executes one <c>AnimtilesHandler</c> pass and performs the NMI-visible transfer.
    /// These three retail lists contain timed source frames, <c>goto</c>, and no other
    /// commands; encountering anything else fails loudly instead of freezing the texture.
    /// </summary>
    public void Step(ISnesAddressSpace bus, SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        LastSourceAddress = null;
        if (!IsActive)
            return;

        instructionTimer = unchecked((ushort)(instructionTimer - 1));
        if (instructionTimer != 0)
            return;

        ushort cursor = instructionPointer;
        for (int guard = 0; guard < 32; guard++)
        {
            ushort instructionOrDuration = ReadWord(bus, cursor);
            if ((instructionOrDuration & 0x8000) == 0)
            {
                if (instructionOrDuration == 0)
                {
                    throw new InvalidDataException(
                        $"Room-FX animated-tile object $87:{objectPointer:X4} has a zero-duration " +
                        $"frame at $87:{cursor:X4}.");
                }

                ushort sourcePointer = ReadWord(bus, unchecked((ushort)(cursor + 2)));
                instructionTimer = instructionOrDuration;
                instructionPointer = unchecked((ushort)(cursor + 4));
                LastSourceAddress = RoomFxRomData.Banks.AnimatedTiles | sourcePointer;
                vram.ExecuteHardwareDmaWrite(
                    bus,
                    LastSourceAddress.Value,
                    transferByteCount,
                    encodedVramDestination);
                return;
            }

            switch (instructionOrDuration)
            {
                case AnimatedTileInstructionCodes.Delete:
                    Reset();
                    return;

                case AnimatedTileInstructionCodes.Goto:
                    cursor = ReadWord(bus, unchecked((ushort)(cursor + 2)));
                    break;

                default:
                    throw new NotSupportedException(
                        $"Room-FX animated-tile object $87:{objectPointer:X4} reached " +
                        $"untranslated instruction $87:{instructionOrDuration:X4} at " +
                        $"$87:{cursor:X4}.");
            }
        }

        throw new InvalidDataException(
            $"Room-FX animated-tile object $87:{objectPointer:X4} exceeded 32 leading " +
            $"instructions at $87:{cursor:X4}.");
    }

    /// <summary>Clears the room-owned object when its FX record is replaced or absent.</summary>
    public void Reset()
    {
        IsActive = false;
        LastSourceAddress = null;
        objectPointer = 0;
        instructionPointer = 0;
        instructionTimer = 0;
        transferByteCount = 0;
        encodedVramDestination = 0;
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        RomDataReader.ReadWordFixedBank(
            bus,
            RoomFxRomData.Banks.AnimatedTiles | pointer);
}
