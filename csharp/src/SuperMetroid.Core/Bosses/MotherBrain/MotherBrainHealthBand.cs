namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One of Mother Brain's four health bands (#2259): the damage colours her body, brain and rear legs show once her corpse
/// has revived. She is in the highest band whose <see cref="MinimumHealth"/> her health reaches, and in
/// <see cref="MotherBrainHealthBandDefinitions.NearlyDead"/> below them all.
/// </summary>
public sealed partial class MotherBrainHealthBand
{
    /// <summary>Defines a band.</summary>
    /// <param name="minimumHealth">The least health in the band: zero for the lowest band, which holds all the health below the others.</param>
    /// <param name="key">The band's name in its shade's name.</param>
    internal MotherBrainHealthBand(int minimumHealth, [InstalledKey] string key)
    {
        MinimumHealth = minimumHealth;
        Key = key;
    }

    /// <summary>The least health in the band: zero for the lowest band, which holds all the health below the others.</summary>
    public int MinimumHealth { get; }

    /// <summary>The band's name in its shade's name.</summary>
    public string Key { get; }

    /// <summary>
    /// The band Mother Brain is in at <paramref name="health"/>. Whether her corpse has revived is decided by the fight; this
    /// selects the band every time without checking the encounter's phase.
    /// </summary>
    public static MotherBrainHealthBand Of(int health) =>
        MotherBrainHealthBandDefinitions.HighestFirst.First(band => health >= band.MinimumHealth);

    /// <inheritdoc/>
    public override string ToString() => Key;
}
