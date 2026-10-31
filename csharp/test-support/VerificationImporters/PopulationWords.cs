namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The words of a native population entry that carry its species' settings (#1472), as
/// <see cref="EnemySpawnSettingsCodec"/> reads and writes them.
/// </summary>
/// <param name="Initialization">The initialization word, which a few species read as packed settings.</param>
/// <param name="SpeciesPropertyBits">The low byte of the property word, which crawlers read as the side of their block.</param>
/// <param name="ExtraProperties">The extra-property word, which shutters read as settings.</param>
/// <param name="Parameter1">The first parameter word.</param>
/// <param name="Parameter2">The second parameter word.</param>
internal readonly record struct PopulationWords(
    int Initialization, int SpeciesPropertyBits, int ExtraProperties, int Parameter1, int Parameter2)
{
    /// <summary>Words that carry only the two parameters.</summary>
    public static PopulationWords Parameters(int parameter1, int parameter2) => new(0, 0, 0, parameter1, parameter2);
}
