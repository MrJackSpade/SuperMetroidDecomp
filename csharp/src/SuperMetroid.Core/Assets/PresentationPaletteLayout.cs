using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>CGRAM ranges the room-FX layer-3 blend routines write (#630).</summary>
[PpuLayoutBoundary("catalog: room-FX layer-3 blend palette")]
public static class RoomFxPaletteLayout
{
    /// <summary>CGADD $19-$1B: BG3 palette 6, the three layer-3 FX blend colours.</summary>
    public static readonly PpuLayoutCell Layer3Blend = PpuMemoryLayout.Colors(nameof(Layer3Blend),
        RoomFxRomData.Layer3.PaletteBlendDestinationIndex, RoomFxRomData.Layer3.PaletteBlendColorCount, "Bgr555 layer-3 blend");

    /// <summary>CGADD $1B: the layer-3 blend colour a blend without a third colour clears to black.</summary>
    public static readonly PpuLayoutCell Layer3Empty = PpuMemoryLayout.Colors(nameof(Layer3Empty),
        RoomFxRomData.Layer3.EmptyPaletteColorIndex, 1, "Bgr555 layer-3 blend colour");
}

/// <summary>Typed view of the room-FX palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: room-FX palettes")]
public readonly struct RoomFxPalettes(SnesCgram cgram)
{
    /// <summary>The layer-3 blend colours.</summary>
    public CgramPalette Layer3Blend => new(cgram, RoomFxPaletteLayout.Layer3Blend);

    /// <summary>The cleared layer-3 blend colour.</summary>
    public CgramPalette Layer3Empty => new(cgram, RoomFxPaletteLayout.Layer3Empty);
}

/// <summary>CGRAM ranges the game-over screen animates (#630).</summary>
[PpuLayoutBoundary("catalog: game-over palette")]
public static class GameOverPaletteLayout
{
    /// <summary>CGADD $C0-$CF: OBJ palette 4, the game-over Baby Metroid colours.</summary>
    public static readonly PpuLayoutCell Baby = PpuMemoryLayout.Colors(nameof(Baby),
        GameOverRomData.BabyAnimation.PaletteDestinationIndex, GameOverRomData.BabyAnimation.PaletteColorCount, "Bgr555 OBJ palette 4");
}

/// <summary>Typed view of the game-over palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: game-over palettes")]
public readonly struct GameOverPalettes(SnesCgram cgram)
{
    /// <summary>The Baby Metroid colours.</summary>
    public CgramPalette Baby => new(cgram, GameOverPaletteLayout.Baby);
}
