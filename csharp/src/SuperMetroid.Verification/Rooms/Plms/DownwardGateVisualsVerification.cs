using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Rooms.Plms.DownwardGatePlmsVerification;
using static SuperMetroid.Verification.Rooms.Plms.LookupDownwardGateStockVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks editable downward-gate PLM art without changing column or trigger collision.</summary>
internal static class DownwardGateVisualsVerification
{
    /// <summary>Checks edited column and trigger tiles survive actual PLM drawing and later camera streaming.</summary>
    internal static void VerifyDownwardGateVisuals()
    {
        InstalledRoomPlmDownwardGateVisuals stock = InstalledRoomPlmDownwardGateVisuals.Stock();
        RoomPlmVisualRunEntry[] entries = DownwardGatePlmDraws.All
            .Select(list => new RoomPlmVisualRunEntry(
                DownwardGatePlmDraws.VisualId(list.Draw),
                list.Runs.Span
                    .SelectToArray(run => run.Blocks.Span
                        .SelectToArray(block => block.Appearance))
                    ))
            .ToArray();
        // The synthetic population's header begins at the opening list, which
        // selects the sixth resident column frame before its gate pre-instruction.
        RoomPlmVisualRunEntry column = entries.Single(entry =>
            entry.Id == "column-frame-5");
        column.Runs[0][1] = RoomBlockAppearance.Of(0x053);
        RoomPlmVisualRunEntry trigger = entries.Single(entry =>
            entry.Id == "blue-left-trigger");
        trigger.Runs[1][0] = RoomBlockAppearance.Of(0x053);
        var edited = new InstalledRoomPlmDownwardGateVisuals(entries);
        column.Runs[0][1] = RoomBlockAppearance.Of(0x054);
        AssertEqual(RoomBlockAppearance.Of(0x053), edited.GetAppearance(NativePlmDraws.At<DownwardGateDraw>(0xa55d), 0, 1),
            "gate visual catalog copies author data");
        AssertEqual(RoomBlockAppearance.Of(0x0ff), stock.GetAppearance(NativePlmDraws.At<DownwardGateDraw>(0xa55d), 0, 1),
            "stock gate visual retains the physical column art");

        DownwardGateVisualsRenderResult
            Render(InstalledRoomPlmDownwardGateVisuals visuals)
        {
            (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int gateBlockIndex) = CreateDownwardGateFixture(
                    DownwardGateTriggerBehavior.BlueLeft, visuals);
            StepDownwardGatePlm(plms, level);
            AssertEqual(visuals.GetAppearance(NativePlmDraws.At<DownwardGateDraw>(0xa55d), 0, 1) == RoomBlockAppearance.Of(0x053)
                    ? RoomBlockAppearance.Of(0x053) : RoomBlockAppearance.Of(0x0ff),
                level.DisplayedAppearance(gateBlockIndex + level.Size.X),
                "first gate PLM step draws its selected second-column visual word");
            StepDownwardGatePlm(plms, level);
            int columnIndex = gateBlockIndex + level.Size.X;
            int triggerIndex = gateBlockIndex - 1;
            RoomBlockAppearance streamed = level.DisplayedAppearance(columnIndex);
            RoomBlockAppearance triggerStreamed = level.DisplayedAppearance(triggerIndex);
            return new DownwardGateVisualsRenderResult(level.GetCollisionBlockByIndex(columnIndex).Terrain,
                streamed,
                level.GetCollisionBlockByIndex(triggerIndex).Terrain,
                triggerStreamed);
        }

        var native = Render(stock);
        var changed = Render(edited);
        AssertEqual(RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.ShootableBlock), native.Physical,
            "stock gate draw retains its physical second-column word");
        AssertEqual(native.Physical, changed.Physical,
            "gate block art cannot change column collision");
        AssertEqual(native.TriggerPhysical, changed.TriggerPhysical,
            "gate trigger art cannot change shot-block collision");
        AssertEqual(RoomBlockAppearance.Of(0x0ff), native.Streamed,
            "stock gate block streams its native block");
        AssertEqual(RoomBlockAppearance.Of(0x053), changed.Streamed,
            "edited gate block art survives camera streaming");
        AssertEqual(RoomBlockAppearance.Of(0x053), changed.TriggerStreamed,
            "edited trigger art survives camera streaming");

        AssertThrows<InvalidDataException>(
            () => new InstalledRoomPlmDownwardGateVisuals(entries.Skip(1)),
            "gate visual catalog rejects missing frames");
        column.Runs[0][1] = RoomBlockAppearance.Of(0x053);
        AssertThrows<InvalidDataException>(
            () => new InstalledRoomPlmDownwardGateVisuals(entries.Select(entry =>
                entry.Id == "column-frame-5" ? entry with { Id = "unknown-frame" } : entry)),
            "gate visual catalog rejects unknown frame IDs");

        Suite(nameof(VerifyDownwardGateVisualInstallation), () => VerifyDownwardGateVisualInstallation());
    }

    /// <summary>Checks extracted stock, override survival, and rejection of corrupt or physical-bit edits.</summary>
    private static void VerifyDownwardGateVisualInstallation()
    {
        using var testRootScratch = new TestScratchDirectory("downward-gate-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc"));
            RoomPlmDownwardGateVisualFiles.Extract(rom,
                installation.RoomPlmDownwardGateVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmDownwardGateVisualFiles.ValidateStock(
                installation.RoomPlmDownwardGateVisualDirectory);
            AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Gate stock oracle revision");
            VerifyDownwardGateStockMapping(rom, installation.LoadRoomPlmDownwardGateVisuals());

            string stockPath = Path.Combine(installation.RoomPlmDownwardGateVisualDirectory,
                RoomPlmDownwardGateVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted gate JSON is empty.");
            JsonNode column = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "column-frame-5");
            column.Required("runs").Required(0)[1] = PlmVisualJson.Appearance(0x053);
            Directory.CreateDirectory(installation.RoomPlmDownwardGateVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmDownwardGateVisualOverrideDirectory,
                RoomPlmDownwardGateVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual(RoomBlockAppearance.Of(0x053),
                installation.LoadRoomPlmDownwardGateVisuals().GetAppearance(NativePlmDraws.At<DownwardGateDraw>(0xa55d), 0, 1),
                "gate visual override selects edited column art");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmDownwardGateVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x053),
                RoomPlmDownwardGateVisualFiles.Load(refreshed,
                    installation.RoomPlmDownwardGateVisualOverrideDirectory).GetAppearance(NativePlmDraws.At<DownwardGateDraw>(0xa55d), 0, 1),
                "gate visual override survives stock replacement");

            column.Required("runs").Required(0)[1] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmDownwardGateVisuals(),
                "gate override rejects collision bits instead of silently restoring stock");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmDownwardGateVisuals(),
                "gate stock manifest hash rejects corruption");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
