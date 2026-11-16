using SuperMetroid.Core.Animation;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>The Ripper species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RipperBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. RipperPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new RipperEntity(this, place, common);

    /// <summary>Resolves horizontal wall collisions for all three Ripper variants.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Shares room collision behavior across ordinary Ripper, grapple Ripper, and jet Ripper AI.</summary>
    internal RipperBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Initializes the grapple Ripper.</summary>
    internal static void InitializeGrappleRipper(RoomEnemyPlace place)
    {
        var state = CreateRipperState(place);

        // The population entry names the patrol speed and the starting direction.
        GrappleRipperSpawnSettings settings = place.SpawnSettings<GrappleRipperSpawnSettings>();
        state.Speeds = new EnemyLinearSpeed(settings.Speed);
        LoadRipperVelocity(state, settings.Direction switch
        {
            GrappleRipperStartDirection.Left => false,
            GrappleRipperStartDirection.Right => true,
            _ => throw new InvalidOperationException($"GrappleRipper start direction {settings.Direction} is not a direction."),
        });
        SetRipperAnimation(
            place,
            state.XVelocity.WholePixels < 0 ? RipperSegment.GrappleRipperMovingLeft : RipperSegment.GrappleRipperMovingRight);

        // Unlike the wall-collision helper, patrol-bound reversal does not clamp the actor
        // back to either endpoint. The one-frame fractional overshoot is and becomes
        // visible again when the direction reverses on the next pass.
        state.MinimumXPosition = settings.MinimumX;
        state.MaximumXPosition = settings.MaximumX;
    }

    /// <summary>Runs the grapple Ripper's main AI.</summary>
    private void RunGrappleRipperMain(
        RoomEnemyPlace place,
        RipperEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.XVelocity;
        bool reverse = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            place,
            displacement);
        if (!reverse && state.XVelocity.WholePixels < 0)
        {
            reverse = CoordinateAxis.Difference(place.Position.X.Whole, state.MinimumXPosition) < 0;
        }
        if (!reverse && state.XVelocity.WholePixels >= 0)
        {
            reverse = CoordinateAxis.Difference(place.Position.X.Whole, state.MaximumXPosition) >= 0;
        }
        if (reverse)
        {
            ReverseRipperVariant(
                place,
                positiveSegment: RipperSegment.GrappleRipperMovingRight,
                negativeSegment: RipperSegment.GrappleRipperMovingLeft);
        }
    }

    /// <summary>Initializes the jet Ripper.</summary>
    internal static void InitializeJetRipper(RoomEnemyPlace place)
    {
        RipperEnemyState state = CreateRipperState(place);
        state.Speeds = new EnemyLinearSpeed(place.SpawnSettings<RipperSpawnSettings>().Speed);

        // The jet Ripper shows the grapple Ripper's animations, facing the way it flies (#1842).
        bool movingPositive = place.SpawnSettings<RipperSpawnSettings>().Direction == HorizontalDirection.Right;
        LoadRipperVelocity(state, movingPositive);
        SetRipperAnimation(place, movingPositive ? RipperSegment.GrappleRipperMovingRight : RipperSegment.GrappleRipperMovingLeft);
    }

    /// <summary>Runs the jet Ripper's main AI.</summary>
    private void RunJetRipperMain(RoomEnemyPlace place, RipperEnemyState state, RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.XVelocity;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        ReverseRipperVariant(
            place,
            positiveSegment: RipperSegment.GrappleRipperMovingRight,
            negativeSegment: RipperSegment.GrappleRipperMovingLeft);
    }

    /// <summary>Initializes the Ripper.</summary>
    internal static void InitializeRipper(RoomEnemyPlace place)
    {
        // The spawn's direction chooses the positive speed and the right-facing animation, or the negative speed and the
        // left-facing animation. Unlike a reversal, initialization keeps the population's frame timer.
        bool movingRight = place.SpawnSettings<RipperSpawnSettings>().Direction == HorizontalDirection.Right;
        RipperEnemyState state = CreateRipperState(place);
        state.Playback = new(RipperAnimations.StartingWith(
            movingRight ? RipperSegment.RipperMovingRight : RipperSegment.RipperMovingLeft), AnimationPlace.StartOfAnimation);

        // The state retains the speed row because the reversal reuses it.
        state.Speeds = new EnemyLinearSpeed(place.SpawnSettings<RipperSpawnSettings>().Speed);
        LoadRipperVelocity(state, movingRight);
    }

    /// <summary>Runs the Ripper's main AI.</summary>
    private void RunRipperMain(RoomEnemyPlace place, RoomLevelData level)
    {
        // The horizontal mover consumes the signed 1/65536-pixel velocity as a displacement. On collision the
        // mover aligns the actor flush with the wall before Ripper swaps direction
        // and animation.
        RipperEnemyState state = place.RequireSpeciesState<RipperEnemyState>();
        SignedSixteenSixteen displacement = state.XVelocity;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        bool wasMovingLeft = state.XVelocity.WholePixels < 0;
        bool nowMovingRight = wasMovingLeft;
        LoadRipperVelocity(state, nowMovingRight);
        SetRipperAnimation(place, nowMovingRight ? RipperSegment.RipperMovingRight : RipperSegment.RipperMovingLeft);
    }

    /// <summary>Loads a Ripper-family enemy's velocity from its linear-speed row.</summary>
    /// <param name="state">The enemy's state, whose speed row is read.</param>
    /// <param name="movingRight">Whether the row's positive speed is loaded rather than its negative speed.</param>
    internal static void LoadRipperVelocity(RipperEnemyState state, bool movingRight) =>
        state.XVelocity = movingRight ? state.Speeds.Positive : state.Speeds.Negative;

    /// <summary>
    /// Runs the common post-shot tail. Normal shot AI owns health, death,
    /// impact, and freeze timers; this family-specific tail only pins the direction-specific
    /// frozen frame when the projectile that hit it actually froze it.
    /// </summary>
    private static void ResolveGrappleRipperJetRipperShotAfterCommon(RoomEnemyPlace place)
    {
        if (place.FrozenTimer == 0)
            return;
        place.Frame = place.RequireSpeciesState<RipperEnemyState>().XVelocity.WholePixels < 0
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

    /// <summary>Flips a grapple Ripper or jet Ripper speed pair and restarts the corresponding loop.</summary>
    internal static void ReverseRipperVariant(
        RoomEnemyPlace place,
        RipperSegment positiveSegment,
        RipperSegment negativeSegment)
    {
        RipperEnemyState state = place.RequireSpeciesState<RipperEnemyState>();
        bool wasMovingNegative = state.XVelocity.WholePixels < 0;
        LoadRipperVelocity(state, movingRight: wasMovingNegative);
        SetRipperAnimation(place, wasMovingNegative ? positiveSegment : negativeSegment);
    }

    /// <summary>Installs a flying loop from its first step on the next enemy frame.</summary>
    private static void SetRipperAnimation(RoomEnemyPlace place, RipperSegment segment)
    {
        place.RequireSpeciesState<RipperEnemyState>().Playback = new(RipperAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.Restart();
    }

    /// <summary>Advances Ripper's typed animation.</summary>
    internal static void AdvanceRipperTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        RipperEnemyState state = place.RequireSpeciesState<RipperEnemyState>();
        AnimationPlayback<RipperSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, RipperAnimations.Segments, ref playback,
            static cue => throw new InvalidOperationException($"Ripper's cue-free animation raised {cue}."));
        state.Playback = playback;
    }

    /// <summary>Preserves the direction-specific frozen map after common bomb reactions.</summary>
    internal static EnemyBombReaction RipperBombReaction { get; } = new()
    {
        AfterCommon = c => ResolveGrappleRipperJetRipperShotAfterCommon(c.Enemy),
    };

    /// <summary>Runs grapple Ripper's main AI.</summary>
    internal void RunGrappleRipperMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunGrappleRipperMain(place, place.RequireSpeciesState<RipperEnemyState>(), frame.Level);
    }

    /// <summary>Runs jet Ripper's main AI.</summary>
    internal void RunJetRipperMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunJetRipperMain(place, place.RequireSpeciesState<RipperEnemyState>(), frame.Level);
    }

    /// <summary>Runs Ripper's main AI.</summary>
    internal void RunRipperMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunRipperMain(place, frame.Level);
    }

    /// <summary>Pins grapple Ripper and jet Ripper frozen visuals after accepted shot reactions.</summary>
    internal static EnemyShotReaction RipperShotReaction { get; } = new()
    {
        AfterCommon = c => ResolveGrappleRipperJetRipperShotAfterCommon(c.Enemy),
    };
}
