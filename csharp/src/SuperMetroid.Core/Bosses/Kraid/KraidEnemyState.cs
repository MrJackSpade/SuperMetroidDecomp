using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Shared Kraid encounter state, plus the room-level effects whose
/// owners sit outside any one enemy. No combat simplification
/// is represented here: thresholds and formulas are the game's own.
/// </summary>
public sealed class KraidEnemyState
{
    /// <summary>
    /// Begins an encounter whose parts each hold it from their setup (#2218): each part's state knows the place that joins
    /// <paramref name="parts"/>' reference to it, and each animated part plays the animation its setup starts (#2298).
    /// </summary>
    /// <param name="parts">Kraid's references to his parts in his room.</param>
    internal KraidEnemyState(KraidParts parts)
    {
        Ensure.NotNull(parts);
        BodyState = new(this, parts.BodyReference);
        ArmState = new(this, parts.ArmReference);
        TopLintState = new(this, parts.TopLintReference);
        MiddleLintState = new(this, parts.MiddleLintReference);
        BottomLintState = new(this, parts.BottomLintReference);
        FootState = new(this, parts.FootReference);
        FirstNailState = new(this, parts.FirstNailReference, sibling: parts.SecondNailReference);
        SecondNailState = new(this, parts.SecondNailReference, sibling: parts.FirstNailReference);
    }

    /// <summary>Kraid's body's, sprite parts' and eyes' colours (#1512).</summary>
    internal KraidColours Colours { get; } = new();

    /// <summary>His body's own state. Like every part's, it remains owned by the room after the body is deleted.</summary>
    internal KraidBodyState BodyState { get; }

    /// <summary>His arm's own state.</summary>
    internal KraidArmState ArmState { get; }

    /// <summary>His top lint's own state.</summary>
    internal KraidLintState TopLintState { get; }

    /// <summary>His middle lint's own state.</summary>
    internal KraidLintState MiddleLintState { get; }

    /// <summary>His bottom lint's own state.</summary>
    internal KraidLintState BottomLintState { get; }

    /// <summary>His foot's own state.</summary>
    internal KraidFootState FootState { get; }

    /// <summary>His first fingernail's own state.</summary>
    internal KraidNailState FirstNailState { get; }

    /// <summary>His second fingernail's own state.</summary>
    internal KraidNailState SecondNailState { get; }

