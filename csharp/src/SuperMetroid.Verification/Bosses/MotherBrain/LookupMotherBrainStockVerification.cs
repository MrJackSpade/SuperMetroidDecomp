using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks the installed pictures of Mother Brain's fake-death room frames against the cartridge's draw lists (#1557).</summary>
internal static class LookupMotherBrainStockVerification
{
    /// <summary>Compares the twenty-two frames' 230 written cells with the cartridge, then the import and its rejections.</summary>
    internal static void VerifyMotherBrainStockMapping(CartridgeImportAddressSpace rom, InstalledMotherBrainFakeDeathFrames installed)
    {
        LookupEscapeGateStockFrame[] frames = [new(0x94a3, "fill-wall"), new(0x94b1, "escape-door"),
            new(0x9505, "background-row-2"), new(0x9523, "background-row-3"), new(0x9541, "background-row-4"),
            new(0x955f, "background-row-5"), new(0x957d, "background-row-6"), new(0x959b, "background-row-7"),
            new(0x95b9, "background-row-8"), new(0x95d7, "background-row-9"), new(0x95f5, "background-row-a"),
            new(0x9613, "background-row-b"), new(0x9631, "background-row-c"), new(0x964f, "background-row-d"),
            new(0x966d, "background-row-e-unused"), new(0x968b, "background-row-f-unused"),
            new(0x96a9, "clear-ceiling-block"), new(0x96b1, "clear-ceiling-tube"),
            new(0x96bf, "clear-bottom-middle-side-tube"), new(0x96cb, "clear-bottom-middle-tubes"),
            new(0x96ef, "clear-bottom-left-tube"), new(0x9703, "clear-bottom-right-tube")];
        foreach (InstalledMotherBrainFakeDeathFrames pictures in new[] { InstalledMotherBrainFakeDeathFrames.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectMotherBrainFakeDeathVisualFiles.Family, frames, 230);
        AssertImportAndRejections(rom, installed.Images, RoomObjectMotherBrainFakeDeathVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectMotherBrainFakeDeathVisualDirectory);
    }
}
