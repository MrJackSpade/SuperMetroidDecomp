using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Crocomire's five native RGB5 transfer images, including boundary words.</summary>
public static class CrocomireColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return CrocomireColorCatalog.Write(new CrocomireColorDocument
        {
            Version = CrocomireColorFormat.Version,
            FightBody = Read(CrocomirePaletteRomData.FightBodySource,
                CrocomirePaletteRomData.FightBodyCount),
            InitialWall = Read(CrocomirePaletteRomData.InitialWallSource,
                CrocomirePaletteRomData.InitialWallCount),
            InitialProjectile = Read(CrocomirePaletteRomData.InitialProjectileSource,
                CrocomirePaletteRomData.InitialProjectileCount),
            SkeletonArm = Read(CrocomirePaletteRomData.SkeletonArmSource,
                CrocomirePaletteRomData.SkeletonArmCount),
            WallSpikes = Read(CrocomirePaletteRomData.WallSpikesSource,
                CrocomirePaletteRomData.WallSpikesCount),
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int index = 0; index < count; index++)
            {
                int address = source + index * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(bus, address);
                if ((native & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Crocomire color ${address:X6} has an unrepresentable high bit.");
                colors[index] = new PaletteRgb5
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
