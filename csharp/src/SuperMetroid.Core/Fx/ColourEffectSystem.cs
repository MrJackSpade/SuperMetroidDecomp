using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;
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
/// A room's pictures load in their standard colours, but many rooms immediately animate some of them with the effects the
/// room's effect starts. Landing Site is one of them: its lightning flashes the scrolling-sky horizon's colours.
/// </para>
/// <para>
/// The effects take their turns in the order they started (<see cref="effects"/>), which Samus's heat shimmer
/// (<see cref="ColourEffectId.SamusInHeat"/>) observes. Any number of effects run at once.
/// </para>
/// </remarks>
[RoomLifetime]
public sealed partial class ColourEffectSystem : IGameplayShadingSource
{
    /// <summary>
    /// The running effects, in turn order: the order they started (#1592, #1598), an effect that starts joining after every
    /// running effect. The order is observable through Samus's heat shimmer: started first in every heated room, it takes
    /// its turn before the room's heat glow that publishes the heat phase, so the shimmer, which restarts her suit's pulse
    /// on that phase, follows a new phase one update late. The effects whose turns gameplay observes, the heat shimmer
    /// that burns Samus, the heat glow that paces it and the beacon that sounds, are all started by the room's effect as
    /// the room loads and run until the room ends.
    /// </summary>
    internal readonly List<RoomColourEffect> effects = [];
    /// <summary>
    /// The effects whose animations ran to their end (#1597), in the order they settled: each keeps its final frame on its
    /// colours, without taking a turn, until an effect showing the same colours settles after it or the room ends.
    /// </summary>
    private readonly List<RoomColourEffect> settled = [];
    /// <summary>
    /// The frames Samus's suit groups, the escape's shutter flash and the effects enemies show showed during the current
    /// update, in order (#1512, #1783, #2544).
    /// </summary>
    private readonly ColourEffectFramesOnOwners shownOnOwners = new();
    /// <summary>Whether the effects run at all and the heat phases, which outlive each room's effects.</summary>
    private readonly ColourEffectSessionState _session;

    /// <summary>
    /// The sound requests the colour effects' sounds join as they make them (#2057, #2544): the update's of the runtime
    /// running them, given as the room's effects are built. A frontend screen's effects make no sound
    /// (<see cref="ForScreen"/>).
    /// </summary>
    private readonly GameplaySoundRequests sounds;

    /// <summary>
    /// Creates one room's colour effects, none running yet, over the session's lasting state, their sounds joining
    /// <paramref name="sounds"/>.
    /// </summary>
    /// <param name="session">Whether the effects run and the heat phases, which room loading never clears.</param>
    /// <param name="sounds">The update's sound requests of the runtime running the effects.</param>
    internal ColourEffectSystem(ColourEffectSessionState session, GameplaySoundRequests sounds)
    {
        _session = Ensure.NotNull(session);
        this.sounds = Ensure.NotNull(sounds);
    }

    /// <summary>
    /// A frontend screen's colour effects, none running yet (#2223, #2544): a screen shares no session with gameplay, and
    /// its effects make no sound, which <see cref="StartScreen"/> checks of each, so the effects have no sounds to join.
    /// </summary>
    internal static ColourEffectSystem ForScreen() =>
        new(new ColourEffectSessionState(), new GameplaySoundRequests(new ScreenWithoutSounds()));

    /// <summary>Whether the colour effects run at all, checked first each update; it lives on the session.</summary>
    public bool ColourEffectsRun => _session.ColourEffectsRun;

    /// <summary>
    /// Starts an effect outside the room's effect during gameplay: Samus's load appearance, a background statue's grey-out
    /// or the escape's flashes. It must start: the start is rejected loudly when it does not. A frontend screen starts its
    /// effects with <see cref="StartScreen"/>, and an effect an enemy shows starts with that enemy
    /// (<see cref="StartShownOn"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The effect did not start: its start skips it, or an enemy shows it.</exception>
    public void Start(
        ColourEffectId effect,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated = false)
    {
        if (!TryStart(effect, equippedItems, areaMiniBossDefeated, ColourEffectTarget.OfGameplay(effect)))
            throw new InvalidOperationException($"Colour effect {effect} could not start.");
    }

