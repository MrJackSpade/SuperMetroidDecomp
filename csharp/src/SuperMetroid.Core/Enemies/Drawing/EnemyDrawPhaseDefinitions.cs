namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// The counter phases that time every enemy's hurt flash, frozen blink and draw shake (#1606), and how long a wall jump
/// shakes the enemy it launched from. The common enemy draw reads them, and so do the bosses and species that draw their
/// own flash or blink the same way.
/// </summary>
internal static class EnemyDrawPhaseDefinitions
{
    /// <summary>How many of its draws a solid or frozen enemy shakes after Samus wall-jumps off it.</summary>
    internal const int WallJumpLaunchShakeDraws = 64;

    /// <summary>
    /// The phase of the enemy-pass counter on which a hurt enemy shows the hurt flash: set for two passes of every four,
    /// while its own colours show for the other two.
    /// </summary>
    internal static readonly UpdateCounterField HurtFlashShown = new(1, 1);

    /// <summary>
    /// The phase of a thawing enemy's frozen timer on which it still shows its frozen colours: set for two updates of
    /// every four of the thaw's final stretch, while its own colours show for the other two.
    /// </summary>
    internal static readonly UpdateCounterField FrozenColoursShown = new(1, 1);

    /// <summary>
    /// The phase of a shaking enemy's own update count on which its draw shifts one pixel left; while clear it shifts
    /// one pixel right.
    /// </summary>
    internal static readonly UpdateCounterField ShakeShiftsLeft = new(1, 1);
}
