using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks map, save, and recharge station visual frames against native PLM draw lists.</summary>
internal static class LookupStationStockVerification
{
    /// <summary>Compares the twenty station frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections (#1557).</summary>
    internal static void VerifyStationStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectStationVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9f25,"map-frame-0"),new LookupEscapeGateStockFrame(0x9f6d,"energy-frame-0"),new LookupEscapeGateStockFrame(0x9f91,"missile-frame-0"),
            new LookupEscapeGateStockFrame(0x9f31,"map-frame-1"),new LookupEscapeGateStockFrame(0x9f79,"energy-frame-1"),new LookupEscapeGateStockFrame(0x9f9d,"missile-frame-1"),
            new LookupEscapeGateStockFrame(0x9f3d,"map-frame-2"),new LookupEscapeGateStockFrame(0x9f85,"energy-frame-2"),new LookupEscapeGateStockFrame(0x9fa9,"missile-frame-2"),
            new LookupEscapeGateStockFrame(0x9a3f,"save-idle"),new LookupEscapeGateStockFrame(0x9a9f,"save-active-a"),new LookupEscapeGateStockFrame(0x9a6f,"save-active-b"),
            new LookupEscapeGateStockFrame(0x9f49,"map-right-retracted"),new LookupEscapeGateStockFrame(0x9f55,"map-right-extended"),new LookupEscapeGateStockFrame(0x9f5b,"map-left-retracted"),new LookupEscapeGateStockFrame(0x9f67,"map-left-extended"),
            new LookupEscapeGateStockFrame(0x9fb5,"resource-right-retracted"),new LookupEscapeGateStockFrame(0x9fbb,"resource-right-extended"),new LookupEscapeGateStockFrame(0x9fc1,"resource-left-retracted"),new LookupEscapeGateStockFrame(0x9fc7,"resource-left-extended")];
        foreach (InstalledRoomObjectStationVisuals pictures in new[] { InstalledRoomObjectStationVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectStationVisualFiles.Family, frames, WrittenCells(RoomObjectStationVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<StationDraw>((ushort)raw)), "Station stock unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectStationVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectStationVisualDirectory);
    }
}
