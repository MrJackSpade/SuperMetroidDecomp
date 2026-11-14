using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Torizo's state beyond what every enemy has, kept in named fields rather than in
/// shared boss values, so no common enemy field takes on a second meaning.
/// </summary>
public sealed class TorizoEnemyState : ILitTorizo
{
    /// <summary>Attaches variant-specific boss state to the Torizo's existing enemy in the room.</summary>
    internal TorizoEnemyState(RoomEnemySlot slot, bool isGolden)
    {
        Slot = slot;
        IsGolden = isGolden;
    }

    /// <summary>The owning room enemy body slot, whose common position, health, properties, and animation state this extension supplements.</summary>
    public RoomEnemySlot Slot { get; }

    /// <summary>Which way Torizo faces, which leg is forward and whether it is turning around.</summary>
    internal TorizoPostureFlags Posture { get; set; }

    /// <summary>The behaviour latches: a caught Super Missile, a stun, and Bomb Torizo's blown-up face and gut.</summary>
    internal TorizoBehaviorFlags Behavior { get; set; }
    /// <summary>Whether the owning definition is Golden Torizo, rather than Bomb Torizo; fixed at initialization.</summary>
    public bool IsGolden { get; }

    /// <summary>Whether this Torizo has no health left: its belly's glow ends on the first update it sees so (#1700).</summary>
    public bool IsDefeated => Slot.Health == 0;

    /// <summary>Persistent target colors and numerator shared by awakening and death fades.</summary>
    internal TorizoColours Colours { get; } = new();

    /// <summary>The body animation playing and where it stands in it (#1600).</summary>
    internal AnimationPlayback<TorizoAnimation> BodyPlayback { get; set; }

    /// <summary>Where the body's play returns or lands: a call's return point, or a landing's animation.</summary>
    internal AnimationPlayback<TorizoAnimation> ReturnPlayback { get; set; }

    /// <summary>Where the gut explosion interrupted the body's play, resumed afterwards.</summary>
    internal AnimationPlayback<TorizoAnimation> InterruptedPlayback { get; set; }

    /// <summary>Delayed in-air transition counter.</summary>
    public int AirTransitionTimer { get; internal set; }

    /// <summary>
    /// Whether the body's animation is locked: the awakening animation locks it and
    /// later unlocks it. While it is locked Bomb Torizo rejects shots before common damage, and Golden Torizo sends
    /// them to common damage without its catches or its stun. Every test is locked versus unlocked.
    /// </summary>
    public bool AnimationLocked { get; internal set; }

    /// <summary>
    /// Stepping cues counted since a decision last reset it, from zero to
    /// <see cref="LargestCount"/>. Golden Torizo's repeated-jump branch fires once it reaches eight.
    /// </summary>
    public int DecisionCounter { get; internal set; }

    /// <summary>Whether Golden Torizo's eye beams explode on impact; its eye-beam animation enables them.</summary>
    public bool EyeBeamExplosionsEnabled { get; internal set; }

    /// <summary>
    /// Consecutive frames Samus has space-jumped, from zero to <see cref="LargestCount"/>.
    /// Past 360 it lets Golden Torizo's forward leap fire without its input and random gates.
    /// </summary>
    public int SamusSpaceJumpFrames { get; internal set; }

    /// <summary>
    /// Signed 1/256-pixel velocity of the Torizo's jumps and falls. A walking step moves the Torizo by its own whole pixels and
    /// leaves its sideways velocity at <see cref="TorizoWalkLeftVelocity"/>.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// Signed 1/256-pixel acceleration a jump adds to <see cref="Velocity"/> every frame until it lands. Only the Y axis is
    /// meaningful; X stays zero, so adding the whole vector leaves the horizontal velocity unchanged.
    /// </summary>
    public EightEightVector<RoomPixels> JumpAcceleration { get; internal set; }

    /// <summary>The main phase the Torizo runs each frame, or null until its first animation selects one.</summary>
    public TorizoPhase? Phase { get; internal set; }

    /// <summary>
    /// The behaviour the Torizo runs before each animation step while active, or null until an animation installs one.
    /// </summary>
    public TorizoFrameBehavior? FrameBehavior { get; internal set; }

    /// <summary>Latch preventing repeat entry into the death script after lethal damage; remains set for this state instance.</summary>
    public bool DeathStarted { get; internal set; }

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
