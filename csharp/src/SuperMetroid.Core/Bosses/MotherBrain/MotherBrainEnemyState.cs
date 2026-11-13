using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Shared state behind Mother Brain's two retail enemy records, the body and the brain.
/// Most of these values belong to neither enemy. A dedicated object therefore keeps both enemies' places in the room
/// separate while one owner serves the two.
/// </summary>
public sealed class MotherBrainEnemyState
{
    /// <summary>Starts the shared encounter state with her body before her head is attached.</summary>
    internal MotherBrainEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Her body, which owns the encounter and runs first.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>Her head, the brain, attached by its initializer.</summary>
    public RoomEnemySlot Head { get => field ?? throw new InvalidOperationException("Mother Brain's head slot is not attached; head initialization attaches it."); internal set; }

    /// <summary>The encounter stage that shot, bomb and footstep handling branch on; the glass phase at room entry.</summary>
    public MotherBrainForm Form { get; internal set; }

    /// <summary>The contact regions that currently hurt Samus: the brain alone at room entry, all three from the ascent.</summary>
    public MotherBrainHitboxParts HitboxesEnabled { get; internal set; }

    /// <summary>The body record's function, selecting first-phase, resurrection, combat, or cutscene control for its next AI turn.</summary>
    public MotherBrainBodyFunction Function { get; internal set; }
    /// <summary>The brain record's independent function, selecting its custom head-only or head-and-neck draw setup.</summary>
    public MotherBrainBrainFunction BrainFunction { get; internal set; }

    /// <summary>
    /// The body function's timer. Every fake-death pause counts it down and moves on once it runs past zero;
    /// zero therefore expires immediately at -1.
    /// </summary>
    public int FunctionTimer { get; internal set; }

    /// <summary>
    /// Body-animation state written by private animation cues. The body AI reads this
    /// pose independently of the current frame, notably while waiting for the slow
    /// post-ascent uncrouch to publish <see cref="MotherBrainBodyPose.Standing"/>.
    /// </summary>
    public MotherBrainBodyPose Pose { get; internal set; }

    /// <summary>Zero-based count of fake-death grey palette steps taken, 0 through the eight-step table length plus one.</summary>
    public int GrayFadeIndex { get; internal set; }

    /// <summary>Updates until the next fake-death explosion, on an eight-frame cadence.</summary>
    public int FakeDeathExplosionTimer { get; internal set; }
    /// <summary>Wrapping index 0 through 7 into authored fake-death explosion positions; decremented modulo eight whenever the explosion timer underflows.</summary>
    public int FakeDeathExplosionIndex { get; internal set; }

    /// <summary>
    /// The room flash's current frame (#641), or null while no flash runs. Its timer counts upward, unlike enemy
    /// animation frame holds, because the room-flash handler compares elapsed frames with each duration.
    /// </summary>
    public int? RoomFlashFrame { get; internal set; }
    /// <summary>Elapsed-update count for the current room-flash frame, advanced upward by the room-flash handler and reset when the frame changes; never above the frame's two-update duration.</summary>
    public int RoomFlashTimer { get; internal set; }

    /// <summary>Head-record sub-dispatch and its independent underflow timer.</summary>
    public MotherBrainTubeCollapseFunction TubeCollapseFunction { get; internal set; }
    /// <summary>Independent countdown used by selected tube spawn stages; a stage expires once it runs past zero.</summary>
    public int TubeCollapseTimer { get; internal set; }


    /// <summary>The colours the encounter has loaded over its sprites, her body layer and her room (#1512).</summary>
    internal MotherBrainColours Colours { get; } = new();

    /// <summary>
    /// The brain's own animation (#641, #1600), separate from the head enemy's dummy animation: its place rests on the
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

    /// <summary>The body enemy's animation and where it stands (#641, #1600).</summary>
    internal AnimationPlayback<MotherBrainBodySegment> BodyPlayback { get; set; } =
        new(MotherBrainAnimations.BodyStartingWith(MotherBrainBodySegment.InitialDummy), AnimationPlace.StartOfAnimation);

    /// <summary>The head enemy's animation and where it stands; it keeps the initial dummy for life.</summary>
    internal AnimationPlayback<MotherBrainBodySegment> HeadSlotPlayback { get; set; } =
        new(MotherBrainAnimations.BodyStartingWith(MotherBrainBodySegment.InitialDummy), AnimationPlace.StartOfAnimation);

