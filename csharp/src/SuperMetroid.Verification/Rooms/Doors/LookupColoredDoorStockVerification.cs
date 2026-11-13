using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks the installed pictures of the 48 colored-door frames against the cartridge's draw lists (#1557).</summary>
internal static class LookupColoredDoorStockVerification
{
    /// <summary>Compares each four-cell colored-door frame's picture with the cartridge, then the import and its rejections.</summary>
    internal static void VerifyColoredDoorStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectColoredDoorVisuals installed)
    {
        LookupColoredCapFamily[] families = [
            new LookupColoredCapFamily(0xa767,"yellow-left"),new LookupColoredCapFamily(0xa797,"yellow-right"),new LookupColoredCapFamily(0xa7c7,"yellow-up"),new LookupColoredCapFamily(0xa7f7,"yellow-down"),
            new LookupColoredCapFamily(0xa827,"green-left"),new LookupColoredCapFamily(0xa857,"green-right"),new LookupColoredCapFamily(0xa887,"green-up"),new LookupColoredCapFamily(0xa8b7,"green-down"),
            new LookupColoredCapFamily(0xa8e7,"red-left"),new LookupColoredCapFamily(0xa917,"red-right"),new LookupColoredCapFamily(0xa947,"red-up"),new LookupColoredCapFamily(0xa977,"red-down")];
        var frames = families.SelectMany(family => Enumerable.Range(0,4).Select(frame =>
            new LookupEscapeGateStockFrame(Pointer:(ushort)(family.First + 12 * frame), Id:$"{family.Name}-frame-{frame}"))).ToArray();
        foreach (InstalledRoomObjectColoredDoorVisuals pictures in new[] { InstalledRoomObjectColoredDoorVisuals.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectColoredDoorVisualFiles.Family, frames, 192);
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<ColoredDoorCaps.Draw>((ushort)raw)), "Colored door unknown stock pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectColoredDoorVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectColoredDoorVisualDirectory);
    }
}
