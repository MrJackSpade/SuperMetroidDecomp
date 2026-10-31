using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Shared Kraid encounter state at WRAM <c>$7E:7800</c>, plus the room-level effects whose
/// native owners sit outside an individual enemy record. No host-only combat simplification
/// is represented here: thresholds and function pointers are copied from retail arithmetic.
/// </summary>
public sealed class KraidEnemyState
{
    /// <summary>Gets eight native-order part workspaces: body, arm, three lints, foot, and two fingernails; they remain owned by the room after the body is deleted.</summary>
    public KraidPartState[] Parts { get; } = Enumerable.Range(0, 8)
        .Select(_ => new KraidPartState())
        .ToArray();

    /// <summary>
    /// An unresolved encounter value: mouth-reopen continuations set it to two ($A7:AF24/$BC58) and nothing reads it.
    /// It holds zero or two.
    /// </summary>
    public int Unknown2 { get; internal set; }
    /// <summary>Gets the body think countdown selected from the native random-delay table and consumed before the next mouth attack.</summary>
    public WrappingCountdown ThinkingTimer { get; internal set; }
    /// <summary>
    /// Gets the room position whose Y is the highest Samus may be pushed to when Kraid's body ejects her: Y 324 for the
    /// small form and 164 once growth raises the clamp. Only the Y axis is meaningful; Samus's position is compared with
    /// it by the wrapped spatial difference on Y.
    /// </summary>
    public Position16<RoomPixels> EjectionLimit { get; internal set; }
    /// <summary>Gets the mouth's shot latches: a charged hit this pass, an armed reopen and a pending reopen.</summary>
    public KraidMouthFlags MouthFlags { get; internal set; }
    /// <summary>
    /// Gets the number of mouth reopens still pending, from zero to two hundred fifty-five. A charged body hit arms
    /// three; each reopen continuation removes one.
    /// </summary>
    public int PendingMouthReopens { get; internal set; }
    /// <summary>Health captured by the living initializer before combat can change it.</summary>
    public int InitialHealth { get; internal set; }
    /// <summary>Returns the cartridge threshold for the selected eighth of the initializer's captured health.</summary>
    /// <param name="index">A zero-based table index from zero through seven, selecting one through eight eighths.</param>
    /// <returns>The initial health truncated to one eighth, multiplied by the selected number of eighths.</returns>
    public int HealthEighthThreshold(int index) => KraidHealthThresholds.Eighth(InitialHealth, index);
    /// <summary>
    /// Gets the room position the second-phase body walks to: X from 208 to 384. Only the X axis is meaningful; the
    /// body's position is compared with it by the wrapped spatial difference on X.
    /// </summary>
    public Position16<RoomPixels> WalkTarget { get; internal set; }
    /// <summary>Gets the hurt-palette phase, from zero to six: started at six by a vulnerable mouth hit and stepped down by its frame timer.</summary>
    public int HurtFrame { get; internal set; }
    /// <summary>Gets the countdown pacing hurt-palette phases, normally reloaded to two and reused by background fades during death.</summary>
    public WrappingCountdown HurtFrameTimer { get; internal set; }
    /// <summary>The head pose Kraid is showing, which selects the head image; null until the head program shows its first pose.</summary>
    internal KraidHeadFrame? DisplayedHeadFrame { get; set; }

    /// <summary>
    /// Where Kraid's head program resumes; its timer is the body's head countdown. Null until the first attack, glow
    /// or death installs a program.
    /// </summary>
    internal AnimationProgramCursor<KraidHeadProgram>? HeadProgram { get; set; }

    /// <summary>Moves the hurt-palette phase one step toward zero; the caller has established the phase is running.</summary>
    internal void StepHurtFrameDown() => HurtFrame = HurtFrame > 0
        ? HurtFrame - 1
        : throw new InvalidOperationException($"Stepping Kraid's hurt phase {HurtFrame} breaks its caller's running-phase guard.");

    /// <summary>
    /// Arms three mouth reopens. The cartridge merges the count bitwise into what remains, which from any count it
    /// reaches (zero to three) leaves three.
    /// </summary>
    internal void ArmMouthReopens() => PendingMouthReopens |= ArmedMouthReopens;

    /// <summary>
    /// Removes one pending mouth reopen. The count wraps from zero to two hundred fifty-five as the cartridge's count
    /// byte does; every caller reaches here only while a reopen is pending.
    /// </summary>
    internal void RemoveOneMouthReopen() => PendingMouthReopens = PendingMouthReopens == 0
        ? MaximumPendingMouthReopens
        : PendingMouthReopens - 1;

