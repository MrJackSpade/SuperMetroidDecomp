using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

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
[RoomLifetime]
public sealed class RoomPaletteFxSystem
{
    /// <summary>Native bank-$8D allocator capacity and descending handler range.</summary>
    private const int SlotCount = 8;

    /// <summary>Persistent interpreter state for the eight palette-FX objects.</summary>
    internal readonly PaletteFxSlot[] slots = Enumerable.Range(0, SlotCount)
        .Select(_ => new PaletteFxSlot())
        .ToArray();
    /// <summary>Momentary sound publications cleared at the start of each handler pass.</summary>
    private readonly List<SoundRequest> soundRequests = [];
    /// <summary>Live Power Bomb state used to suppress palette-FX sound requests.</summary>
    [NonSerialized] private IActivePowerBomb? audioPowerBomb;
    /// <summary>Momentary music publications cleared at the start of each handler pass.</summary>
    private readonly List<DelayedMusicRequest> musicRequests = [];
    /// <summary>The handler enable bit and heat palette phases, which outlive each room's slots.</summary>
    private readonly PaletteFxSessionState _session;

    /// <summary>Creates one room's eight empty palette-FX slots over the session's handler state.</summary>
    /// <param name="session">The handler enable bit and heat palette phases, which room loading never clears.</param>
    public RoomPaletteFxSystem(PaletteFxSessionState session) => _session = Ensure.NotNull(session);

    /// <summary>
    /// Bit 15 of <c>PaletteFXObject_Enable</c> ($1E79), checked first by the handler; it lives on the session.
    /// </summary>
    public bool HandlerEnabled => _session.HandlerEnabled;

    /// <summary>Sound calls published by palette bytecode during the current frame.</summary>
    public IReadOnlyList<SoundRequest> SoundRequests => soundRequests;

    /// <summary>Music calls published by palette bytecode during the current frame.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => musicRequests;

    /// <summary>Whether a particular cartridge definition currently owns a native slot.</summary>
    public bool IsDefinitionActive(PaletteFxObject definition) =>
        slots.Any(slot => slot.Object == definition);

    /// <summary>
    /// Spawns a non-room palette object through the same native allocator/interpreter used
    /// by room FX. Samus's load appearance uses definitions $E1F4/$E1F8/$E1FC and must not
    /// be reimplemented as a host-side tint.
    /// </summary>
    public void SpawnDefinition(
        PaletteFxObject definition,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated = false)
    {
        Spawn(definition, equippedItems, areaMiniBossDefeated);
        if (!IsDefinitionActive(definition))
        {
            throw new InvalidOperationException(
                $"Palette-FX object {definition} could not acquire a native slot.");
        }
    }

    /// <summary>
    /// Builds one room's palette-FX objects: exactly the bits selected by the matching
    /// sixteen-byte bank-$83 FX record.
    /// </summary>
    /// <param name="session">The handler enable bit and heat palette phases, which room loading never clears.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="area">The room's area, which selects each bit's object.</param>
    /// <param name="equippedItems">Samus's equipment, which selects suit-dependent programs.</param>
    /// <param name="areaMiniBossDefeated">Whether the area's mini-boss is dead.</param>
    public static RoomPaletteFxSystem ForRoom(
        PaletteFxSessionState session,
        RoomFxDefinition? fx,
        CartridgeDoorHeader? door,
        AreaId area,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated) =>
        ForRecord(session, fx, door, area, equippedItems, areaMiniBossDefeated, null);

    /// <summary>Builds one room's palette-FX objects from the selected or supplied FX record's area-specific bits.</summary>
    /// <param name="session">The handler enable bit and heat palette phases, which room loading never clears.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="area">The room's area, which selects each bit's object.</param>
    /// <param name="equippedItems">Samus's equipment, which selects suit-dependent programs.</param>
    /// <param name="areaMiniBossDefeated">Whether the area's mini-boss is dead.</param>
    /// <param name="suppliedDefinition">A record to use instead of the door's selection.</param>
    internal static RoomPaletteFxSystem ForRecord(PaletteFxSessionState session, RoomFxDefinition? fx, CartridgeDoorHeader? door,
        AreaId area, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated,
        RoomFxRecordDefinition? suppliedDefinition)
    {
        var system = new RoomPaletteFxSystem(session);
        system.SpawnRecordObjects(fx, door, area, equippedItems, areaMiniBossDefeated, suppliedDefinition);
        return system;
    }

