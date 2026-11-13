using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Shared Kraid encounter state, plus the room-level effects whose
/// owners sit outside an individual enemy record. No host-only combat simplification
/// is represented here: thresholds and functions are copied from retail arithmetic.
/// </summary>
public sealed class KraidEnemyState
{
    /// <summary>Kraid's body's, sprite parts' and eyes' colours (#1512).</summary>
    internal KraidColours Colours { get; } = new();

    /// <summary>His body's own state. Like every part's, it remains owned by the room after the body is deleted.</summary>
    internal KraidBodyState BodyState { get; } = new();

    /// <summary>His arm's own state.</summary>
    internal KraidArmState ArmState { get; } = new();

    /// <summary>His top lint's own state.</summary>
    internal KraidLintState TopLintState { get; } = new();

    /// <summary>His middle lint's own state.</summary>
    internal KraidLintState MiddleLintState { get; } = new();

    /// <summary>His bottom lint's own state.</summary>
    internal KraidLintState BottomLintState { get; } = new();

    /// <summary>His foot's own state.</summary>
    internal KraidFootState FootState { get; } = new();

    /// <summary>His first fingernail's own state.</summary>
    internal KraidNailState FirstNailState { get; } = new();

    /// <summary>His second fingernail's own state.</summary>
    internal KraidNailState SecondNailState { get; } = new();

    /// <summary>The state of Kraid's part <paramref name="part"/>.</summary>
    internal KraidPartState Part(KraidPart part) => part switch
    {
        KraidPart.Kraid => BodyState,
        KraidPart.Arm => ArmState,
        KraidPart.LintTop => TopLintState,
        KraidPart.LintMiddle => MiddleLintState,
        KraidPart.LintBottom => BottomLintState,
        KraidPart.Foot => FootState,
        KraidPart.Nail => FirstNailState,
        KraidPart.NailBad => SecondNailState,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidPart)} {part}."),
    };

    /// <summary>Gets the body think countdown selected from the random-delay table and consumed before the next mouth attack.</summary>
    public int ThinkingTimer { get; internal set; }
    /// <summary>
    /// Gets the room position whose Y is the highest Samus may be pushed to when Kraid's body ejects her: Y 324 for the
    /// small form and 164 once growth raises the clamp. Only the Y axis is meaningful; Samus's position is compared with
    /// it by the wrapped spatial difference on Y.
    /// </summary>
    public Position16<RoomPixels> EjectionLimit { get; internal set; }
    /// <summary>Gets the mouth's shot latches: a charged hit this pass, an armed reopen and a pending reopen.</summary>
    public KraidMouthFlags MouthFlags { get; internal set; }
    /// <summary>
    /// Gets the number of mouth reopens still pending, from zero to three. A charged body hit arms three; each reopen
    /// continuation removes one, and the last removal clears the mouth with the count (#1627).
    /// </summary>
    public int PendingMouthReopens { get; internal set; }
    /// <summary>Health captured by the living initializer before combat can change it.</summary>
    public int InitialHealth { get; internal set; }
    /// <summary>Returns the game's threshold for the selected eighth of the initializer's captured health.</summary>
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
    public int HurtFrameTimer { get; internal set; }
    /// <summary>The head pose Kraid is showing, which selects the head image; null until the head animation shows its first pose.</summary>
    internal KraidHeadFrame? DisplayedHeadFrame { get; set; }

    /// <summary>
    /// Kraid's playing head animation and where it stands; its timer is the body's head countdown. Null until the first
    /// attack, glow or death installs an animation.
    /// </summary>
    internal AnimationPlayback<KraidHeadSegment>? HeadPlayback { get; set; }

    /// <summary>Moves the hurt-palette phase one step toward zero; the caller has established the phase is running.</summary>
    internal void StepHurtFrameDown() => HurtFrame = HurtFrame > 0
        ? HurtFrame - 1
        : throw new InvalidOperationException($"Stepping Kraid's hurt phase {HurtFrame} breaks its caller's running-phase guard.");

    /// <summary>
    /// Arms three mouth reopens. The game merges the count bitwise into what remains, which from any count it
    /// reaches (zero to three) leaves three.
    /// </summary>
    internal void ArmMouthReopens() => PendingMouthReopens |= ArmedMouthReopens;

    /// <summary>
    /// Removes one pending mouth reopen (#1627). Every caller reaches here only while the mouth's reopen is pending, which a
    /// mouth hit marks only after arming three, and the removal that leaves none clears the mouth, so a reopen is always
    /// pending here: the game's wrap of its count from zero is never reached, and removing from none breaks that
    /// order.
    /// </summary>
    /// <exception cref="InvalidOperationException">No reopen is pending.</exception>
    internal void RemoveOneMouthReopen() => PendingMouthReopens = PendingMouthReopens > 0
        ? PendingMouthReopens - 1
        : throw new InvalidOperationException("Removing a mouth reopen from none breaks Kraid's mouth-reopen order.");

    /// <summary>Clears every mouth latch and the pending reopen count together, as the game clears them at once.</summary>
    internal void ClearMouthReactions()
    {
        MouthFlags = KraidMouthFlags.None;
        PendingMouthReopens = 0;
    }

    /// <summary>The reopen count a charged body hit arms.</summary>
    private const int ArmedMouthReopens = 3;


    /// <summary>
    /// How Kraid's body appears (#1352, #1466, #1515): behind the room's sprites once his body is ready, in front of them
    /// once he has grown, hidden in a defeated room and once his death has sunk him. Once <see cref="BodyShown"/>, his
    /// whole body shows it at once, with his head at its current pose over it.
    /// </summary>
    internal KraidBodyAppearance BodyAppearance { get; set; }

    /// <summary>
    /// Whether his body shows on his body layer (#1515): from his rise through the floor on, or from the growth or death
    /// that shows it again in a room he was found in later.
    /// </summary>
    internal bool BodyShown { get; private set; }

    /// <summary>His body shows on his body layer from now on, as it appears.</summary>
    internal void ShowBody() => BodyShown = true;

    /// <summary>
    /// True while Kraid's body layer stands in for the room's background layer behind the level: his two-by-two-screen
    /// body layer from his room's load, and after his death the hidden layer for the rest of the visit (#1427).
    /// </summary>
    public bool ShowsBodyLayer { get; internal set; }

    /// <summary>
    /// Kraid's body is ready to show: his body appears behind the room's sprites, which his installed body sheet pictures,
    /// and his body layer is shown from here, his body on it once he rises (#1352, #1466).
    /// </summary>
    internal void InitializeBodyLayer()
    {
        BodyAppearance = KraidBodyAppearance.BehindSprites;
        ShowsBodyLayer = true;
    }

    /// <summary>
    /// Where Kraid's body lies in the room (#1617): the room pixel his body's parts' area's top-left corner lies on, which his
    /// AI sets from his body's position. It is set at AI time and latched with the frame.
    /// </summary>
    public Position16<RoomPixels> BodyOrigin { get; internal set; }

    /// <summary>Gets the sinking sound countdown, initially forty-three and reloaded to thirty after each request.</summary>
    public int DeathSoundTimer { get; internal set; }
}
