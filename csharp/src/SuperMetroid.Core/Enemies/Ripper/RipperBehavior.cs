using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The Ripper species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RipperBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PartFactories.Keys];

    /// <summary>The grapple Ripper, the ordinary Ripper or the jet Ripper (#2146).</summary>
    /// <exception cref="KeyNotFoundException">The place holds no Ripper definition.</exception>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        PartFactories[Ensure.NotNull(occupant).DefinitionId](this, occupant, place, common);

    /// <summary>
    /// The factory of each part the species creates, by the definition that places it: the species serves these
    /// definitions, and each builds its own part from the species it is given.
    /// </summary>
    private static readonly Dictionary<EnemyDefinitionId, EnemyPartFactory<RipperBehavior>> PartFactories =
        new()
        {
            [EnemyDefinitionId.GrappleRipper] = static (species, occupant, place, common) => EnemyOccupant.Of(new GrappleRipperEntity(species, occupant, place, common)),
            [EnemyDefinitionId.Ripper] = static (species, occupant, place, common) => EnemyOccupant.Of(new OrdinaryRipperEntity(species, occupant, place, common)),
            [EnemyDefinitionId.JetRipper] = static (species, occupant, place, common) => EnemyOccupant.Of(new JetRipperEntity(species, occupant, place, common)),
        };

    /// <summary>Resolves horizontal wall collisions for all three Ripper variants.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Shares room collision behavior across ordinary Ripper, grapple Ripper, and jet Ripper AI.</summary>
    internal RipperBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Initializes the grapple Ripper.</summary>
    internal static void InitializeGrappleRipper(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = CreateRipperState(place);

        // The placement names the starting direction; the patrol speed and bounds are read where they are used.
        state.Heading = occupant.SpawnSettings<GrappleRipperSpawnSettings>().Direction;
        SetRipperAnimation(
            place,
            state.Heading == HorizontalDirection.Left ? RipperSegment.GrappleRipperMovingLeft : RipperSegment.GrappleRipperMovingRight);
    }

    /// <summary>Runs the grapple Ripper's main AI.</summary>
    private void RunGrappleRipperMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        RipperEnemyState state,
        RoomLevelData level)
    {
        GrappleRipperSpawnSettings settings = occupant.SpawnSettings<GrappleRipperSpawnSettings>();
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
            ReverseRipperVariant(place);
        }
    }

    /// <summary>Initializes the jet Ripper.</summary>
    internal static void InitializeJetRipper(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        RipperEnemyState state = CreateRipperState(place);

        // The jet Ripper shows the grapple Ripper's animations, facing the way it flies (#1842).
        state.Heading = occupant.SpawnSettings<RipperSpawnSettings>().Direction;
        SetRipperAnimation(place, state.Heading == HorizontalDirection.Right ? RipperSegment.GrappleRipperMovingRight : RipperSegment.GrappleRipperMovingLeft);
    }

    /// <summary>Runs the jet Ripper's main AI.</summary>
    private void RunJetRipperMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, RipperEnemyState state, RoomLevelData level)
    {
        SignedSixteenSixteen displacement = FlyingVelocity(occupant.SpawnSettings<RipperSpawnSettings>().Speed, state.Heading);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        ReverseRipperVariant(place);
    }

    /// <summary>Initializes the Ripper.</summary>
    internal static void InitializeRipper(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        // The spawn's direction picks the way it flies and its facing animation, which starts on the placement's
        // timing.
        HorizontalDirection heading = occupant.SpawnSettings<RipperSpawnSettings>().Direction;
        RipperEnemyState state = CreateRipperState(place);
        state.Playback = new(RipperAnimations.StartingWith(
            heading == HorizontalDirection.Right ? RipperSegment.RipperMovingRight : RipperSegment.RipperMovingLeft), AnimationPlace.StartOfAnimation);

        // It flies its spawn speed toward its heading.
        state.Heading = heading;
    }

    /// <summary>Runs the Ripper's main AI.</summary>
    private void RunRipperMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, RoomLevelData level)
    {
        // The horizontal mover moves the Ripper by its velocity. On collision the
        // mover aligns the actor flush with the wall before Ripper swaps direction
        // and animation.
        RipperEnemyState state = place.RequireSpeciesState<RipperEnemyState>();
        SignedSixteenSixteen displacement = FlyingVelocity(occupant.SpawnSettings<RipperSpawnSettings>().Speed, state.Heading);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        Reverse(state);
        SetRipperAnimation(place, state.Heading == HorizontalDirection.Right ? RipperSegment.RipperMovingRight : RipperSegment.RipperMovingLeft);
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
    private static void ResolveGrappleRipperJetRipperShotAfterCommon(RoomEnemyPlace place)
    {
        if (place.Freeze.IsOver)
            return;
        place.Frame = place.RequireSpeciesState<RipperEnemyState>().Heading == HorizontalDirection.Left
            ? EnemyFrameId.RipperFrozenFacingLeft
            : EnemyFrameId.RipperFrozenFacingRight;
    }

    /// <summary>Starts a Ripper-family enemy's state on its place.</summary>
    private static RipperEnemyState CreateRipperState(RoomEnemyPlace place)
    {
        var state = new RipperEnemyState();
        place.SetSpeciesState(state);
        return state;
    }

    /// <summary>
    /// Turns a grapple Ripper or jet Ripper round and restarts the grapple Ripper's flying loop facing its new heading,
    /// which both show (#1842).
    /// </summary>
    private static void ReverseRipperVariant(RoomEnemyPlace place)
    {
        RipperEnemyState state = place.RequireSpeciesState<RipperEnemyState>();
        Reverse(state);
        SetRipperAnimation(place, state.Heading == HorizontalDirection.Right ? RipperSegment.GrappleRipperMovingRight : RipperSegment.GrappleRipperMovingLeft);
    }

    /// <summary>Starts a flying loop, shown from the next update.</summary>
    private static void SetRipperAnimation(RoomEnemyPlace place, RipperSegment segment)
    {
        place.RequireSpeciesState<RipperEnemyState>().Playback = new(RipperAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.Restart();
    }

    /// <summary>Advances Ripper's typed animation.</summary>
    internal static void AdvanceRipperTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        RipperEnemyState state = place.RequireSpeciesState<RipperEnemyState>();
        AnimationPlayback<RipperSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, RipperAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"Ripper's cue-free animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Preserves the direction-specific frozen frame after common bomb reactions.</summary>
    internal static EnemyBombReaction RipperBombReaction { get; } = new()
    {
        AfterCommon = c => ResolveGrappleRipperJetRipperShotAfterCommon(c.Enemy),
    };

    /// <summary>Runs grapple Ripper's main AI.</summary>
    internal void RunGrappleRipperMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunGrappleRipperMain(occupant, place, place.RequireSpeciesState<RipperEnemyState>(), frame.Level);
    }

    /// <summary>Runs jet Ripper's main AI.</summary>
    internal void RunJetRipperMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunJetRipperMain(occupant, place, place.RequireSpeciesState<RipperEnemyState>(), frame.Level);
    }

    /// <summary>Runs Ripper's main AI.</summary>
    internal void RunRipperMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunRipperMain(occupant, place, frame.Level);
    }

    /// <summary>Pins grapple Ripper and jet Ripper frozen visuals after accepted shot reactions.</summary>
    internal static EnemyShotReaction RipperShotReaction { get; } = new()
    {
        AfterCommon = c => ResolveGrappleRipperJetRipperShotAfterCommon(c.Enemy),
    };
}
