using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>The Mochtroid species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MochtroidBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Mochtroid], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new MochtroidEntity(this, place, common));

    /// <summary>Resolves axis movement against terrain in the order for each flight mode.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Queues an individual drain sound for each qualifying attached actor.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Provides the shared enemy counter that gates attached drain sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Runs common touch damage when Samus has an active contact-damage mode.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds movement, audio, frame counter, and collision services for Mochtroid AI.</summary>
    internal MochtroidBehavior(EnemyTerrain terrain, EnemyAudioQueue audio, EnemyFrameContext frameContext, EnemyCollision collision)
    {
        _terrain = terrain;
        _audio = audio;
        _frameContext = frameContext;
        _collision = collision;
    }

    /// <summary>Initializes the Mochtroid.</summary>
    internal static void InitializeMochtroid(RoomEnemyPlace place)
    {
        // A new Mochtroid has started no animation, so its first request always starts one. Ordinary room loads
        // start every Mochtroid this way.
        var state = new MochtroidEnemyState
        {
            MovementMode = MochtroidMovementMode.NotTouchingSamus,
        };
        place.SetSpeciesState(state);
        SetMochtroidAnimation(
            place, state, MochtroidSegment.FreeFlight);
    }

    /// <summary>Runs a Mochtroid for one update, resetting its movement before it moves.</summary>
    private void RunMochtroidMain(
        RoomEnemyPlace place,
        SamusState samus,
        RoomLevelData level)
    {
        MochtroidEnemyState state = place.RequireSpeciesState<MochtroidEnemyState>();
        MochtroidMovementMode selectedMode = state.MovementMode;
        state.MovementMode = MochtroidMovementMode.NotTouchingSamus;
        switch (selectedMode)
        {
            case MochtroidMovementMode.NotTouchingSamus:
                RunMochtroidFreeFlight(place, state, samus, level);
                return;
            case MochtroidMovementMode.TouchingSamus:
                RunMochtroidAttachedFlight(place, state, samus, level);
                return;
        }
    }

    /// <summary>Accelerates toward Samus, resolving Y before X and clearing velocity on terrain contact.</summary>
    private void RunMochtroidFreeFlight(
        RoomEnemyPlace place,
        MochtroidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Each axis's speed moves toward Samus by an amount in proportion to the distance
        // between them: a proportional attraction toward Samus, not a fixed-speed seek,
        // and the speed's fraction shows before either whole part reaches its fastest flight.
        int yDifference = place.Position.Y.Whole - samus.Position.Y.Whole;
        SignedSixteenSixteen yVelocity = ClampMochtroidVelocity(state.Velocity.Y - EnemyMath.HomingPull(yDifference));
        StoreMochtroidYVelocity(state, yVelocity);
        if (_terrain.MoveEnemyVertically(level, place, yVelocity))
            StoreMochtroidYVelocity(state, SignedSixteenSixteen.Zero);

        // Y is intentionally resolved first. Horizontal collision therefore sees the
        // vertically adjusted position, matching the game's vertical then horizontal collision.
        int xDifference = place.Position.X.Whole - samus.Position.X.Whole;
        SignedSixteenSixteen xVelocity = ClampMochtroidVelocity(state.Velocity.X - EnemyMath.HomingPull(xDifference));
        StoreMochtroidXVelocity(state, xVelocity);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, xVelocity))
            StoreMochtroidXVelocity(state, SignedSixteenSixteen.Zero);

        SetMochtroidAnimation(
            place, state, MochtroidSegment.FreeFlight);
    }

    /// <summary>Caps a velocity by its whole-pixel component at <see cref="MochtroidBehaviorDefinitions.FastestFlight"/>.</summary>
    private static SignedSixteenSixteen ClampMochtroidVelocity(SignedSixteenSixteen velocity) => EnemyMath.LimitedEitherWay(velocity, MochtroidBehaviorDefinitions.FastestFlight);

    /// <summary>Moves toward Samus at <see cref="MochtroidBehaviorDefinitions.AttachedSpeed"/> on each axis, resolving X before Y while attached.</summary>
    private void RunMochtroidAttachedFlight(
        RoomEnemyPlace place,
        MochtroidEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Attached mode discards proportional acceleration. It moves exactly its attached speed
        // on each axis toward Samus, with X collision resolved before Y and ignored return values.
        SignedSixteenSixteen xVelocity = place.Position.X.Whole == samus.Position.X.Whole
            ? SignedSixteenSixteen.Zero
            : MochtroidBehaviorDefinitions.AttachedSpeed.Toward(
                place.Position.X.Whole > samus.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right);
        StoreMochtroidXVelocity(state, xVelocity);
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, xVelocity);

        SignedSixteenSixteen yVelocity = place.Position.Y.Whole == samus.Position.Y.Whole
            ? SignedSixteenSixteen.Zero
            : MochtroidBehaviorDefinitions.AttachedSpeed.Toward(
                place.Position.Y.Whole > samus.Position.Y.Whole ? VerticalDirection.Up : VerticalDirection.Down);
        StoreMochtroidYVelocity(state, yVelocity);
        _terrain.MoveEnemyVertically(level, place, yVelocity);
    }

    /// <summary>Stores the current horizontal velocity in the species state.</summary>
    private static void StoreMochtroidXVelocity(MochtroidEnemyState state, SignedSixteenSixteen velocity)
    {
        state.Velocity = state.Velocity.WithX(velocity);
    }

    /// <summary>Stores the current vertical velocity in the species state.</summary>
    private static void StoreMochtroidYVelocity(MochtroidEnemyState state, SignedSixteenSixteen velocity)
    {
        state.Velocity = state.Velocity.WithY(velocity);
    }

    /// <summary>Starts a changed flight or attachment animation and restarts its frame timers (#2145).</summary>
    private static void SetMochtroidAnimation(
        RoomEnemyPlace place,
        MochtroidEnemyState state,
        MochtroidSegment segment) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, MochtroidAnimations.StartingWith(segment));

    /// <summary>Resolves the family-specific portion of a Mochtroid's touch.</summary>
    private void ResolveMochtroidTouch(
        RoomEnemyPlace place,
        MochtroidEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        state.MovementMode = MochtroidMovementMode.TouchingSamus;
        SetMochtroidAnimation(
            place, state, MochtroidSegment.Attached);
        state.DrainPeriod.Step();

        bool applyCommonTouch = samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None;
        if (!applyCommonTouch)
        {
            // Every attached Mochtroid queues its own drain sound, so a room full of them queues one each.
            if (MetroidCadenceDefinitions.DrainSound.IsDue(_frameContext.EnemyPassCount) && samus.Health >= MetroidBehaviorDefinitions.DrainSoundLeastHealth)
                _audio.QueueEnemySound(RoomSound.IncrementalEnergy, priority: SoundRequestPriority.Normal);

            if (state.DrainPeriod.IsOver)
            {
                state.DrainPeriod = UpdateCountdown.Lasting(MochtroidBehaviorDefinitions.AttachedDrainPeriod);
                applyCommonTouch = true;
            }
        }

        if (!applyCommonTouch)
            return;

        if (samus.HorizontalSpeed.BodyAttack == SamusBodyAttack.None)
        {
            // The Mochtroid's touch hurts Samus without knocking her back: only the suit-divided damage applies.
            SamusSuitDamage.Hurt(samus, place.Definition.Damage);
        }
        else
        {
            // Samus's body attack hurts the Mochtroid as it hurts any enemy. This
            // includes Speed Booster, shinespark, Screw Attack, and pseudo-Screw.
            _collision.ResolveOrdinaryTouch(place, samus, pseudoScrew);
        }

        // Unlike any other enemy, the Mochtroid cancels at once the invincibility and knockback its
        // touch's damage started: Samus stays attached rather than being knocked back with a grace
        // period, so another drain cycle can follow.
        samus.ClearHurtTimers();
    }

    /// <summary>Advances Mochtroid's typed animation.</summary>
    internal static void AdvanceMochtroidTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.AdvanceSilently(place, MochtroidAnimations.Segments,
            static (MochtroidEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);

    /// <summary>Runs Mochtroid's main AI.</summary>
    internal void RunMochtroidMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMochtroidMain(place, frame.Samus, frame.Level);
    }

    /// <summary>Runs the attachment and drain reaction for Samus overlap.</summary>
    internal void ReactToMochtroidTouch(EnemyTouchContext context) =>
        ResolveMochtroidTouch(context.Place, context.Place.RequireSpeciesState<MochtroidEnemyState>(), context.Samus, context.PseudoScrew);

    /// <summary>The Mochtroid's own shot reaction, which runs the common shot damage with no stage of its own.</summary>
    internal static EnemyShotReaction MochtroidShotReaction { get; } = new();
}