    /// <summary>Spawns the selected or supplied FX record's area-specific palette bits.</summary>
    private void SpawnRecordObjects(RoomFxDefinition? fx, CartridgeDoorHeader? door,
        AreaId area, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated,
        RoomFxRecordDefinition? suppliedDefinition)
    {
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

            PaletteFxObject definition = RoomPaletteFxDefinitions.GetAreaDefinition(areaIndex, bit);
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
    public void Step(IPaletteFxColourOwners owners, IPaletteFxColorSource colors, PaletteFxConditions conditions,
        IActivePowerBomb? powerBomb = null)
    {
        Ensure.NotNull(owners);
        Ensure.NotNull(colors);
        audioPowerBomb = powerBomb;

        // PaletteFXObject_Handler owns a new publication window on every game frame.
        // Requests are momentary calls into bank $80, not persistent object state.
        soundRequests.Clear();
        musicRequests.Clear();

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            PaletteFxSlot slot = slots[slotIndex];
            if (slot.Object is null)
                continue;

            RunPreInstruction(slot, slotIndex, conditions);
            if (slot.Object is null)
                continue;

            // `$8D:C552` decrements before comparing with one. Unsigned underflow is
            // intentional cartridge behavior for malformed or externally edited state.
            slot.InstructionTimer = slot.InstructionTimer.Decrement().Remaining;
            if (!slot.InstructionTimer.IsZero)
                continue;

            ExecuteProgram(owners, colors, slot);
        }
    }

