using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Game-driven owner for the eight palette-FX slots created by room FX.
/// </summary>
/// <remarks>
/// Room graphics load a useful static palette, but many rooms immediately replace part of
/// it with programs selected by the active FX record. Landing Site is one of them:
/// palette-FX bit zero flashes the scrolling-sky horizon's lightning colours. Keeping the
/// programs, timers, and the colours each object animates here avoids encoding a
/// screenshot's colors as a room-specific correction.
/// </remarks>
[RoomLifetime]
public sealed class RoomPaletteFxSystem : IGameplayShadingSource
{
    /// <summary>Palette-FX pool capacity and descending handler range.</summary>
    private const int SlotCount = 8;

    /// <summary>Persistent interpreter state for the eight palette-FX objects.</summary>
    internal readonly PaletteFxSlot[] slots = Enumerable.Range(0, SlotCount)
        .Select(_ => new PaletteFxSlot())
        .ToArray();
    /// <summary>The record each colour group last showed, in the order they were shown (#1512).</summary>
    internal readonly PaletteFxShown shown = new();
    /// <summary>The records Samus's suit groups showed during the current handler pass, in order (#1512).</summary>
    private readonly List<ShownPaletteFxRecord> suitShown = [];
    /// <summary>The records the enemies' groups showed during the current handler pass, in order (#1512).</summary>
    private readonly List<ShownPaletteFxRecord> enemyShown = [];
    /// <summary>Momentary sound publications cleared at the start of each handler pass.</summary>
    private readonly List<SoundRequest> soundRequests = [];
    /// <summary>Live Power Bomb state used to suppress palette-FX sound requests.</summary>
    private IActivePowerBomb? audioPowerBomb;
    /// <summary>Momentary music publications cleared at the start of each handler pass.</summary>
    private readonly List<DelayedMusicRequest> musicRequests = [];
    /// <summary>The handler enable bit and heat palette phases, which outlive each room's slots.</summary>
    private readonly PaletteFxSessionState _session;

    /// <summary>Creates one room's eight empty palette-FX slots over the session's handler state.</summary>
    /// <param name="session">The handler enable bit and heat palette phases, which room loading never clears.</param>
    public RoomPaletteFxSystem(PaletteFxSessionState session) => _session = Ensure.NotNull(session);

    /// <summary>
    /// Bit 15 of the palette-FX enable word, checked first by the handler; it lives on the session.
    /// </summary>
    public bool HandlerEnabled => _session.HandlerEnabled;

    /// <summary>Sound calls published by palette bytecode during the current frame.</summary>
    public IReadOnlyList<SoundRequest> SoundRequests => soundRequests;

    /// <summary>Music calls published by palette bytecode during the current frame.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => musicRequests;

    /// <summary>Whether a particular definition currently owns a slot.</summary>
    public bool IsDefinitionActive(PaletteFxObject definition) =>
        slots.Any(slot => slot.Object == definition);

