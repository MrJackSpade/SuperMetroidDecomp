namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations (#641): the wingbeats its hover plays and one segment per swoop animation a
/// branch enters, in authored order. Their steps are <see cref="RioAnimations"/>.
/// </summary>
internal enum RioSegment
{
    /// <summary>
    /// A wingbeat with its legs tucked (#2451, #2512): the idle starts with one, and the hover, which the idle runs on into
    /// and a swoop ends in, plays it over (<see cref="RioAnimations.TuckedWingbeats"/>) before its legs hang.
    /// </summary>
    TuckedWingbeat,
    /// <summary>
    /// The hover's wingbeat with its legs hanging, which it plays over (<see cref="RioAnimations.HangingWingbeats"/>) before the idle starts again (#2451).
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
