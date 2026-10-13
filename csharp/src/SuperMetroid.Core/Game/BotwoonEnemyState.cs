using SuperMetroid.Core.Assets;
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
    /// <summary>Botwoon's live head slot, whose variables D through F select three independent callbacks.</summary>
    private readonly RoomEnemySlot _head;

    /// <summary>Attaches the named boss state to its physical head enemy slot.</summary>
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
    /// <summary>The head animation the AI last installed; reinstalling the same program does not restart it.</summary>
    internal BotwoonHeadProgram HeadProgram { get; set; } = BotwoonHeadProgram.Hidden;
    /// <summary>The next step of the head animation's timeline.</summary>
    internal int HeadAnimationPosition { get; set; }
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

    /// <summary>One kilobyte of native position history, represented as 256 whole-pixel positions.</summary>
    internal Position16<RoomPixels>[] History { get; } = new Position16<RoomPixels>[256];

    /// <summary>Four delayed head positions used to choose the visible head orientation.</summary>
    internal Position16<RoomPixels>[] HeadHistory { get; } = new Position16<RoomPixels>[4];
}

/// <summary>Observable request emitted by Botwoon's specialized item-drop tail.</summary>
/// <param name="Position">Wrapping room position, randomized to X 64 through 191 and Y 128 through 191 by the specialized sixteen-drop producer.</param>
/// <param name="ItemDropChancesPointer">The retained head definition's native six-entry item-chance-table pointer, consumed by ordinary pickup selection.</param>
public readonly record struct BotwoonDropRequest(
    Position16<RoomPixels> Position,
    ushort ItemDropChancesPointer);

/// <summary>Delayed music queue request emitted by Botwoon's completed death sequence.</summary>
/// <param name="Command">Full cartridge music-command word; normal Botwoon completion selects track three.</param>
/// <param name="Delay">The queue admission delay; normal completion uses the authored eight-frame delay.</param>
public readonly record struct BotwoonMusicRequest(MusicCommand Command, MusicCommandDelay Delay);