    /// <summary>Earthquake timer copied into the head-shake timer after the glass event.</summary>
    public int BrainMainShakeTimer { get; internal set; }

    /// <summary>Shared flag that asks Mother Brain Rinkas and turrets to remove themselves.</summary>
    public bool DeleteTurretsAndRinkas { get; internal set; }

    /// <summary>
    /// Set by the head main on frames where her brain is drawn after the ordinary enemy
    /// queues. It is deliberately rebuilt every frame rather than treated as
    /// ordinary visibility because both physical records carry the hide-drawing property.
    /// </summary>
    public bool DrawBrain { get; internal set; }

    /// <summary>
    /// The rainbow frame Mother Brain's body and sprites take the next time the rainbow attack's alternate-frame palette
    /// step fires, from zero to one below <see cref="Artwork.MotherBrainRainbowPaletteFormat.RainbowFrameCount"/>; the
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

    /// <summary>Lower-neck angle; the neck dispatchers add and subtract the angular speed with full-turn wraparound, and its table index places joints 0 through 2.</summary>
    public RotationAngle LowerNeckAngle { get; internal set; }
    /// <summary>Upper-neck angle; moved with full-turn wraparound like the lower angle, and its table index places joints 3 and 4.</summary>
    public RotationAngle UpperNeckAngle { get; internal set; }
    /// <summary>Neck angular speed in 8.8 angle units, added or subtracted by both neck motion dispatchers each enabled update; every producer writes a positive speed of at most 2304.</summary>
    public int NeckAngleDelta { get; internal set; }
    /// <summary>Whether articulated neck angle dispatch is enabled; positions and custom drawing remain separate from this motion flag.</summary>
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

    /// <summary>
    /// Where her phase-two body layer is placed: the layer position shown at the screen's top-left. Its vertical
    /// coordinate moves oppositely to the body's vertical movement, keeping the large body attached.
    /// </summary>
    public Position16<BackgroundPixels> BodyLayerPlacement { get; internal set; }
    /// <summary>Whether body AI has placed her body layer, which then shows in place of the room's background layer.</summary>
    public bool PlacesBodyLayer { get; internal set; }
    /// <summary>ADF41C's death flicker hiding her body layer this frame, separate from the sprites' invisibility.</summary>
    public bool DeathHidesBodyLayer { get; internal set; }
    /// <summary>One-shot latch for her death clearing her body.</summary>
    public bool DeathBodyLayerCleared { get; internal set; }
    /// <summary>Live escape message owner installed by the escape setup.</summary>
    public EscapeTypewriterState? EscapeTypewriter { get; internal set; }

    /// <summary>Four-frame cadence cursor for the eight ascent dust positions.</summary>
    public int BodySubFunctionTimer { get; internal set; }

    /// <summary>Zero-based colour step used while leaving fake-death grey.</summary>
    public int GrayTransitionCounter { get; internal set; }

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
    /// Attached-mouth offset selector used by the drool step. The game
    /// increments before spawning and wraps at six, so a zero-initialized encounter emits
    /// drool at mouth offset one first.
    /// </summary>
    public int DroolProjectileParameter { get; internal set; }

    /// <summary>The walk credit this encounter owns and an attached sequence shares.</summary>
    internal MotherBrainWalkCounter Walk { get; } = new();

    /// <summary>Walk credit: standing turns earn it toward a forward step and the player's hits spend it.</summary>
    public MotherBrainWalkCredit WalkCounter
    {
        get => Walk.Credit;
        internal set => Walk.Credit = value;
    }

    /// <summary>
    /// The step of the current attack. Head animation runs independently while this advances from selection, through
    /// 64 cooldown frames, and back to the ordinary thinking function.
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
    /// starts from this exact 16.16 position, advances the cursor by the aimed velocity,
    /// then receives its own randomized secondary velocity.
    /// </summary>
    /// <remarks>Charging starts it at the beam position with zero subpixels (body Y minus 48).</remarks>
    public FixedPosition<RoomPixels> HandBeamNextPosition { get; internal set; }
    /// <summary>Signed 8.8 aimed velocity; advances the shared cursor before each child receives its independent randomized scatter velocity.</summary>
    public EightEightVector<RoomPixels> HandBeamNextVelocity { get; internal set; }
    /// <summary>Aim angle, calculated from hand to Samus at charging setup and used as the children's randomized scatter-angle base.</summary>
    public TrigonometryAngle HandBeamNextAngle { get; internal set; }

