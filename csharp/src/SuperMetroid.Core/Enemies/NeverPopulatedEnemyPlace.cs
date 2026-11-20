namespace SuperMetroid.Core.Enemies;

/// <summary>The history of a place no enemy has been spawned in since its room loaded (#2558).</summary>
public sealed class NeverPopulatedEnemyPlace : EnemyPlaceHistory
{
    /// <summary>The one history every unpopulated place shares.</summary>
    internal static NeverPopulatedEnemyPlace Instance { get; } = new();

    /// <summary>Creates the shared history.</summary>
    private NeverPopulatedEnemyPlace()
    {
    }
}
