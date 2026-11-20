namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The Yapping Maw's eight attack bites and six cooldown animations (#641).
/// Their steps are <see cref="YappingMawAnimations"/>.
/// </summary>
internal enum YappingMawSegment
{
    /// <summary>
    /// Biting up: its jaws wide, then snapping shut with the bite sound and held shut a long while, repeating.
    /// </summary>
    AttackingUp,
    /// <summary>
    /// Biting up and right: its jaws wide, then snapping shut with the bite sound and held shut a long while,
    /// repeating.
    /// </summary>
    AttackingUpRight,
    /// <summary>
    /// Biting right: its jaws wide, then snapping shut with the bite sound and held shut a long while, repeating.
    /// </summary>
    AttackingRight,
    /// <summary>
    /// Biting down and right: its jaws wide, then snapping shut with the bite sound and held shut a long while,
    /// repeating.
    /// </summary>
    AttackingDownRight,
    /// <summary>
    /// Biting down: its jaws wide, then snapping shut with the bite sound and held shut a long while, repeating.
    /// </summary>
    AttackingDown,
    /// <summary>
    /// Biting down and left: its jaws wide, then snapping shut with the bite sound and held shut a long while,
    /// repeating.
    /// </summary>
    AttackingDownLeft,
    /// <summary>
    /// Biting left: its jaws wide, then snapping shut with the bite sound and held shut a long while, repeating.
    /// </summary>
    AttackingLeft,
    /// <summary>
    /// Biting up and left: its jaws wide, then snapping shut with the bite sound and held shut a long while, repeating.
    /// </summary>
    AttackingUpLeft,
    /// <summary>
    /// Holding Samus up and right with its jaws shut, then straight up, then chewing her above it, repeating.
    /// </summary>
    CooldownUpRight,
    /// <summary>Holding Samus straight up, then chewing her above it, repeating.</summary>
    CooldownUp,
    /// <summary>The jaws chewing up on the held Samus, which every cooldown aimed up loops (#2276).</summary>
    ChewingUp,
    /// <summary>
    /// Holding Samus up and left with its jaws shut, then straight up and chewing her above it, repeating.
    /// </summary>
    CooldownUpLeft,
    /// <summary>
    /// Holding Samus down and right with its jaws shut, then straight down, then chewing her below it, repeating.
    /// </summary>
    CooldownDownRight,
    /// <summary>Holding Samus straight down, then chewing her below it, repeating.</summary>
    CooldownDown,
    /// <summary>The jaws chewing down on the held Samus, which every cooldown aimed down loops (#2276).</summary>
    ChewingDown,
    /// <summary>
    /// Holding Samus down and left with its jaws shut, then straight down and chewing her below it, repeating.
    /// </summary>
    CooldownDownLeft,
}