    /// <summary>
    /// Starts an effect whose frames <paramref name="enemy"/> shows on its draws (#2288): a sprite statue's grey-out, handed
    /// the statue it greys, as the gunship's glow is handed the gunship (#1783). It must start: the start is rejected loudly
    /// when it does not.
    /// </summary>
    /// <exception cref="InvalidOperationException">The effect did not start: its start skips it, or no enemy shows it.</exception>
    internal void StartShownOn(ColourEffectId effect, IGlowingEnemy enemy, SamusEquipmentFlags equippedItems)
    {
        if (!TryStart(effect, equippedItems, areaMiniBossDefeated: false, new ColourEffectTarget.OnGlowingEnemy(enemy)))
            throw new InvalidOperationException($"Colour effect {effect} could not start.");
    }

    /// <summary>
    /// Starts a frontend screen's effect after the running effects (#2223), as <see cref="StepScreen"/> steps it: a screen
    /// has no Samus, no equipment, no Torizo and no area, so the start hands the effect none of them. Every effect a screen
    /// starts plays its own animation from its start and watches nothing.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The effect's start chooses from Samus's equipment or the area's mini-boss, the effect watches a condition, which only
    /// gameplay has, makes a sound, which no screen's effect makes, or shows colours no frontend screen shows.
    /// </exception>
    internal void StartScreen(ColourEffectId effect)
    {
        ColourEffectDefinition definition = ColourEffectAnimations.Definition(effect);
        if (definition.Start != ColourEffectStart.Plain)
            throw new InvalidOperationException($"Colour effect {effect} starts by {definition.Start}, which no frontend screen has.");
        if (definition.Condition is not ColourEffectCondition.WatchesNothing)
            throw new InvalidOperationException($"Colour effect {effect} watches {definition.Condition}, which no frontend screen has.");
        if (ColourEffectShownFrames.Steps(effect).Any(beat => beat is AnimationBeat<int, ColourEffectCue>.Event { Raised: ColourEffectCue.QueueSound }))
            throw new InvalidOperationException($"Colour effect {effect} makes a sound, which no frontend screen's effect makes.");
        effects.Add(new RoomColourEffect(effect, framesOf: effect, ColourEffectTarget.ScreenPictures));
    }

    /// <summary>
    /// Starts the glow an enemy's placement started (#1733): the glow keeps <paramref name="start"/>'s enemy and shows each
    /// of its frames on that enemy for as long as it runs (#1783), which is until the room's colour effects are cleared. It
    /// must start: the start is rejected loudly when it does not.
    /// </summary>
    /// <exception cref="InvalidOperationException">The glow did not start: its start skips it.</exception>
    internal void StartEnemyGlow(EnemyGlowStart start, SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        Ensure.NotNull(start);
        if (!TryStart(start.Glow, equippedItems, areaMiniBossDefeated: false, new ColourEffectTarget.OnGlowingEnemy(start.Enemy)))
            throw new InvalidOperationException($"Enemy glow {start.Glow} could not start.");
    }

    /// <summary>
    /// Starts a Torizo's belly glow (#1700), which watches <paramref name="torizo"/>, the Torizo whose fight start asked for
    /// it, and ends on the first update that Torizo has no health left. It must start: the start is rejected loudly when it
    /// does not.
    /// </summary>
    /// <exception cref="InvalidOperationException">The glow did not start: its start skips it.</exception>
    internal void StartTorizoBellyGlow(
        ColourEffectId glow,
        ILitTorizo torizo,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems)
    {
        // The glow shows on the Torizo that lit it (#2240), as the gunship's shows on the gunship (#1783).
        if (!TryStart(glow, equippedItems, areaMiniBossDefeated: false, new ColourEffectTarget.OnLitTorizo(torizo)))
            throw new InvalidOperationException($"Torizo belly glow {glow} could not start.");
    }

