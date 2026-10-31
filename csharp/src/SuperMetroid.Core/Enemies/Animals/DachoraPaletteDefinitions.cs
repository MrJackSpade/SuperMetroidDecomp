namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Dachora's packed palette-animation word: the high byte is the palette index, the low byte its timer.</summary>
internal static class DachoraPaletteDefinitions
{
    /// <summary>One speed-palette step: the next palette and sixteen timer ticks.</summary>
    internal const ushort SpeedStep = 272;

    /// <summary>The speed palette cycles back once the word reaches this.</summary>
    internal const ushort SpeedEnd = 1040;

    /// <summary>The speed palette cycle's restart word.</summary>
    internal const ushort SpeedRestart = 784;

    /// <summary>One shine-palette step: the next palette.</summary>
    internal const ushort ShineStep = 256;

    /// <summary>The shine palette ends once the word reaches this.</summary>
    internal const ushort ShineEnd = 1024;
}
