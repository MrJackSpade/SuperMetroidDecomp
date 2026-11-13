using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks downward gate column and colored trigger draw lists against bank-$84 PLM data.</summary>
internal static class LookupDownwardGateStockVerification
{
    /// <summary>Compares the fourteen gate frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections (#1557).</summary>
    internal static void VerifyDownwardGateStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectDownwardGateVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0xa517,"column-frame-0"),new LookupEscapeGateStockFrame(0xa525,"column-frame-1"),
            new LookupEscapeGateStockFrame(0xa533,"column-frame-2"),new LookupEscapeGateStockFrame(0xa541,"column-frame-3"),new LookupEscapeGateStockFrame(0xa54f,"column-frame-4"),new LookupEscapeGateStockFrame(0xa55d,"column-frame-5"),
            new LookupEscapeGateStockFrame(0xa5d7,"blue-left-trigger"),new LookupEscapeGateStockFrame(0xa5e3,"blue-right-trigger"),new LookupEscapeGateStockFrame(0xa5eb,"green-left-trigger"),
            new LookupEscapeGateStockFrame(0xa5f7,"green-right-trigger"),new LookupEscapeGateStockFrame(0xa5ff,"red-left-trigger"),new LookupEscapeGateStockFrame(0xa60b,"red-right-trigger"),
            new LookupEscapeGateStockFrame(0xa613,"yellow-left-trigger"),new LookupEscapeGateStockFrame(0xa61f,"yellow-right-trigger")];
        foreach (InstalledRoomObjectDownwardGateVisuals pictures in new[] { InstalledRoomObjectDownwardGateVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectDownwardGateVisualFiles.Family, frames, WrittenCells(RoomObjectDownwardGateVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<DownwardGateDraw>((ushort)raw)), "Gate stock unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectDownwardGateVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectDownwardGateVisualDirectory);
    }
}
