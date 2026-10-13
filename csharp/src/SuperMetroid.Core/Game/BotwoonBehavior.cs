using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Botwoon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BotwoonBehavior : IEnemySpecies
{
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

    /// <summary>Binds Botwoon's head, body, palette, collision, and drop services.</summary>
    internal BotwoonBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, RoomSpriteObjectPool spriteObjects, EnemySlotTable slotTable, EnemyPresentation presentation, EnemyCollision collision, EnemyDeathsAndDrops deathsAndDrops)
    {
        _room = room;
        _projectiles = projectiles;
        _spriteObjects = spriteObjects;
        _slotTable = slotTable;
        _presentation = presentation;
        _collision = collision;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>Number of independent chance-table pickups requested when the wall crumbles.</summary>
    private const int BotwoonSpecialDropCount = 16;

    /// <summary>Per-room movement, head-animation, body, and wall-collapse state.</summary>
    internal BotwoonEnemyState? _botwoonState;
    /// <summary>Drop requests retained in their native RNG order for the current room.</summary>
    private readonly List<BotwoonDropRequest> _botwoonDropRequests = new();

    /// <summary>Last library-two sound selected by Botwoon in the current enemy frame.</summary>
    public ushort? LastBotwoonSoundEffect { get; private set; }

    /// <summary>Specialized drop request published when all body pieces have landed.</summary>
    public BotwoonDropRequest? LastBotwoonDropRequest { get; private set; }

    /// <summary>
    /// Hardcoded bank-$84 PLM requested by Botwoon. $B797 is the already-defeated wall;
    /// $B79B is the live crumble sequence.
    /// </summary>
    public PlmHeaderId? LastBotwoonWallPlm { get; private set; }

    /// <summary>Track three, queued with the native eight-frame delay after wall cleanup.</summary>
    public BotwoonMusicRequest? LastBotwoonMusicRequest { get; private set; }

    /// <summary>Discards prior-room Botwoon state and all published sound, drop, wall, and music requests.</summary>
    internal void ResetBotwoonRoomState()
    {
        _botwoonState = null;
        LastBotwoonSoundEffect = null;
        LastBotwoonDropRequest = null;
        _botwoonDropRequests.Clear();
        LastBotwoonWallPlm = null;
        LastBotwoonMusicRequest = null;
    }

    /// <summary>Ports <c>Botwoon_Init</c> at <c>$B3:9583</c>.</summary>
    private void InitializeBotwoon(RoomEnemySlot head)
    {
        var state = new BotwoonEnemyState(head);
        _botwoonState = state;
        // `$B3:9583` installs InstList_Botwoon_Hide: the head starts hidden in its hole.
        state.HeadProgram = BotwoonHeadProgram.Hidden;
        state.HeadAnimationPosition = 0;
        head.InstructionTimer = 1;
        head.Timer = 0;

        // A previously defeated Botwoon does not allocate body actors. The room loader
        // installs the permanent open wall and makes both one-screen scroll entries blue.
        if (_room.Services.Bosses.IsDefeated(BossBits.AreaMiniBoss))
        {
            LastBotwoonWallPlm = PlmHeaderId.ClearBotwoonWall;
            head.Properties = head.Properties.With(EnemyProperties.Deleted);
            state.WallCrumbleRequested = true;
            state.BossBitSet = true;
            return;
        }

        // SpawnEprojWithGfx is called with arguments 24,22,...,0. The shared allocator
        // searches native slots $22 down to $00, so retaining both orderings is essential:
        // death delay is based on projectile slot while body spacing is based on argument.
        for (int argument = 24; argument >= 0; argument -= 2)
            SpawnBotwoonBodySegment(head, state, unchecked((ushort)argument));

        state.Function = BotwoonEnemyFunction.InitialDelay;
        state.MovementFunction = BotwoonMovementFunction.MoveTowardHole;
        state.HeadFunction = BotwoonHeadFunction.AnimateFromMovement;
        state.InitialDelayTimer = 256;
        var initialSpeed = BotwoonSpeedDefinitions.ForHealthPhase(0);
        state.Speed = initialSpeed.MovementSpeed;
        state.SegmentSpacingBytes = initialSpeed.SegmentSpacingBytes;
        state.InsideHole = true;
        state.PreviousInsideHole = true;
        state.InitialAction = true;
        state.TargetHoleOffset = 0;
        state.MaximumHealth = head.Health;
        state.HalfHealth = unchecked((ushort)(head.Health >> 1));
        state.QuarterHealth = unchecked((ushort)(head.Health >> 2));
        state.PreviousHealth = head.Health;
        state.PaletteDestinationByteOffset =
            unchecked((ushort)((head.PaletteIndex >> 4) + 256));
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
            state.DeathTimer = 240;
            state.PendingDeath = false;
            state.ExitTransitionComplete = false;
        }

        switch (state.Function)
        {
            case BotwoonEnemyFunction.InitialDelay:
                state.InitialDelayTimer = unchecked((ushort)(state.InitialDelayTimer - 1));
                if (state.InitialDelayTimer == 0)
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
                state.DeathTimer = unchecked((ushort)(state.DeathTimer + 1));
                if (unchecked((short)(state.DeathTimer - 256)) >= 0)
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
                    $"Botwoon function $B3:{(ushort)state.Function:X4} is not translated.");
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
            RunBotwoonMovingFrame(head, state, samus, detectHole: true);
            return;
        }

        state.PathComplete = false;
        ushort choice = 0;
        if (!state.InsideHole && !state.InitialAction && state.HealthPhase == 0)
            choice = unchecked((ushort)(_room.Random.Generate() & 0x000e));
        state.InitialAction = false;

        if (choice is 0 or 2 or 4)
        {
            state.Function = BotwoonEnemyFunction.FollowAuthoredPath;
            state.MovementFunction = BotwoonMovementFunction.LoadAuthoredPath;
            state.AttackTimer = 0;
            state.HeadFunction = BotwoonHeadFunction.AnimateFromMovement;
            ChooseBotwoonPath(head, state);
        }
        else
        {
            state.Function = BotwoonEnemyFunction.SpitWhileHidden;
            state.HeadFunction = BotwoonHeadFunction.AimAtSamus;
            state.AttackTimer = 48;
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

        RunBotwoonMovingFrame(head, state, samus, detectHole: false);
    }

    /// <summary>Waits for the spit timer, then chooses whether the next path begins inside a hole.</summary>
    private void RunBotwoonHiddenSpitState(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (state.AttackTimer != 0)
        {
            RunBotwoonHeadFunction(head, state, samus);
            return;
        }

        state.PathComplete = false;
        state.Function = BotwoonEnemyFunction.FollowAuthoredPath;
        state.MovementFunction = BotwoonMovementFunction.LoadAuthoredPath;
        state.HeadFunction = BotwoonHeadFunction.AnimateFromMovement;

        ushort nextInside = state.PendingDeath
            ? (ushort)0
            : unchecked((ushort)(_room.Random.Generate() & 1));
        state.InsideHole = nextInside != 0;
        state.PreviousInsideHole = state.InsideHole;
        if (!state.InsideHole)
        {
            state.PathChoiceOffset = 0;
        }
        else
        {
            state.HoleLatch = false;
            state.SavedHoleRingByteOffset = 0xffff;
        }
        ChooseBotwoonPath(head, state);
    }

    /// <summary>Moves the head, writes position history, places and orients body segments, then tests holes.</summary>
    private void RunBotwoonMovingFrame(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus,
        bool detectHole)
    {
        RunBotwoonMovement(head, state);
        RecordBotwoonHeadPosition(head, state);
        PositionBotwoonBody(state);
        RunBotwoonHeadFunction(head, state, samus);
        OrientBotwoonBody(head, state);
        state.RingByteOffset = unchecked((ushort)((state.RingByteOffset + 4) & 0x03ff));
        if (detectHole)
            DetectBotwoonHole(head, state);
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
                    $"Botwoon movement $B3:{(ushort)state.MovementFunction:X4} is not translated.");
        }
    }

    /// <summary>Steers the head toward its selected hole until inside/outside state changes.</summary>
    private static void MoveBotwoonTowardHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        BotwoonHoleDefinition hole =
            BotwoonNavigationDefinitions.HoleForByteOffset(state.TargetHoleOffset);
        Offset16<RoomPixels> delta = hole.Target - head.Position.Whole;
        byte angle = EnemyMath.CalculateCartridgeAngle(new(
            ClampBotwoonTargetDelta(delta.X), ClampBotwoonTargetDelta(delta.Y)));
        state.TargetAngle = angle;
        state.MovementAngle = unchecked((byte)(64 - angle));

        if (state.InsideHole == state.PreviousInsideHole)
        {
            AddBotwoonAngleVector(head, state.MovementAngle, state.Speed);
        }
        else
        {
            state.PreviousInsideHole = state.InsideHole;
            state.PathComplete = true;
        }
    }

    /// <summary>Restricts target distance to the signed angle routine's supported range.</summary>
    private static short ClampBotwoonTargetDelta(short value) =>
        value < -256 ? (short)-255 : value >= 256 ? (short)255 : value;

    /// <summary>Uses health phase, current hole, and native RNG to choose a path-table offset.</summary>
    private void ChooseBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        UpdateBotwoonHealthPhase(head, state);
        ushort insideBase = state.InsideHole ? (ushort)128 : (ushort)0;
        state.PathChoiceOffset = unchecked((ushort)(
            (_room.Random.Generate() & 0x0018) + insideBase + 4 * state.TargetHoleOffset));
    }

    /// <summary>Changes traversal speed and body spacing at half and quarter health outside holes.</summary>
    internal static void UpdateBotwoonHealthPhase(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.InsideHole)
            return;

        byte phase = 0;
        if (head.Health != 0 && unchecked((short)(head.Health - state.HalfHealth)) < 0)
        {
            phase = unchecked((short)(head.Health - state.QuarterHealth)) < 0
                ? (byte)2
                : (byte)1;
        }
        state.HealthPhase = phase;
        var speed = BotwoonSpeedDefinitions.ForHealthPhase(phase);
        state.Speed = speed.MovementSpeed;
        state.SegmentSpacingBytes = speed.SegmentSpacingBytes;
    }

    /// <summary>Loads a path pointer, direction, and destination hole from the chosen descriptor.</summary>
    private static void LoadBotwoonPathDescriptor(BotwoonEnemyState state)
    {
        BotwoonPathDescriptorDefinition descriptor =
            BotwoonNavigationDefinitions.PathForChoiceByteOffset(state.PathChoiceOffset);
        state.PathPointer = descriptor.PathPointer;
        state.PathDirection = descriptor.Direction;
        state.TargetHoleOffset = descriptor.TargetHoleByteOffset;
        if (state.PathDirection < 0)
            state.PathPointer = unchecked((ushort)(state.PathPointer - 4));
        state.PathComplete = false;
        state.MovementFunction = BotwoonMovementFunction.FollowAuthoredPath;
    }

    /// <summary>Consumes path samples according to current speed until a terminal sentinel is reached.</summary>
    private static void FollowBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        Offset16<RoomPixels> total = Offset16<RoomPixels>.Zero;
        int step = state.PathDirection < 0 ? -2 : 2;
        for (int sample = 0; sample < state.Speed; sample++)
        {
            Offset16<RoomPixels> movement =
                BotwoonNavigationDefinitions.MovementSampleForPointer(state.PathPointer);
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
            state.PathPointer = unchecked((ushort)(state.PathPointer + step));
        }

        head.Position += state.PathDirection < 0 ? -total : total;
    }

    /// <summary>Writes the head's whole-pixel position into the 1024-byte history ring.</summary>
    private static void RecordBotwoonHeadPosition(RoomEnemySlot head, BotwoonEnemyState state)
    {
        int ringIndex = state.RingByteOffset >> 2;
        state.History[ringIndex] = head.Position.Whole;
    }

    /// <summary>Spaces thirteen body projectiles along older head-history samples.</summary>
    private static void PositionBotwoonBody(BotwoonEnemyState state)
    {
        ushort historyOffset = unchecked((ushort)(
            (state.RingByteOffset - state.SegmentSpacingBytes) & 0x03ff));
        for (int argument = 24; argument >= 0; argument -= 2)
        {
            int segmentIndex = argument >> 1;
            RoomEnemyProjectileSlot segment = state.BodySegments[segmentIndex]
                ?? throw new InvalidOperationException(
                    $"Botwoon body argument {argument} lost its projectile slot.");

            ToggleBotwoonSegmentAtHole(state, segmentIndex, historyOffset);
            int historyIndex = historyOffset >> 2;
            segment.Position = segment.Position.WithWhole(state.History[historyIndex]);
            historyOffset = unchecked((ushort)(
                (historyOffset - state.SegmentSpacingBytes) & 0x03ff));
        }
    }

    /// <summary>Changes a segment's collision mode when its delayed history cursor crosses a hole.</summary>
    private static void ToggleBotwoonSegmentAtHole(
        BotwoonEnemyState state,
        int segmentIndex,
        ushort historyOffset)
    {
        if (state.SavedHoleRingByteOffset == 0xffff ||
            state.SavedHoleRingByteOffset != historyOffset)
            return;

        RoomEnemyProjectileSlot segment = state.BodySegments[segmentIndex]!;
        bool wasInside = state.SegmentInsideHole[segmentIndex];
        state.SegmentInsideHole[segmentIndex] = !wasInside;
        segment.CanDamageSamus = wasInside;
        segment.CollisionOption = wasInside ? (ushort)1 : (ushort)2;

        if (segmentIndex != 0)
            return;
        state.HoleLatch = false;
        state.ExitTransitionComplete = !state.InsideHole;
        state.SavedHoleRingByteOffset = 0xffff;
    }

    /// <summary>Faces each body sprite toward the preceding segment or head.</summary>
    private static void OrientBotwoonBody(RoomEnemySlot head, BotwoonEnemyState state)
    {
        for (int argument = 24; argument >= 0; argument -= 2)
        {
            int segmentIndex = argument >> 1;
            RoomEnemyProjectileSlot segment = state.BodySegments[segmentIndex]!;
            ushort extraAngle = state.SegmentInsideHole[segmentIndex] ? (ushort)256 : (ushort)0;
            short dx;
            short dy;
            if (argument == 24)
            {
                dx = unchecked((short)(head.Position.X.Whole - segment.Position.X.Whole));
                dy = unchecked((short)(head.Position.Y.Whole - segment.Position.Y.Whole));
            }
            else
            {
                RoomEnemyProjectileSlot previous = state.BodySegments[segmentIndex + 1]!;
                dx = unchecked((short)(previous.Position.X.Whole - segment.Position.X.Whole));
                dy = unchecked((short)(previous.Position.Y.Whole - segment.Position.Y.Whole));
                if (argument == 0)
                    extraAngle = unchecked((ushort)(extraAngle + 512));
            }

            ushort angle = unchecked((ushort)(extraAngle + EnemyMath.CalculateCartridgeAngle(new(dx, dy))));
            segment.DirectionParameter = unchecked((ushort)(2 * (angle >> 5)));
        }
    }

    /// <summary>Latches the first hole rectangle entered by the head and marks its history offset.</summary>
    internal static void DetectBotwoonHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.HoleLatch)
            return;

        for (int rectangleOffset = 24; rectangleOffset >= 0; rectangleOffset -= 8)
        {
            BotwoonHoleDefinition hole = BotwoonNavigationDefinitions.HoleForByteOffset(
                unchecked((ushort)rectangleOffset));
            Offset16<RoomPixels> fromTopLeft = head.Position.Whole - hole.TopLeft;
            Offset16<RoomPixels> fromBottomRight = head.Position.Whole - hole.BottomRight;
            if (fromTopLeft.X >= 0 && fromBottomRight.X < 0 &&
                fromTopLeft.Y >= 0 && fromBottomRight.Y < 0)
            {
                state.HoleLatch = true;
                state.InsideHole = !state.InsideHole;
                state.SavedHoleRingByteOffset = state.RingByteOffset;
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
                ushort next = unchecked((ushort)(state.AttackTimer - 1));
                state.AttackTimer = next;
                if (unchecked((short)next) < 0)
                {
                    state.AttackTimer = 0;
                    state.HeadFunction = BotwoonHeadFunction.AnimateFromMovement;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Botwoon head callback $B3:{(ushort)state.HeadFunction:X4} is not translated.");
        }
    }

    /// <summary>Uses recent head displacement to select visible facing or hidden hole animation.</summary>
    internal static void AnimateBotwoonHeadFromMovement(RoomEnemySlot head, BotwoonEnemyState state)
    {
        Offset16<RoomPixels> moved = head.Position.Whole - state.HeadHistory[3];
        short dx = moved.X;
        short dy = moved.Y;
        if (dx != 0 || dy != 0)
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
                program = BotwoonHeadProgram.MovingToward(EnemyMath.CalculateCartridgeAngle(new(dx, dy)));
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
        byte angle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - head.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - head.Position.Y.Whole))));
        InstallBotwoonHeadProgram(head, state, BotwoonHeadProgram.SpittingToward(angle));
        state.SpitAngle = unchecked((byte)(64 - angle));
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
        head.InstructionTimer = 1;
        head.Timer = 0;
    }

    /// <summary>
    /// Runs Botwoon's head animation where the enemy instruction interpreter would: the frame
    /// expires on the update its native timer would, and a newly shown frame raises the
    /// new-frame flag exactly as a native timed frame does.
    /// </summary>
    internal void AdvanceBotwoonHeadAnimation(RoomEnemySlot head, BotwoonEnemyState state)
    {
        ushort oldTimer = head.InstructionTimer;
        head.InstructionTimer = unchecked((ushort)(oldTimer - 1));
        if (oldTimer != 1)
        {
            head.ExtraProperties = head.ExtraProperties.Without(EnemyExtraProperties.NewInstructionFrame);
            return;
        }

        int position = state.HeadAnimationPosition;
        AnimationAdvance<BotwoonHeadFrame> advance = BotwoonAnimations.Head(state.HeadProgram)
            .Advance(ref position, cue => ApplyBotwoonHeadCue(head, state, cue));
        state.HeadAnimationPosition = position;
        // A hold is the native sleep: the timer wraps from zero and the last frame stays.
        if (advance.Held)
            return;
        head.InstructionTimer = (ushort)advance.Duration;
        head.SpritemapPointer = BotwoonVisualDefinitions.HeadSpritemap(advance.Frame);
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
        ushort speed = BotwoonSpeedDefinitions.ForHealthPhase(state.HealthPhase).SpitSpeed;
        byte angle = unchecked((byte)(state.SpitAngle + startOffset));
        for (int projectile = 0; projectile < count; projectile++)
        {
            SpawnBotwoonSpit(head, angle, speed);
            angle = unchecked((byte)(angle + 16));
        }
    }

    /// <summary>Copies the next health-band OBJ palette when Botwoon's health crosses its threshold.</summary>
    internal void UpdateBotwoonHealthPalette(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.PalettePhaseByteOffset ==
            BotwoonHealthPaletteDefinitions.CompletePhaseByteOffset)
            return;
        if (!BotwoonHealthPaletteDefinitions.ShouldAdvance(
                state.PalettePhaseByteOffset,
                head.Health))
            return;

        int destinationColor = state.PaletteDestinationByteOffset >> 1;
        int colorCount = 256 - destinationColor;
        if (_room.Artwork?.BotwoonColors is { } colors)
        {
            // The retail actor owns OBJ palette seven. A restored/non-retail offset
            // would copy past this authored image into adjacent ROM data; do not
            // silently substitute a different visual result for that state.
            if (destinationColor != BotwoonHealthPaletteDefinitions.DestinationColor ||
                colorCount != BotwoonHealthPaletteDefinitions.ColorsPerPalette)
                throw new InvalidDataException(
                    $"Botwoon palette destination {destinationColor} is outside the retail sprite palette.");
            int band = state.PalettePhaseByteOffset / sizeof(ushort);
            for (int color = 0; color < colorCount; color++)
                _presentation.BossPalettes.BotwoonBody.Write(color, colors.HealthColor(band, color));
        }
        else throw new InvalidOperationException("Botwoon requires installed health colors.");
        state.PalettePhaseByteOffset = unchecked((ushort)(state.PalettePhaseByteOffset + 2));
    }

    /// <summary>Accelerates the defeated head down to Y=200, then hides it and waits for body landing.</summary>
    private void RunBotwoonHeadFall(RoomEnemySlot head, BotwoonEnemyState state)
    {
        int displacement = EnemyMath.ReadQuadraticEnemySpeed(
            unchecked((ushort)(SnesLittleEndian.HighByte(state.DeathFallAccumulator))),
            negative: false);
        head.Position = head.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(displacement));
        if (unchecked((short)(head.Position.Y.Whole - 200)) < 0)
        {
            state.DeathFallAccumulator = unchecked((ushort)(state.DeathFallAccumulator + 192));
            return;
        }

        head.Position = head.Position.WithWholeY(200);
        state.Function = BotwoonEnemyFunction.WaitForBody;
        _projectiles.SpawnRoomGraphicsDustExplosion(head.Position.Whole, animationIndex: 0x001d);
        LastBotwoonSoundEffect = BotwoonSoundDefinitions.Explosion;
        head.Properties = head.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
    }

    /// <summary>Requests the crumble PLM and sixteen RNG-positioned drops before wall explosions.</summary>
    private void BeginBotwoonWallExplosions(RoomEnemySlot head, BotwoonEnemyState state)
    {
        state.Function = BotwoonEnemyFunction.WallExplosions;
        LastBotwoonWallPlm = PlmHeaderId.CrumbleBotwoonWall;
        state.WallCrumbleRequested = true;

        // `Enemy_ItemDrop_Botwoon` at `$A0:BA3E` emits sixteen independent pickup
        // requests. Each position uses both bytes of one RNG result: X samples low seven
        // bits and Y samples bits 8..13. Each request is also materialized immediately as
        // a real $F337 actor using the retained six-byte item-chance pointer.
        for (int dropIndex = 0; dropIndex < BotwoonSpecialDropCount; dropIndex++)
        {
            ushort random = _room.Random.Generate();
            var request = new BotwoonDropRequest(
                Position: new Position16<RoomPixels>(
                    unchecked((ushort)((random & 0x007f) + 64)),
                    unchecked((ushort)(((SnesLittleEndian.HighByte(random) & 0x3f)) + 128))),
                ItemDropChancesPointer: head.Definition.ItemDropChancesPointer);
            _botwoonDropRequests.Add(request);
            LastBotwoonDropRequest = request;
            _deathsAndDrops.SpawnEnemyDropFromChanceTable(
                request.Position,
                request.ItemDropChancesPointer);
        }
        state.DropRequested = true;
        state.WallExplosionFrame = 0;
        state.LargeExplosionTimer = 0;
        state.SmallExplosionTimer = 0;
    }

    /// <summary>Runs the 192-frame wall sequence, then sets the boss bit and queues delayed music.</summary>
    private void RunBotwoonWallExplosions(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.WallExplosionFrame >= 192)
        {
            head.Properties = head.Properties.With(EnemyProperties.Deleted);
            state.BossBitSet = true;
            _room.Services.Bosses.SetDefeated(BossBits.AreaMiniBoss);
            // `$B3:9B32` invokes QueueMusic_Delayed8(3) only after the complete 192-frame
            // wall-explosion phase, not when health first reaches zero.
            LastBotwoonMusicRequest = new BotwoonMusicRequest(
                MusicCommand.SelectTrack(3),
                MusicCommandDelay.EightFrames);
            return;
        }

        if (state.WallExplosionFrame >= 64)
        {
            NativeWordCounterStep largeExplosionTimer =
                NativeWordCounter.Decrement(state.LargeExplosionTimer);
            state.LargeExplosionTimer = largeExplosionTimer.Value;
            if (largeExplosionTimer.IsNegative)
            {
                state.LargeExplosionTimer = 12;
                _spriteObjects.SpawnRoomSpriteObject(
                    new(unchecked((ushort)((_room.Random.Generate() & 0x001f) + 232)), unchecked((ushort)(state.WallExplosionFrame + (_room.Random.Generate() & 0x001f) - 8))),
                    RoomSpriteObjectKind.BotwoonLargeExplosion,
                    graphicsIndex: 0x0a00);
                LastBotwoonSoundEffect = BotwoonSoundDefinitions.Explosion;
            }

            NativeWordCounterStep smallExplosionTimer =
                NativeWordCounter.Decrement(state.SmallExplosionTimer);
            state.SmallExplosionTimer = smallExplosionTimer.Value;
            if (smallExplosionTimer.IsNegative)
            {
                state.SmallExplosionTimer = 4;
                for (int explosion = 0; explosion < 2; explosion++)
                {
                    _spriteObjects.SpawnRoomSpriteObject(
                        new(unchecked((ushort)((_room.Random.Generate() & 0x003f) + 224)), unchecked((ushort)(state.WallExplosionFrame + (_room.Random.Generate() & 0x001f) - 8))),
                        RoomSpriteObjectKind.BotwoonSmallExplosion,
                        graphicsIndex: 0x0a00);
                }
            }
        }
        state.WallExplosionFrame = unchecked((ushort)(state.WallExplosionFrame + 1));
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
        byte angle,
        ushort magnitude)
    {
        int xMagnitude = EnemyMath.ReadUnsignedSineMagnitudeProduct(angle, magnitude, 0x40);
        int yMagnitude = EnemyMath.ReadUnsignedSineMagnitudeProduct(angle, magnitude, 0x80);
        int xDisplacement = ((angle + 64) & 0x80) != 0 ? -xMagnitude : xMagnitude;
        int yDisplacement = ((angle + 128) & 0x80) != 0 ? -yMagnitude : yMagnitude;
        head.Position = head.Position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(xDisplacement));
        head.Position = head.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(yDisplacement));
    }


    /// <summary>Requires the initialized Botwoon head in physical slot zero.</summary>
    internal BotwoonEnemyState RequireBotwoonState(RoomEnemySlot head) =>
        _botwoonState is not null && head.SlotIndex == 0
            ? _botwoonState
            : throw new InvalidOperationException(
                $"Enemy slot {head.SlotIndex} has no initialized Botwoon state.");

    /// <summary>Routes bomb hits through the shared shot callback without generic death animation.</summary>
    internal void RegisterBotwoonBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.BotwoonShot)
        {
            RunsGenericDeath = false,
            BeforeCommon = c => RequireBotwoonState(c.Enemy).PreviousHealth = c.HealthBefore,
            AfterCommon = c => ResolveBotwoonCombatAfterCommon(c.Enemy),
        });
    }

    /// <summary>Registers body-segment and spit pre-instruction handlers.</summary>
    internal void RegisterBotwoonProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunBotwoonBodyPreInstruction(context.Projectile, context.NmiFrameCounter8);
        }, EnemyProjectilePreInstruction.BotwoonsBody);

        table.RegisterPreInstruction(context =>
        {
            RunBotwoonSpitPreInstruction(context.Projectile, context.Camera);
        }, EnemyProjectilePreInstruction.BotwoonsSpit);
    }

    /// <summary>Associates Botwoon's definition pointer with its room-load initializer.</summary>
    internal void RegisterBotwoonInitializationAi(EnemyAiTable table)
    {
        table.RegisterInitialization(EnemyAiRoutine.InitAI_Botwoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeBotwoon(slot);
        });
    }

    /// <summary>Associates the head slot with Botwoon's per-frame state machine.</summary>
    internal void RegisterBotwoonMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Botwoon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon, (slot, frame, nmiFrameCounter8) =>
        {
            RunBotwoonMain(slot, RequireBotwoonState(slot), frame.Samus);
        });
    }

    /// <summary>Applies power-bomb damage while preserving Botwoon's delayed death traversal.</summary>
    internal void RegisterBotwoonPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.BotwoonPowerBomb)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = (enemy, _) => ResolveBotwoonCombatAfterCommon(enemy),
        });
    }

    /// <summary>Records pre-hit health for palette phases and skips generic death on shot hits.</summary>
    internal void RegisterBotwoonShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.BotwoonShot)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = (c, _) =>
            {
                RequireBotwoonState(c.Enemy).PreviousHealth = c.HealthBefore;
                ResolveBotwoonCombatAfterCommon(c.Enemy);
            },
        });
    }

    /// <summary>Resolves Samus contact while preserving Botwoon's special zero-health tail.</summary>
    internal void RegisterBotwoonTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.BotwoonTouch,
            context =>
            {
                _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, skipDeathAnimation: true);
                ResolveBotwoonCombatAfterCommon(context.Slot);
            });
    }
    /// <summary>Ports <c>EprojInit_BotwoonsBody</c> at <c>$86:EA31</c>.</summary>
    internal void SpawnBotwoonBodySegment(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        ushort spawnArgument)
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
            unchecked((ushort)(head.VramTilesIndex | head.PaletteIndex)));
        segment.Position = FixedPosition<RoomPixels>.FromWhole(head.Position.Whole);
        segment.Velocity = segment.Velocity.WithY(new SnesSignedEightEight(0));

        // ai_var_A is the live spawn argument. Every nonzero segment begins with orientation
        // $10; argument zero is the tail cap and begins at $30.
        ushort orientation = spawnArgument != 0 ? (ushort)16 : (ushort)48;
        segment.StartAnimation(BotwoonProjectileAnimation.ForSegment(BotwoonSegmentProgram.FromSelector(orientation)));
        segment.DirectionParameter = orientation;
        segment.Velocity = segment.Velocity.WithX(new SnesSignedEightEight((ushort)BotwoonBodyFunction.Main));

        int segmentIndex = spawnArgument >> 1;
        state.BodySegments[segmentIndex] = segment;
        state.SegmentInsideHole[segmentIndex] = true;
        segment.CollisionOption = 2;
        segment.CanDamageSamus = false;
    }

    /// <summary>Ports <c>EprojPreInstr_BotwoonsBody</c> and its four function states.</summary>
    internal void RunBotwoonBodyPreInstruction(
        RoomEnemyProjectileSlot segment,
        byte randomEnemyCounter)
    {
        BotwoonEnemyState state = _botwoonState ?? throw new InvalidOperationException(
            "A live Botwoon body segment has no owning head state.");
        BotwoonBodyFunction function =
            ClosedNativeWords.Decode<BotwoonBodyFunction>(segment.Velocity.X.RawValue, "Botwoon body function");
        if (state.BodyDeathStarted && function == BotwoonBodyFunction.Main)
        {
            function = BotwoonBodyFunction.BeginDying;
            segment.Velocity = segment.Velocity.WithX(new SnesSignedEightEight((ushort)function));
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
                segment.Velocity = segment.Velocity.WithX(new SnesSignedEightEight((ushort)BotwoonBodyFunction.DyingDelay));
                goto case BotwoonBodyFunction.DyingDelay;

            case BotwoonBodyFunction.DyingDelay:
                segment.DirectionParameter = unchecked((ushort)(segment.DirectionParameter + 1));
                if (unchecked((short)(segment.DirectionParameter - 256)) >= 0)
                    segment.Velocity = segment.Velocity.WithX(new SnesSignedEightEight((ushort)BotwoonBodyFunction.Falling));
                segment.InstructionTimer = 0;
                ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
                return;

            case BotwoonBodyFunction.Falling:
                RunBotwoonBodyFall(segment, state, randomEnemyCounter);
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
        byte randomEnemyCounter)
    {
        BotwoonProjectileAnimation animation = segment.Animation as BotwoonProjectileAnimation
            ?? throw new InvalidOperationException($"Botwoon body segment {segment.SlotIndex} has no segment animation.");
        if (animation.Install(BotwoonSegmentProgram.FromSelector(segment.DirectionParameter)))
            segment.InstructionTimer = 1;
        ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
    }

    /// <summary>Uses palette seven unless the flashing head selects palette zero this frame.</summary>
    private void ApplyBotwoonBodyHurtPalette(
        RoomEnemyProjectileSlot segment,
        byte randomEnemyCounter)
    {
        segment.GraphicsIndex = new SnesObjAttributeWord(segment.GraphicsIndex)
            .WithPaletteBits(SnesObjPalettes.Index7.PaletteBits);
        RoomEnemySlot head = _slotTable._slots[0];
        if (head.FlashTimer != 0 && (randomEnemyCounter & 2) != 0)
            segment.GraphicsIndex = new SnesObjAttributeWord(segment.GraphicsIndex)
                .WithPaletteIndex(0);
    }

    /// <summary>Accelerates a dying segment to Y=200 and marks the tail cap's final landing.</summary>
    private void RunBotwoonBodyFall(
        RoomEnemyProjectileSlot segment,
        BotwoonEnemyState state,
        byte randomEnemyCounter)
    {
        int displacement = EnemyMath.ReadQuadraticEnemySpeed(
            unchecked((ushort)(SnesEightEight.WholeOf(segment.Velocity.Y.RawValue))),
            negative: false);
        segment.Position = segment.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(displacement));
        if (unchecked((short)(segment.Position.Y.Whole - 200)) < 0)
        {
            segment.Velocity = segment.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(segment.Velocity.Y.RawValue + 192))));
            segment.InstructionTimer = 0;
            ApplyBotwoonBodyHurtPalette(segment, randomEnemyCounter);
            return;
        }

        segment.Position = segment.Position.WithWholeY(200);
        segment.Velocity = segment.Velocity.WithX(new SnesSignedEightEight((ushort)BotwoonBodyFunction.Landed));
        // The landed segment hands its animation to the shared big-explosion list.
        segment.StartNativeProgram(BotwoonProjectileCodePointers.BodyLandedInstruction);
        segment.GraphicsIndex = EnemyPaletteBits.Palette5;
        segment.CanDamageSamus = false;
        segment.CollisionOption = 2;
        LastBotwoonSoundEffect = BotwoonSoundDefinitions.Explosion;

        // Native projectile index $0A is the last allocated body point and owns the
        // synchronization flag. Mapping by retained spawn argument is equivalent while also
        // remaining correct in an isolated audit that preoccupies lower projectile slots.
        if (ReferenceEquals(state.BodySegments[0], segment))
            state.LastBodySegmentLanded = true;
    }

    /// <summary>Ports <c>EprojInit_BotwoonsSpit</c> at <c>$86:EBC6</c>.</summary>
    internal void SpawnBotwoonSpit(RoomEnemySlot head, byte angle, ushort speed)
    {
        RoomEnemyProjectileSlot? spit = _projectiles.AllocateEnemyProjectile();
        if (spit is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spit,
            RoomEnemyProjectileKind.BotwoonSpit,
            unchecked((ushort)(head.VramTilesIndex | head.PaletteIndex)));
        spit.Position = FixedPosition<RoomPixels>.FromWhole(head.Position.Whole);
        spit.StartAnimation(BotwoonProjectileAnimation.Spit());
        spit.DirectionParameter = angle;

        // ConvertAngleToXy stores unsigned quarter-circle magnitudes; E73E applies signs
        // from the retained angle every frame. Keep both halves so shallow trajectories do
        // not lose their subpixel motion.
        int xMagnitude = EnemyMath.ReadUnsignedSineMagnitudeProduct(angle, speed, 0x40);
        int yMagnitude = EnemyMath.ReadUnsignedSineMagnitudeProduct(angle, speed, 0x80);
        spit.Velocity = spit.Velocity.WithX(new SnesSignedEightEight(SnesFixedPosition.WholeOf(xMagnitude)));
        spit.Variable0 = unchecked((ushort)xMagnitude);
        spit.Velocity = spit.Velocity.WithY(new SnesSignedEightEight(SnesFixedPosition.WholeOf(yMagnitude)));
        spit.Variable1 = unchecked((ushort)yMagnitude);
    }

    /// <summary>Ports <c>EprojPreInstr_BotwoonsSpit</c> at <c>$86:EC05</c>.</summary>
    internal static void RunBotwoonSpitPreInstruction(
        RoomEnemyProjectileSlot spit,
        Position16<RoomPixels> camera)
    {
        int xMagnitude = unchecked(unchecked((int)SnesSixteenSixteen.Compose(spit.Velocity.X.RawValue, spit.Variable0)));
        int yMagnitude = unchecked(unchecked((int)SnesSixteenSixteen.Compose(spit.Velocity.Y.RawValue, spit.Variable1)));
        int xDisplacement = ((spit.DirectionParameter + 64) & 0x80) != 0
            ? -xMagnitude
            : xMagnitude;
        int yDisplacement = ((spit.DirectionParameter + 128) & 0x80) != 0
            ? -yMagnitude
            : yMagnitude;
        spit.Position = spit.Position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(xDisplacement));
        spit.Position = spit.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(yDisplacement));

        if (unchecked((short)(spit.Position.X.Whole - camera.X)) < 0 ||
            unchecked((short)(camera.X + 256 - spit.Position.X.Whole)) < 0 ||
            unchecked((short)(spit.Position.Y.Whole - camera.Y)) < 0 ||
            unchecked((short)(camera.Y + 256 - spit.Position.Y.Whole)) < 0)
        {
            spit.Clear();
        }
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetBotwoonRoomState();
        LastBotwoonSoundEffect = null;
        LastBotwoonDropRequest = null;
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastBotwoonSoundEffect = null;
        LastBotwoonDropRequest = null;
        LastBotwoonWallPlm = null;
        LastBotwoonMusicRequest = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastBotwoonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }

    /// <summary>Queues this frame's published music requests through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacyMusic(EnemyAudioQueue audio)
    {
        audio.QueueLegacyMusic(LastBotwoonMusicRequest?.Command, LastBotwoonMusicRequest?.Delay);
    }
}
