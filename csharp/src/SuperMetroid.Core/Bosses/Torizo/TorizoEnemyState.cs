using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Torizo's state beyond what every enemy has. The Torizo's entity owns it as its species state (#2030).
/// </summary>
public sealed class TorizoEnemyState
{
    /// <summary>Creates the state of a Torizo of <paramref name="variant"/>, Bomb or Golden Torizo.</summary>
    internal TorizoEnemyState(TorizoVariant variant)
    {
        Variant = Ensure.NotNull(variant);
    }

    /// <summary>Which way Torizo faces, which foot leads and whether it is turning around (#2237).</summary>
    internal TorizoPosture Posture { get; set; }

    /// <summary>Whether Golden Torizo has caught a Super Missile; later shots bypass the catch and stun him.</summary>
    internal bool CaughtSuperMissile { get; set; }

    /// <summary>
    /// Whether Golden Torizo is stunned: a Super Missile hit or another family's shot stuns him, his stun-health interrupt
    /// reads it, and his stun animation's unmark cue clears it.
    /// </summary>
    internal bool Stunned { get; set; }

    /// <summary>Whether Bomb Torizo's face is blown up; its faceless animations walk on from here, and its death sets it.</summary>
    internal bool FaceBlownUp { get; set; }

    /// <summary>
    /// How the Torizo walks as it now is (#2472): its own walk with its face, and Bomb Torizo's faceless walk once its face
    /// is blown up. Its turn toward a Samus behind it is this walk's turn about.
    /// </summary>
    internal TorizoWalker Walker => FaceBlownUp ? TorizoWalkerDefinitions.FacelessBombTorizo : Variant.Walker;

    /// <summary>Whether Bomb Torizo's gut is blown up, which its first low-health interruption sets, and its death too.</summary>
    internal bool GutBlownUp { get; set; }
    /// <summary>Which Torizo this is, Bomb or Golden Torizo, whose values its fight reads (#1931, #2410); fixed at initialization.</summary>
    internal TorizoVariant Variant { get; }

    /// <summary>His colours: the colouring shown, the fade his awakening or death runs toward it and his belly's glow (#2236).</summary>
    internal TorizoColours Colours { get; } = new();

    /// <summary>The body animation playing and where it stands in it (#1600).</summary>
    internal AnimationPlayback<TorizoBodySegment> BodyPlayback { get; set; }

    /// <summary>
    /// The animation the Torizo's jump or fall continues in, as the jump or fall names it (#2028, #2411): a jump's fall, which
    /// its rise ends in, or a fall's landing. Every jump and fall names it as it starts to leap, before its leap moves it.
    /// </summary>
    internal TorizoAnimation Landing { get; set; }

    /// <summary>
    /// Where an attack or a stagger interrupted the Torizo's walk, which resumes there when it ends (#2028): only an attack
    /// that interrupted the walk resumes it.
    /// </summary>
    internal AnimationPlayback<TorizoBodySegment> InterruptedWalk { get; set; }

    /// <summary>Where the gut explosion interrupted the body's play, resumed afterwards.</summary>
    internal AnimationPlayback<TorizoBodySegment> InterruptedPlayback { get; set; }

    /// <summary>
    /// The wait before the walking Torizo turns toward Samus, or null while no turn is due (#1931, #2410): a walking step
    /// loads it when Samus is behind the Torizo, a wall in its way clears it, and its walking motion steps it.
    /// </summary>
    public BossCountdown? TurnTowardSamus { get; internal set; }

    /// <summary>
    /// Whether the body's animation is locked: the awakening animation locks it and
    /// later unlocks it. While it is locked Bomb Torizo rejects shots before common damage, and Golden Torizo sends
    /// them to common damage without its catches or its stun. Every test is locked versus unlocked.
    /// </summary>
    public bool AnimationLocked { get; internal set; }

    /// <summary>
    /// Stepping cues counted since a decision last reset it. Golden Torizo's repeated-jump decision fires once it
    /// reaches <see cref="TorizoDecisionDefinitions.RepeatedJumpSteps"/> and resets it, and that decision follows every
    /// one of its stepping cues; Bomb Torizo never reads it (#1945).
    /// </summary>
    public int DecisionCounter { get; internal set; }

    /// <summary>
    /// The eggs Golden Torizo has laid that may remain (#2332): its egg-laying run repeats while any of them is still an
    /// egg. An egg that hatches into nothing or breaks leaves its place, which another projectile may take.
    /// </summary>
    internal List<EnemyProjectile> LaidEggs { get; } = [];

    /// <summary>Whether any egg Golden Torizo laid remains; the eggs that have gone are forgotten.</summary>
    internal bool EggsRemain()
    {
        LaidEggs.RemoveAll(egg => egg.Kind != RoomEnemyProjectileKind.GoldenTorizoEgg);
        return LaidEggs.Count > 0;
    }

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
    /// The velocity of the Torizo's jumps and falls, in pixels per update. A walking step moves the Torizo by its own whole pixels and
    /// leaves its sideways velocity at <see cref="TorizoWalkLeftVelocity"/>. Any change to its vertical speed ends the
    /// resting fall (<see cref="FallsAtRestingSpeed"/>).
    /// </summary>
    public PixelVelocity<RoomPixels> Velocity
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
    /// lands without a quake. Only that fall moves at that speed exactly: gravity and the leaps add five thirty-seconds of a pixel an update
    /// to a rest or to a leap's rise, neither of which comes back to it.
    /// </summary>
    internal bool FallsAtRestingSpeed { get; private set; }

    /// <summary>
    /// The downward acceleration a jump adds to <see cref="Velocity"/>'s vertical speed every frame until it
    /// lands (#2235): one axis, since a jump never changes its sideways speed.
    /// </summary>
    public SignedEightEight JumpAcceleration { get; internal set; }

    /// <summary>The main phase the Torizo runs each frame; it starts settling under gravity as it wakes.</summary>
    public TorizoPhase Phase { get; internal set; } = TorizoPhase.Falling;

    /// <summary>How the body moves itself each update while it fights, or null while it holds still (#1931).</summary>
    internal TorizoBodyMotion? Motion { get; set; }

    /// <summary>Whether the Torizo has started dying: lethal damage starts its death once, and it stays dying while it lasts.</summary>
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
