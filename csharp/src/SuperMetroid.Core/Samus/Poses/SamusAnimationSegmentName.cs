namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What a segment of a Samus animation is for (#2048): each segment of an animation has its own name, and a frame of the
/// animation is a frame of one named segment (<see cref="SamusAnimationFrame"/>).
/// </summary>
internal enum SamusAnimationSegmentName
{
    /// <summary>The only segment of an animation that has one.</summary>
    Main,

    /// <summary>The run's stride, which the running cadence times while she has momentum.</summary>
    Run,

    /// <summary>
    /// The running animation's slower stride after the run, ten frames of four and three ticks that the run's own playback
    /// never reaches: only a frame read past an earlier animation lands in it, and every frame such a read reaches shows the
    /// run's last stride.
    /// </summary>
    SlowStride,

    /// <summary>
    /// The running animation's one long frame after the slower stride, which only a read past an earlier animation reaches;
    /// it shows the run's last stride.
    /// </summary>
    StrideHold,

    /// <summary>
    /// The knockback animation's slow recoil after its own frames, four frames that only a read past an earlier animation
    /// would reach; no read reaches them, so they show nothing.
    /// </summary>
    SlowRecoil,

    /// <summary>
    /// The knockback animation's two slower recoil frames after the slow recoil, which no read reaches, so they show nothing.
    /// </summary>
    SlowerRecoil,

    /// <summary>
    /// The knockback animation's one long frame after the slower recoil, which only a read past an earlier animation
    /// reaches; it shows the knockback's held recoil.
    /// </summary>
    RecoilHold,

    /// <summary>
    /// The morph ball animation's frame after the roll, which only a read past an earlier animation reaches; it shows the
    /// roll's last picture and hands on to the roll.
    /// </summary>
    RollHold,

    /// <summary>The drained crouch's collapse, which hands her over to the drained fall at its end.</summary>
    Collapse,

    /// <summary>The drained crouch's fall, held until she lands.</summary>
    Fall,

    /// <summary>The drained crouch held on the floor, from which Up can stand her up.</summary>
    Held,

    /// <summary>The drained stand-up, which ends in the ordinary standing pose.</summary>
    StandUp,

    /// <summary>The left-facing drained crouch's failed attempt to stand, which falls back into the held crouch.</summary>
    FailedStand,

    /// <summary>The failed stand's last frame, which falls back into the held crouch.</summary>
    FailedStandEnd,

    /// <summary>The left-facing drained crouch held still while the Hyper Beam is acquired.</summary>
    Frozen,

    /// <summary>The standing and crouching breathing, which continues into the low-energy breathing on low energy.</summary>
    Breathe,

    /// <summary>The low-energy breathing.</summary>
    LowEnergyBreathe,

    /// <summary>The fall's frames before it gathers speed.</summary>
    FallStart,

    /// <summary>The fast fall's frames, which the movement starts once she falls fast enough.</summary>
    FastFall,

    /// <summary>The aimed fall's first frame, held for a tick before the aim.</summary>
    Aim,

    /// <summary>The aimed fall's aim, held once reached.</summary>
    AimHeld,

    /// <summary>The knockback's own frames.</summary>
    Knockback,

    /// <summary>The ball's roll.</summary>
    Roll,

    /// <summary>The spin's somersault.</summary>
    Spin,

    /// <summary>The spin's frames from which a wall jump can launch.</summary>
    WallJumpReady,

    /// <summary>The wall jump's push off the wall, which selects the spin that follows.</summary>
    Launch,

    /// <summary>The ordinary wall-jump spin.</summary>
    OrdinarySpin,

    /// <summary>The Space Jump wall-jump spin.</summary>
    SpaceJumpSpin,

    /// <summary>The Screw Attack wall-jump spin.</summary>
    ScrewAttackSpin,

    /// <summary>The Crystal Flash's rise.</summary>
    Rise,

    /// <summary>The Crystal Flash's glow while it drains ammunition.</summary>
    Glow,

    /// <summary>The Crystal Flash's finish, which ends in the ordinary standing pose.</summary>
    Finish,

    /// <summary>The front-facing pose's still frame.</summary>
    Still,

    /// <summary>The front-facing pose's appearance at the Zebes start.</summary>
    Appearance,
}
