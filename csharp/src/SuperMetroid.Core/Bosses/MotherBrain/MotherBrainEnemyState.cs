using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Shared state behind Mother Brain's two enemies, the body and the brain.
/// Most of these values belong to neither enemy. A dedicated object therefore keeps both enemies' places in the room
/// separate while one owner serves the two.
/// </summary>
public sealed class MotherBrainEnemyState : SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing
{
    /// <inheritdoc />
    Position16<RoomPixels>? SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyPlacement => BodyPlacedAt;

    /// <inheritdoc />
    bool SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.BodyHidden => DeathHidesBodyLayer;

    /// <inheritdoc/>
    bool SuperMetroid.Core.Enemies.BossBodies.IBossBodyShowing.HidesBackgroundLayer => DeathHidesBodyLayer;

    /// <summary>Starts the shared encounter state with her body and her head (#2298).</summary>
    /// <param name="body">The enemy holding her body's place, which begins the encounter as it sets up.</param>
    /// <param name="head">Her head's place, which her room lists right after her body.</param>
    /// <param name="walk">The walk credit the encounter's setup creates for it.</param>
    /// <param name="bursts">The burst cycle the encounter's setup creates for it (#1872).</param>
    /// <param name="brain">The brain animation the encounter's setup creates for it (#2317).</param>
    internal MotherBrainEnemyState(RoomEnemyOccupant body, RoomEnemyPlace head, MotherBrainWalkCounter walk, MotherBrainBurstCycle bursts,
        MotherBrainBrainAnimation brain)
    {
        BodyOccupant = Ensure.NotNull(body);
        Head = Ensure.NotNull(head);
        Walk = Ensure.NotNull(walk);
        Bursts = Ensure.NotNull(bursts);
        Brain = Ensure.NotNull(brain);
    }

    /// <summary>
    /// The enemy holding her body's place (#2558), which owns the encounter: its definition's damage is her contact damage,
    /// which her body's, brain's and neck's touches and her laser deal.
    /// </summary>
    public RoomEnemyOccupant BodyOccupant { get; }

    /// <summary>Her body, which owns the encounter and runs first.</summary>
    public RoomEnemyPlace Body => BodyOccupant.Place;

    /// <summary>Her head, the brain, which her room lists right after her body: her body's setup begins the encounter with it (#2298).</summary>
    public RoomEnemyPlace Head { get; }

    /// <summary>The encounter stage that shot, bomb and footstep handling depend on; the glass phase at room entry.</summary>
    public MotherBrainForm Form { get; internal set; }

    /// <summary>The contact regions that currently hurt Samus: the brain alone at room entry, all three from the ascent.</summary>
    public MotherBrainHitboxParts HitboxesEnabled { get; internal set; }