    /// <summary>
    /// The already verified rainbow/finish-off/Baby/phase-three state machine, attached to
    /// the real body/head pair only after phase-two health reaches zero. Physical actor
    /// coordinates and animations remain owned by this room system; this object owns the long
    ///  function chain and forced-Samus calculations.
    /// </summary>
    public MotherBrainRainbowBeamAttackSequence? RainbowBeamSequence
    {
        get;
        internal set
        {
            // An attached sequence reads and writes the encounter's own walk credit.
            value?.ShareWalkCounter(Walk);
            field = value;
        }
    }

    /// <summary>
    /// The dynamically allocated physical Baby Metroid enemy record created by
    /// the final beam's Baby spawn. Keeping the slot and its extended cutscene state side by side makes
    /// the cross-enemy index observable without pretending the Baby is a draw-only
    /// effect owned by Mother Brain's body.
    /// </summary>
    public RoomEnemySlot? BabyMetroidSlot { get; internal set; }
    /// <summary>Extended cutscene AI state attached to the dynamically allocated baby slot; its independent turn owns latch, draining, healing, and death handshakes.</summary>
    public BabyMetroidCutsceneState? BabyMetroid { get; internal set; }


    /// <summary>
    /// Last Baby animation started on the Baby's enemy. That enemy's playback advances
    /// independently, so this latch changes only when Baby AI explicitly invokes the
    /// helper that starts its animation.
    /// </summary>
    internal MotherBrainBabySegment BabyAppliedAnimation { get; set; }

    /// <summary>The cutscene Baby's slot animation and where it stands (#641, #1600).</summary>
    internal AnimationPlayback<MotherBrainBabySegment> BabyPlayback { get; set; } = MotherBrainBabyAnimations.Starting(default);

    /// <summary>
    /// Shared cry counter incremented by onion-ring collision and consumed by the
    /// Baby's later healing/idle functions. It is deliberately a count, not a boolean:
    /// <c>INC</c> permits more than one ring to land before the next Baby turn.
    /// </summary>
    internal int PendingBabyCryCount { get; set; }

    /// <summary>Whether body AI currently shows the rainbow beam.</summary>
    public bool RainbowBeamActive { get; internal set; }
    /// <summary>The rainbow beam's colour and cone, advanced by the simulation rather than by painting a frame.</summary>
    public MotherBrainRainbowBeamEffect RainbowBeam { get; } = new();
    /// <summary>Current angular aim published from the beam's forced-movement owner and consumed by the rainbow beam effect.</summary>
    public RotationAngle RainbowBeamAngle { get; internal set; }
    /// <summary>Rainbow-beam angular width, published by body AI as the beam widens or narrows; independent of pixel width at a given screen row.</summary>
    public RotationAngle RainbowBeamAngularWidth { get; internal set; }

    /// <summary>
    /// Last brain animation copied from the rainbow state machine into the brain.
    /// The brain cursor advances away from the animation start, so comparing against it
    /// would reinstall the animation and reset its timer every frame. This separate
    /// producer-owned latch mirrors the fact that the brain-animation request runs only when body AI
    /// explicitly requests a different brain animation.
    /// </summary>
    internal MotherBrainHeadSegment? RainbowAppliedHeadAnimation { get; set; }

    /// <summary>
    /// Last <see cref="MotherBrainLateFightState.SmallPurpleBreathGenerationWriteCount"/>
    /// published to <see cref="SmallPurpleBreathGenerationEnabled"/>, so a brain-animation clear
    /// survives until the body next writes the flag.
    /// </summary>
    internal uint RainbowAppliedSmallPurpleBreathWrites { get; set; }


    /// <summary>
    /// Clamped aim angle, a sixteenth through nine thirty-seconds of a turn, written by the head's aim step
    /// and consumed by the next onion-ring spawn.
    /// </summary>
    public TrigonometryAngle OnionRingsTargetAngle { get; internal set; }
}
