namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$A5 function pointers stored in Draygon's body <c>var_A</c>. Keeping the ROM
/// addresses as enum values makes a debugger watch directly comparable with the disassembly
/// while preventing the main dispatcher from becoming a collection of unexplained words.
/// </summary>
public enum DraygonAiFunction : ushort
{
    /// <summary><c>$A5:871B Function_DraygonBody_FightIntro_InitialDelay</c>: loads four Evir sprites, then waits 256 gameplay frames.</summary>
    IntroInitialDelay = 0x871b,
    /// <summary><c>$A5:878B Function_DraygonBody_FightIntro_Dance</c>: advances the opening Evir movement stream for 1,232 frames before the first swoop.</summary>
    IntroDance = 0x878b,
    /// <summary><c>$A5:87F4 Function_DraygonBody_SwoopRight_Setup</c>: constructs the swoop-height table, aims horizontal velocity at Samus, and installs right-facing graphics.</summary>
    SwoopRightSetup = 0x87f4,
    /// <summary><c>$A5:88B1 Function_DraygonBody_SwoopRight_Descending</c>: traverses height samples backward while moving right toward the bottom of the swoop.</summary>
    SwoopRightDescending = 0x88b1,
    /// <summary><c>$A5:8922 Function_DraygonBody_SwoopRight_Apex</c>: recalculates horizontal velocity toward the right exit for the ascending half.</summary>
    SwoopRightApex = 0x8922,
    /// <summary><c>$A5:8951 Function_DraygonBody_SwoopRight_Ascending</c>: traverses height samples forward and selects a leftward swoop or goop pass from the current RNG word.</summary>
    SwoopRightAscending = 0x8951,
    /// <summary><c>$A5:89B3 Function_DraygonBody_SwoopLeft_Setup</c>: reuses the generated height table, aims leftward velocity at Samus, and installs left-facing graphics.</summary>
    SwoopLeftSetup = 0x89b3,
    /// <summary><c>$A5:8A00 Function_DraygonBody_SwoopLeft_Descending</c>: traverses height samples backward while moving left toward the bottom of the swoop.</summary>
    SwoopLeftDescending = 0x8a00,
    /// <summary><c>$A5:8A50 Function_DraygonBody_SwoopLeft_Apex</c>: recalculates horizontal velocity toward the left reset position for the ascending half.</summary>
    SwoopLeftApex = 0x8a50,
    /// <summary><c>$A5:8A90 Function_DraygonBody_SwoopLeft_Ascending</c>: completes the leftward ascent, resets X, and selects a rightward swoop or goop pass from the current RNG word.</summary>
    SwoopLeftAscending = 0x8a90,
    /// <summary><c>$A5:8B0A Function_DraygonBody_GoopRight_Setup</c>: starts the one-pixel-per-frame rightward goop path at X=$FFB0, Y=$0180.</summary>
    GoopRightSetup = 0x8b0a,
    /// <summary><c>$A5:8B52 Function_DraygonBody_GoopRight_MoveUntilSamusInRange</c>: follows the oscillating path until horizontal separation from Samus is below $D0 pixels.</summary>
    GoopRight = 0x8b52,
    /// <summary><c>$A5:8BAE Function_DraygonBody_GoopRight_FiringGoops</c>: requests right-facing goop lists from RNG samples and begins chasing when goop slows Samus.</summary>
    GoopRightTail = 0x8bae,
    /// <summary><c>$A5:8C33 Function_DraygonBody_GoopRight_MoveUntilOffScreen</c>: finishes the rightward oscillating exit without firing goop.</summary>
    GoopRightRecovery = 0x8c33,
    /// <summary><c>$A5:8C8E Function_DraygonBody_GoopLeft_Setup</c>: starts the one-pixel-per-frame leftward goop path at the right reset X and Y=$0180.</summary>
    GoopLeftSetup = 0x8c8e,
    /// <summary><c>$A5:8CD4 Function_DraygonBody_GoopLeft_MoveUntilSamusInRange</c>: follows the oscillating path until horizontal separation from Samus is below $D0 pixels.</summary>
    GoopLeft = 0x8cd4,
    /// <summary><c>$A5:8D30 Function_DraygonBody_GoopLeft_FiringGoops</c>: requests left-facing goop lists from RNG samples and begins chasing when goop slows Samus.</summary>
    GoopLeftTail = 0x8d30,
    /// <summary><c>$A5:8DB2 Function_DraygonBody_GoopLeft_MoveUntilOffScreen</c>: completes the leftward exit, still allowing attached goop to trigger a chase.</summary>
    GoopLeftRecovery = 0x8db2,
    /// <summary><c>$A5:8E19 Function_DraygonBody_ChaseSamus</c>: moves toward goop-slowed Samus at two pixels per frame and checks the eight-pixel claw window.</summary>
    TryGrabSamus = 0x8e19,
    /// <summary><c>$A5:8F10 Function_DraygonBody_RepelledByGrapple</c>: despite the managed name, drops an active grapple and enters release without grabbing Samus.</summary>
    GrabbedSamus = 0x8f10,
    /// <summary><c>$A5:8F1E Function_DraygonBody_GrabbedSamus_MovingToTargetPosition</c>: carries Samus toward room pixel ($100,$180), then starts the rising spiral.</summary>
    CarrySamus = 0x8f1e,
    /// <summary><c>$A5:8FD6 Function_DraygonBody_GrabbedSamus_RisingSpiralMovement</c>: expands and raises the carry orbit, with RNG-selected incidental tail whips.</summary>
    FlailWithSamus = 0x8fd6,
    /// <summary><c>$A5:90D4 Function_DraygonBody_GrabbedSamus_TailWhip</c>: holds the grabbed pair together during a 64-frame whip, then resumes the spiral.</summary>
    TailWhipWithSamus = 0x90d4,
    /// <summary><c>$A5:9105 Function_DraygonBody_GrabbedSamus_FinalSpanking_Start</c>: installs the facing-specific four-repeat finishing tail-whip list.</summary>
    FinalTailWhips = 0x9105,
    /// <summary><c>$A5:9124 Function_DraygonBody_GrabbedSamus_FinalSpanking_Ongoing</c>: keeps Samus attached until the tail list changes the body function to release.</summary>
    FinalTailWhipsWait = 0x9124,
    /// <summary><c>$A5:9128 Function_DraygonBody_GrabbedSamus_FlailTail_FlyStraightUp</c>: releases Samus, restores ordinary collision, and starts a tail flail before retreat.</summary>
    ReleaseSamus = 0x9128,
    /// <summary><c>$A5:9154 Function_DraygonBody_GrabbedSamus_FlyStraightUp</c>: retreats upward four pixels per frame, then resets for a rightward swoop.</summary>
    FlyStraightUp = 0x9154,
    /// <summary><c>$A5:9185 Function_DraygonBody_DeathSequence_DriftToDeathSpot</c>: drifts toward room pixel ($100,$1E0), then spawns burial Evirs and retires the arms and tail.</summary>
    Dying = 0x9185,
    /// <summary><c>$A5:9294 Function_DraygonBody_DeathSequence_WaitForEvirs</c>: despite the managed name, waits $1A0 frames while Evirs approach before the body sinks.</summary>
    DyingSink = 0x9294,
    /// <summary><c>$A5:92AB Function_DraygonBody_DeathSequence_BuriedByEvirs</c>: sinks one pixel per frame to Y=$240, scatters drops, deletes the remaining parts, and persists defeat.</summary>
    DyingFinish = 0x92ab,
}