    /// <summary>
    /// Builds one room's colour effects: exactly the colour effects the room's effect starts, their sounds joining
    /// <paramref name="sounds"/>.
    /// </summary>
    /// <param name="session">Whether the effects run and the heat phases, which room loading never clears.</param>
    /// <param name="fx">The room state's effect selection, from which <paramref name="door"/> picks the room's effect.</param>
    /// <param name="door">The entry door, which selects the room's effect.</param>
    /// <param name="equippedItems">Samus's equipment, which selects suit-dependent animations.</param>
    /// <param name="areaMiniBossDefeated">Whether the area's mini-boss is dead.</param>
    /// <param name="sounds">The update's sound requests of the runtime running the effects.</param>
    internal static ColourEffectSystem ForRoom(
        ColourEffectSessionState session,
        RoomFxSelection? fx,
        DoorDefinition? door,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated,
        GameplaySoundRequests sounds) =>
        ForEffect(session, fx, door, equippedItems, areaMiniBossDefeated, null, sounds);

    /// <summary>Builds one room's colour effects from the colour effects the selected or supplied room effect starts.</summary>
    /// <param name="session">Whether the effects run and the heat phases, which room loading never clears.</param>
    /// <param name="fx">The room state's effect selection, from which <paramref name="door"/> picks the room's effect.</param>
    /// <param name="door">The entry door, which selects the room's effect.</param>
    /// <param name="equippedItems">Samus's equipment, which selects suit-dependent animations.</param>
    /// <param name="areaMiniBossDefeated">Whether the area's mini-boss is dead.</param>
    /// <param name="suppliedEffect">A room effect to use instead of the door's selection.</param>
    /// <param name="sounds">The update's sound requests of the runtime running the effects.</param>
    internal static ColourEffectSystem ForEffect(ColourEffectSessionState session, RoomFxSelection? fx, DoorDefinition? door,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems, bool areaMiniBossDefeated,
        RoomEffectDefinition? suppliedEffect, GameplaySoundRequests sounds)
    {
        var system = new ColourEffectSystem(session, sounds);
        if ((suppliedEffect ?? fx?.Select(door)) is { } roomEffect)
        {
            // Colour effects and block animations have separate owners and lists; the room effect's colour effects start in its order.
            foreach (ColourEffectId effect in roomEffect.ColourEffects)
                system.TryStart(effect, equippedItems, areaMiniBossDefeated, ColourEffectTarget.OfGameplay(effect));
        }
        return system;
    }

    /// <summary>Runs one update of the colour effects during gameplay, each in its turn.</summary>
    /// <remarks>
    /// No colour is written (#1512, #1597): each effect shows the frame it holds now, which the room's pictures take as a
    /// shade, and the frames of Samus's and the enemies' groups are handed to their owners through
    /// <see cref="SuitShown"/>, <see cref="ShutterFlashShown"/> and <see cref="GlowsShown"/>, whose colourings the import
    /// pre-colours per frame.
    /// </remarks>
    /// <param name="conditions">The update's conditions the effects watch.</param>
    /// <exception cref="InvalidOperationException">A frame is shown in colours no gameplay picture shows.</exception>
    public void Step(ColourEffectInputs conditions)
    {
        Ensure.NotNull(conditions);
        RunTurns(effect => CheckCondition(effect, conditions));
    }

    /// <summary>
    /// Runs one update of a frontend screen's colour effects (#1512, #1597, #2199): each effect shows the frame it holds
    /// now, which the screen shades its pictures with, and no owner's colours are written. A screen has no Samus, no
    /// equipment and no area, so it hands its effects no conditions: none of the effects a screen starts
    /// (<see cref="StartScreen"/>) watches one.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// A frame is shown in colours no frontend screen shows, or an effect watches a condition, which only gameplay has.
    /// </exception>
    internal void StepScreen()
    {
        RunTurns(RefuseScreenCondition);
    }