    /// <summary>
    /// The stage her body is in before her late fight (#2208), which names the type of the phase it runs on its next AI turn;
    /// setting a stage's phase enters that stage. Her body's setup leaves it in the <see cref="MotherBrainBodyStage.Glass"/>
    /// stage. From her rainbow beam on her late fight runs her body in stages of its own
    /// (<see cref="MotherBrainLateFightState.Stage"/>), and this one stays at the second phase's fight it ended.
    /// </summary>
    public MotherBrainBodyStage Stage { get; private set; }

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.FakeDeathDescent"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainDescentPhase DescentPhase
    {
        get => Stage == MotherBrainBodyStage.FakeDeathDescent ? field : throw NotIn(MotherBrainBodyStage.FakeDeathDescent);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.FakeDeathDescent;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.FakeDeathAscent"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainAscentPhase AscentPhase
    {
        get => Stage == MotherBrainBodyStage.FakeDeathAscent ? field : throw NotIn(MotherBrainBodyStage.FakeDeathAscent);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.FakeDeathAscent;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.Phase2Fight"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainPhase2FightPhase Phase2FightPhase
    {
        get => Stage == MotherBrainBodyStage.Phase2Fight ? field : throw NotIn(MotherBrainBodyStage.Phase2Fight);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.Phase2Fight;
        }
    }

    /// <summary>The failure of reading a stage's phase while her body is in another.</summary>
    private InvalidOperationException NotIn(MotherBrainBodyStage stage) =>
        new($"Mother Brain's body is in its {Stage} stage, not {stage}.");

    /// <summary>
    /// The mural's row her fake death's ascent reveals next (#2211), from the top row when the reveal starts, two rows on
    /// each update of <see cref="MotherBrainAscentPhase.DrawMural"/>; none once every row is revealed.
    /// </summary>
    public ListStep<MotherBrainMuralRow>? NextMuralRow { get; internal set; } = MotherBrainMuralRow.First;
    /// <summary>The brain's independent action, selecting its custom head-only or head-and-neck draw setup.</summary>
    public MotherBrainBrainAction BrainAction { get; internal set; }

    /// <summary>
    /// The body phase's timer (<see cref="BossCountdown"/>): every timed body phase counts it down and moves on as
    /// it runs out.
    /// </summary>
    public BossCountdown PhaseTimer { get; } = new();

    /// <summary>
    /// Body-animation state written by private animation cues. The body AI reads this
    /// pose independently of the current frame, notably while waiting for the slow
    /// post-ascent uncrouch to publish <see cref="MotherBrainBodyPose.Standing"/>.
    /// </summary>
    public MotherBrainBodyPose Pose { get; internal set; }

    /// <summary>Her greying at her fake death playing: its colourings, each held for <see cref="MotherBrainWaitDefinitions.FakeDeathGreyStepUpdates"/>, and then it is over.</summary>
    public MotherBrainFadeProgress FakeDeathGreying { get; } = new(MotherBrainFadeDefinitions.FakeDeathToGrey);

    /// <summary>The wait before the next fake-death explosion (<see cref="MotherBrainWaitDefinitions.FakeDeathExplosionIntervalUpdates"/>).</summary>
    public BossCountdown FakeDeathExplosionTimer { get; } = new();
    /// <summary>
    /// Where the next fake-death explosion bursts (#2315): each burst takes the next of
    /// <see cref="MotherBrainTubeCollapseDefinitions.FakeDeathExplosionPositions"/>, the first after the last.
    /// </summary>
    public ListStep<Position16<RoomPixels>> NextFakeDeathExplosion { get; internal set; } = ListStep<Position16<RoomPixels>>.FirstOf(MotherBrainTubeCollapseDefinitions.FakeDeathExplosionPositions);

    /// <summary>
    /// Her room's flash playing (#641, #2259, #2404): the shared animation player looping its images, or null while
    /// no flash runs.
    /// </summary>
    internal AnimationPlayer<MotherBrainRoomFlashSegment, MotherBrainRoomFlashImage, NoAnimationCue>? RoomFlash { get; set; }

    /// <summary>Where the tube collapse stands, or null until her fake-death flash starts the collapse.</summary>
    internal MotherBrainTubeCollapseProgress? TubeCollapse { get; set; }
    /// <summary>Independent countdown used by selected tube spawn stages; a stage moves on as it runs out.</summary>
    public BossCountdown TubeCollapseTimer { get; } = new();


    /// <summary>The colours the encounter has loaded over its sprites, her body layer and her room (#1512).</summary>
    internal MotherBrainColours Colours { get; } = new();

    /// <summary>
    /// Her brain's own animation (#641, #1600, #2317), separate from the head place, which shows her brain's hit area; it
    /// plays at her brain's draw.
    /// </summary>
    public MotherBrainBrainAnimation Brain { get; }

    /// <summary>
    /// The body enemy's animation and where it stands (#641, #1600), or null before its first animation, while her body
    /// shows her brain's hit area (#2220).
    /// </summary>
    internal AnimationPlayback<MotherBrainBodySegment>? BodyPlayback { get; set; }

    /// <summary>
    /// Her brain's own shake (#2469), counted down at each draw that shows her brain, as her brain's animation plays there:
    /// the glass event's earthquake starts it for the earthquake's length, and the brain's shake cue, the Hyper Beam recoil
    /// and the drained beam's refresh for <see cref="MotherBrainFightDefinitions.BrainShakeUpdates"/>.
    /// </summary>
    internal MotherBrainShake BrainShake { get; } = new(MotherBrainCadenceDefinitions.BrainShakeStep);

    /// <summary>Shared flag that asks Mother Brain Rinkas and turrets to remove themselves.</summary>
    public bool DeleteTurretsAndRinkas { get; internal set; }

    /// <summary>
    /// The brain action her head's main last carried out (#2562), which decides what her own draw shows after the ordinary
    /// enemy queues: her brain, with her neck while the action draws it. Null until her head's first main, before which her
    /// own draw shows nothing. Her head's place is hidden from the ordinary enemy draw throughout.
    /// </summary>
    public MotherBrainBrainAction? BrainActionDrawn { get; internal set; }

    /// <summary>Whether her own draw shows her brain: once her head's main has run.</summary>
    public bool DrawBrain => BrainActionDrawn is not null;

    /// <summary>
    /// The rainbow image Mother Brain's body and sprites take the next time the rainbow attack's alternate-frame colour
    /// step fires (#2259), at its place in the cycle (#2470); the step moves it on, the first image after the last.
    /// </summary>
    public ListStep<MotherBrainRainbowImage> NextRainbowImage { get; internal set; } = MotherBrainRainbowImage.CycleStart;

    /// <summary>Whether her brain's draw appends her articulated neck joints: while her head's last main drew her neck.</summary>
    public bool DrawNeck => BrainActionDrawn == MotherBrainBrainAction.ShowsBrainOnNeck;

    /// <summary>
    /// Her neck (#2314): the lower joints hang from her body on the lower angle, the upper joints from the last lower joint
    /// on the upper angle. Neck drawing and collision use them, and the last joint places the head.
    /// </summary>
    public MotherBrainNeck Neck { get; } = new();

    /// <summary>Lower-neck angle; the neck steps add and subtract the angular speed with full-turn wraparound, and its whole step places the lower joints.</summary>
    public RotationAngle LowerNeckAngle { get; internal set; }
    /// <summary>Upper-neck angle; moved with full-turn wraparound like the lower angle, and its whole step places the upper joints.</summary>
    public RotationAngle UpperNeckAngle { get; internal set; }
    /// <summary>
    /// How far both neck angles turn each update the neck moves (<see cref="MotherBrainNeckTurnDefinitions"/>), forward or
    /// back as each angle's movement says.
    /// </summary>
    public RotationAngle NeckTurnPerUpdate { get; internal set; }
    /// <summary>Whether the neck's two angles turn; positions and custom drawing remain separate from this motion flag.</summary>
    public bool NeckMovementEnabled { get; internal set; }
    /// <summary>How the lower neck moves: not at all, bobbing down and up, or a one-way lower or raise; reaching a limit changes it.</summary>
    public MotherBrainNeckMovement LowerNeckMovement { get; internal set; }
    /// <summary>How the upper neck moves; it moves after the lower neck, so a raise targets the lower neck's newly updated angle.</summary>
    public MotherBrainNeckMovement UpperNeckMovement { get; internal set; }

    /// <summary>True while the floor clips Mother Brain's body as she rises from the fake death.</summary>
    public bool RisingFloorClipActive { get; internal set; }

    /// <summary>Where her body is placed in the room (#1949, #2031): not yet, with her, or at the room's corner once she is gone.</summary>
    public MotherBrainBodyPlacement BodyPlacement { get; internal set; }

    /// <summary>
    /// The room pixel her body's frames are placed from (#1949, #2031), or null before it is placed: her position while it
    /// follows her, so each of her body's pictures lies where its installed frame places it against her; the room's
    /// top-left corner once she is gone, where the escape warning written over the background lies.
    /// </summary>
    private Position16<RoomPixels>? BodyPlacedAt => BodyPlacements[BodyPlacement](this);

    /// <summary>Where each body placement places her body's frames from.</summary>
    private static IdentityTable<MotherBrainBodyPlacement, Func<MotherBrainEnemyState, Position16<RoomPixels>?>> BodyPlacements => field ??= new(new Dictionary<MotherBrainBodyPlacement, Func<MotherBrainEnemyState, Position16<RoomPixels>?>>
    {
        [MotherBrainBodyPlacement.FollowsBody] = static brain => brain.Body.Position.Whole,
        [MotherBrainBodyPlacement.AtRoomCorner] = static _ => Position16<RoomPixels>.Zero,
        [MotherBrainBodyPlacement.Unplaced] = static _ => null,
    });

    /// <summary>Whether her death flicker hides her body this frame, separate from the sprites' invisibility.</summary>
    public bool DeathHidesBodyLayer { get; internal set; }
    /// <summary>Live escape message owner installed by the escape setup.</summary>
    public EscapeTypewriterState? EscapeTypewriter { get; internal set; }

    /// <summary>
    /// Where her ascent's dust, her death explosions and the escape door's dust burst next (#1872): each effect continues
    /// the place the one before it left. The phase-three sequence attached to the encounter is built over it.
    /// </summary>
    public MotherBrainBurstCycle Bursts { get; }

    /// <summary>How far her return from her fake death's grey has come (#2251).</summary>
    public MotherBrainFadeProgress FromFakeDeathGrey { get; } = new(MotherBrainFadeDefinitions.FakeDeathFromGrey);

    /// <summary>How far her greying as the Baby drains her has come.</summary>
    public MotherBrainFadeProgress DrainedGreying { get; } = new(MotherBrainFadeDefinitions.DrainToGrey);

    /// <summary>How far her revival from that grey has come.</summary>
    public MotherBrainFadeProgress RevivalFromGrey { get; } = new(MotherBrainFadeDefinitions.RevivalFromGrey);

    /// <summary>How far her final death's fade of her body has come.</summary>
    public MotherBrainFadeProgress DeathBodyFade { get; } = new(MotherBrainFadeDefinitions.DeathBody);

    /// <summary>How far her corpse's greying has come.</summary>
    public MotherBrainFadeProgress CorpseGreying { get; } = new(MotherBrainFadeDefinitions.CorpseFade);

    /// <summary>Allows the head animation's mouth-drool generation, enabled during live combat and cleared by low-power/death transitions.</summary>
    public bool DroolGenerationEnabled { get; internal set; }
    /// <summary>Allows the head's small purple breath/dust producer, enabled after stretching and disabled for the drained corpse interval.</summary>
    public bool SmallPurpleBreathGenerationEnabled { get; internal set; }

    /// <summary>
    /// Set by the small breath's
    /// setup and cleared by its last animation step, so at most one
    /// small breath is alive.
    /// </summary>
    public bool SmallPurpleBreathActive { get; internal set; }

    /// <summary>
    /// The mouth point her next drool hangs from (#2315): each drool takes the next of
    /// <see cref="MotherBrainFightDefinitions.DroolMouthOffsets"/>, the first after the last.
    /// </summary>
    public ListStep<Offset16<RoomPixels>> NextDroolMouthPoint { get; internal set; } = ListStep<Offset16<RoomPixels>>.FirstOf(MotherBrainFightDefinitions.DroolMouthOffsets);

    /// <summary>The walk credit this encounter holds, over which an attached sequence is built.</summary>
    internal MotherBrainWalkCounter Walk { get; }

    /// <summary>Walk credit: standing turns earn it toward a forward step and the player's hits spend it.</summary>
    public MotherBrainWalkCredit WalkCounter
    {
        get => Walk.Credit;
        internal set => Walk.Credit = value;
    }

    /// <summary>
    /// The step of the current attack. Head animation runs independently while this advances from selection, through
    /// the attack's cooldown, and back to the ordinary thinking phase.
    /// </summary>
    public MotherBrainAttackPhase AttackPhase { get; internal set; }

    /// <summary>The cooldown after her second phase chooses an attack, which the attack's cooldown phase runs out.</summary>
    public BossCountdown AttackCooldown { get; } = new();

    /// <summary>
    /// Number of live Mother Brain bombs. Phase two refuses another bomb when this is at
    /// least one; <see cref="RecordBombSpawned"/> and <see cref="RecordBombRemoved"/> keep it.
    /// </summary>
    public int BombCounter { get; private set; }

    /// <summary>Counts a bomb the shared bomb setup has just spawned.</summary>
    internal void RecordBombSpawned() => BombCounter++;

    /// <summary>
    /// Uncounts a bomb that has just been deleted. Every deleted bomb was counted when it
    /// spawned, so the count is positive here; a zero count means a bomb was deleted twice.
    /// </summary>
    internal void RecordBombRemoved() => BombCounter = BombCounter > 0
        ? BombCounter - 1
        : throw new InvalidOperationException("A Mother Brain bomb was deleted while none was counted.");

    /// <summary>
    /// The body's target X, used while the bomb decision walks the body toward
    /// one of its bomb positions before entering its posture sequence.
    /// </summary>
    public int BodyTargetXPosition { get; internal set; }

    /// <summary>
    /// The step of the hand-beam attack. This is the ordinary phase-two red hand-beam attack, not the later rainbow
    /// beam used when the head's health reaches zero.
    /// </summary>
    public MotherBrainHandBeamPhase HandBeamPhase { get; internal set; }

    /// <summary>
    /// Where the hand beam's next projectile starts. Every fired child
    /// starts from this exact position, fraction of a pixel included, moves this position on by the aimed velocity,
    /// then receives its own randomized secondary velocity.
    /// </summary>
    /// <remarks>Charging starts it at the beam position, above her body, with no fraction of a pixel.</remarks>
    public FixedPosition<RoomPixels> HandBeamNextPosition { get; internal set; }
    /// <summary>The aimed velocity; moves the shared start position on before each child receives its independent randomized scatter velocity.</summary>
    public PixelVelocity<RoomPixels> HandBeamNextVelocity { get; internal set; }
    /// <summary>Aim angle, calculated from hand to Samus at charging setup and used as the children's randomized scatter-angle base.</summary>
    public TrigonometryAngle HandBeamNextAngle { get; internal set; }

    /// <summary>
    /// Her late fight (#2471): the rainbow, finish-off, Baby and phase-three phases, begun once phase-two health reaches
    /// zero (<see cref="BeginLateFight"/>), or null before then. It runs over this encounter, reading and writing its body,
    /// brain, phase, neck and flags in place (#2209), and keeps only what the late fight alone needs: the beam, the forced
    /// Samus movement, the Baby and her death's progress. Her body runs it from its rainbow-beam stage on, and her brain's
    /// cues act on it from then.
    /// </summary>
    public MotherBrainRainbowBeamAttackSequence? RainbowBeamSequence { get; private set; }

    /// <summary>
    /// Begins her late fight over this encounter, whose walk credit, bursts (#1872) and state it runs, and holds it from
    /// here.
    /// </summary>
    /// <returns>The late fight begun.</returns>
    internal MotherBrainRainbowBeamAttackSequence BeginLateFight() => RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence(this);

    /// <summary>
    /// The Baby's cries still to come: each onion ring that hits it adds one, and its later healing and idle phases take
    /// them. It is a count, not a flag: more than one ring can land before the Baby's next turn.
    /// </summary>
    internal int PendingBabyCryCount { get; set; }

    /// <summary>The rainbow beam's colour and cone as she fires it.</summary>
    public MotherBrainRainbowBeamEffect RainbowBeam { get; } = new();

    /// <summary>
    /// Advances her rainbow beam's effect for this update from her brain at <paramref name="head"/>: it shows while the
    /// late fight's attack holds the beam, aimed and as wide as the attack has it, stepping through
    /// <paramref name="colours"/>, and is hidden otherwise.
    /// </summary>
    internal void StepRainbowBeam(Position16<RoomPixels> head, MotherBrainRainbowBeamColours colours)
    {
        if (RainbowBeamSequence is { RainbowAttack.BeamActive: true } beam)
            RainbowBeam.Show(head, beam.RainbowBeamAngle, beam.RainbowAttack.AngularWidth, colours);
        else
            RainbowBeam.Hide();
    }


    /// <summary>
    /// Clamped aim angle, between the aim's lowest and highest angles, written by the head's aim step
    /// and consumed by the next onion-ring spawn.
    /// </summary>
    public TrigonometryAngle OnionRingsTargetAngle { get; internal set; }
}
