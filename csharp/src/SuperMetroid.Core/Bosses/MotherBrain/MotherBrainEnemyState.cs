using SuperMetroid.Core.Animation;
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

    /// <summary>Starts the shared encounter state with her body before her head is attached.</summary>
    /// <param name="body">Her body's place.</param>
    /// <param name="walk">The walk credit the encounter's setup creates for it.</param>
    /// <param name="bursts">The burst cycle the encounter's setup creates for it (#1872).</param>
    internal MotherBrainEnemyState(RoomEnemyPlace body, MotherBrainWalkCounter walk, MotherBrainBurstCycle bursts)
    {
        Body = body;
        Walk = Ensure.NotNull(walk);
        Bursts = Ensure.NotNull(bursts);
    }

    /// <summary>Her body, which owns the encounter and runs first.</summary>
    public RoomEnemyPlace Body { get; }

    /// <summary>Her head, the brain, attached by its initializer.</summary>
    public RoomEnemyPlace Head { get => field ?? throw new InvalidOperationException("Mother Brain's head place is not attached; head initialization attaches it."); internal set; }

    /// <summary>The encounter stage that shot, bomb and footstep handling branch on; the glass phase at room entry.</summary>
    public MotherBrainForm Form { get; internal set; }

    /// <summary>The contact regions that currently hurt Samus: the brain alone at room entry, all three from the ascent.</summary>
    public MotherBrainHitboxParts HitboxesEnabled { get; internal set; }

    /// <summary>
    /// The stage her body is in (#2208), which names the type of the phase it runs on its next AI turn; setting a stage's
    /// phase enters that stage. Her body's setup leaves it in the <see cref="MotherBrainBodyStage.Glass"/> stage.
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

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.RainbowBeam"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainRainbowPhase RainbowPhase
    {
        get => Stage == MotherBrainBodyStage.RainbowBeam ? field : throw NotIn(MotherBrainBodyStage.RainbowBeam);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.RainbowBeam;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.FinishingSamusOff"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainFinishOffPhase FinishOffPhase
    {
        get => Stage == MotherBrainBodyStage.FinishingSamusOff ? field : throw NotIn(MotherBrainBodyStage.FinishingSamusOff);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.FinishingSamusOff;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.DrainedByBaby"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainDrainedPhase DrainedPhase
    {
        get => Stage == MotherBrainBodyStage.DrainedByBaby ? field : throw NotIn(MotherBrainBodyStage.DrainedByBaby);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.DrainedByBaby;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.Reviving"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainRevivalPhase RevivalPhase
    {
        get => Stage == MotherBrainBodyStage.Reviving ? field : throw NotIn(MotherBrainBodyStage.Reviving);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.Reviving;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.Phase3Fight"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainPhase3FightPhase Phase3FightPhase
    {
        get => Stage == MotherBrainBodyStage.Phase3Fight ? field : throw NotIn(MotherBrainBodyStage.Phase3Fight);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.Phase3Fight;
        }
    }

    /// <summary>The phase of the <see cref="MotherBrainBodyStage.Dying"/> stage the body runs; setting it enters that stage.</summary>
    /// <exception cref="InvalidOperationException">The body is in another stage.</exception>
    public MotherBrainDeathPhase DeathPhase
    {
        get => Stage == MotherBrainBodyStage.Dying ? field : throw NotIn(MotherBrainBodyStage.Dying);
        internal set
        {
            field = value;
            Stage = MotherBrainBodyStage.Dying;
        }
    }

    /// <summary>The failure of reading a stage's phase while her body is in another.</summary>
    private InvalidOperationException NotIn(MotherBrainBodyStage stage) =>
        new($"Mother Brain's body is in its {Stage} stage, not {stage}.");

    /// <summary>
    /// How many of the mural's rows her fake death's ascent has drawn (#2211): zero when the reveal starts, two more each
    /// update of <see cref="MotherBrainAscentPhase.DrawMural"/>.
    /// </summary>
    public int MuralRowsDrawn { get; internal set; }
    /// <summary>The brain's independent action, selecting its custom head-only or head-and-neck draw setup.</summary>
    public MotherBrainBrainAction BrainAction { get; internal set; }

    /// <summary>
    /// The body phase's timer (<see cref="MotherBrainCountdown"/>): every timed body phase counts it down and moves on as
    /// it runs out.
    /// </summary>
    public MotherBrainCountdown PhaseTimer { get; } = new();

    /// <summary>
    /// Body-animation state written by private animation cues. The body AI reads this
    /// pose independently of the current frame, notably while waiting for the slow
    /// post-ascent uncrouch to publish <see cref="MotherBrainBodyPose.Standing"/>.
    /// </summary>
    public MotherBrainBodyPose Pose { get; internal set; }

    /// <summary>How far her greying at her fake death has come: its eight colourings, then the step that completes it.</summary>
    public MotherBrainFadeProgress FakeDeathGreying { get; } = new();

    /// <summary>Updates until the next fake-death explosion, on an eight-frame cadence.</summary>
    public MotherBrainCountdown FakeDeathExplosionTimer { get; } = new();
    /// <summary>Wrapping index 0 through 7 into authored fake-death explosion positions; decremented modulo eight whenever the explosion timer runs out.</summary>
    public int FakeDeathExplosionIndex { get; internal set; }

    /// <summary>
    /// The room flash's current frame (#641), or null while no flash runs. Its timer counts upward, unlike enemy
    /// animation frame holds, because the room flash's step compares elapsed frames with each duration.
    /// </summary>
    public int? RoomFlashFrame { get; internal set; }
    /// <summary>Elapsed-update count for the current room-flash frame, advanced upward by the room flash's step and reset when the frame changes; never above the frame's two-update duration.</summary>
    public int RoomFlashTimer { get; internal set; }

    /// <summary>The brain's tube-collapse step, or null until her fake-death flash starts the collapse.</summary>
    public MotherBrainTubeCollapseStep? TubeCollapseStep { get; internal set; }
    /// <summary>Independent countdown used by selected tube spawn stages; a stage moves on as it runs out.</summary>
    public MotherBrainCountdown TubeCollapseTimer { get; } = new();


    /// <summary>The colours the encounter has loaded over its sprites, her body layer and her room (#1512).</summary>
    internal MotherBrainColours Colours { get; } = new();

    /// <summary>
    /// The brain's own animation (#641, #1600), separate from the head place, which shows her brain's hit area: its place rests on the
    /// frame being shown, which its next beat is. Null means the brain is not live and not drawn. Only the animation
    /// setter and the draw-time runner write it.
    /// </summary>
    internal AnimationPlayback<MotherBrainHeadSegment>? BrainPlayback { get; set; }

    /// <summary>
    /// How long the brain's frame has shown. It counts draws up from one
    /// and the animation advances once it exceeds the frame's duration, so each frame shows for
    /// its duration plus one draw.
    /// </summary>
    public int BrainFrameTimer { get; internal set; }

    /// <summary>
    /// The body enemy's animation and where it stands (#641, #1600), or null before its first animation, while her body
    /// shows her brain's hit area (#2220).
    /// </summary>
    internal AnimationPlayback<MotherBrainBodySegment>? BodyPlayback { get; set; }

    /// <summary>
    /// How many more of her brain's draws shake it, counted down by each draw: the glass event's earthquake loads it, and
    /// the brain's shake cue, the Hyper Beam recoil and the drained beam's refresh load fifty.
    /// </summary>
    public int BrainMainShakeTimer { get; internal set; }

    /// <summary>Shared flag that asks Mother Brain Rinkas and turrets to remove themselves.</summary>
    public bool DeleteTurretsAndRinkas { get; internal set; }

    /// <summary>
    /// Set by the head main on frames where her brain is drawn after the ordinary enemy
    /// queues. It is deliberately rebuilt every frame rather than treated as
    /// ordinary visibility because both the body and the brain carry the hide-drawing property.
    /// </summary>
    public bool DrawBrain { get; internal set; }

    /// <summary>
    /// The rainbow frame Mother Brain's body and sprites take the next time the rainbow attack's alternate-frame colour
    /// step fires, from zero to one below <see cref="Artwork.MotherBrainRainbowColourFormat.RainbowFrameCount"/>; the
    /// step advances it and returns to the first frame after the last.
    /// </summary>
    public int NextRainbowFrame { get; internal set; }

    /// <summary>Whether her brain's draw appends five articulated neck joints.</summary>
    public bool DrawNeck { get; internal set; }

    /// <summary>
    /// Each neck joint's pixel distance from its anchor, indexed by joint (<see cref="MotherBrainNeckJointDefinitions"/>):
    /// the lower joints' from the shared body anchor, the upper joints' from the upper-neck anchor. The main tube's landing
    /// initializes them.
    /// </summary>
    public int[] NeckJointDistances { get; } = new int[MotherBrainNeckJointDefinitions.Count];

    /// <summary>
    /// The neck joints' wrapping world-pixel positions, indexed by joint (<see cref="MotherBrainNeckJointDefinitions"/>):
    /// the lower joints hang from the body on the lower angle, the upper joints from the upper-neck anchor on the upper
    /// angle. Neck drawing and collision use them, and the brain anchor places the head.
    /// </summary>
    public Position16<RoomPixels>[] NeckJoints { get; } = new Position16<RoomPixels>[MotherBrainNeckJointDefinitions.Count];

    /// <summary>Lower-neck angle; the neck steps add and subtract the angular speed with full-turn wraparound, and its whole step places joints 0 through 2.</summary>
    public RotationAngle LowerNeckAngle { get; internal set; }
    /// <summary>Upper-neck angle; moved with full-turn wraparound like the lower angle, and its whole step places joints 3 and 4.</summary>
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

    /// <summary>
    /// The updates her rise has spent readying her standing body so far, fewer than
    /// <see cref="MotherBrainReadyingDefinitions.StandingBodyDuration"/>. Zero both starts the readying and is the resting
    /// value after its last update, because readying that is not running starts from its first update.
    /// </summary>
    public int StandingBodyReadyingElapsed
    {
        get;
        internal set => field = (uint)value < MotherBrainReadyingDefinitions.StandingBodyDuration
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "Mother Brain's rise readies her standing body over twelve updates.");
    }

    /// <summary>True while the floor clips Mother Brain's body as she rises from the fake death.</summary>
    public bool RisingFloorClipActive { get; internal set; }

    /// <summary>Where her body is placed in the room (#1949, #2031): not yet, with her, or at the room's corner once she is gone.</summary>
    public MotherBrainBodyPlacement BodyPlacement { get; internal set; }

    /// <summary>
    /// The room pixel her body's frames are placed from (#1949, #2031), or null before it is placed: her position while it
    /// follows her, so each of her body's pictures lies where its installed frame places it against her; the room's
    /// top-left corner once she is gone, where the escape warning written over the background lies.
    /// </summary>
    public Position16<RoomPixels>? BodyPlacedAt => BodyPlacement switch
    {
        MotherBrainBodyPlacement.FollowsBody => Body.Position.Whole,
        MotherBrainBodyPlacement.AtRoomCorner => Position16<RoomPixels>.Zero,
        MotherBrainBodyPlacement.Unplaced => null,
        _ => throw new InvalidOperationException($"Undefined body placement {BodyPlacement}."),
    };

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
    public MotherBrainFadeProgress FromFakeDeathGrey { get; } = new();

    /// <summary>How far her greying as the Baby drains her has come.</summary>
    public MotherBrainFadeProgress DrainedGreying { get; } = new();

    /// <summary>How far her revival from that grey has come.</summary>
    public MotherBrainFadeProgress RevivalFromGrey { get; } = new();

    /// <summary>How far her final death's fade of her body has come.</summary>
    public MotherBrainFadeProgress DeathBodyFade { get; } = new();

    /// <summary>How far her corpse's greying has come.</summary>
    public MotherBrainFadeProgress CorpseGreying { get; } = new();

    /// <summary>Allows the head animation's mouth-drool generation, enabled during live combat and cleared by low-power/death transitions.</summary>
    public bool DroolGenerationEnabled { get; internal set; }
    /// <summary>Allows the head's small purple breath/dust producer, enabled after stretching and disabled for the drained corpse interval.</summary>
    public bool SmallPurpleBreathGenerationEnabled { get; internal set; }

    /// <summary>
    /// Set by the small breath's
    /// initializer and cleared by its last animation step, so at most one
    /// small breath is alive.
    /// </summary>
    public bool SmallPurpleBreathActive { get; internal set; }

    /// <summary>
    /// The place in <see cref="MotherBrainBehavior.MotherBrainDroolOffsets"/> of the mouth point the last drool hung from.
    /// Each drool takes the next of the six points, the first after the last, so a fresh encounter's first drool hangs
    /// from the second.
    /// </summary>
    public int LastDroolMouthPoint { get; internal set; }

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
    /// 64 cooldown frames, and back to the ordinary thinking phase.
    /// </summary>
    public MotherBrainAttackPhase AttackPhase { get; internal set; }

    /// <summary>The 64-frame attack cooldown the attack's cooldown phase counts down.</summary>
    public int AttackCooldown { get; internal set; }

    /// <summary>
    /// Number of live Mother Brain bombs. Phase two refuses another bomb when this is at
    /// least one; <see cref="RecordBombSpawned"/> and <see cref="RecordBombRemoved"/> keep it.
    /// </summary>
    public int BombCounter { get; internal set; }

    /// <summary>Counts a bomb the shared bomb initializer has just spawned.</summary>
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
    /// X=64 or 96 before entering its posture sequence.
    /// </summary>
    public int BodyTargetXPosition { get; internal set; }

    /// <summary>
    /// The step of the hand-beam attack. This is the ordinary phase-two red hand-beam attack, not the later rainbow
    /// beam used when the head's health reaches zero.
    /// </summary>
    public MotherBrainHandBeamPhase HandBeamPhase { get; internal set; }

    /// <summary>
    /// Shared cursor used by the hand-beam projectile chain. Every fired child
    /// starts from this exact 1/65536-pixel position, advances the cursor by the aimed velocity,
    /// then receives its own randomized secondary velocity.
    /// </summary>
    /// <remarks>Charging starts it at the beam position with zero subpixels (body Y minus 48).</remarks>
    public FixedPosition<RoomPixels> HandBeamNextPosition { get; internal set; }
    /// <summary>Signed 1/256-pixel aimed velocity; advances the shared cursor before each child receives its independent randomized scatter velocity.</summary>
    public EightEightVector<RoomPixels> HandBeamNextVelocity { get; internal set; }
    /// <summary>Aim angle, calculated from hand to Samus at charging setup and used as the children's randomized scatter-angle base.</summary>
    public TrigonometryAngle HandBeamNextAngle { get; internal set; }

    /// <summary>
    /// The rainbow, finish-off, Baby and phase-three phases, attached once phase-two health reaches zero. They run over
    /// this encounter, reading and writing its body, brain, phase, neck and flags in place (#2209), and keep only what the
    /// late fight alone needs: the beam, the forced Samus movement, the Baby's arrival and her death's progress.
    /// </summary>
    public MotherBrainRainbowBeamAttackSequence? RainbowBeamSequence
    {
        get;
        internal set
        {
            // An attached sequence runs this encounter: its walk credit, its bursts (#1872) and its state are this one's.
            if (value is not null && !ReferenceEquals(value.Encounter, this))
                throw new InvalidOperationException("A rainbow-beam sequence attached to Mother Brain's encounter is built over that encounter.");
            field = value;
        }
    }

    /// <summary>
    /// The Baby Metroid's place in the room's enemy list, which the final beam's Baby spawn fills: the Baby is an enemy of
    /// its own, which Mother Brain's attacks find there, not an effect her body draws.
    /// </summary>
    public RoomEnemyPlace? BabyMetroidPlace { get; internal set; }
    /// <summary>The Baby Metroid's cutscene at that place: on its own turn it latches on, drains, heals Samus and dies.</summary>
    public BabyMetroidCutsceneState? BabyMetroid { get; internal set; }


    /// <summary>
    /// Last Baby animation started on the Baby's enemy. That enemy's playback advances
    /// independently, so this latch changes only when Baby AI explicitly invokes the
    /// helper that starts its animation.
    /// </summary>
    internal MotherBrainBabySegment BabyAppliedAnimation { get; set; }

    /// <summary>The cutscene Baby's place animation and where it stands (#641, #1600).</summary>
    internal AnimationPlayback<MotherBrainBabySegment> BabyPlayback { get; set; } = MotherBrainBabyAnimations.Starting(default);

    /// <summary>
    /// The Baby's cries still to come: each onion ring that hits it adds one, and its later healing and idle phases take
    /// them. It is a count, not a flag: more than one ring can land before the Baby's next turn.
    /// </summary>
    internal int PendingBabyCryCount { get; set; }

    /// <summary>The rainbow beam's colour and cone, advanced by the simulation rather than by painting a frame.</summary>
    public MotherBrainRainbowBeamEffect RainbowBeam { get; } = new();

    /// <summary>
    /// Advances her rainbow beam's effect for this update from her brain at <paramref name="head"/>: it shows while the
    /// late fight's attack holds the beam, aimed and as wide as the attack has it, and is hidden otherwise.
    /// </summary>
    internal void StepRainbowBeam(Position16<RoomPixels> head)
    {
        if (RainbowBeamSequence is { RainbowAttack.BeamActive: true } beam)
            RainbowBeam.Show(head, beam.RainbowBeamAngle, beam.RainbowAttack.AngularWidth);
        else
            RainbowBeam.Hide();
    }


    /// <summary>
    /// Clamped aim angle, a sixteenth through nine thirty-seconds of a turn, written by the head's aim step
    /// and consumed by the next onion-ring spawn.
    /// </summary>
    public TrigonometryAngle OnionRingsTargetAngle { get; internal set; }
}
