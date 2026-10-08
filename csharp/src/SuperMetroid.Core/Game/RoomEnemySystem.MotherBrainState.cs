namespace SuperMetroid.Core.Game;


/// <summary>
/// Index into Mother Brain's three-entry phase-two attack dispatcher at
/// <c>$A9:B654</c>. The values are array indices in the cartridge, not host-only states.
/// </summary>
public enum MotherBrainAttackPhase : ushort
{
    /// <summary>Index 0 selects the next head/body attack using Samus movement and sampled RNG, then seeds the 64-update cooldown.</summary>
    ChooseAttack = 0,
    /// <summary>Index 1 decrements the unsigned attack cooldown until zero while the chosen head bytecode runs independently.</summary>
    Cooldown = 1,
    /// <summary>Index 2 resets the attack sub-dispatcher and returns the body to ordinary phase-two thinking.</summary>
    EndAttack = 2,
}

/// <summary>
/// Index into Mother Brain's four-entry hand-beam dispatcher at <c>$A9:B887</c>. Phase two
/// is deliberately bytecode-owned: the body list advances it only after its complete
/// charge/fire/hold animation has elapsed.
/// </summary>
public enum MotherBrainHandBeamPhase : ushort
{
    /// <summary>Index 0 requests the slow backward gait toward X $28, then sets the charging neck targets.</summary>
    BackUp = 0,
    /// <summary>Index 1 waits for every active Mother Brain bomb to leave the shared projectile pool before installing the hand-beam body list.</summary>
    WaitForBombs = 1,
    /// <summary>Index 2 is an inert body-AI wait; body bytecode owns charging, recursive projectile emission, the 240-update hold, and phase advance.</summary>
    Firing = 2,
    /// <summary>Index 3 restores the neutral head list and neck targets, resets this dispatcher, and returns to thinking.</summary>
    Finish = 3,
}

/// <summary>Values written by Mother Brain's body instruction opcodes at $A9:9700-$972F.</summary>
public enum MotherBrainBodyPose : ushort
{
    /// <summary>Native pose 0, written by $A9:9700; standing allows posture changes and ordinary attack/walking decisions.</summary>
    Standing = 0,
    /// <summary>Native pose 1, written by $A9:9708 while authored walking bytecode owns the body's displacement.</summary>
    Walking = 1,
    /// <summary>Native pose 2, written by $A9:9718 during crouch/uncrouch transitions; posture helpers wait for bytecode to publish an endpoint.</summary>
    CrouchingTransition = 2,
    /// <summary>Native pose 3, written by $A9:9710 at the crouched endpoint; the standing helper selects the matching uncrouch list.</summary>
    Crouched = 3,
    /// <summary>Native pose 4, written by $A9:9720 for the ordinary red hand-beam body animation.</summary>
    DeathBeam = 4,
    /// <summary>Native pose 6, written by $A9:9728 at the leaning-down endpoint; standing requires its distinct recovery list.</summary>
    LeaningDown = 6,
}

