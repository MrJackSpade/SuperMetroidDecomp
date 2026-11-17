namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="RioAnimations"/>.
/// </summary>
internal enum RioSegment
{
    /// <summary>Hovering with its legs tucked as its wings beat, then letting its legs hang, repeating.</summary>
    Idle,
    /// <summary>
    /// Hovering after a swoop with its legs tucked as its wings beat, then letting them hang, repeating.
    /// </summary>
    PostSwoopIdle,
    /// <summary>The opening of the swoop, which holds its last pose until it finishes.</summary>
    SwoopOpening,
    /// <summary>The swoop's looping wing beat.</summary>
    SwoopLoop,
    /// <summary>
    /// Pulling out of its swoop: its claws curled, then its legs spreading and hanging as its wings beat.
    /// </summary>
    SwoopCooldown,
}
