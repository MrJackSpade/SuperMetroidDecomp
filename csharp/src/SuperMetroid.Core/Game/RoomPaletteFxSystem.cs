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
    private const int FxBank = 0x830000;
    private const int PaletteFxBank = 0x8d0000;
    private const int AreaPaletteFxPointerTable = 0x83ac46;
    private const int SlotCount = 8;
    private const int FxRecordByteCount = 16;

    // Bank-$8D instruction words. These are executable pointers in the retail stream, not
    // an invented host opcode enum; retaining their native values makes debugger state line
    // up with disassembly and causes an unsupported program to identify itself precisely.
    private const ushort WaitInstruction = 0xc595;
    private const ushort ColorPlus2Instruction = 0xc599;
    private const ushort ColorPlus3Instruction = 0xc5a2;
    private const ushort ColorPlus4Instruction = 0xc5ab;
    private const ushort ColorPlus8Instruction = 0xc5b4;
    private const ushort ColorPlus9Instruction = 0xc5bd;
    private const ushort ColorPlus15Instruction = 0xc5c6;
    private const ushort DeleteInstruction = 0xc5cf;
    private const ushort SetPreInstruction = 0xc5d4;
    private const ushort ClearPreInstruction = 0xc5dd;
    private const ushort GotoInstruction = 0xc61e;
    private const ushort DecrementTimerAndGotoInstruction = 0xc639;
    private const ushort SetTimerInstruction = 0xc648;
    private const ushort SetColorIndexInstruction = 0xc655;
    private const ushort QueueMusicInstruction = 0xc65e;
    private const ushort QueueSfx1Instruction = 0xc66a;
    private const ushort QueueSfx2Instruction = 0xc673;
    private const ushort QueueSfx3Instruction = 0xc67c;
    private const ushort SetPaletteFxIndexInstruction = 0xf1c6;

    private const ushort NullSetup = 0xc685;
    private const ushort IntroSetup = 0xe204;
    private const ushort NorfairSetup = 0xe440;
    private const ushort BrinstarSetup = 0xf730;

    private const ushort NullPreInstruction = 0xc526;
    private const ushort ClearedPreInstruction = 0xc5e3;
    private const ushort IntroPreInstruction = 0xe20b;
    private const ushort EnemyZeroHealthPreInstruction = 0xe2e0;
    private const ushort HeatPreInstruction = 0xe379;
    private const ushort SwitchAboveY380PreInstruction = 0xec59;
    private const ushort SwitchAboveY380SecondPreInstruction = 0xed84;
    private const ushort MiniBossPreInstruction = 0xeec5;
    private const ushort CrossSlotPreInstruction = 0xf621;

    private readonly PaletteFxSlot[] slots = Enumerable.Range(0, SlotCount)
        .Select(_ => new PaletteFxSlot())
        .ToArray();

    /// <summary>Number of bank-$8D objects currently occupying native slots.</summary>
    public int ActiveCount => slots.Count(slot => slot.Id != 0);

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

        if (fxPointer == 0)
            return;
        int areaIndex = AreaIds.ToIndex(area);

        ushort record = SelectFxRecord(bus, fxPointer, doorPointer);
        if (record == 0)
            return;

        // FxDef offsets $0D/$0E are independent palette-FX and animtile bitsets. Reading
        // only the former is deliberate; a separate bank-$87 owner must consume the latter.
        byte paletteFxBits = bus.ReadByte(FxBank | unchecked((ushort)(record + 13)));
        if (paletteFxBits == 0)
            return;

        ushort areaList = ReadWord(bus, AreaPaletteFxPointerTable + areaIndex * 2);
        for (int bit = 0; bit < SlotCount; bit++)
        {
            if ((paletteFxBits & (1 << bit)) == 0)
                continue;

            ushort definition = ReadWord(bus, FxBank | unchecked((ushort)(areaList + bit * 2)));
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
        slot.PreInstruction = NullPreInstruction;
        slot.InstructionPointer = ReadBank8dWord(bus, unchecked((ushort)(definition + 2)));
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        ushort setup = ReadBank8dWord(bus, definition);
        switch (setup)
        {
            case NullSetup:
                return;

            case IntroSetup:
                slot.PreInstruction = IntroPreInstruction;
                return;

            case NorfairSetup:
                // `$8D:E440` chooses the complete program from the same live suit bits
                // used by Samus. Gravity has priority when both bits are present.
                slot.InstructionPointer = equippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                    ? (ushort)0xe8b6
                    : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                        ? (ushort)0xe68a
                        : (ushort)0xe45e;
                return;

            case BrinstarSetup:
                if (areaMiniBossDefeated)
                    slot.Clear();
                return;

            default:
                throw new NotSupportedException(
                    $"Palette-FX object $8D:{definition:X4} setup $8D:{setup:X4} is not translated.");
        }
    }

    private static ushort SelectFxRecord(
        ISnesAddressSpace bus,
        ushort fxPointer,
        ushort doorPointer)
    {
        ushort record = fxPointer;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort candidateDoor = ReadWord(bus, FxBank | record);
            if (candidateDoor == 0 || candidateDoor == doorPointer)
                return record;
            if (candidateDoor == ushort.MaxValue)
                return 0;
            record = unchecked((ushort)(record + FxRecordByteCount));
        }

        throw new InvalidDataException(
            $"Room FX list $83:{fxPointer:X4} did not terminate while selecting door $83:{doorPointer:X4}.");
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
            case NullPreInstruction:
            case ClearedPreInstruction:
            case IntroPreInstruction:
                return;

            case EnemyZeroHealthPreInstruction:
                if (enemyZeroIsDead)
                    slot.Clear();
                return;

            case SwitchAboveY380PreInstruction:
                if (samusY < 0x0380)
                {
                    slot.InstructionTimer = 1;
                    slot.InstructionPointer = 0xeb43;
                }
                return;

            case SwitchAboveY380SecondPreInstruction:
                if (samusY < 0x0380)
                {
                    slot.InstructionTimer = 1;
                    slot.InstructionPointer = 0xec76;
                }
                return;

            case MiniBossPreInstruction:
                if (areaMiniBossDefeated)
                    slot.Clear();
                return;

            case HeatPreInstruction:
                throw new NotSupportedException(
                    "Room palette-FX heat pre-instruction $8D:E379 requires untranslated periodic-damage side effects.");

            case CrossSlotPreInstruction:
                throw new NotSupportedException(
                    "Room palette-FX cross-slot pre-instruction $8D:F621 requires untranslated adjacent-WRAM inspection.");

            default:
                throw new NotSupportedException(
                    $"Room palette-FX pre-instruction $8D:{slot.PreInstruction:X4} is not translated " +
                    $"for object $8D:{slot.Id:X4} (Samus Y=${samusY:X4}, items=${equippedItems:X4}).");
        }
    }

    private static void ExecuteProgram(
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
                case DeleteInstruction:
                    slot.Clear();
                    return;

                case SetPreInstruction:
                    slot.PreInstruction = ReadBank8dWord(bus, unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;

                case ClearPreInstruction:
                    slot.PreInstruction = ClearedPreInstruction;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;

                case GotoInstruction:
                    cursor = ReadBank8dWord(bus, unchecked((ushort)(cursor + 2)));
                    break;

                case DecrementTimerAndGotoInstruction:
                    slot.Timer = unchecked((ushort)(slot.Timer - 1));
                    cursor = slot.Timer == 0
                        ? unchecked((ushort)(cursor + 4))
                        : ReadBank8dWord(bus, unchecked((ushort)(cursor + 2)));
                    break;

                case SetTimerInstruction:
                    // The assembly writes only the low byte addressed by the physical
                    // object index. The high byte was cleared at spawn and remains intact.
                    slot.Timer = (ushort)((slot.Timer & 0xff00) |
                        bus.ReadByte(PaletteFxBank | unchecked((ushort)(cursor + 2))));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case SetColorIndexInstruction:
                    slot.ColorByteIndex = ReadBank8dWord(bus, unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;

                case QueueMusicInstruction:
                case QueueSfx1Instruction:
                case QueueSfx2Instruction:
                case QueueSfx3Instruction:
                    throw new NotSupportedException(
                        $"Room palette-FX object $8D:{slot.Id:X4} requested audio command " +
                        $"$8D:{word:X4} at $8D:{cursor:X4}; its queue handoff is not translated.");

                case SetPaletteFxIndexInstruction:
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
                case WaitInstruction:
                    // Native receives the cursor two bytes before this opcode and saves
                    // j+4. In direct terms that is simply the word following `$C595`.
                    slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                    return;
                case ColorPlus2Instruction:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 4));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case ColorPlus3Instruction:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 6));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case ColorPlus4Instruction:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 8));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case ColorPlus8Instruction:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 16));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case ColorPlus9Instruction:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 18));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case ColorPlus15Instruction:
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
        ReadWord(bus, PaletteFxBank | pointer);

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
