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
    /// <summary>Fixed number of native palette-FX objects indexed by their room-slot bit.</summary>
    private const int SlotCount = 8;

    /// <summary>Live native objects in descending-handler slot order.</summary>
    private readonly PaletteFxSlot[] slots = Enumerable.Range(0, SlotCount)
        .Select(_ => new PaletteFxSlot())
        .ToArray();
    /// <summary>Sound requests emitted by palette instructions during the current frame.</summary>
    private readonly List<PaletteFxSoundRequest> soundRequests = [];
    /// <summary>Current power-bomb state used to suppress incompatible palette sound requests; not serialized with the owner.</summary>
    [NonSerialized] private SamusPowerBombExplosionState? audioPowerBomb;
    /// <summary>Music requests emitted by palette instructions during the current frame.</summary>
    private readonly List<PaletteFxMusicRequest> musicRequests = [];
    /// <summary>Palette animation index published by the Norfair FX objects for Samus's heat palette.</summary>
    private ushort samusInHeatPaletteIndex;
    /// <summary>Last heat-palette index installed into this slot's instruction program.</summary>
    private ushort previousSamusInHeatPaletteIndex;

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
        bool areaMiniBossDefeated) =>
        LoadRoomCore(bus, fxPointer, doorPointer, area, equippedItems, areaMiniBossDefeated, null);

    /// <summary>Resets active objects and applies either a supplied FX record or the record selected from room pointers.</summary>
    /// <param name="bus">Address space used by spawned objects during later interpreter steps.</param>
    /// <param name="fxPointer">Room FX table pointer; zero means the room has no palette-FX record.</param>
    /// <param name="doorPointer">Door-specific selector used to resolve the matching room FX record.</param>
    /// <param name="area">Area whose per-bit palette-FX definitions are spawned.</param>
    /// <param name="equippedItems">Equipment state used by setup callbacks that choose a program.</param>
    /// <param name="areaMiniBossDefeated">Controls conditional Brinstar palette-object setup.</param>
    /// <param name="suppliedDefinition">Optional preselected room-FX record, bypassing pointer-based selection.</param>
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
            Spawn(bus, definition, equippedItems, areaMiniBossDefeated);
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
                bus,
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

            ExecuteProgram(bus, cgram, colors, slot);
        }
    }

    /// <summary>Allocates and initializes one palette-FX object, silently ignoring spawns when all eight slots are occupied.</summary>
    /// <param name="bus">Address space retained by the object interpreter's caller.</param>
    /// <param name="definition">Bank-$8D definition identifying setup and initial instruction behavior.</param>
    /// <param name="equippedItems">Equipment bits used by suit-dependent setup routines.</param>
    /// <param name="areaMiniBossDefeated">Whether conditional Brinstar setup should clear the new object.</param>
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

        RoomPaletteFxDefinition compiled = RoomPaletteFxDefinitions.Get(definition);
        slot.Id = definition;
        slot.ColorByteIndex = 0;
        slot.PreInstruction = PaletteFxPreInstructionCodes.Null;
        slot.InstructionPointer = compiled.InitialInstructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;

        ushort setup = compiled.SetupCallback;
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

    /// <summary>Applies the slot's native pre-instruction before its timed palette instruction is advanced.</summary>
    /// <param name="bus">Address space used by any pre-instruction data reads.</param>
    /// <param name="slot">Active palette object whose pre-instruction is dispatched.</param>
    /// <param name="slotIndex">Physical native slot index, used by adjacent-slot inspection.</param>
    /// <param name="samusY">Current Samus Y coordinate for vertical palette switches.</param>
    /// <param name="equippedItems">Current equipment for heat and setup behavior.</param>
    /// <param name="enemyZeroIsDead">Whether the enemy-linked object should be deleted.</param>
    /// <param name="areaMiniBossDefeated">Whether the conditional area object should be deleted.</param>
    /// <param name="samus">Optional active Samus state used by heat damage.</param>
    /// <param name="nmiFrameCounter">Frame counter used to gate periodic heat damage sound.</param>
    private void RunPreInstruction(
        ISnesAddressSpace bus,
        PaletteFxSlot slot,
        int slotIndex,
        ushort samusY,
        ushort equippedItems,
        bool enemyZeroIsDead,
        bool areaMiniBossDefeated,
        SamusState? samus,
        ushort nmiFrameCounter)
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
                if (samusY <
                    CrateriaLightningPaletteFxProgramMechanicsDefinitions.VerticalSwitchSamusY)
                {
                    slot.InstructionTimer = 1;
                    slot.InstructionPointer = PaletteFxInstructionListPointers.AboveY380;
                }
                return;

            case PaletteFxPreInstructionCodes.SwitchAboveY380Second:
                if (samusY <
                    CrateriaLightningPaletteFxProgramMechanicsDefinitions.VerticalSwitchSamusY)
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
                RunHeatPreInstruction(slot, equippedItems, samus, nmiFrameCounter);
                return;

            case PaletteFxPreInstructionCodes.InspectAdjacentSlot:
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
                throw new NotSupportedException(
                    $"Room palette-FX pre-instruction $8D:{slot.PreInstruction:X4} is not translated " +
                    $"for object $8D:{slot.Id:X4} (Samus Y=${samusY:X4}, items=${equippedItems:X4}).");
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

    /// <summary>Executes leading palette bytecode until it reaches a timed color record or deletes its object.</summary>
    /// <param name="bus">Address space used for mechanics words and bytecode operands.</param>
    /// <param name="cgram">CGRAM destination modified by any executed color record.</param>
    /// <param name="colors">Installed presentation source for palette-color words not owned by mechanics bytecode.</param>
    /// <param name="slot">Object whose cursor, timers, pre-instruction, and audio requests are updated.</param>
    private void ExecuteProgram(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        IPaletteFxColorSource colors,
        PaletteFxSlot slot)
    {
        ushort cursor = slot.InstructionPointer;
        for (int commandGuard = 0; commandGuard < 256; commandGuard++)
        {
            ushort word = ReadBank8dWord(bus, cursor);
            if ((word & 0x8000) == 0)
            {
                slot.InstructionTimer = word;
                WritePaletteRecord(bus, cgram, colors, slot, unchecked((ushort)(cursor + 2)));
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
                        ReadBank8dByte(bus, unchecked((ushort)(cursor + 2))));
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
                        PaletteFxAudioQueueLimits.SoundEffects,
                        SoundSuppressed: audioPowerBomb?.IsActive == true));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                case PaletteFxInstructionCodes.SetPaletteFxIndex:
                    // `$8D:F1C6` consumes one byte, despite living among word-sized
                    // commands. Advancing by three is essential: advancing by four would
                    // parse the high byte of the following duration as an opcode.
                    samusInHeatPaletteIndex = ReadBank8dByte(
                        bus,
                        unchecked((ushort)(cursor + 2)));
                    cursor = unchecked((ushort)(cursor + 3));
                    break;

                default:
                    throw new InvalidDataException(
                        $"Unknown palette-FX command $8D:{word:X4} at $8D:{cursor:X4} " +
                        $"for object $8D:{slot.Id:X4}.");
            }
        }

        throw new InvalidDataException(
            $"Palette-FX object $8D:{slot.Id:X4} exceeded 256 leading commands at $8D:{cursor:X4}.");
    }

    /// <summary>Writes a mixed color/skip record into CGRAM and saves the cursor following its wait command.</summary>
    /// <param name="bus">Address space retained for the interpreter's shared record contract.</param>
    /// <param name="cgram">CGRAM modified by each resolved color word.</param>
    /// <param name="colors">Installed source used when a record word is presentation color data.</param>
    /// <param name="slot">Object supplying the destination byte index and receiving its next instruction cursor.</param>
    /// <param name="cursor">Address of the first word in the color record.</param>
    private static void WritePaletteRecord(
        ISnesAddressSpace bus,
        SnesCgram cgram,
        IPaletteFxColorSource colors,
        PaletteFxSlot slot,
        ushort cursor)
    {
        ushort colorByteIndex = slot.ColorByteIndex;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort word = ReadPaletteRecordWord(cursor, colors);
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

    /// <summary>Resolves a mechanics-owned bank-$8D word without falling back to installed color data.</summary>
    /// <param name="bus">Address space required by the native read boundary.</param>
    /// <param name="pointer">Bank-$8D word address in the translated mechanics domain.</param>
    /// <returns>The compiled mechanics word.</returns>
    /// <exception cref="InvalidDataException">No compiled mechanics value exists at the address.</exception>
    private static ushort ReadBank8dWord(ISnesAddressSpace bus, ushort pointer)
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

    /// <summary>Reads a mixed color/wait record through its explicitly supplied presentation.</summary>
    private static ushort ReadPaletteRecordWord(ushort pointer, IPaletteFxColorSource colors)
    {
        if (RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(pointer, out ushort compiled))
            return compiled;
        if (colors.TryReadColor(pointer, out ushort color))
            return color;

        throw new InvalidDataException(
            $"Palette-FX word $8D:{pointer:X4} has no compiled mechanics or installed color definition.");
    }

    /// <summary>Resolves a mechanics-owned bank-$8D byte, including byte operands in mixed-width instructions.</summary>
    /// <param name="bus">Address space required by the native read boundary.</param>
    /// <param name="pointer">Bank-$8D byte address in the translated mechanics domain.</param>
    /// <returns>The compiled mechanics byte.</returns>
    /// <exception cref="InvalidDataException">No compiled mechanics value exists at the address.</exception>
    private static byte ReadBank8dByte(ISnesAddressSpace bus, ushort pointer)
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

    /// <summary>Mutable host representation of the six words in one native palette-FX object slot.</summary>
    private sealed class PaletteFxSlot
    {
        /// <summary>Definition pointer; zero marks this slot as free.</summary>
        public ushort Id { get; set; }
        /// <summary>Destination CGRAM byte index advanced by color-record skip commands.</summary>
        public ushort ColorByteIndex { get; set; }
        /// <summary>Bank-$8D pre-instruction dispatched before timed instruction processing.</summary>
        public ushort PreInstruction { get; set; }
        /// <summary>Current bank-$8D instruction or color-record cursor.</summary>
        public ushort InstructionPointer { get; set; }
        /// <summary>Countdown controlling when the current instruction list is processed.</summary>
        public ushort InstructionTimer { get; set; }
        /// <summary>Auxiliary countdown consumed by decrement-and-branch bytecode.</summary>
        public ushort Timer { get; set; }

        /// <summary>Returns the slot to its free state and clears all associated interpreter words.</summary>
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
