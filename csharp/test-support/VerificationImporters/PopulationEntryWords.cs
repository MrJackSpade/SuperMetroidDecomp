namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The seven words of a native population entry after its definition pointer (#1472).</summary>
/// <param name="X">The room column.</param>
/// <param name="Y">The room row.</param>
/// <param name="Initialization">The initialization word.</param>
/// <param name="Properties">The property word.</param>
/// <param name="ExtraProperties">The extra-property word.</param>
/// <param name="Parameter1">The first parameter word.</param>
/// <param name="Parameter2">The second parameter word.</param>
internal readonly record struct PopulationEntryWords(
    int X, int Y, int Initialization, int Properties, int ExtraProperties, int Parameter1, int Parameter2);
