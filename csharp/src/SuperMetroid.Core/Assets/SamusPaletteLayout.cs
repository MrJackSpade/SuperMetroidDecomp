using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>CGRAM ranges written by Samus, beam, room-FX and menu palette routines (#630).</summary>
[PpuLayoutBoundary("catalog: Samus, beam, room-FX and menu palettes")]
public static class SamusCgramLayout
{
    /// <summary>CGADD $C0-$CF: OBJ palette 4, Samus's suit colours (suit, charge, Hyper and full-body cycles).</summary>
    public static readonly PpuLayoutCell Suit = PpuMemoryLayout.Colors(nameof(Suit),
        SamusPaletteRomData.Common.SamusObjPaletteStart, PpuMemoryLayout.ColorsPerRow, "Bgr555 OBJ palette 4");

    /// <summary>CGADD $E0-$EF: OBJ palette 6, the beam palette loaded by <c>$90:ACCD</c>.</summary>
    public static readonly PpuLayoutCell Beam = PpuMemoryLayout.Colors(nameof(Beam),
        SamusProjectileRomData.Palettes.BeamDestinationIndex, BeamPaletteDefinitions.ColorCount, "Bgr555 OBJ palette 6");

    /// <summary>CGADD $E0-$E9: the Crystal Flash body colours in OBJ palette 6.</summary>
    public static readonly PpuLayoutCell CrystalFlashBody = PpuMemoryLayout.Colors(nameof(CrystalFlashBody),
        SamusPaletteRomData.CrystalFlash.BodyCgramStart, CrystalFlashColorFormat.BodyColorCount, "Bgr555 Crystal Flash body");

    /// <summary>CGADD $EA-$EF: the Crystal Flash bubble colours in OBJ palette 6.</summary>
    public static readonly PpuLayoutCell CrystalFlashBubble = PpuMemoryLayout.Colors(nameof(CrystalFlashBubble),
        SamusPaletteRomData.CrystalFlash.BubbleCgramStart, CrystalFlashColorFormat.BubbleColorCount, "Bgr555 Crystal Flash bubble");

    /// <summary>CGADD $19-$1B: BG3 palette 6, the three layer-3 FX blend colours.</summary>
    public static readonly PpuLayoutCell Layer3Blend = PpuMemoryLayout.Colors(nameof(Layer3Blend),
        RoomFxRomData.Layer3.PaletteBlendDestinationIndex, RoomFxRomData.Layer3.PaletteBlendColorCount, "Bgr555 layer-3 blend");

    /// <summary>CGADD $1B: the layer-3 blend colour a blend without a third colour clears to black.</summary>
    public static readonly PpuLayoutCell Layer3Empty = PpuMemoryLayout.Colors(nameof(Layer3Empty),
        RoomFxRomData.Layer3.EmptyPaletteColorIndex, 1, "Bgr555 layer-3 blend colour");

    /// <summary>CGADD $C0-$CF: OBJ palette 4, the game-over Baby Metroid colours.</summary>
    public static readonly PpuLayoutCell GameOverBaby = PpuMemoryLayout.Colors(nameof(GameOverBaby),
        GameOverRomData.BabyAnimation.PaletteDestinationIndex, GameOverRomData.BabyAnimation.PaletteColorCount, "Bgr555 OBJ palette 4");
}

/// <summary>Typed view of Samus's and the beam's palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: Samus and beam palettes")]
public readonly struct SamusPalettes(SnesCgram cgram)
{
    /// <summary>Samus's suit palette.</summary>
    public CgramPalette Suit => new(cgram, SamusCgramLayout.Suit);
    /// <summary>The beam palette.</summary>
    public CgramPalette Beam => new(cgram, SamusCgramLayout.Beam);
    /// <summary>The Crystal Flash body colours.</summary>
    public CgramPalette CrystalFlashBody => new(cgram, SamusCgramLayout.CrystalFlashBody);
    /// <summary>The Crystal Flash bubble colours.</summary>
    public CgramPalette CrystalFlashBubble => new(cgram, SamusCgramLayout.CrystalFlashBubble);
}

/// <summary>Typed view of the room-FX palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: room-FX palettes")]
public readonly struct RoomFxPalettes(SnesCgram cgram)
{
    /// <summary>The layer-3 blend colours.</summary>
    public CgramPalette Layer3Blend => new(cgram, SamusCgramLayout.Layer3Blend);
    /// <summary>The cleared layer-3 blend colour.</summary>
    public CgramPalette Layer3Empty => new(cgram, SamusCgramLayout.Layer3Empty);
}

/// <summary>Typed view of the game-over palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: game-over palettes")]
public readonly struct GameOverPalettes(SnesCgram cgram)
{
    /// <summary>The Baby Metroid colours.</summary>
    public CgramPalette Baby => new(cgram, SamusCgramLayout.GameOverBaby);
}
