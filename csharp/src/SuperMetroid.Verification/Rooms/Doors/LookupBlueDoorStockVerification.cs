using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks the installed pictures of the blue-door caps against the cartridge's draw lists, closed caps included (#1557).</summary>
internal static class LookupBlueDoorStockVerification
{
    /// <summary>
    /// Compares all four orientations' sixteen frames and their four closed caps with the cartridge, then the import and
    /// its rejections.
    /// </summary>
    internal static void VerifyBlueDoorStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectBlueDoorVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [
            new LookupEscapeGateStockFrame(0xa9b3,"left-frame-0"),new LookupEscapeGateStockFrame(0xa9bf,"left-frame-1"),new LookupEscapeGateStockFrame(0xa9cb,"left-frame-2"),new LookupEscapeGateStockFrame(0xa9d7,"left-frame-3"),
            new LookupEscapeGateStockFrame(0xa9ef,"right-frame-0"),new LookupEscapeGateStockFrame(0xa9fb,"right-frame-1"),new LookupEscapeGateStockFrame(0xaa07,"right-frame-2"),new LookupEscapeGateStockFrame(0xaa13,"right-frame-3"),
            new LookupEscapeGateStockFrame(0xaa2b,"up-frame-0"),new LookupEscapeGateStockFrame(0xaa37,"up-frame-1"),new LookupEscapeGateStockFrame(0xaa43,"up-frame-2"),new LookupEscapeGateStockFrame(0xaa4f,"up-frame-3"),
            new LookupEscapeGateStockFrame(0xaa67,"down-frame-0"),new LookupEscapeGateStockFrame(0xaa73,"down-frame-1"),new LookupEscapeGateStockFrame(0xaa7f,"down-frame-2"),new LookupEscapeGateStockFrame(0xaa8b,"down-frame-3")];
        ushort[] aliases = [0xa9a7,0xa9e3,0xaa1f,0xaa5b];
        foreach (InstalledRoomObjectBlueDoorVisuals pictures in new[] { InstalledRoomObjectBlueDoorVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectBlueDoorVisualFiles.Family, frames, 64);
            foreach (ushort alias in aliases)
                AssertAliasPicturedAsNative(rom, pictures.Image(NativeObjectDraws.At<BlueDoorCaps.Draw>(alias)), alias, "Blue physical alias original stock visuals");
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw) && !aliases.Contains((ushort)raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<BlueDoorCaps.Draw>((ushort)raw)), "Blue door unknown stock pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectBlueDoorVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectBlueDoorVisualDirectory);
    }
}
