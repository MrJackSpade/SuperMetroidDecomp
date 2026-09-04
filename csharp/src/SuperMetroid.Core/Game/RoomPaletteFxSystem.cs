using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Cartridge-driven owner for the eight bank-$8D palette-FX slots created by room FX.
/// </summary>
/// <remarks>
/// Room graphics load a useful static palette, but many rooms immediately replace part of
/// it with bytecode selected by the active bank-$83 FX record. Landing Site is one of them:
/// palette-FX bit zero writes colors 84 onward, which are used by the scrolling-sky horizon.
/// Keeping the native pointers, timers, and byte-indexed destination here avoids encoding a
/// screenshot's colors as a room-specific correction.
/// </remarks>
public sealed class RoomPaletteFxSystem
{
    private const int SlotCount = 8;

    private readonly PaletteFxSlot[] slots = Enumerable.Range(0, SlotCount)
        .Select(_ => new PaletteFxSlot())
        .ToArray();
    private readonly List<PaletteFxSoundRequest> soundRequests = [];
    private readonly List<PaletteFxMusicRequest> musicRequests = [];

    /// <summary>Number of bank-$8D objects currently occupying native slots.</summary>
    public int ActiveCount => slots.Count(slot => slot.Id != 0);

    /// <summary>Sound calls published by palette bytecode during the current frame.</summary>
    public IReadOnlyList<PaletteFxSoundRequest> SoundRequests => soundRequests;

    /// <summary>Music calls published by palette bytecode during the current frame.</summary>
    public IReadOnlyList<PaletteFxMusicRequest> MusicRequests => musicRequests;

    /// <summary>Whether a particular cartridge definition currently owns a native slot.</summary>
    public bool IsDefinitionActive(ushort definition) =>
        slots.Any(slot => slot.Id == definition);

    /// <summary>
    /// Spawns a non-room palette object through the same native allocator/interpreter used
    /// by room FX. Samus's load appearance uses definitions $E1F4/$E1F8/$E1FC and must not
    /// be reimplemented as a host-side tint.
    /// </summary>
    public void SpawnDefinition(
        ISnesAddressSpace bus,
        ushort definition,
        ushort equippedItems,
        bool areaMiniBossDefeated = false)
    {
        ArgumentNullException.ThrowIfNull(bus);
        Spawn(bus, definition, equippedItems, areaMiniBossDefeated);
        if (!IsDefinitionActive(definition))
        {
            throw new InvalidOperationException(
                $"Palette-FX definition $8D:{definition:X4} could not acquire a native slot.");
        }
    }

    /// <summary>
    /// Clears the old room's objects and spawns exactly the bits selected by the matching
    /// sixteen-byte bank-$83 FX record.
    /// </summary>
    public void LoadRoom(
        ISnesAddressSpace bus,
        ushort fxPointer,
        ushort doorPointer,
        AreaId area,
        ushort equippedItems,
        bool areaMiniBossDefeated)
    {
        ArgumentNullException.ThrowIfNull(bus);
        foreach (PaletteFxSlot slot in slots)
            slot.Clear();
        soundRequests.Clear();
        musicRequests.Clear();

        if (fxPointer == 0)
            return;
        int areaIndex = AreaIds.ToIndex(area);

        ushort record = RoomFxRomData.SelectRecord(bus, fxPointer, doorPointer);
        if (record == 0)
            return;

        // FxDef offsets $0D/$0E are independent palette-FX and animtile bitsets. Reading
        // only the former is deliberate; a separate bank-$87 owner must consume the latter.
        byte paletteFxBits = RoomFxRomData.ReadRecordByte(
            bus,
            record,
            RoomFxRomData.Record.PaletteFxBitsetOffset);
        if (paletteFxBits == 0)
            return;

        ushort areaList = ReadWord(
            bus,
            RoomFxRomData.Tables.AreaPaletteFxObjectListPointers + areaIndex * 2);
        for (int bit = 0; bit < SlotCount; bit++)
        {
            if ((paletteFxBits & (1 << bit)) == 0)
                continue;

            ushort definition = ReadWord(
                bus,
                RoomFxRomData.Banks.RoomDefinitions |
                    unchecked((ushort)(areaList + bit * 2)));
            Spawn(bus, definition, equippedItems, areaMiniBossDefeated);
        }
    }

