namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Rio's idle, swoop and cooldown animations (#641): one segment per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="RioAnimations"/>.
/// </summary>
internal enum RioSegment
{
    /// <summary>The idle animation.</summary>
    Idle,
    /// <summary>The post swoop idle animation.</summary>
    PostSwoopIdle,
    /// <summary>The opening of the swoop, which holds its last pose until it finishes.</summary>
    SwoopOpening,
    /// <summary>The swoop's looping wing beat.</summary>
    SwoopLoop,
    /// <summary>The swoop cooldown animation.</summary>
    SwoopCooldown,
}
