using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Samus.Movement.SamusMovementRomDataVerification;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Samus.Drawing;

/// <summary>Audits pose-indexed Samus rendering tables and fixed artwork ranges against the retail ROM.</summary>
internal static class SamusRenderingRomDataVerification
{
    /// <summary>
    /// Validates complete pose-indexed rendering tables and every fixed suit/cannon
    /// range against the private retail ROM when it is available.
    /// </summary>
    internal static void VerifySamusRenderingRomData()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus rendering ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        int poseCount = SamusBodyArtworkCatalog.PoseCount;

        TouchRange(bus, SamusRenderingRomData.Body.TopSpritemapBaseIndices,
            poseCount * sizeof(ushort), "top spritemap base-index table");
        TouchRange(bus, SamusRenderingRomData.Body.BottomSpritemapBaseIndices,
            poseCount * sizeof(ushort), "bottom spritemap base-index table");
        foreach (int palette in new[]
        {
            SamusRenderingRomData.Body.PowerSuitPalette,
            SamusRenderingRomData.Body.VariaSuitPalette,
            SamusRenderingRomData.Body.GravitySuitPalette,
        })
        {
            TouchRange(bus, palette,
                SamusRenderingRomData.Body.SuitPaletteColorCount * sizeof(ushort),
                $"suit palette ${palette:X6}");
        }

        TouchRange(bus, SamusCartridgeTables.AnimationDefinitionListPointers,
            poseCount * sizeof(ushort), "pose animation-definition pointers");
        TouchRange(bus, SamusCartridgeTables.TopDefinitionListPointers,
            SamusBodyFrameTableConstants.TopDefinitionSetCount * sizeof(ushort),
            "top DMA-definition pointers");
        TouchRange(bus, SamusCartridgeTables.BottomDefinitionListPointers,
            SamusBodyFrameTableConstants.BottomDefinitionSetCount * sizeof(ushort),
            "bottom DMA-definition pointers");

        // Every pose must lead to a complete four-byte selector record in bank $92. This
        // checks the indirection used by live rendering, not only the pointer table itself.
        for (int pose = 0; pose < poseCount; pose++)
        {
            ushort pointer = CartridgeImportSource.Require(bus).ReadCartridgeWord((SamusCartridgeTables.AnimationDefinitionListPointers +
                    pose * sizeof(ushort)));
            AssertTrue(pointer >= 0x8000,
                $"pose ${pose:X2} graphics selector remains in bank $92 ROM");
            TouchRange(
                bus,
                SamusRenderingRomDataBanksConstants.GraphicsDefinitions | pointer,
                SamusBodyFrameTableConstants.AnimationRecordByteCount,
                $"pose ${pose:X2} frame-zero graphics selector");
        }

        TouchRange(bus, SamusCartridgeTables.PoseDrawingDataPointers,
            poseCount * sizeof(ushort), "arm-cannon pose drawing pointers");
        TouchRange(bus, SamusRenderingRomData.ArmCannon.SpriteAttributes,
            SamusRenderingRomData.ArmCannon.DirectionCount * sizeof(ushort),
            "arm-cannon sprite attributes");
        TouchRange(bus, SamusCartridgeTables.TileListPointers,
            SamusRenderingRomData.ArmCannon.DirectionCount * sizeof(ushort),
            "arm-cannon tile-list pointers");

        Console.WriteLine(
            $"  Samus rendering ROM data: {poseCount} pose records, three suit palettes, " +
            "body DMA definitions, and all arm-cannon directions are in range.");
    }
}
