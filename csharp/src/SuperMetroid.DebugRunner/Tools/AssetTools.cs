/// <summary>
/// Developer tools that export cartridge source evidence and generate checked-in definition
/// catalogs. They run from the repository root against the repository ROM; their outputs go to
/// ignored <c>csharp/test-temp</c> folders or to the generated source files they own.
/// </summary>
internal static class AssetTools
{
    /// <summary>Runs the developer tool named by <paramref name="args"/>, or returns null when none matches.</summary>
    internal static int? Run(string[] args)
    {
        switch (args)
        {
            case ["--dump-instruction-layouts", var outputPath]:
                InstructionLayoutDump.DumpInstructionLayouts(outputPath);
                return 0;
            case ["--generate-enemy-visual-selectors"]:
                return EnemyVisualSelectorGenerator.GenerateEnemyVisualSelectors();
            case ["--generate-cpu-bus-inventory"]:
                return CpuBusInventoryGenerator.GenerateCpuBusInventory();
            case ["--generate-ppu-layout-inventory"]:
                return PpuLayoutInventoryGenerator.GeneratePpuLayoutInventory();
            case ["--generate-room-fx-records"]:
                RoomFxRecordGenerator.GenerateRoomFxRecordDefinitions();
                return 0;
            case ["--generate-room-level-stream-corpus"]:
                RoomLevelStreamCorpus.GenerateRoomLevelStreamCorpus();
                return 0;
            case ["--shutter-native-arc"]:
                return ShutterNativeArc.ExportShutterBombArc();
            case ["--shutter-arc-trace"]:
                return ShutterNativeArc.TraceShutterBombArc();
            case ["--shutter-morph-approaches"]:
                return ShutterNativeArc.SweepShutterMorphApproaches();
            case ["--export-shallow-water-jump"]:
                return NativeCaptureSeeds.ExportShallowWaterJumpSeed();
            case ["--aerial-spread-trace", var aerialTrace]:
                return NativeCaptureSeeds.WriteBombSpreadTrace(aerialTrace, wallRoute: false);
            case ["--wall-spread-trace", var wallTrace]:
                return NativeCaptureSeeds.WriteBombSpreadTrace(wallTrace, wallRoute: true);
            default:
                return null;
        }
    }
}
