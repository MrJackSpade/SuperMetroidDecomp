using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks the installed pictures of Draygon's cannon frames against the cartridge's draw lists (#1557).</summary>
internal static class LookupDraygonCannonStockVerification
{
    /// <summary>Compares the twelve shield and damage frames' pictures with the cartridge, then the import and its rejections.</summary>
    internal static void VerifyDraygonCannonStockMapping(CartridgeImportAddressSpace rom, InstalledDraygonCannonFrames installed)
    {
        LookupEscapeGateStockFrame[] frames = [new(0x9fcd, "right-shield-a"), new(0x9fdd, "right-shield-b"), new(0xa02d, "right-damaged-a"),
            new(0xa03d, "right-damaged-b"), new(0xa04d, "right-damaged-c"), new(0xa05d, "right-damaged-d"), new(0xa0ed, "left-shield-a"),
            new(0xa101, "left-shield-b"), new(0xa165, "left-damaged-a"), new(0xa179, "left-damaged-b"), new(0xa18d, "left-damaged-c"),
            new(0xa1a1, "left-damaged-d")];
        foreach (InstalledDraygonCannonFrames pictures in new[] { InstalledDraygonCannonFrames.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectDraygonCannonVisualFiles.Family, frames, 48);
        AssertImportAndRejections(rom, installed.Images, RoomObjectDraygonCannonVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectDraygonCannonVisualDirectory);
    }
}
