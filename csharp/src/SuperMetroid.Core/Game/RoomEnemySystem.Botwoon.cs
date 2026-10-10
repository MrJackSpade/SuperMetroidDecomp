using SuperMetroid.Core.Hardware;

using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$B3 function pointer stored in Botwoon's variable D. The values are kept as
/// cartridge addresses so debugger watches can be compared directly with the native actor.
/// </summary>
public enum BotwoonEnemyFunction : ushort
{
    /// <summary><c>$B3:9878 Function_Botwoon_Initial</c>: decrements the 256-update opening delay and selects hole traversal when it reaches zero.</summary>
    InitialDelay = 0x9878,
    /// <summary><c>$B3:989D Function_Botwoon_GoThroughHole</c>: moves toward the target hole, detects crossing, and chooses an authored path or stationary spit action.</summary>
    ChooseNextAction = 0x989d,
    /// <summary><c>$B3:99A4 Function_Botwoon_MovingAround</c>: advances an authored movement path and body history until its terminator selects direct hole movement.</summary>
    FollowAuthoredPath = 0x99a4,
    /// <summary><c>$B3:99E4 Function_Botwoon_Spitting</c>: holds position for the aimed spit animation/cooldown, then chooses another path and inside-hole state.</summary>
    SpitWhileHidden = 0x99e4,
    /// <summary><c>$B3:9A46 Function_Botwoon_DeathSequence_PreDeathDelay</c>: increments the head's death timer from 240 to 256 while body actors begin their staggered death delays.</summary>
    DeathDelay = 0x9a46,
    /// <summary><c>$B3:9A5E Function_Botwoon_DeathSequence_FallingToGround</c>: accelerates through the quadratic speed table until head Y reaches 200, then hides the landed head.</summary>
    HeadFalling = 0x9a5e,
    /// <summary><c>$B3:9ACA Function_Botwoon_DeathSequence_WaitForBodyToFallToGround</c>: waits for the final body point's landing flag before requesting drops and wall crumbling.</summary>
    WaitForBody = 0x9aca,
    /// <summary><c>$B3:9AF9 Function_Botwoon_DeathSequence_CrumblingWall</c>: runs the 192-update wall-explosion phase, then deletes the head, sets the miniboss bit, and queues track three.</summary>
    WallExplosions = 0x9af9,
}

/// <summary>Literal movement callback stored in Botwoon's variable E.</summary>
public enum BotwoonMovementFunction : ushort
{
    /// <summary><c>$B3:9BB7 Function_Botwoon_Movement_DirectlyTowardTargetHole</c>: computes a clamped target vector and moves until the inside-hole transition is observed.</summary>
    MoveTowardHole = 0x9bb7,
    /// <summary><c>$B3:E250 Function_Botwoon_Movement_StartMovingAccordingToMovementData</c>: loads the chosen descriptor's pointer, direction, and target hole, then follows its first samples on the same call.</summary>
    LoadAuthoredPath = 0xe250,
    /// <summary><c>$B3:E28C Function_Botwoon_Movement_MoveAccordingToMovementData</c>: consumes speed-count signed X/Y byte pairs in the selected direction until a $80 terminator marks completion.</summary>
    FollowAuthoredPath = 0xe28c,
}

/// <summary>Literal head/attack callback stored in Botwoon's variable F.</summary>
public enum BotwoonHeadFunction : ushort
{
    /// <summary><c>$B3:9DC0 Function_Botwoon_Head_MovingAround</c>: derives orientation from delayed head positions and selects hidden/tangible drawing according to hole state.</summary>
    AnimateFromMovement = 0x9dc0,
    /// <summary><c>$B3:9E7D Function_Botwoon_Head_Spitting_SetAngleAndShow</c>: aims at Samus, installs the spit list, and immediately selects the five-shot animation wait or three-shot volley.</summary>
    AimAtSamus = 0x9e7d,
    /// <summary><c>$B3:9EE0 Function_Botwoon_Head_Spitting_Spawn5SpitProjectiles</c>: waits for the head bytecode's spit-frame flag, then emits five projectiles spaced sixteen angle units apart.</summary>
    WaitForSpitFrame = 0x9ee0,
    /// <summary><c>$B3:9F34 Function_Botwoon_Head_Spitting_Spawn3SpitProjectiles</c>: emits three aimed projectiles immediately and selects the attack cooldown.</summary>
    SpawnImmediateVolley = 0x9f34,
    /// <summary><c>$B3:9F7A Function_Botwoon_Head_Spitting_Cooldown</c>: decrements the attack timer through zero to signed underflow, pins it to zero, and restores movement-based animation.</summary>
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