/// <summary>
/// Native function pointers stored in the head record's secondary Mother Brain dispatcher.
/// During fake death this independent state machine serializes the physical tube actors,
/// ceiling projectiles, and one-frame bank-$84 room mutations.
/// </summary>
public enum MotherBrainTubeCollapseFunction : ushort
{
    /// <summary><c>$A9:8949 Function_MotherBrainBody_SpawnTubesFallingWhenLessThan4Proj</c>: admits collapse when four projectile slots are free, then requests physical bottom-left tube enemy 0.</summary>
    WaitForFourFreeProjectileSlots = 0x8949,
    /// <summary><c>$A9:896E Function_MotherBrainBody_ClearBottomLeftTube</c>: requests the bank-$84 removal at block (5,9) and seeds the 32-count top-right delay.</summary>
    ClearBottomLeftTube = 0x896e,
    /// <summary><c>$A9:8983 Function_MotherBrainBody_SpawnTopRightTubeFallingProjectile</c>: after timer underflow requests the ceiling projectile at pixel (152,47).</summary>
    SpawnTopRightTube = 0x8983,
    /// <summary><c>$A9:89A0 Function_MotherBrainBody_ClearCeilingBlockColumn9</c>: clears ceiling block (9,2) and seeds the next 32-count tube delay.</summary>
    ClearCeilingColumn9 = 0x89a0,
    /// <summary><c>$A9:89B5 Function_MotherBrainBody_SpawnTopLeftTubeFallingProjectile</c>: after timer underflow requests the ceiling projectile at pixel (104,47).</summary>
    SpawnTopLeftTube = 0x89b5,
    /// <summary><c>$A9:89D2 Function_MotherBrainBody_ClearCeilingBlockColumn6</c>: clears ceiling block (6,2), selects the next physical tube, and seeds its shared timer.</summary>
    ClearCeilingColumn6 = 0x89d2,
    /// <summary><c>$A9:89E7 Function_MotherBrainBody_SpawnTubesFalling1</c>: requests physical bottom-right tube enemy 1, independently of the ceiling-projectile pool.</summary>
    SpawnBottomRightTube = 0x89e7,
    /// <summary><c>$A9:89FA Function_MotherBrainBody_ClearBottomRightTube</c>: requests bottom-right removal at block (10,9) and selects physical tube enemy 2.</summary>
    ClearBottomRightTube = 0x89fa,
    /// <summary><c>$A9:8A0F Function_MotherBrainBody_SpawnTubesFalling2</c>: requests physical bottom-middle-left tube enemy 2.</summary>
    SpawnBottomMiddleLeftTube = 0x8a0f,
    /// <summary><c>$A9:8A22 Function_MotherBrainBody_ClearBottomMiddleLeftTube</c>: removes the side tube at block (6,10) and starts the 32-count upper-middle-left delay.</summary>
    ClearBottomMiddleLeftTube = 0x8a22,
    /// <summary><c>$A9:8A37 Function_MotherBrainBody_SpawnTopMiddleLeftTubeFallingProj</c>: after timer underflow requests the ceiling projectile at pixel (120,59).</summary>
    SpawnTopMiddleLeftTube = 0x8a37,
    /// <summary><c>$A9:8A54 Function_MotherBrainBody_ClearCeilingTubeColumn7</c>: clears ceiling tube (7,2) and seeds the 32-count upper-middle-right delay.</summary>
    ClearCeilingColumn7 = 0x8a54,
    /// <summary><c>$A9:8A69 Function_MotherBrainBody_SpawnTopMiddleRightTubeFallingProj</c>: after timer underflow requests the ceiling projectile at pixel (136,59).</summary>
    SpawnTopMiddleRightTube = 0x8a69,
    /// <summary><c>$A9:8A86 Function_MotherBrainBody_ClearCeilingTubeColumn8</c>: clears ceiling tube (8,2) and selects physical tube enemy 3.</summary>
    ClearCeilingColumn8 = 0x8a86,
    /// <summary><c>$A9:8A9B Function_MotherBrainBody_SpawnTubesFalling3</c>: requests physical bottom-middle-right tube enemy 3.</summary>
    SpawnBottomMiddleRightTube = 0x8a9b,
    /// <summary><c>$A9:8AAE Function_MotherBrainBody_ClearBottomMiddleRightTube</c>: removes the side tube at block (9,10) and seeds the two-count main-tube delay.</summary>
    ClearBottomMiddleRightTube = 0x8aae,
    /// <summary><c>$A9:8AC3 Function_MotherBrainBody_SpawnTubesFalling4</c>: after timer underflow requests the main falling-tube enemy whose descent carries the head into ascent setup.</summary>
    SpawnMainTube = 0x8ac3,
    /// <summary><c>$A9:8AD6 Function_MotherBrainBody_ClearBottomMiddleTubes</c>: removes the remaining tubes at block (7,7) and installs its own RTS as the terminal subfunction.</summary>
    ClearBottomMiddleTubes = 0x8ad6,
    /// <summary><c>$A9:8AE4 Function_MotherBrainBody_ClearBottomMiddleTubes.return</c>: inert tube-subfunction endpoint while physical falling actors continue independently.</summary>
    Finished = 0x8ae4,
}

