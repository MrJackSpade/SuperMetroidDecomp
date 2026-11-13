using SuperMetroid.Core.Programs;
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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Kamer2, EnemyDefinitionId.Tripper];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PlatformsEntity(this, slot, common);

    /// <summary>Clips Tripper/Kamer horizontal and vertical displacement against room terrain.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds the platform movement family to terrain collision.</summary>
    internal PlatformsBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of the platforms' animations: each publishes the horizontal direction main AI moves in.</summary>
    private static void ApplyPlatformCue(RoomEnemySlot slot, PlatformCue cue)
    {
        switch (cue.Kind)
        {
            case PlatformCueKind.MoveLeft:
                // The ordinary and duplicate commands are equivalent: both publish horizontal
                // dispatcher index zero before the next frame.
                SetPlatformHorizontalMovementFromCue(slot, PlatformHorizontalMovement.Left);
                break;
            case PlatformCueKind.MoveRight:
                SetPlatformHorizontalMovementFromCue(slot, PlatformHorizontalMovement.Right);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PlatformCueKind)} {cue.Kind}.");
        }
    }


    /// <summary>Recognizes the two enemy definitions sharing platform movement.</summary>
    internal static bool IsPlatformDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.Tripper or EnemyDefinitionId.Kamer2;

    /// <summary>Runs the two entry points and common initializer.</summary>
    internal static void InitializePlatform(RoomEnemySlot slot)
    {
        bool isKamer = slot.DefinitionId == EnemyDefinitionId.Kamer2;
        var state = new PlatformEnemyState();
        slot.SetSpeciesState(state);

        state.IsSuspensorPlatform = isKamer;
        // Main AI dispatches the complete value through a two-entry table, so a value outside it is corrupt room
        // data and is rejected here rather than normalized to rightward movement.
        PlatformSpawnSettings settings = slot.SpawnSettings<PlatformSpawnSettings>();
        state.XMovement = settings.Movement;

        state.Playback = new(PlatformAnimations.StartingWith(PlatformAnimationChoices.SelectAnimation(
            isKamer, axis: Axis.Horizontal, state.XMovement)), AnimationPlace.StartOfAnimation);

        // The spawn settings name the speed the platform moves at.
        state.RightSpeed = settings.Speed;
        state.LeftSpeed = SignedSixteenSixteen.Zero - settings.Speed;

        state.YMovement = PlatformVerticalMovement.Rising;
        state.PreviousYMovement = PlatformVerticalMovement.Rising;
        state.VerticallyMovingArtInstalled = false;
        state.VerticallyStillArtInstalled = false;
        state.RestingHeight = new Position16<RoomPixels>(0, slot.Position.Y.Whole) + new Offset16<RoomPixels>(0, 1);
        state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
        state.MaximumYSpeedTableIndex = EnemyQuadraticSpeedIndex.FromRows(settings.VerticalAccelerationUpdates);
    }

    /// <summary>Runs the Tripper and Kamer's main AI.</summary>
    private void RunPlatformMain(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Platform AI requires the active Samus actor.");
        if (level is null)
            throw new InvalidOperationException("Platform AI requires active room collision data.");

        // Every frame starts in the rising dispatcher and changes to sinking only when the
        // exact asymmetric "Samus from below" test succeeds. This is also the solid-
        // platform riding gate; ordinary radius overlap would trigger from the sides.
        state.YMovement = IsSamusRidingPlatform(slot, samus)
            ? PlatformVerticalMovement.Sinking
            : PlatformVerticalMovement.Rising;

        // The sinking carry measures the horizontal step from the position before it.
        int preHorizontalX = slot.Position.X.Whole;
        MovePlatformHorizontally(slot, state, level);
        MovePlatformVertically(slot, state, samus, level, preHorizontalX);

        // Main AI resets acceleration after running the first frame in a newly
        // selected vertical state. Preserve that slightly surprising ordering: a new phase
        // can use quadratic entry one now, then begins from entry zero on its next frame.
        if (state.YMovement != state.PreviousYMovement)
            state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
        state.PreviousYMovement = state.YMovement;
    }

    /// <summary>Exact wrapped-coordinate test of Samus riding the platform from above.</summary>
    internal static bool IsSamusRidingPlatform(RoomEnemySlot slot, SamusState samus)
    {
        int xDistance = CoordinateAxis.Wrap(samus.Position.X.Whole - slot.Position.X.Whole);
        if (CoordinateAxis.AsSigned(xDistance) < 0)
            xDistance = CoordinateAxis.Wrap(-xDistance);

        // A distance within Samus's radius goes directly to the Y test. Otherwise the
        // remainder must be strictly less than the platform radius; equality is not overlap.
        if (xDistance >= samus.Kinematics.Radius.X)
        {
            int remainder = CoordinateAxis.Wrap(xDistance - samus.Kinematics.Radius.X);
            if (remainder >= slot.Radius.X)
                return false;
        }

        // The +3 bias requires Samus's center to remain above the enemy center. After taking
        // the wrapped magnitude, Y accepts equality with the enemy radius (unlike X).
        int yDifference = CoordinateAxis.Wrap(samus.Position.Y.Whole + 3 - slot.Position.Y.Whole);
        if (CoordinateAxis.AsSigned(yDifference) >= 0)
            return false;
        int yDistance = CoordinateAxis.Wrap(-yDifference);
        if (yDistance < samus.Kinematics.Radius.Y)
            return true;
        int yRemainder = CoordinateAxis.Wrap(yDistance - samus.Kinematics.Radius.Y);
        return yRemainder <= slot.Radius.Y;
    }

    /// <summary>Applies horizontal movement, reversing the dispatcher and still-art animation on wall collision.</summary>
    private void MovePlatformHorizontally(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        bool movingLeft = state.XMovement switch
        {
            PlatformHorizontalMovement.Left => true,
            PlatformHorizontalMovement.Right => false,
            _ => throw new InvalidDataException(
                $"Platform X movement {state.XMovement} exceeds its two-entry table."),
        };
        SignedSixteenSixteen displacement = movingLeft ? state.LeftSpeed : state.RightSpeed;
        if (!_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, displacement))
            return;

        // A wall collision reverses the dispatcher and immediately installs the opposite
        // vertically-still animation. The first step of that animation republishes the same index
        // when the animations run later in this enemy frame.
        state.XMovement = movingLeft
            ? PlatformHorizontalMovement.Right
            : PlatformHorizontalMovement.Left;
        InstallPlatformVerticallyStillForCurrentDirection(slot, state);
    }

    /// <summary>Routes the platform through rise or sink physics and carries a riding Samus by accepted motion.</summary>
    /// <param name="slot">The platform's slot.</param>
    /// <param name="state">The platform's state.</param>
    /// <param name="samus">Samus, carried while riding.</param>
    /// <param name="level">The room's collision data.</param>
    /// <param name="preHorizontalX">The platform's whole X before this update's horizontal movement.</param>
    private void MovePlatformVertically(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int preHorizontalX)
    {
        switch (state.YMovement)
        {
            case PlatformVerticalMovement.Rising:
                RaisePlatform(slot, state, level);
                return;
            case PlatformVerticalMovement.Sinking:
                SinkPlatform(slot, state, samus, level, preHorizontalX);
                return;
            default:
                throw new InvalidDataException(
                    $"Platform Y movement {state.YMovement} exceeds its two-entry table.");
        }
    }

    /// <summary>Returns a depressed platform toward its spawn height with the one-pixel stop offset.</summary>
    private void RaisePlatform(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        RoomLevelData level)
    {
        // The signed comparison is intentional. Normal platforms stop once they have risen
        // from their depressed position to one pixel below their original spawn center.
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, state.RestingHeight.Y) < 0)
        {
            state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
            SetPlatformVerticallyStillAnimation(slot, state);
            return;
        }

        SetPlatformVerticallyMovingAnimation(slot, state);
        IncrementAndClampPlatformYSpeed(state);
        SignedSixteenSixteen displacement = state.YSpeedTableIndex.Row.Negative;
        if (!_terrain.MoveEnemyVertically(level, slot, displacement))
            return;

        state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
        SetPlatformVerticallyStillAnimation(slot, state);
    }

    /// <summary>Accelerates a ridden platform downward and publishes its collision-clipped displacement to Samus.</summary>
    /// <param name="slot">The platform's slot.</param>
    /// <param name="state">The platform's state.</param>
    /// <param name="samus">Samus, carried by the platform's accepted movement.</param>
    /// <param name="level">The room's collision data.</param>
    /// <param name="preHorizontalX">The platform's whole X before this update's horizontal movement.</param>
    private void SinkPlatform(
        RoomEnemySlot slot,
        PlatformEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int preHorizontalX)
    {
        // The sinking step performs a comparison followed by a call that does nothing; it
        // has no branch or side effect. Sinking therefore always continues to acceleration.
        IncrementAndClampPlatformYSpeed(state);

        // Add the wrapped whole-pixel horizontal step to Samus's extra displacement; fractional
        // displacement is deliberately untouched because this enemy carries only whole pixels.
        int xDelta = CoordinateAxis.Wrap(slot.Position.X.Whole - preHorizontalX);
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(CoordinateAxis.AsSigned(xDelta), 0);

        int preVerticalY = slot.Position.Y.Whole;
        SignedSixteenSixteen displacement = state.YSpeedTableIndex.Row.Positive;
        if (_terrain.MoveEnemyVertically(level, slot, displacement))
        {
            state.YSpeedTableIndex = EnemyQuadraticSpeedIndex.Start;
            SetPlatformVerticallyStillAnimation(slot, state);
        }

        // This delta is measured after collision clipping, so Samus follows the platform's
        // accepted motion rather than the requested quadratic speed.
        int yDelta = CoordinateAxis.Wrap(slot.Position.Y.Whole - preVerticalY);
        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, CoordinateAxis.AsSigned(yDelta));
    }

    /// <summary>
    /// Steps the place along the quadratic speed table one row on without crossing its variant-specific cap. The place
    /// stays within the first row through the cap (at most row 255) and one step can only reach row 256, so the game's
    /// signed comparison is an ordinary one.
    /// </summary>
    internal static void IncrementAndClampPlatformYSpeed(PlatformEnemyState state)
    {
        state.YSpeedTableIndex += EnemyQuadraticSpeedIndex.OneRow;
        if (state.YSpeedTableIndex >= state.MaximumYSpeedTableIndex)
            state.YSpeedTableIndex = state.MaximumYSpeedTableIndex;
    }

    /// <summary>Installs moving art once per phase and always clears the still-art flag at the merge label.</summary>
    private static void SetPlatformVerticallyMovingAnimation(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        if (!state.VerticallyMovingArtInstalled)
        {
            state.VerticallyMovingArtInstalled = true;
            InstallPlatformVerticallyMovingForCurrentDirection(slot, state);
        }
        // The direction cue runs after the already-installed branch target, so it executes even when
        // no new animation was needed this frame.
        state.VerticallyStillArtInstalled = false;
    }

    /// <summary>Installs stationary art once per phase and always clears the moving-art flag.</summary>
    private static void SetPlatformVerticallyStillAnimation(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        if (!state.VerticallyStillArtInstalled)
        {
            state.VerticallyStillArtInstalled = true;
            InstallPlatformVerticallyStillForCurrentDirection(slot, state);
        }
        // Same merge-label detail as the moving helper: the opposite flag is always clear.
        state.VerticallyMovingArtInstalled = false;
    }

    /// <summary>Selects the moving animation from suspensor identity and current horizontal dispatcher.</summary>
    internal static void InstallPlatformVerticallyMovingForCurrentDirection(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        StartPlatformAnimation(slot, state, PlatformAnimationChoices.SelectAnimation(
            state.IsSuspensorPlatform, axis: Axis.Vertical, state.XMovement));
    }

    /// <summary>Selects the stationary animation from suspensor identity and current horizontal dispatcher.</summary>
    internal static void InstallPlatformVerticallyStillForCurrentDirection(
        RoomEnemySlot slot,
        PlatformEnemyState state)
    {
        StartPlatformAnimation(slot, state, PlatformAnimationChoices.SelectAnimation(
            state.IsSuspensorPlatform, axis: Axis.Horizontal, state.XMovement));
    }

    /// <summary>Starts the selected animation with a one-tick frame timer.</summary>
    private static void StartPlatformAnimation(RoomEnemySlot slot, PlatformEnemyState state, PlatformSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, PlatformAnimations.StartingWith(segment));
    }

    /// <summary>Runs the four equivalent movement cues.</summary>
    private static void SetPlatformHorizontalMovementFromCue(
        RoomEnemySlot slot,
        PlatformHorizontalMovement movement)
    {
        slot.RequireSpeciesState<PlatformEnemyState>().XMovement = movement;
    }

    /// <summary>Replaces a frozen Tripper's map after common Bomb reaction using its movement direction.</summary>
    internal static void AddPlatformsBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Tripper &&
                                                       c.SelectedShotAi == EnemyReaction.TripperShot)
        {
            AfterCommon = c =>
            {
                if (c.Enemy.FrozenTimer != 0)
                    c.Enemy.Frame = TripperFrozenFrame(c.Enemy.RequireSpeciesState<PlatformEnemyState>().XMovement);
            },
        });
    }

    /// <summary>Runs Platforms's main AI.</summary>
    internal void RunPlatformsMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPlatformMain(slot, slot.RequireSpeciesState<PlatformEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Platforms's typed animation.</summary>
    internal static void AdvancePlatformsTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, PlatformAnimations.Segments,
            static (PlatformEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyPlatformCue(slot, cue);
                return null;
            });

    /// <summary>Applies Tripper's direction-specific frozen map after a successful common shot freeze.</summary>
    internal static void AddPlatformsShotReactions(EnemySharedReactions table)
    {
        // Tripper's private tail runs after common shot AI on the immediate-freeze path and replaces the current
        // frame with a direction-specific two-piece frozen map.
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Tripper &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.TripperShot)
        {
            AfterCommon = c =>
            {
                if (c.Froze)
                    c.Enemy.Frame = TripperFrozenFrame(c.Enemy.RequireSpeciesState<PlatformEnemyState>().XMovement);
            },
        });
    }

    /// <summary>Registers the platforms' ignoring touch so ordinary overlap does not damage Samus.</summary>
    internal static void AddPlatformsTouch(EnemySharedReactions table)
    {
        // Platform solidity and the asymmetric rider test live in other handlers; ordinary body overlap must neither
        // injure Samus nor synthesize knockback here.
        table.AddTouch(slot => IsPlatformDefinition(slot.DefinitionId) &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.Ignore,
            static _ => { });
    }

    /// <summary>
    /// The frozen Tripper's frame. The frozen draw tests the complete X-movement value for zero: every nonzero value shows
    /// the right-moving frame, including values outside the normal dispatcher domain.
    /// </summary>
    private static EnemyFrameId TripperFrozenFrame(PlatformHorizontalMovement direction) =>
        direction == PlatformHorizontalMovement.Left ? EnemyFrameId.TripperFrozenMovingLeft : EnemyFrameId.TripperFrozenMovingRight;
}
