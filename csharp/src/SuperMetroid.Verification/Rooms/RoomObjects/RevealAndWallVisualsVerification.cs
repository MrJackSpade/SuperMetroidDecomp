using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
namespace SuperMetroid.Verification.Rooms.RoomObjects;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the installed pictures of the bombed reveals, Crateria's escape passage, the old Tourian escape shaft wall and
/// the escape animals' rescue wall (#1557) against the cartridge's own draws.
/// </summary>
internal static class RevealAndWallVisualsVerification
{
    /// <summary>
    /// Confirms every installed frame shows, cell by cell, the block the cartridge's draw list writes there, and that a
    /// document missing, repeating, misnaming or reshaping a frame is rejected.
    /// </summary>
    internal static void VerifyRevealAndWallVisuals()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        InstalledRoomObjectRevealAndWallVisuals installed = InstalledRoomObjectRevealAndWallVisuals.Stock();
        LookupEscapeGateStockFrame[] frames = [.. RoomObjectRevealAndWallVisualFiles.All.Select(frame =>
            new LookupEscapeGateStockFrame(RoomObjectDrawAddressCodec.Of(frame.Draw), RoomObjectRevealAndWallVisualFiles.VisualId(frame.Draw)))];
        AssertPicturedAsNative(rom, installed.Images, RoomObjectRevealAndWallVisualFiles.Family, frames, WrittenCells(RoomObjectRevealAndWallVisualFiles.Family));
        AssertImportAndRejections(rom, installed.Images, RoomObjectRevealAndWallVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectRevealAndWallVisualDirectory);
        Console.WriteLine($"Reveal and wall visuals: {frames.Length} installed frames match the cartridge's draws.");
    }

    /// <summary>Runs the focused reveal and wall picture check against the retail ROM.</summary>
    [VerificationCommand("--reveal-and-wall-visuals")]
    internal static int RevealAndWallVisualsCommand()
    {
        Suite(nameof(VerifyRevealAndWallVisuals), () => VerifyRevealAndWallVisuals());
        return 0;
    }
}