/// <summary>Native function pointers used by Mother Brain's separate brain record.</summary>
public enum MotherBrainBrainFunction : ushort
{
    /// <summary><c>$A9:87A2 Function_MotherBrain_SetupBrainAndNeckToBeDrawn</c>: advances neck geometry unless time is frozen, attaches the head to joint 4, and installs the combined draw hook.</summary>
    SetupBrainAndNeckToBeDrawn = 0x87a2,
    /// <summary><c>$A9:87D0 Function_MotherBrain_SetupBrainToBeDrawn</c>: installs the head-only custom draw hook without articulated neck drawing.</summary>
    SetupBrainToBeDrawn = 0x87d0,
}

/// <summary>
/// Shared state behind retail enemy records <c>$EC7F</c> (body) and <c>$EC3F</c> (brain).
/// The SNES stores most of these words in named extended WRAM rather than in either common
/// <c>$40</c>-byte enemy slot. A dedicated object therefore preserves both the physical slot
/// boundary and the original one-owner/two-record relationship.
/// </summary>
public sealed class MotherBrainEnemyState
{
    internal MotherBrainEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Physical slot zero, enemy definition <c>$EC7F</c>.</summary>
    public RoomEnemySlot Body { get; }

    /// <summary>Physical slot one, enemy definition <c>$EC3F</c>.</summary>
    public RoomEnemySlot? Head { get; internal set; }

    /// <summary>
    /// Exact <c>$7E:9000/$7E:9700</c> corpse graphics and rot-table producer initialized by
    /// the head record even though it is not consumed until the much later death sequence.
    /// </summary>
    public MotherBrainCorpseRottingState CorpseRotting { get; } = new();

    /// <summary>Mother Brain form word: zero is glass/first phase, one begins fake death.</summary>
    public ushort Form { get; internal set; }

    /// <summary>Native shared hitbox-enable word, initialized to two.</summary>
    public ushort HitboxesEnabled { get; internal set; }

    /// <summary>The body record's bank-$A9 function pointer, selecting first-phase, resurrection, combat, or cutscene control for its next AI turn.</summary>
    public MotherBrainBodyFunction Function { get; internal set; }
    /// <summary>The brain record's independent bank-$A9 function pointer selecting its custom head-only or head-and-neck draw setup.</summary>
    public MotherBrainBrainFunction BrainFunction { get; internal set; }

    /// <summary>
    /// Native <c>mbn_var_F</c>. Every fake-death pause decrements this unsigned word and
    /// branches when bit 15 becomes set; zero therefore expires immediately to $FFFF.
    /// </summary>
    public ushort FunctionTimer { get; internal set; }

    /// <summary>
    /// Body-animation state written by private instruction opcodes. The body AI reads this
    /// word independently of the current spritemap, notably while waiting for the slow
    /// post-ascent uncrouch to publish <see cref="MotherBrainBodyPose.Standing"/>.
    /// </summary>
    public MotherBrainBodyPose Pose { get; internal set; }

    /// <summary>Zero-based palette-step count stored in native <c>mbn_var_37</c>.</summary>
    public ushort GrayFadeIndex { get; internal set; }

    /// <summary>Eight-frame cadence and wrapping coordinate cursor for fake-death dust.</summary>
    public ushort FakeDeathExplosionTimer { get; internal set; }
    /// <summary>Wrapping index 0 through 7 into authored fake-death explosion positions; decremented modulo eight whenever the explosion timer underflows.</summary>
    public ushort FakeDeathExplosionIndex { get; internal set; }

    /// <summary>
    /// Bank-$A9 room-palette bytecode pointer/timer. The timer counts upward, unlike enemy
    /// instruction timers, because handler $D192 compares elapsed frames with each duration.
    /// </summary>
    public ushort RoomPaletteInstructionPointer { get; internal set; }
    /// <summary>Elapsed-update count for the active room-palette timed entry, advanced upward by $A9:D192 and reset when its pointer changes.</summary>
    public ushort RoomPaletteInstructionTimer { get; internal set; }

    /// <summary>Head-record sub-dispatch and its independent underflow timer.</summary>
    public MotherBrainTubeCollapseFunction TubeCollapseFunction { get; internal set; }
    /// <summary>Independent wrapping word countdown used by selected tube spawn stages; expiration occurs after decrement sets the sign bit.</summary>
    public ushort TubeCollapseTimer { get; internal set; }

