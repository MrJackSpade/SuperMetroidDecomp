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
/// The effects take their turns in the order they started (<see cref="ColourEffectTurnOrder"/>), which Samus's heat
/// shimmer (<see cref="PaletteFxObject.SamusInHeat"/>) observes. Any number of effects run at once.
/// </para>
/// </remarks>
[RoomLifetime]
public sealed class RoomPaletteFxSystem : IGameplayShadingSource
{
    /// <summary>The running effects, in turn order (<see cref="ColourEffectTurnOrder"/>).</summary>
    internal readonly List<RoomColourEffect> effects = [];
    /// <summary>
    /// The effects whose programs ran to their end (#1597), in the order they settled: each keeps its final frame on its
    /// colours, without taking a turn, until an effect showing the same colours settles after it or the room ends.
    /// </summary>
    private readonly List<RoomColourEffect> settled = [];
    /// <summary>The frames Samus's suit groups showed during the current update, in order (#1512).</summary>
    private readonly List<ShownPaletteFxFrame> suitShown = [];
    /// <summary>The frames the enemies' groups showed during the current update, in order (#1512).</summary>
    private readonly List<ShownPaletteFxFrame> enemyShown = [];
    /// <summary>Whether the effects run at all and the heat phases, which outlive each room's effects.</summary>
    private readonly PaletteFxSessionState _session;

    /// <summary>Creates one room's colour effects, none running yet, over the session's lasting state.</summary>
    /// <param name="session">Whether the effects run and the heat phases, which room loading never clears.</param>
    public RoomPaletteFxSystem(PaletteFxSessionState session) => _session = Ensure.NotNull(session);

    /// <summary>Whether the colour effects run at all, checked first each update; it lives on the session.</summary>
    public bool HandlerEnabled => _session.HandlerEnabled;

