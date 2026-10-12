using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Live CGRAM colours GameState_A_LoadingNextRoom writes directly during a door transition (#630).</summary>
[PpuLayoutBoundary("catalog: door transition live palette writes")]
public static class DoorTransitionPaletteLayout
{
    /// <summary>CGADD $E0-$EF: OBJ palette 6, the beam colours <c>$90:ACCD</c> writes live after restoring the faded source colours.</summary>
    public static readonly PpuLayoutCell BeamPalette = PpuMemoryLayout.Colors(nameof(BeamPalette), 0xe0, 16, "Bgr555 beam palette");

    /// <summary>CGADD $C4: Palettes_SpriteP4C4, the visor colour <c>$82:E52B-$E52E</c> resets when door scrolling completes.</summary>
    public static readonly PpuLayoutCell Visor = PpuMemoryLayout.Colors(nameof(Visor), DoorTransitionPaletteDefinitions.VisorColorIndex, 1,
        "Bgr555 visor colour");
}

/// <summary>Typed view of the door transition's live palette writes (#630).</summary>
[PpuLayoutBoundary("rendering view: door transition live palette writes")]
public readonly struct DoorTransitionPalettes(SnesCgram cgram)
{
    /// <summary>Copies the beam colours from <paramref name="cgramImage"/>, a whole CGRAM image in colour order.</summary>
    public void RestoreBeamFrom(ReadOnlySpan<Bgr555> cgramImage)
    {
        var beam = new CgramPalette(cgram, DoorTransitionPaletteLayout.BeamPalette);
        beam.CopyFrom(cgramImage.Slice(DoorTransitionPaletteLayout.BeamPalette.Offset, beam.Count));
    }

    /// <summary>The single visor colour.</summary>
    public CgramPalette Visor => new(cgram, DoorTransitionPaletteLayout.Visor);
}