/// <summary>
/// Draygon's encounter-wide WRAM extension. The four visible enemy records remain physical
/// <see cref="RoomEnemySlot"/> instances; only the bank-$A5 words outside those common
/// records live here. This avoids pretending that the boss is one ordinary compact actor.
/// </summary>
public sealed class DraygonEnemyState
{
    internal DraygonEnemyState(RoomEnemySlot body) => Body = body;

    /// <summary>Native enemy slot $0000, which owns encounter movement, health, and the body-function dispatcher.</summary>
    public RoomEnemySlot Body { get; }
    /// <summary>Native enemy slot $0040, installed by the eye initializer; tracks Samus using coordinates copied from the body.</summary>
    public RoomEnemySlot? Eye { get; internal set; }
    /// <summary>Native enemy slot $0080, installed by the tail initializer; its instruction lists drive whips and the final release handshake.</summary>
    public RoomEnemySlot? Tail { get; internal set; }
    /// <summary>Native enemy slot $00C0, installed by the arms initializer; its animation follows the body's authoritative coordinates.</summary>
    public RoomEnemySlot? Arms { get; internal set; }

    /// <summary>Native <c>enemy_bg2_tilemap_size</c>, measured in words.</summary>
    public ushort Bg2TilemapSize { get; internal set; }

    /// <summary>True after the $1000-byte enemy BG2 surface has been filled with tile $338.</summary>
    public bool BackgroundTilemapPrepared { get; internal set; }

    /// <summary>Room-loading IRQ command selected by Draygon's body instruction stream.</summary>
    public ushort RoomLoadingIrqCommand { get; internal set; }

