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
    /// <summary>The swooping part 1 animation.</summary>
    SwoopingPart1,
    /// <summary>The swooping part 2 animation.</summary>
    SwoopingPart2,
    /// <summary>The swoop cooldown animation.</summary>
    SwoopCooldown,
}
