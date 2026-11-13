using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.SporeSpawn;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Checks editable Spore Spawn ceiling artwork while retaining compiled PLM physics and timing.</summary>
internal static class SporeSpawnCeilingVisualsVerification
{
    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifySporeSpawnCeilingVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Spore ceiling stock oracle revision");
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectSporeSpawnCeilingVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectSporeSpawnCeilingVisualFilesConstants.ValidateStock(
                installation.RoomObjectSporeSpawnCeilingVisualDirectory);
            VerifySporeSpawnCeilingStockMapping(rom, installation.LoadRoomObjectSporeSpawnCeilingVisuals());

            string stockPath = Path.Combine(
                installation.RoomObjectSporeSpawnCeilingVisualDirectory,
                RoomObjectSporeSpawnCeilingVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectSporeSpawnCeilingVisualFilesConstants.ValidateStock(
                    installation.RoomObjectSporeSpawnCeilingVisualDirectory),
                "tampered Spore Spawn ceiling stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Spore Spawn ceiling visuals: four native frames and strict failures pass.");
    }

    /// <summary>Compares the four ceiling frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections.</summary>
    private static void VerifySporeSpawnCeilingStockMapping(CartridgeImportAddressSpace rom,
        InstalledRoomObjectSporeSpawnCeilingVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9413,"clear-ceiling"), new LookupEscapeGateStockFrame(0x9423,"crumble-frame-0"),
            new LookupEscapeGateStockFrame(0x9433,"crumble-frame-1"), new LookupEscapeGateStockFrame(0x9443,"crumble-frame-2")];
        foreach (InstalledRoomObjectSporeSpawnCeilingVisuals pictures in new[] { InstalledRoomObjectSporeSpawnCeilingVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectSporeSpawnCeilingVisualFiles.Family, frames, WrittenCells(RoomObjectSporeSpawnCeilingVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<SporeSpawnCeilingDraw>((ushort)raw)), "Spore ceiling unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectSporeSpawnCeilingVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectSporeSpawnCeilingVisualDirectory);
    }

    /// <summary>Runs Spore Spawn ceiling visual verification.</summary>
    [VerificationCommand("--spore-spawn-ceiling-visuals")]
    internal static int SporeSpawnCeilingVisualsCommand()
    {
        Suite(nameof(VerifySporeSpawnCeilingVisuals), () => VerifySporeSpawnCeilingVisuals());
        return 0;
    }
}
