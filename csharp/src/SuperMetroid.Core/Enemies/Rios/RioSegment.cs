namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="RioAnimations"/>.
/// </summary>
internal enum RioSegment
{
    /// <summary>
    /// Hovering: one wingbeat with its legs tucked, before the hover a swoop ends in, which the idle repeats (#2451).
    /// </summary>
    Idle,
    /// <summary>
    /// Hovering after a swoop: a wingbeat with its legs tucked, which the hover plays three times before its legs hang
    /// (#2451).
    /// </summary>
    PostSwoopIdle,
    /// <summary>
    /// The hover's wingbeat with its legs hanging, which it plays twice before the idle starts again (#2451).
    /// </summary>
    HangingWingbeat,
    /// <summary>The opening of the swoop, which holds its last pose until it finishes.</summary>
    SwoopOpening,
    /// <summary>The swoop's looping wing beat.</summary>
    SwoopLoop,
    /// <summary>
    /// Pulling out of its swoop: its claws curled, then its legs spreading and hanging as its wings beat.
    /// </summary>
    SwoopCooldown,
}
