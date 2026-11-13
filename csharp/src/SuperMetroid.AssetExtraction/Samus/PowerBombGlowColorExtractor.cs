using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports the explosion's glow colours as native RGB5 colors without exporting radius or HDMA mechanics.</summary>
/// <remarks>The one decoder of the cartridge's three-byte fixed-colour triplets: each byte keeps its low five bits.</remarks>
[EncodingOwner("Power Bomb glow-colour triplet")]
public static class PowerBombGlowColorExtractor
{
        /// <summary>The five-bit channel in each stored fixed-colour byte.</summary>
        public const int ComponentMask = 0b1_1111;

    /// <summary>Imports the sixteen pre-explosion and thirty-two explosion glow colours used by Power Bomb effects.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the native three-byte fixed-color sequences.</param>
    /// <returns>New UTF-8 JSON bytes containing ordered RGB5 colors, with each native component masked to 0..31.</returns>
    /// <remarks>Radius-based selection, HDMA geometry, and shared Crystal Flash or Ceres explosion behavior remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return PowerBombGlowColorFile.Write(new PowerBombGlowColorDocument
        {
            Version = PowerBombGlowColorFile.Version,
            PreExplosion = Read(bus, PowerBombGlowSequence.PreExplosion),
            Explosion = Read(bus, PowerBombGlowSequence.Explosion),
        });
    }

    /// <summary>Decodes one glow sequence from native three-byte RGB triplets into five-bit channels.</summary>
    private static PaletteRgb5[] Read(ISnesAddressSpace bus, PowerBombGlowSequence sequence)
    {
        byte[] source = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            PowerBombColorSources.Start(sequence),
            PowerBombGlowColorFormat.Count(sequence) * SamusSourceDefinitions.PowerBombBytesPerColor);
        var colors = new PaletteRgb5[PowerBombGlowColorFormat.Count(sequence)];
        for (int index = 0; index < colors.Length; index++)
        {
            int offset = index * SamusSourceDefinitions.PowerBombBytesPerColor;
            colors[index] = new PaletteRgb5
            {
                Red = source[offset] & ComponentMask,
                Green = source[offset + 1] & ComponentMask,
                Blue = source[offset + 2] & ComponentMask,
            };
        }
        return colors;
    }
}