    /// <summary>Exact delayed music writes made during the most recent enemy frame.</summary>
    public IReadOnlyList<MotherBrainMusicRequest> MusicRequests => _musicRequests;

    /// <summary>Hardcoded bank-$84 objects requested during the most recent enemy frame.</summary>
    public IReadOnlyList<MotherBrainPlmRequest> PlmRequests => _plmRequests;

    /// <summary>Last library-two fake-death/tube sound emitted during this enemy frame.</summary>
    public ushort? LastSoundEffect { get; internal set; }

    /// <summary>Last library-one rainbow-beam sound emitted during this enemy frame.</summary>
    public ushort? LastSoundEffectLibrary1 { get; internal set; }

    /// <summary>Last library-three sound emitted by a Mother Brain private opcode.</summary>
    public ushort? LastSoundEffectLibrary3 { get; internal set; }

    /// <summary>Count of dynamically spawned physical falling-tube enemy records.</summary>
    public int SpawnedFallingTubeCount { get; internal set; }

    /// <summary>Pending nonzero FX entry from initialization/descent; the runtime consumes it once, then clears it.</summary>
    public ushort FxEntry { get; internal set; }

    /// <summary>Whether the unpause hook must restore Mother Brain's BG2 image and beam SFX.</summary>
    public bool EnableUnpauseHook { get; internal set; }

    /// <summary>True after all <c>$800</c> enemy-BG2 words have been filled with tile <c>$0338</c>.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>Palette selector shared by the four neck segments.</summary>
    public ushort NeckPaletteIndex { get; internal set; }

    /// <summary>Palette selector applied by the custom brain drawing routine.</summary>
    public ushort BrainPaletteIndex { get; internal set; }

    /// <summary>Countdown used by the normal head-palette setup routine, initially ten.</summary>
    public ushort BrainPaletteTimer { get; internal set; }

    /// <summary>
    /// <c>MotherBrainBody.brainInstListPointer</c> ($7E:8002): the brain's own bank-$A9
    /// instruction list, separate from the head enemy's dummy list. Bit 15 set means live;
    /// otherwise the brain is not drawn. Only <c>$A9:C447</c> and the draw-time processor
    /// (<c>$A9:92AF</c>) write it.
    /// </summary>
    public ushort BrainInstructionPointer { get; internal set; }

    /// <summary>
    /// <c>MotherBrainBody.brainInstructionTimer</c> ($7E:8000). It counts draws up from one
    /// and the list advances once it exceeds the entry's duration, so each frame shows for
    /// its duration plus one draw.
    /// </summary>
    public ushort BrainInstructionTimer { get; internal set; }

    /// <summary>Earthquake timer copied into the head-shake word after the glass event.</summary>
    public ushort BrainMainShakeTimer { get; internal set; }

    /// <summary>Shared flag that asks Mother Brain Rinkas and turrets to remove themselves.</summary>
    public bool DeleteTurretsAndRinkas { get; internal set; }

    /// <summary>
    /// Set by the head main on frames where the cartridge's enemy-graphics-drawn hook points
    /// at <c>$A9:87DD</c>. It is deliberately rebuilt every frame rather than treated as
    /// ordinary visibility because both physical records carry property bit <c>$0100</c>.
    /// </summary>
    public bool DrawBrain { get; internal set; }

    /// <summary>Byte cursor at $7E:7842 into Mother Brain's native rainbow palette list.</summary>
    public ushort RainbowPaletteCursor { get; internal set; }

    /// <summary>Whether the active bank-$A9 draw hook appends five articulated neck joints.</summary>
    public bool DrawNeck { get; internal set; }

    /// <summary>Native fake-ascent neck lengths installed by <c>$A9:903F</c>.</summary>
    public ushort NeckSegment0Distance { get; internal set; }
    /// <summary>Lower-neck joint 1's pixel distance from the shared body anchor, initialized to ten by $A9:903F.</summary>
    public ushort NeckSegment1Distance { get; internal set; }
    /// <summary>Lower-neck joint 2's pixel distance from the shared body anchor, initialized to twenty; joint 2 also anchors the upper half.</summary>
    public ushort NeckSegment2Distance { get; internal set; }
    /// <summary>Upper-neck joint 3's pixel distance from joint 2, initialized to ten by $A9:903F.</summary>
    public ushort NeckSegment3Distance { get; internal set; }
    /// <summary>Upper-neck joint 4's pixel distance from joint 2, initialized to twenty; its resulting position anchors the brain.</summary>
    public ushort NeckSegment4Distance { get; internal set; }

