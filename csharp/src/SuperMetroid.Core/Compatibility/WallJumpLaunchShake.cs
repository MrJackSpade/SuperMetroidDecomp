namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The solid enemy Samus's latest wall jump launched from (#2562), which the next update's enemies shake: a later check of
/// the same update replaces it or withdraws it, and a room change withdraws it.
/// </summary>
/// <remarks>
/// Evidence: the game's wall jump records the enemy it kicked off, and the enemy processing of the next update, which runs
/// before Samus's movement, shakes that enemy and clears the record.
/// Lifetime: Samus's, naming an enemy of the room it was made in.
/// </remarks>
/// <typeparam name="TEnemy">The solid enemy launched from.</typeparam>
[CompatibilityService("the enemy a wall jump launched from shakes at the next update's enemies")]
public sealed partial class WallJumpLaunchShake<TEnemy>
    where TEnemy : class
{
    /// <summary>The enemy launched from, or null when no launch from an enemy waits.</summary>
    public TEnemy? LaunchedFrom { get; private set; }

    /// <summary>A wall jump launches from <paramref name="enemy"/>.</summary>
    internal void Launch(TEnemy enemy) => LaunchedFrom = Ensure.NotNull(enemy);

    /// <summary>A check that does not launch, or a room change, withdraws the launch.</summary>
    internal void Withdraw() => LaunchedFrom = null;

    /// <summary>The next update's enemies take the launch: the enemy to shake, or null.</summary>
    internal TEnemy? Take()
    {
        TEnemy? enemy = LaunchedFrom;
        LaunchedFrom = null;
        return enemy;
    }
}
