using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The menu missile's and the file-select helmet's frame holds as the import reads them from the cartridge (#1615); the
/// game reads them only from the installed menu presentations.
/// </summary>
internal static class MenuAnimationTimings
{
    /// <summary>The ticks the menu missile holds each frame: the one value every frame's timer reload holds.</summary>
    /// <exception cref="InvalidDataException">The cartridge's reloads differ, which one installed hold cannot carry.</exception>
    internal static int MissileFrameDuration(ISnesAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        int duration = RomDataReader.ReadWordFixedBank(cartridge, MenuAnimationSourceDefinitions.MissileFrameDurations);
        for (int frame = 1; frame < MenuMissileAnimationDefinitions.FrameCount; frame++)
        {
            int reload = RomDataReader.ReadWordFixedBank(cartridge, MenuAnimationSourceDefinitions.MissileFrameDurations + frame * sizeof(ushort));
            if (reload != duration)
                throw new InvalidDataException($"The menu missile's frame {frame} reloads its timer with {reload} ticks, not the {duration} of frame 0.");
        }
        return duration;
    }

    /// <summary>The ticks the file-select helmet holds each frame.</summary>
    internal static int HelmetFrameDuration(ISnesAddressSpace bus) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MenuAnimationSourceDefinitions.HelmetFrameDuration);
}