    /// <summary>Five current world-space neck joints; segment four anchors the brain.</summary>
    public MotherBrainNeckPoint NeckSegment0 { get; internal set; }
    /// <summary>Joint 1's wrapping world-pixel position, calculated with the lower angle and its distance; used by neck drawing and collision.</summary>
    public MotherBrainNeckPoint NeckSegment1 { get; internal set; }
    /// <summary>Joint 2's wrapping world-pixel position, shared by lower-neck drawing/collision and upper-neck geometry as its anchor.</summary>
    public MotherBrainNeckPoint NeckSegment2 { get; internal set; }
    /// <summary>Joint 3's wrapping world-pixel position relative to joint 2, calculated with the upper angle and used by neck drawing/collision.</summary>
    public MotherBrainNeckPoint NeckSegment3 { get; internal set; }
    /// <summary>Joint 4's wrapping world-pixel position relative to joint 2; the articulated head follows its X and its Y minus twenty-one pixels.</summary>
    public MotherBrainNeckPoint NeckSegment4 { get; internal set; }

    /// <summary>8.8-style angle words and dispatch indices used by both neck halves.</summary>
    public ushort LowerNeckAngle { get; internal set; }
    /// <summary>Upper-neck angle in native 8.8 angle units; its high byte selects the 256-entry trigonometric cycle for joints 3 and 4.</summary>
    public ushort UpperNeckAngle { get; internal set; }
    /// <summary>Wrapping 8.8 angle step added or subtracted by both neck motion dispatchers each enabled update.</summary>
    public ushort NeckAngleDelta { get; internal set; }
    /// <summary>Whether articulated neck angle dispatch is enabled; positions and custom drawing remain separate from this motion flag.</summary>
    public bool NeckMovementEnabled { get; internal set; }
    /// <summary>Lower-neck table byte offset: 0 inert, 2/4 alternating down/up, 6 one-way lowering, or 8 one-way raising; endpoints update this offset.</summary>
    public ushort LowerNeckMovementIndex { get; internal set; }
    /// <summary>Upper-neck table byte offset 0, 2, 4, 6, or 8; processed after the lower half so raising targets observe its newly updated angle.</summary>
    public ushort UpperNeckMovementIndex { get; internal set; }

    /// <summary>
    /// Pointer to the next seven-byte entry in the multi-frame sprite-tile transfer list.
    /// Zero means no list is active, exactly matching native WRAM <c>$7E:8004</c>.
    /// </summary>
    public ushort SpriteTileTransferEntryPointer { get; internal set; }

    /// <summary>True while bank-$88's rising layer-mask object is alive.</summary>
    public bool RisingHdmaActive { get; internal set; }

    /// <summary>Last verified room layer-blending configuration written by the body AI.</summary>
    public LayerBlendingConfiguration LayerBlendingDefaultConfig { get; internal set; }

    /// <summary>Direct BG2 scroll-register mirrors owned by the phase-two body art.</summary>
    public ushort Bg2XScroll { get; internal set; }
    /// <summary>Native BG2 vertical scroll mirror updated oppositely to physical body Y movement, keeping the large background-rendered body attached.</summary>
    public ushort Bg2YScroll { get; internal set; }
    /// <summary>Whether body AI has published encounter-owned BG2 scroll values for the runtime to apply instead of ordinary room scrolling.</summary>
    public bool HasBg2ScrollOverride { get; internal set; }
    /// <summary>ADF41C's main-screen BG2 flicker, separate from the OAM invisibility bit.</summary>
    public bool DeathBg2Hidden { get; internal set; }
    /// <summary>One-shot latch for the AFE0 body tilemap clear.</summary>
    public bool DeathBg2Cleared { get; internal set; }
    /// <summary>Live escape message owner installed by $A6:C23F.</summary>
    public EscapeTypewriterState? EscapeTypewriter { get; internal set; }

