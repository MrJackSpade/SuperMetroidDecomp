using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Debugger-facing projection of Botwoon's large extra-RAM allocation. The original uses several disjoint WRAM pages
/// and aliases thirteen segment records through enemy-shaped structures. Named host fields retain those exact meanings
/// without pretending the words belong to thirteen ordinary enemy slots. The values the cartridge keeps in the head's
/// common record are emptied with it when the head is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class BotwoonEnemyState
{
    /// <summary>
    /// The bank-$B3 main-function pointer, owning initial waiting, traversal/spitting, and synchronized death
    /// progression; none once a kill has emptied the record.
    /// </summary>
    public BotwoonEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The independent bank-$B3 movement callback, choosing direct hole movement or descriptor-driven signed path
    /// samples; none once a kill has emptied the record.
    /// </summary>
    public BotwoonMovementFunction? MovementFunction { get; internal set; }

    /// <summary>
    /// The independent bank-$B3 head callback, controlling movement orientation, aiming, spit-frame admission, and
    /// cooldown; none once a kill has emptied the record.
    /// </summary>
    public BotwoonHeadFunction? HeadFunction { get; internal set; }

    /// <summary>Cursor 0-255 into the X/Y history ring; each moving frame writes its head position and advances modulo 256.</summary>
    public int RingIndex { get; internal set; }
    /// <summary>History-entry distance between successive body samples, selected inversely to movement speed: 6, 4, or 3.</summary>
    public int SegmentSpacingEntries { get; internal set; }
    /// <summary>Initial unsigned countdown, seeded to 256 and decremented until zero before normal traversal begins.</summary>
    public WrappingCountdown InitialDelayTimer { get; internal set; }
    /// <summary>Spit-action cooldown, seeded to 48 for the stationary attack and decremented to signed underflow by the head callback before being pinned to zero.</summary>
    public WrappingCountdown AttackTimer { get; internal set; }
    /// <summary>
    /// Pre-fall death count: seeded to 240 once the tail completes the hole transition and incremented once per update
    /// until it reaches 256, when the head starts falling; it stays within 240..256.
    /// </summary>
    public int DeathTimer { get; internal set; }
    /// <summary>
    /// Head-fall acceleration in 256ths of a quadratic-speed step: each still-airborne update adds 192, and the whole
    /// steps select the fall speed. The fall reaches the floor before the speed table runs out, so it stays well below
    /// 95 whole steps.
    /// </summary>
    public int DeathFallAccumulator { get; internal set; }
    /// <summary>Elapsed wall-explosion update count 0 through 192, also used as the vertical progression of randomized explosion positions.</summary>
    public int WallExplosionFrame { get; internal set; }
    /// <summary>Large wall-explosion countdown, active from update 64 and reloaded to 12 after signed underflow.</summary>
    public WrappingCountdown LargeExplosionTimer { get; internal set; }
    /// <summary>Small wall-explosion countdown, active from update 64 and reloaded to four after signed underflow to emit a pair of effects.</summary>
    public WrappingCountdown SmallExplosionTimer { get; internal set; }
    /// <summary>Health-selected movement magnitude 2, 3, or 4: pixels per direct-vector update or signed path samples consumed per authored-path update.</summary>
    public int Speed { get; internal set; }
    /// <summary>The hole, zero through three, that direct movement targets and the next path choice starts from.</summary>
    public int TargetHoleIndex { get; internal set; }
    /// <summary>Wrapping movement angle in 256ths of a turn: a quarter turn minus the clamped head-to-hole angle, consumed by the signed fixed-point vector helper.</summary>
    public RotationAngle MovementAngle { get; internal set; }
    /// <summary>Wrapping spit angle in 256ths of a turn: a quarter turn minus the head-to-Samus angle; volley children add their spread offsets to it.</summary>
    public RotationAngle SpitAngle { get; internal set; }
    /// <summary>
    /// The authored path chosen next, zero through 31: one of four random variants for the target hole, in the visible
    /// half of the table or, from inside a hole, the hidden half.
    /// </summary>
    public int PathChoice { get; internal set; }
    /// <summary>The next movement sample of the current path, advanced or reversed by one sample per step.</summary>
    public int PathSampleIndex { get; internal set; }
    /// <summary>Signed descriptor direction: negative walks the samples backward and negates accumulated displacements; nonnegative walks forward.</summary>
    [PackedFields("signed path descriptor word; only its sign is read, choosing backward or forward sample order")]
    public int PathDirection { get; internal set; }
    /// <summary>The head animation the AI last installed; reinstalling the same program does not restart it.</summary>
    internal BotwoonHeadProgram HeadProgram { get; set; } = BotwoonHeadProgram.Hidden;
    /// <summary>The next step of the head animation's timeline.</summary>
    internal int HeadAnimationPosition { get; set; }
    /// <summary>CGRAM destination color; retail initialization targets color $F0, the start of OBJ palette seven.</summary>
    public int PaletteFirstColour { get; internal set; }
    /// <summary>Band 0 through 7 selecting the next health threshold/palette; 8 marks completion of all eight palette steps.</summary>
    public int PalettePhase { get; internal set; }
    /// <summary>Head health retained from initialization, independently of subsequent damage and the palette threshold table.</summary>
    public int MaximumHealth { get; internal set; }
    /// <summary>Initial maximum health shifted right one, used as the strict threshold for movement health phase 1.</summary>
    public int HalfHealth { get; internal set; }
    /// <summary>Initial maximum health shifted right two, used as the strict threshold for the fastest movement health phase.</summary>
    public int QuarterHealth { get; internal set; }
    /// <summary>Native previous-health extended-word snapshot, initialized from the head's spawn health separately from its maximum/threshold fields.</summary>
    public int PreviousHealth { get; internal set; }
    /// <summary>Movement/spit speed record index 0, 1, or 2, refreshed when choosing a path outside a hole according to half/quarter-health thresholds.</summary>
    [IntegerQuantity("count of health thresholds passed, zero through two, indexing the speed records")]
    public int HealthPhase { get; internal set; }
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
    /// <summary>Ring index saved at a head hole crossing so delayed body samples toggle at the same position; null (native $FFFF) means no crossing remains pending.</summary>
    public int? SavedHoleRingIndex { get; internal set; }

    /// <summary>Thirteen actors, indexed by native spawn argument 0,2,...24 divided by two.</summary>
    public RoomEnemyProjectileSlot?[] BodySegments { get; } =
        new RoomEnemyProjectileSlot?[13];

    /// <summary>The spawned body segment at <paramref name="index"/>; body movement runs only after all thirteen spawn.</summary>
    internal RoomEnemyProjectileSlot Segment(int index) => BodySegments[index] ??
        throw new InvalidOperationException($"Botwoon body segment {index} has not spawned or lost its projectile slot.");

    /// <summary>
    /// Native variable 10 for each aliased segment record. One means that body point is travelling inside the
    /// wall/hole; crossing the head's saved ring offset toggles it.
    /// </summary>
    internal bool[] SegmentInsideHole { get; } = new bool[13];

    /// <summary>
    /// Each body segment's phase, by segment index (#1413). The cartridge kept it in the segment projectile's X-velocity
    /// word, which no body motion reads; the phase is the segment's own state here.
    /// </summary>
    internal BotwoonBodyFunction[] SegmentFunctions { get; } = new BotwoonBodyFunction[13];

    /// <summary>One kilobyte of native position history, represented as 256 whole-pixel positions.</summary>
    internal Position16<RoomPixels>[] History { get; } = new Position16<RoomPixels>[256];

    /// <summary>Four delayed head positions used to choose the visible head orientation.</summary>
    internal Position16<RoomPixels>[] HeadHistory { get; } = new Position16<RoomPixels>[4];

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        MovementFunction = null;
        HeadFunction = null;
    }
}
