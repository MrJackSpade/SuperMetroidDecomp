using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>The Platforms species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PlatformsBehavior : IEnemySpecies, IEnemyEntityFactory
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.SuspensorPlatform, EnemyDefinitionId.Tripper];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new PlatformsEntity(this, place, common);

    /// <summary>Clips the Tripper's and the suspensor platform's horizontal and vertical displacement against room terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds the platform movement family to terrain collision.</summary>
    internal PlatformsBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of the platforms' animations: each publishes the horizontal direction main AI moves in.</summary>
    private static void ApplyPlatformCue(RoomEnemyPlace place, PlatformCue cue) =>
        SetPlatformHorizontalMovementFromCue(place, cue.Movement);

    /// <summary>Runs the two entry points and common setup.</summary>
    internal static void InitializePlatform(RoomEnemyPlace place)
    {
        bool isSuspensorPlatform = place.DefinitionId == EnemyDefinitionId.SuspensorPlatform;
        var state = new PlatformEnemyState();
        place.SetSpeciesState(state);

        state.IsSuspensorPlatform = isSuspensorPlatform;
        // The platform moves left or right; any other movement is corrupt room data and is rejected rather than
        // normalized to rightward movement.
        PlatformSpawnSettings settings = place.SpawnSettings<PlatformSpawnSettings>();
        state.XMovement = settings.Movement;

        state.Playback = new(PlatformAnimations.StartingWith(PlatformAnimationChoices.SelectAnimation(
            isSuspensorPlatform, axis: Axis.Horizontal, state.XMovement)), AnimationPlace.StartOfAnimation);

        // The spawn settings name the speed the platform moves at.
        state.Speed = settings.Speed;

        state.YMovement = PlatformVerticalMovement.Rising;
        state.PreviousYMovement = PlatformVerticalMovement.Rising;
        state.VerticalArt = null;
        state.RestingY = place.Position.Y.Whole + 1;
        state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        state.MaximumYSpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(settings.VerticalAccelerationUpdates);
    }

    /// <summary>Runs the Tripper's and the suspensor platform's main AI.</summary>
    private void RunPlatformMain(
        RoomEnemyPlace place,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Every frame starts in the rising phase and changes to sinking only when the
        // exact asymmetric "Samus from below" test succeeds. This is also the solid-
        // platform riding gate; ordinary radius overlap would trigger from the sides.
        state.YMovement = EnemyCollision.IsSamusStandingOn(place, samus)
            ? PlatformVerticalMovement.Sinking
            : PlatformVerticalMovement.Rising;

        // The sinking carry measures the horizontal step from the position before it.
        int preHorizontalX = place.Position.X.Whole;
        MovePlatformHorizontally(place, state, level);
        MovePlatformVertically(place, state, samus, level, preHorizontalX);

        // Main AI resets acceleration after running the first frame in a newly
        // selected vertical state. Preserve that slightly surprising ordering: a new phase
        // can use the curve's second mark now, then begins from the curve's start on its next frame.
        if (state.YMovement != state.PreviousYMovement)
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        state.PreviousYMovement = state.YMovement;
    }

    /// <summary>Applies horizontal movement, reversing the horizontal direction and still-art animation on wall collision.</summary>
    private void MovePlatformHorizontally(
        RoomEnemyPlace place,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        SignedSixteenSixteen displacement = state.XMovement == HorizontalDirection.Left ? -state.Speed : state.Speed;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        // A wall collision reverses the horizontal direction and immediately installs the opposite
        // vertically-still animation. The first step of that animation shows the same frame again
        // when the animations run later in this enemy frame.
        state.XMovement = state.XMovement == HorizontalDirection.Left
            ? HorizontalDirection.Right
            : HorizontalDirection.Left;
        InstallPlatformVerticallyStillForCurrentDirection(place, state);
    }

    /// <summary>Routes the platform through rise or sink physics and carries a riding Samus by accepted motion.</summary>
    /// <param name="place">The platform's place.</param>
    /// <param name="state">The platform's state.</param>
    /// <param name="samus">Samus, carried while riding.</param>
    /// <param name="level">The room's collision data.</param>
    /// <param name="preHorizontalX">The platform's whole X before this update's horizontal movement.</param>
    private void MovePlatformVertically(
        RoomEnemyPlace place,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int preHorizontalX)
    {
        switch (state.YMovement)
        {
            case PlatformVerticalMovement.Rising:
                RaisePlatform(place, state, level);
                return;
            case PlatformVerticalMovement.Sinking:
                SinkPlatform(place, state, samus, level, preHorizontalX);
                return;
            default:
                throw new InvalidDataException(
                    $"Platform Y movement {state.YMovement} is neither rising nor falling.");
        }
    }

    /// <summary>Returns a depressed platform toward its spawn height with the one-pixel stop offset.</summary>
    private void RaisePlatform(
        RoomEnemyPlace place,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        // Normal platforms stop once they have risen from their depressed position to one pixel below their original
        // spawn center, which way the difference lies deciding it.
        if (CoordinateAxis.Difference(place.Position.Y.Whole, state.RestingY) < 0)
        {
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
            SetPlatformVerticallyStillAnimation(place, state);
            return;
        }

        SetPlatformVerticallyMovingAnimation(place, state);
        IncrementAndClampPlatformYSpeed(state);
        SignedSixteenSixteen displacement = state.YSpeedCurvePlace.Speed.Negative;
        if (!_terrain.MoveEnemyVertically(level, place, displacement))
            return;

        state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        SetPlatformVerticallyStillAnimation(place, state);
    }

    /// <summary>Accelerates a ridden platform downward and publishes its collision-clipped displacement to Samus.</summary>
    /// <param name="place">The platform's place.</param>
    /// <param name="state">The platform's state.</param>
    /// <param name="samus">Samus, carried by the platform's accepted movement.</param>
    /// <param name="level">The room's collision data.</param>
    /// <param name="preHorizontalX">The platform's whole X before this update's horizontal movement.</param>
    private void SinkPlatform(
        RoomEnemyPlace place,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int preHorizontalX)
    {
        // Sinking always accelerates.
        IncrementAndClampPlatformYSpeed(state);

        // Add the whole-pixel horizontal step to Samus's extra displacement; fractional
        // displacement is deliberately untouched because this enemy carries only whole pixels.
        int xDelta = place.Position.X.Whole - preHorizontalX;
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(xDelta, 0);

        int preVerticalY = place.Position.Y.Whole;
        SignedSixteenSixteen displacement = state.YSpeedCurvePlace.Speed.Positive;
        if (_terrain.MoveEnemyVertically(level, place, displacement))
        {
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
            SetPlatformVerticallyStillAnimation(place, state);
        }

        // This delta is measured after collision clipping, so Samus follows the platform's
        // accepted motion rather than the requested quadratic speed.
        int yDelta = place.Position.Y.Whole - preVerticalY;
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, yDelta);
    }

    /// <summary>
    /// Steps the place along the quadratic speed curve one mark on without crossing its variant-specific cap. The place
    /// stays within the curve's start through the cap (at most mark 255), and one step can only reach mark 256.
    /// </summary>
    internal static void IncrementAndClampPlatformYSpeed(PlatformEnemyState state)
    {
        state.YSpeedCurvePlace += QuadraticSpeedCurvePlace.OneMark;
        if (state.YSpeedCurvePlace >= state.MaximumYSpeedCurvePlace)
            state.YSpeedCurvePlace = state.MaximumYSpeedCurvePlace;
    }

    /// <summary>Starts moving art once per moving phase, whatever art a wall has started since (#2145).</summary>
    private static void SetPlatformVerticallyMovingAnimation(
        RoomEnemyPlace place,
        PlatformEnemyState state)
    {
        if (state.VerticalArt == PlatformVerticalArt.Moving)
            return;
        state.VerticalArt = PlatformVerticalArt.Moving;
        InstallPlatformVerticallyMovingForCurrentDirection(place, state);
    }

    /// <summary>Starts stationary art once per resting phase (#2145).</summary>
    private static void SetPlatformVerticallyStillAnimation(
        RoomEnemyPlace place,
        PlatformEnemyState state)
    {
        if (state.VerticalArt == PlatformVerticalArt.Still)
            return;
        state.VerticalArt = PlatformVerticalArt.Still;
        InstallPlatformVerticallyStillForCurrentDirection(place, state);
    }

    /// <summary>Selects the moving animation from suspensor identity and current horizontal direction.</summary>
    internal static void InstallPlatformVerticallyMovingForCurrentDirection(
        RoomEnemyPlace place,
        PlatformEnemyState state)
    {
        StartPlatformAnimation(place, state, PlatformAnimationChoices.SelectAnimation(
            state.IsSuspensorPlatform, axis: Axis.Vertical, state.XMovement));
    }

    /// <summary>Selects the stationary animation from suspensor identity and current horizontal direction.</summary>
    internal static void InstallPlatformVerticallyStillForCurrentDirection(
        RoomEnemyPlace place,
        PlatformEnemyState state)
    {
        StartPlatformAnimation(place, state, PlatformAnimationChoices.SelectAnimation(
            state.IsSuspensorPlatform, axis: Axis.Horizontal, state.XMovement));
    }

    /// <summary>Starts the selected animation with a one-tick frame timer.</summary>
    private static void StartPlatformAnimation(RoomEnemyPlace place, PlatformEnemyState state, PlatformSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, PlatformAnimations.StartingWith(segment));
    }

    /// <summary>Sets the direction main AI moves the platform, <paramref name="movement"/>, as a movement cue asks.</summary>
    private static void SetPlatformHorizontalMovementFromCue(
        RoomEnemyPlace place,
        HorizontalDirection movement)
    {
        place.RequireSpeciesState<PlatformEnemyState>().XMovement = movement;
    }

    /// <summary>Replaces a frozen Tripper's frame after common Bomb reaction using its movement direction.</summary>
    internal static EnemyBombReaction TripperBombReaction { get; } = new()
    {
        AfterCommon = c =>
        {
            if (c.Enemy.FrozenTimer != 0)
                c.Enemy.Frame = TripperFrozenFrame(c.Enemy.RequireSpeciesState<PlatformEnemyState>().XMovement);
        },
    };

    /// <summary>Runs Platforms's main AI.</summary>
    internal void RunPlatformsMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunPlatformMain(place, place.RequireSpeciesState<PlatformEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Platforms's typed animation.</summary>
    internal static void AdvancePlatformsTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, PlatformAnimations.Segments,
            static (PlatformEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyPlatformCue(place, cue);
                return null;
            });

    /// <summary>Applies Tripper's direction-specific frozen frame after a successful common shot freeze.</summary>
    /// <remarks>Tripper's private tail runs after common shot AI on the immediate-freeze path and replaces the current
    /// frame with its direction's frozen frame.</remarks>
    internal static EnemyShotReaction TripperShotReaction { get; } = new()
    {
        AfterCommon = c =>
        {
            if (c.Froze)
                c.Enemy.Frame = TripperFrozenFrame(c.Enemy.RequireSpeciesState<PlatformEnemyState>().XMovement);
        },
    };

    /// <summary>
    /// The frozen Tripper's frame: the left-moving frame while it moves left, the right-moving frame while it moves right.
    /// </summary>
    private static EnemyFrameId TripperFrozenFrame(HorizontalDirection direction) =>
        direction == HorizontalDirection.Left ? EnemyFrameId.TripperFrozenFacingLeft : EnemyFrameId.TripperFrozenFacingRight;
}
