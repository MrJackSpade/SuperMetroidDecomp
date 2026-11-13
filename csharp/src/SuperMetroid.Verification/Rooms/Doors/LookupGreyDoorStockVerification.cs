using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks the installed pictures of the grey-door and shared clear frames against the cartridge's draw lists (#1557).</summary>
internal static class LookupGreyDoorStockVerification
{
    /// <summary>Compares the twenty four-cell frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections.</summary>
    internal static void VerifyGreyDoorStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectGreyDoorVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa677,"clear-left"),new LookupEscapeGateStockFrame(0xa683,"clear-right"),new LookupEscapeGateStockFrame(0xa68f,"clear-up"),new LookupEscapeGateStockFrame(0xa69b,"clear-down"),
            new LookupEscapeGateStockFrame(0xa6a7,"grey-left-frame-0"),new LookupEscapeGateStockFrame(0xa6b3,"grey-left-frame-1"),new LookupEscapeGateStockFrame(0xa6bf,"grey-left-frame-2"),new LookupEscapeGateStockFrame(0xa6cb,"grey-left-frame-3"),
            new LookupEscapeGateStockFrame(0xa6d7,"grey-right-frame-0"),new LookupEscapeGateStockFrame(0xa6e3,"grey-right-frame-1"),new LookupEscapeGateStockFrame(0xa6ef,"grey-right-frame-2"),new LookupEscapeGateStockFrame(0xa6fb,"grey-right-frame-3"),
            new LookupEscapeGateStockFrame(0xa707,"grey-up-frame-0"),new LookupEscapeGateStockFrame(0xa713,"grey-up-frame-1"),new LookupEscapeGateStockFrame(0xa71f,"grey-up-frame-2"),new LookupEscapeGateStockFrame(0xa72b,"grey-up-frame-3"),
            new LookupEscapeGateStockFrame(0xa737,"grey-down-frame-0"),new LookupEscapeGateStockFrame(0xa743,"grey-down-frame-1"),new LookupEscapeGateStockFrame(0xa74f,"grey-down-frame-2"),new LookupEscapeGateStockFrame(0xa75b,"grey-down-frame-3")];
        foreach (InstalledRoomObjectGreyDoorVisuals pictures in new[] { InstalledRoomObjectGreyDoorVisuals.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectGreyDoorVisualFiles.Family, frames, 80);
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<GreyDoorCaps.Draw>((ushort)raw)), "Grey door unknown stock pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectGreyDoorVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectGreyDoorVisualDirectory);
    }
}