    /// <summary>Variable D's bank-$B3 main-function pointer, owning initial waiting, traversal/spitting, and synchronized death progression.</summary>
    public BotwoonEnemyFunction Function
    {
        get => (BotwoonEnemyFunction)_head.VariableD;
        internal set => _head.VariableD = (ushort)value;
    }

    /// <summary>Variable E's independent bank-$B3 movement callback, choosing direct hole movement or descriptor-driven signed path samples.</summary>
    public BotwoonMovementFunction MovementFunction
    {
        get => (BotwoonMovementFunction)_head.VariableE;
        internal set => _head.VariableE = (ushort)value;
    }

    /// <summary>Variable F's independent bank-$B3 head callback, controlling movement orientation, aiming, spit-frame admission, and cooldown.</summary>
    public BotwoonHeadFunction HeadFunction
    {
        get => (BotwoonHeadFunction)_head.VariableF;
        internal set => _head.VariableF = (ushort)value;
    }

    /// <summary>Four-byte-aligned cursor $000-$3FC into the 1-KiB X/Y history ring; each moving frame writes its head position and advances modulo $400.</summary>
    public ushort RingByteOffset { get; internal set; }
    /// <summary>Byte distance between successive body samples in the history ring, selected inversely to movement speed: 24, 16, or 12.</summary>
    public ushort SegmentSpacingBytes { get; internal set; }
    /// <summary>Initial unsigned countdown, seeded to 256 and decremented until zero before normal traversal begins.</summary>
    public ushort InitialDelayTimer { get; internal set; }
    /// <summary>Spit-action cooldown, seeded to 48 for the stationary attack and decremented to signed underflow by the head callback before being pinned to zero.</summary>
    public ushort AttackTimer { get; internal set; }
    /// <summary>Pre-fall death count, seeded to 240 once the tail completes the hole transition and incremented toward 256.</summary>
    public ushort DeathTimer { get; internal set; }
    /// <summary>Head-fall acceleration cursor; its high byte indexes the quadratic speed table and each still-airborne update adds $00C0.</summary>
    public ushort DeathFallAccumulator { get; internal set; }
    /// <summary>Elapsed wall-explosion update count 0 through 192, also used as the vertical progression of randomized explosion positions.</summary>
    public ushort WallExplosionFrame { get; internal set; }
    /// <summary>Large wall-explosion countdown, active from update 64 and reloaded to 12 after signed underflow.</summary>
    public ushort LargeExplosionTimer { get; internal set; }
    /// <summary>Small wall-explosion countdown, active from update 64 and reloaded to four after signed underflow to emit a pair of effects.</summary>
    public ushort SmallExplosionTimer { get; internal set; }
    /// <summary>Health-selected movement magnitude 2, 3, or 4: pixels per direct-vector update or signed path samples consumed per authored-path update.</summary>
    public ushort Speed { get; internal set; }
    /// <summary>Byte offset 0, 8, 16, or 24 selecting one of four hole rectangles and its direct-movement target.</summary>
    public ushort TargetHoleOffset { get; internal set; }
    /// <summary>Raw byte-angle result retained in a word for the clamped head-to-hole vector, before conversion to the movement angle convention.</summary>
    public ushort TargetAngle { get; internal set; }
    /// <summary>Wrapping movement byte-angle, calculated as 64 minus the target angle and consumed by the signed fixed-point vector helper.</summary>
    public byte MovementAngle { get; internal set; }
    /// <summary>Wrapping spit byte-angle, calculated as 64 minus the head-to-Samus angle; volley children add their spread offsets to it.</summary>
    public byte SpitAngle { get; internal set; }
    /// <summary>Byte offset into the authored descriptor table, combining an RNG choice, the current target hole, and the inside-hole table half.</summary>
    public ushort PathChoiceOffset { get; internal set; }
    /// <summary>Low-word pointer in bank $B3 to the next signed X/Y movement-byte pair, advanced or reversed by two bytes per sample.</summary>
    public ushort PathPointer { get; internal set; }
    /// <summary>Signed descriptor direction: negative walks the sample pointer backward and negates accumulated displacements; nonnegative walks forward.</summary>
    public short PathDirection { get; internal set; }
    /// <summary>Last head list explicitly installed by AI; separate from the advancing physical instruction pointer so unchanged orientation does not restart bytecode.</summary>
    public ushort InstalledHeadInstruction { get; internal set; }
    /// <summary>CGRAM destination measured in bytes; retail initialization targets color $F0, the start of OBJ palette seven.</summary>
    public ushort PaletteDestinationByteOffset { get; internal set; }
    /// <summary>Even byte offset 0 through 14 selecting the next health threshold/palette; offset 16 marks completion of all eight palette steps.</summary>
    public ushort PalettePhaseByteOffset { get; internal set; }
    /// <summary>Head health retained from initialization, independently of subsequent damage and the palette threshold table.</summary>
    public ushort MaximumHealth { get; internal set; }
    /// <summary>Initial maximum health shifted right one, used as the strict threshold for movement health phase 1.</summary>
    public ushort HalfHealth { get; internal set; }
    /// <summary>Initial maximum health shifted right two, used as the strict threshold for the fastest movement health phase.</summary>
    public ushort QuarterHealth { get; internal set; }
    /// <summary>Native previous-health extended-word snapshot, initialized from the head's spawn health separately from its maximum/threshold fields.</summary>
    public ushort PreviousHealth { get; internal set; }
    /// <summary>Movement/spit speed record index 0, 1, or 2, refreshed when choosing a path outside a hole according to half/quarter-health thresholds.</summary>
    public byte HealthPhase { get; internal set; }
    /// <summary>Whether the head travels within the wall/hole; crossing a hole rectangle toggles this state and hidden movement disables collision.</summary>
    public bool InsideHole { get; internal set; }
    /// <summary>Inside-hole state observed by the direct-movement callback on its previous call; a difference marks that traversal complete.</summary>
    public bool PreviousInsideHole { get; internal set; }
    /// <summary>Suppresses repeated rectangle-trigger toggles until the last body point reaches the saved ring offset or a new hidden traversal clears it.</summary>
    public bool HoleLatch { get; internal set; }
    /// <summary>Initial-action latch forcing the first completed hole traversal into authored movement rather than consuming the normal spit-choice RNG.</summary>
    public bool InitialAction { get; internal set; }
    /// <summary>Movement completion signal, raised by a path terminator or observed hole-state transition and consumed by the main action dispatcher.</summary>
    public bool PathComplete { get; internal set; }
    /// <summary>Flag set by the head animation's private spit opcode; the five-projectile callback consumes and clears it on emission.</summary>
    public bool SpitFrameReached { get; internal set; }
    /// <summary>Fatal-hit request that preserves ongoing traversal until the final body point completes the next outward hole transition.</summary>
    public bool PendingDeath { get; internal set; }
    /// <summary>Tail synchronization signal set when body argument zero crosses the saved history offset while the head is outside, permitting pending death to begin.</summary>
    public bool ExitTransitionComplete { get; internal set; }
    /// <summary>Shared death-start flag observed by all thirteen independent body-projectile turns to replace their normal animation with staggered falling.</summary>
    public bool BodyDeathStarted { get; internal set; }
    /// <summary>Landing signal from retained body argument zero, releasing the head's wait-for-body phase into wall crumbling and specialized drops.</summary>
    public bool LastBodySegmentLanded { get; internal set; }
    /// <summary>Witness that the specialized sixteen-pickup request sequence has been emitted after the final body landing.</summary>
    public bool DropRequested { get; internal set; }
    /// <summary>Witness that initialization or death requested the appropriate bank-$84 Botwoon wall-clear/crumble PLM.</summary>
    public bool WallCrumbleRequested { get; internal set; }
    /// <summary>Whether defeat persistence has been recognized or written; live death sets the area miniboss bit only after the complete wall-explosion phase.</summary>
    public bool BossBitSet { get; internal set; }
    /// <summary>Ring byte offset saved at a head hole crossing so delayed body samples toggle at the same position; $FFFF means no crossing remains pending.</summary>
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
/// <param name="X">Wrapping room-world pixel X, randomized to 64 through 191 by the specialized sixteen-drop producer.</param>
/// <param name="Y">Wrapping room-world pixel Y, randomized to 128 through 191 by that same producer.</param>
/// <param name="ItemDropChancesPointer">The retained head definition's native six-entry item-chance-table pointer, consumed by ordinary pickup selection.</param>
public readonly record struct BotwoonDropRequest(
    ushort X,
    ushort Y,
    ushort ItemDropChancesPointer);

/// <summary>Delayed music queue request emitted by Botwoon's completed death sequence.</summary>
/// <param name="Command">Full cartridge music-command word; normal Botwoon completion selects track three.</param>
/// <param name="Delay">The queue admission delay; normal completion uses the authored eight-frame delay.</param>
public readonly record struct BotwoonMusicRequest(MusicCommand Command, MusicCommandDelay Delay);

/// <summary>
/// Cartridge-faithful translation of Botwoon definition <c>$F293</c>. Movement remains
/// table-driven: the four hole rectangles, random path descriptors, and instruction selectors
/// are compiled as fixed mechanics metadata together with the complete signed path corpus.
/// Selected instruction programs and health palettes remain authored cartridge presentation
/// data rather than host approximations.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort BotwoonTouchAi = EnemyAiCodePointers.BankB3.BotwoonTouch;
    internal const ushort BotwoonShotAi = EnemyAiCodePointers.BankB3.BotwoonShot;
    internal const ushort BotwoonPowerBombAi = EnemyAiCodePointers.BankB3.BotwoonPowerBomb;

