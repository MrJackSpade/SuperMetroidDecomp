using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's encounter state, which all of his parts share, and the room's effects that belong to no one part of him.
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

    /// <summary>
    /// His body's thinking before its next mouth attack (#2538): each thinking setup starts it for a random number of
    /// <see cref="KraidArenaDefinitions.ThinkingDelayStepUpdates"/>, and he roars on its last update. It holds once it has
    /// run out, so the thinking his grown roar returns him to never ends of itself, and a hurt roar returns him to it as
    /// it stood.
    /// </summary>
    public HeldBossCountdown ThinkingWait { get; } = new();
    /// <summary>
    /// Gets the highest room Y Samus may be pushed to when Kraid's body ejects her: 324 for the small form and 164 once
    /// growth raises the clamp.
    /// </summary>
    public int EjectionLimitY { get; internal set; }
    /// <summary>
    /// Whether his mouth's reopens are armed (#2400): a charged body hit that started his eye glow arms them, and only a
    /// clear of his mouth disarms them.
    /// </summary>
    internal bool MouthReopenArmed { get; set; }

    /// <summary>
    /// Whether his mouth reopens as it closes (#2400): a mouth hit while the reopens are armed marks it, and a closing mouth
    /// that finds it removes one pending reopen.
    /// </summary>
    internal bool MouthReopenMarked { get; set; }

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
    /// Kraid's playing head animation and where it stands; <see cref="HeadHold"/> paces it. He rises with his roar selected,
    /// the animation his rise's last update selects again for his first attack; nothing reads his head before then.
    /// </summary>
    internal AnimationPlayback<KraidHeadSegment> HeadPlayback { get; set; } = KraidHeadAnimations.Starting(KraidHeadSegment.Roar).Playback;

    /// <summary>
    /// His head's hold on the pose it shows (#2538): each pose the head shows or enters holds for its duration, and the head
    /// animation plays on to its next pose on the hold's last update. Once an animation has ended the hold has run out and
    /// holds, so the head keeps its pose until another animation starts. His rise and his closing mouth hold the pose
    /// without an animation playing (<see cref="HoldHeadUntilRise"/>, <see cref="HoldClosedMouth"/>).
    /// </summary>
    internal HeldBossCountdown HeadHold { get; } = new();

    /// <summary>
    /// Starts the head animation that starts at <paramref name="animation"/> (#2399): the head enters its opening pose without
    /// showing it, and holds it for that pose's duration.
    /// </summary>
    internal void StartHeadAnimation(KraidHeadSegment animation) => EnterHead(KraidHeadAnimations.Starting(animation));

    /// <summary>
    /// Selects the head animation that starts at <paramref name="animation"/> (#2399): the head stands past its opening pose,
    /// as starting it leaves it, while its hold runs on as it is.
    /// </summary>
    internal void SelectHeadAnimation(KraidHeadSegment animation) => HeadPlayback = KraidHeadAnimations.Starting(animation).Playback;

    /// <summary>
    /// Resumes the roar as growth does from <paramref name="resume"/> (#2399): the head enters the resumed pose without
    /// showing it, and holds it for that pose's duration.
    /// </summary>
    internal void ResumeHeadRoar(KraidHeadGrowthResume resume) => EnterHead(KraidHeadAnimations.GrowthResume(resume));

    /// <summary>Stands the head past the pose <paramref name="entry"/> entered, holding that pose for its duration.</summary>
    private void EnterHead(KraidHeadEntry entry)
    {
        HeadPlayback = entry.Playback;
        HeadHold.LastFor(entry.Delay);
    }

    /// <summary>Shows the head pose <paramref name="shown"/> his head animation reached, holding it for its duration.</summary>
    internal void ShowHeadPose(AnimationShown<KraidHeadSegment, KraidHeadFrame>.Frame shown)
    {
        HeadHold.LastFor(shown.Duration);
        // His body layer shows the pose over his body from here (#1515).
        DisplayedHeadFrame = shown.Shown;
    }

    /// <summary>His head holds its pose for <see cref="KraidArenaDefinitions.RiseHeadDelayUpdates"/> as he waits to rise.</summary>
    internal void HoldHeadUntilRise() => HeadHold.LastFor(KraidArenaDefinitions.RiseHeadDelayUpdates);

    /// <summary>
    /// His mouth has closed after an attack or a hurt roar: his head holds its pose for
    /// <see cref="KraidArenaDefinitions.MouthClosedHeadDelayUpdates"/>.
    /// </summary>
    internal void HoldClosedMouth() => HeadHold.LastFor(KraidArenaDefinitions.MouthClosedHeadDelayUpdates);

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
        MouthReopenArmed = false;
        MouthReopenMarked = false;
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
    /// True while the room shows Kraid's body layer behind the level: from his room's load until his sunk body is hidden,
    /// when the room's own background shows (#2573).
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
    /// His wait for his sinking body's next rumble (#2538): his sinking starts it for
    /// <see cref="KraidArenaDefinitions.SinkFirstSoundUpdates"/>, and each rumble, on its last update, starts it again for
    /// <see cref="KraidArenaDefinitions.SinkSoundIntervalUpdates"/>.
    /// </summary>
    public BossCountdown SinkRumbleWait { get; } = new();
}
