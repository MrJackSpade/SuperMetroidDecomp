using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Torizo's state beyond what every enemy has, kept in named fields rather than in
/// shared boss values, so no common enemy field takes on a second meaning. The Torizo's entity owns it as its species
/// state (#2030).
/// </summary>
public sealed class TorizoEnemyState
{
    /// <summary>Creates the state of a Bomb or Golden Torizo.</summary>
    internal TorizoEnemyState(bool isGolden)
    {
        IsGolden = isGolden;
    }

    /// <summary>Which way Torizo faces, which leg is forward and whether it is turning around.</summary>
    internal TorizoPostureFlags Posture { get; set; }

    /// <summary>The behaviour latches: a caught Super Missile, a stun, and Bomb Torizo's blown-up face and gut.</summary>
    internal TorizoBehaviorFlags Behavior { get; set; }
    /// <summary>Whether the owning definition is Golden Torizo, rather than Bomb Torizo; fixed at initialization.</summary>
    public bool IsGolden { get; }

    /// <summary>Persistent target colors and numerator shared by awakening and death fades.</summary>
    internal TorizoColours Colours { get; } = new();

    /// <summary>The body animation playing and where it stands in it (#1600).</summary>
    internal AnimationPlayback<TorizoAnimation> BodyPlayback { get; set; }

    /// <summary>The animation the Torizo's jump or fall lands it in, as the jump or fall names it (#2028); null before its first.</summary>
    internal TorizoAnimation? Landing { get; set; }

    /// <summary>
    /// Where an attack or a stagger interrupted the Torizo's walk, which resumes there when it ends (#2028); null before the
    /// first interruption.
    /// </summary>
    internal AnimationPlayback<TorizoAnimation>? InterruptedWalk { get; set; }

    /// <summary>Where the gut explosion interrupted the body's play, resumed afterwards.</summary>
    internal AnimationPlayback<TorizoAnimation> InterruptedPlayback { get; set; }

    /// <summary>
    /// Updates until the walking Torizo turns toward Samus, or zero while no turn is due: a walking step arms it when
    /// Samus is behind the Torizo, and its walking motion counts it down (#1931).
    /// </summary>
    public int TurnTimer { get; internal set; }

    /// <summary>
    /// Whether the body's animation is locked: the awakening animation locks it and
    /// later unlocks it. While it is locked Bomb Torizo rejects shots before common damage, and Golden Torizo sends
    /// them to common damage without its catches or its stun. Every test is locked versus unlocked.
    /// </summary>
    public bool AnimationLocked { get; internal set; }

    /// <summary>
    /// Stepping cues counted since a decision last reset it. Golden Torizo's repeated-jump branch fires once it reaches
    /// <see cref="TorizoDecisionDefinitions.RepeatedJumpSteps"/> and resets it, and that branch follows every one of its
    /// stepping cues; Bomb Torizo never reads it. It never nears the 65536 counts at which the original game's count wrapped
    /// (#1945), so it counts on.
    /// </summary>
    public int DecisionCounter { get; internal set; }

    /// <summary>Whether Golden Torizo's eye beams explode on impact; its eye-beam animation enables them.</summary>
    public bool EyeBeamExplosionsEnabled { get; internal set; }

    /// <summary>
    /// Consecutive frames Samus has space-jumped, 0 through 65535 (<see cref="WrappingCounter"/>). Past
    /// <see cref="TorizoDecisionDefinitions.SpaceJumpLeapFrames"/> it lets Golden Torizo's forward leap fire without its
    /// input and random gates. The wrap is gameplay (#1945): after 65536 consecutive space-jumping frames the count starts
    /// again from zero, and the leap waits for its gates again for the next 361 frames.
    /// </summary>
    public int SamusSpaceJumpFrames { get; internal set; }

    /// <summary>
    /// Signed 1/256-pixel velocity of the Torizo's jumps and falls. A walking step moves the Torizo by its own whole pixels and
    /// leaves its sideways velocity at <see cref="TorizoWalkLeftVelocity"/>. Any change to its vertical speed ends the
    /// resting fall (<see cref="FallsAtRestingSpeed"/>).
    /// </summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get;
        internal set
        {
            if (value.Y != field.Y)
                FallsAtRestingSpeed = false;
            field = value;
        }
    }

    /// <summary>
    /// Whether the Torizo falls at <see cref="TorizoMotionDefinitions.RestingFallSpeed"/>, as its setup, a landing and a
    /// walk off a ledge leave it, its vertical speed unchanged since (#1945): a Torizo that meets the ground falling so
    /// lands without a quake. The original game marked this by the speed alone; no other fall reaches it exactly, since
    /// gravity and the leaps add forty 256ths an update to a rest or to a leap's rise, neither of which comes back to it.
    /// </summary>
    internal bool FallsAtRestingSpeed { get; private set; }

    /// <summary>
    /// Signed 1/256-pixel acceleration a jump adds to <see cref="Velocity"/> every frame until it lands. Only the Y axis is
    /// meaningful; X stays zero, so adding the whole vector leaves the horizontal velocity unchanged.
    /// </summary>
    public EightEightVector<RoomPixels> JumpAcceleration { get; internal set; }

    /// <summary>The main phase the Torizo runs each frame; it starts settling under gravity as it wakes.</summary>
    public TorizoPhase Phase { get; internal set; } = TorizoPhase.Falling;

    /// <summary>How the body moves itself each update while it fights, or null while it holds still (#1931).</summary>
    internal TorizoBodyMotion? Motion { get; set; }

    /// <summary>The definition of this Torizo, Bomb or Golden, whose values its motions read (#1931).</summary>
    internal TorizoInitializationDefinition Definition =>
        IsGolden ? TorizoInitializationDefinitions.Golden : TorizoInitializationDefinitions.Bomb;

    /// <summary>Latch preventing repeat entry into the death script after lethal damage; remains set for this state instance.</summary>
    public bool DeathStarted { get; internal set; }

    /// <summary>Counts one stepping cue.</summary>
    internal void CountDecisionStep() => DecisionCounter++;

    /// <summary>Counts one more consecutive space-jumping frame, wrapping from 65535 to zero.</summary>
    internal void CountSamusSpaceJumpFrame() => SamusSpaceJumpFrames = WrappingCounter.IncrementCount(SamusSpaceJumpFrames);

    /// <summary>Sets the Torizo falling at <see cref="TorizoMotionDefinitions.RestingFallSpeed"/>: it rests until its vertical speed next changes.</summary>
    internal void FallAtRestingSpeed()
    {
        Velocity = Velocity.WithY(TorizoMotionDefinitions.RestingFallSpeed);
        FallsAtRestingSpeed = true;
    }
}
