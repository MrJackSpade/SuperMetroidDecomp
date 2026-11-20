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
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new PlatformsEntity(this, occupant, place, common));

    /// <summary>Clips the Tripper's and the suspensor platform's horizontal and vertical displacement against room terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds the platform movement family to terrain collision.</summary>
    internal PlatformsBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of the platforms' animations: each publishes the horizontal direction main AI moves in.</summary>
    private static void ApplyPlatformCue(RoomEnemyPlace place, PlatformEnemyState platformEnemyState, PlatformCue cue) =>
        SetPlatformHorizontalMovementFromCue(place, platformEnemyState, cue.Movement);

    /// <summary>
    /// Sets up a Tripper or a suspensor platform: its direction and speed from its spawn settings, its first rise and the
    /// height it rests at below where it was placed.
    /// </summary>
    internal static void InitializePlatform(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new PlatformEnemyState
        {
            Kind = place.Holds(EnemyDefinitionId.SuspensorPlatform) ? PlatformKind.SuspensorPlatform : PlatformKind.Tripper,
        };
        place.SetSpeciesState(state);

        // The platform moves left or right as its spawn settings say.
        PlatformSpawnSettings settings = occupant.SpawnSettings<PlatformSpawnSettings>();
        state.XMovement = settings.Movement;

        state.Playback = new(PlatformAnimations.StartingWith(PlatformAnimationChoices.SelectAnimation(
            state.Kind, PlatformVerticalArt.Still, state.XMovement)), AnimationPlace.StartOfAnimation);

        state.PreviousYMovement = PlatformVerticalMovement.Rising;
        state.RestingY = place.Position.Y.Whole + PlatformMotionDefinitions.RestingDepthBelowPlacement;
    }

    /// <summary>Runs the Tripper's and the suspensor platform's main AI.</summary>
    private void RunPlatformMain(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // Every frame starts in the rising phase and changes to sinking only when the
        // exact asymmetric "Samus from below" test succeeds. This is also the solid-
        // platform riding gate; ordinary radius overlap would trigger from the sides. The choice is this update's own
        // (#2473); only the last one carries to the next update.
        PlatformVerticalMovement movement = EnemyCollision.IsSamusStandingOn(place, samus)
            ? PlatformVerticalMovement.Sinking
            : PlatformVerticalMovement.Rising;

        // The sinking carry measures the horizontal step from the position before it.
        int preHorizontalX = place.Position.X.Whole;
        MovePlatformHorizontally(occupant, place, state, level);
        MovePlatformVertically(occupant, place, state, samus, level, movement, preHorizontalX);

        // A newly selected vertical movement restarts the speed curve only after its first update:
        // that update can move by the curve's second mark, and the next starts from the curve's start.
        if (movement != state.PreviousYMovement)
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        state.PreviousYMovement = movement;
    }

    /// <summary>Applies horizontal movement, reversing the horizontal direction and still-art animation on wall collision.</summary>
    private void MovePlatformHorizontally(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        // The spawn settings name the speed the platform moves at.
        SignedSixteenSixteen speed = occupant.SpawnSettings<PlatformSpawnSettings>().Speed;
        SignedSixteenSixteen displacement = speed.Toward(state.XMovement);
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, displacement))
            return;

        // A wall collision reverses the horizontal direction and immediately starts the opposite
        // vertically-still animation. The first step of that animation shows the same frame again
        // when the animations run later in this enemy frame.
        state.XMovement = state.XMovement == HorizontalDirection.Left
            ? HorizontalDirection.Right
            : HorizontalDirection.Left;
        StartPlatformStillAnimationForHeading(place, state);
    }

    /// <summary>Routes the platform through rise or sink physics and carries a riding Samus by accepted motion.</summary>
    /// <param name="occupant">The enemy holding <paramref name="place"/>.</param>
    /// <param name="place">The platform's place.</param>
    /// <param name="state">The platform's state.</param>
    /// <param name="samus">Samus, carried while riding.</param>
    /// <param name="level">The room's collision data.</param>
    /// <param name="movement">The vertical movement this update's rider test chose.</param>
    /// <param name="preHorizontalX">The platform's whole X before this update's horizontal movement.</param>
    private void MovePlatformVertically(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level,
        PlatformVerticalMovement movement,
        int preHorizontalX)
    {
        switch (movement)
        {
            case PlatformVerticalMovement.Rising:
                RaisePlatform(occupant, place, state, level);
                return;
            case PlatformVerticalMovement.Sinking:
                SinkPlatform(occupant, place, state, samus, level, preHorizontalX);
                return;
            default:
                throw new InvalidDataException(
                    $"Platform Y movement {movement} is neither rising nor falling.");
        }
    }

    /// <summary>Returns a depressed platform toward its spawn height, stopping at its resting depth.</summary>
    private void RaisePlatform(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        // Normal platforms stop once they have risen from their depressed position past their resting height, just
        // below their original spawn center.
        if (place.Position.Y.Whole < state.RestingY)
        {
            state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
            SetPlatformVerticallyStillAnimation(place, state);
            return;
        }

        SetPlatformVerticallyMovingAnimation(place, state);
        IncrementAndClampPlatformYSpeed(occupant, state);
        SignedSixteenSixteen displacement = state.YSpeedCurvePlace.Speed.Negative;
        if (!_terrain.MoveEnemyVertically(level, place, displacement))
            return;

        state.YSpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        SetPlatformVerticallyStillAnimation(place, state);
    }

    /// <summary>Accelerates a ridden platform downward and publishes its collision-clipped displacement to Samus.</summary>
    /// <param name="occupant">The enemy holding <paramref name="place"/>.</param>
    /// <param name="place">The platform's place.</param>
    /// <param name="state">The platform's state.</param>
    /// <param name="samus">Samus, carried by the platform's accepted movement.</param>
    /// <param name="level">The room's collision data.</param>
    /// <param name="preHorizontalX">The platform's whole X before this update's horizontal movement.</param>
    private void SinkPlatform(
        RoomEnemyOccupant occupant, RoomEnemyPlace place,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int preHorizontalX)
    {
        // Sinking always accelerates.
        IncrementAndClampPlatformYSpeed(occupant, state);

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
    /// Steps the place one mark on along the quadratic speed curve, holding it at the cap its spawn settings name: as many
    /// marks on as their vertical acceleration updates.
    /// </summary>
    private static void IncrementAndClampPlatformYSpeed(RoomEnemyOccupant occupant, PlatformEnemyState state)
    {
        QuadraticSpeedCurvePlace cap = QuadraticSpeedCurvePlace.FromMarks(occupant.SpawnSettings<PlatformSpawnSettings>().VerticalAccelerationUpdates);
        state.YSpeedCurvePlace += QuadraticSpeedCurvePlace.OneMark;
        if (state.YSpeedCurvePlace >= cap)
            state.YSpeedCurvePlace = cap;
    }

    /// <summary>Starts moving art once per moving phase, whatever art a wall has started since (#2145).</summary>
    private static void SetPlatformVerticallyMovingAnimation(
        RoomEnemyPlace place,
        PlatformEnemyState state)
    {
        if (state.VerticalArt == PlatformVerticalArt.Moving)
            return;
        state.VerticalArt = PlatformVerticalArt.Moving;
        StartPlatformMovingAnimationForHeading(place, state);
    }

    /// <summary>Starts stationary art once per resting phase (#2145).</summary>
    private static void SetPlatformVerticallyStillAnimation(
        RoomEnemyPlace place,
        PlatformEnemyState state)
    {
        if (state.VerticalArt == PlatformVerticalArt.Still)
            return;
        state.VerticalArt = PlatformVerticalArt.Still;
        StartPlatformStillAnimationForHeading(place, state);
    }

    /// <summary>Selects the moving animation from which platform it is and its current horizontal direction.</summary>
    private static void StartPlatformMovingAnimationForHeading(
        RoomEnemyPlace place,
        PlatformEnemyState state)
    {
        StartPlatformAnimation(place, state, PlatformAnimationChoices.SelectAnimation(
            state.Kind, PlatformVerticalArt.Moving, state.XMovement));
    }

    /// <summary>Selects the stationary animation from which platform it is and its current horizontal direction.</summary>
    private static void StartPlatformStillAnimationForHeading(
        RoomEnemyPlace place,
        PlatformEnemyState state)
    {
        StartPlatformAnimation(place, state, PlatformAnimationChoices.SelectAnimation(
            state.Kind, PlatformVerticalArt.Still, state.XMovement));
    }

    /// <summary>Starts the selected animation, its first step playing on the next update.</summary>
    private static void StartPlatformAnimation(RoomEnemyPlace place, PlatformEnemyState state, PlatformSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, PlatformAnimations.StartingWith(segment));
    }

    /// <summary>Sets the direction main AI moves the platform, <paramref name="movement"/>, as a movement cue asks.</summary>
    private static void SetPlatformHorizontalMovementFromCue(
        RoomEnemyPlace place, PlatformEnemyState platformEnemyState,
        HorizontalDirection movement)
    {
        platformEnemyState.XMovement = movement;
    }

    /// <summary>Replaces a frozen Tripper's frame after common Bomb reaction using its movement direction.</summary>
    internal static EnemyBombReaction TripperBombReaction { get; } = new()
    {
        AfterCommon = c =>
        {
            if (c.Enemy.Freeze.IsRunning)
                c.Enemy.Frame = TripperFrozenFrame(c.Enemy.RequireSpeciesState<PlatformEnemyState>().XMovement);
        },
    };

    /// <summary>Runs Platforms's main AI.</summary>
    internal void RunPlatformsMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, PlatformEnemyState platformEnemyState, IEnemyAiFrame frame)
    {
        RunPlatformMain(occupant, place, platformEnemyState, frame.Samus, frame.Level);
    }

    /// <summary>Advances Platforms's typed animation.</summary>
    internal static void AdvancePlatformsTypedAnimation(RoomEnemyPlace place, PlatformEnemyState platformEnemyState) =>
        EnemyAnimationPlayback.Advance(place, platformEnemyState, PlatformAnimations.Segments,
            static (PlatformEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyPlatformCue(place, platformEnemyState, cue);
                return null;
            });

    /// <summary>Applies Tripper's direction-specific frozen frame after a successful common shot freeze.</summary>
    /// <remarks>After the common shot reaction freezes a Tripper at once, its current frame is replaced with its direction's
    /// frozen frame.</remarks>
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
