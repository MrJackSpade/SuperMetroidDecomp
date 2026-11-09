using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Torizo's state beyond its ordinary enemy record, kept in named fields rather than in
/// shared boss variables, so no common enemy field takes on a second meaning.
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
    /// <summary>Whether the owning definition is Golden Torizo, rather than Bomb Torizo; fixed at initialization.</summary>
    public bool IsGolden { get; }

    /// <summary>Persistent target colors and numerator shared by awakening and death fades.</summary>
    internal TorizoColours Colours { get; } = new();

    /// <summary>The body program's cursor.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram> BodyProgram { get; set; }

    /// <summary><c>toriz_var_00</c>: the body program's return/landing point.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram> ReturnCursor { get; set; }

    /// <summary><c>toriz_var_01</c>: the body program the gut explosion interrupted, restored afterwards.</summary>
    internal AnimationProgramCursor<TorizoBodyProgram> InterruptedCursor { get; set; }

    /// <summary><c>toriz_var_03</c>: delayed in-air transition counter.</summary>
    public int AirTransitionTimer { get; internal set; }

    /// <summary>
    /// <c>toriz_var_04</c>, held as whether the body's animation is locked: the awakening list locks it and
    /// later unlocks it. While it is locked Bomb Torizo rejects shots before common damage, and Golden Torizo sends
    /// them to common damage without its catches or its stun. The original game marks the lock with a nonzero word that
    /// nothing reads; every test is locked versus unlocked.
    /// </summary>
    public bool AnimationLocked { get; internal set; }

    /// <summary>
    /// <c>toriz_var_09</c>: stepping cues counted since a decision last reset it, from zero to
    /// <see cref="LargestCount"/>. Golden Torizo's repeated-jump branch fires once it reaches eight.
    /// </summary>
    public int DecisionCounter { get; internal set; }

    /// <summary>Whether Golden Torizo's eye beams explode on impact; its eye-beam list enables them.</summary>
    public bool EyeBeamExplosionsEnabled { get; internal set; }

    /// <summary>
    /// <c>toriz_var_07</c>: consecutive frames Samus has space-jumped, from zero to <see cref="LargestCount"/>.
    /// Past 360 it lets Golden Torizo's forward leap fire without its input and random gates.
    /// </summary>
    public int SamusSpaceJumpFrames { get; internal set; }

    /// <summary>
    /// Signed 8.8 velocity of the Torizo's jumps and falls. A walking step moves the Torizo by its own whole pixels and
    /// leaves its sideways velocity at <see cref="TorizoWalkLeftVelocity"/>.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Signed 8.8 acceleration a jump adds to <see cref="Velocity"/> every frame until it lands. Only the Y axis is
    /// meaningful; X stays zero, so adding the whole vector leaves the horizontal velocity unchanged.
    /// </summary>
    public EightEightVector<RoomPixels> JumpAcceleration { get; internal set; }

    /// <summary>The main function the Torizo runs each frame.</summary>
    public TorizoFunction Function { get; internal set; }

    /// <summary>The behaviour the Torizo runs before each animation step while active.</summary>
    public TorizoFrameBehavior FrameBehavior { get; internal set; }

    /// <summary>Host lifecycle latch set when the awakening check observes the hand trigger gone and resumes Bomb Torizo's awakening animation script.</summary>
    public bool AwakeningReleased { get; internal set; }
    /// <summary>Host latch preventing repeat entry into the death script after lethal damage; remains set for this state instance.</summary>
    public bool DeathStarted { get; internal set; }
    /// <summary>Whether the area's Torizo defeat bit was already set at initialization or was set by the translated death step.</summary>
    public bool BossBitSet { get; internal set; }
    /// <summary>Host latch recording the death instruction's 16-pickup scatter request in the variant's arena; does not mean those pickups were collected.</summary>
    public bool ItemDropRequested { get; internal set; }

    /// <summary>
    /// The largest value <see cref="DecisionCounter"/> and <see cref="SamusSpaceJumpFrames"/> reach: counting one more
    /// wraps either to zero, as the game's counters do.
    /// </summary>
    internal const int LargestCount = 65535;

    /// <summary>Counts one stepping cue, wrapping from <see cref="LargestCount"/> to zero.</summary>
    internal void CountDecisionStep() => DecisionCounter = NextCount(DecisionCounter);

    /// <summary>Counts one more consecutive space-jumping frame, wrapping from <see cref="LargestCount"/> to zero.</summary>
    internal void CountSamusSpaceJumpFrame() => SamusSpaceJumpFrames = NextCount(SamusSpaceJumpFrames);

    /// <summary>The count after <paramref name="count"/>, wrapping from <see cref="LargestCount"/> to zero.</summary>
    private static int NextCount(int count) => count == LargestCount ? 0 : count + 1;
}
