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
        InstalledRoomObjectPictures installed)
    {
        LookupEscapeGateStockFrame[] frames = [
            new LookupEscapeGateStockFrame(0xa9b3,"left-closed"),new LookupEscapeGateStockFrame(0xa9bf,"left-ajar"),new LookupEscapeGateStockFrame(0xa9cb,"left-half-open"),new LookupEscapeGateStockFrame(0xa9d7,"left-nearly-open"),
            new LookupEscapeGateStockFrame(0xa9ef,"right-closed"),new LookupEscapeGateStockFrame(0xa9fb,"right-ajar"),new LookupEscapeGateStockFrame(0xaa07,"right-half-open"),new LookupEscapeGateStockFrame(0xaa13,"right-nearly-open"),
            new LookupEscapeGateStockFrame(0xaa2b,"up-closed"),new LookupEscapeGateStockFrame(0xaa37,"up-ajar"),new LookupEscapeGateStockFrame(0xaa43,"up-half-open"),new LookupEscapeGateStockFrame(0xaa4f,"up-nearly-open"),
            new LookupEscapeGateStockFrame(0xaa67,"down-closed"),new LookupEscapeGateStockFrame(0xaa73,"down-ajar"),new LookupEscapeGateStockFrame(0xaa7f,"down-half-open"),new LookupEscapeGateStockFrame(0xaa8b,"down-nearly-open")];
        ushort[] aliases = [0xa9a7,0xa9e3,0xaa1f,0xaa5b];
        foreach (InstalledRoomObjectPictures pictures in new[] { RoomObjectPicturesStock.BlueDoor, installed })
        {
            AssertEqual(frames.Length + aliases.Length, pictures.Images.All.Count, "Blue door pictures every frame and every closed cap drawn solid");
            AssertPicturedAsNative(rom, FamilyFrames(pictures.Images, RoomObjectBlueDoorVisualFiles.Family), RoomObjectBlueDoorVisualFiles.Family, frames, 64);
            foreach (ushort alias in aliases)
                AssertAliasPicturedAsNative(rom, pictures.Image(NativeObjectDraws.At<BlueDoorCaps.Draw>(alias)), alias, "Blue physical alias original stock visuals");
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw) && !aliases.Contains((ushort)raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<BlueDoorCaps.Draw>((ushort)raw)), "Blue door unknown stock pointer");
        AssertImportAndRejections(rom, FamilyFrames(installed.Images, RoomObjectBlueDoorVisualFiles.Family), RoomObjectBlueDoorVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectBlueDoorVisualDirectory);
    }
}