    /// <summary>Visible prefix of the enemy BG2 staging tilemap requested at ascent completion.</summary>
    public ushort EnemyBg2TilemapSize { get; internal set; }
    /// <summary>Witness that body setup, ascent completion, or death has requested its staging BG2 tilemap update; size names the requested prefix.</summary>
    public bool EnemyBg2TilemapTransferRequested { get; internal set; }

    /// <summary>Four-frame cadence cursor for the eight ascent dust positions.</summary>
    public ushort BodySubFunctionTimer { get; internal set; }

    /// <summary>Zero-based palette pointer-table index used while leaving fake-death grey.</summary>
    public ushort GrayTransitionCounter { get; internal set; }

    /// <summary>Enables the head's ordinary palette processing; resurrection and draining transitions suspend it while their own palettes own the colors.</summary>
    public bool BrainPaletteHandlingEnabled { get; internal set; }
    /// <summary>Allows head bytecode's mouth-drool generation, enabled during live combat and cleared by low-power/death transitions.</summary>
    public bool DroolGenerationEnabled { get; internal set; }
    /// <summary>Allows the head's small purple breath/dust producer, enabled after stretching and disabled for the drained corpse interval.</summary>
    public bool SmallPurpleBreathGenerationEnabled { get; internal set; }

    /// <summary>
    /// <c>MotherBrainBody.smallPurpleBreathActiveFlag</c> ($7E:786A): set by the small breath's
    /// initializer ($86:CA9B) and cleared by its last instruction ($86:CAEE), so at most one
    /// small breath is alive.
    /// </summary>
    public bool SmallPurpleBreathActive { get; internal set; }

    /// <summary>
    /// Attached-mouth offset selector used by drool opcode <c>$A9:9B3C</c>. Native
    /// increments before spawning and wraps at six, so a zero-initialized encounter emits
    /// parameter one first.
    /// </summary>
    public ushort DroolProjectileParameter { get; internal set; }

    /// <summary>Native phase-two walking/shot-reaction accumulator.</summary>
    public ushort WalkCounter { get; internal set; }

    /// <summary>
    /// Native <c>attackPhase</c> dispatch index used by <c>$A9:B64B</c>. Head animation
    /// runs independently while this advances from selection, through 64 cooldown frames,
    /// and back to the ordinary thinking function.
    /// </summary>
    public MotherBrainAttackPhase AttackPhase { get; internal set; }

    /// <summary>Unsigned 64-frame attack cooldown decremented by <c>$A9:B764</c>.</summary>
    public ushort AttackCooldown { get; internal set; }

    /// <summary>
    /// Number of active Mother Brain bombs. Phase two refuses another bomb when this is
    /// at least one; the shared bank-$86 bomb implementation owns increments and decrements.
    /// </summary>
    public ushort BombCounter { get; internal set; }

    /// <summary>
    /// Native <c>bodyTargetXPosition</c> used while the bomb decision walks the body toward
    /// X=$40 or $60 before entering its posture sequence.
    /// </summary>
    public ushort BodyTargetXPosition { get; internal set; }

    /// <summary>
    /// Native <c>deathBeamAttackPhase</c> consumed by <c>$A9:B87D</c>. Despite the historical
    /// symbol name, this is the ordinary phase-two red hand-beam attack, not the later HDMA
    /// rainbow beam used when the head's health reaches zero.
    /// </summary>
    public MotherBrainHandBeamPhase HandBeamPhase { get; internal set; }

