using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Areas.Maridia;

/// <summary>Checks the installed elevatube block picture against its native draw and rejects every other draw (#1557).</summary>
internal static class LookupElevatubeStockVerification
{
    /// <summary>Compares the stock and installed pictures with the cartridge, then the import and its rejections.</summary>
    internal static void VerifyElevatubeStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectMaridiaElevatubeVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9367, "elevatube-block")];
        foreach (InstalledRoomObjectMaridiaElevatubeVisuals pictures in new[] { InstalledRoomObjectMaridiaElevatubeVisuals.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectMaridiaElevatubeVisualFiles.Family, frames, WrittenCells(RoomObjectMaridiaElevatubeVisualFiles.Family));
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (pointer != 0x9367)
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<MaridiaElevatubeObjectDefinitions.Draw>((ushort)pointer)), "Elevatube full pointer domain");
        AssertImportAndRejections(rom, installed.Images, RoomObjectMaridiaElevatubeVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectMaridiaElevatubeVisualDirectory);
    }
}