    /// <summary>
    /// The audio requests the colour effects' sounds join as they make them: the runtime's update's, once it points the
    /// room's systems at it; the effects' own record otherwise, which a frontend screen's effects keep and reach no queue.
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; private set; } = new();

    /// <summary>Points the colour effects' sounds at <paramref name="requests"/>, the update's audio requests of the runtime running them.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests) => AudioRequests = Ensure.NotNull(requests);

    /// <summary>Whether <paramref name="effect"/> is running.</summary>
    public bool IsRunning(PaletteFxObject effect) => effects.Exists(running => running.Effect == effect);

    /// <summary>
    /// Starts an effect outside the room's FX record: Samus's load appearance, a Torizo's belly, a statue's grey-out, the
    /// escape's flashes or a screen's effects. It must start: the start is rejected loudly when it does not.
    /// </summary>
    /// <exception cref="InvalidOperationException">The effect did not start: its start skips it.</exception>
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
    /// No colour is written (#1512, #1597): each effect shows the frame it holds now, which the room's pictures take as a
    /// shade, and the frames of Samus's and the enemies' groups are handed to their owners through
    /// <see cref="SuitShown"/> and <see cref="EnemyShown"/>, whose colourings the import pre-colours per frame.
    /// </remarks>
    /// <param name="conditions">The update's conditions the effects watch.</param>
    /// <exception cref="InvalidOperationException">A frame is shown in colours no gameplay picture shows.</exception>
    public void Step(PaletteFxConditions conditions)
    {
        RunTurns(Ensure.NotNull(conditions), ShowGameplayFrame);
    }

    /// <summary>
    /// Runs one update of a frontend screen's colour effects (#1512, #1597): each effect shows the frame it holds now,
    /// which the screen shades its pictures with, and no owner's colours are written.
    /// </summary>
    /// <exception cref="InvalidOperationException">A frame is shown in colours no frontend screen shows.</exception>
    internal void StepScreen(PaletteFxConditions conditions)
    {
        RunTurns(conditions, ShowScreenFrame);
    }

    /// <summary>
    /// Gives each running effect its turn, in the order they started: it checks its condition, counts down, and when its
    /// next step is due runs its steps to the next frame, which goes to <paramref name="show"/>. Effects that end are
    /// dropped from the turns once every turn has run; one that settled keeps its final frame shown.
    /// </summary>
    private void RunTurns(PaletteFxConditions conditions, Action<RoomColourEffect, PaletteFxFrame> show)
    {
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
        foreach (RoomColourEffect effect in effects)
        {
            if (!effect.Settled)
                continue;
            settled.RemoveAll(earlier => earlier.Colours == effect.Colours);
            settled.Add(effect);
        }
        effects.RemoveAll(effect => effect.Ended);
    }

    /// <summary>Starts <paramref name="effect"/> after the running effects, unless its start skips it.</summary>
    /// <returns>Whether the effect started.</returns>
    /// <exception cref="InvalidOperationException">The effect names a start of no defined kind.</exception>
    private bool TryStart(
        PaletteFxObject effect,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated)
    {
        RoomPaletteFxDefinition definition = RoomPaletteFxSelection.Get(effect);
        PaletteFxSegment program;
        switch (definition.Start)
        {
            case ColourEffectStart.Plain:
                program = definition.InitialAnimation;
                break;
            case ColourEffectStart.ChoosesHeatAnimationForSuit:
                // The heat shimmer chooses its whole program from the same live suit Samus wears; Gravity before Varia.
                program = PaletteFxHeatAnimations.Start(PaletteFxHeatAnimations.SuitFor(equippedItems));
                break;
            case ColourEffectStart.SkippedOnceAreaMiniBossIsDead:
                if (areaMiniBossDefeated)
                    return false;
                program = definition.InitialAnimation;
                break;
            default:
                throw new InvalidOperationException($"Colour effect {effect} has undefined start {definition.Start}.");
        }

        ColourEffectTurnOrder.Join(effects, new RoomColourEffect(effect, program));
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
                    effect.Restart(PaletteFxAnimations.StartingWith(high.Restart));
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
            if (PaletteFxHeatData.DamageSoundEveryEighthUpdate.IsDue(frameCounter) &&
                samus.Health > PaletteFxHeatData.DamageSoundEnergyThreshold)
            {
                AudioRequests.QueueSound(RoomSound.IncrementalEnergy, PaletteFxHeatData.DamageSoundPriority);
            }
        }

        if (_session.PublishedHeatPhase == _session.ShimmerHeatPhase)
            return;

        _session.ShimmerHeatPhase = _session.PublishedHeatPhase;
        PaletteFxHeatSuit suit = PaletteFxHeatAnimations.SuitFor(equippedItems);
        effect.Restart(PaletteFxHeatAnimations.Shimmer(suit), PaletteFxHeatAnimations.Phase(suit, _session.PublishedHeatPhase));
    }

    /// <summary>Carries out a step of <paramref name="effect"/>'s program that reaches beyond it: a heat phase or a sound.</summary>
    /// <exception cref="InvalidDataException">A sound step names no sound.</exception>
    /// <exception cref="InvalidOperationException">The step is of a kind the effect keeps itself, or of no defined kind.</exception>
    private void CarryOut(RoomColourEffect effect, PaletteFxCue cue)
    {
        switch (cue)
        {
            case PaletteFxCue.PublishHeatPhase publishHeatPhase:
                _session.PublishedHeatPhase = publishHeatPhase.Phase;
                return;
            case PaletteFxCue.QueueSound queueSound:
                AudioRequests.QueueSound(
                    queueSound.Sound ?? throw new InvalidDataException(
                        $"Colour effect {effect.Effect} queued a sound step without a sound."),
                    PaletteFxSoundPriorities.CueSound);
                return;
            default:
                throw new InvalidOperationException($"Colour effect {effect.Effect} handed on step {cue}, which it keeps itself.");
        }
    }

    /// <summary>
    /// Checks one frame a frontend screen's effect shows (#1512). A program names its colours before its first frame, so a
    /// frame shown before that is a defect in the program.
    /// </summary>
    /// <exception cref="InvalidOperationException">The frame is shown before its colours are named, or in colours no screen shows.</exception>
    private static void ShowScreenFrame(RoomColourEffect effect, PaletteFxFrame frame)
    {
        PaletteFxColours animated = effect.Colours ?? throw new InvalidOperationException(
            $"Colour effect {effect.Effect} showed {frame} before its program named the colours it animates.");
        if (!PaletteFxScreenColourDefinitions.FrontendScreenGroups.Contains(animated))
            throw new InvalidOperationException($"Colour effect {effect.Effect} showed {animated}, which no frontend screen shows.");
    }

    /// <summary>
    /// The frames Samus's suit groups showed during the last update, in order (#1512): she lays the shade of each over
    /// her suit, as each frame overwrote her suit's colours then.
    /// </summary>
    internal IReadOnlyList<ShownPaletteFxFrame> SuitShown => suitShown;

    /// <summary>
    /// The frames the enemies' groups showed during the last update, in order (#1512): each enemy lays the shade of
    /// each over its draws, as each frame overwrote its colours then.
    /// </summary>
    internal IReadOnlyList<ShownPaletteFxFrame> EnemyShown => enemyShown;

    /// <summary>
    /// The frame <paramref name="colours"/> show now (#1512, #1597, #1598): the frame laid last on them, in the order
    /// <see cref="Shown"/> lays the looks; null while no effect shows them.
    /// </summary>
    internal PaletteFxFrame? FrameOf(PaletteFxColours colours)
    {
        PaletteFxFrame? last = null;
        foreach (ShownPaletteFxFrame shown in ShownFrames())
        {
            if (shown.Colours == colours)
                last = shown.Frame;
        }
        return last;
    }

    /// <summary>
    /// The looks the colour groups show now, in the order they are laid (#1597, #1598): the settled effects' final frames,
    /// then the running effects' frames in turn order. A look laid later lies over an earlier one on the colours it writes.
    /// </summary>
    internal IReadOnlyList<ShownPaletteFxFrame> Shown => ShownFrames();

    /// <summary>Whether a running effect animates <paramref name="colours"/> (#1597).</summary>
    internal bool Animates(PaletteFxColours colours) => effects.Exists(effect => !effect.Ended && effect.Colours == colours);

    /// <summary>
    /// Lays the shade of each look the room's colour groups show now over the room's pictures (#1512, #1597), in the order
    /// <see cref="Shown"/> gives them: the settled effects' final frames, then the running effects' frames in turn order.
    /// </summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach ((PaletteFxColours colours, PaletteFxFrame shown) in ShownFrames())
        {
            if (PaletteFxRoomColourDefinitions.RoomColours.Contains(colours))
                frame.Shade(GameplayPictures.Room, PaletteFxShades.Of(colours, shown));
        }
    }

    /// <summary>
    /// The looks the colour groups show now (#1597, #1598): each settled effect's final frame in the order they settled,
    /// then each running effect's frame in turn order. An effect its condition ended shows nothing, and no effect's frame
    /// outlasts it.
    /// </summary>
    private List<ShownPaletteFxFrame> ShownFrames()
    {
        var shown = new List<ShownPaletteFxFrame>(settled.Count + effects.Count);
        foreach (RoomColourEffect effect in settled)
        {
            if (effect.Colours is { } colours && effect.Frame is { } frame)
                shown.Add(new ShownPaletteFxFrame(colours, frame));
        }
        foreach (RoomColourEffect effect in effects)
        {
            if (!effect.Ended && effect.Colours is { } colours && effect.Frame is { } frame)
                shown.Add(new ShownPaletteFxFrame(colours, frame));
        }
        return shown;
    }

    /// <summary>
    /// Shows one frame in the colours the effect shows during gameplay (#1512, #1597): the effect holds the frame, which
    /// the room's pictures take as a shade, and a frame of Samus's or an enemy's group is also kept for that owner, which
    /// lays its shade over its own draws. A program names its colours before its first frame, so a frame shown before
    /// that is a defect in the program.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The frame is shown before its colours are named, or in colours no room picture, Samus nor enemy shows.
    /// </exception>
    private void ShowGameplayFrame(RoomColourEffect effect, PaletteFxFrame frame)
    {
        PaletteFxColours animated = effect.Colours ?? throw new InvalidOperationException(
            $"Colour effect {effect.Effect} showed {frame} before its program named the colours it animates.");
        if (PaletteFxRoomColourDefinitions.RoomColours.Contains(animated))
            return;
        if (PaletteFxSamusColourDefinitions.SuitColours.Contains(animated))
        {
            suitShown.Add(new ShownPaletteFxFrame(animated, frame));
            return;
        }
        if (PaletteFxEnemyColourDefinitions.EnemyColours.Contains(animated))
        {
            enemyShown.Add(new ShownPaletteFxFrame(animated, frame));
            return;
        }
        throw new InvalidOperationException($"Colour effect {effect.Effect} showed {animated}, which no gameplay picture shows.");
    }
}