    /// <summary>Clears every mouth latch and the pending reopen count together, as the cartridge clears their shared word.</summary>
    internal void ClearMouthReactions()
    {
        MouthFlags = KraidMouthFlags.None;
        PendingMouthReopens = 0;
    }

    /// <summary>The reopen count a charged body hit arms.</summary>
    private const int ArmedMouthReopens = 3;

    /// <summary>The largest pending reopen count; one below zero wraps to it.</summary>
    private const int MaximumPendingMouthReopens = 255;

    /// <summary>
    /// How Kraid's body appears when a half of it is next shown (#1352, #1466): behind the room's sprites once his body is
    /// ready, in front of them once he has grown, hidden in a defeated room and once his death clears his layer. Rise,
    /// growth and death show each half only on their authored updates, so his shown body lags it.
    /// </summary>
    internal KraidBodyAppearance BodyAppearance { get; set; }

    /// <summary>The body frame Kraid's body layer shows (#1352).</summary>
    internal KraidBodyFrame ShownBody { get; set; } = KraidBodyFrame.Hidden;

    /// <summary>
    /// True while Kraid's body layer stands in for the room's background layer behind the level: his two-by-two-screen
    /// body layer from his room's load, and after his death the hidden layer for the rest of the visit (#1427).
    /// </summary>
    public bool ShowsBodyLayer { get; internal set; }

    /// <summary>
    /// Kraid's body is ready to show (<c>$A7:AAC6</c>): his body appears behind the room's sprites, which his installed
    /// body sheet pictures, and his body layer is shown from here (#1352, #1466).
    /// </summary>
    internal void InitializeBodyLayer()
    {
        BodyAppearance = KraidBodyAppearance.BehindSprites;
        ShowsBodyLayer = true;
    }

    /// <summary>Shows the upper half of his body as it now appears, covering any head drawn over it.</summary>
    internal void ShowUpperBody() => ShownBody = ShownBody with { Upper = BodyAppearance, Head = null };

    /// <summary>Shows the lower half of his body as it now appears.</summary>
    internal void ShowLowerBody() => ShownBody = ShownBody with { Lower = BodyAppearance };

    /// <summary>Draws head pose <paramref name="frame"/> over the upper half of his body.</summary>
    internal void ShowHead(KraidHeadFrame frame) => ShownBody = ShownBody with { Head = frame };

    /// <summary>
    /// Where Kraid's body layer is placed: the layer position shown at the screen's top-left, which his AI sets from the
    /// camera and his body's position. It is set at AI time and latched with the frame.
    /// </summary>
    public Position16<BackgroundPixels> BodyLayerPlacement { get; internal set; }

    /// <summary>Number of `$A7:C995` rock/quake spawn requests made by retail cadence.</summary>
    public int RiseRockSpawnRequestCount { get; internal set; }

    /// <summary>Rise-rock requests that acquired a real bank-$86 projectile slot.</summary>
    public int SpawnedRiseRockCount { get; internal set; }

    /// <summary>Retail `$BC0A` spat-rock requests issued while head tilemap three is active.</summary>
    public int SpitRockRequestCount { get; internal set; }

    /// <summary>Spat-rock requests that acquired a real bank-$86 projectile slot.</summary>
    public int SpawnedSpitRockCount { get; internal set; }

    /// <summary>Number of private `$AF94` roar opcodes consumed from the head stream.</summary>
    public int RoarRequestCount { get; internal set; }

    /// <summary>Gets the number of growth-sequence ceiling rocks that acquired a live bank-$86 projectile slot.</summary>
    public int CeilingRockSpawnCount { get; internal set; }

    /// <summary>Gets whether growth has relinquished the first-screen camera lock for the enlarged encounter.</summary>
    public bool CameraReleasedForSecondPhase { get; internal set; }
    /// <summary>Gets the sinking sound countdown, initially forty-three and reloaded to thirty after each request.</summary>
    public WrappingCountdown DeathSoundTimer { get; internal set; }
    /// <summary>Gets the number of authored sinking-table events consumed during the death descent.</summary>
    public int SinkTableEventCount { get; internal set; }
    /// <summary>Gets the number of pickup actors requested by Kraid's authored death scatter, independent of pool allocation success.</summary>
    public int DeathDropRequestCount { get; internal set; }
    /// <summary>Gets whether this encounter's death handoff newly wrote the persistent area-boss defeat bit.</summary>
    public bool BossDefeatPersisted { get; internal set; }
    /// <summary>Gets whether background restoration and the death music/persistence handoff have reached the terminal death entry.</summary>
    public bool DeathSequenceComplete { get; internal set; }
}