    /// <summary>
    /// Shared cursor used by the bank-$86 hand-beam projectile chain. Every fired child
    /// starts from this exact 16.16 position, advances the cursor by the aimed velocity,
    /// then receives its own randomized secondary velocity.
    /// </summary>
    public ushort HandBeamNextXPosition { get; internal set; }
    /// <summary>Fractional low word of the shared 16.16 next-hand-beam X cursor, carried into whole-pixel X when each child advances it.</summary>
    public ushort HandBeamNextXSubposition { get; internal set; }
    /// <summary>Whole-pixel high word of the shared next-hand-beam Y cursor; charging starts it at body Y minus $30, and children advance it before scatter.</summary>
    public ushort HandBeamNextYPosition { get; internal set; }
    /// <summary>Fractional low word of the shared 16.16 next-hand-beam Y cursor, initially zero and preserved across child emissions.</summary>
    public ushort HandBeamNextYSubposition { get; internal set; }
    /// <summary>Signed native 8.8 aimed X velocity encoded in a word; advances the shared cursor before each child receives its independent randomized scatter velocity.</summary>
    public ushort HandBeamNextXVelocity { get; internal set; }
    /// <summary>Signed native 8.8 aimed Y velocity encoded in a word, paired with the X component to advance the shared emission cursor.</summary>
    public ushort HandBeamNextYVelocity { get; internal set; }
    /// <summary>Byte-angle aim retained in a native word, calculated from hand to Samus at charging setup and used as the children's randomized scatter-angle base.</summary>
    public ushort HandBeamNextAngle { get; internal set; }

    /// <summary>
    /// The already verified rainbow/finish-off/Baby/phase-three state machine, attached to
    /// the real body/head pair only after phase-two health reaches zero. Physical actor
    /// coordinates and bytecode remain owned by this room system; this object owns the long
    /// bank-$A9 function chain and forced-Samus calculations.
    /// </summary>
    public MotherBrainRainbowBeamAttackSequence? RainbowBeamSequence { get; internal set; }

    /// <summary>
    /// The dynamically allocated physical <c>$ECBF</c> enemy record created by
    /// <c>$A9:BE1B</c>. Keeping the slot and its extended cutscene state side by side makes
    /// the native cross-enemy index observable without pretending the Baby is a draw-only
    /// effect owned by Mother Brain's body.
    /// </summary>
    public RoomEnemySlot? BabyMetroidSlot { get; internal set; }
    /// <summary>Extended cutscene AI state attached to the dynamically allocated baby slot; its independent turn owns latch, draining, healing, and death handshakes.</summary>
    public BabyMetroidCutsceneState? BabyMetroid { get; internal set; }

    /// <summary>Debugger witness from the Baby's most recent physical enemy turn.</summary>
    public BabyMetroidCutsceneStepResult? LastBabyMetroidStep { get; internal set; }

    /// <summary>
    /// Last Baby instruction list copied into its physical slot. The ordinary bank-$A9
    /// interpreter advances the slot pointer independently, so this latch changes only
    /// when Baby AI explicitly invokes the native set-list helper.
    /// </summary>
    internal ushort BabyAppliedInstructionList { get; set; }

    /// <summary>
    /// Shared cry counter incremented by bank-$86 onion-ring collision and consumed by the
    /// Baby's later healing/idle functions. It is deliberately a count, not a boolean:
    /// native <c>INC</c> permits more than one ring to land before the next Baby turn.
    /// </summary>
    internal ushort PendingBabyCryCount { get; set; }

    /// <summary>Debugger witness from the most recent live rainbow body-function call.</summary>
    public MotherBrainRainbowBeamAttackStepResult? LastRainbowBeamStep { get; internal set; }

    /// <summary>Live bank-$88 rainbow-beam presentation state published by body AI.</summary>
    public bool RainbowBeamHdmaActive { get; internal set; }
    /// <summary>Native window/color state, advanced by emulation rather than host painting.</summary>
    public MotherBrainRainbowBeamHdmaState RainbowBeamHdma { get; } = new();
    /// <summary>Current native angular aim published from the beam's forced-movement owner and consumed by the bank-$88 HDMA presentation.</summary>
    public SnesAngle RainbowBeamAngle { get; internal set; }
    /// <summary>Native 8.8 angular-width word published by body AI as the rainbow beam widens or narrows; independent of pixel width at a given scanline.</summary>
    public ushort RainbowBeamAngularWidth { get; internal set; }
    /// <summary>Whether this body-function call requested a rainbow palette step; cleared at the beginning of every enemy frame.</summary>
    public bool RainbowBeamPaletteRequested { get; internal set; }
    /// <summary>The current frame's optional beam-impact dust/explosion request, published by continuing beam effects and cleared before the next frame.</summary>
    public MotherBrainRainbowExplosionRequest? LastRainbowBeamExplosion { get; internal set; }

