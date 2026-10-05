namespace SuperMetroid.Core.Assets;

/// <summary>Native $90:AC8D beam-character upload geometry, separate from projectile mechanics.</summary>
public static class BeamTileAtlasDefinitions
{
    public const int Width = 64;
    public const int Height = 8;
    public const int ByteCount = 256;
    /// <summary>$90:AC8D writes eight 4-bpp tiles to VRAM word $6300.</summary>
    public const ushort DestinationWord = 0x6300;
    /// <summary>$90:C3B1 contains twelve legal beam-combination pointers before palette data.</summary>
    public const int SelectionCount = 12;
    /// <summary>$9A:F200, Tiles_PowerBeam; native power-beam character source.</summary>
    public const int PowerSource = 0x9af200;
    /// <summary>$9A:F400, Tiles_IceBeam; native ice-beam character source.</summary>
    public const int IceSource = 0x9af400;
    /// <summary>$9A:F600, Tiles_WaveBeam; native wave and ice/wave character source.</summary>
    public const int WaveSource = 0x9af600;
    /// <summary>$9A:F800, Tiles_PlasmaBeam; shared native plasma-combination character source.</summary>
    public const int PlasmaSource = 0x9af800;
    /// <summary>$9A:FA00, Tiles_Spazer; shared native Spazer-combination character source.</summary>
    public const int SpazerSource = 0x9afa00;

    /// <summary>$90:C3B1, BeamTilesPointers; resolves a legacy source to its base selection.
    /// Native shared sheets do not encode the independently editable combination identity.</summary>
    public static int LegacySelectionFor(int sourceAddress) => sourceAddress switch
    {
        PowerSource => 0, WaveSource => 1, IceSource => 2,
        SpazerSource => 4, PlasmaSource => 8, _ => -1,
    };

    public static string FileName(int selection)
    {
        if ((uint)selection >= SelectionCount) throw new ArgumentOutOfRangeException(nameof(selection));
        return $"beam-{selection:X2}-tiles.png";
    }
}
