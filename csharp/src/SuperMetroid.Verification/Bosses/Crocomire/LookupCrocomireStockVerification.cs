using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>Checks Crocomire bridge and wall PLM visuals against native draw words.</summary>
internal static class LookupCrocomireStockVerification
{
    /// <summary>Compares the five arena frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections.</summary>
    internal static void VerifyCrocomireStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectCrocomireVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9b5b,"clear-bridge"), new LookupEscapeGateStockFrame(0x9b73,"crumble-bridge-block"),
            new LookupEscapeGateStockFrame(0x9b79,"clear-bridge-block"), new LookupEscapeGateStockFrame(0x9b7f,"clear-invisible-wall"), new LookupEscapeGateStockFrame(0x9bbb,"create-invisible-wall")];
        foreach (InstalledRoomObjectCrocomireVisuals pictures in new[] { InstalledRoomObjectCrocomireVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectCrocomireVisualFiles.Family, frames, WrittenCells(RoomObjectCrocomireVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<CrocomireArenaDraw>((ushort)raw)), "Crocomire unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectCrocomireVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectCrocomireVisualDirectory);
    }
}
