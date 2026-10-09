using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Crocomire's five native RGB5 transfer images, including boundary words.</summary>
public static class CrocomireColorExtractor
{
    /// <summary>Exports Crocomire's fight-body restoration, initial wall/projectile transfers, skeleton-arm palette, and wall-spike palette with their exact native extents.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for bank-$A4 color bands at $B89D, $B8BD, $B8DD, $B8FD, and $B91D respectively.</param>
    /// <returns>A new UTF-8 JSON buffer with five named arrays of 8, 17, 17, 16, and 16 RGB5 colors; each channel is 0..31.</returns>
    /// <remarks>The seventeen-word initial transfers include the next source palette's first word rather than truncating at sixteen. CGRAM destinations, hurt-flash restoration, wall-break conditions, and encounter timing remain compiled.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A source color sets bit 15, which the RGB5 document cannot represent.</exception>
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
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
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
