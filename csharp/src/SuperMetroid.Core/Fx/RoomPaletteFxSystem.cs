using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

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
    /// <summary>Native bank-$8D allocator capacity and descending handler range.</summary>
    private const int SlotCount = 8;

    /// <summary>Persistent interpreter state for the eight palette-FX objects.</summary>
    internal readonly PaletteFxSlot[] slots = Enumerable.Range(0, SlotCount)
        .Select(_ => new PaletteFxSlot())
        .ToArray();
    /// <summary>Momentary sound publications cleared at the start of each handler pass.</summary>
    private readonly List<PaletteFxSoundRequest> soundRequests = [];
    /// <summary>Live Power Bomb state used to suppress palette-FX sound requests.</summary>
    [NonSerialized] private SamusPowerBombExplosionState? audioPowerBomb;
    /// <summary>Momentary music publications cleared at the start of each handler pass.</summary>
    private readonly List<PaletteFxMusicRequest> musicRequests = [];
    /// <summary>
    /// Heat palette phase published by the Norfair object for the next descending-slot pass. Only compared for
    /// equality and used to select a heat program, so it is held as a plain phase number.
    /// </summary>
    internal int samusInHeatPaletteIndex;
    /// <summary>Last heat palette phase applied to Samus; prevents redundant program restarts.</summary>
    internal int previousSamusInHeatPaletteIndex;

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
        ushort definition,
        SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated = false)
    {
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
        RoomFxDefinition? fx,
        CartridgeDoorHeader? door,
        AreaId area,
        SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated) =>
        LoadRoomCore(fx, door, area, equippedItems, areaMiniBossDefeated, null);

    /// <summary>Clears the old room's slots and spawns the selected FX record's area-specific palette bits.</summary>
    internal void LoadRoomCore(RoomFxDefinition? fx, CartridgeDoorHeader? door,
        AreaId area, SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated,
        RoomFxRecordDefinition? suppliedDefinition)
    {
        foreach (PaletteFxSlot slot in slots)
            slot.Clear();
        soundRequests.Clear();
        musicRequests.Clear();

        if (suppliedDefinition is null && fx is null)
            return;
        int areaIndex = AreaIds.ToIndex(area);

        if ((suppliedDefinition ?? fx?.Select(door)) is not { } record)
            return;

        // Palette FX and animated tiles have separate owners and bitsets.
        byte paletteFxBits = record.PaletteFxBitset;
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
    public void Step(SnesCgram cgram, IPaletteFxColorSource colors, PaletteFxConditions conditions,
        SamusPowerBombExplosionState? powerBomb = null)
    {
        Ensure.NotNull(cgram);
        Ensure.NotNull(colors);
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

            RunPreInstruction(slot, slotIndex, conditions);
            if (slot.Id == 0)
                continue;

            // `$8D:C552` decrements before comparing with one. Unsigned underflow is
            // intentional cartridge behavior for malformed or externally edited state.
            slot.InstructionTimer = slot.InstructionTimer.Decrement().Remaining;
            if (!slot.InstructionTimer.IsZero)
                continue;

            ExecuteProgram(cgram, colors, slot);
        }
    }

    /// <summary>Allocates the highest free native slot and applies definition-specific setup for Intro, Norfair, or Brinstar.</summary>
    private void Spawn(
        ushort definition,
        SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems,
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
        slot.ColorIndex = 0;
        slot.PreInstruction = PaletteFxPreInstruction.Null;
        slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(compiled.InitialProgram);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        PaletteFxSetup setup = compiled.SetupCallback;
        switch (setup)
        {
            case PaletteFxSetup.Null:
                return;

            case PaletteFxSetup.Intro:
                slot.PreInstruction = PaletteFxPreInstruction.Intro;
                return;

            case PaletteFxSetup.Norfair:
                // `$8D:E440` chooses the complete program from the same live suit bits
                // used by Samus. Gravity has priority when both bits are present.
                slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(
                    PaletteFxHeatPrograms.Start(PaletteFxHeatPrograms.SuitFor(equippedItems)));
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

    /// <summary>Runs the slot's condition handler before timer and bytecode execution, including room heat and cutscene gates.</summary>
    private void RunPreInstruction(PaletteFxSlot slot, int slotIndex, PaletteFxConditions conditions)
    {
        if (slot.PreInstruction is not { } preInstruction || !Enum.IsDefined(preInstruction))
        {
            throw new NotSupportedException(
                $"Room palette-FX pre-instruction {slot.PreInstruction} is not translated " +
                $"for object $8D:{slot.Id:X4} (Samus Y=${conditions.SamusY:X4}, items=${conditions.EquippedItems.ToNativeWord():X4}).");
        }
        switch (preInstruction)
        {
            case PaletteFxPreInstruction.Null:
            case PaletteFxPreInstruction.Cleared:
            case PaletteFxPreInstruction.Intro:
                return;

            case PaletteFxPreInstruction.DeleteWhenEnemyZeroDies:
                if (conditions.EnemyZeroIsDead)
                    slot.Clear();
                return;

            case PaletteFxPreInstruction.SwitchAboveY380:
                if (conditions.SamusY <
                    CrateriaLightningPaletteFxColorDefinitions.VerticalSwitchSamusY)
                {
                    slot.InstructionTimer = NativeCountdown.Start(1);
                    slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxProgram.Crateria11);
                }
                return;

            case PaletteFxPreInstruction.SwitchAboveY380Second:
                if (conditions.SamusY <
                    CrateriaLightningPaletteFxColorDefinitions.VerticalSwitchSamusY)
                {
                    slot.InstructionTimer = NativeCountdown.Start(1);
                    slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxProgram.CrateriaAboveY380Second);
                }
                return;

            case PaletteFxPreInstruction.DeleteWhenAreaMiniBossDies:
                if (conditions.AreaMiniBossDefeated)
                    slot.Clear();
                return;

            case PaletteFxPreInstruction.Heat:
                RunHeatPreInstruction(slot, conditions.EquippedItems, conditions.Samus, conditions.NmiFrameCounter);
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
        SuperMetroid.Core.Samus.SamusEquipmentFlags equippedItems,
        SamusState? samus,
        int nmiFrameCounter)
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
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxHeatPrograms.Phase(
            PaletteFxHeatPrograms.SuitFor(equippedItems),
            samusInHeatPaletteIndex));
    }

    /// <summary>Runs the slot's typed program until it shows its next timed colour record or deletes the object.</summary>
    private void ExecuteProgram(
        SnesCgram cgram,
        IPaletteFxColorSource colors,
        PaletteFxSlot slot)
    {
        AnimationProgramCursor<PaletteFxProgram> cursor = slot.Cursor;
        NativeCountdown timer = slot.Timer;
        AnimationProgramAdvance<PaletteFxFrame> advance = PaletteFxAnimations.Programs.Advance(ref cursor, ref timer,
            (cue, _) =>
            {
                Publish(slot, cue);
                return null;
            });
        slot.Cursor = cursor;
        slot.Timer = timer;
        switch (advance.Kind)
        {
            case AnimationProgramAdvanceKind.Show:
                slot.InstructionTimer = NativeCountdown.FromRaw((ushort)advance.Duration);
                WritePaletteRecord(cgram, colors, slot, advance.Frame);
                return;
            case AnimationProgramAdvanceKind.Stop:
                slot.Clear();
                return;
            case AnimationProgramAdvanceKind.Wait:
            case AnimationProgramAdvanceKind.Hold:
                throw new InvalidOperationException($"Palette-FX program {cursor.Program} reached a {advance.Kind}, which no palette program has.");
            default:
                throw new InvalidOperationException($"Undefined {nameof(AnimationProgramAdvanceKind)} {advance.Kind}.");
        }
    }

    /// <summary>
    /// The CGRAM colour a native SetColorIndex byte index selects; an odd byte index names no colour. The typed
    /// programs already carry colour indices; this decodes a byte index read from a native snapshot.
    /// </summary>
    internal static int PaletteFxColorIndex(ushort byteIndex, ushort objectId) => (byteIndex & 1) == 0
        ? byteIndex / sizeof(ushort)
        : throw new InvalidDataException($"Palette-FX object $8D:{objectId:X4} selected odd CGRAM byte index ${byteIndex:X4}.");

    /// <summary>Carries out one palette-FX program command.</summary>
    private void Publish(PaletteFxSlot slot, PaletteFxCue cue)
    {
        switch (cue.Kind)
        {
            case PaletteFxCueKind.SetColorIndex:
                slot.ColorIndex = cue.ColorIndex;
                return;
            case PaletteFxCueKind.SetPreInstruction:
                slot.PreInstruction = cue.PreInstruction;
                return;
            case PaletteFxCueKind.ClearPreInstruction:
                slot.PreInstruction = PaletteFxPreInstruction.Cleared;
                return;
            case PaletteFxCueKind.SetHeatPaletteIndex:
                samusInHeatPaletteIndex = cue.HeatPhase;
                return;
            case PaletteFxCueKind.QueueSoundLibrary2:
                soundRequests.Add(new PaletteFxSoundRequest(
                    cue.Sound ?? throw new InvalidDataException(
                        $"Palette-FX object $8D:{slot.Id:X4} queued a library-2 sound cue without a sound."),
                    PaletteFxAudioQueueLimits.SoundEffects,
                    SoundSuppressed: audioPowerBomb?.IsActive == true));
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PaletteFxCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Writes one timed colour record to CGRAM from the slot's colour index, skipping the entries its layout skips.</summary>
    [PpuLayoutBoundary("codec: palette-FX instruction colour-index operands ($8D SetColorIndex)")]
    private static void WritePaletteRecord(
        SnesCgram cgram,
        IPaletteFxColorSource colors,
        PaletteFxSlot slot,
        PaletteFxFrame frame)
    {
        int colorIndex = slot.ColorIndex;
        // The record's colour words follow its timer word; each skip command occupies one word too.
        ushort colorWord = unchecked((ushort)(PaletteFxRecordDefinitions.Address(frame) + sizeof(ushort)));
        foreach (PaletteFxRecordPart part in PaletteFxRecordDefinitions.Layout(frame))
        {
            if (part.Skip)
            {
                colorIndex += part.Count;
                colorWord = unchecked((ushort)(colorWord + sizeof(ushort)));
                continue;
            }
            for (int index = 0; index < part.Count; index++)
            {
                if (!colors.TryReadColor(colorWord, out Bgr555 color))
                    throw new InvalidDataException($"Palette-FX colour $8D:{colorWord:X4} of object $8D:{slot.Id:X4} has no installed definition.");
                if (colorIndex >= CgramPalettes.ColorCount)
                {
                    throw new InvalidDataException(
                        $"Palette-FX object $8D:{slot.Id:X4} selected invalid CGRAM colour {colorIndex}.");
                }
                // The colour index is palette-FX instruction data ($8D:xxxx SetColorIndex).
                new CgramPalettes(cgram).All.Write(colorIndex, color);
                colorIndex++;
                colorWord = unchecked((ushort)(colorWord + sizeof(ushort)));
            }
        }
    }

    /// <summary>Serializable native object state retained between palette-FX handler passes.</summary>
    internal sealed class PaletteFxSlot
    {
        /// <summary>Definition pointer; zero marks an available object slot.</summary>
        public ushort Id { get; set; }
        /// <summary>CGRAM colour selected by the most recent SetColorIndex command (native byte offset twice this).</summary>
        public int ColorIndex { get; set; }
        /// <summary>
        /// Pre-instruction evaluated before this slot's program timer; none while the slot is free, which never runs
        /// because the slot is skipped while free.
        /// </summary>
        public PaletteFxPreInstruction? PreInstruction { get; set; }
        /// <summary>Where this object's typed program resumes.</summary>
        public AnimationProgramCursor<PaletteFxProgram> Cursor { get; set; }
        /// <summary>Frames remaining before this object's next bytecode step.</summary>
        public NativeCountdown InstructionTimer { get; set; }
        /// <summary>Loop counter consumed by the decrement-and-goto command.</summary>
        public NativeCountdown Timer { get; set; }

        /// <summary>Releases the object and clears every native interpreter word for reuse.</summary>
        public void Clear()
        {
            Id = 0;
            ColorIndex = 0;
            PreInstruction = null;
            Cursor = default;
            InstructionTimer = NativeCountdown.Start(0);
            Timer = NativeCountdown.Start(0);
        }
    }
}
