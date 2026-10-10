using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

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
    [NonSerialized] private SamusPowerBombExplosionState? audioPowerBomb;
    private readonly List<PaletteFxMusicRequest> musicRequests = [];
    private ushort samusInHeatPaletteIndex;
    private ushort previousSamusInHeatPaletteIndex;

    /// <summary>
    /// Bit 15 of <c>PaletteFXObject_Enable</c> ($1E79), checked first by the handler. Game
    /// data loading sets it; the escape timer's expiry (<c>$90:E106</c>) clears it.
    /// </summary>
    public bool HandlerEnabled { get; private set; } = true;

    /// <summary><c>Disable_PaletteFXObjects</c> ($8D:C4CD).</summary>
    public void DisableHandler() => HandlerEnabled = false;

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
        Spawn(definition, equippedItems, areaMiniBossDefeated);
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
        bool areaMiniBossDefeated) =>
        LoadRoomCore(bus, fxPointer, doorPointer, area, equippedItems, areaMiniBossDefeated, null);

    private void LoadRoomCore(ISnesAddressSpace bus, ushort fxPointer, ushort doorPointer,
        AreaId area, ushort equippedItems, bool areaMiniBossDefeated,
        RoomFxRecordDefinition? suppliedDefinition)
    {
        ArgumentNullException.ThrowIfNull(bus);
        foreach (PaletteFxSlot slot in slots)
            slot.Clear();
        soundRequests.Clear();
        musicRequests.Clear();

        if (fxPointer == 0)
            return;
        int areaIndex = AreaIds.ToIndex(area);

        ushort record = suppliedDefinition?.Pointer ?? RoomFxRecordDefinitions.Select(fxPointer, doorPointer);
        if (record == 0)
            return;

        // Palette FX and animated tiles have separate owners and bitsets.
        byte paletteFxBits = (suppliedDefinition ?? RoomFxRecordDefinitions.Get(record)).PaletteFxBitset;
        if (paletteFxBits == 0)
            return;

        for (int bit = 0; bit < SlotCount; bit++)
        {
            if ((paletteFxBits & (1 << bit)) == 0)
                continue;

            ushort definition = RoomPaletteFxDefinitions.GetAreaDefinition(areaIndex, bit);
            Spawn(definition, equippedItems, areaMiniBossDefeated);
        }
    }

    /// <summary>Runs one native <c>PaletteFXObject_Handler</c> pass in descending slot order.</summary>
    /// <remarks>
    /// Colors are a required execution dependency, not an optional bind operation.
    /// Every caller, including a restored owner, must supply its current installed
    /// presentation on every step. Construction creates only serializable program
    /// state; it cannot hide an unbound color source until a later instruction.
    /// </remarks>
    public void Step(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        IPaletteFxColorSource colors,
        ushort samusY,
        ushort equippedItems,
        bool enemyZeroIsDead,
        bool areaMiniBossDefeated,
        SamusState? samus = null,
        ushort nmiFrameCounter = 0,
        SamusPowerBombExplosionState? powerBomb = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);
        ArgumentNullException.ThrowIfNull(colors);
        audioPowerBomb = powerBomb;

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
                slotIndex,
                samusY,
                equippedItems,
                enemyZeroIsDead,
                areaMiniBossDefeated,
                samus,
                nmiFrameCounter);
            if (slot.Id == 0)
                continue;

            // `$8D:C552` decrements before comparing with one. Unsigned underflow is
            // intentional cartridge behavior for malformed or externally edited state.
            slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
            if (slot.InstructionTimer != 0)
                continue;

            ExecuteProgram(cgram, colors, slot);
        }
    }

    private void Spawn(
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

        RoomPaletteFxDefinition compiled = RoomPaletteFxDefinitions.Get(definition);
        slot.Id = definition;
        slot.ColorByteIndex = 0;
        slot.PreInstruction = (ushort)PaletteFxPreInstruction.Null;
        slot.InstructionPointer = compiled.InitialInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        PaletteFxSetup setup = compiled.SetupCallback;
        switch (setup)
        {
            case PaletteFxSetup.Null:
                return;

            case PaletteFxSetup.Intro:
                slot.PreInstruction = (ushort)PaletteFxPreInstruction.Intro;
                return;

            case PaletteFxSetup.Norfair:
                // `$8D:E440` chooses the complete program from the same live suit bits
                // used by Samus. Gravity has priority when both bits are present.
                slot.InstructionPointer = equippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                    ? PaletteFxInstructionListPointers.NorfairGravitySuit
                    : equippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                        ? PaletteFxInstructionListPointers.NorfairVariaSuit
                        : PaletteFxInstructionListPointers.NorfairPowerSuit;
                return;

            case PaletteFxSetup.Brinstar:
                if (areaMiniBossDefeated)
                    slot.Clear();
                return;

            default:
                throw new InvalidOperationException(
                    $"Palette-FX object $8D:{definition:X4} has undefined setup {setup}.");
        }
    }

    private void RunPreInstruction(
        PaletteFxSlot slot,
        int slotIndex,
        ushort samusY,
        ushort equippedItems,
        bool enemyZeroIsDead,
        bool areaMiniBossDefeated,
        SamusState? samus,
        ushort nmiFrameCounter)
    {
        var preInstruction = (PaletteFxPreInstruction)slot.PreInstruction;
        if (!Enum.IsDefined(preInstruction))
        {
            throw new NotSupportedException(
                $"Room palette-FX pre-instruction $8D:{slot.PreInstruction:X4} is not translated " +
                $"for object $8D:{slot.Id:X4} (Samus Y=${samusY:X4}, items=${equippedItems:X4}).");
        }
        switch (preInstruction)
        {
            case PaletteFxPreInstruction.Null:
            case PaletteFxPreInstruction.Cleared:
            case PaletteFxPreInstruction.Intro:
                return;

            case PaletteFxPreInstruction.DeleteWhenEnemyZeroDies:
                if (enemyZeroIsDead)
                    slot.Clear();
                return;

            case PaletteFxPreInstruction.SwitchAboveY380:
                if (samusY <
                    CrateriaLightningPaletteFxProgramMechanicsDefinitions.VerticalSwitchSamusY)
                {
                    slot.InstructionTimer = 1;
                    slot.InstructionPointer = PaletteFxInstructionListPointers.AboveY380;
                }
                return;

            case PaletteFxPreInstruction.SwitchAboveY380Second:
                if (samusY <
                    CrateriaLightningPaletteFxProgramMechanicsDefinitions.VerticalSwitchSamusY)
                {
                    slot.InstructionTimer = 1;
                    slot.InstructionPointer = PaletteFxInstructionListPointers.AboveY380Second;
                }
                return;

            case PaletteFxPreInstruction.DeleteWhenAreaMiniBossDies:
                if (areaMiniBossDefeated)
                    slot.Clear();
                return;

            case PaletteFxPreInstruction.Heat:
                RunHeatPreInstruction(slot, equippedItems, samus, nmiFrameCounter);
                return;

            case PaletteFxPreInstruction.InspectAdjacentSlot:
                // X is the native byte offset, not a count of objects spawned. F621
                // deliberately indexes from Enable rather than IDs. Use the live owner
                // arrays so deletions earlier in this descending pass are visible.
                ushort adjacentWord = slotIndex switch
                {
                    0 => PaletteFxMemoryLayout.HandlerEnabled,
                    PaletteFxMemoryLayout.CurrentIndexWordOffset => (ushort)(slotIndex * sizeof(ushort)),
                    _ => slots[slotIndex - PaletteFxMemoryLayout.IdArrayWordOffset].Id,
                };
                if (adjacentWord != 0) slot.Clear();
                return;

            default:
                throw new InvalidOperationException($"Undefined palette-FX pre-instruction {preInstruction}.");
        }
    }

    /// <summary>
    /// Ports <c>PreInstruction_PaletteFXObject_SamusInHeat</c> at $8D:E379. The palette
    /// object deliberately owns both presentation and damage: an unprotected Samus gains
    /// one quarter-unit of 16.16 damage per frame, while a second Norfair object publishes
    /// the animation index consumed here on the following descending-slot pass.
    /// </summary>
    private void RunHeatPreInstruction(
        PaletteFxSlot slot,
        ushort equippedItems,
        SamusState? samus,
        ushort nmiFrameCounter)
    {
        bool protectedFromHeat = equippedItems.HasAny(
            SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit);
        if (!protectedFromHeat && samus is not null)
        {
            samus.LiquidPhysics.AccumulatePeriodicDamage(
                PaletteFxHeatData.SubdamagePerFrame,
                wholeDamage: 0);
            if ((nmiFrameCounter & 7) == 0 &&
                samus.Health > PaletteFxHeatData.DamageSoundEnergyThreshold)
            {
                soundRequests.Add(new PaletteFxSoundRequest(
                    SoundEffectLibrary3Sounds.EnvironmentalDamage,
                    PaletteFxHeatData.DamageSoundMaximumQueued,
                    SoundSuppressed: audioPowerBomb?.IsActive == true));
            }
        }

        if (samusInHeatPaletteIndex == previousSamusInHeatPaletteIndex)
            return;

        previousSamusInHeatPaletteIndex = samusInHeatPaletteIndex;
        slot.InstructionTimer = 1;
        slot.InstructionPointer = PaletteFxHeatInstructionListDefinitions.ResolveForEquippedItems(
            equippedItems,
            samusInHeatPaletteIndex);
    }

    private void ExecuteProgram(
        SnesCgram cgram,
        IPaletteFxColorSource colors,
        PaletteFxSlot slot)
    {
        ushort cursor = slot.InstructionPointer;
        for (int commandGuard = 0; commandGuard < 256; commandGuard++)
        {
            ushort word = ReadBank8dWord(cursor);
            if ((word & 0x8000) == 0)
            {
                slot.InstructionTimer = word;
                WritePaletteRecord(cgram, colors, slot, unchecked((ushort)(cursor + 2)));
                return;
            }

            var instruction = (PaletteFxInstruction)word;
            if (!Enum.IsDefined(instruction))
            {
                throw new InvalidDataException(
                    $"Unknown palette-FX command $8D:{word:X4} at $8D:{cursor:X4} " +
                    $"for object $8D:{slot.Id:X4}.");
            }
            switch (instruction)
            {
                case PaletteFxInstruction.Delete:
                    slot.Clear();
                    return;

                case PaletteFxInstruction.SetPreInstruction:
                    slot.PreInstruction = ReadBank8dWord(unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;

                case PaletteFxInstruction.ClearPreInstruction:
                    slot.PreInstruction = (ushort)PaletteFxPreInstruction.Cleared;
                    cursor = unchecked((ushort)(cursor + 2));
                    break;

                case PaletteFxInstruction.Goto:
                    cursor = ReadBank8dWord(unchecked((ushort)(cursor + 2)));
                    break;

                case PaletteFxInstruction.DecrementTimerAndGoto:
                    slot.Timer = unchecked((ushort)(slot.Timer - 1));
                    cursor = slot.Timer == 0
                        ? unchecked((ushort)(cursor + 4))
                        : ReadBank8dWord(unchecked((ushort)(cursor + 2)));
                    break;

                case PaletteFxInstruction.SetTimer:
                    // The assembly writes only the low byte addressed by the physical
                    // object index. The high byte was cleared at spawn and remains intact.
                    slot.Timer = (ushort)((slot.Timer & 0xff00) |
                        ReadBank8dByte(unchecked((ushort)(cursor + 2))));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case PaletteFxInstruction.SetColorIndex:
                    slot.ColorByteIndex = ReadBank8dWord(unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 4));
                    break;

                case PaletteFxInstruction.QueueMusic:
                    musicRequests.Add(new PaletteFxMusicRequest(
                        MusicCommand.FromCartridge(ReadBank8dByte(
                            unchecked((ushort)(cursor + 2)))),
                        MusicCommandDelay.EightFrames));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case PaletteFxInstruction.QueueSfx1:
                case PaletteFxInstruction.QueueSfx2:
                case PaletteFxInstruction.QueueSfx3:
                    // This case group has already excluded every other word. A final
                    // library-three fallback mirrors the third opcode without introducing
                    // an impossible/error branch into an otherwise exhaustive dispatcher.
                    SoundEffectLibrary library =
                        instruction == PaletteFxInstruction.QueueSfx1
                            ? SoundEffectLibrary.Library1
                            : instruction == PaletteFxInstruction.QueueSfx2
                                ? SoundEffectLibrary.Library2
                                : SoundEffectLibrary.Library3;
                    soundRequests.Add(new PaletteFxSoundRequest(
                        SoundEffectId.FromCartridge(
                            library,
                            ReadBank8dByte(unchecked((ushort)(cursor + 2)))),
                        PaletteFxAudioQueueLimits.SoundEffects,
                        SoundSuppressed: audioPowerBomb?.IsActive == true));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case PaletteFxInstruction.SetPaletteFxIndex:
                    // `$8D:F1C6` consumes one byte, despite living among word-sized
                    // commands. Advancing by three is essential: advancing by four would
                    // parse the high byte of the following duration as an opcode.
                    samusInHeatPaletteIndex = ReadBank8dByte(
                        unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case PaletteFxInstruction.Wait:
                case PaletteFxInstruction.ColorPlus2:
                case PaletteFxInstruction.ColorPlus3:
                case PaletteFxInstruction.ColorPlus4:
                case PaletteFxInstruction.ColorPlus8:
                case PaletteFxInstruction.ColorPlus9:
                case PaletteFxInstruction.ColorPlus15:
                    throw new InvalidDataException(
                        $"Inline palette-FX command {instruction} leads a record at $8D:{cursor:X4} " +
                        $"for object $8D:{slot.Id:X4}.");

                default:
                    throw new InvalidOperationException($"Undefined palette-FX command {instruction}.");
            }
        }

        throw new InvalidDataException(
            $"Palette-FX object $8D:{slot.Id:X4} exceeded 256 leading commands at $8D:{cursor:X4}.");
    }

    private static void WritePaletteRecord(
        SnesCgram cgram,
        IPaletteFxColorSource colors,
        PaletteFxSlot slot,
        ushort cursor)
    {
        ushort colorByteIndex = slot.ColorByteIndex;
        for (int guard = 0; guard < 256; guard++)
        {
            PaletteRecordEntry entry = ReadPaletteRecordEntry(cursor, colors);
            if (entry.Color is Bgr555 color)
            {
                if ((colorByteIndex & 1) != 0 || colorByteIndex >= SnesCgram.ByteCount)
                {
                    throw new InvalidDataException(
                        $"Palette-FX object $8D:{slot.Id:X4} selected invalid CGRAM byte " +
                        $"index ${colorByteIndex:X4} at $8D:{cursor:X4}.");
                }
                cgram.SetColor(colorByteIndex / 2, color);
                colorByteIndex = unchecked((ushort)(colorByteIndex + 2));
                cursor = unchecked((ushort)(cursor + 2));
                continue;
            }

            ushort command = entry.Command;
            var inline = (PaletteFxInstruction)command;
            if (!Enum.IsDefined(inline))
            {
                throw new InvalidDataException(
                    $"Unsupported inline palette-FX command $8D:{command:X4} at " +
                    $"$8D:{cursor:X4} for object $8D:{slot.Id:X4}.");
            }
            switch (inline)
            {
                case PaletteFxInstruction.Wait:
                    // Native receives the cursor two bytes before this opcode and saves
                    // j+4. In direct terms that is simply the word following `$C595`.
                    slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                    return;
                case PaletteFxInstruction.ColorPlus2:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 4));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstruction.ColorPlus3:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 6));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstruction.ColorPlus4:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 8));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstruction.ColorPlus8:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 16));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstruction.ColorPlus9:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 18));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstruction.ColorPlus15:
                    colorByteIndex = unchecked((ushort)(colorByteIndex + 30));
                    cursor = unchecked((ushort)(cursor + 2));
                    break;
                case PaletteFxInstruction.Delete:
                case PaletteFxInstruction.SetPreInstruction:
                case PaletteFxInstruction.ClearPreInstruction:
                case PaletteFxInstruction.Goto:
                case PaletteFxInstruction.DecrementTimerAndGoto:
                case PaletteFxInstruction.SetTimer:
                case PaletteFxInstruction.SetColorIndex:
                case PaletteFxInstruction.QueueMusic:
                case PaletteFxInstruction.QueueSfx1:
                case PaletteFxInstruction.QueueSfx2:
                case PaletteFxInstruction.QueueSfx3:
                case PaletteFxInstruction.SetPaletteFxIndex:
                    throw new InvalidDataException(
                        $"Unsupported inline palette-FX command {inline} at " +
                        $"$8D:{cursor:X4} for object $8D:{slot.Id:X4}.");
                default:
                    throw new InvalidOperationException($"Undefined palette-FX command {inline}.");
            }
        }

        throw new InvalidDataException(
            $"Palette-FX object $8D:{slot.Id:X4} did not terminate its color record.");
    }

    private static ushort ReadBank8dWord(ushort pointer)
    {
        if (RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                pointer,
                out ushort compiled))
        {
            return compiled;
        }

        throw new InvalidDataException(
            $"Palette-FX mechanics word $8D:{pointer:X4} has no compiled definition.");
    }

    /// <summary>One word of a mixed color/command record: a color, or an inline command at $8000 and above.</summary>
    private readonly record struct PaletteRecordEntry(Bgr555? Color, ushort Command);

    /// <summary>Reads a mixed color/wait record through its explicitly supplied presentation.</summary>
    private static PaletteRecordEntry ReadPaletteRecordEntry(ushort pointer, IPaletteFxColorSource colors)
    {
        // $8D:C4E1 treats a record word below $8000 as a color and any other word as a command.
        if (RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort compiled))
            return (compiled & 0x8000) == 0 ? new(Bgr555.FromWord(compiled), 0) : new(null, compiled);
        if (colors.TryReadColor(pointer, out Bgr555 color))
            return new(color, 0);

        throw new InvalidDataException(
            $"Palette-FX word $8D:{pointer:X4} has no compiled mechanics or installed color definition.");
    }

    private static byte ReadBank8dByte(ushort pointer)
    {
        if (RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte(
                pointer,
                out byte compiled))
        {
            return compiled;
        }

        throw new InvalidDataException(
            $"Palette-FX byte $8D:{pointer:X4} has no compiled mechanics definition.");
    }

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