    /// <summary>The boss-room load disables the normal minimap and marks its four tiles.</summary>
    public bool MinimapDisabledAndBossTilesExplored { get; internal set; }

    /// <summary>Native <c>$7E:7800</c>: left-side off-screen reset X position.</summary>
    public ushort LeftSideResetXPosition { get; internal set; }

    /// <summary>Native <c>$7E:7802</c>: common reset Y position.</summary>
    public ushort ResetYPosition { get; internal set; }

    /// <summary>Native <c>$7E:7804</c>: right-side off-screen reset X position.</summary>
    public ushort RightSideResetXPosition { get; internal set; }

    /// <summary>Native <c>$7E:781E</c>: 8.8 acceleration used to construct swoop heights.</summary>
    public ushort SwoopYAcceleration { get; internal set; }

    /// <summary>Native <c>$7E:808C</c> byte index into the intro dance stream.</summary>
    public ushort FightIntroDanceIndex { get; internal set; }

    /// <summary>Native long WRAM offsets applied by the tail-whip instruction stream.</summary>
    public ushort BodyGraphicsXDisplacement { get; internal set; }
    /// <summary>Signed pixel offset encoded as a native word, written by $A5:9E0A and added to body graphics Y without moving the enemy records.</summary>
    public ushort BodyGraphicsYDisplacement { get; internal set; }

    /// <summary>
    /// The native table at $7E:9002 is written every four bytes and addressed by a byte
    /// offset. This dense view stores those meaningful words at <c>offset / 4</c>.
    /// </summary>
    public ushort[] SwoopYPositions { get; } = new ushort[0x0201];

    /// <summary>Number of generated descent samples, excluding the terminal reset-Y word.</summary>
    public int SwoopPathEntryCount { get; internal set; }

    /// <summary>Native long facing word: zero left, one right.</summary>
    public bool FacingRight { get; internal set; }

    /// <summary>Number of physical object-$18 breath bubbles admitted during a swoop.</summary>
    public int BreathBubblesSpawned { get; internal set; }

    /// <summary>Native long goop-path cosine angle, constrained to its low byte.</summary>
    public ushort GoopYOscillationAngle { get; internal set; }

    /// <summary>Native long count initialized to sixteen when a firing pass begins.</summary>
    public ushort GoopCounter { get; internal set; }

    /// <summary>Count of wall-turret projectiles admitted to the shared pool; disabled cannons and exhausted allocations do not increment it.</summary>
    public int WallTurretsSpawned { get; internal set; }
    /// <summary>Count of goop projectiles admitted by the facing-specific body-list producers at $A5:9F7C/$9FAE.</summary>
    public int GoopProjectilesSpawned { get; internal set; }

    /// <summary>
    /// Native long WRAM <c>$7E:780C/$780E</c>. Draygon expands this 16.16 radius by
    /// exactly <c>$0000.2000</c> per spiral frame, so keeping both words is necessary to
    /// reproduce the eight-frame pixel cadence rather than rounding it into host floats.
    /// </summary>
    public ushort SpiralXRadius { get; internal set; }
    /// <summary>Native $7E:780E fractional low word paired with <see cref="SpiralXRadius"/>; each $2000 increment accumulates one eighth of a pixel.</summary>
    public ushort SpiralXSubradius { get; internal set; }

    /// <summary>Native center point <c>$7E:7810/$7812</c> used by the carry spiral.</summary>
    public ushort SpiralCenterX { get; internal set; }
    /// <summary>Native $7E:7812 whole-pixel room Y of the carry orbit's center; initialized to $180 and lifted through its fractional companion.</summary>
    public ushort SpiralCenterY { get; internal set; }

    /// <summary>
    /// Fractional low word paired with <see cref="SpiralCenterY"/>. Retail subtracts
    /// <c>$0000.4000</c> each spiral frame, lifting the center by one pixel every four.
    /// </summary>
    public ushort SpiralCenterYSubposition { get; internal set; }

    /// <summary>Low-byte polar angle and 8.8 angular delta at <c>$7E:7814/$7816</c>.</summary>
    public ushort SpiralAngle { get; internal set; }
    /// <summary>Native $7E:7816 unsigned 8.8 angle step; initialized to $0800, decremented by one per spiral frame, and applied through its high byte.</summary>
    public ushort SpiralAngleDelta { get; internal set; }

    /// <summary>Native <c>$7E:7818</c> countdown for an incidental 64-frame tail whip.</summary>
    public ushort TailWhipTimer { get; internal set; }

    /// <summary>Debugger-visible proof that the physical chase reached its grab window.</summary>
    public int SuccessfulGrabs { get; internal set; }

