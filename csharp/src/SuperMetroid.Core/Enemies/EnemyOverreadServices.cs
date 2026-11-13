namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The compatibility services that reproduce what enemies read past the end of their own data
/// (#1500), as the import pipeline decoded them from the player's game data. The host installs them on the enemy
/// session with the rest of the installed content; an enemy consults its service only on the path that reads past its
/// data. Lifetime: the installation's, shared by every room of the session.
/// </summary>
public sealed class EnemyOverreadServices
{
    /// <summary>Keeps every decoded service.</summary>
    /// <param name="magdolliteApexGrowth">Whether a Magdollite arm grows a final segment at its full height, by its slot.</param>
    /// <param name="fallingSparkUnauthoredLaunch">The drift a falling spark takes on the random choice past its authored launches.</param>
    internal EnemyOverreadServices(MagdolliteApexGrowth magdolliteApexGrowth, FallingSparkUnauthoredLaunch fallingSparkUnauthoredLaunch)
    {
        MagdolliteApexGrowth = Ensure.NotNull(magdolliteApexGrowth);
        FallingSparkUnauthoredLaunch = Ensure.NotNull(fallingSparkUnauthoredLaunch);
    }

    /// <summary>Whether a Magdollite arm grows one final segment on reaching its full height, by the slot it occupies.</summary>
    internal MagdolliteApexGrowth MagdolliteApexGrowth { get; }

    /// <summary>The drift a falling spark takes when its random launch choice lies past the authored launches.</summary>
    internal FallingSparkUnauthoredLaunch FallingSparkUnauthoredLaunch { get; }
}