    /// <summary>
    /// Spawns a non-room palette object through the same allocator/interpreter used
    /// by room FX. Samus's load appearance uses its own three definitions and must not
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
                $"Palette-FX object {definition} could not acquire a slot.");
        }
    }

    /// <summary>
    /// Builds one room's palette-FX objects: exactly the bits selected by the matching
    /// sixteen-byte FX record.
    /// </summary>
    /// <param name="session">The handler enable bit and heat palette phases, which room loading never clears.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="area">The room's area, which selects each bit's object.</param>
    /// <param name="equippedItems">Samus's equipment, which selects suit-dependent programs.</param>
    /// <param name="areaMiniBossDefeated">Whether the area's mini-boss is dead.</param>
    public static RoomPaletteFxSystem ForRoom(
        PaletteFxSessionState session,
        RoomFxHeader? fx,
        DoorDefinition? door,
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
    internal static RoomPaletteFxSystem ForRecord(PaletteFxSessionState session, RoomFxHeader? fx, DoorDefinition? door,
        AreaId area, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated,
        RoomFxRecordDefinition? suppliedDefinition)
    {
        var system = new RoomPaletteFxSystem(session);
        system.SpawnRecordObjects(fx, door, area, equippedItems, areaMiniBossDefeated, suppliedDefinition);
        return system;
    }

    /// <summary>Spawns the selected or supplied FX record's area-specific palette bits.</summary>
    private void SpawnRecordObjects(RoomFxHeader? fx, DoorDefinition? door,
        AreaId area, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated,
        RoomFxRecordDefinition? suppliedDefinition)
    {
        if (suppliedDefinition is null && fx is null)
            return;
        int areaIndex = AreaIds.ToIndex(area);

        if ((suppliedDefinition ?? fx?.Select(door)) is not { } record)
            return;

        // Palette FX and animated tiles have separate owners and bitsets.
        RoomFxEffectSlots paletteFxSlots = record.PaletteFxSlots;
        if (paletteFxSlots == RoomFxEffectSlots.None)
            return;

        for (int bit = 0; bit < SlotCount; bit++)
        {
            if (!paletteFxSlots.Includes(bit))
                continue;

            PaletteFxObject definition = RoomPaletteFxSelection.GetAreaDefinition(areaIndex, bit);
            Spawn(definition, equippedItems, areaMiniBossDefeated);
        }
    }

    /// <summary>Runs one <c>PaletteFXObject_Handler</c> pass in descending slot order during gameplay.</summary>
    /// <remarks>
    /// No colour is written (#1512): each record shown is recorded as its colour group's last, which the room's pictures
    /// take as a shade, and the records of Samus's and the enemies' groups are handed to their owners through
    /// <see cref="SuitShown"/> and <see cref="EnemyShown"/>, whose colourings the import pre-colours per record.
    /// </remarks>
    /// <param name="conditions">The frame's handler inputs.</param>
    /// <param name="powerBomb">The live Power Bomb, which suppresses palette-FX sounds while it is active.</param>
    /// <exception cref="InvalidOperationException">A record is shown in colours no gameplay picture shows.</exception>
    public void Step(PaletteFxConditions conditions, IActivePowerBomb? powerBomb = null)
    {
        audioPowerBomb = powerBomb;
        RunHandler(conditions, ShowPaletteRecord);
    }

    /// <summary>
    /// Runs one <c>PaletteFXObject_Handler</c> pass for a frontend screen (#1512): each record shown is recorded as its colour
    /// group's last, which the screen shades its pictures with, and no owner's colours are written.
    /// </summary>
    /// <exception cref="InvalidOperationException">A record is shown in colours no frontend screen shows.</exception>
    internal void StepScreen(PaletteFxConditions conditions)
    {
        audioPowerBomb = null;
        RunHandler(conditions, ShowScreenRecord);
    }

    /// <summary>
    /// Runs one <c>PaletteFXObject_Handler</c> pass for a frontend screen that orders each record shown among its own colour
    /// loads (#1512): each record is recorded as its colour group's last and handed to <paramref name="shown"/> as it is shown,
    /// so a record shown again after a load overwrote its colours shows over that load, as the handler's write does.
    /// </summary>
    /// <exception cref="InvalidOperationException">A record is shown in colours no frontend screen shows.</exception>
    internal void StepScreen(PaletteFxConditions conditions, Action<PaletteFxColours, PaletteFxFrame> shown)
    {
        Ensure.NotNull(shown);
        audioPowerBomb = null;
        RunHandler(conditions, (slot, frame) =>
        {
            ShowScreenRecord(slot, frame);
            shown(slot.Target ?? throw new InvalidOperationException($"Palette-FX object {slot.Object} named no colours."), frame);
        });
    }

    /// <summary>Runs the slots in descending order, handing each record a slot's program shows to <paramref name="show"/>.</summary>
    private void RunHandler(PaletteFxConditions conditions, Action<PaletteFxSlot, PaletteFxFrame> show)
    {
        // PaletteFXObject_Handler owns a new publication window on every game frame.
        // Requests are momentary calls into the shared queues, not persistent object state.
        soundRequests.Clear();
        musicRequests.Clear();
        suitShown.Clear();
        enemyShown.Clear();

        for (int slotIndex = SlotCount - 1; slotIndex >= 0; slotIndex--)
        {
            PaletteFxSlot slot = slots[slotIndex];
            if (slot.Object is null)
                continue;

            RunFrameBehavior(slot, slotIndex, conditions);
            if (slot.Object is null)
                continue;

            // The timer decrements before comparing with one; every colour record lasts at least one update and a
            // spawned or redirected object starts at one, so a running object's timer never steps from zero.
            if (--slot.FrameHold != 0)
                continue;

            ExecuteProgram(slot, show);
        }
    }

    /// <summary>Allocates the highest free slot and applies definition-specific setup for Intro, Norfair, or Brinstar.</summary>
    private void Spawn(
        PaletteFxObject definition,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated)
    {
        // Spawn_PaletteFXObject searches byte offsets 14,12,...,0, which is slot order
        // 7..0. A full pool silently rejects another spawn; this is a documented
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

        RoomPaletteFxDefinition compiled = RoomPaletteFxSelection.Get(definition);
        slot.Object = definition;
        slot.Target = null;
        slot.FrameBehavior = PaletteFxFrameBehavior.Null;
        slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(compiled.InitialProgram);
        slot.FrameHold = 1;
        slot.Timer = WrappingCountdown.Start(0);

        PaletteFxSetup setup = compiled.SetupCallback;
        switch (setup)
        {
            case PaletteFxSetup.Null:
                return;

            case PaletteFxSetup.Intro:
                slot.FrameBehavior = PaletteFxFrameBehavior.Intro;
                return;

            case PaletteFxSetup.Norfair:
                // The heat object chooses the complete program from the same live suit bits
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
    private void RunFrameBehavior(PaletteFxSlot slot, int slotIndex, PaletteFxConditions conditions)
    {
        if (slot.FrameBehavior is not { } frameBehavior || !Enum.IsDefined(frameBehavior))
        {
            throw new NotSupportedException(
                $"Room palette-FX pre-instruction {slot.FrameBehavior} is not translated " +
                $"for object {slot.Object} (Samus Y=${conditions.SamusY:X4}, items={conditions.EquippedItems}).");
        }
        switch (frameBehavior)
        {
            case PaletteFxFrameBehavior.Null:
            case PaletteFxFrameBehavior.Cleared:
            case PaletteFxFrameBehavior.Intro:
                return;

            case PaletteFxFrameBehavior.DeleteWhenEnemyZeroDies:
                if (conditions.EnemyZeroIsDead)
                    slot.Clear();
                return;

            case PaletteFxFrameBehavior.SwitchAboveY380:
                if (conditions.SamusY <
                    CrateriaLightningPaletteFxColorDefinitions.VerticalSwitchSamusY)
                {
                    slot.FrameHold = 1;
                    slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxProgram.Crateria11);
                }
                return;

            case PaletteFxFrameBehavior.SwitchAboveY380Second:
                if (conditions.SamusY <
                    CrateriaLightningPaletteFxColorDefinitions.VerticalSwitchSamusY)
                {
                    slot.FrameHold = 1;
                    slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxProgram.CrateriaAboveY380Second);
                }
                return;

            case PaletteFxFrameBehavior.DeleteWhenAreaMiniBossDies:
                if (conditions.AreaMiniBossDefeated)
                    slot.Clear();
                return;

            case PaletteFxFrameBehavior.Heat:
                RunHeatFrameBehavior(slot, conditions.EquippedItems, conditions.Samus, conditions.FrameCounter);
                return;

            case PaletteFxFrameBehavior.InspectAdjacentSlot:
                if (InspectedNeighbourIsSet(slotIndex)) slot.Clear();
                return;

            default:
                throw new InvalidOperationException($"Undefined palette-FX pre-instruction {frameBehavior}.");
        }
    }

    /// <summary>
    /// Whether the neighbour the inspecting pre-instruction looks at is set. It looks
    /// <see cref="PaletteFxSlotInspectionDefinitions.InspectedSlotDistance"/> slots below its own, reading the live
    /// slots so deletions earlier in this descending pass are visible. A slot with no slot that far below looks at
    /// the handler's own enable and running-slot state instead, which are always set while the handler runs.
    /// </summary>
    private bool InspectedNeighbourIsSet(int slotIndex) =>
        slotIndex < PaletteFxSlotInspectionDefinitions.InspectedSlotDistance
        || slots[slotIndex - PaletteFxSlotInspectionDefinitions.InspectedSlotDistance].Object is not null;

    /// <summary>
    /// Runs the Samus-in-heat per-frame behaviour step. The palette
    /// object deliberately owns both presentation and damage: an unprotected Samus gains
    /// one quarter-unit of 16.16 damage per frame, while a second Norfair object publishes
    /// the animation index consumed here on the following descending-slot pass.
    /// </summary>
    private void RunHeatFrameBehavior(
        PaletteFxSlot slot,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        IRoomSamus? samus,
        int frameCounter)
    {
        bool protectedFromHeat = equippedItems.HasAny(
            SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit);
        if (!protectedFromHeat && samus is not null)
        {
            samus.TakePeriodicDamage(
                PaletteFxHeatData.SubdamagePerFrame,
                wholeDamage: 0);
            if ((frameCounter & 7) == 0 &&
                samus.Health > PaletteFxHeatData.DamageSoundEnergyThreshold)
            {
                soundRequests.Add(new SoundRequest(
                    Library3Sound.IncrementalEnergy,
                    PaletteFxHeatData.DamageSoundPriority,
                    SoundSuppressed: audioPowerBomb?.IsActive == true));
            }
        }

        if (_session.SamusInHeatPaletteIndex == _session.PreviousSamusInHeatPaletteIndex)
            return;

        _session.PreviousSamusInHeatPaletteIndex = _session.SamusInHeatPaletteIndex;
        slot.FrameHold = 1;
        slot.Cursor = AnimationProgramCursor<PaletteFxProgram>.Start(PaletteFxHeatPrograms.Phase(
            PaletteFxHeatPrograms.SuitFor(equippedItems),
            _session.SamusInHeatPaletteIndex));
    }

    /// <summary>Runs the slot's typed program until it shows its next timed colour record or deletes the object.</summary>
    private void ExecuteProgram(PaletteFxSlot slot, Action<PaletteFxSlot, PaletteFxFrame> show)
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
                slot.FrameHold = shown.Duration;
                show(slot, shown.Frame);
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
            case PaletteFxCue.SetFrameBehavior setFrameBehavior:
                slot.FrameBehavior = setFrameBehavior.FrameBehavior;
                return;
            case PaletteFxCue.Command { Kind: PaletteFxCueKind.ClearFrameBehavior }:
                slot.FrameBehavior = PaletteFxFrameBehavior.Cleared;
                return;
            case PaletteFxCue.SetHeatPaletteIndex setHeatPaletteIndex:
                _session.SamusInHeatPaletteIndex = setHeatPaletteIndex.HeatPhase;
                return;
            case PaletteFxCue.QueueSoundLibrary2 queueSoundLibrary2:
                soundRequests.Add(new SoundRequest(
                    queueSoundLibrary2.Sound ?? throw new InvalidDataException(
                        $"Palette-FX object {slot.Object} queued a library-2 sound cue without a sound."),
                    PaletteFxSoundPriorities.InstructionSound,
                    SoundSuppressed: audioPowerBomb?.IsActive == true));
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PaletteFxCueKind)} {cue}.");
        }
    }

    /// <summary>
    /// Records one timed colour record as the last its colour group showed on a frontend screen (#1512). A program names
    /// its colours before its first record, so a record shown before that is a defect in the program.
    /// </summary>
    /// <exception cref="InvalidOperationException">The record is shown before its colours are named, or in colours no screen shows.</exception>
    private void ShowScreenRecord(PaletteFxSlot slot, PaletteFxFrame frame)
    {
        PaletteFxColours animated = slot.Target ?? throw new InvalidOperationException(
            $"Palette-FX object {slot.Object} showed record {frame.Record} before its program named the colours it animates.");
        if (!PaletteFxScreenColourDefinitions.FrontendScreenGroups.Contains(animated))
            throw new InvalidOperationException($"Palette-FX object {slot.Object} showed {animated}, which no frontend screen shows.");
        shown.Show(animated, frame);
    }

    /// <summary>
    /// The records Samus's suit groups showed during the last handler pass, in order (#1512): she lays the shade of each over
    /// her suit, as each record overwrote her suit's colours then.
    /// </summary>
    internal IReadOnlyList<ShownPaletteFxRecord> SuitShown => suitShown;

    /// <summary>
    /// The records the enemies' groups showed during the last handler pass, in order (#1512): each enemy lays the shade of
    /// each over its draws, as each record overwrote its colours then.
    /// </summary>
    internal IReadOnlyList<ShownPaletteFxRecord> EnemyShown => enemyShown;

    /// <summary>The record <paramref name="colours"/> last showed in this room, or null while it has shown none (#1512).</summary>
    internal PaletteFxFrame? RecordOf(PaletteFxColours colours) => shown.RecordOf(colours);

    /// <summary>
    /// Lays the shade of the record each of the room's colour groups last showed over the room's pictures, in the order
    /// they were shown (#1512).
    /// </summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach ((PaletteFxColours colours, PaletteFxFrame record) in shown.InOrder)
        {
            if (PaletteFxRoomColourDefinitions.RoomColours.Contains(colours))
                frame.Shade(GameplayPictures.Room, PaletteFxShades.Of(colours, record));
        }
    }

    /// <summary>
    /// Shows one timed colour record in the colours the slot animates during gameplay (#1512): the record becomes its
    /// group's last, which the room's pictures take as a shade, and a record of Samus's or an enemy's group is also kept
    /// for that owner, which lays its shade over its own draws. A program names its colours before its first record, so
    /// a record shown before that is a defect in the program.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The record is shown before its colours are named, or in colours no room picture, Samus nor enemy shows.
    /// </exception>
    private void ShowPaletteRecord(PaletteFxSlot slot, PaletteFxFrame frame)
    {
        PaletteFxColours animated = slot.Target ?? throw new InvalidOperationException(
            $"Palette-FX object {slot.Object} showed record {frame.Record} before its program named the colours it animates.");
        shown.Show(animated, frame);
        if (PaletteFxRoomColourDefinitions.RoomColours.Contains(animated))
            return;
        if (PaletteFxSamusColourDefinitions.SuitColours.Contains(animated))
        {
            suitShown.Add(new ShownPaletteFxRecord(animated, frame));
            return;
        }
        if (PaletteFxEnemyColourDefinitions.EnemyColours.Contains(animated))
        {
            enemyShown.Add(new ShownPaletteFxRecord(animated, frame));
            return;
        }
        throw new InvalidOperationException($"Palette-FX object {slot.Object} showed {animated}, which no gameplay picture shows.");
    }

    /// <summary>Object state retained between palette-FX handler passes.</summary>
    internal sealed class PaletteFxSlot
    {
        /// <summary>The palette-FX object the slot runs; null marks an available slot.</summary>
        public PaletteFxObject? Object { get; set; }
        /// <summary>
        /// The colours the most recent <see cref="PaletteFxCue.SetColours"/> cue selected; none until the object's
        /// program names them.
        /// </summary>
        public PaletteFxColours? Target { get; set; }
        /// <summary>
        /// Pre-instruction evaluated before this slot's program timer; none while the slot is free, which never runs
        /// because the slot is skipped while free.
        /// </summary>
        public PaletteFxFrameBehavior? FrameBehavior { get; set; }
        /// <summary>Where this object's typed program resumes.</summary>
        public AnimationProgramCursor<PaletteFxProgram> Cursor { get; set; }
        /// <summary>Frames remaining before this object's next bytecode step.</summary>
        public int FrameHold { get; set; }
        /// <summary>Loop counter consumed by the decrement-and-goto command.</summary>
        public WrappingCountdown Timer { get; set; }

        /// <summary>Releases the object and clears every interpreter word for reuse.</summary>
        public void Clear()
        {
            Object = null;
            Target = null;
            FrameBehavior = null;
            Cursor = default;
            FrameHold = 0;
            Timer = WrappingCountdown.Start(0);
        }
    }
}
