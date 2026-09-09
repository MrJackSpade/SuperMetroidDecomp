namespace SuperMetroid.Core.Game;

/// <summary>Native $AD:E3D5 damage-dependent palettes for Mother Brain's final phase.</summary>
public static class MotherBrainHealthPaletteRomData
{
    /// <summary>$AD:E6A2: four body/brain palette pointers.</summary>
    public const int BrainTable = 0xade6a2;
    /// <summary>$AD:E742: four back-leg palette pointers.</summary>
    public const int BackLegTable = 0xade742;
    /// <summary>$AD:E3E4 first strict health threshold ($2328).</summary>
    public const ushort FirstThreshold = 9000;
    /// <summary>$AD:E3EE second strict health threshold ($1518).</summary>
    public const ushort SecondThreshold = 5400;
    /// <summary>$AD:E3F8 final strict health threshold ($0708).</summary>
    public const ushort FinalThreshold = 1800;
}
