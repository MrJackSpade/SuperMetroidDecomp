using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The Ripper species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RipperBehavior : IEnemySpecies, IEnemySettingsFactory<GrappleRipperSpawnSettings>, IEnemySettingsFactory<RipperSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.GrappleRipper, EnemyDefinitionId.Ripper, EnemyDefinitionId.JetRipper];

    /// <inheritdoc/>
    void IEnemySpeciesFactory.EnrollIn(EnemySpeciesRegistry registry)
    {
        Ensure.NotNull(registry).Add<GrappleRipperSpawnSettings>(this);
        registry.Add<RipperSpawnSettings>(this);
    }

    /// <summary>The grapple Ripper, from the patrol its placement names (#2146).</summary>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, GrappleRipperSpawnSettings settings) =>
        new GrappleRipperEntity(this, occupant, place, common, settings);

    /// <summary>The ordinary Ripper or the jet Ripper, from the flight its placement names (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds neither Ripper's definition.</exception>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, RipperSpawnSettings settings)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.JetRipper)
            return new JetRipperEntity(this, occupant, place, common, settings);
        if (definition == EnemyDefinitionId.Ripper)
            return new OrdinaryRipperEntity(this, occupant, place, common, settings);
        throw new InvalidOperationException($"The Ripper species flies no {place.DefinitionId}.");
    }

    /// <summary>Resolves horizontal wall collisions for all three Ripper variants.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Shares room collision behavior across ordinary Ripper, grapple Ripper, and jet Ripper AI.</summary>
    internal RipperBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Initializes the grapple Ripper.</summary>
    internal static RipperEnemyState InitializeGrappleRipper(RoomEnemyOccupant occupant, RoomEnemyPlace place, GrappleRipperSpawnSettings settings)
    {
        RipperEnemyState state = CreateRipperState();

        // The population entry names the starting direction; the patrol speed and bounds are read where they are used.
        state.Heading = settings.Direction;
        SetRipperAnimation(
            place, state,
            state.Heading == HorizontalDirection.Left ? RipperSegment.GrappleRipperMovingLeft : RipperSegment.GrappleRipperMovingRight);
        return state;
    }

    /// <summary>Runs the grapple Ripper's main AI.</summary>
    private void RunGrappleRipperMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        RipperEnemyState state,
        GrappleRipperSpawnSettings settings,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = FlyingVelocity(settings.Speed, state.Heading);
        bool reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            place,
            displacement);
        // Unlike a wall, a patrol bound does not move the Ripper back flush with it: the update that passes the bound
        // keeps its overshoot, which shows again when the Ripper reverses on its next pass. The Ripper turns back once its
        // column lies left of the left bound, or at or right of the right one.
        if (!reverse && state.Heading == HorizontalDirection.Left)
        {
            reverse = place.Position.X.Whole < settings.MinimumX;
        }
        if (!reverse && state.Heading == HorizontalDirection.Right)
        {
            reverse = place.Position.X.Whole >= settings.MaximumX;
        }
        if (reverse)
        {
            ReverseRipperVariant(place, state);
        }
    }

    /// <summary>Initializes the jet Ripper.</summary>
    internal static RipperEnemyState InitializeJetRipper(RoomEnemyOccupant occupant, RoomEnemyPlace place, RipperSpawnSettings settings)
    {
        RipperEnemyState state = CreateRipperState();

        // The jet Ripper shows the grapple Ripper's animations, facing the way it flies (#1842).
        state.Heading = settings.Direction;
        SetRipperAnimation(place, state, state.Heading == HorizontalDirection.Right ? RipperSegment.GrappleRipperMovingRight : RipperSegment.GrappleRipperMovingLeft);
        return state;
    }

    /// <summary>Runs the jet Ripper's main AI.</summary>
    private void RunJetRipperMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, RipperEnemyState state, RipperSpawnSettings settings, RoomLevelData level)
    {
        SignedSixteenSixteen displacement = FlyingVelocity(settings.Speed, state.Heading);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        ReverseRipperVariant(place, state);
    }

    /// <summary>Initializes the Ripper.</summary>
    internal static RipperEnemyState InitializeRipper(RoomEnemyOccupant occupant, RipperSpawnSettings settings)
    {
        // The spawn's direction chooses the positive speed and the right-facing animation, or the negative speed and the
        // left-facing animation. Unlike a reversal, initialization keeps the population's frame timer.
        HorizontalDirection heading = settings.Direction;
        RipperEnemyState state = CreateRipperState();
        state.Playback = new(RipperAnimations.StartingWith(
            heading == HorizontalDirection.Right ? RipperSegment.RipperMovingRight : RipperSegment.RipperMovingLeft), AnimationPlace.StartOfAnimation);

        // It flies its spawn speed toward its heading.
        state.Heading = heading;
        return state;
    }

    /// <summary>Runs the Ripper's main AI.</summary>
    private void RunRipperMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, RipperEnemyState state, RipperSpawnSettings settings, RoomLevelData level)
    {
        // The horizontal mover moves the Ripper by its velocity. On collision the
        // mover aligns the actor flush with the wall before Ripper swaps direction
        // and animation.
        SignedSixteenSixteen displacement = FlyingVelocity(settings.Speed, state.Heading);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        Reverse(state);
        SetRipperAnimation(place, state, state.Heading == HorizontalDirection.Right ? RipperSegment.RipperMovingRight : RipperSegment.RipperMovingLeft);
    }

    /// <summary>
    /// The horizontal displacement each update of a Ripper-family enemy flying <paramref name="speed"/>, the spawn speed
    /// its own main AI reads from its own settings (#2512), toward <paramref name="heading"/> (#2278, #2450).
    /// </summary>
    private static SignedSixteenSixteen FlyingVelocity(SignedSixteenSixteen speed, HorizontalDirection heading) => speed.Toward(heading);

    /// <summary>Turns a Ripper-family enemy round: it flies its spawn speed the other way (#2278).</summary>
    private static void Reverse(RipperEnemyState state) =>
        state.Heading = state.Heading == HorizontalDirection.Left ? HorizontalDirection.Right : HorizontalDirection.Left;

    /// <summary>
    /// Finishes a Ripper's reaction to a shot, after the shot's common damage has dealt with health, death, impact and
    /// freezing: a shot that froze it pins the frozen frame for the way it faces.
    /// </summary>
    private static void ResolveGrappleRipperJetRipperShotAfterCommon(RoomEnemyPlace place, RipperEnemyState state)
    {
        if (place.Freeze.IsOver)
            return;
        place.Frame = state.Heading == HorizontalDirection.Left
            ? EnemyFrameId.RipperFrozenFacingLeft
            : EnemyFrameId.RipperFrozenFacingRight;
    }

    /// <summary>Starts a Ripper-family enemy's state.</summary>
    private static RipperEnemyState CreateRipperState()
    {
        return new RipperEnemyState();
    }

    /// <summary>
    /// Turns a grapple Ripper or jet Ripper round and restarts the grapple Ripper's flying loop facing its new heading,
    /// which both show (#1842).
    /// </summary>
    private static void ReverseRipperVariant(RoomEnemyPlace place, RipperEnemyState state)
    {
        Reverse(state);
        SetRipperAnimation(place, state, state.Heading == HorizontalDirection.Right ? RipperSegment.GrappleRipperMovingRight : RipperSegment.GrappleRipperMovingLeft);
    }

    /// <summary>Starts a flying loop, shown from the next update.</summary>
    private static void SetRipperAnimation(RoomEnemyPlace place, RipperEnemyState state, RipperSegment segment)
    {
        state.Playback = new(RipperAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.Restart();
    }

    /// <summary>Advances Ripper's typed animation.</summary>
    internal static void AdvanceRipperTypedAnimation(RoomEnemyPlace place, RipperEnemyState state, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<RipperSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, RipperAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"Ripper's cue-free animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Preserves the direction-specific frozen frame after common bomb reactions.</summary>
    /// <param name="state">The enemy's state, which the reaction acts on, after a hit has killed it too.</param>
    internal static EnemyBombReaction RipperBombReactionOf(RipperEnemyState state) => new()
    {
        AfterCommon = c => ResolveGrappleRipperJetRipperShotAfterCommon(c.Enemy, state),
    };

    /// <summary>Runs grapple Ripper's main AI.</summary>
    internal void RunGrappleRipperMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, RipperEnemyState state, GrappleRipperSpawnSettings settings, IEnemyAiFrame frame)
    {
        RunGrappleRipperMain(occupant, place, state, settings, frame.Level);
    }

    /// <summary>Runs jet Ripper's main AI.</summary>
    internal void RunJetRipperMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, RipperEnemyState state, RipperSpawnSettings settings, IEnemyAiFrame frame)
    {
        RunJetRipperMain(occupant, place, state, settings, frame.Level);
    }

    /// <summary>Runs Ripper's main AI.</summary>
    internal void RunRipperMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, RipperEnemyState state, RipperSpawnSettings settings, IEnemyAiFrame frame)
    {
        RunRipperMain(occupant, place, state, settings, frame.Level);
    }

    /// <summary>Pins grapple Ripper and jet Ripper frozen visuals after accepted shot reactions.</summary>
    /// <param name="state">The enemy's state, which the reaction acts on, after a hit has killed it too.</param>
    internal static EnemyShotReaction RipperShotReactionOf(RipperEnemyState state) => new()
    {
        AfterCommon = c => ResolveGrappleRipperJetRipperShotAfterCommon(c.Enemy, state),
    };
}
