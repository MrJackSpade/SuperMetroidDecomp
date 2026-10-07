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
            case ["--lookup-stream-4-hud-template-source"]:
                ExportLookupStream4HudTemplateSource(LoadRepositoryRom());
                return 0;
            case ["--lookup-stream-4-hud-auto-source"]:
                ExportLookupStream4HudAutoSource(LoadRepositoryRom());
                return 0;
            case ["--lookup-stream-4-draygon-health-source"]:
                ExportLookupStream4DraygonHealthSource(LoadRepositoryRom());
                return 0;
            case ["--lookup-stream-4-norfair-reveal-source"]:
                ExportLookupStream4NorfairRevealSource(LoadRepositoryRom());
                return 0;
            case ["--lookup-zebes-planet-art"]:
                ExportZebesPlanetArtwork(LoadRepositoryRom());
                return 0;
            case ["--lookup-ceres-large-blast-art"]:
                ExportCeresLargeBlastArtwork(LoadRepositoryRom());
                return 0;
            case ["--lookup-ceres-asteroid-art"]:
                ExportCeresAsteroidArtwork(LoadRepositoryRom());
                return 0;
            case ["--lookup-ceres-placement-art"]:
                ExportCeresPlacementArtwork();
                return 0;
            case ["--lookup-intro-collision-art"]:
                ExportIntroCollisionArtwork(LoadRepositoryRom());
                return 0;
            case ["--lookup-ending-explosion-art"]:
                ExportEndingExplosionArtworkEvidence(LoadRepositoryRom());
                return 0;
            case ["--lookup-ending-font-layout"]:
                ExportEndingFontLayout();
                return 0;
            case ["--lookup-ending-gunship-art"]:
                ExportEndingGunshipPaletteEvidence();
                return 0;
            case ["--lookup-ending-logo-color-art"]:
                ExportEndingLogoPaletteEvidence(LoadRepositoryRom());
                return 0;
            case ["--lookup-ending-reward-arm-art"]:
                ExportEndingRewardGestureArtwork(LoadRepositoryRom(), suitless: false);
                return 0;
            case ["--lookup-ending-reward-hair-art"]:
                ExportEndingRewardGestureArtwork(LoadRepositoryRom(), suitless: true);
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

    /// <summary>Writes the retail ending font atlas for comparison with the installed font.</summary>
    private static void ExportEndingFontLayout()
    {
        string output = Path.GetFullPath("csharp/test-temp/1165-ending-font-original.png");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllBytes(output, SuperMetroid.AssetExtraction.EndingFontAtlasExtractor.Extract(LoadRepositoryRom()));
        Console.WriteLine(output);
    }
}
