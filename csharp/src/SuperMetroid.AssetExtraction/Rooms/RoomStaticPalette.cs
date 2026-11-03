using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The base background colours one room graphics-set palette source gives a room load (#1464, #1512), in the
/// cartridge's order, which the installed palette keeps. Only the import pipeline reads them, to pre-colour room sheets.
/// </summary>
public sealed class RoomStaticPalette
{
    /// <summary>The background colours, in the cartridge's order.</summary>
    private readonly RgbColor[] colours;

    /// <summary>Holds the colours the import pipeline read from the installed palette.</summary>
    /// <param name="colours">The background colours a room load gives, in the cartridge's order.</param>
    internal RoomStaticPalette(RgbColor[] colours) => this.colours = Ensure.NotNull(colours);

    /// <summary>The background colours a room load gives, in the cartridge's order.</summary>
    public ReadOnlySpan<RgbColor> Colours => colours;
}
