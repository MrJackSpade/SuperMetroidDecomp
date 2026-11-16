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
        LookupEscapeGateStockFrame[] frames = [new(0x9fcd, "right-shield"), new(0x9fdd, "right-shield-flash"), new(0xa02d, "right-sparks-peaking-at-11-and-20"),
            new(0xa03d, "right-sparks-peaking-at-3-and-28"), new(0xa04d, "right-sparks-peaking-at-6-and-25"), new(0xa05d, "right-sparks-flat"), new(0xa0ed, "left-shield"),
            new(0xa101, "left-shield-flash"), new(0xa165, "left-sparks-peaking-at-11-and-20"), new(0xa179, "left-sparks-peaking-at-3-and-28"), new(0xa18d, "left-sparks-peaking-at-6-and-25"),
            new(0xa1a1, "left-sparks-flat")];
        foreach (InstalledDraygonCannonFrames pictures in new[] { InstalledDraygonCannonFrames.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectDraygonCannonVisualFiles.Family, frames, 48);
        AssertImportAndRejections(rom, installed.Images, RoomObjectDraygonCannonVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectDraygonCannonVisualDirectory);
    }
}
