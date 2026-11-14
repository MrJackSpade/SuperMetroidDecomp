namespace SuperMetroid.Core.Enemies;

/// <summary>What an enemy does while it is hurt and flashing.</summary>
public enum EnemyHurtResponse
{
    /// <summary>
    /// The species' own hurt behaviour where the species has one; otherwise the enemy's frozen or ordinary behaviour runs
    /// while it flashes.
    /// </summary>
    SpeciesOwn,

    /// <summary>The enemy holds still: nothing runs until the flash ends.</summary>
    Inert,
}
