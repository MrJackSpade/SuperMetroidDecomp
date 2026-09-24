using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Fixed bank-$84 resident downward-gate and eight shot-trigger instruction streams.
/// Referenced draw-list payloads and bank-$86 gate actor art are separate data.
/// </summary>
internal static class DownwardGatePlmProgramDefinitions
{
    /// <summary>Resident gate's first closed-frame entry at $84:BC13.</summary>
    private const ushort ClosedStart = 0xbc13;
    /// <summary>Cartridge library-three movement sound operand in both gate phases.</summary>
    private const byte MovementSound = DownwardGatePlmRomData.MovementSound;
    /// <summary>Opening gate's odd-byte sound operand at $84:BC29.</summary>
    private const ushort OpeningSoundAddress = 0xbc29;
    /// <summary>Closing gate's odd-byte sound operand at $84:BC4C.</summary>
    private const ushort ClosingSoundAddress = 0xbc4c;

    private static readonly IReadOnlyDictionary<ushort, ushort> Words = BuildWords();
    private static readonly IReadOnlyDictionary<ushort, byte> Bytes =
        new Dictionary<ushort, byte>
        {
            [OpeningSoundAddress] = MovementSound,
            [ClosingSoundAddress] = MovementSound,
        };

    internal static IEnumerable<(ushort Address, ushort Value)> MechanicsWords =>
        Words.Select(pair => (pair.Key, pair.Value));

    internal static IEnumerable<(ushort Address, byte Value)> MechanicsBytes =>
        Bytes.Select(pair => (pair.Key, pair.Value));

    internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
        Words.TryGetValue(address, out value);

    internal static bool TryReadMechanicsByte(ushort address, out byte value) =>
        Bytes.TryGetValue(address, out value);

    private static IReadOnlyDictionary<ushort, ushort> BuildWords()
    {
        var words = new Dictionary<ushort, ushort>();
        void Add(int address, ushort value)
        {
            if (!words.TryAdd(checked((ushort)address), value))
                throw new InvalidDataException(
                    $"Duplicate compiled downward-gate control word $84:{address:X4}.");
        }

        void Timed(int address, ushort duration, ushort drawPointer)
        {
            Add(address, duration);
            Add(address + 2, drawPointer);
        }

        Timed(ClosedStart, 1, 0xa517);
        Add(0xbc17, RoomPlmInstructionCodes.ClearDownwardGateTrigger);
        Add(0xbc19, RoomPlmInstructionCodes.InstallPreInstruction);
        Add(0xbc1b, DownwardGatePreInstructionCodes.WakeIfTriggered);
        Add(0xbc1d, RoomPlmInstructionCodes.Sleep);
        Timed(0xbc1f, 16, 0xa517);
        Add(0xbc23, RoomPlmInstructionCodes.SpawnDownwardGateProjectile);
        Add(0xbc25, (ushort)RoomEnemyProjectileKind.DownwardGateMoving);
        Add(0xbc27, RoomPlmInstructionCodes.QueueSoundLibrary3Maximum6);
        Timed(0xbc2a, 16, 0xa525);
        Timed(0xbc2e, 16, 0xa533);
        Timed(0xbc32, 16, 0xa541);
        Timed(0xbc36, 24, 0xa54f);

        Timed(RoomPlmInstructionLists.DownwardGateOpening, 1, 0xa55d);
        Add(0xbc3e, RoomPlmInstructionCodes.ClearDownwardGateTrigger);
        Add(0xbc40, RoomPlmInstructionCodes.InstallPreInstruction);
        Add(0xbc42, DownwardGatePreInstructionCodes.WakeIfTriggeredOrSamusBelow);
        Add(0xbc44, RoomPlmInstructionCodes.Sleep);
        Add(0xbc46, RoomPlmInstructionCodes.WakeDownwardGateProjectile);
        // Native $BBF0 consumes but ignores this bank-$86 list operand.
        Add(0xbc48, DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep);
        Add(0xbc4a, RoomPlmInstructionCodes.QueueSoundLibrary3Maximum6);
        Timed(0xbc4d, 16, 0xa54f);
        Timed(0xbc51, 16, 0xa541);
        Timed(0xbc55, 16, 0xa533);
        Timed(0xbc59, 24, 0xa525);
        Add(0xbc5d, RoomPlmInstructionCodes.Goto);
        Add(0xbc5f, ClosedStart);

        ushort[] triggerDraws =
            [0xa5d7, 0xa5e3, 0xa5eb, 0xa5f7, 0xa5ff, 0xa60b, 0xa613, 0xa61f];
        for (int index = 0; index < triggerDraws.Length; index++)
        {
            int start = RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft + 6 * index;
            Timed(start, 1, triggerDraws[index]);
            Add(start + 4, RoomPlmInstructionCodes.Delete);
        }

        return words;
    }
}
