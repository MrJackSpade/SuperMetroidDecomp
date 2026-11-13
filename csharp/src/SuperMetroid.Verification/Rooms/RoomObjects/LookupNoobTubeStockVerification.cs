using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks stock and installed Noob Tube visual words against native PLM draw lists.</summary>
internal static class LookupNoobTubeStockVerification
{
    /// <summary>Compares the seven tube frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections (#1557).</summary>
    internal static void VerifyNoobTubeStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectNoobTubeVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x98d1,"intact"),new LookupEscapeGateStockFrame(0x98d7,"damaged"),new LookupEscapeGateStockFrame(0x98dd,"opened"),new LookupEscapeGateStockFrame(0x98e3,"cleared"),new LookupEscapeGateStockFrame(0x9953,"broken-late"),new LookupEscapeGateStockFrame(0x9991,"opened-rows"),new LookupEscapeGateStockFrame(0x99e5,"broken-full")];
        foreach (InstalledRoomObjectNoobTubeVisuals pictures in new[] { InstalledRoomObjectNoobTubeVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectNoobTubeVisualFiles.Family, frames, WrittenCells(RoomObjectNoobTubeVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<NoobTubeDraw>((ushort)raw)), "Tube stock unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectNoobTubeVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectNoobTubeVisualDirectory);
    }
}
