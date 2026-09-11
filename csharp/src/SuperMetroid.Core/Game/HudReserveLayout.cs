namespace SuperMetroid.Core.Game;

/// <summary>Native reserve indicator definitions shared by pause controls and HUD drawing.</summary>
internal static class HudReserveLayout
{
    /// <summary>$80:998B, Tilemap_HUD_autoReserve; six words followed by the empty variant.</summary>
    public const int AutoTable = 0x80998b;
    /// <summary>$82:AF33, blank tile word written when AUTO changes to MANUAL.</summary>
    public const ushort Blank = 0x2c0f;
    /// <summary>$82:AF36-$AF4A, two AUTO cells in each mutable HUD row, relative to $7E:C608.</summary>
    public static ReadOnlySpan<int> TileIndices => [8, 9, 40, 41, 72, 73];
}
