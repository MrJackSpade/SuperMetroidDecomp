using SuperMetroid.AssetExtraction;

/// <summary>
/// Installation, Android-export and state-fixture tools that operate on caller-supplied files. The
/// Android release gate drives the installation commands; the rest serve local diagnostics.
/// </summary>
internal static class IntegrationTools
{
    /// <summary>Runs the tool named by <paramref name="args"/>, or returns null when none matches.</summary>
    internal static int? Run(string[] args)
    {
        switch (args)
        {
            case ["--validate-extracted-installation", var extractedRoot]:
                var validated = GameAssetInstaller.ValidateExtractedContent(extractedRoot);
                Console.WriteLine($"PASS all required extracted resources: {validated.ContentDirectory}; no ROM import or repair.");
                return 0;
            case ["--prepare-extracted-installation", var importRom, var importRoot]:
                if (Directory.Exists(importRoot))
                    throw new IOException($"The diagnostic import destination already exists: {importRoot}");
                var installed = GameAssetInstaller.Install(importRom, importRoot);
                GameAssetInstaller.ValidateExtractedContent(installed.Root);
                Console.WriteLine($"PASS fresh extracted installation: {installed.Root}; source ROM was imported, not mapped into gameplay.");
                return 0;
            case ["--compare-assembly-metadata", var originalAssembly, var linkedAssembly]:
                return AssemblyMetadataVerification.Run(originalAssembly, linkedAssembly);
            case ["--compare-file-select-capture", var journalPath, var wavePath]:
                return FileSelectCaptureComparison.Run(journalPath, wavePath);
            case ["--replay-android-bundle", var bundle, var recordingName]:
                return AndroidBundleReplay.Run(bundle, recordingName);
            case ["--export-autonomous-performance-state", var frameText, var autonomousDestination]:
                return AutonomousPerformanceStateFixture.Export(int.Parse(frameText), autonomousDestination);
            case ["--export-ceres-descent-state", var destination]:
                return CeresDescentStateFixture.Export(destination);
            case ["--export-reported-eye-state", var eyeDestination]:
                return ReportedEyeStateFixture.Export(eyeDestination);
            case ["--export-room-performance-state", var scene, var performanceDestination]:
                return RoomPerformanceStateFixture.Export(scene, performanceDestination);
            default:
                return null;
        }
    }
}
