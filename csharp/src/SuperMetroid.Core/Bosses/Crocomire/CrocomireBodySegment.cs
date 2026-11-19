namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// The runs of beats Crocomire's body, melting and skeleton animations play (#1938, #2399), each named for what it shows. A
/// run several animations show is one segment they each play: his claw swing opens his projectile attack, his damage wait's
/// moving claws and his closed-mouth Power Bomb reaction, and his gaping mouth beats through his roar and his charge near
/// the spike wall, played as often as each holds it. His fight and death start the animations by
/// <see cref="CrocomireBodyAnimation"/>, never by segment.
/// </summary>
public enum CrocomireBodySegment
{
    /// <summary>His idle sway before the fight starts.</summary>
    IdleSway,
    /// <summary>His claw swing: three swings raising dust, each ending with a big explosion's sound.</summary>
    ClawSwing,
    /// <summary>Opening his mouth with a cry after the claw swing that opens his projectile attack, before his spitting lunges.</summary>
    ProjectileAttackOpenMouth,
    /// <summary>His spitting lunges, each led by the fight cue that spits a shot, looping until his volley is spent.</summary>
    ProjectileAttackSpitting,
    /// <summary>A pause before his step forward.</summary>
    StepForwardAfterDelay,
    /// <summary>
    /// His roaring advance: his mouth opening wide with a cry, then a stride on alternate feet that raises dust and shakes
    /// the screen, his mouth shutting at its end; it may first start his projectile attack.
    /// </summary>
    StepForward,
    /// <summary>The start of his step back.</summary>
    StepBack,
    /// <summary>His step back.</summary>
    SteppingBack,
    /// <summary>Waiting for a hit, standing.</summary>
    AwaitingHit,
    /// <summary>The fight cue after the damage wait's claw swing, which steps him back on a hit before he roars.</summary>
    ClawSwingFightCue,
    /// <summary>His roar opening his mouth with a cry.</summary>
    RoarOpenMouth,
    /// <summary>
    /// His gaping mouth shown for two updates, then the fight cue that steps him back on a hit: his roar and his charge near
    /// the spike wall each play it again and again.
    /// </summary>
    Gaping,
    /// <summary>His roar's long gaping hold, each of its two pictures followed by a fight cue.</summary>
    RoarGapingHold,
    /// <summary>His roar closing his mouth.</summary>
    RoarCloseMouth,
    /// <summary>His closed mouth after the roar, then the fight cue after which he roars again.</summary>
    RoarCloseMouthLoop,
    /// <summary>The Power Bomb reaction struck with his mouth fully open.</summary>
    PowerBombReactionMouthFullyOpen,
    /// <summary>The Power Bomb reaction struck with his mouth partly open.</summary>
    PowerBombReactionMouthPartiallyOpen,
    /// <summary>The Power Bomb reaction's charge: his steps forward, looping while the charge lasts.</summary>
    PowerBombReactionCharge,
    /// <summary>His charge near the spike wall opening his mouth with a cry.</summary>
    NearSpikeWallChargeOpenMouth,
    /// <summary>His charge near the spike wall closing his gaping mouth before his steps toward the wall.</summary>
    NearSpikeWallChargeCloseMouth,
    /// <summary>His charge near the spike wall, its steps toward the wall, looping until he backs off.</summary>
    NearSpikeWallChargeSteps,
    /// <summary>Backing off from the spike wall.</summary>
    BackOffFromSpikeWall,
    /// <summary>His flesh melting off his skull, its top row above the acid.</summary>
    MeltingFleshTopRow,
    /// <summary>His flesh melting off his skull, its top two rows above the acid.</summary>
    MeltingFleshTopTwoRows,
    /// <summary>His flesh melting off his skull, its top three rows above the acid.</summary>
    MeltingFleshTopThreeRows,
    /// <summary>His flesh melting off his skull, its top four rows above the acid.</summary>
    MeltingFleshTopFourRows,
    /// <summary>His bared skull over the rest of him melting, its top row above the acid.</summary>
    MeltingBareSkullTopRow,
    /// <summary>His bared skull over the rest of him melting, its top two rows above the acid.</summary>
    MeltingBareSkullTopTwoRows,
    /// <summary>His bared skull over the rest of him melting, its top three rows above the acid.</summary>
    MeltingBareSkullTopThreeRows,
    /// <summary>His bared skull over the rest of him melting, its top four rows above the acid.</summary>
    MeltingBareSkullTopFourRows,
    /// <summary>Sinking after the bridge collapses under him.</summary>
    BridgeCollapsed,
    /// <summary>His skeleton toppling, leaning and tipping over, ten updates each.</summary>
    SkeletonFalling,
    /// <summary>His skeleton falling apart.</summary>
    SkeletonFallsApart,
    /// <summary>His skeleton at rest after it falls apart.</summary>
    SkeletonStable,
    /// <summary>His dead skeleton.</summary>
    Dead,
    /// <summary>The bubbles his sunken skeleton leaves as it is carried away: forming, bursting and fading, then adrift.</summary>
    SkeletonFlowingDownRiver,
}
