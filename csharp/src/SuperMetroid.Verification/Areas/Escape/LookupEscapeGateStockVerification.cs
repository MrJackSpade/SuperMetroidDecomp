using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks the installed pictures of the escape gate's frames against the cartridge's draw lists (#1557).</summary>
internal static class LookupEscapeGateStockVerification
{
    /// <summary>Compares the open, half-closed and closed frames' pictures with the cartridge, then the import and its rejections.</summary>
    internal static void VerifyEscapeGateStockMapping(CartridgeImportAddressSpace rom, InstalledEscapeGateFrames installed)
    {
        LookupEscapeGateStockFrame[] frames = [new(0x9473, "open"), new(0x947f, "half-closed"), new(0x948b, "closed")];
        foreach (InstalledEscapeGateFrames pictures in new[] { InstalledEscapeGateFrames.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectEscapeGateVisualFiles.Family, frames, 12);
        AssertImportAndRejections(rom, installed.Images, RoomObjectEscapeGateVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectEscapeGateVisualDirectory);
    }
}
