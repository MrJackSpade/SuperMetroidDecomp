using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$B3 function pointer stored in Botwoon's variable D. The values are kept as
/// cartridge addresses so debugger watches can be compared directly with the native actor.
/// </summary>
public enum BotwoonEnemyFunction : ushort
{
    InitialDelay = 0x9878,
    ChooseNextAction = 0x989d,
    FollowAuthoredPath = 0x99a4,
    SpitWhileHidden = 0x99e4,
    DeathDelay = 0x9a46,
    HeadFalling = 0x9a5e,
    WaitForBody = 0x9aca,
    WallExplosions = 0x9af9,
}

/// <summary>Literal movement callback stored in Botwoon's variable E.</summary>
public enum BotwoonMovementFunction : ushort
{
    MoveTowardHole = 0x9bb7,
    LoadAuthoredPath = 0xe250,
    FollowAuthoredPath = 0xe28c,
}

/// <summary>Literal head/attack callback stored in Botwoon's variable F.</summary>
public enum BotwoonHeadFunction : ushort
{
    AnimateFromMovement = 0x9dc0,
    AimAtSamus = 0x9e7d,
    WaitForSpitFrame = 0x9ee0,
    SpawnImmediateVolley = 0x9f34,
    AttackCooldown = 0x9f7a,
}

/// <summary>
/// Debugger-facing projection of Botwoon's large extra-RAM allocation. The original uses
/// several disjoint WRAM pages and aliases thirteen segment records through enemy-shaped
/// structures. Named host fields retain those exact meanings without pretending the words
/// belong to thirteen ordinary enemy slots.
/// </summary>
public sealed class BotwoonEnemyState
{
    private readonly RoomEnemySlot _head;

    internal BotwoonEnemyState(RoomEnemySlot head) => _head = head;

    public BotwoonEnemyFunction Function
    {
        get => (BotwoonEnemyFunction)_head.VariableD;
        internal set => _head.VariableD = (ushort)value;
    }

    public BotwoonMovementFunction MovementFunction
    {
        get => (BotwoonMovementFunction)_head.VariableE;
        internal set => _head.VariableE = (ushort)value;
    }

    public BotwoonHeadFunction HeadFunction
    {
        get => (BotwoonHeadFunction)_head.VariableF;
        internal set => _head.VariableF = (ushort)value;
    }

    public ushort RingByteOffset { get; internal set; }
    public ushort SegmentSpacingBytes { get; internal set; }
    public ushort InitialDelayTimer { get; internal set; }
    public ushort AttackTimer { get; internal set; }
    public ushort DeathTimer { get; internal set; }
    public ushort DeathFallAccumulator { get; internal set; }
    public ushort WallExplosionFrame { get; internal set; }
    public ushort LargeExplosionTimer { get; internal set; }
    public ushort SmallExplosionTimer { get; internal set; }
    public ushort Speed { get; internal set; }
    public ushort TargetHoleOffset { get; internal set; }
    public ushort TargetAngle { get; internal set; }
    public byte MovementAngle { get; internal set; }
    public byte SpitAngle { get; internal set; }
    public ushort PathChoiceOffset { get; internal set; }
    public ushort PathPointer { get; internal set; }
    public short PathDirection { get; internal set; }
    public ushort InstalledHeadInstruction { get; internal set; }
    public ushort PaletteDestinationByteOffset { get; internal set; }
    public ushort PalettePhaseByteOffset { get; internal set; }
    public ushort MaximumHealth { get; internal set; }
    public ushort HalfHealth { get; internal set; }
    public ushort QuarterHealth { get; internal set; }
    public ushort PreviousHealth { get; internal set; }
    public byte HealthPhase { get; internal set; }
    public bool InsideHole { get; internal set; }
    public bool PreviousInsideHole { get; internal set; }
    public bool HoleLatch { get; internal set; }
    public bool InitialAction { get; internal set; }
    public bool PathComplete { get; internal set; }
    public bool SpitFrameReached { get; internal set; }
    public bool PendingDeath { get; internal set; }
    public bool ExitTransitionComplete { get; internal set; }
    public bool BodyDeathStarted { get; internal set; }
    public bool LastBodySegmentLanded { get; internal set; }
    public bool DropRequested { get; internal set; }
    public bool WallCrumbleRequested { get; internal set; }
    public bool BossBitSet { get; internal set; }
    public ushort SavedHoleRingByteOffset { get; internal set; } = 0xffff;

