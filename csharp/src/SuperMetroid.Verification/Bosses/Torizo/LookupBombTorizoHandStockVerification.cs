using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks the installed pictures of the intact and cleared Bomb Torizo hand frames against the cartridge (#1557).</summary>
internal static class LookupBombTorizoHandStockVerification
{
    /// <summary>Compares both frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections.</summary>
    internal static void VerifyBombTorizoHandStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectBombTorizoHandVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9877,"intact"),new LookupEscapeGateStockFrame(0x989d,"cleared")];
        foreach (InstalledRoomObjectBombTorizoHandVisuals pictures in new[] { InstalledRoomObjectBombTorizoHandVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectBombTorizoHandVisualFiles.Family, frames, WrittenCells(RoomObjectBombTorizoHandVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<BombTorizoHandDraw>((ushort)raw)), "Hand stock unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectBombTorizoHandVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectBombTorizoHandVisualDirectory);
    }
}