    private const int BotwoonSpecialDropCount = 16;

    private BotwoonEnemyState? _botwoonState;
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
        head.CurrentInstruction = BotwoonInstructionDefinitions.HiddenHeadInstruction;
        head.InstructionTimer = 1;
        head.Timer = 0;

        // A previously defeated Botwoon does not allocate body actors. The room loader
        // installs the permanent open wall and makes both one-screen scroll entries blue.
        if (RequireAreaMiniBossDefeated())
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
        state.InstalledHeadInstruction = BotwoonInstructionDefinitions.HiddenHeadInstruction;
        // `$B3:961B` ORs $0400: the head starts hidden in its hole, out of every collision pass.
        head.Properties = head.Properties.With(EnemyProperties.IgnoreSamusCollision);

        for (int history = 0; history < 4; history++)
        {
            state.HeadHistoryX[history] = head.XPosition;
            state.HeadHistoryY[history] = head.YPosition;
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
            // `$B3:992C` (AND #$FBFF) makes the spitting head tangible again.
            head.Properties = head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
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

    private static void RunBotwoonMovement(RoomEnemySlot head, BotwoonEnemyState state)
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

    private static void MoveBotwoonTowardHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        BotwoonHoleDefinition hole =
            BotwoonNavigationDefinitions.HoleForByteOffset(state.TargetHoleOffset);
        short dx = ClampBotwoonTargetDelta(
            unchecked((short)(hole.TargetX - head.XPosition)));
        short dy = ClampBotwoonTargetDelta(
            unchecked((short)(hole.TargetY - head.YPosition)));
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

    private static void UpdateBotwoonHealthPhase(RoomEnemySlot head, BotwoonEnemyState state)
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

    private static void FollowBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        short totalX = 0;
        short totalY = 0;
        int step = state.PathDirection < 0 ? -2 : 2;
        for (int sample = 0; sample < state.Speed; sample++)
        {
            BotwoonMovementSample movement =
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
            totalX = unchecked((short)(totalX + movement.X));
            totalY = unchecked((short)(totalY + movement.Y));
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

    private static void DetectBotwoonHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.HoleLatch)
            return;

        for (int rectangleOffset = 24; rectangleOffset >= 0; rectangleOffset -= 8)
        {
            BotwoonHoleDefinition hole = BotwoonNavigationDefinitions.HoleForByteOffset(
                unchecked((ushort)rectangleOffset));
            if (unchecked((short)(head.XPosition - hole.Left)) >= 0 &&
                unchecked((short)(head.XPosition - hole.Right)) < 0 &&
                unchecked((short)(head.YPosition - hole.Top)) >= 0 &&
                unchecked((short)(head.YPosition - hole.Bottom)) < 0)
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

    private static void AnimateBotwoonHeadFromMovement(RoomEnemySlot head, BotwoonEnemyState state)
    {
        short dx = unchecked((short)(head.XPosition - state.HeadHistoryX[3]));
        short dy = unchecked((short)(head.YPosition - state.HeadHistoryY[3]));
        if (dx != 0 || dy != 0)
        {
            ushort instruction;
            if (state.InsideHole)
            {
                head.Layer = 7;
                // `$B3:9DF7` ORs $0400 while the head travels inside the wall.
                head.Properties = head.Properties.With(EnemyProperties.IgnoreSamusCollision);
                instruction = BotwoonInstructionDefinitions.HiddenHeadInstruction;
            }
            else
            {
                head.Layer = 2;
                // `$B3:9E10` (AND #$FBFF) restores collision once the head is outside.
                head.Properties = head.Properties.Without(EnemyProperties.IgnoreSamusCollision);
                byte angle = CalculateCartridgeAngle(dx, dy);
                instruction = BotwoonInstructionDefinitions.HeadMovementInstruction(angle);
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
        ushort instruction = BotwoonInstructionDefinitions.HeadSpitInstruction(angle);
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
        ushort speed = BotwoonSpeedDefinitions.ForHealthPhase(state.HealthPhase).SpitSpeed;
        byte angle = unchecked((byte)(state.SpitAngle + startOffset));
        for (int projectile = 0; projectile < count; projectile++)
        {
            SpawnBotwoonSpit(head, angle, speed);
            angle = unchecked((byte)(angle + 16));
        }
    }

    private void UpdateBotwoonHealthPalette(RoomEnemySlot head, BotwoonEnemyState state)
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
        if (TileArtwork?.BotwoonColors is { } colors)
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
                _cgram!.SetColor(destinationColor + color,
                    colors.HealthColor(band, color));
        }
        else throw new InvalidOperationException("Botwoon requires installed health colors.");
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
        head.Properties = head.Properties.With(
            EnemyProperties.IgnoreSamusCollision | EnemyProperties.Invisible);
    }

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

        // `$B3:96F5` is `ORA #$0400` (ROM bytes 09 00 04): the dying head leaves the
        // projectile, bomb and touch passes, so a later hit cannot interrupt its fall.
        head.Properties = head.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }

    private static void AddBotwoonAngleVector(
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
        // The shared interpreter hands every remaining negative word here; only
        // Botwoon's private opcodes belong to this handler.
        if (head.EnemyDefinitionPointer != EnemyDefinitionId.Botwoon ||
            !Enum.IsDefined((BotwoonInstruction)opcode))
            return false;
        BotwoonEnemyState state = RequireBotwoonState(head);
        switch ((BotwoonInstruction)opcode)
        {
            case BotwoonInstruction.EnemyRadius_8x10:
            case BotwoonInstruction.EnemyRadius_8x10_duplicate:
            case BotwoonInstruction.EnemyRadius_8x10_duplicate_again:
            case BotwoonInstruction.EnemyRadius_8x10_duplicate_again2:
                head.XRadius = 8;
                head.YRadius = 16;
                break;
            case BotwoonInstruction.EnemyRadius_CxC:
            case BotwoonInstruction.EnemyRadius_CxC_duplicate:
            case BotwoonInstruction.EnemyRadius_CxC_duplicate_again:
            case BotwoonInstruction.EnemyRadius_CxC_duplicate_again2:
                head.XRadius = 12;
                head.YRadius = 12;
                break;
            case BotwoonInstruction.EnemyRadius_10x8:
            case BotwoonInstruction.EnemyRadius_10x8_duplicate:
                head.XRadius = 16;
                head.YRadius = 8;
                break;
            case BotwoonInstruction.SetSpittingFlag:
                state.SpitFrameReached = true;
                break;
            case BotwoonInstruction.QueueSpitSFX:
                LastBotwoonSoundEffect = 0x007c;
                break;
            default:
                throw new InvalidOperationException($"Undefined Botwoon instruction ${opcode:X4}.");
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
