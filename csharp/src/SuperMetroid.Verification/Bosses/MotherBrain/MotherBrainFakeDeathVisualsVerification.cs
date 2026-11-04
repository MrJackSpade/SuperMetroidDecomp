using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.MotherBrain.LookupMotherBrainStockVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainFakeDeathPlmsVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Checks fake-death artwork replacement while retaining Mother Brain's native collision and draw state.</summary>
internal static class MotherBrainFakeDeathVisualsVerification
{
    /// <summary>Checks fake-death stock draws, live visual edits, collision isolation, override persistence, and failures.</summary>
    internal static void VerifyMotherBrainFakeDeathVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain stock oracle revision");
        // A private installation copy, whose override is written and whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmMotherBrainFakeDeathVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomPlmMotherBrainFakeDeathVisualFilesConstants.ValidateStock(
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory);
            RoomPlmMotherBrainFakeDeathVisualCatalog stock =
                installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
            VerifyMotherBrainStockMapping(rom, stock);

            string stockPath = Path.Combine(
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory,
                RoomPlmMotherBrainFakeDeathVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException(
                    "Extracted Mother Brain fake-death JSON is empty.");
            Entry("background-row-d").Required("blocks")[12] = InstalledAppearanceJson.Cell(0x58);
            Entry("clear-bottom-right-tube").Required("blocks")[5] = InstalledAppearanceJson.Cell(0x59);
            Directory.CreateDirectory(
                installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory,
                RoomPlmMotherBrainFakeDeathVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmMotherBrainFakeDeathVisualCatalog edited =
                installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
            AssertEqual(RoomBlockAppearance.Of(0x58),
                edited.GetAppearance(PlmDraw.Of(MotherBrainFakeDeathDraw.BackgroundRowD),
                    0, 12),
                "Mother Brain override edits far background tile");
            AssertEqual(RoomBlockAppearance.Of(0x59),
                edited.GetAppearance(
                    PlmDraw.Of(MotherBrainFakeDeathDraw.ClearBottomRightTube),
                    1, 0),
                "Mother Brain override edits second-run tube tile");
            VerifyMotherBrainVisualDraw(PlmHeaderId.MotherBrainsBackgroundRowD,
                new(12, 0), RoomTerrainBlock.Of(0x249, BlockMirroring.None, RoomCollisionType.Slope), RoomBlockAppearance.Of(0x58), edited);
            VerifyMotherBrainVisualDraw(PlmHeaderId.ClearMotherBrainBottomRightTube,
                new(-1, 0), RoomTerrainBlock.Of(0xff, BlockMirroring.None, RoomCollisionType.Air), RoomBlockAppearance.Of(0x59), edited);
            VerifyMotherBrainVisualDraw(PlmHeaderId.MotherBrainsBackgroundRowD,
                new(12, 0), RoomTerrainBlock.Of(0x249, BlockMirroring.None, RoomCollisionType.Slope), RoomTerrainBlock.Of(0x249, BlockMirroring.None, RoomCollisionType.Slope).Appearance,
                null);

            string refreshed = Path.Combine(copy.Root, "refreshed-stock");
            AssertEqual(RoomBlockAppearance.Of(0x59),
                ReExtractMotherBrainFakeDeathStock(rom, refreshed,
                    installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory)
                    .GetAppearance(
                        PlmDraw.Of(MotherBrainFakeDeathDraw.ClearBottomRightTube),
                        1, 0),
                "Mother Brain override survives stock replacement");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmMotherBrainFakeDeathVisualFilesConstants.ValidateStock(
                    installation.RoomPlmMotherBrainFakeDeathVisualDirectory),
                "tampered Mother Brain fake-death stock fails manifest validation");

            JsonNode Entry(string id) => document.Required("entries").AsArray().RequiredElements()
                .Single(entry => entry.Required("id").GetValue<string>() == id);
        }
        Console.WriteLine(
            "Mother Brain fake-death visuals: 22 stock draws, live row/tube edits, collision isolation, override persistence and strict failures pass.");
    }

    /// <summary>Checks a selected mutation's physical block and redrawn visual tile with source reads guarded.</summary>
    private static void VerifyMotherBrainVisualDraw(PlmHeaderId header,
        Offset16<RoomBlocks> blockOffset, RoomTerrainBlock physical, RoomBlockAppearance visual,
        RoomPlmMotherBrainFakeDeathVisualCatalog? visuals)
    {
        const int width = 32;
        RoomTerrainBlock[] terrain = Enumerable.Repeat(RoomTerrainBlock.Of(0x123, BlockMirroring.None, RoomCollisionType.SolidBlock), width * 16).ToArray();
        RoomLevelData level = CreateRoom(new(width, 16), terrain, new RoomBlockBehavior[terrain.Length]);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmMotherBrainFakeDeathVisuals = visuals });
        AssertTrue(plms.BossRoomMutations.TrySpawnMotherBrainMutation(level, new(5, 3), header),
            $"Mother Brain visual header {header} allocates");
        var guard = NewMotherBrainFakeDeathSourceGuard(new TestAddressSpace());
        plms.Step(level);
        int blockIndex = (3 + blockOffset.Y) * width + 5 + blockOffset.X;
        AssertEqual(physical,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            $"Mother Brain visual header {header} retains physical block");
        // The redraw identifies its block by the block's DrawPLM BG1 ring destination.
        AssertEqual(visual, level.DisplayedAppearance(blockIndex),
            $"Mother Brain visual header {header} presents edited tile");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain visual header {header} reads no compiled source");
    }

    /// <summary>Runs the Mother Brain fake-death visual asset suite.</summary>
    [VerificationCommand("--mother-brain-fake-death-visuals")]
    internal static int MotherBrainFakeDeathVisualsCommand()
    {
        Suite(nameof(VerifyMotherBrainFakeDeathVisuals), () => VerifyMotherBrainFakeDeathVisuals());
        return 0;
    }

    /// <summary>Extracts stock Mother Brain fake-death visuals from the cartridge into <paramref name="refreshed"/> and loads them under <paramref name="overrides"/>.</summary>
    [PrivateInstallation("Runs RoomPlmMotherBrainFakeDeathVisualFiles.Extract into a fresh stock directory beside the copy's Mother Brain fake-death override, to check that a stock replacement keeps the edited bottom-right tube clear block (appearance 59) selected; the copied stock is the installer's output and never re-runs the extractor.")]
    private static RoomPlmMotherBrainFakeDeathVisualCatalog ReExtractMotherBrainFakeDeathStock(ISnesAddressSpace rom, string refreshed, string overrides)
    {
        RoomPlmMotherBrainFakeDeathVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
        return RoomPlmMotherBrainFakeDeathVisualFiles.Load(refreshed, overrides);
    }
}
