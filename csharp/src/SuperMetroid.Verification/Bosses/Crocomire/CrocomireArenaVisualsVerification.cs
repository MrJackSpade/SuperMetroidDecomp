using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireArenaPlmsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.LookupCrocomireStockVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
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
    /// <summary>Checks stock mapping, visible edits, physical block isolation, override persistence, and asset rejection.</summary>
    internal static void VerifyCrocomireArenaVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Crocomire stock oracle revision");
        using var testRootScratch = new TestScratchDirectory("crocomire-arena-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmCrocomireVisualFiles.Extract(rom,
                installation.RoomPlmCrocomireVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmCrocomireVisualFiles.ValidateStock(
                installation.RoomPlmCrocomireVisualDirectory);
            RoomPlmCrocomireVisualCatalog stock =
                installation.LoadRoomPlmCrocomireVisuals();
            VerifyCrocomireStockMapping(rom, stock);

            string stockPath = Path.Combine(
                installation.RoomPlmCrocomireVisualDirectory,
                RoomPlmCrocomireVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Crocomire JSON is empty.");
            Entry("clear-bridge").Required("blocks")[9] = InstalledAppearanceJson.Cell(0x58);
            Entry("create-invisible-wall").Required("blocks")[23] = InstalledAppearanceJson.Cell(0x59);
            Directory.CreateDirectory(
                installation.RoomPlmCrocomireVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmCrocomireVisualOverrideDirectory,
                RoomPlmCrocomireVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmCrocomireVisualCatalog edited =
                installation.LoadRoomPlmCrocomireVisuals();
            AssertEqual(RoomBlockAppearance.Of(0x58),
                edited.GetAppearance(PlmDraw.Of(CrocomireArenaDraw.ClearBridge), 0, 9),
                "Crocomire override edits far bridge block");
            AssertEqual(RoomBlockAppearance.Of(0x59),
                edited.GetAppearance(PlmDraw.Of(CrocomireArenaDraw.CreateInvisibleWall), 2, 7),
                "Crocomire override edits final wall column");
            VerifyCrocomireVisualDraw(PlmHeaderId.ClearCrocomireBridge,
                new(9, 0), RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air), RoomBlockAppearance.Of(0x58), edited);
            VerifyCrocomireVisualDraw(PlmHeaderId.CreateCrocomireInvisibleWall,
                new(2, 7), RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.SolidBlock), RoomBlockAppearance.Of(0x59), edited);
            VerifyCrocomireVisualDraw(PlmHeaderId.ClearCrocomireBridge,
                new(9, 0), RoomTerrainBlock.Of(0x80, BlockMirroring.None, RoomCollisionType.Air), RoomBlockAppearance.Of(0x80), null);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmCrocomireVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x59),
                RoomPlmCrocomireVisualFiles.Load(refreshed,
                    installation.RoomPlmCrocomireVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(CrocomireArenaDraw.CreateInvisibleWall),
                        2, 7),
                "Crocomire override survives stock replacement");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmCrocomireVisualFiles.ValidateStock(
                    installation.RoomPlmCrocomireVisualDirectory),
                "tampered Crocomire stock fails manifest validation");

            JsonNode Entry(string id) => document.Required("entries").AsArray().RequiredElements()
                .Single(entry => entry.Required("id").GetValue<string>() == id);
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine("Crocomire arena visuals: five stock draws, bridge/wall edits, physical isolation, override persistence and strict failures pass.");
    }

    /// <summary>Runs one arena mutation and checks its collision word, immediate BG1 update, and ROM-free visual source.</summary>
    private static void VerifyCrocomireVisualDraw(PlmHeaderId header,
        Offset16<RoomBlocks> blockOffset, RoomTerrainBlock physical, RoomBlockAppearance visual,
        RoomPlmCrocomireVisualCatalog? visuals)
    {
        const int width = 32;
        RoomTerrainBlock[] terrain = Enumerable.Repeat(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), width * 16).ToArray();
        RoomLevelData level = CreateRoom(new(width, 16), terrain, new RoomBlockBehavior[terrain.Length]);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmCrocomireVisuals = visuals });
        AssertTrue(plms.BossRoomMutations.TrySpawnCrocomireArenaMutation(level, new(5, 3), header),
            $"Crocomire visual header {header} allocates");
        var guard = NewCrocomireSourceGuard(new TestAddressSpace());
        plms.Step(level);
        int blockIndex = (3 + blockOffset.Y) * width + 5 + blockOffset.X;
        AssertEqual(physical,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            $"Crocomire visual header {header} retains physical block");
        AssertEqual(visual, level.DisplayedAppearance(blockIndex),
            $"Crocomire visual header {header} presents edited tile");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Crocomire visual header {header} reads no compiled source");
    }

    /// <summary>Runs the focused Crocomire arena artwork and PLM checks.</summary>
    [VerificationCommand("--crocomire-arena-visuals")]
    internal static int CrocomireArenaVisualsCommand()
    {
        Suite(nameof(VerifyCrocomireArenaVisuals), () => VerifyCrocomireArenaVisuals());
        return 0;
    }
}
