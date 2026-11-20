using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Exports the explosion's glow colours as installed colours without exporting radius or HDMA mechanics.</summary>
/// <remarks>The one decoder of the cartridge's three-byte fixed-colour triplets: each byte keeps its low five bits.</remarks>
[EncodingOwner("Power Bomb glow-colour triplet")]
public static class PowerBombGlowColorExtractor
{
        /// <summary>The five-bit channel in each stored fixed-colour byte.</summary>
        public const int ComponentMask = 0b1_1111;

    /// <summary>Imports the sixteen pre-explosion and thirty-two explosion glow colours used by Power Bomb effects.</summary>
    /// <param name="bus">Non-null cartridge import address space containing the native three-byte fixed-color sequences.</param>
    /// <returns>New UTF-8 JSON bytes containing the ordered colours, each native component masked to its five-bit level.</returns>
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

    /// <summary>Decodes one glow sequence from native three-byte triplets of five-bit levels into the installed colours.</summary>
    private static string[] Read(ISnesAddressSpace bus, PowerBombGlowSequence sequence)
    {
        byte[] source = RomDataReader.ReadFixedBank(bus,
            PowerBombColorSources.Start(sequence),
            PowerBombGlowColorFormat.Count(sequence) * SamusSourceDefinitions.PowerBombBytesPerColor);
        var colors = new string[PowerBombGlowColorFormat.Count(sequence)];
        for (int index = 0; index < colors.Length; index++)
        {
            int offset = index * SamusSourceDefinitions.PowerBombBytesPerColor;
            colors[index] = InstalledColourText.Format(new FadeLevelColour(source[offset] & ComponentMask,
                source[offset + 1] & ComponentMask, source[offset + 2] & ComponentMask).ToColour());
        }
        return colors;
    }
}
