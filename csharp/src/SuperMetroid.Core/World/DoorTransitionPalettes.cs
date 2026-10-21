using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.World;

/// <summary>
/// Typed view of the live CGRAM colours GameState_A_LoadingNextRoom writes directly during a door
/// transition (#630): the beam palette <c>$90:ACCD</c> restores and the visor colour
/// <c>$82:E52B-$E52E</c> resets when door scrolling completes.
/// </summary>
[PpuLayoutBoundary("rendering view: door transition live palette writes")]
public readonly struct DoorTransitionPalettes(IPaletteColors cgram)
{
    /// <summary>Copies the beam colours from <paramref name="cgramImage"/>, a whole CGRAM image in colour order.</summary>
    public void RestoreBeamFrom(ReadOnlySpan<Bgr555> cgramImage)
    {
        var beam = new CgramPalette(cgram, SamusPaletteLayout.Beam);
        beam.CopyFrom(cgramImage.Slice(SamusPaletteLayout.Beam.Offset, beam.Count));
    }

    /// <summary>The single visor colour.</summary>
    public CgramPalette Visor => new(cgram, SamusPaletteLayout.Visor);
}