    /// <summary>Thirteen actors, indexed by native spawn argument 0,2,...24 divided by two.</summary>
    public RoomEnemyProjectileSlot?[] BodySegments { get; } =
        new RoomEnemyProjectileSlot?[13];

    /// <summary>
    /// Native variable 10 for each aliased segment record. One means that body point is
    /// travelling inside the wall/hole; crossing the head's saved ring offset toggles it.
    /// </summary>
    internal bool[] SegmentInsideHole { get; } = new bool[13];

    /// <summary>One kilobyte of native position history, represented as 256 X/Y pairs.</summary>
    internal ushort[] HistoryX { get; } = new ushort[256];
    internal ushort[] HistoryY { get; } = new ushort[256];

    /// <summary>Four delayed head positions used to choose the visible head orientation.</summary>
    internal ushort[] HeadHistoryX { get; } = new ushort[4];
    internal ushort[] HeadHistoryY { get; } = new ushort[4];
}

/// <summary>Observable request emitted by Botwoon's specialized item-drop tail.</summary>
public readonly record struct BotwoonDropRequest(
    ushort X,
    ushort Y,
    ushort ItemDropChancesPointer);

/// <summary>Delayed music queue request emitted by Botwoon's completed death sequence.</summary>
public readonly record struct BotwoonMusicRequest(MusicCommand Command, MusicCommandDelay Delay);