    /// <summary>His lint <paramref name="part"/>'s own state.</summary>
    internal KraidLintState Lint(KraidLintPart part) => part switch
    {
        KraidLintPart.Top => TopLintState,
        KraidLintPart.Middle => MiddleLintState,
        KraidLintPart.Bottom => BottomLintState,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidLintPart)} {part}."),
    };

    /// <summary>His fingernail <paramref name="part"/>'s own state.</summary>
    internal KraidNailState Nail(KraidNailPart part) => part switch
    {
        KraidNailPart.First => FirstNailState,
        KraidNailPart.Second => SecondNailState,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidNailPart)} {part}."),
    };

    /// <summary>His three lints' own states, top first.</summary>
    internal IEnumerable<KraidLintState> Lints => [TopLintState, MiddleLintState, BottomLintState];

    /// <summary>His two fingernails' own states, first first.</summary>
    internal IEnumerable<KraidNailState> Nails => [FirstNailState, SecondNailState];

    /// <summary>Gets the updates his body thinks before its next mouth attack: a random number of 64-update steps.</summary>
    public int ThinkingTimer { get; internal set; }
    /// <summary>
    /// Gets the highest room Y Samus may be pushed to when Kraid's body ejects her: 324 for the small form and 164 once
    /// growth raises the clamp.
    /// </summary>
    public int EjectionLimitY { get; internal set; }
    /// <summary>
    /// Whether a charged shot hit his mouth or body this pass (#2400): his body's pass clears it before it tests, a charged
    /// beam into his mouth or a charged shot into his body sets it, and a body hit that starts his eye glow reads it.
    /// </summary>
    internal bool ChargedHitThisPass { get; set; }

    /// <summary>
    /// Whether his mouth's reopens are armed (#2400): a charged body hit that started his eye glow arms them, and only a
    /// clear of his mouth disarms them.
    /// </summary>
    internal bool MouthReopenArmed { get; set; }

    /// <summary>
    /// Whether his mouth reopens as it closes (#2400): a mouth hit while the reopens are armed marks it, and a closing mouth
    /// that finds it removes one pending reopen.
    /// </summary>
    internal bool MouthReopenPending { get; set; }

    /// <summary>
    /// Gets the number of mouth reopens still pending, from zero to three. A charged body hit arms three; each reopen
    /// continuation removes one, and the last removal clears the mouth with the count (#1627).
    /// </summary>
    public int PendingMouthReopens { get; internal set; }
    /// <summary>Health captured by the living setup before combat can change it.</summary>
    public int InitialHealth { get; internal set; }
    /// <summary><paramref name="eighths"/> eighths of the health Kraid initialized with, one eighth truncated first (#1890).</summary>
    /// <param name="eighths">How many eighths, one through eight.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eighths"/> is not one through eight.</exception>
    public int HealthEighths(int eighths) => KraidHealthThresholds.Eighths(InitialHealth, eighths);

    /// <summary>Seven eighths of Kraid's initial health: below it, the small Kraid grows (#1890).</summary>
    internal int SevenEighthsHealth => HealthEighths(7);

    /// <summary>Half of Kraid's initial health, as four eighths: below it, his arm slows (#1890).</summary>
    internal int HalfHealth => HealthEighths(4);
    /// <summary>
    /// Gets the room X the second-phase body walks to, from 208 to 384.
    /// </summary>
    public int WalkTargetX { get; internal set; }
    /// <summary>
    /// His hurt flash (#2400): the colours a mouth hit flashes him through, played one update at a time by the shared
    /// player. At rest it shows his health band's colours, as it does before his first mouth hit.
    /// </summary>
    internal AnimationPlayer<KraidHurtFlashSegment, KraidHurtFlashShade, NoAnimationCue> HurtFlash { get; private set; } =
        KraidHurtFlashAnimations.Resting();

    /// <summary>A mouth hit starts his hurt flash again from its beginning, whatever it showed.</summary>
    internal void StartHurtFlash() => HurtFlash = KraidHurtFlashAnimations.Starting();

    /// <summary>His health running out cuts his hurt flash short: it rests on his health band's colours from here.</summary>
    internal void SettleHurtFlash() => HurtFlash = KraidHurtFlashAnimations.Resting();

    /// <summary>The head pose Kraid is showing, which selects the head image; null until the head animation shows its first pose.</summary>
    internal KraidHeadFrame? DisplayedHeadFrame { get; set; }

    /// <summary>
    /// Kraid's playing head animation and where it stands; its timer is the body's head countdown. Null until the first
    /// attack, glow or death installs an animation.
    /// </summary>
    internal AnimationPlayback<KraidHeadSegment>? HeadPlayback { get; set; }

    /// <summary>
    /// Starts the head animation that starts at <paramref name="animation"/> (#2399): the head enters its opening pose without
    /// showing it, and the head timer holds that pose's duration.
    /// </summary>
    internal void StartHeadAnimation(KraidHeadSegment animation) => EnterHead(KraidHeadAnimations.Starting(animation));

    /// <summary>
    /// Selects the head animation that starts at <paramref name="animation"/> (#2399): the head stands past its opening pose,
    /// as starting it leaves it, while the head timer runs on as it is.
    /// </summary>
    internal void SelectHeadAnimation(KraidHeadSegment animation) => HeadPlayback = KraidHeadAnimations.Starting(animation).Playback;

    /// <summary>
    /// Resumes the roar as growth does from <paramref name="resume"/> (#2399): the head enters the resumed pose without
    /// showing it, and the head timer holds that pose's duration.
    /// </summary>
    internal void ResumeHeadRoar(KraidHeadGrowthResume resume) => EnterHead(KraidHeadAnimations.GrowthResume(resume));

    /// <summary>Stands the head past the pose <paramref name="entry"/> entered, its timer holding that pose's duration.</summary>
    private void EnterHead(KraidHeadEntry entry)
    {
        HeadPlayback = entry.Playback;
        BodyState.HeadTimer = entry.Delay;
    }

    /// <summary>Arms three mouth reopens, whatever number of them was still pending (#1890).</summary>
    internal void ArmMouthReopens() => PendingMouthReopens = ArmedMouthReopens;

    /// <summary>
    /// Removes one pending mouth reopen (#1627). Kraid removes one only while the mouth's reopen is pending, which a
    /// mouth hit marks only after arming three, and the removal that leaves none clears the mouth, so a reopen is always
    /// pending here: the game's wrap of its count from zero is never reached, and removing from none breaks that
    /// order.
    /// </summary>
    /// <exception cref="InvalidOperationException">No reopen is pending.</exception>
    internal void RemoveOneMouthReopen() => PendingMouthReopens = PendingMouthReopens > 0
        ? PendingMouthReopens - 1
        : throw new InvalidOperationException("Removing a mouth reopen from none breaks Kraid's mouth-reopen order.");

    /// <summary>Forgets every hit on his mouth and body that would reopen it, and the reopens still pending.</summary>
    internal void ClearMouthReactions()
    {
        ChargedHitThisPass = false;
        MouthReopenArmed = false;
        MouthReopenPending = false;
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

    /// <summary>Gets the sinking sound countdown, initially forty-three and reloaded to thirty after each request.</summary>
    public int DeathSoundTimer { get; internal set; }
}
