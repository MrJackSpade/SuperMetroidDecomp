using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Editable Ceres Ridley Mode-7 zoom shades, separate from movement and rotation.</summary>
public sealed class CeresRidleyFloorColorCatalog
{
    /// <summary>Immutable decoded shade rows indexed by the native zoom word's high byte.</summary>
    internal readonly CeresRidleyFloorPaintDefinitions rows;

    /// <summary>Copies validated RGB5 rows into the fixed zoom-shade selector.</summary>
    internal CeresRidleyFloorColorCatalog(Bgr555[][] rows) => this.rows = new(rows);

    /// <summary>Reads an authored color by the high byte of the native zoom word.</summary>
    public Bgr555 Resolve(int zoomHighByte, int color)
    {
        if ((uint)zoomHighByte >= CeresRidleyPaletteDefinitions.GetawayZoomRowCount)
            throw new ArgumentOutOfRangeException(nameof(zoomHighByte));
        return rows.Resolve(zoomHighByte, color);
    }

    /// <summary>Reproduces $A6:B0EF's fifteen-color zoom shade write to CGRAM 81..95, preserving BG palette 5 color zero and leaving zoom/rotation mechanics unchanged.</summary>
    /// <param name="colours">The colours to update.</param>
    /// <param name="zoomHighByte">High byte of the current native zoom word, 0..8; not a frame timer or byte offset into the stored rows.</param>
    /// <exception cref="ArgumentOutOfRangeException">The zoom row is outside 0..8.</exception>
    public void Apply(GameplayColours colours, int zoomHighByte)
    {
        Ensure.NotNull(colours);
        _ = Resolve(zoomHighByte, 0);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.GetawayZoomColorCount; color++)
            new RidleyColours(colours).CeresFloorZoom.Write(color, rows.Resolve(zoomHighByte, color));
    }
}
