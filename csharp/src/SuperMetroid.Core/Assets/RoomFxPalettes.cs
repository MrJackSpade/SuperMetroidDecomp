namespace SuperMetroid.Core.Assets;

/// <summary>Typed view of the room-FX palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: room-FX palettes")]
public readonly struct RoomFxPalettes(SnesCgram cgram)
{
    /// <summary>The layer-3 blend colours.</summary>
    public CgramPalette Layer3Blend => new(cgram, RoomFxPaletteLayout.Layer3Blend);

    /// <summary>The cleared layer-3 blend colour.</summary>
    public CgramPalette Layer3Empty => new(cgram, RoomFxPaletteLayout.Layer3Empty);
}
