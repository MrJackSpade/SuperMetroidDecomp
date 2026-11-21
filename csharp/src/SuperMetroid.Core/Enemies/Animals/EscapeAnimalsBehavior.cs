using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The escape animals species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EscapeAnimalsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    /// <summary>An escaping Etecoon, from its settings, or the escaping Dachora (#2146).</summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EscapeEtecoonSpawnSettings>([EnemyDefinitionId.EtecoonEscape], (occupant, settings, place, common) =>
            EnemyOccupant.Running(new EscapeEtecoonEntity(this, occupant, place, common, settings)));
        registrar.Spawns<EnemySpawnSettings>([EnemyDefinitionId.DachoraEscape], (occupant, _, place, common) =>
            EnemyOccupant.PlacedOnly(new EscapeDachoraEntity(this, occupant, place, common)));
    }

    /// <summary>Provides room events and live Samus state used by both escape-animal branches.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Runs Etecoon's slope-ignoring horizontal move and settling ground probe.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds escape-animal AI to room events and terrain collision for the current enemy system.</summary>
    internal EscapeAnimalsBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Initializes the escape Etecoon.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal EscapeEtecoonEnemyState InitializeEscapeEtecoon(EscapeEtecoonSpawnSettings escapeEtecoonSpawnSettings, RoomEnemyPlace place)
    {
        EscapeEtecoonInitialization initialization =
            EscapeEtecoons.Initialization(escapeEtecoonSpawnSettings.Role);
        var state = new EscapeEtecoonEnemyState(initialization.Action);

        // The critters-escaped event means the animals already escaped on an earlier visit, so the animal leaves the
        // room before it shows, moves or acts.
        if (HasCrittersEscaped())
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return state;
        }

        // An escaping animal runs its animation, is left out of the ordinary touch pass against Samus and
        // stands solid to her.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus | EnemyContact.SolidToSamus);
        place.AnimationClock.Restart();

        place.Position = place.Position.WithWhole(initialization.Position);
        state.Playback = new(EscapeEtecoonAnimations.StartingWith(initialization.Animation), AnimationPlace.StartOfAnimation);
        state.StartWalking(initialization.Direction, initialization.Speed);
        return state;
    }

    /// <summary>Initializes the escape Dachora.</summary>
    /// <returns>The state the Dachora holds from its setup.</returns>
    internal EscapeDachoraEnemyState InitializeEscapeDachora(RoomEnemyPlace place)
    {
        var state = new EscapeDachoraEnemyState();
        if (HasCrittersEscaped())
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return state;
        }

        // Dachora's placement already animates and keeps it from touching Samus; the setup makes it animate again.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.Restart();
        state.Playback = new(EscapeDachoraAnimations.RunningAroundLowTide, AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>Runs an escaping Etecoon's action.</summary>
    private void RunEscapeEtecoonMain(
        RoomEnemyPlace place,
        EscapeEtecoonEnemyState state,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case EscapeEtecoonAction.ExpressingGratitude:
                return;

            case EscapeEtecoonAction.Escaping:
                AddEscapeAnimalX(place, EscapeAnimalMotionDefinitions.EtecoonExitSpeed);
                return;

            case EscapeEtecoonAction.AwaitingEscape:
                // The wait changes only the animation. It deliberately leaves the
                // existing frame hold intact, so the new animation begins on the same
                // update boundary the game observes.
                if (HasCrittersEscaped())
                    state.Playback = new(EscapeEtecoonAnimations.StartingWith(EscapeEtecoonSegment.ExpressGratitudeThenEscape), AnimationPlace.StartOfAnimation);
                return;

            case EscapeEtecoonAction.Walking:
                RunEscapeEtecoonWalking(place, state, level);
                return;

            default:
                throw new InvalidDataException($"Escape Etecoon has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Walks and falls, including the collision-only event branch.</summary>
    private void RunEscapeEtecoonWalking(
        RoomEnemyPlace place,
        EscapeEtecoonEnemyState state,
        RoomLevelData level)
    {
        // The animal moves horizontally by its speed, ignoring slopes that are not square. The event test is made only
        // when that move collides, so it never turns an animal around in open space.
        SignedSixteenSixteen horizontalDisplacement = state.Velocity.ToSixteenSixteenDelta();
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontalDisplacement))
        {
            place.AnimationClock.StepOnNextUpdate();
            state.TurnAround();
            state.Playback = new(EscapeEtecoonAnimations.StartingWith(HasCrittersEscaped()
                ? EscapeEtecoonSegment.RunningForEscape
                : state.Direction == HorizontalDirection.Left
                    ? EscapeEtecoonSegment.RunningLeftLowTide
                    : EscapeEtecoonSegment.RunningRightLowTide), AnimationPlace.StartOfAnimation);
        }

        // The animal moves down by its gravity even after meeting a wall, whether or not it meets the floor: the
        // one-pixel move settles it on the room's floor.
        _ = _terrain.MoveEnemyVertically(level, place, EscapeAnimalMotionDefinitions.EtecoonGravity);
    }

    /// <summary>Runs an escaping Etecoon's typed animation where the shared enemy animation would.</summary>
    internal static void AdvanceEscapeEtecoonTypedAnimation(RoomEnemyPlace place, EscapeEtecoonEnemyState state, EnemyAnimationCueContext context)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<EscapeEtecoonSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, EscapeEtecoonAnimations.Segments, ref playback,
            cue => ApplyEscapeEtecoonCue(place, state, context.Samus, cue) is { } target
                ? new AnimationContinuation<EscapeEtecoonSegment>(EscapeEtecoonAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Runs the escaping Dachora's typed animation where the shared enemy animation would.</summary>
    internal void AdvanceEscapeDachoraTypedAnimation(RoomEnemyPlace place, EscapeDachoraEnemyState state, EnemyAnimationCueContext context)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<EscapeDachoraSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, EscapeDachoraAnimations.Segments, ref playback,
            cue => ApplyEscapeDachoraCue(place, context.Samus, cue));
        state.Playback = playback;
    }

    /// <summary>Carries out one cue of an Etecoon's animation, returning the segment a branch continues at, or null to run on.</summary>
    private static EscapeEtecoonSegment? ApplyEscapeEtecoonCue(RoomEnemyPlace place, EscapeEtecoonEnemyState state, SamusState samus, EscapeEtecoonCue cue)
    {
        switch (cue)
        {
            case EscapeEtecoonCue.Signal { Kind: EscapeEtecoonCueKind.Escapes }:
                state.Escape();
                return null;
            case EscapeEtecoonCue.Signal { Kind: EscapeEtecoonCueKind.ExpressesGratitude }:
                state.ExpressGratitude();
                return null;
            case EscapeEtecoonCue.LowTideRunStarts lowTide:
                return IsAcidLow(samus) ? null : HighTideRun(lowTide.Running);
            case EscapeEtecoonCue.Move move:
                place.Position = place.Position.WithWholeX(place.Position.X.Whole + move.Offset);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EscapeEtecoonCue)} {cue}.");
        }
    }

    /// <summary>The run an escaping Etecoon starts the way it runs once the rising acid is high (#2167).</summary>
    private static EscapeEtecoonSegment HighTideRun(HorizontalDirection running) =>
        running == HorizontalDirection.Left ? EscapeEtecoonSegment.RunningLeftHighTide : EscapeEtecoonSegment.RunningRightHighTide;

    /// <summary>
    /// Carries out one cue of the Dachora's animation, returning where the rising tide or the critters' escape takes the
    /// play at a lap's end, or null to run on.
    /// </summary>
    private AnimationContinuation<EscapeDachoraSegment>? ApplyEscapeDachoraCue(RoomEnemyPlace place, SamusState samus, EscapeDachoraCue cue)
    {
        switch (cue)
        {
            case EscapeDachoraCue.Move move:
                place.Position = place.Position.WithWholeX(place.Position.X.Whole + move.Offset);
                return null;
            case EscapeDachoraCue.LowTideLapEnds lowTide:
                return IsAcidLow(samus) ? null : RisingTideTakesOver(lowTide.Lap);
            case EscapeDachoraCue.LapEnds lap:
                return HasCrittersEscaped() ? EscapeTakesOver(lap.Lap) : null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EscapeDachoraCue)} {cue}.");
        }
    }

    /// <summary>
    /// Where the rising tide takes the escaping Dachora at the end of a low-tide lap the way <paramref name="lap"/> ran
    /// (#2167): the end of the high-tide lap that way, counting on the laps the low-tide run had left.
    /// </summary>
    private static AnimationContinuation<EscapeDachoraSegment> RisingTideTakesOver(HorizontalDirection lap) =>
        lap == HorizontalDirection.Left ? EscapeDachoraAnimations.HighTideLapsAfterLeftLap : EscapeDachoraAnimations.HighTideLapsAfterRightLap;

    /// <summary>
    /// Where the critters' escape takes the escaping Dachora at the end of a lap the way <paramref name="lap"/> ran (#2167):
    /// the run for the escape, from its pause after a lap to the left and from its acceleration after one to the right.
    /// </summary>
    private static AnimationContinuation<EscapeDachoraSegment> EscapeTakesOver(HorizontalDirection lap) =>
        lap == HorizontalDirection.Left ? EscapeDachoraAnimations.EscapeAfterLeftLap : EscapeDachoraAnimations.EscapeAfterRightLap;

    /// <summary>Whether the critters have escaped, which both escaping animals read.</summary>
    private bool HasCrittersEscaped() => _room.Services.Events.Has(StoryEvent.CrittersEscaped);

    /// <summary>
    /// Whether the escaping animals find the acid low: at or below
    /// <see cref="EscapeAnimalMotionDefinitions.LowAcidSurfaceY"/>, or absent from Samus's room.
    /// </summary>
    private static bool IsAcidLow(SamusState samus) =>
        samus.LiquidPhysics.LavaAcidSurfaceY is not int surface || surface >= EscapeAnimalMotionDefinitions.LowAcidSurfaceY;

    /// <summary>Moves the animal horizontally by <paramref name="displacement"/>.</summary>
    private static void AddEscapeAnimalX(RoomEnemyPlace place, SignedSixteenSixteen displacement) =>
        place.Position = place.Position.WithX(place.Position.X.MovedBy(displacement));

    /// <summary>Runs the escape Etecoon's main AI.</summary>
    internal void RunEscapeEtecoonMainAi(RoomEnemyPlace place, EscapeEtecoonEnemyState escapeEtecoonEnemyState, IEnemyAiFrame frame) =>
        RunEscapeEtecoonMain(place, escapeEtecoonEnemyState, frame.Level);
}
