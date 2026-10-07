internal static partial class AssetTools
{
    /// <summary>Runs the developer tool named by <paramref name="args"/>, or returns null when none matches.</summary>
    internal static int? Run(string[] args)
    {
        switch (args)
        {
            case ["--dump-instruction-layouts", var outputPath]:
                DumpInstructionLayouts(outputPath);
                return 0;
            case ["--generate-enemy-visual-selectors"]:
                return GenerateEnemyVisualSelectors();
            case ["--generate-room-fx-records"]:
                GenerateRoomFxRecordDefinitions();
                return 0;
            case ["--generate-room-level-stream-corpus"]:
                GenerateRoomLevelStreamCorpus();
                return 0;
            case ["--shutter-native-arc"]:
                return ExportShutterBombArc();
            case ["--shutter-arc-trace"]:
                return TraceShutterBombArc();
            case ["--shutter-morph-approaches"]:
                return SweepShutterMorphApproaches();
            case ["--export-shallow-water-jump"]:
                return ExportShallowWaterJumpSeed();
            case ["--aerial-spread-trace", var aerialTrace]:
                return WriteBombSpreadTrace(aerialTrace, wallRoute: false);
            case ["--wall-spread-trace", var wallTrace]:
                return WriteBombSpreadTrace(wallTrace, wallRoute: true);
            default:
                return null;
        }
    }
}
