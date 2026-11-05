using SuperMetroid.AssetExtraction;
using SuperMetroid.Core;
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

/// <summary>Checks downward-gate PLM art and its column and trigger collision.</summary>
internal static class DownwardGateVisualsVerification
{
    /// <summary>Checks the stock column tile through actual PLM drawing and later camera streaming, and catalog rejections.</summary>
    internal static void VerifyDownwardGateVisuals()
    {
        InstalledRoomPlmDownwardGateVisuals stock = InstalledRoomPlmDownwardGateVisuals.Stock();
        RoomPlmVisualRunEntry[] entries = RoomPlmDownwardGateVisualFiles.All
            .Select(list => new RoomPlmVisualRunEntry(
                RoomPlmDownwardGateVisualFiles.VisualId(list.Draw),
                list.Runs.Span
                    .SelectToArray(run => run.Blocks.Span
                        .SelectToArray(block => block.Appearance))
                    ))
            .ToArray();
        // The synthetic population's header begins at the opening list, which
        // selects the sixth resident column frame before its gate pre-instruction.
        AssertEqual(RoomBlockAppearance.Of(0x0ff), stock.GetAppearance(NativePlmDraws.At<DownwardGateDraw>(0xa55d), 0, 1),
            "stock gate visual retains the physical column art");

        DownwardGateVisualsRenderResult
            Render(InstalledRoomPlmDownwardGateVisuals visuals)
        {
            (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms, int gateBlockIndex) = CreateDownwardGateFixture(
                    DownwardGateTriggerBehavior.BlueLeft, visuals);
            StepDownwardGatePlm(plms, level);
            AssertEqual(RoomBlockAppearance.Of(0x0ff),
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
        AssertEqual(RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.ShootableBlock), native.Physical,
            "stock gate draw retains its physical second-column word");
        AssertEqual(RoomBlockAppearance.Of(0x0ff), native.Streamed,
            "stock gate block streams its native block");

        AssertThrows<InvalidDataException>(
            () => RoomPlmDownwardGateVisualFiles.Create(entries.Skip(1)),
            "gate visual catalog rejects missing frames");
        AssertThrows<InvalidDataException>(
            () => RoomPlmDownwardGateVisualFiles.Create(entries.Select(entry =>
                entry.Id == "column-frame-5" ? entry with { Id = "unknown-frame" } : entry)),
            "gate visual catalog rejects unknown frame IDs");

        Suite(nameof(VerifyDownwardGateVisualInstallation), () => VerifyDownwardGateVisualInstallation());
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyDownwardGateVisualInstallation()
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmDownwardGateVisualDirectory);
        GameInstallation installation = copy.Installation;
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        RoomPlmDownwardGateVisualFilesConstants.ValidateStock(
            installation.RoomPlmDownwardGateVisualDirectory);
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Gate stock oracle revision");
        VerifyDownwardGateStockMapping(rom, installation.LoadRoomPlmDownwardGateVisuals());

        string stockPath = Path.Combine(installation.RoomPlmDownwardGateVisualDirectory,
            RoomPlmDownwardGateVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomPlmDownwardGateVisuals(),
            "gate stock manifest hash rejects corruption");
    }

}
