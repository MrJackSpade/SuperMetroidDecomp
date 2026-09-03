using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Validates complete pose-indexed rendering tables and every fixed suit/cannon/DMA
    /// range against the private retail ROM when it is available.
    /// </summary>
    static void VerifySamusRenderingRomData()
    {
        AssertTrue(
            SamusRenderingRomData.TileTransfers.TopDestinations.First <
                SamusRenderingRomData.TileTransfers.TopDestinations.Second,
            "Samus top-half DMA destinations retain native ordering");
        AssertTrue(
            SamusRenderingRomData.TileTransfers.BottomDestinations.First <
                SamusRenderingRomData.TileTransfers.BottomDestinations.Second,
            "Samus bottom-half DMA destinations retain native ordering");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus rendering ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int poseCount = typeof(SamusPoseIds)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(byte))
            .Select(field => (byte)field.GetRawConstantValue()!)
            .Max() + 1;

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

        TouchRange(bus, SamusRenderingRomData.TileTransfers.AnimationDefinitionListPointers,
            poseCount * sizeof(ushort), "pose animation-definition pointers");
        TouchRange(bus, SamusRenderingRomData.TileTransfers.TopDefinitionListPointers,
            SamusRenderingRomData.TileTransfers.TopDefinitionSetCount * sizeof(ushort),
            "top DMA-definition pointers");
        TouchRange(bus, SamusRenderingRomData.TileTransfers.BottomDefinitionListPointers,
            SamusRenderingRomData.TileTransfers.BottomDefinitionSetCount * sizeof(ushort),
            "bottom DMA-definition pointers");

        // Every pose must lead to a complete four-byte selector record in bank $92. This
        // checks the indirection used by live rendering, not only the pointer table itself.
        for (int pose = 0; pose < poseCount; pose++)
        {
            ushort pointer = ReadRenderingWord(
                bus,
                SamusRenderingRomData.TileTransfers.AnimationDefinitionListPointers +
                    pose * sizeof(ushort));
            AssertTrue(pointer >= 0x8000,
                $"pose ${pose:X2} graphics selector remains in bank $92 ROM");
            TouchRange(
                bus,
                SamusRenderingRomData.Banks.GraphicsDefinitions | pointer,
                SamusRenderingRomData.TileTransfers.AnimationRecordByteCount,
                $"pose ${pose:X2} frame-zero graphics selector");
        }

        TouchRange(bus, SamusRenderingRomData.ArmCannon.OpenFlags,
            6, "arm-cannon HUD open flags");
        TouchRange(bus, SamusRenderingRomData.ArmCannon.PoseDrawingDataPointers,
            poseCount * sizeof(ushort), "arm-cannon pose drawing pointers");
        TouchRange(bus, SamusRenderingRomData.ArmCannon.SpriteAttributes,
            SamusRenderingRomData.ArmCannon.DirectionCount * sizeof(ushort),
            "arm-cannon sprite attributes");
        TouchRange(bus, SamusRenderingRomData.ArmCannon.TileListPointers,
            SamusRenderingRomData.ArmCannon.DirectionCount * sizeof(ushort),
            "arm-cannon tile-list pointers");

        Console.WriteLine(
            $"  Samus rendering ROM data: {poseCount} pose records, three suit palettes, " +
            "body DMA definitions, and all arm-cannon directions are in range.");
    }

    private static ushort ReadRenderingWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