    /// <summary>
    /// Last head list copied from the rainbow state machine into the physical head slot.
    /// The physical instruction pointer advances away from the list origin, so comparing
    /// against <c>Head.CurrentInstruction</c> would reinstall the list and reset its timer
    /// every frame. This separate producer-owned latch mirrors the fact that `$A9:C447`
    /// runs only when body AI explicitly requests a different head program.
    /// </summary>
    internal ushort RainbowAppliedHeadInstructionList { get; set; }

    /// <summary>
    /// Last <see cref="MotherBrainRainbowBeamAttackSequence.SmallPurpleBreathGenerationWriteCount"/>
    /// published to <see cref="SmallPurpleBreathGenerationEnabled"/>, so a brain-list clear
    /// survives until the body next writes the flag.
    /// </summary>
    internal uint RainbowAppliedSmallPurpleBreathWrites { get; set; }

    /// <summary>
    /// Last drop request emitted when an exploding Samus bomb destroys Mother Brain's bomb.
    /// Pickup selection remains owned by the common enemy-drop seam; retaining the head
    /// definition and exact coordinate makes that request inspectable in the debugger.
    /// </summary>
    public MotherBrainBombDropRequest? LastBombDropRequest { get; internal set; }

    /// <summary>
    /// Clamped byte-angle written by head opcode <c>$A9:9E5B</c> and consumed by the next
    /// <c>$A9:9E29</c> onion-ring spawn. It remains a word because native extended WRAM is
    /// word-addressed even though the calculation deliberately operates in 8-bit mode.
    /// </summary>
    public ushort OnionRingsTargetAngle { get; internal set; }

    private readonly ushort[] _initialTurretParameters = new ushort[12];
    private readonly List<MotherBrainMusicRequest> _musicRequests = new();
    private readonly List<MotherBrainPlmRequest> _plmRequests = new();

    internal void RecordInitialTurretRequests()
    {
        for (ushort parameter = 0; parameter < _initialTurretParameters.Length; parameter++)
            _initialTurretParameters[parameter] = parameter;
    }

    internal void BeginFrame()
    {
        _musicRequests.Clear();
        _plmRequests.Clear();
        LastSoundEffect = null;
        LastSoundEffectLibrary1 = null;
        LastSoundEffectLibrary3 = null;
        LastBombDropRequest = null;
        LastRainbowBeamStep = null;
        LastBabyMetroidStep = null;
        RainbowBeamPaletteRequested = false;
        LastRainbowBeamExplosion = null;
    }

    internal void RequestMusic(MusicCommand command, MusicCommandDelay delay) =>
        _musicRequests.Add(new MotherBrainMusicRequest(command, delay));

    internal void RequestPlm(byte blockX, byte blockY, ushort header) =>
        _plmRequests.Add(new MotherBrainPlmRequest(blockX, blockY, header));
}

/// <summary>
/// One typed <c>QueueMusic_Delayed*</c> call. The command retains its complete cartridge
/// word, including unknown commands, without conflating data uploads with track indices.
/// </summary>
/// <param name="Command">The full native music queue word, identifying stop, data load, or track selection.</param>
/// <param name="Delay">The authored music-queue delay before the command is eligible for consumption.</param>
public readonly record struct MotherBrainMusicRequest(MusicCommand Command, MusicCommandDelay Delay);

/// <summary>One literal <c>SpawnHardcodedPLM</c> call issued by Mother Brain's bank-$A9 AI.</summary>
/// <param name="BlockX">Foreground block-column coordinate, measured in sixteen-pixel room blocks.</param>
/// <param name="BlockY">Foreground block-row coordinate, measured in sixteen-pixel room blocks.</param>
/// <param name="Header">Low-word PLM definition-header pointer in fixed bank $84, consumed by the room PLM allocator.</param>
public readonly record struct MotherBrainPlmRequest(byte BlockX, byte BlockY, ushort Header);

/// <summary>One <c>$86:C5BB</c> enemy-drop request produced by a destroyed Mother Brain bomb.</summary>
public readonly record struct MotherBrainBombDropRequest();
