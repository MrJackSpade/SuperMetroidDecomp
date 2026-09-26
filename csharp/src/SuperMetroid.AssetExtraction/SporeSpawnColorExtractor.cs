using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Spore Spawn's initial, health, and death RGB5 images.</summary>
public static class SporeSpawnColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return SporeSpawnColorCatalog.Write(new SporeSpawnColorDocument
        {
            Version = SporeSpawnColorFormat.Version,
            Spores = Read(SporeSpawnColorRomData.SporeSource),
            Health = ReadFrames(SporeSpawnColorRomData.HealthSource,
                SporeSpawnColorRomData.HealthFrameCount),
            DeathSprite = ReadFrames(SporeSpawnColorRomData.DeathSpriteSource,
                SporeSpawnColorRomData.DeathSpriteFrameCount),
            DeathLevel = ReadFrames(SporeSpawnColorRomData.DeathLevelSource,
                SporeSpawnColorRomData.DeathSceneFrameCount),
            DeathBackground = ReadFrames(SporeSpawnColorRomData.DeathBackgroundSource,
                SporeSpawnColorRomData.DeathSceneFrameCount),
        });

        PaletteRgb5[][] ReadFrames(int source, int count)
        {
            var frames = new PaletteRgb5[count][];
            for (int frame = 0; frame < count; frame++)
                frames[frame] = Read(source + frame * SporeSpawnColorRomData.FrameByteCount);
            return frames;
        }

        PaletteRgb5[] Read(int source)
        {
            var colors = new PaletteRgb5[SporeSpawnColorRomData.ColorsPerFrame];
            for (int color = 0; color < colors.Length; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(bus, address);
                if ((native & 0x8000) != 0)
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
