using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports authored visor RGB5 colors without exposing their phase selectors.</summary>
public static class SamusVisorColorExtractor
{
    /// <summary>Exports the six consecutive authored visor colors at $9B:A3C0 used by normal-room animation and X-ray Scope presentation.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for the twelve-byte visor color band.</param>
    /// <returns>A new UTF-8 JSON buffer with six colors in native byte-offset order, represented as RGB5 channels 0..31.</returns>
    /// <remarks>Bit 15 is omitted. This is a single-color selection band, not six complete suit palettes; visor phase/timer selectors and X-ray activation behavior stay compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            SamusVisorColorFormat.SourceAddress,
            SamusVisorColorFormat.ColorCount * sizeof(ushort));
        var colors = new PaletteRgb5[SamusVisorColorFormat.ColorCount];
        for (int index = 0; index < colors.Length; index++)
        {
            ushort word = unchecked((ushort)(source[index * 2] | source[index * 2 + 1] << 8));
            colors[index] = new PaletteRgb5
            {
                Red = word & 31,
                Green = (word >> 5) & 31,
                Blue = (word >> 10) & 31,
            };
        }
        return SamusVisorColorCatalog.Write(new SamusVisorColorDocument
        {
            Version = SamusVisorColorFormat.Version,
            Colors = colors,
        });
    }
}
