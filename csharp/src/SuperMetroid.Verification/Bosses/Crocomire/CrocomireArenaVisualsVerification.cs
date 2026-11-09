using SuperMetroid.Core.Bosses.Crocomire;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireArenaObjectsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.LookupCrocomireStockVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Checks editable Crocomire bridge and invisible-wall draw art through stock extraction and production PLMs.</summary>
internal static class CrocomireArenaVisualsVerification
{
    /// <summary>Checks the stock mapping, the compiled bridge clear's physical block and tile, and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyCrocomireArenaVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire stock oracle revision");
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectCrocomireVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectCrocomireVisualFilesConstants.ValidateStock(
                installation.RoomObjectCrocomireVisualDirectory);
            InstalledRoomObjectCrocomireVisuals stock =
                installation.LoadRoomObjectCrocomireVisuals();
            VerifyCrocomireStockMapping(rom, stock);
            VerifyCrocomireVisualDraw(CrocomireArenaMutation.ClearBridge,
                new(9, 0), RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air), RoomBlockAppearance.Of(0x80), stock);

            string stockPath = Path.Combine(
                installation.RoomObjectCrocomireVisualDirectory,
                RoomObjectCrocomireVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectCrocomireVisualFilesConstants.ValidateStock(
                    installation.RoomObjectCrocomireVisualDirectory),
                "tampered Crocomire stock fails loudly where it is read");
        }
        Console.WriteLine("Crocomire arena visuals: five stock draws, the compiled bridge clear and strict failures pass.");
    }

    /// <summary>Runs one arena mutation and checks its collision word, immediate BG1 update, and ROM-free visual source.</summary>
    private static void VerifyCrocomireVisualDraw(CrocomireArenaMutation mutation,
        Offset16<RoomBlocks> blockOffset, RoomTerrainBlock physical, RoomBlockAppearance visual,
        InstalledRoomObjectCrocomireVisuals visuals)
    {
        const int width = 32;
        RoomTerrainBlock[] terrain = Enumerable.Repeat(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), width * 16).ToArray();
        RoomLevelData level = CreateRoom(new(width, 16), terrain, new RoomBlockBehavior[terrain.Length]);
        var roomObjects = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectCrocomireVisuals = visuals });
        AssertTrue(roomObjects.BossRoomMutations.CrocomireArena.TrySpawn(level, new(new(5, 3), mutation)),
            $"Crocomire visual mutation {mutation} allocates");
        var guard = NewCrocomireSourceGuard(new TestAddressSpace());
        roomObjects.Step(level);
        int blockIndex = (3 + blockOffset.Y) * width + 5 + blockOffset.X;
        AssertEqual(physical,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            $"Crocomire visual mutation {mutation} retains physical block");
        AssertEqual(visual, level.DisplayedAppearance(blockIndex),
            $"Crocomire visual mutation {mutation} presents its tile");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Crocomire visual mutation {mutation} reads no compiled source");
    }

    /// <summary>Runs the focused Crocomire arena artwork and PLM checks.</summary>
    [VerificationCommand("--crocomire-arena-visuals")]
    internal static int CrocomireArenaVisualsCommand()
    {
        Suite(nameof(VerifyCrocomireArenaVisuals), () => VerifyCrocomireArenaVisuals());
        return 0;
    }

}
