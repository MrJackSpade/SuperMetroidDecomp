using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Debugger-visible state that extends Bomb Torizo's ordinary $40-byte enemy slot. The
/// original actor aliases the bank-$AA boss variable arrays at $7800-$781F; spelling those
/// words out here keeps the state inspectable without pretending unrelated common slot
/// fields have the same meaning.
/// </summary>
public sealed class TorizoEnemyState
{
    /// <summary>Attaches variant-specific boss variables to the existing physical enemy slot.</summary>
    internal TorizoEnemyState(RoomEnemySlot slot, bool isGolden)
    {
        Slot = slot;
        IsGolden = isGolden;
    }

    /// <summary>The owning room enemy body slot, whose common position, health, properties, and instruction state this extension supplements.</summary>
    public RoomEnemySlot Slot { get; }

    /// <summary>Which way Torizo faces, which leg is forward and whether it is turning around.</summary>
    internal TorizoPostureFlags Posture { get; set; }

    /// <summary>The behaviour latches: a caught Super Missile, a stun, and Bomb Torizo's blown-up face and gut.</summary>
    internal TorizoBehaviorFlags Behavior { get; set; }
    /// <summary>Whether the owning definition is Golden Torizo ($EF7F), rather than Bomb Torizo ($EEFF); fixed at initialization.</summary>
    public bool IsGolden { get; }

    /// <summary>Persistent target colors and numerator shared by awakening and death fades.</summary>
    internal GradualPaletteTransition? PaletteTransition { get; set; }

    /// <summary>The body program's cursor, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram> BodyProgram { get; set; }

    /// <summary>Native <c>toriz_var_00</c>: the body program's return/landing point.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram> ReturnCursor { get; set; }

    /// <summary>Native <c>toriz_var_01</c>: the body program the gut explosion interrupted, restored afterwards.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram> InterruptedCursor { get; set; }

    /// <summary>Native <c>toriz_var_03</c>: delayed in-air transition counter.</summary>
    public WrappingCountdown AirTransitionTimer { get; internal set; }

    /// <summary>
    /// Native <c>toriz_var_04</c>, held as whether the body's animation is locked: the awakening list locks it and
    /// later unlocks it. While it is locked Bomb Torizo rejects shots before common damage, and Golden Torizo sends
    /// them to common damage without its catches or its stun. The cartridge marks the lock with a nonzero word that
    /// nothing reads; every test is locked versus unlocked.
    /// </summary>
    public bool AnimationLocked { get; internal set; }

    /// <summary>
    /// Native <c>toriz_var_09</c>: stepping cues counted since a decision last reset it, from zero to
    /// <see cref="LargestCount"/>. Golden Torizo's repeated-jump branch fires once it reaches eight.
    /// </summary>
    public int DecisionCounter { get; internal set; }

    /// <summary>Whether Golden Torizo's eye beams explode on impact; its eye-beam list enables them.</summary>
    public bool EyeBeamExplosionsEnabled { get; internal set; }

    /// <summary>
    /// Native <c>toriz_var_07</c>: consecutive frames Samus has space-jumped, from zero to <see cref="LargestCount"/>.
    /// Past 360 it lets Golden Torizo's forward leap fire without its input and random gates.
    /// </summary>
    public int SamusSpaceJumpFrames { get; internal set; }

    /// <summary>
    /// Signed 8.8 jump velocity words. The walking instructions reuse the native X word for a
    /// signed whole-pixel walk delta, which they read as whole pixels.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Signed 8.8 acceleration a jump adds to <see cref="Velocity"/> every frame until it lands. Only the Y axis is
    /// meaningful; X stays zero, so adding the whole vector leaves the horizontal velocity unchanged.
    /// </summary>
    public EightEightVector<RoomPixels> JumpAcceleration { get; internal set; }

    /// <summary>Native <c>toriz_var_E</c> main-function dispatcher pointer.</summary>
    public TorizoFunction Function { get; internal set; }

    /// <summary>Native <c>toriz_var_F</c> active-state pre-instruction pointer.</summary>
    public TorizoPreInstruction PreInstruction { get; internal set; }

    /// <summary>Host lifecycle latch set when $AA:C6C6 observes the $D6EA hand-trigger PLM gone and resumes Bomb Torizo's awakening instruction list.</summary>
    public bool AwakeningReleased { get; internal set; }
    /// <summary>Host latch preventing repeat entry into death list $AA:B1C8 after lethal damage; remains set for this state instance.</summary>
    public bool DeathStarted { get; internal set; }
    /// <summary>Whether the area's Torizo defeat bit ($04) was already set at initialization or was set by the translated $AA:B24D death instruction.</summary>
    public bool BossBitSet { get; internal set; }
    /// <summary>Host latch recording the death instruction's 16-pickup scatter request in the variant's arena; does not mean those pickups were collected.</summary>
    public bool ItemDropRequested { get; internal set; }

    /// <summary>
    /// The largest value <see cref="DecisionCounter"/> and <see cref="SamusSpaceJumpFrames"/> reach: counting one more
    /// wraps either to zero, as the cartridge's counters do.
    /// </summary>
    internal const int LargestCount = 65535;

    /// <summary>Counts one stepping cue, wrapping from <see cref="LargestCount"/> to zero.</summary>
    internal void CountDecisionStep() => DecisionCounter = NextCount(DecisionCounter);

    /// <summary>Counts one more consecutive space-jumping frame, wrapping from <see cref="LargestCount"/> to zero.</summary>
    internal void CountSamusSpaceJumpFrame() => SamusSpaceJumpFrames = NextCount(SamusSpaceJumpFrames);

    /// <summary>The count after <paramref name="count"/>, wrapping from <see cref="LargestCount"/> to zero.</summary>
    private static int NextCount(int count) => count == LargestCount ? 0 : count + 1;
}
