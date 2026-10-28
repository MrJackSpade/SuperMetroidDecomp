using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>The Botwoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class BotwoonBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Botwoon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BotwoonEntity(this, slot, common);

    /// <summary>Active room services and native RNG used by path and drop decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Botwoon's thirteen body actors and spit projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns the wall-phase explosion sprites.</summary>
    private readonly RoomSpriteObjectPool _spriteObjects;
    /// <summary>Locates the head in physical enemy slot zero for body flashing.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Updates Botwoon's health-band OBJ colors.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Runs common touch behavior before Botwoon's special death tail.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns the sixteen special drops from the native chance table.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Binds Botwoon's head, body, palette, collision, and drop services.</summary>
    internal BotwoonBehavior(BotwoonBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _spriteObjects = dependencies.SpriteObjects;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
    }

    /// <summary>Number of independent chance-table pickups requested when the wall crumbles.</summary>
    private const int BotwoonSpecialDropCount = 16;

    /// <summary>Per-room movement, head-animation, body, and wall-collapse state.</summary>
    internal BotwoonEnemyState? BotwoonState { get; set; }
    /// <summary>Last sound selected by Botwoon in the current enemy frame.</summary>
    public SoundEffectId? LastBotwoonSoundEffect { get; internal set; }

    /// <summary>The value the pre-fall death count starts from once the tail completes its hole transition.</summary>
    private const int BotwoonDeathDelayStart = 240;
    /// <summary>The pre-fall death count at which the head starts falling.</summary>
    private const int BotwoonDeathDelayEnd = 256;
    /// <summary>Fall acceleration added each airborne update, in 256ths of a quadratic-speed step.</summary>
    private const int BotwoonDeathFallAcceleration = 192;
    /// <summary>Fall acceleration units per whole quadratic-speed step.</summary>
    private const int BotwoonDeathFallUnitsPerStep = 256;
    /// <summary>Updates the wall-explosion phase lasts before the boss is removed.</summary>
    private const int BotwoonWallExplosionDuration = 192;
    /// <summary>Wall-explosion update from which the explosion timers run.</summary>
    private const int BotwoonWallExplosionsBegin = 64;


    /// <summary>Track three, queued with the native eight-frame delay after wall cleanup.</summary>
    public DelayedMusicRequest? LastBotwoonMusicRequest { get; private set; }

    /// <summary>Ports <c>Botwoon_Init</c> at <c>$B3:9583</c>.</summary>
    internal void InitializeBotwoon(RoomEnemySlot head)
    {
        var state = new BotwoonEnemyState();
        BotwoonState = state;
        head.SetSpeciesState(state, state.ClearWithRecord);
        // `$B3:9583` installs InstList_Botwoon_Hide: the head starts hidden in its hole.
        state.HeadProgram = BotwoonHeadProgram.Hidden;
        state.HeadAnimationPosition = 0;
        head.InstructionTimer = WrappingCountdown.Start(1);
        head.Timer = WrappingCountdown.Start(0);

        // A previously defeated Botwoon does not allocate body actors. The room loader
        // installs the permanent open wall and makes both one-screen scroll entries blue.
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaMiniBoss))
        {
            _frameResults.BotwoonWall = PlmHeaderId.ClearBotwoonWall;
            head.Properties = head.Properties.With(EnemyProperties.Deleted);
            state.WallCrumbleRequested = true;
            state.BossBitSet = true;
            return;
        }

        // SpawnEprojWithGfx is called with arguments 24,22,...,0. The shared allocator
        // searches native slots $22 down to $00, so retaining both orderings is essential:
        // death delay is based on projectile slot while body spacing is based on argument.
        for (int argument = 24; argument >= 0; argument -= 2)
            SpawnBotwoonBodySegment(head, state, argument);

        state.Function = BotwoonEnemyFunction.InitialDelay;
        state.MovementFunction = BotwoonMovementFunction.MoveTowardHole;
        state.HeadFunction = BotwoonHeadFunction.AnimateFromMovement;
        state.InitialDelayTimer = WrappingCountdown.Start(256);
        var initialSpeed = BotwoonSpeedDefinitions.ForHealthPhase(0);
        state.Speed = initialSpeed.MovementSpeed;
        state.SegmentSpacingEntries = initialSpeed.SegmentSpacingEntries;
        state.InsideHole = true;
        state.PreviousInsideHole = true;
        state.InitialAction = true;
        state.TargetHoleIndex = 0;
        state.MaximumHealth = head.Health;
        state.HalfHealth = head.Health >> 1;
        state.QuarterHealth = head.Health >> 2;
        state.PreviousHealth = head.Health;
        state.PaletteFirstColour = SpriteImage.SpriteColourCount + head.PaletteRow * GameplayBasePaletteFormat.SpriteColorCount;
        // `$B3:961B` ORs $0400: the head starts hidden in its hole, out of every collision pass.
        head.Properties = head.Properties.With(EnemyProperties.IgnoreSamusCollision);

        for (int history = 0; history < 4; history++)
        {
            state.HeadHistory[history] = head.Position.Whole;
        }
        // BotwoonPositionHistory ($7E:9000-$93FF) is never seeded by InitAI_Botwoon. It
        // lies inside the $7E:7000-$97FF range Initialise_Enemies ($A0:8AA9) zeroes on room
        // load, so body segments read (0,0) until the ring has been written that far; the
        // freshly constructed state's arrays are that zeroed RAM.
    }

    /// <summary>Ports <c>Botwoon_Main</c> and its variable-D dispatcher.</summary>
    private void RunBotwoonMain(RoomEnemySlot head, BotwoonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Botwoon main AI requires the active Samus actor.");

        // A fatal hit does not delete Botwoon. He keeps following his current path until
        // the tail has crossed the next hole, then the synchronized head/body death begins.
        if (state.PendingDeath && state.ExitTransitionComplete)
        {
            state.BodyDeathStarted = true;
            state.Function = BotwoonEnemyFunction.DeathDelay;
            state.DeathTimer = BotwoonDeathDelayStart;
            state.PendingDeath = false;
            state.ExitTransitionComplete = false;
        }

        switch (state.Function)
        {
            case BotwoonEnemyFunction.InitialDelay:
                state.InitialDelayTimer = state.InitialDelayTimer.Decrement().Remaining;
                if (state.InitialDelayTimer.IsZero)
                    state.Function = BotwoonEnemyFunction.ChooseNextAction;
                break;

            case BotwoonEnemyFunction.ChooseNextAction:
                RunBotwoonActionSelector(head, state, samus);
                break;

            case BotwoonEnemyFunction.FollowAuthoredPath:
                RunBotwoonAuthoredPathState(head, state, samus);
                break;

            case BotwoonEnemyFunction.SpitWhileHidden:
                RunBotwoonHiddenSpitState(head, state, samus);
                break;

            case BotwoonEnemyFunction.DeathDelay:
                state.DeathTimer++;
                if (state.DeathTimer >= BotwoonDeathDelayEnd)
                    state.Function = BotwoonEnemyFunction.HeadFalling;
                break;

            case BotwoonEnemyFunction.HeadFalling:
                RunBotwoonHeadFall(head, state);
                break;

            case BotwoonEnemyFunction.WaitForBody:
                if (state.LastBodySegmentLanded)
                    BeginBotwoonWallExplosions(head, state);
                break;

            case BotwoonEnemyFunction.WallExplosions:
                RunBotwoonWallExplosions(head, state);
                break;

            default:
                throw new InvalidDataException(
                    $"Botwoon function {state.Function} is not translated.");
        }

        UpdateBotwoonHealthPalette(head, state);
    }

    /// <summary>Chooses an authored traversal or hidden spit action after the current path completes.</summary>
    private void RunBotwoonActionSelector(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (!state.PathComplete)
        {
            RunBotwoonMovingFrame(head, state, samus);
            DetectBotwoonHole(head, state);
            return;
        }

        state.PathComplete = false;
        int choice = 0;
        if (!state.InsideHole && !state.InitialAction && state.HealthPhase == 0)
            choice = _room.Random.Generate() & BotwoonNavigationDefinitions.ActionChoiceMask;
        state.InitialAction = false;

        if (choice is 0 or 2 or 4)
        {
            state.Function = BotwoonEnemyFunction.FollowAuthoredPath;
            state.MovementFunction = BotwoonMovementFunction.LoadAuthoredPath;
            state.AttackTimer = WrappingCountdown.Start(0);
            state.HeadFunction = BotwoonHeadFunction.AnimateFromMovement;
            ChooseBotwoonPath(head, state);
        }
        else
        {
            state.Function = BotwoonEnemyFunction.SpitWhileHidden;
            state.HeadFunction = BotwoonHeadFunction.AimAtSamus;
            state.AttackTimer = WrappingCountdown.Start(48);
            // `$B3:992C` (AND #$FBFF) makes the spitting head tangible again.
            head.Properties = head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
        }
    }

    /// <summary>Finishes a path at its terminal marker, then returns to action selection.</summary>
    private void RunBotwoonAuthoredPathState(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (state.PathComplete)
        {
            state.PathComplete = false;
            state.Function = BotwoonEnemyFunction.ChooseNextAction;
            state.MovementFunction = BotwoonMovementFunction.MoveTowardHole;
            if (state.InsideHole)
                state.HoleLatch = false;
            return;
        }

        RunBotwoonMovingFrame(head, state, samus);
    }

    /// <summary>Waits for the spit timer, then chooses whether the next path begins inside a hole.</summary>
    private void RunBotwoonHiddenSpitState(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (!state.AttackTimer.IsZero)
        {
            RunBotwoonHeadFunction(head, state, samus);
            return;
        }

        state.PathComplete = false;
        state.Function = BotwoonEnemyFunction.FollowAuthoredPath;
        state.MovementFunction = BotwoonMovementFunction.LoadAuthoredPath;
        state.HeadFunction = BotwoonHeadFunction.AnimateFromMovement;

        state.InsideHole = !state.PendingDeath && (_room.Random.Generate() & 1) != 0;
        state.PreviousInsideHole = state.InsideHole;
        if (!state.InsideHole)
        {
            state.PathChoice = 0;
        }
        else
        {
            state.HoleLatch = false;
            state.SavedHoleRingIndex = null;
        }
        ChooseBotwoonPath(head, state);
    }

    /// <summary>Moves the head, writes position history, places and orients body segments, then tests holes.</summary>
    private void RunBotwoonMovingFrame(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        RunBotwoonMovement(head, state);
        RecordBotwoonHeadPosition(head, state);
        PositionBotwoonBody(state);
        RunBotwoonHeadFunction(head, state, samus);
        OrientBotwoonBody(head, state);
        state.RingIndex = BotwoonNavigationDefinitions.WrapHistoryIndex(state.RingIndex + 1);
    }

    /// <summary>Dispatches movement toward a hole or through an authored path descriptor.</summary>
    internal static void RunBotwoonMovement(RoomEnemySlot head, BotwoonEnemyState state)
    {
        switch (state.MovementFunction)
        {
            case BotwoonMovementFunction.MoveTowardHole:
                MoveBotwoonTowardHole(head, state);
                return;
            case BotwoonMovementFunction.LoadAuthoredPath:
                LoadBotwoonPathDescriptor(state);
                FollowBotwoonPath(head, state);
                return;
            case BotwoonMovementFunction.FollowAuthoredPath:
                FollowBotwoonPath(head, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Botwoon movement {state.MovementFunction} is not translated.");
        }
    }

    /// <summary>Steers the head toward its selected hole until inside/outside state changes.</summary>
    private static void MoveBotwoonTowardHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        BotwoonHoleDefinition hole =
            BotwoonNavigationDefinitions.HoleAt(state.TargetHoleIndex);
        Offset16<RoomPixels> delta = hole.Target - head.Position.Whole;
        Bearing bearing = EnemyMath.CalculateBearing(new(
            ClampBotwoonTargetDelta(delta.X), ClampBotwoonTargetDelta(delta.Y)));
        state.MovementAngle = bearing.ToTableAngle().ToAngle();

        if (state.InsideHole == state.PreviousInsideHole)
        {
            AddBotwoonAngleVector(head, state.MovementAngle.ToByteAngle(), state.Speed);
        }
        else
        {
            state.PreviousInsideHole = state.InsideHole;
            state.PathComplete = true;
        }
    }

    /// <summary>Restricts target distance to the signed angle routine's supported range.</summary>
    private static short ClampBotwoonTargetDelta(int value) =>
        (short)(value < -256 ? -255 : value >= 256 ? 255 : value);

    /// <summary>Uses health phase, current hole, and native RNG to choose an authored path.</summary>
    private void ChooseBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        UpdateBotwoonHealthPhase(head, state);
        int hiddenBase = state.InsideHole ? BotwoonNavigationDefinitions.HiddenPathChoiceBase : 0;
        int variant = (_room.Random.Generate() & BotwoonNavigationDefinitions.PathChoiceMask) >>
            BotwoonNavigationDefinitions.PathChoiceVariantShift;
        state.PathChoice = variant + hiddenBase + BotwoonNavigationDefinitions.PathVariantsPerHole * state.TargetHoleIndex;
    }

    /// <summary>Changes traversal speed and body spacing at half and quarter health outside holes.</summary>
    internal static void UpdateBotwoonHealthPhase(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.InsideHole)
            return;

        int phase = 0;
        if (head.Health != 0 && head.Health < state.HalfHealth)
            phase = head.Health < state.QuarterHealth ? 2 : 1;
        state.HealthPhase = phase;
        var speed = BotwoonSpeedDefinitions.ForHealthPhase(phase);
        state.Speed = speed.MovementSpeed;
        state.SegmentSpacingEntries = speed.SegmentSpacingEntries;
    }

    /// <summary>Loads a path pointer, direction, and destination hole from the chosen descriptor.</summary>
    private static void LoadBotwoonPathDescriptor(BotwoonEnemyState state)
    {
        BotwoonPathDescriptorDefinition descriptor =
            BotwoonNavigationDefinitions.PathForChoice(state.PathChoice);
        state.PathSampleIndex = descriptor.FirstSampleIndex;
        state.PathDirection = descriptor.Direction;
        state.TargetHoleIndex = descriptor.TargetHoleIndex;
        // A reverse stream starts at its closing separator; the first sample read is two before it.
        if (state.PathDirection < 0)
            state.PathSampleIndex -= 2;
        state.PathComplete = false;
        state.MovementFunction = BotwoonMovementFunction.FollowAuthoredPath;
    }

    /// <summary>Consumes path samples according to current speed until a terminal sentinel is reached.</summary>
    private static void FollowBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        Offset16<RoomPixels> total = Offset16<RoomPixels>.Zero;
        int step = state.PathDirection < 0 ? -1 : 1;
        for (int sample = 0; sample < state.Speed; sample++)
        {
            Offset16<RoomPixels> movement =
                BotwoonNavigationDefinitions.MovementSample(state.PathSampleIndex);
            if (movement.X == sbyte.MinValue)
            {
                state.PathComplete = true;
                return;
            }
            if (movement.Y == sbyte.MinValue)
            {
                state.PathComplete = true;
                return;
            }
            total += movement;
            state.PathSampleIndex += step;
        }

        head.Position += state.PathDirection < 0 ? -total : total;
    }

    /// <summary>Writes the head's whole-pixel position into the 1024-byte history ring.</summary>
    private static void RecordBotwoonHeadPosition(RoomEnemySlot head, BotwoonEnemyState state)
    {
        state.History[state.RingIndex] = head.Position.Whole;
    }

    /// <summary>Spaces thirteen body projectiles along older head-history samples.</summary>
    private static void PositionBotwoonBody(BotwoonEnemyState state)
    {
        int historyIndex = BotwoonNavigationDefinitions.WrapHistoryIndex(state.RingIndex - state.SegmentSpacingEntries);
        for (int argument = 24; argument >= 0; argument -= 2)
        {
            int segmentIndex = argument >> 1;
            RoomEnemyProjectileSlot segment = state.Segment(segmentIndex);

            ToggleBotwoonSegmentAtHole(state, segmentIndex, historyIndex);
            segment.Position = segment.Position.WithWhole(state.History[historyIndex]);
            historyIndex = BotwoonNavigationDefinitions.WrapHistoryIndex(historyIndex - state.SegmentSpacingEntries);
        }
    }

    /// <summary>Changes a segment's collision mode when its delayed history cursor crosses a hole.</summary>
    private static void ToggleBotwoonSegmentAtHole(
        BotwoonEnemyState state,
        int segmentIndex,
        int historyIndex)
    {
        if (state.SavedHoleRingIndex != historyIndex)
            return;

        RoomEnemyProjectileSlot segment = state.Segment(segmentIndex);
        bool wasInside = state.SegmentInsideHole[segmentIndex];
        state.SegmentInsideHole[segmentIndex] = !wasInside;
        segment.CanDamageSamus = wasInside;
        segment.CollisionOption = wasInside ? EnemyProjectileCollisionOption.IndestructibleDud : EnemyProjectileCollisionOption.NoCollision;

        if (segmentIndex != 0)
            return;
        state.HoleLatch = false;
        state.ExitTransitionComplete = !state.InsideHole;
        state.SavedHoleRingIndex = null;
    }

    /// <summary>Faces each body sprite toward the preceding segment or head.</summary>
    private static void OrientBotwoonBody(RoomEnemySlot head, BotwoonEnemyState state)
    {
        for (int argument = 24; argument >= 0; argument -= 2)
        {
            int segmentIndex = argument >> 1;
            RoomEnemyProjectileSlot segment = state.Segment(segmentIndex);
            int extraAngle = state.SegmentInsideHole[segmentIndex] ? 256 : 0;
            Offset16<RoomPixels> toLeader;
            if (argument == 24)
            {
                toLeader = head.Position.Whole - segment.Position.Whole;
            }
            else
            {
                RoomEnemyProjectileSlot previous = state.Segment(segmentIndex + 1);
                toLeader = previous.Position.Whole - segment.Position.Whole;
                if (argument == 0)
                    extraAngle += 512;
            }

            // The bearing word plus at most 768 keeps the selector's sixteen-bit wrap; the
            // selector itself is at most 4094.
            int angle = unchecked((ushort)(extraAngle + EnemyMath.CalculateBearing(toLeader).RawValue));
            segment.DirectionParameter = 2 * (angle >> 5);
        }
    }

    /// <summary>Latches the first hole rectangle entered by the head and marks its history offset.</summary>
    internal static void DetectBotwoonHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.HoleLatch)
            return;

        // The cartridge tests the holes from the last to the first and latches the first it finds.
        for (int holeIndex = BotwoonNavigationDefinitions.HoleCount - 1; holeIndex >= 0; holeIndex--)
        {
            BotwoonHoleDefinition hole = BotwoonNavigationDefinitions.HoleAt(holeIndex);
            Offset16<RoomPixels> fromTopLeft = head.Position.Whole - hole.TopLeft;
            Offset16<RoomPixels> fromBottomRight = head.Position.Whole - hole.BottomRight;
            if (fromTopLeft.X >= 0 && fromBottomRight.X < 0 &&
                fromTopLeft.Y >= 0 && fromBottomRight.Y < 0)
            {
                state.HoleLatch = true;
                state.InsideHole = !state.InsideHole;
                state.SavedHoleRingIndex = state.RingIndex;
                return;
            }
        }
        state.HoleLatch = false;
    }

    /// <summary>Dispatches head animation, aim, spit-frame wait, immediate volley, or cooldown.</summary>
    private void RunBotwoonHeadFunction(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        switch (state.HeadFunction)
        {
            case BotwoonHeadFunction.AnimateFromMovement:
                AnimateBotwoonHeadFromMovement(head, state);
                return;
            case BotwoonHeadFunction.AimAtSamus:
                AimBotwoonAtSamus(head, state, samus);
                return;
            case BotwoonHeadFunction.WaitForSpitFrame:
                if (state.SpitFrameReached)
                {
                    SpawnBotwoonSpitVolley(head, state, count: 5, startOffset: -32);
                    state.SpitFrameReached = false;
                    state.HeadFunction = BotwoonHeadFunction.AttackCooldown;
                }
                return;
            case BotwoonHeadFunction.SpawnImmediateVolley:
                SpawnBotwoonSpitVolley(head, state, count: 3, startOffset: -16);
                state.HeadFunction = BotwoonHeadFunction.AttackCooldown;
                return;
            case BotwoonHeadFunction.AttackCooldown:
                CountdownStep cooldown = state.AttackTimer.Decrement();
                state.AttackTimer = cooldown.Remaining;
                if (cooldown.IsNegative)
                {
                    state.AttackTimer = WrappingCountdown.Start(0);
                    state.HeadFunction = BotwoonHeadFunction.AnimateFromMovement;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Botwoon head callback {state.HeadFunction} is not translated.");
        }
    }

    /// <summary>Uses recent head displacement to select visible facing or hidden hole animation.</summary>
    internal static void AnimateBotwoonHeadFromMovement(RoomEnemySlot head, BotwoonEnemyState state)
    {
        Offset16<RoomPixels> moved = head.Position.Whole - state.HeadHistory[3];
        if (moved.X != 0 || moved.Y != 0)
        {
            BotwoonHeadProgram program;
            if (state.InsideHole)
            {
                head.Layer = 7;
                // `$B3:9DF7` ORs $0400 while the head travels inside the wall.
                head.Properties = head.Properties.With(EnemyProperties.IgnoreSamusCollision);
                program = BotwoonHeadProgram.Hidden;
            }
            else
            {
                head.Layer = 2;
                // `$B3:9E10` (AND #$FBFF) restores collision once the head is outside.
                head.Properties = head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                program = BotwoonHeadProgram.MovingToward(EnemyMath.CalculateBearing(moved));
            }
            InstallBotwoonHeadProgram(head, state, program);
        }

        for (int index = 3; index > 0; index--)
        {
            state.HeadHistory[index] = state.HeadHistory[index - 1];
        }
        state.HeadHistory[0] = head.Position.Whole;
    }

    /// <summary>Aims the spit program at Samus and selects delayed or immediate volley timing.</summary>
    internal void AimBotwoonAtSamus(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        head.Layer = 2;
        Bearing bearing = EnemyMath.CalculateBearing(
            new(unchecked((short)(samus.Position.X.Whole - head.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - head.Position.Y.Whole))));
        InstallBotwoonHeadProgram(head, state, BotwoonHeadProgram.SpittingToward(bearing));
        state.SpitAngle = bearing.ToTableAngle().ToAngle();
        state.HeadFunction = state.Function == BotwoonEnemyFunction.SpitWhileHidden
            ? BotwoonHeadFunction.WaitForSpitFrame
            : BotwoonHeadFunction.SpawnImmediateVolley;
        RunBotwoonHeadFunction(head, state, samus);
    }

    /// <summary>Restarts the head instruction timer only when its selected program changes.</summary>
    private static void InstallBotwoonHeadProgram(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        BotwoonHeadProgram program)
    {
        if (program == state.HeadProgram)
            return;
        state.HeadProgram = program;
        state.HeadAnimationPosition = 0;
        head.InstructionTimer = WrappingCountdown.Start(1);
        head.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Advances Botwoon's typed animation.</summary>
    internal void AdvanceBotwoonTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        AdvanceBotwoonHeadAnimation(slot, RequireBotwoonState(slot));
    }

    /// <summary>
    /// Runs Botwoon's head animation where the enemy instruction interpreter would: the frame
    /// expires on the update its native timer would, and a newly shown frame raises the
    /// new-frame flag exactly as a native timed frame does.
    /// </summary>
    internal void AdvanceBotwoonHeadAnimation(RoomEnemySlot head, BotwoonEnemyState state)
    {
        CountdownStep instructionTimer = head.InstructionTimer.Decrement();
        head.InstructionTimer = instructionTimer.Remaining;
        if (!instructionTimer.IsZero)
        {
            head.ExtraProperties = head.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        var cursor = new AnimationProgramCursor<BotwoonHeadProgram>(state.HeadProgram, state.HeadAnimationPosition);
        AnimationProgramAdvance<BotwoonHeadFrame> advance = BotwoonAnimations.HeadPrograms.Advance(ref cursor,
            cue => ApplyBotwoonHeadCue(head, state, cue));
        state.HeadAnimationPosition = cursor.Position;
        // A hold is the native sleep: the timer wraps from zero and the last frame stays.
        if (advance is AnimationProgramAdvance<BotwoonHeadFrame>.Hold)
            return;
        if (advance is not AnimationProgramAdvance<BotwoonHeadFrame>.Show shown)
            throw new InvalidOperationException($"Botwoon's head program ended an update with {advance}.");
        head.InstructionTimer = WrappingCountdown.FromRaw((ushort)shown.Duration);
        head.Frame = BotwoonVisualDefinitions.HeadFrames.Of(shown.Frame);
        head.ExtraProperties = head.ExtraProperties.With(EnemyExtraProperties.NewInstructionFrame);
    }

    /// <summary>Applies frame cues for facing radius, spit audio, or the volley trigger.</summary>
    private void ApplyBotwoonHeadCue(RoomEnemySlot head, BotwoonEnemyState state, BotwoonHeadCue cue)
    {
        switch (cue)
        {
            case BotwoonHeadCue.ApplyFacingRadius:
                head.Radius = BotwoonAnimations.HeadRadius(state.HeadProgram.Facing);
                return;
            case BotwoonHeadCue.QueueSpitSound:
                LastBotwoonSoundEffect = BotwoonSoundDefinitions.Spit;
                return;
            case BotwoonHeadCue.ReachSpitFrame:
                state.SpitFrameReached = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BotwoonHeadCue)} {cue}.");
        }
    }

    /// <summary>Emits three or five spit projectiles at sixteen-angle increments around the aimed direction.</summary>
    private void SpawnBotwoonSpitVolley(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        int count,
        int startOffset)
    {
        int speed = BotwoonSpeedDefinitions.ForHealthPhase(state.HealthPhase).SpitSpeed;
        ByteAngle angle = state.SpitAngle.AddTableUnits(startOffset).ToByteAngle();
        for (int projectile = 0; projectile < count; projectile++)
        {
            SpawnBotwoonSpit(head, angle, speed);
            angle = angle.AddTableUnits(16);
        }
    }

    /// <summary>Copies the next health-band OBJ palette when Botwoon's health crosses its threshold.</summary>
    internal void UpdateBotwoonHealthPalette(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.PalettePhase == BotwoonHealthPaletteDefinitions.CompletePhase)
            return;
        if (!BotwoonHealthPaletteDefinitions.ShouldAdvance(state.PalettePhase, head.Health))
            return;

        int destinationColor = state.PaletteFirstColour;
        int colorCount = 256 - destinationColor;
        if (_bossArtwork.Artwork?.BotwoonColors is { } colors)
        {
            // The retail actor owns OBJ palette seven. A restored/non-retail offset
            // would copy past this authored image into adjacent ROM data; do not
            // silently substitute a different visual result for that state.
            if (destinationColor != BotwoonHealthPaletteDefinitions.FirstColour ||
                colorCount != BotwoonHealthPaletteDefinitions.ColorsPerPalette)
                throw new InvalidDataException(
                    $"Botwoon palette destination {destinationColor} is outside the retail sprite palette.");
            int band = state.PalettePhase;
            for (int color = 0; color < colorCount; color++)
                _presentation.BossColours().BotwoonBody.Write(color, colors.HealthColor(band, color));
        }
        else throw new InvalidOperationException("Botwoon requires installed health colors.");
        state.PalettePhase++;
    }

    /// <summary>Accelerates the defeated head down to Y=200, then hides it and waits for body landing.</summary>
    private void RunBotwoonHeadFall(RoomEnemySlot head, BotwoonEnemyState state)
    {
        int displacement = EnemyMath.ReadQuadraticEnemySpeed(
            checked((ushort)(state.DeathFallAccumulator / BotwoonDeathFallUnitsPerStep)),
            negative: false);
        head.Position = head.Position.AdvanceY(SignedSixteenSixteen.FromRaw(displacement));
        if (unchecked((short)(head.Position.Y.Whole - 200)) < 0)
        {
            state.DeathFallAccumulator += BotwoonDeathFallAcceleration;
            return;
        }

        head.Position = head.Position.WithWholeY(200);
        state.Function = BotwoonEnemyFunction.WaitForBody;
        _projectiles.SpawnRoomGraphicsDustExplosion(head.Position.Whole, MiscDustAnimation.BigExplosion);
        LastBotwoonSoundEffect = BotwoonSoundDefinitions.Explosion;
        head.Properties = head.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
    }

    /// <summary>Requests the crumble PLM and sixteen RNG-positioned drops before wall explosions.</summary>
    private void BeginBotwoonWallExplosions(RoomEnemySlot head, BotwoonEnemyState state)
    {
        state.Function = BotwoonEnemyFunction.WallExplosions;
        _frameResults.BotwoonWall = PlmHeaderId.CrumbleBotwoonWall;
        state.WallCrumbleRequested = true;

        // `Enemy_ItemDrop_Botwoon` at `$A0:BA3E` emits sixteen independent pickup
        // requests. Each position uses both bytes of one RNG result: X samples low seven
        // bits and Y samples bits 8..13. Each pickup is materialized immediately as
        // a real $F337 actor from the head definition's item chances.
        for (int dropIndex = 0; dropIndex < BotwoonSpecialDropCount; dropIndex++)
        {
            int random = _room.Random.Generate();
            var position = new Position16<RoomPixels>(
                unchecked((ushort)((random & 127) + 64)),
                unchecked((ushort)((SharedRandomState.UpperByte(random) & 63) + 128)));
            _deathsAndDrops.SpawnEnemyDropFromChanceTable(
                position,
                head.Definition.DropChances);
        }
        state.DropRequested = true;
        state.WallExplosionFrame = 0;
        state.LargeExplosionTimer = WrappingCountdown.Start(0);
        state.SmallExplosionTimer = WrappingCountdown.Start(0);
    }

    /// <summary>Runs the 192-frame wall sequence, then sets the boss bit and queues delayed music.</summary>
    private void RunBotwoonWallExplosions(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.WallExplosionFrame >= BotwoonWallExplosionDuration)
        {
            head.Properties = head.Properties.With(EnemyProperties.Deleted);
            state.BossBitSet = true;
            _room.Services.Bosses.SetDefeated(BossBits.AreaMiniBoss);
            // `$B3:9B32` invokes QueueMusic_Delayed8(3) only after the complete 192-frame
            // wall-explosion phase, not when health first reaches zero.
            LastBotwoonMusicRequest = new DelayedMusicRequest(
                MusicRequest.SelectTrack(3),
                MusicQueueDelay.Shortest);
            return;
        }

        if (state.WallExplosionFrame >= BotwoonWallExplosionsBegin)
        {
            CountdownStep largeExplosionTimer =
                state.LargeExplosionTimer.Decrement();
            state.LargeExplosionTimer = largeExplosionTimer.Remaining;
            if (largeExplosionTimer.IsNegative)
            {
                state.LargeExplosionTimer = WrappingCountdown.Start(12);
                _spriteObjects.SpawnRoomSpriteObject(
                    new(unchecked((ushort)((_room.Random.Generate() & 31) + 232)), unchecked((ushort)(state.WallExplosionFrame + (_room.Random.Generate() & 31) - 8))),
                    RoomSpriteObjectKind.BotwoonLargeExplosion,
                    graphics: SpriteGraphics.InPalette(5));
                LastBotwoonSoundEffect = BotwoonSoundDefinitions.Explosion;
            }

            CountdownStep smallExplosionTimer =
                state.SmallExplosionTimer.Decrement();
            state.SmallExplosionTimer = smallExplosionTimer.Remaining;
            if (smallExplosionTimer.IsNegative)
            {
                state.SmallExplosionTimer = WrappingCountdown.Start(4);
                for (int explosion = 0; explosion < 2; explosion++)
                {
                    _spriteObjects.SpawnRoomSpriteObject(
                        new(unchecked((ushort)((_room.Random.Generate() & 63) + 224)), unchecked((ushort)(state.WallExplosionFrame + (_room.Random.Generate() & 31) - 8))),
                        RoomSpriteObjectKind.BotwoonSmallExplosion,
                        graphics: SpriteGraphics.InPalette(5));
                }
            }
        }
        state.WallExplosionFrame++;
    }

    /// <summary>
    /// Runs the private tail shared by <c>Botwoon_Touch</c>, <c>Botwoon_Shot</c>, and
    /// <c>Botwoon_Powerbomb</c>. All three callbacks deliberately use a common damage helper
    /// that skips the ordinary death animation: zero health is only a request to finish the
    /// current traversal. The head and thirteen body actors remain alive until the tail has
    /// crossed the next hole, exactly as <c>$B3:96C6</c> requires.
    /// </summary>
    internal void ResolveBotwoonCombatAfterCommon(RoomEnemySlot head)
    {
        if (head.Health != 0)
            return;

        BotwoonEnemyState state = RequireBotwoonState(head);
        state.PendingDeath = true;

        // `$B3:96F5` is `ORA #$0400` (ROM bytes 09 00 04): the dying head leaves the
        // projectile, bomb and touch passes, so a later hit cannot interrupt its fall.
        head.Properties = head.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Applies the native angle table's signed X/Y displacement to the head.</summary>
    private static void AddBotwoonAngleVector(
        RoomEnemySlot head,
        ByteAngle angle,
        int magnitude)
    {
        int xMagnitude = EnemyMath.ReadAbsoluteCosineProduct(angle, checked((ushort)magnitude));
        int yMagnitude = EnemyMath.ReadAbsoluteSineProduct(angle, checked((ushort)magnitude));
        int xDisplacement = EnemyMath.CosineIsNegative(angle) ? -xMagnitude : xMagnitude;
        int yDisplacement = EnemyMath.SineIsNegative(angle) ? -yMagnitude : yMagnitude;
        head.Position = head.Position.AdvanceX(SignedSixteenSixteen.FromRaw(xDisplacement));
        head.Position = head.Position.AdvanceY(SignedSixteenSixteen.FromRaw(yDisplacement));
    }

    /// <summary>Requires the initialized Botwoon head in physical slot zero.</summary>
    internal BotwoonEnemyState RequireBotwoonState(RoomEnemySlot head) =>
        BotwoonState is not null && head.SlotIndex == 0
            ? BotwoonState
            : throw new InvalidOperationException(
                $"Enemy slot {head.SlotIndex} has no initialized Botwoon state.");

    /// <summary>Routes bomb hits through the shared shot callback without generic death animation.</summary>
    internal void AddBotwoonBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Botwoon &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.BotwoonShot)
        {
            RunsGenericDeath = false,
            BeforeCommon = c => RequireBotwoonState(c.Enemy).PreviousHealth = c.HealthBefore,
            AfterCommon = c => ResolveBotwoonCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Registers body-segment and spit pre-instruction handlers.</summary>
    internal void AddBotwoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunBotwoonBodyPreInstruction(context.Projectile, context.FrameCounter8);
        }, EnemyProjectilePreInstruction.BotwoonsBody);

        table.AddPreInstruction(context =>
        {
            RunBotwoonSpitPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.BotwoonsSpit);
    }

    /// <summary>Ports Botwoon's main AI.</summary>
    internal void RunBotwoonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBotwoonMain(slot, RequireBotwoonState(slot), frame.Samus);
    }

    /// <summary>Applies power-bomb damage while preserving Botwoon's delayed death traversal.</summary>
    internal void AddBotwoonPowerBombReactions(EnemySharedReactions table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Botwoon &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.BotwoonPowerBomb)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = (enemy, _) => ResolveBotwoonCombatAfterCommon(enemy),
        });
    }

    /// <summary>Records pre-hit health for palette phases and skips generic death on shot hits.</summary>
    internal void AddBotwoonShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Botwoon &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BotwoonShot)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = c =>
            {
                RequireBotwoonState(c.Enemy).PreviousHealth = c.HealthBefore;
                ResolveBotwoonCombatAfterCommon(c.Enemy);
            },
        });
    }

    /// <summary>Resolves Samus contact while preserving Botwoon's special zero-health tail.</summary>
    internal void AddBotwoonTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Botwoon &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.BotwoonTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
                ResolveBotwoonCombatAfterCommon(context.Slot);
            });
    }
    /// <summary>Ports <c>EprojInit_BotwoonsBody</c> at <c>$86:EA31</c>.</summary>
    internal void SpawnBotwoonBodySegment(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        int spawnArgument)
    {
        RoomEnemyProjectileSlot? segment = _projectiles.AllocateEnemyProjectile();
        if (segment is null)
        {
            throw new InvalidOperationException(
                "Botwoon requires thirteen free enemy-projectile slots during room load.");
        }

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            segment,
            RoomEnemyProjectileKind.BotwoonBody,
            head.Graphics);
        segment.Position = FixedPosition<RoomPixels>.FromWhole(head.Position.Whole);
        segment.Velocity = segment.Velocity.WithY(new SignedEightEight(0));

        // ai_var_A is the live spawn argument. Every nonzero segment begins with orientation
        // $10; argument zero is the tail cap and begins at $30.
        int orientation = spawnArgument != 0 ? 16 : 48;
        segment.StartAnimation(BotwoonProjectileAnimation.ForSegment(BotwoonSegmentProgram.FromSelector(orientation)));
        segment.DirectionParameter = orientation;

        int segmentIndex = spawnArgument >> 1;
        state.BodySegments[segmentIndex] = segment;
        state.SegmentFunctions[segmentIndex] = BotwoonBodyFunction.Main;
        state.SegmentInsideHole[segmentIndex] = true;
        segment.CollisionOption = EnemyProjectileCollisionOption.NoCollision;
        segment.CanDamageSamus = false;
    }

    /// <summary>Ports <c>EprojPreInstr_BotwoonsBody</c> and its four function states.</summary>
    internal void RunBotwoonBodyPreInstruction(
        RoomEnemyProjectileSlot segment,
        int randomEnemyCounter)
    {
        BotwoonEnemyState state = BotwoonState ?? throw new InvalidOperationException(
            "A live Botwoon body segment has no owning head state.");
        int segmentIndex = Array.IndexOf(state.BodySegments, segment);
        if (segmentIndex < 0)
            throw new InvalidOperationException($"Projectile slot {segment.SlotIndex} is not one of Botwoon's body segments.");
        BotwoonBodyFunction function = state.SegmentFunctions[segmentIndex];
        if (state.BodyDeathStarted && function == BotwoonBodyFunction.Main)
        {
            function = BotwoonBodyFunction.BeginDying;
            state.SegmentFunctions[segmentIndex] = function;
        }

        switch (function)
        {
            case BotwoonBodyFunction.Main:
                AnimateBotwoonBodySegment(segment, randomEnemyCounter);
                return;

            case BotwoonBodyFunction.BeginDying:
                // The delay is based on native projectile index, not body order. Botwoon
                // normally owns indexes $0A..$22 because allocation descended from $22.
                segment.DirectionParameter = unchecked((ushort)(
                    4 * (segment.SlotIndex * 2) + 96));
                state.SegmentFunctions[segmentIndex] = BotwoonBodyFunction.DyingDelay;
                goto case BotwoonBodyFunction.DyingDelay;

            case BotwoonBodyFunction.DyingDelay:
                // The delay starts below 256 and ends on reaching it, so the count never nears the word's wrap.
                segment.DirectionParameter++;
                if (unchecked((short)(segment.DirectionParameter - 256)) >= 0)
                    state.SegmentFunctions[segmentIndex] = BotwoonBodyFunction.Falling;
                segment.InstructionTimer = WrappingCountdown.Start(0);
                ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
                return;

            case BotwoonBodyFunction.Falling:
                RunBotwoonBodyFall(segment, segmentIndex, state, randomEnemyCounter);
                return;

            case BotwoonBodyFunction.Landed:
                return;

            default:
                throw new InvalidOperationException($"Undefined BotwoonBodyFunction {function}.");
        }
    }

    /// <summary>Installs a segment program for its angle and applies the head's hurt flash.</summary>
    internal void AnimateBotwoonBodySegment(
        RoomEnemyProjectileSlot segment,
        int randomEnemyCounter)
    {
        BotwoonProjectileAnimation animation = segment.Animation as BotwoonProjectileAnimation
            ?? throw new InvalidOperationException($"Botwoon body segment {segment.SlotIndex} has no segment animation.");
        if (animation.Install(BotwoonSegmentProgram.FromSelector(segment.DirectionParameter)))
            segment.InstructionTimer = WrappingCountdown.Start(1);
        ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
    }

    /// <summary>Uses palette seven unless the flashing head selects palette zero this frame.</summary>
    private void ApplyBotwoonBodyHurtPalette(
        RoomEnemyProjectileSlot segment,
        int randomEnemyCounter)
    {
        segment.Graphics = segment.Graphics.InRow(7);
        RoomEnemySlot head = _slotTable.Slots[0];
        if (!head.FlashTimer.IsZero && (randomEnemyCounter & 2) != 0)
            segment.Graphics = segment.Graphics.InRow(0);
    }

    /// <summary>Accelerates a dying segment to Y=200 and marks the tail cap's final landing.</summary>
    private void RunBotwoonBodyFall(
        RoomEnemyProjectileSlot segment,
        int segmentIndex,
        BotwoonEnemyState state,
        int randomEnemyCounter)
    {
        int displacement = EnemyMath.ReadQuadraticEnemySpeed(
            unchecked((ushort)(EightEight.WholeOf(segment.Velocity.Y.RawValue))),
            negative: false);
        segment.Position = segment.Position.AdvanceY(SignedSixteenSixteen.FromRaw(displacement));
        if (unchecked((short)(segment.Position.Y.Whole - 200)) < 0)
        {
            segment.Velocity = segment.Velocity.WithY(new SignedEightEight(unchecked((ushort)(segment.Velocity.Y.RawValue + 192))));
            segment.InstructionTimer = WrappingCountdown.Start(0);
            ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
            return;
        }

        segment.Position = segment.Position.WithWholeY(200);
        state.SegmentFunctions[segmentIndex] = BotwoonBodyFunction.Landed;
        // The landed segment hands its animation to the shared big-explosion list.
        segment.StartAnimation(new MiscDustProjectileAnimation(MiscDustProjectileProgram.BigExplosion));
        segment.Graphics = SpriteGraphics.InPalette(5);
        segment.CanDamageSamus = false;
        segment.CollisionOption = EnemyProjectileCollisionOption.NoCollision;
        LastBotwoonSoundEffect = BotwoonSoundDefinitions.Explosion;

        // Native projectile index $0A is the last allocated body point and owns the
        // synchronization flag. Mapping by retained spawn argument is equivalent while also
        // remaining correct in an isolated audit that preoccupies lower projectile slots.
        if (ReferenceEquals(state.BodySegments[0], segment))
            state.LastBodySegmentLanded = true;
    }

    /// <summary>Ports <c>EprojInit_BotwoonsSpit</c> at <c>$86:EBC6</c>.</summary>
    /// <param name="head">Botwoon's head.</param>
    /// <param name="angle">The spit's direction.</param>
    /// <param name="speed">The spit speed, 0 through 65535.</param>
    internal void SpawnBotwoonSpit(RoomEnemySlot head, ByteAngle angle, int speed)
    {
        RoomEnemyProjectileSlot? spit = _projectiles.AllocateEnemyProjectile();
        if (spit is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spit,
            RoomEnemyProjectileKind.BotwoonSpit,
            head.Graphics);
        spit.Position = FixedPosition<RoomPixels>.FromWhole(head.Position.Whole);
        spit.StartAnimation(BotwoonProjectileAnimation.Spit());
        spit.DirectionParameter = angle.RawValue;

        // ConvertAngleToXy stores unsigned quarter-circle magnitudes; E73E applies signs
        // from the retained angle every frame. Keep both halves so shallow trajectories do
        // not lose their subpixel motion.
        int xMagnitude = EnemyMath.ReadAbsoluteCosineProduct(angle, checked((ushort)speed));
        int yMagnitude = EnemyMath.ReadAbsoluteSineProduct(angle, checked((ushort)speed));
        spit.Velocity = spit.Velocity.WithX(new SignedEightEight(FixedCoordinate.WholeOf(xMagnitude)));
        spit.Variable0 = unchecked((ushort)xMagnitude);
        spit.Velocity = spit.Velocity.WithY(new SignedEightEight(FixedCoordinate.WholeOf(yMagnitude)));
        spit.Variable1 = unchecked((ushort)yMagnitude);
    }

    /// <summary>Ports <c>EprojPreInstr_BotwoonsSpit</c> at <c>$86:EC05</c>.</summary>
    internal static void RunBotwoonSpitPreInstruction(
        RoomEnemyProjectileSlot spit,
        Position16<RoomPixels> camera)
    {
        int xMagnitude = unchecked(unchecked((int)SixteenSixteen.Compose(spit.Velocity.X.RawValue, checked((ushort)spit.Variable0))));
        int yMagnitude = unchecked(unchecked((int)SixteenSixteen.Compose(spit.Velocity.Y.RawValue, checked((ushort)spit.Variable1))));
        int xDisplacement = EnemyMath.CosineIsNegative(new ByteAngle(unchecked((byte)(spit.DirectionParameter)))) ? -xMagnitude : xMagnitude;
        int yDisplacement = EnemyMath.SineIsNegative(new ByteAngle(unchecked((byte)(spit.DirectionParameter)))) ? -yMagnitude : yMagnitude;
        spit.Position = spit.Position.AdvanceX(SignedSixteenSixteen.FromRaw(xDisplacement));
        spit.Position = spit.Position.AdvanceY(SignedSixteenSixteen.FromRaw(yDisplacement));

        if (unchecked((short)(spit.Position.X.Whole - camera.X)) < 0 ||
            unchecked((short)(camera.X + 256 - spit.Position.X.Whole)) < 0 ||
            unchecked((short)(spit.Position.Y.Whole - camera.Y)) < 0 ||
            unchecked((short)(camera.Y + 256 - spit.Position.Y.Whole)) < 0)
        {
            spit.Clear();
        }
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastBotwoonSoundEffect = null;
        LastBotwoonMusicRequest = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        if (LastBotwoonSoundEffect is { } sound)
            audio.QueueEnemySound(sound, maximumQueued: SoundQueueLimit.Max6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastBotwoonMusicRequest?.Request, LastBotwoonMusicRequest?.Delay);
    }
}