    /// <summary>Number of ROM tail-hit opcodes that actually damaged grabbed Samus.</summary>
    public int TailWhipHits { get; internal set; }

    /// <summary>Suit-scaled damage dealt by the most recent tail-hit instruction.</summary>
    public ushort LastTailWhipDamage { get; internal set; }

    /// <summary>
    /// Native byte offset <c>$7E:781C</c> into the eight health-palette bands. It advances
    /// 0,2,...,14 because the threshold table is word-addressed even though each palette
    /// record contains four colors.
    /// </summary>
    public ushort HealthPaletteTableByteIndex { get; internal set; }

    /// <summary>
    /// Native death-vector angle and its deliberately unused 16.16 velocity words. The
    /// cartridge computes these once in the fatal reaction, then recalculates movement each
    /// death frame; retaining them documents and exposes that observable WRAM side effect.
    /// </summary>
    public ushort DeathMovementAngle { get; internal set; }
    /// <summary>Whole-pixel X magnitude of the 16.16 death velocity calculated on the fatal reaction; retained for WRAM parity but not used by drift movement.</summary>
    public ushort UnusedDeathXSpeed { get; internal set; }
    /// <summary>Fractional X magnitude paired with <see cref="UnusedDeathXSpeed"/>; the fatal reaction writes it, but death frames recalculate their own vector.</summary>
    public ushort UnusedDeathXSubspeed { get; internal set; }
    /// <summary>Whole-pixel Y magnitude of the 16.16 death velocity calculated on the fatal reaction; retained for WRAM parity but not used by drift movement.</summary>
    public ushort UnusedDeathYSpeed { get; internal set; }
    /// <summary>Fractional Y magnitude paired with <see cref="UnusedDeathYSpeed"/>; the fatal reaction writes it, but death frames recalculate their own vector.</summary>
    public ushort UnusedDeathYSubspeed { get; internal set; }

    /// <summary>Number of random body-list explosions admitted during the fatal animation.</summary>
    public int DeathAnimationObjectsSpawned { get; internal set; }

    /// <summary>Number of fixed-cadence smoke objects requested below the burial line.</summary>
    public int DeathSmokeObjectsSpawned { get; internal set; }

    /// <summary>True after the six table-positioned burial Evirs replace the shared pool.</summary>
    public bool DeathEvirsSpawned { get; internal set; }

    /// <summary>Debugger-visible cartridge requests emitted by the terminal boss path.</summary>
    public ushort? LastSoundLibrary3 { get; internal set; }
    /// <summary>Music command published when the dying body reaches its burial point; selects track three, or remains null before that transition.</summary>
    public MusicCommand? MusicRequest { get; internal set; }
    /// <summary>True after the terminal burial routine scatters sixteen physical pickups using the body's drop definition.</summary>
    public bool ItemDropRequested { get; internal set; }
    /// <summary>True after terminal burial invokes the persistent area-boss-defeated callback; prevents repeating that persistence write.</summary>
    public bool BossDefeatPersisted { get; internal set; }

    /// <summary>Most recent library-two SFX requested by Draygon's body instruction list.</summary>
    public ushort? LastSoundLibrary2 { get; internal set; }

    /// <summary>Set when the init routine schedules Evir tiles for VRAM $6D00.</summary>
    public bool IntroEvirGraphicsLoaded { get; internal set; }

    /// <summary>Number of physical object-$3B intro Evirs admitted to the shared pool.</summary>
    public int IntroEvirsSpawned { get; internal set; }

    /// <summary>Number of times the opening dance routine advanced its four actors.</summary>
    public int IntroDanceFrames { get; internal set; }

    /// <summary>Number of exact 64-frame turret-selection RNG draws observed.</summary>
    public int TurretCadenceChecks { get; internal set; }

    /// <summary>
    /// Exact extra-enemy WRAM words disabled by Draygon setup or a destroyed room PLM.
    /// This is the shared storage observed by bank $A5's firing selector.
    /// </summary>
    internal HashSet<ushort> DisabledCannonWords { get; } = [];

    /// <summary>Bank-$A5 body-function address stored in native <c>var_A</c>; body main AI dispatches this before copying coordinates to the three parts.</summary>
    public DraygonAiFunction Function
    {
        get => (DraygonAiFunction)Body.VariableA;
        internal set => Body.VariableA = (ushort)value;
    }

    /// <summary>Body <c>var_B</c>: intro elapsed-frame count, swoop-table byte offset advanced by four, or burial-wait countdown according to <see cref="Function"/>.</summary>
    public ushort FunctionTimer
    {
        get => Body.VariableB;
        internal set => Body.VariableB = value;
    }
}