    /// <summary>
    /// Gives each running effect its turn, in the order they started: <paramref name="checkCondition"/> checks its
    /// condition, it counts down, and when its next step is due runs its steps to the next frame, which it shows on its
    /// target (#2544). Effects that end are dropped from the turns once every turn has run; one that settled keeps its
    /// final frame shown.
    /// </summary>
    private void RunTurns(Action<RoomColourEffect> checkCondition)
    {
        shownOnOwners.Clear();

        foreach (RoomColourEffect effect in effects)
        {
            checkCondition(effect);
            if (effect.Ended)
                continue;
            // A frame of an effect an owner shows is kept for that owner, which lays its shade over its own draws.
            if (effect.TakeTurn(cue => CarryOut(effect, cue)) is { } frame && effect.Target is ColourEffectTarget.OnOwner owner)
                owner.Show(new ShownColourEffectFrame(effect.Colours, frame), shownOnOwners);
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
    /// <param name="effect">The effect to start.</param>
    /// <param name="equippedItems">Samus's equipment, which selects suit-dependent animations.</param>
    /// <param name="areaMiniBossDefeated">Whether the area's mini-boss is dead.</param>
    /// <param name="target">What the effect shows its frames on (#2544), which accepts only an effect showing its colours.</param>
    /// <returns>Whether the effect started.</returns>
    /// <exception cref="InvalidOperationException">The effect names a start of no defined kind, or its target shows none of its colours.</exception>
    private bool TryStart(
        ColourEffectId effect,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        bool areaMiniBossDefeated,
        ColourEffectTarget target)
    {
        ColourEffectDefinition definition = ColourEffectAnimations.Definition(effect);
        ColourEffectId framesOf;
        switch (definition.Start)
        {
            case ColourEffectStart.Plain:
                framesOf = effect;
                break;
            case ColourEffectStart.ChoosesHeatAnimationForSuit:
                // The heat shimmer plays the pulse of the same live suit Samus wears, in its frames; Gravity before Varia.
                framesOf = ColourEffectHeatAnimations.Pulse(ColourEffectHeatAnimations.SuitFor(equippedItems));
                break;
            case ColourEffectStart.SkippedOnceAreaMiniBossIsDead:
                if (areaMiniBossDefeated)
                    return false;
                framesOf = effect;
                break;
            default:
                throw new InvalidOperationException($"Colour effect {effect} has undefined start {definition.Start}.");
        }

        effects.Add(new RoomColourEffect(effect, framesOf, target));
        return true;
    }

    /// <summary>
    /// Checks the condition <paramref name="effect"/> watches, before its countdown runs: from the update after its first
    /// frame (<see cref="RoomColourEffect.WatchedCondition"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The effect watches a condition of no defined kind.</exception>
    private void CheckCondition(RoomColourEffect effect, ColourEffectInputs conditions)
    {
        switch (effect.WatchedCondition)
        {
            case ColourEffectCondition.WatchesNothing:
                return;
            case ColourEffectCondition.EndsWhenTorizoDies:
                // An effect watching for a Torizo's defeat lights that Torizo, its target (ColourEffectTarget.OnLitTorizo.Accept).
                if (effect.Target is ColourEffectTarget.OnLitTorizo { Torizo.IsDefeated: true })
                    effect.End();
                return;
            case ColourEffectCondition.EndsWhenAreaMiniBossDies:
                if (conditions.AreaMiniBossDefeated)
                    effect.End();
                return;
            case ColourEffectCondition.RestartsWhileSamusIsHigh high:
                if (conditions.SamusY < high.Height)
                    effect.RestartCycle();
                return;
            case ColourEffectCondition.BurnsSamusInHeat:
                BurnSamusInHeat(effect, conditions.EquippedItems, conditions.Samus, conditions.FrameCounter);
                return;
            default:
                throw new InvalidOperationException($"Colour effect {effect.Effect} watches undefined condition {effect.Condition}.");
        }
    }

    /// <summary>
    /// Checks that a frontend screen's <paramref name="effect"/> watches no condition (#2199): the conditions are Samus's,
    /// her equipment's, a Torizo's and the area mini-boss's, none of which a screen has.
    /// </summary>
    /// <exception cref="InvalidOperationException">The effect watches a condition.</exception>
    private static void RefuseScreenCondition(RoomColourEffect effect)
    {
        if (effect.WatchedCondition is not ColourEffectCondition.WatchesNothing)
            throw new InvalidOperationException($"Colour effect {effect.Effect} watches {effect.WatchedCondition}, which no frontend screen has.");
    }

    /// <summary>
    /// The heat shimmer's condition. The shimmer owns both its look and the heat's damage: an unprotected Samus takes the
    /// heat's fractional damage each update, and the shimmer restarts on the heat phase the Norfair glow
    /// published, which that glow, taking its turn after the shimmer, publishes for the following update.
    /// </summary>
    private void BurnSamusInHeat(
        RoomColourEffect effect,
        SuperMetroid.Core.Progression.SamusEquipmentFlags equippedItems,
        IRoomSamusAccess samus,
        int frameCounter)
    {
        bool protectedFromHeat = equippedItems.HasAny(
            SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit);
        if (!protectedFromHeat)
        {
            IRoomSamus burned = samus.Current;
            burned.TakePeriodicDamage(ColourEffectHeatData.DamagePerUpdate);
            if (ColourEffectHeatData.HeatDamageSound.IsDue(frameCounter) &&
                burned.Health > ColourEffectHeatData.DamageSoundEnergyThreshold)
            {
                sounds.QueueSound(RoomSound.IncrementalEnergy, ColourEffectHeatData.DamageSoundPriority);
            }
        }

        if (_session.PublishedHeatPhase == _session.ShimmerHeatPhase)
            return;

        _session.ShimmerHeatPhase = _session.PublishedHeatPhase;
        ColourEffectHeatSuit suit = ColourEffectHeatAnimations.SuitFor(equippedItems);
        effect.Restart(ColourEffectHeatAnimations.Pulse(suit), ColourEffectHeatAnimations.Phase(suit, _session.PublishedHeatPhase));
    }

    /// <summary>Carries out a step of <paramref name="effect"/>'s animation that reaches beyond it: a heat phase or a sound.</summary>
    /// <exception cref="InvalidOperationException">The step is of no defined kind.</exception>
    private void CarryOut(RoomColourEffect effect, ColourEffectCue cue)
    {
        switch (cue)
        {
            case ColourEffectCue.PublishHeatPhase publishHeatPhase:
                _session.PublishedHeatPhase = publishHeatPhase.Phase;
                return;
            case ColourEffectCue.QueueSound queueSound:
                sounds.QueueSound(queueSound.Sound, ColourEffectSoundPriorities.CueSound);
                return;
            default:
                throw new InvalidOperationException($"Colour effect {effect.Effect} raised undefined step {cue}.");
        }
    }

    /// <summary>
    /// The frames Samus's suit groups showed during the last update, in order (#1512): she shows each frame's shade
    /// over her suit in turn.
    /// </summary>
    internal IReadOnlyList<ShownColourEffectFrame> SuitShown => shownOnOwners.Suit;

    /// <summary>
    /// The frames the escape's shutter flash showed during the last update, in order (#1512, #2288): the room's
    /// destroyable shutters show each frame's shade over their draws in turn.
    /// </summary>
    internal IReadOnlyList<ColourEffectFrame> ShutterFlashShown => shownOnOwners.ShutterFlash;

    /// <summary>
    /// The frames the effects enemies show showed during the last update, in order (#1783, #2288): each is shown by the enemy
    /// its start handed it, which lays its shade over that enemy's draws.
    /// </summary>
    internal IReadOnlyList<ShownEnemyGlow> GlowsShown => shownOnOwners.Glows;

    /// <summary>
    /// The frame <paramref name="colours"/> show now (#1512, #1597, #1598): the frame laid last on them, in the order
    /// <see cref="Shown"/> lays the looks; null while no effect shows them.
    /// </summary>
    internal ColourEffectFrame? FrameOf(ColourEffectColours colours)
    {
        ColourEffectFrame? last = null;
        foreach (ShownColourEffectFrame shown in ShownFrames())
        {
            if (shown.Colours == colours)
                last = shown.Frame;
        }
        return last;
    }

    /// <summary>
    /// The looks the colour groups show now, in the order they are laid (#1597, #1598): the settled effects' final frames,
    /// then the running effects' frames in turn order. A look laid later lies over an earlier one on the colours it
    /// shows.
    /// </summary>
    internal IReadOnlyList<ShownColourEffectFrame> Shown => ShownFrames();

    /// <summary>Whether a running effect animates <paramref name="colours"/> (#1597).</summary>
    internal bool Animates(ColourEffectColours colours) => effects.Exists(effect => effect.AnimatesColours && effect.Colours == colours);

    /// <summary>
    /// Lays the shade of each look the effects showing on the room's pictures show now over them (#1512, #1597, #2544), in
    /// the order <see cref="Shown"/> gives them: the settled effects' final frames, then the running effects' frames in turn
    /// order.
    /// </summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (ShowingEffect showing in ShowingEffects())
        {
            if (showing.Effect.Target == ColourEffectTarget.RoomPictures)
                frame.Shade(GameplayPictures.Room, ColourEffectShades.Of(showing.Effect.Colours, showing.Frame));
        }
    }

    /// <summary>
    /// The looks the colour groups show now (#1597, #1598): each settled effect's final frame in the order they settled,
    /// then each running effect's frame in turn order. An effect its condition ended shows nothing, and no effect's frame
    /// outlasts it.
    /// </summary>
    private List<ShownColourEffectFrame> ShownFrames() =>
        [.. ShowingEffects().Select(showing => new ShownColourEffectFrame(showing.Effect.Colours, showing.Frame))];

    /// <summary>
    /// Each effect that shows a look now with the frame it shows, in the order the looks are laid (#1597, #1598): each settled
    /// effect's final frame in the order they settled, then each running effect's frame in turn order.
    /// </summary>
    private IEnumerable<ShowingEffect> ShowingEffects()
    {
        foreach (RoomColourEffect effect in settled)
        {
            if (effect.Frame is { } frame)
                yield return new ShowingEffect(effect, frame);
        }
        foreach (RoomColourEffect effect in effects)
        {
            if (!effect.Ended && effect.Frame is { } frame)
                yield return new ShowingEffect(effect, frame);
        }
    }

    /// <summary>An effect that shows a look now, with the frame it shows.</summary>
    /// <param name="Effect">The effect.</param>
    /// <param name="Frame">The frame it shows.</param>
    private readonly partial record struct ShowingEffect(RoomColourEffect Effect, ColourEffectFrame Frame);

    /// <summary>
    /// The sound queues of a frontend screen's colour effects, which make no sound (#2544): <see cref="StartScreen"/> starts
    /// no effect that raises one, so no request ever reaches these.
    /// </summary>
    private sealed partial class ScreenWithoutSounds : IGameplaySoundQueue
    {
        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">Always: a frontend screen's colour effect made a sound, which none makes.</exception>
        public SoundQueueAdmission QueueSound(SoundRequest request) =>
            throw new InvalidOperationException($"A frontend screen's colour effect requested {request}, which no screen's effect makes.");
    }

}
