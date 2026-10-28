using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Extracts Spore Spawn's initial, health, and death RGB5 images.</summary>
public static class SporeSpawnColorExtractor
{
    /// <summary>Exports the independent spore palette, four health-dependent body images, and separate sprite/level/background death-color sequences.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for bank-$A5 color bands beginning at $E359, $E379, $E3F9, $E4F9, and $E5D9 respectively.</param>
    /// <returns>A new UTF-8 JSON buffer with sixteen colors per row: one spore row, four health rows, eight death-sprite rows, and seven rows each for death-level and death-background; channels are RGB5 values 0..31.</returns>
    /// <remarks>Rows include color zero and retain native source order. Health thresholds, layer destinations, and death-script timing remain compiled; extraction neither runs AI nor installs CGRAM.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A source color sets unrepresentable bit 15.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        return SporeSpawnColorFile.Write(new SporeSpawnColorDocument
        {
            Version = SporeSpawnColorFormat.Version,
            Spores = Read(SporeSpawnSourceDefinitions.SporeColors),
            Health = ReadFrames(SporeSpawnSourceDefinitions.HealthColors,
                SporeSpawnColorDefinitions.HealthFrameCount),
            DeathSprite = ReadFrames(SporeSpawnSourceDefinitions.DeathSpriteColors,
                SporeSpawnColorDefinitions.DeathSpriteFrameCount),
            DeathLevel = ReadFrames(SporeSpawnSourceDefinitions.DeathLevelColors,
                SporeSpawnColorDefinitions.DeathSceneFrameCount),
            DeathBackground = ReadFrames(SporeSpawnSourceDefinitions.DeathBackgroundColors,
                SporeSpawnColorDefinitions.DeathSceneFrameCount),
        });

        PaletteRgb5[][] ReadFrames(int source, int count)
        {
            var frames = new PaletteRgb5[count][];
            for (int frame = 0; frame < count; frame++)
                frames[frame] = Read(source + frame * SporeSpawnSourceDefinitions.ImageBytes);
            return frames;
        }

        PaletteRgb5[] Read(int source)
        {
            var colors = new PaletteRgb5[SporeSpawnColorDefinitions.ColorsPerFrame];
            for (int color = 0; color < colors.Length; color++)
            {
                int address = source + color * SporeSpawnSourceDefinitions.ColorWordBytes;
                int native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!RgbColor.IsColorWord(checked((ushort)native)))
                    throw new InvalidDataException(
                        $"Spore Spawn color ${address:X6} has an unrepresentable high bit.");
                colors[color] = new PaletteRgb5
                {
                    Red = native & 31,
                    Green = native >> 5 & 31,
                    Blue = native >> 10 & 31,
                };
            }
            return colors;
        }
    }
}
