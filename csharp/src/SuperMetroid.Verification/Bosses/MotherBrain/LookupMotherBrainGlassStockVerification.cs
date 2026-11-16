using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks the installed pictures of Mother Brain's glass frames against the cartridge's draw lists (#1557).</summary>
internal static class LookupMotherBrainGlassStockVerification
{
    /// <summary>Compares the eleven glass frames' pictures with the cartridge, then the import and its rejections.</summary>
    internal static void VerifyMotherBrainGlassStockMapping(CartridgeImportAddressSpace rom, InstalledMotherBrainGlassFrames installed)
    {
        LookupEscapeGateStockFrame[] frames = [new(0x9717, "initial"), new(0x971d, "pane-cracked"), new(0x9731, "pane-cracked-through"),
            new(0x9745, "pane-transition"), new(0x974f, "pane-shifted"), new(0x9769, "pane-shifted-crumbling"), new(0x9781, "pane-shifted-emptying"),
            new(0x978f, "rim-buckling"), new(0x97b7, "rim-bursting"), new(0x97e7, "rim-gone"), new(0x9817, "cleared")];
        foreach (InstalledMotherBrainGlassFrames pictures in new[] { InstalledMotherBrainGlassFrames.Stock(), installed })
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectMotherBrainGlassVisualFiles.Family, frames, 92);
        AssertImportAndRejections(rom, installed.Images, RoomObjectMotherBrainGlassVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectMotherBrainGlassVisualDirectory);
    }
}
