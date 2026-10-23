namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Cartridge values and helpers of <see cref="MotherBrainRainbowPaletteRomData"/> that only verification reads.</summary>
internal static class MotherBrainRainbowPaletteRomDataConstants
{
    /// <summary>$A9:BD1D byte destination $82, body background colors.</summary>
    public const int BodyColor = 0x41;
    /// <summary>$A9:BD1D byte destination $122, brain and neck sprite colors.</summary>
    public const int BrainColor = 0x91;
}
