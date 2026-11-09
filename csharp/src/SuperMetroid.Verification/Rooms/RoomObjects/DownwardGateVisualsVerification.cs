using SuperMetroid.AssetExtraction;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Rooms.RoomObjects.DownwardGateObjectsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupDownwardGateStockVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks downward-gate PLM art and its column and trigger collision.</summary>
internal static class DownwardGateVisualsVerification
{
    /// <summary>Checks the stock column tile through actual PLM drawing and later camera streaming, and catalog rejections.</summary>
    internal static void VerifyDownwardGateVisuals()
    {
        InstalledRoomObjectDownwardGateVisuals stock = InstalledRoomObjectDownwardGateVisuals.Stock();
        // The synthetic population's header begins at the opening list, which
        // selects the sixth resident column frame before its gate pre-instruction.
        RoomObjectDraw sixthColumn = NativeObjectDraws.At<DownwardGateDraw>(0xa55d);
        AssertEqual(RoomBlockAppearance.Of(0x0ff),
            stock.Image(sixthColumn).At(RoomObjectDownwardGateVisualFiles.Family.Layouts[sixthColumn].Runs.Span[0].CellAt(1)),
            "stock gate visual retains the physical column art");

        DownwardGateVisualsRenderResult
            Render()
        {
            (TestAddressSpace bus, RoomLevelData level, RoomObjectSystem roomObjects, int gateBlockIndex) = CreateDownwardGateFixture(
                    DownwardGateTriggerBehavior.BlueLeft);
            StepDownwardGateObject(roomObjects, level);
            AssertEqual(RoomBlockAppearance.Of(0x0ff),
                level.DisplayedAppearance(gateBlockIndex + level.Size.X),
                "first gate PLM step draws its selected second-column visual word");
            StepDownwardGateObject(roomObjects, level);
            int columnIndex = gateBlockIndex + level.Size.X;
            int triggerIndex = gateBlockIndex - 1;
            RoomBlockAppearance streamed = level.DisplayedAppearance(columnIndex);
            RoomBlockAppearance triggerStreamed = level.DisplayedAppearance(triggerIndex);
            return new DownwardGateVisualsRenderResult(level.GetCollisionBlockByIndex(columnIndex).Terrain,
                streamed,
                level.GetCollisionBlockByIndex(triggerIndex).Terrain,
                triggerStreamed);
        }

        var native = Render();
        AssertEqual(RoomTerrainBlock.Of(0x0ff, BlockMirroring.None, RoomCollisionType.ShootableBlock), native.Physical,
            "stock gate draw retains its physical second-column word");
        AssertEqual(RoomBlockAppearance.Of(0x0ff), native.Streamed,
            "stock gate block streams its native block");


        Suite(nameof(VerifyDownwardGateVisualInstallation), () => VerifyDownwardGateVisualInstallation());
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyDownwardGateVisualInstallation()
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectDownwardGateVisualDirectory);
        GameInstallation installation = copy.Installation;
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        RoomObjectDownwardGateVisualFilesConstants.ValidateStock(
            installation.RoomObjectDownwardGateVisualDirectory);
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Gate stock oracle revision");
        VerifyDownwardGateStockMapping(rom, installation.LoadRoomObjectDownwardGateVisuals());

        string stockPath = Path.Combine(installation.RoomObjectDownwardGateVisualDirectory,
            RoomObjectDownwardGateVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectDownwardGateVisuals(),
            "gate stock manifest hash rejects corruption");
    }
}
