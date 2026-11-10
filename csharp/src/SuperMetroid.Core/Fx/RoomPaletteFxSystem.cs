using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The colour effects a room, or a frontend screen, runs (#1554): its glows, flashes, fades and Samus's heat shimmer,
/// each a <see cref="RoomColourEffect"/> that keeps its own step, countdown, colours and condition.
/// </summary>
/// <remarks>
/// <para>
/// Room graphics load a useful static palette, but many rooms immediately animate part of it with the effects the
/// active FX record starts. Landing Site is one of them: its lightning flashes the scrolling-sky horizon's colours.
/// </para>
/// <para>
/// The effects take their turns in the order <see cref="ColourEffectTurnOrder"/> gives them, which Samus's heat shimmer
/// (<see cref="PaletteFxObject.SamusInHeat"/>) and effects sharing colours observe.
/// </para>
/// </remarks>
[RoomLifetime]
public sealed class RoomPaletteFxSystem : IGameplayShadingSource
{
    /// <summary>The running effects, in turn order (<see cref="ColourEffectTurnOrder"/>).</summary>
    internal readonly List<RoomColourEffect> effects = [];
    /// <summary>The record each colour group last showed, in the order they were shown (#1512).</summary>
    internal readonly PaletteFxShown shown = new();
    /// <summary>The records Samus's suit groups showed during the current update, in order (#1512).</summary>
    private readonly List<ShownPaletteFxRecord> suitShown = [];
    /// <summary>The records the enemies' groups showed during the current update, in order (#1512).</summary>
    private readonly List<ShownPaletteFxRecord> enemyShown = [];
    /// <summary>Momentary sound publications cleared at the start of each update.</summary>
    private readonly List<SoundRequest> soundRequests = [];
    /// <summary>Live Power Bomb state used to suppress colour-effect sound requests.</summary>
    private IActivePowerBomb? audioPowerBomb;
    /// <summary>Momentary music publications cleared at the start of each update.</summary>
    private readonly List<DelayedMusicRequest> musicRequests = [];
    /// <summary>Whether the effects run at all and the heat phases, which outlive each room's effects.</summary>
    private readonly PaletteFxSessionState _session;

    /// <summary>Creates one room's colour effects, none running yet, over the session's lasting state.</summary>
    /// <param name="session">Whether the effects run and the heat phases, which room loading never clears.</param>
    public RoomPaletteFxSystem(PaletteFxSessionState session) => _session = Ensure.NotNull(session);

    /// <summary>Whether the colour effects run at all, checked first each update; it lives on the session.</summary>
    public bool HandlerEnabled => _session.HandlerEnabled;

    /// <summary>Sound calls the colour effects published during the current update.</summary>
    public IReadOnlyList<SoundRequest> SoundRequests => soundRequests;

    /// <summary>Music calls the colour effects published during the current update.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => musicRequests;

    /// <summary>Whether <paramref name="effect"/> is running.</summary>
    public bool IsRunning(PaletteFxObject effect) => effects.Exists(running => running.Effect == effect);

    /// <summary>
    /// Starts an effect outside the room's FX record: Samus's load appearance, a Torizo's belly, a statue's grey-out, the
    /// escape's flashes or a screen's effects. It must start: the start is rejected loudly when it does not.
    /// </summary>
    /// <exception cref="InvalidOperationException">The effect did not start: every turn is held or its start skips it.</exception>
    public void Start(
        PaletteFxObject effect,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated = false)
    {
        if (!TryStart(effect, equippedItems, areaMiniBossDefeated))
            throw new InvalidOperationException($"Colour effect {effect} could not start.");
    }

    /// <summary>
    /// Builds one room's colour effects: exactly the effects the matching FX record starts.
    /// </summary>
    /// <param name="session">Whether the effects run and the heat phases, which room loading never clears.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="equippedItems">Samus's equipment, which selects suit-dependent programs.</param>
    /// <param name="areaMiniBossDefeated">Whether the area's mini-boss is dead.</param>
    public static RoomPaletteFxSystem ForRoom(
        PaletteFxSessionState session,
        RoomFxHeader? fx,
        DoorDefinition? door,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated) =>
        ForRecord(session, fx, door, equippedItems, areaMiniBossDefeated, null);

    /// <summary>Builds one room's colour effects from the effects the selected or supplied FX record starts.</summary>
    /// <param name="session">Whether the effects run and the heat phases, which room loading never clears.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="equippedItems">Samus's equipment, which selects suit-dependent programs.</param>
    /// <param name="areaMiniBossDefeated">Whether the area's mini-boss is dead.</param>
    /// <param name="suppliedDefinition">A record to use instead of the door's selection.</param>
    internal static RoomPaletteFxSystem ForRecord(PaletteFxSessionState session, RoomFxHeader? fx, DoorDefinition? door,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated,
        RoomFxRecordDefinition? suppliedDefinition)
    {
        var system = new RoomPaletteFxSystem(session);
        if ((suppliedDefinition ?? fx?.Select(door)) is { } record)
        {
            // Colour effects and animated tiles have separate owners and lists; the record's effects start in its order.
            foreach (PaletteFxObject effect in record.PaletteEffects)
                system.TryStart(effect, equippedItems, areaMiniBossDefeated);
        }
        return system;
    }

    /// <summary>Runs one update of the colour effects during gameplay, each in its turn.</summary>
    /// <remarks>
    /// No colour is written (#1512): each record shown is recorded as its colour group's last, which the room's pictures
    /// take as a shade, and the records of Samus's and the enemies' groups are handed to their owners through
    /// <see cref="SuitShown"/> and <see cref="EnemyShown"/>, whose colourings the import pre-colours per record.
    /// </remarks>
    /// <param name="conditions">The update's conditions the effects watch.</param>
    /// <param name="powerBomb">The live Power Bomb, which suppresses colour-effect sounds while it is active.</param>
    /// <exception cref="InvalidOperationException">A record is shown in colours no gameplay picture shows.</exception>
    public void Step(PaletteFxConditions conditions, IActivePowerBomb? powerBomb = null)
    {
        audioPowerBomb = powerBomb;
        RunTurns(conditions, ShowPaletteRecord);
    }

    /// <summary>
    /// Runs one update of a frontend screen's colour effects (#1512): each record shown is recorded as its colour group's
    /// last, which the screen shades its pictures with, and no owner's colours are written.
    /// </summary>
    /// <exception cref="InvalidOperationException">A record is shown in colours no frontend screen shows.</exception>
    internal void StepScreen(PaletteFxConditions conditions)
    {
        audioPowerBomb = null;
        RunTurns(conditions, ShowScreenRecord);
    }

    /// <summary>
    /// Runs one update of a frontend screen's colour effects that orders each record shown among its own colour loads
    /// (#1512): each record is recorded as its colour group's last and handed to <paramref name="shown"/> as it is shown,
    /// so a record shown again after a load overwrote its colours shows over that load.
    /// </summary>
    /// <exception cref="InvalidOperationException">A record is shown in colours no frontend screen shows.</exception>
    internal void StepScreen(PaletteFxConditions conditions, Action<PaletteFxColours, PaletteFxFrame> shown)
    {
        Ensure.NotNull(shown);
        audioPowerBomb = null;
        RunTurns(conditions, (effect, frame) =>
        {
            ShowScreenRecord(effect, frame);
            shown(effect.Colours ?? throw new InvalidOperationException($"Colour effect {effect.Effect} named no colours."), frame);
        });
    }

    /// <summary>
    /// Gives each running effect its turn, from the lowest turn up: it checks its condition, counts down, and when its
    /// next step is due runs its steps to the next record, which goes to <paramref name="show"/>. Effects that end are
    /// dropped once every turn has run.
    /// </summary>
    private void RunTurns(PaletteFxConditions conditions, Action<RoomColourEffect, PaletteFxFrame> show)
    {
        // Each update owns a new publication window: requests are momentary calls into the shared queues.
        soundRequests.Clear();
        musicRequests.Clear();
        suitShown.Clear();
        enemyShown.Clear();

        foreach (RoomColourEffect effect in effects)
        {
            CheckCondition(effect, conditions);
            if (effect.Ended || !effect.CountDown())
                continue;
            if (effect.Advance(cue => CarryOut(effect, cue)) is { } frame)
                show(effect, frame);
        }
        effects.RemoveAll(effect => effect.Ended);
    }

    /// <summary>Starts <paramref name="effect"/> on the turn its start takes, unless every turn is held or its start skips it.</summary>
    /// <returns>Whether the effect started.</returns>
    /// <exception cref="InvalidOperationException">The effect names a start of no defined kind.</exception>
    private bool TryStart(
        PaletteFxObject effect,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated)
    {
        if (ColourEffectTurnOrder.TurnForStart(effects, running => running.Turn) is not { } turn)
            return false;

        RoomPaletteFxDefinition definition = RoomPaletteFxSelection.Get(effect);
        PaletteFxProgram program;
        switch (definition.Start)
        {
            case ColourEffectStart.Plain:
                program = definition.InitialProgram;
                break;
            case ColourEffectStart.ChoosesHeatProgramForSuit:
                // The heat shimmer chooses its whole program from the same live suit Samus wears; Gravity before Varia.
                program = PaletteFxHeatPrograms.Start(PaletteFxHeatPrograms.SuitFor(equippedItems));
                break;
            case ColourEffectStart.SkippedOnceAreaMiniBossIsDead:
                if (areaMiniBossDefeated)
                    return false;
                program = definition.InitialProgram;
                break;
            default:
                throw new InvalidOperationException($"Colour effect {effect} has undefined start {definition.Start}.");
        }

        ColourEffectTurnOrder.Join(effects, new RoomColourEffect(effect, turn, program), running => running.Turn);
        return true;
    }

    /// <summary>Checks the condition <paramref name="effect"/> watches, before its countdown runs.</summary>
    /// <exception cref="InvalidOperationException">The effect watches a condition of no defined kind.</exception>
    private void CheckCondition(RoomColourEffect effect, PaletteFxConditions conditions)
    {
        switch (effect.Condition)
        {
            case null:
                return;
            case ColourEffectCondition.EndsWhenTorizoDies:
                if (conditions.TorizoIsDead)
                    effect.End();
                return;
            case ColourEffectCondition.EndsWhenAreaMiniBossDies:
                if (conditions.AreaMiniBossDefeated)
                    effect.End();
                return;
            case ColourEffectCondition.RestartsWhileSamusIsHigh high:
                if (conditions.SamusY < high.Height)
                    effect.Restart(high.Restart);
                return;
            case ColourEffectCondition.BurnsSamusInHeat:
                BurnSamusInHeat(effect, conditions.EquippedItems, conditions.Samus, conditions.FrameCounter);
                return;
            default:
                throw new InvalidOperationException($"Colour effect {effect.Effect} watches undefined condition {effect.Condition}.");
        }
    }

    /// <summary>
    /// The heat shimmer's condition. The shimmer deliberately owns both its look and the heat's damage: an unprotected
    /// Samus takes one quarter-unit of 16.16 damage per update, and the shimmer restarts on the heat phase the Norfair glow
    /// published, which that glow, taking its turn after the shimmer, publishes for the following update.
    /// </summary>
    private void BurnSamusInHeat(
        RoomColourEffect effect,
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
                    RoomSound.IncrementalEnergy,
                    PaletteFxHeatData.DamageSoundPriority,
                    SoundSuppressed: audioPowerBomb?.IsActive == true));
            }
        }

        if (_session.SamusInHeatPaletteIndex == _session.PreviousSamusInHeatPaletteIndex)
            return;

        _session.PreviousSamusInHeatPaletteIndex = _session.SamusInHeatPaletteIndex;
        effect.Restart(PaletteFxHeatPrograms.Phase(
            PaletteFxHeatPrograms.SuitFor(equippedItems),
            _session.SamusInHeatPaletteIndex));
    }

    /// <summary>Carries out a step of <paramref name="effect"/>'s program that reaches beyond it: a heat phase or a sound.</summary>
    /// <exception cref="InvalidDataException">A sound step names no sound.</exception>
    /// <exception cref="InvalidOperationException">The step is of a kind the effect keeps itself, or of no defined kind.</exception>
    private void CarryOut(RoomColourEffect effect, PaletteFxCue cue)
    {
        switch (cue)
        {
            case PaletteFxCue.SetHeatPaletteIndex setHeatPaletteIndex:
                _session.SamusInHeatPaletteIndex = setHeatPaletteIndex.HeatPhase;
                return;
            case PaletteFxCue.QueueSound queueSound:
                soundRequests.Add(new SoundRequest(
                    queueSound.Sound ?? throw new InvalidDataException(
                        $"Colour effect {effect.Effect} queued a sound step without a sound."),
                    PaletteFxSoundPriorities.InstructionSound,
                    SoundSuppressed: audioPowerBomb?.IsActive == true));
                return;
            default:
                throw new InvalidOperationException($"Colour effect {effect.Effect} handed on step {cue}, which it keeps itself.");
        }
    }

    /// <summary>
    /// Records one timed colour record as the last its colour group showed on a frontend screen (#1512). A program names
    /// its colours before its first record, so a record shown before that is a defect in the program.
    /// </summary>
    /// <exception cref="InvalidOperationException">The record is shown before its colours are named, or in colours no screen shows.</exception>
    private void ShowScreenRecord(RoomColourEffect effect, PaletteFxFrame frame)
    {
        PaletteFxColours animated = effect.Colours ?? throw new InvalidOperationException(
            $"Colour effect {effect.Effect} showed record {frame.Record} before its program named the colours it animates.");
        if (!PaletteFxScreenColourDefinitions.FrontendScreenGroups.Contains(animated))
            throw new InvalidOperationException($"Colour effect {effect.Effect} showed {animated}, which no frontend screen shows.");
        shown.Show(animated, frame);
    }

    /// <summary>
    /// The records Samus's suit groups showed during the last update, in order (#1512): she lays the shade of each over
    /// her suit, as each record overwrote her suit's colours then.
    /// </summary>
    internal IReadOnlyList<ShownPaletteFxRecord> SuitShown => suitShown;

    /// <summary>
    /// The records the enemies' groups showed during the last update, in order (#1512): each enemy lays the shade of
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
    /// Shows one timed colour record in the colours the effect shows during gameplay (#1512): the record becomes its
    /// group's last, which the room's pictures take as a shade, and a record of Samus's or an enemy's group is also kept
    /// for that owner, which lays its shade over its own draws. A program names its colours before its first record, so
    /// a record shown before that is a defect in the program.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The record is shown before its colours are named, or in colours no room picture, Samus nor enemy shows.
    /// </exception>
    private void ShowPaletteRecord(RoomColourEffect effect, PaletteFxFrame frame)
    {
        PaletteFxColours animated = effect.Colours ?? throw new InvalidOperationException(
            $"Colour effect {effect.Effect} showed record {frame.Record} before its program named the colours it animates.");
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
        throw new InvalidOperationException($"Colour effect {effect.Effect} showed {animated}, which no gameplay picture shows.");
    }
}
