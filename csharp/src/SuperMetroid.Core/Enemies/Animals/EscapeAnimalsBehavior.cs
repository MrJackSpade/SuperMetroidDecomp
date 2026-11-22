using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The escape animals species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class EscapeAnimalsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    /// <summary>An escaping Etecoon, from its settings, or the escaping Dachora (#2146).</summary>
    /// <param name="registrar">The room's registry being composed.</param>
    public void OfferCreations(EnemySpeciesRegistrar registrar)
    {
        Ensure.NotNull(registrar).Spawns<EscapeEtecoonSpawnSettings>([EnemyDefinitionId.EtecoonEscape], CreateEtecoon);
        registrar.Spawns<EnemySpawnSettings>([EnemyDefinitionId.DachoraEscape], CreateDachora);
    }

    /// <summary>An escaping Etecoon, from its settings.</summary>
    private EnemyOccupant CreateEtecoon(RoomEnemyOccupant occupant, EscapeEtecoonSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.Running(new EscapeEtecoonEntity(this, occupant, place, common, settings));

    /// <summary>The escaping Dachora.</summary>
    private EnemyOccupant CreateDachora(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        EnemyOccupant.PlacedOnly(new EscapeDachoraEntity(this, occupant, place, common));

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

    /// <summary>What <see cref="RunEscapeEtecoonMain"/> does for each <see cref="EscapeEtecoonAction"/> member, each member's own arm.</summary>
    [field: NonSerialized]
    private IdentityTable<EscapeEtecoonAction, Action<RoomEnemyPlace, EscapeEtecoonEnemyState, RoomLevelData>> RunEscapeEtecoonMainForEscapeEtecoonAction => field ??= new(new Dictionary<EscapeEtecoonAction, Action<RoomEnemyPlace, EscapeEtecoonEnemyState, RoomLevelData>>
    {
        [EscapeEtecoonAction.ExpressingGratitude] = (place, state, level) => { },
        [EscapeEtecoonAction.Escaping] = (place, state, level) => AddEscapeAnimalX(place, EscapeAnimalMotionDefinitions.EtecoonExitSpeed),
        [EscapeEtecoonAction.AwaitingEscape] = (place, state, level) =>
        {
            // The wait changes only the animation. It deliberately leaves the
            // existing frame hold intact, so the new animation begins on the same
            // update boundary the game observes.
            if (HasCrittersEscaped())
                state.Playback = new(EscapeEtecoonAnimations.StartingWith(EscapeEtecoonSegment.ExpressGratitudeThenEscape), AnimationPlace.StartOfAnimation);
        },
        [EscapeEtecoonAction.Walking] = (place, state, level) => RunEscapeEtecoonWalking(place, state, level),
    });

    /// <summary>Runs an escaping Etecoon's action.</summary>
    private void RunEscapeEtecoonMain(
        RoomEnemyPlace place,
        EscapeEtecoonEnemyState state,
        RoomLevelData level)
    {
        RunEscapeEtecoonMainForEscapeEtecoonAction[state.Action](place, state, level);
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

    /// <summary>What <see cref="ApplyEscapeEtecoonCue"/> comes to for each <see cref="EscapeEtecoonCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<EscapeEtecoonCueKind, Func<EscapeEtecoonEnemyState, EscapeEtecoonSegment?>> ApplyEscapeEtecoonCueForEscapeEtecoonCueKind => field ??= new(new Dictionary<EscapeEtecoonCueKind, Func<EscapeEtecoonEnemyState, EscapeEtecoonSegment?>>
    {
        [EscapeEtecoonCueKind.Escapes] = static state =>
        {
            state.Escape();
            return null;
        },
        [EscapeEtecoonCueKind.ExpressesGratitude] = static state =>
        {
            state.ExpressGratitude();
            return null;
        },
    });

    /// <summary>Carries out one cue of an Etecoon's animation, returning the segment a branch continues at, or null to run on.</summary>
    private static EscapeEtecoonSegment? ApplyEscapeEtecoonCue(RoomEnemyPlace place, EscapeEtecoonEnemyState state, SamusState samus, EscapeEtecoonCue cue)
    {
        return cue.Match(ApplyEscapeEtecoonCueCases, new ApplyEscapeEtecoonCueContext(samus, place, state));
    }

    /// <summary>What <see cref="ApplyEscapeEtecoonCue"/> does on each <see cref="EscapeEtecoonCue"/>, built once (#2553).</summary>
    private static EscapeEtecoonCueCases<ApplyEscapeEtecoonCueContext, EscapeEtecoonSegment?> ApplyEscapeEtecoonCueCases { get; } = new(
        LowTideRunStarts: static (lowTide, context) => IsAcidLow(context.Samus) ? null : HighTideRun(lowTide.Running),
        Move: static (move, context) =>
        {
            context.Place.Position = context.Place.Position.WithWholeX(context.Place.Position.X.Whole + move.Offset);
            return null;
        },
        Signal: static (signal, context) =>
        {
            return ApplyEscapeEtecoonCueForEscapeEtecoonCueKind[signal.Kind](context.State);
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Samus">The samus the cases read.</param>
    /// <param name="Place">The place the cases read.</param>
    /// <param name="State">The state the cases read.</param>
    private readonly partial record struct ApplyEscapeEtecoonCueContext(SamusState Samus, RoomEnemyPlace Place, EscapeEtecoonEnemyState State);

    /// <summary>The run an escaping Etecoon starts the way it runs once the rising acid is high (#2167).</summary>
    internal static EscapeEtecoonSegment HighTideRun(HorizontalDirection running) =>
        running == HorizontalDirection.Left ? EscapeEtecoonSegment.RunningLeftHighTide : EscapeEtecoonSegment.RunningRightHighTide;

    /// <summary>
    /// Carries out one cue of the Dachora's animation, returning where the rising tide or the critters' escape takes the
    /// play at a lap's end, or null to run on.
    /// </summary>
    private AnimationContinuation<EscapeDachoraSegment>? ApplyEscapeDachoraCue(RoomEnemyPlace place, SamusState samus, EscapeDachoraCue cue)
    {
        return cue.Match(ApplyEscapeDachoraCueCases, new ApplyEscapeDachoraCueContext(samus, place, this));
    }

    /// <summary>What <see cref="ApplyEscapeDachoraCue"/> does on each <see cref="EscapeDachoraCue"/>, built once (#2553).</summary>
    private static EscapeDachoraCueCases<ApplyEscapeDachoraCueContext, AnimationContinuation<EscapeDachoraSegment>?> ApplyEscapeDachoraCueCases { get; } = new(
        LowTideLapEnds: static (lowTide, context) => IsAcidLow(context.Samus) ? null : RisingTideTakesOver(lowTide.Lap),
        LapEnds: static (lap, context) => context.Owner.HasCrittersEscaped() ? EscapeTakesOver(lap.Lap) : null,
        Move: static (move, context) =>
        {
            context.Place.Position = context.Place.Position.WithWholeX(context.Place.Position.X.Whole + move.Offset);
            return null;
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Samus">The samus the cases read.</param>
    /// <param name="Place">The place the cases read.</param>
    /// <param name="Owner">The <see cref="EscapeAnimalsBehavior"/> reacting.</param>
    private readonly partial record struct ApplyEscapeDachoraCueContext(SamusState Samus, RoomEnemyPlace Place, EscapeAnimalsBehavior Owner);

    /// <summary>
    /// Where the rising tide takes the escaping Dachora at the end of a low-tide lap the way <paramref name="lap"/> ran
    /// (#2167): the end of the high-tide lap that way, counting on the laps the low-tide run had left.
    /// </summary>
    internal static AnimationContinuation<EscapeDachoraSegment> RisingTideTakesOver(HorizontalDirection lap) =>
        lap == HorizontalDirection.Left ? EscapeDachoraAnimations.HighTideLapsAfterLeftLap : EscapeDachoraAnimations.HighTideLapsAfterRightLap;

    /// <summary>
    /// Where the critters' escape takes the escaping Dachora at the end of a lap the way <paramref name="lap"/> ran (#2167):
    /// the run for the escape, from its pause after a lap to the left and from its acceleration after one to the right.
    /// </summary>
    internal static AnimationContinuation<EscapeDachoraSegment> EscapeTakesOver(HorizontalDirection lap) =>
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