    /// <summary>Runs one native <c>PaletteFXObject_Handler</c> pass in descending slot order.</summary>
    public void Step(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        ushort samusY,
        ushort equippedItems,
        bool enemyZeroIsDead,
        bool areaMiniBossDefeated)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        // PaletteFXObject_Handler owns a new publication window on every game frame.
        // Requests are momentary calls into bank $80, not persistent object state.
        soundRequests.Clear();
        musicRequests.Clear();

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            PaletteFxSlot slot = slots[slotIndex];
            if (slot.Id == 0)
                continue;

            RunPreInstruction(
                slot,
                samusY,
                equippedItems,
                enemyZeroIsDead,
                areaMiniBossDefeated);
            if (slot.Id == 0)
                continue;

            // `$8D:C552` decrements before comparing with one. Unsigned underflow is
            // intentional cartridge behavior for malformed or externally edited state.
            slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
            if (slot.InstructionTimer != 0)
                continue;

            ExecuteProgram(bus, cgram, slot);
        }
    }

    private void Spawn(
        ISnesAddressSpace bus,
        ushort definition,
        ushort equippedItems,
        bool areaMiniBossDefeated)
    {
        // Spawn_PaletteFXObject searches byte offsets 14,12,...,0, which is slot order
        // 7..0. A full native pool silently rejects another spawn; this is a documented
        // allocation result, not a failed translation that should be hidden by an exception.
        PaletteFxSlot? slot = null;
        for (int index = SlotCount - 1; index >= 0; index--)
        {
            if (slots[index].Id == 0)
            {
                slot = slots[index];
                break;
            }
        }
        if (slot is null)
            return;

        slot.Id = definition;
        slot.ColorByteIndex = 0;
        slot.PreInstruction = PaletteFxPreInstructionCodes.Null;
        slot.InstructionPointer = ReadBank8dWord(bus, unchecked((ushort)(definition + 2)));
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        ushort setup = ReadBank8dWord(bus, definition);
        switch (setup)
        {
            case PaletteFxSetupCodes.Null:
                return;

            case PaletteFxSetupCodes.Intro:
                slot.PreInstruction = PaletteFxPreInstructionCodes.Intro;
                return;

            case PaletteFxSetupCodes.Norfair:
                // `$8D:E440` chooses the complete program from the same live suit bits
                // used by Samus. Gravity has priority when both bits are present.
                slot.InstructionPointer = equippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                    ? PaletteFxInstructionListPointers.NorfairGravitySuit
                    : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                        ? PaletteFxInstructionListPointers.NorfairVariaSuit
                        : PaletteFxInstructionListPointers.NorfairPowerSuit;
                return;

            case PaletteFxSetupCodes.Brinstar:
                if (areaMiniBossDefeated)
                    slot.Clear();
                return;

            default:
                throw new NotSupportedException(
                    $"Palette-FX object $8D:{definition:X4} setup $8D:{setup:X4} is not translated.");
        }
    }

    private static void RunPreInstruction(
        PaletteFxSlot slot,
        ushort samusY,
        ushort equippedItems,
        bool enemyZeroIsDead,
        bool areaMiniBossDefeated)
    {
        switch (slot.PreInstruction)
        {
            case PaletteFxPreInstructionCodes.Null:
            case PaletteFxPreInstructionCodes.Cleared:
            case PaletteFxPreInstructionCodes.Intro:
                return;

            case PaletteFxPreInstructionCodes.DeleteWhenEnemyZeroDies:
                if (enemyZeroIsDead)
                    slot.Clear();
                return;

            case PaletteFxPreInstructionCodes.SwitchAboveY380:
                if (samusY < 0x0380)
                {
                    slot.InstructionTimer = 1;
                    slot.InstructionPointer = PaletteFxInstructionListPointers.AboveY380;
                }
                return;

            case PaletteFxPreInstructionCodes.SwitchAboveY380Second:
                if (samusY < 0x0380)
                {
                    slot.InstructionTimer = 1;
                    slot.InstructionPointer = PaletteFxInstructionListPointers.AboveY380Second;
                }
                return;

            case PaletteFxPreInstructionCodes.DeleteWhenAreaMiniBossDies:
                if (areaMiniBossDefeated)
                    slot.Clear();
                return;

            case PaletteFxPreInstructionCodes.Heat:
                throw new NotSupportedException(
                    "Room palette-FX heat pre-instruction $8D:E379 requires untranslated periodic-damage side effects.");

            case PaletteFxPreInstructionCodes.InspectAdjacentSlot:
                throw new NotSupportedException(
                    "Room palette-FX cross-slot pre-instruction $8D:F621 requires untranslated adjacent-WRAM inspection.");

            default:
                throw new NotSupportedException(
                    $"Room palette-FX pre-instruction $8D:{slot.PreInstruction:X4} is not translated " +
                    $"for object $8D:{slot.Id:X4} (Samus Y=${samusY:X4}, items=${equippedItems:X4}).");
        }
    }

    private void ExecuteProgram(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        PaletteFxSlot slot)
    {
        ushort cursor = slot.InstructionPointer;
        for (int commandGuard = 0; commandGuard < 256; commandGuard++)
        {
            ushort word = ReadBank8dWord(bus, cursor);
            if ((word & 0x8000) == 0)
            {
                slot.InstructionTimer = word;
                WritePaletteRecord(bus, cgram, slot, unchecked((ushort)(cursor + 2)));
                return;
            }

            switch (word)
            {
                case PaletteFxInstructionCodes.Delete:
                    slot.Clear();
                    return;

                case PaletteFxInstructionCodes.SetPreInstruction:
                    slot.PreInstruction = ReadBank8dWord(bus, unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;

                case PaletteFxInstructionCodes.ClearPreInstruction:
                    slot.PreInstruction = PaletteFxPreInstructionCodes.Cleared;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;

                case PaletteFxInstructionCodes.Goto:
                    cursor = ReadBank8dWord(bus, unchecked((ushort)(cursor + 2)));
                    break;

                case PaletteFxInstructionCodes.DecrementTimerAndGoto:
                    slot.Timer = unchecked((ushort)(slot.Timer - 1));
                    cursor = slot.Timer == 0
                        ? unchecked((ushort)(cursor + 4))
                        : ReadBank8dWord(bus, unchecked((ushort)(cursor + 2)));
                    break;

                case PaletteFxInstructionCodes.SetTimer:
                    // The assembly writes only the low byte addressed by the physical
                    // object index. The high byte was cleared at spawn and remains intact.
                    slot.Timer = (ushort)((slot.Timer & 0xff00) |
                        bus.ReadByte(
                            RoomFxRomData.Banks.PaletteFx |
                            unchecked((ushort)(cursor + 2))));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case PaletteFxInstructionCodes.SetColorIndex:
                    slot.ColorByteIndex = ReadBank8dWord(bus, unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;

                case PaletteFxInstructionCodes.QueueMusic:
                    musicRequests.Add(new PaletteFxMusicRequest(
                        MusicCommand.FromCartridge(ReadBank8dByte(
                            bus,
                            unchecked((ushort)(cursor + 2)))),
                        MusicCommandDelay.EightFrames));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case PaletteFxInstructionCodes.QueueSfx1:
                case PaletteFxInstructionCodes.QueueSfx2:
                case PaletteFxInstructionCodes.QueueSfx3:
                    // This case group has already excluded every other word. A final
                    // library-three fallback mirrors the third opcode without introducing
                    // an impossible/error branch into an otherwise exhaustive dispatcher.
                    SoundEffectLibrary library =
                        word == PaletteFxInstructionCodes.QueueSfx1
                            ? SoundEffectLibrary.Library1
                            : word == PaletteFxInstructionCodes.QueueSfx2
                                ? SoundEffectLibrary.Library2
                                : SoundEffectLibrary.Library3;
                    soundRequests.Add(new PaletteFxSoundRequest(
                        SoundEffectId.FromCartridge(
                            library,
                            ReadBank8dByte(bus, unchecked((ushort)(cursor + 2)))),
                        PaletteFxAudioQueueLimits.SoundEffects));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case PaletteFxInstructionCodes.SetPaletteFxIndex:
                    throw new NotSupportedException(
                        $"Room palette-FX object $8D:{slot.Id:X4} selected the heat-palette index at " +
                        $"$8D:{cursor:X4}; the shared heat owner is not translated.");

                default:
                    throw new InvalidDataException(
                        $"Unknown palette-FX command $8D:{word:X4} at $8D:{cursor:X4} " +
                        $"for object $8D:{slot.Id:X4}.");
            }
        }

        throw new InvalidDataException(
            $"Palette-FX object $8D:{slot.Id:X4} exceeded 256 leading commands at $8D:{cursor:X4}.");
    }

    private static void WritePaletteRecord(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        PaletteFxSlot slot,
        ushort cursor)
    {
        ushort colorByteIndex = slot.ColorByteIndex;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort word = ReadBank8dWord(bus, cursor);
            if ((word & 0x8000) == 0)
            {
                if ((colorByteIndex & 1) != 0 || colorByteIndex >= SnesCgram.ByteCount)
                {
                    throw new InvalidDataException(
                        $"Palette-FX object $8D:{slot.Id:X4} selected invalid CGRAM byte " +
                        $"index ${colorByteIndex:X4} at $8D:{cursor:X4}.");
                }
                cgram.SetColor(colorByteIndex / 2, word);
                colorByteIndex = unchecked((ushort)(colorByteIndex + 2));
                cursor = unchecked((ushort)(cursor + 2));
                continue;
            }

            switch (word)
            {
                case PaletteFxInstructionCodes.Wait:
                    // Native receives the cursor two bytes before this opcode and saves
                    // j+4. In direct terms that is simply the word following `$C595`.
                    slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                    return;
                case PaletteFxInstructionCodes.ColorPlus2:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 4));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstructionCodes.ColorPlus3:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 6));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstructionCodes.ColorPlus4:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 8));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstructionCodes.ColorPlus8:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 16));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstructionCodes.ColorPlus9:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 18));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstructionCodes.ColorPlus15:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 30));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                default:
                    throw new InvalidDataException(
                        $"Unsupported inline palette-FX command $8D:{word:X4} at " +
                        $"$8D:{cursor:X4} for object $8D:{slot.Id:X4}.");
            }
        }

        throw new InvalidDataException(
            $"Palette-FX object $8D:{slot.Id:X4} did not terminate its color record.");
    }

    private static ushort ReadBank8dWord(ISnesAddressSpace bus, ushort pointer) =>
        ReadWord(bus, RoomFxRomData.Banks.PaletteFx | pointer);

    private static byte ReadBank8dByte(ISnesAddressSpace bus, ushort pointer) =>
        bus.ReadByte(RoomFxRomData.Banks.PaletteFx | pointer);

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        RomDataReader.ReadWordFixedBank(bus, address);

    private sealed class PaletteFxSlot
    {
        public ushort Id { get; set; }
        public ushort ColorByteIndex { get; set; }
        public ushort PreInstruction { get; set; }
        public ushort InstructionPointer { get; set; }
        public ushort InstructionTimer { get; set; }
        public ushort Timer { get; set; }

        public void Clear()
        {
            Id = 0;
            ColorByteIndex = 0;
            PreInstruction = 0;
            InstructionPointer = 0;
            InstructionTimer = 0;
            Timer = 0;
        }
    }
}