/// <summary>
/// Cartridge-faithful translation of Botwoon definition <c>$F293</c>. Movement remains
/// table-driven: the four hole rectangles, phase speeds, random path descriptors, signed
/// path deltas, head animation lists, spit speeds, and health palettes are all read from the
/// retail ROM instead of being approximated with host curves.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort BotwoonDefinition = 0xf293;
    internal const ushort BotwoonTouchAi = 0x9fff;
    internal const ushort BotwoonShotAi = 0xa016;
    internal const ushort BotwoonPowerBombAi = 0xa041;

    private const ushort BotwoonInitialInstruction = 0x9389;
    private const int BotwoonMovementInstructionTable = 0xb3946b;
    private const int BotwoonSpitInstructionTable = 0xb3948b;
    private const int BotwoonHoleRectangleTable = 0xb3949b;
    private const int BotwoonSpeedSpacingTable = 0xb394bb;
    private const int BotwoonPaletteTable = 0xb3971b;
    private const int BotwoonPaletteThresholdTable = 0xb3981b;
    private const int BotwoonSpitSpeedTable = 0xb39e77;
    private const int BotwoonPathDescriptorTable = 0xb3e150;
    private const int BotwoonSpecialDropCount = 16;

    private BotwoonEnemyState? _botwoonState;
    private readonly List<BotwoonDropRequest> _botwoonDropRequests = new();

    /// <summary>Botwoon's slot-zero extra state, or null outside its room.</summary>
    public BotwoonEnemyState? Botwoon => _botwoonState;

    /// <summary>Last library-two sound selected by Botwoon in the current enemy frame.</summary>
    public ushort? LastBotwoonSoundEffect { get; private set; }

    /// <summary>Specialized drop request published when all body pieces have landed.</summary>
    public BotwoonDropRequest? LastBotwoonDropRequest { get; private set; }

    /// <summary>All sixteen ROM-authored scatter requests from the most recent death.</summary>
    public IReadOnlyList<BotwoonDropRequest> BotwoonDropRequests => _botwoonDropRequests;

    /// <summary>
    /// Hardcoded bank-$84 PLM requested by Botwoon. $B797 is the already-defeated wall;
    /// $B79B is the live crumble sequence.
    /// </summary>
    public ushort? LastBotwoonWallPlm { get; private set; }

    /// <summary>Track three, queued with the native eight-frame delay after wall cleanup.</summary>
    public BotwoonMusicRequest? LastBotwoonMusicRequest { get; private set; }

    private void ResetBotwoonRoomState()
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
        head.CurrentInstruction = BotwoonInitialInstruction;
        head.InstructionTimer = 1;
        head.Timer = 0;

        // A previously defeated Botwoon does not allocate body actors. The room loader
        // installs the permanent open wall and makes both one-screen scroll entries blue.
        if (RequireAreaMiniBossDefeated())
        {
            LastBotwoonWallPlm = 0xb797;
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
        state.Speed = ReadWord(_bus!, BotwoonSpeedSpacingTable);
        state.SegmentSpacingBytes = ReadWord(_bus!, BotwoonSpeedSpacingTable + 2);
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
        state.InstalledHeadInstruction = BotwoonInitialInstruction;
        head.Properties = unchecked((ushort)(head.Properties | 0x8000));

        for (int history = 0; history < 4; history++)
        {
            state.HeadHistoryX[history] = head.XPosition;
            state.HeadHistoryY[history] = head.YPosition;
        }
        Array.Fill(state.HistoryX, head.XPosition);
        Array.Fill(state.HistoryY, head.YPosition);
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
            choice = unchecked((ushort)(_nextRandom!() & 0x000e));
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
            head.Properties = unchecked((ushort)(head.Properties & ~0x8000));
        }
    }

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
            : unchecked((ushort)(_nextRandom!() & 1));
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

    private void RunBotwoonMovement(RoomEnemySlot head, BotwoonEnemyState state)
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

    private void MoveBotwoonTowardHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        int rectangle = BotwoonHoleRectangleTable + state.TargetHoleOffset;
        short dx = ClampBotwoonTargetDelta(
            unchecked((short)(ReadWord(_bus!, rectangle) + 4 - head.XPosition)));
        short dy = ClampBotwoonTargetDelta(
            unchecked((short)(ReadWord(_bus!, rectangle + 4) + 4 - head.YPosition)));
        byte angle = CalculateCartridgeAngle(dx, dy);
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

    private static short ClampBotwoonTargetDelta(short value) =>
        value < -256 ? (short)-255 : value >= 256 ? (short)255 : value;

    private void ChooseBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        UpdateBotwoonHealthPhase(head, state);
        ushort insideBase = state.InsideHole ? (ushort)128 : (ushort)0;
        state.PathChoiceOffset = unchecked((ushort)(
            (_nextRandom!() & 0x0018) + insideBase + 4 * state.TargetHoleOffset));
    }

    private void UpdateBotwoonHealthPhase(RoomEnemySlot head, BotwoonEnemyState state)
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
        state.Speed = ReadWord(_bus!, BotwoonSpeedSpacingTable + phase * 4);
        state.SegmentSpacingBytes = ReadWord(
            _bus!, BotwoonSpeedSpacingTable + phase * 4 + 2);
    }

    private void LoadBotwoonPathDescriptor(BotwoonEnemyState state)
    {
        int descriptor = BotwoonPathDescriptorTable + state.PathChoiceOffset;
        state.PathPointer = ReadWord(_bus!, descriptor);
        state.PathDirection = unchecked((short)ReadWord(_bus!, descriptor + 2));
        state.TargetHoleOffset = ReadWord(_bus!, descriptor + 4);
        if (state.PathDirection < 0)
            state.PathPointer = unchecked((ushort)(state.PathPointer - 4));
        state.PathComplete = false;
        state.MovementFunction = BotwoonMovementFunction.FollowAuthoredPath;
    }

    private void FollowBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        short totalX = 0;
        short totalY = 0;
        int step = state.PathDirection < 0 ? -2 : 2;
        for (int sample = 0; sample < state.Speed; sample++)
        {
            sbyte dx = unchecked((sbyte)_bus!.ReadByte(
                (int)new SnesAddress(0xb3, state.PathPointer)));
            if (dx == sbyte.MinValue)
            {
                state.PathComplete = true;
                return;
            }
            sbyte dy = unchecked((sbyte)_bus.ReadByte(
                0xb30000 | unchecked((ushort)(state.PathPointer + 1))));
            if (dy == sbyte.MinValue)
            {
                state.PathComplete = true;
                return;
            }
            totalX = unchecked((short)(totalX + dx));
            totalY = unchecked((short)(totalY + dy));
            state.PathPointer = unchecked((ushort)(state.PathPointer + step));
        }

        if (state.PathDirection < 0)
        {
            totalX = unchecked((short)-totalX);
            totalY = unchecked((short)-totalY);
        }
        head.XPosition = unchecked((ushort)(head.XPosition + totalX));
        head.YPosition = unchecked((ushort)(head.YPosition + totalY));
    }

    private static void RecordBotwoonHeadPosition(RoomEnemySlot head, BotwoonEnemyState state)
    {
        int ringIndex = state.RingByteOffset >> 2;
        state.HistoryX[ringIndex] = head.XPosition;
        state.HistoryY[ringIndex] = head.YPosition;
    }

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
            segment.XPosition = state.HistoryX[historyIndex];
            segment.YPosition = state.HistoryY[historyIndex];
            historyOffset = unchecked((ushort)(
                (historyOffset - state.SegmentSpacingBytes) & 0x03ff));
        }
    }

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
                dx = unchecked((short)(head.XPosition - segment.XPosition));
                dy = unchecked((short)(head.YPosition - segment.YPosition));
            }
            else
            {
                RoomEnemyProjectileSlot previous = state.BodySegments[segmentIndex + 1]!;
                dx = unchecked((short)(previous.XPosition - segment.XPosition));
                dy = unchecked((short)(previous.YPosition - segment.YPosition));
                if (argument == 0)
                    extraAngle = unchecked((ushort)(extraAngle + 512));
            }

            ushort angle = unchecked((ushort)(extraAngle + CalculateCartridgeAngle(dx, dy)));
            segment.DirectionParameter = unchecked((ushort)(2 * (angle >> 5)));
        }
    }

    private void DetectBotwoonHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.HoleLatch)
            return;

        for (int rectangleOffset = 24; rectangleOffset >= 0; rectangleOffset -= 8)
        {
            int rectangle = BotwoonHoleRectangleTable + rectangleOffset;
            ushort left = ReadWord(_bus!, rectangle);
            ushort right = ReadWord(_bus!, rectangle + 2);
            ushort top = ReadWord(_bus!, rectangle + 4);
            ushort bottom = ReadWord(_bus!, rectangle + 6);
            if (unchecked((short)(head.XPosition - left)) >= 0 &&
                unchecked((short)(head.XPosition - right)) < 0 &&
                unchecked((short)(head.YPosition - top)) >= 0 &&
                unchecked((short)(head.YPosition - bottom)) < 0)
            {
                state.HoleLatch = true;
                state.InsideHole = !state.InsideHole;
                state.SavedHoleRingByteOffset = state.RingByteOffset;
                return;
            }
        }
        state.HoleLatch = false;
    }

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

    private void AnimateBotwoonHeadFromMovement(RoomEnemySlot head, BotwoonEnemyState state)
    {
        short dx = unchecked((short)(head.XPosition - state.HeadHistoryX[3]));
        short dy = unchecked((short)(head.YPosition - state.HeadHistoryY[3]));
        if (dx != 0 || dy != 0)
        {
            ushort instruction;
            if (state.InsideHole)
            {
                head.Layer = 7;
                head.Properties = unchecked((ushort)(head.Properties | 0x8000));
                instruction = BotwoonInitialInstruction;
            }
            else
            {
                head.Layer = 2;
                head.Properties = unchecked((ushort)(head.Properties & ~0x8000));
                byte angle = CalculateCartridgeAngle(dx, dy);
                instruction = ReadWord(
                    _bus!, BotwoonMovementInstructionTable + (angle >> 5) * 2);
            }
            InstallBotwoonHeadInstruction(head, state, instruction);
        }

        for (int index = 3; index > 0; index--)
        {
            state.HeadHistoryX[index] = state.HeadHistoryX[index - 1];
            state.HeadHistoryY[index] = state.HeadHistoryY[index - 1];
        }
        state.HeadHistoryX[0] = head.XPosition;
        state.HeadHistoryY[0] = head.YPosition;
    }

    private void AimBotwoonAtSamus(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        head.Layer = 2;
        byte angle = CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - head.XPosition)),
            unchecked((short)(samus.YPosition - head.YPosition)));
        ushort instruction = ReadWord(
            _bus!, BotwoonSpitInstructionTable + (unchecked((byte)(angle + 16)) >> 5) * 2);
        InstallBotwoonHeadInstruction(head, state, instruction);
        state.SpitAngle = unchecked((byte)(64 - angle));
        state.HeadFunction = state.Function == BotwoonEnemyFunction.SpitWhileHidden
            ? BotwoonHeadFunction.WaitForSpitFrame
            : BotwoonHeadFunction.SpawnImmediateVolley;
        RunBotwoonHeadFunction(head, state, samus);
    }

    private static void InstallBotwoonHeadInstruction(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        ushort instruction)
    {
        if (instruction == state.InstalledHeadInstruction)
            return;
        state.InstalledHeadInstruction = instruction;
        head.CurrentInstruction = instruction;
        head.InstructionTimer = 1;
        head.Timer = 0;
    }

    private void SpawnBotwoonSpitVolley(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        int count,
        int startOffset)
    {
        ushort speed = ReadWord(_bus!, BotwoonSpitSpeedTable + state.HealthPhase * 2);
        byte angle = unchecked((byte)(state.SpitAngle + startOffset));
        for (int projectile = 0; projectile < count; projectile++)
        {
            SpawnBotwoonSpit(head, angle, speed);
            angle = unchecked((byte)(angle + 16));
        }
    }

    private void UpdateBotwoonHealthPalette(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.PalettePhaseByteOffset == 16)
            return;
        ushort threshold = ReadWord(
            _bus!, BotwoonPaletteThresholdTable + state.PalettePhaseByteOffset);
        if (unchecked((short)(head.Health - threshold)) >= 0)
            return;

        int source = BotwoonPaletteTable + 16 * state.PalettePhaseByteOffset;
        int destinationColor = state.PaletteDestinationByteOffset >> 1;
        int colorCount = 256 - destinationColor;
        _cgram!.LoadFromBus(_bus!, source, colorCount, destinationColor);
        state.PalettePhaseByteOffset = unchecked((ushort)(state.PalettePhaseByteOffset + 2));
    }

    private void RunBotwoonHeadFall(RoomEnemySlot head, BotwoonEnemyState state)
    {
        int displacement = ReadQuadraticEnemySpeed(
            unchecked((ushort)(state.DeathFallAccumulator >> 8)),
            negative: false);
        (head.YPosition, head.YSubposition) = AddBotwoonFixed(
            head.YPosition,
            head.YSubposition,
            displacement);
        if (unchecked((short)(head.YPosition - 200)) < 0)
        {
            state.DeathFallAccumulator = unchecked((ushort)(state.DeathFallAccumulator + 192));
            return;
        }

        head.YPosition = 200;
        state.Function = BotwoonEnemyFunction.WaitForBody;
        SpawnRoomGraphicsDustExplosion(head.XPosition, head.YPosition, animationIndex: 0x001d);
        LastBotwoonSoundEffect = 0x0024;
        head.Properties = unchecked((ushort)(head.Properties | 0x0500));
    }

    private void BeginBotwoonWallExplosions(RoomEnemySlot head, BotwoonEnemyState state)
    {
        state.Function = BotwoonEnemyFunction.WallExplosions;
        LastBotwoonWallPlm = 0xb79b;
        state.WallCrumbleRequested = true;

        // `Enemy_ItemDrop_Botwoon` at `$A0:BA3E` emits sixteen independent pickup
        // requests. Each position uses both bytes of one RNG result: X samples low seven
        // bits and Y samples bits 8..13. Each request is also materialized immediately as
        // a real $F337 actor using the retained six-byte item-chance pointer.
        for (int dropIndex = 0; dropIndex < BotwoonSpecialDropCount; dropIndex++)
        {
            ushort random = _nextRandom!();
            var request = new BotwoonDropRequest(
                X: unchecked((ushort)((random & 0x007f) + 64)),
                Y: unchecked((ushort)(((random & 0x3f00) >> 8) + 128)),
                ItemDropChancesPointer: head.Definition.ItemDropChancesPointer);
            _botwoonDropRequests.Add(request);
            LastBotwoonDropRequest = request;
            SpawnEnemyDropFromChanceTable(
                request.X,
                request.Y,
                request.ItemDropChancesPointer);
        }
        state.DropRequested = true;
        state.WallExplosionFrame = 0;
        state.LargeExplosionTimer = 0;
        state.SmallExplosionTimer = 0;
    }

    private void RunBotwoonWallExplosions(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.WallExplosionFrame >= 192)
        {
            head.Properties = head.Properties.With(EnemyProperties.Deleted);
            state.BossBitSet = true;
            RequireSetAreaMiniBossDefeated();
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
                SpawnRoomSpriteObject(
                    unchecked((ushort)((_nextRandom!() & 0x001f) + 232)),
                    unchecked((ushort)(state.WallExplosionFrame + (_nextRandom!() & 0x001f) - 8)),
                    RoomSpriteObjectKind.BotwoonLargeExplosion,
                    graphicsIndex: 0x0a00);
                LastBotwoonSoundEffect = 0x0024;
            }

            NativeWordCounterStep smallExplosionTimer =
                NativeWordCounter.Decrement(state.SmallExplosionTimer);
            state.SmallExplosionTimer = smallExplosionTimer.Value;
            if (smallExplosionTimer.IsNegative)
            {
                state.SmallExplosionTimer = 4;
                for (int explosion = 0; explosion < 2; explosion++)
                {
                    SpawnRoomSpriteObject(
                        unchecked((ushort)((_nextRandom!() & 0x003f) + 224)),
                        unchecked((ushort)(state.WallExplosionFrame + (_nextRandom!() & 0x001f) - 8)),
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
    private void ResolveBotwoonCombatAfterCommon(RoomEnemySlot head)
    {
        if (head.Health != 0)
            return;

        BotwoonEnemyState state = RequireBotwoonState(head);
        state.PendingDeath = true;

        // `$B3:96F5` sets native property $8000. Despite the disassembly's historical
        // "intangible" label, the engine uses this bit to admit the actor to solid-enemy
        // collision. Keeping the raw proven bit avoids assigning a broader enum meaning.
        head.Properties = unchecked((ushort)(head.Properties | 0x8000));
    }

    private void AddBotwoonAngleVector(
        RoomEnemySlot head,
        byte angle,
        ushort magnitude)
    {
        int xMagnitude = ReadUnsignedSineMagnitudeProduct(angle, magnitude, 0x40);
        int yMagnitude = ReadUnsignedSineMagnitudeProduct(angle, magnitude, 0x80);
        int xDisplacement = ((angle + 64) & 0x80) != 0 ? -xMagnitude : xMagnitude;
        int yDisplacement = ((angle + 128) & 0x80) != 0 ? -yMagnitude : yMagnitude;
        (head.XPosition, head.XSubposition) = AddBotwoonFixed(
            head.XPosition,
            head.XSubposition,
            xDisplacement);
        (head.YPosition, head.YSubposition) = AddBotwoonFixed(
            head.YPosition,
            head.YSubposition,
            yDisplacement);
    }

    private static (ushort Position, ushort Subposition) AddBotwoonFixed(
        ushort position,
        ushort subposition,
        int displacement)
    {
        int fixedPosition = unchecked((position << 16) | subposition);
        fixedPosition = unchecked(fixedPosition + displacement);
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    /// <summary>Animation callback dispatcher for $B3:94C7-$9572.</summary>
    private bool TryProcessBotwoonInstruction(
        RoomEnemySlot head,
        ushort opcode,
        ref ushort cursor)
    {
        if (head.EnemyDefinitionPointer != BotwoonDefinition)
            return false;
        BotwoonEnemyState state = RequireBotwoonState(head);
        switch (opcode)
        {
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10:
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate:
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate_again:
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_8x10_duplicate_again2:
                head.XRadius = 8;
                head.YRadius = 16;
                break;
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC:
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate:
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate_again:
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_CxC_duplicate_again2:
                head.XRadius = 12;
                head.YRadius = 12;
                break;
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_10x8:
            case BotwoonCodePointers.Instruction_Botwoon_EnemyRadius_10x8_duplicate:
                head.XRadius = 16;
                head.YRadius = 8;
                break;
            case BotwoonCodePointers.Instruction_Botwoon_SetSpittingFlag:
                state.SpitFrameReached = true;
                break;
            case BotwoonCodePointers.Instruction_Botwoon_QueueSpitSFX:
                LastBotwoonSoundEffect = 0x007c;
                break;
            default:
                return false;
        }
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    private BotwoonEnemyState RequireBotwoonState(RoomEnemySlot head) =>
        _botwoonState is not null && head.SlotIndex == 0
            ? _botwoonState
            : throw new InvalidOperationException(
                $"Enemy slot {head.SlotIndex} has no initialized Botwoon state.");
}