    /// <summary>Allocates the highest free native slot and applies definition-specific setup for Intro, Norfair, or Brinstar.</summary>
    private void Spawn(
        PaletteFxObject definition,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated)
    {
        // Spawn_PaletteFXObject searches byte offsets 14,12,...,0, which is slot order
        // 7..0. A full native pool silently rejects another spawn; this is a documented
        // allocation result, not a failed translation that should be hidden by an exception.
        PaletteFxSlot? slot = null;
        for (int index = SlotCount - 1; index >= 0; index--)
        {
            if (slots[index].Object is null)
            {
                slot = slots[index];
                break;
            }
        }
        if (slot is null)
            return;

        RoomPaletteFxDefinition compiled = RoomPaletteFxDefinitions.Get(definition);
        slot.Object = definition;
        slot.Target = default;
        slot.PreInstruction = PaletteFxPreInstruction.Null;
        slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(compiled.InitialProgram);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

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
                    $"Palette-FX object {definition} has undefined setup {setup}.");
        }
    }

    /// <summary>Runs the slot's condition handler before timer and bytecode execution, including room heat and cutscene gates.</summary>
    private void RunPreInstruction(PaletteFxSlot slot, int slotIndex, PaletteFxConditions conditions)
    {
        if (slot.PreInstruction is not { } preInstruction || !Enum.IsDefined(preInstruction))
        {
            throw new NotSupportedException(
                $"Room palette-FX pre-instruction {slot.PreInstruction} is not translated " +
                $"for object {slot.Object} (Samus Y=${conditions.SamusY:X4}, items=${conditions.EquippedItems.ToNativeWord():X4}).");
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
                    slot.InstructionTimer = WrappingCountdown.Start(1);
                    slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxProgram.Crateria11);
                }
                return;

            case PaletteFxPreInstruction.SwitchAboveY380Second:
                if (conditions.SamusY <
                    CrateriaLightningPaletteFxColorDefinitions.VerticalSwitchSamusY)
                {
                    slot.InstructionTimer = WrappingCountdown.Start(1);
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
                // Slot 0 reads the enable word, which is always set while the handler runs; the
                // current-index slot reads its own nonzero byte offset.
                bool adjacentWordSet = slotIndex switch
                {
                    0 => true,
                    PaletteFxMemoryLayout.CurrentIndexWordOffset => true,
                    _ => slots[slotIndex - PaletteFxMemoryLayout.IdArrayWordOffset].Object is not null,
                };
                if (adjacentWordSet) slot.Clear();
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
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        IRoomSamus? samus,
        int nmiFrameCounter)
    {
        bool protectedFromHeat = equippedItems.HasAny(
            SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit);
        if (!protectedFromHeat && samus is not null)
        {
            samus.TakePeriodicDamage(
                PaletteFxHeatData.SubdamagePerFrame,
                wholeDamage: 0);
            if ((nmiFrameCounter & 7) == 0 &&
                samus.Health > PaletteFxHeatData.DamageSoundEnergyThreshold)
            {
                soundRequests.Add(new SoundRequest(
                    Library3Sound.IncrementalEnergy,
                    PaletteFxHeatData.DamageSoundMaximumQueued,
                    SoundSuppressed: audioPowerBomb?.IsActive == true));
            }
        }

        if (_session.SamusInHeatPaletteIndex == _session.PreviousSamusInHeatPaletteIndex)
            return;

        _session.PreviousSamusInHeatPaletteIndex = _session.SamusInHeatPaletteIndex;
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxHeatPrograms.Phase(
            PaletteFxHeatPrograms.SuitFor(equippedItems),
            _session.SamusInHeatPaletteIndex));
    }

    /// <summary>Runs the slot's typed program until it shows its next timed colour record or deletes the object.</summary>
    private void ExecuteProgram(
        IPaletteFxColourOwners owners,
        IPaletteFxColorSource colors,
        PaletteFxSlot slot)
    {
        AnimationProgramCursor<PaletteFxProgram> cursor = slot.Cursor;
        WrappingCountdown timer = slot.Timer;
        AnimationProgramAdvance<PaletteFxFrame> advance = PaletteFxAnimations.Programs.Advance(ref cursor, ref timer,
            (cue, _) =>
            {
                Publish(slot, cue);
                return null;
            });
        slot.Cursor = cursor;
        slot.Timer = timer;
        switch (advance)
        {
            case AnimationProgramAdvance<PaletteFxFrame>.Show shown:
                slot.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
                WritePaletteRecord(owners, colors, slot, shown.Frame);
                return;
            case AnimationProgramAdvance<PaletteFxFrame>.Stop:
                slot.Clear();
                return;
            case AnimationProgramAdvance<PaletteFxFrame>.Wait or AnimationProgramAdvance<PaletteFxFrame>.Hold:
                throw new InvalidOperationException($"Palette-FX program {cursor.Program} reached a {advance}, which no palette program has.");
            default:
                throw new InvalidOperationException($"Undefined animation program advance {advance}.");
        }
    }

    /// <summary>Carries out one palette-FX program command.</summary>
    private void Publish(PaletteFxSlot slot, PaletteFxCue cue)
    {
        switch (cue)
        {
            case PaletteFxCue.SetColours setColours:
                slot.Target = setColours.Target;
                return;
            case PaletteFxCue.SetPreInstruction setPreInstruction:
                slot.PreInstruction = setPreInstruction.PreInstruction;
                return;
            case PaletteFxCue.Command { Kind: PaletteFxCueKind.ClearPreInstruction }:
                slot.PreInstruction = PaletteFxPreInstruction.Cleared;
                return;
            case PaletteFxCue.SetHeatPaletteIndex setHeatPaletteIndex:
                _session.SamusInHeatPaletteIndex = setHeatPaletteIndex.HeatPhase;
                return;
            case PaletteFxCue.QueueSoundLibrary2 queueSoundLibrary2:
                soundRequests.Add(new SoundRequest(
                    queueSoundLibrary2.Sound ?? throw new InvalidDataException(
                        $"Palette-FX object {slot.Object} queued a library-2 sound cue without a sound."),
                    PaletteFxAudioQueueLimits.SoundEffects,
                    SoundSuppressed: audioPowerBomb?.IsActive == true));
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PaletteFxCueKind)} {cue}.");
        }
    }

    /// <summary>Writes one timed colour record to the slot's target colours, skipping the colours its layout skips.</summary>
    private static void WritePaletteRecord(
        IPaletteFxColourOwners owners,
        IPaletteFxColorSource colors,
        PaletteFxSlot slot,
        PaletteFxFrame frame)
    {
        ColourRun target = owners.Colours(slot.Target.Owner);
        int colorIndex = slot.Target.Colour;
        int written = 0;
        foreach (PaletteFxRecordPart part in PaletteFxRecordDefinitions.Layout(frame))
        {
            if (part.Skip)
            {
                colorIndex += part.Count;
                continue;
            }
            for (int index = 0; index < part.Count; index++)
            {
                if (!colors.TryReadColor(new PaletteFxColorKey(frame, written), out Bgr555 color))
                    throw new InvalidDataException($"Palette-FX colour {written} of record {frame.Record} of object {slot.Object} has no installed definition.");
                if (colorIndex >= target.Count)
                {
                    throw new InvalidDataException(
                        $"Palette-FX object {slot.Object} wrote past {slot.Target.Owner}'s colours at colour {colorIndex}.");
                }
                target.Write(colorIndex, color);
                colorIndex++;
                written++;
            }
        }
    }

    /// <summary>Serializable native object state retained between palette-FX handler passes.</summary>
    internal sealed class PaletteFxSlot
    {
        /// <summary>The palette-FX object the slot runs; null marks an available slot.</summary>
        public PaletteFxObject? Object { get; set; }
        /// <summary>The colours the most recent <see cref="PaletteFxCue.SetColours"/> cue selected.</summary>
        public PaletteFxColours Target { get; set; }
        /// <summary>
        /// Pre-instruction evaluated before this slot's program timer; none while the slot is free, which never runs
        /// because the slot is skipped while free.
        /// </summary>
        public PaletteFxPreInstruction? PreInstruction { get; set; }
        /// <summary>Where this object's typed program resumes.</summary>
        public AnimationProgramCursor<PaletteFxProgram> Cursor { get; set; }
        /// <summary>Frames remaining before this object's next bytecode step.</summary>
        public WrappingCountdown InstructionTimer { get; set; }
        /// <summary>Loop counter consumed by the decrement-and-goto command.</summary>
        public WrappingCountdown Timer { get; set; }

        /// <summary>Releases the object and clears every native interpreter word for reuse.</summary>
        public void Clear()
        {
            Object = null;
            Target = default;
            PreInstruction = null;
            Cursor = default;
            InstructionTimer = WrappingCountdown.Start(0);
            Timer = WrappingCountdown.Start(0);
        }
    }
}
