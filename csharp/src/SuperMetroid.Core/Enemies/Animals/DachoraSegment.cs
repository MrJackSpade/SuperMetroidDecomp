namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The Dachora's run, idle, blink, shinespark charge, echo and fall animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="DachoraAnimations"/>.
/// </summary>
internal enum DachoraSegment
{
    /// <summary>Running left, its far and near legs swinging back, passing and reaching in turn, repeating.</summary>
    RunningLeft,
    /// <summary>Running left with the same stride at a quicker step, repeating.</summary>
    RunningLeftFast,
    /// <summary>Running left with the same stride at its quickest, a new pose every update, repeating.</summary>
    RunningLeftVeryFast,
    /// <summary>
    /// Standing facing left with its head up, then stretching its neck and pecking at the ground twice, repeating.
    /// </summary>
    IdleLeft,
    /// <summary>
    /// Standing facing left with its head up, its eye half shutting and shutting three times, repeating.
    /// </summary>
    BlinkLeft,
    /// <summary>A speed-boost echo trailing the Dachora facing left, held stretched up.</summary>
    EchoLeft,
    /// <summary>Held mid-stride facing left, its near leg passing, while it falls.</summary>
    FallingLeft,
    /// <summary>Running right, its far and near legs swinging back, passing and reaching in turn, repeating.</summary>
    RunningRight,
    /// <summary>Running right with the same stride at a quicker step, repeating.</summary>
    RunningRightFast,
    /// <summary>Running right with the same stride at its quickest, a new pose every update, repeating.</summary>
    RunningRightVeryFast,
    /// <summary>
    /// Standing facing right with its head up, then stretching its neck and pecking at the ground twice, repeating.
    /// </summary>
    IdleRight,
    /// <summary>
    /// Standing facing right with its head up, its eye half shutting and shutting three times, repeating.
    /// </summary>
    BlinkRight,
    /// <summary>Crouched facing right, held while it charges its shinespark.</summary>
    ChargeRight,
    /// <summary>A speed-boost echo trailing the Dachora facing right, held stretched up.</summary>
    EchoRight,
    /// <summary>Held mid-stride facing right, its near leg passing, while it falls.</summary>
    FallingRight,
}
