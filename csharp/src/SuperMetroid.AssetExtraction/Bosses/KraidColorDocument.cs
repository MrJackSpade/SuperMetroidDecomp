using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Editable RGB5 schema for Kraid's room, initial target, body health, sprite health, and death-arm palette sources.</summary>
public sealed record KraidColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="KraidColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen nonnull colors corresponding to $A7:86C7, used as the room-backdrop fade target and installed at CGRAM 96..111 when loading the defeated arena.</summary>
    public required PaletteRgb5[] RoomBackdrop { get; init; }
    /// <summary>Sixteen nonnull colors corresponding to $A7:AAA6's rock sprite palette, staged by initial setup in the host's exposed target-palette band at CGRAM 176..191.</summary>
    public required PaletteRgb5[] InitialTarget { get; init; }
    /// <summary>144 nonnull colors from $A7:B3D3 in nine consecutive sixteen-color bands: white hurt flash followed by eight health bands, selected into BG palette seven at CGRAM 112..127.</summary>
    public required PaletteRgb5[] Health { get; init; }
    /// <summary>144 nonnull colors from $A7:B513 in the same flash/health-band order, selected into sprite palette seven at CGRAM 240..255; may be edited independently of Health.</summary>
    public required PaletteRgb5[] Secondary { get; init; }
    /// <summary>Sixteen nonnull colors corresponding to $A7:B4F3, installed at CGRAM 112..127 for the arm/body death appearance before the death fade.</summary>
    public required PaletteRgb5[] DeathArm { get; init; }
}
