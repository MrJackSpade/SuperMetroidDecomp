using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Confirms only the identified closed-provider adapter contracts, without gameplay or asset files.</summary>
internal static class ClosedPresentationContractChecks
{
    /// <summary>Compiles 49 constructed consumer calls to check reviewed provider classifications, missing names, invalid selections, and mutable-source invalidation.</summary>
    internal static void Run()
    {
        string root = Directory.GetCurrentDirectory();
        var trees = ClosedPresentationContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root, source.Path)), path: source.Path))
            .ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        const string consumer = """
            using SuperMetroid.Core.Areas.Tourian.Artwork;
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Bosses.Crocomire.Artwork;
            using SuperMetroid.Core.Bosses.Draygon.Artwork;
            using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
            using SuperMetroid.Core.Bosses.Phantoon.Artwork;
            using SuperMetroid.Core.Bosses.Ridley.Artwork;
            using SuperMetroid.Core.Bosses.SporeSpawn;
            using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;
            using SuperMetroid.Core.Enemies.Artwork;
            using SuperMetroid.Core.Frontend.FileSelect.Artwork;
            using SuperMetroid.Core.Frontend.GameOver;
            using SuperMetroid.Core.Frontend.GameOver.Artwork;
            using SuperMetroid.Core.Frontend.Options.Artwork;
            using SuperMetroid.Core.Frontend.Pause.Artwork;
            using SuperMetroid.Core.Hardware;
            using SuperMetroid.Core.Hud.Artwork;
            using SuperMetroid.Core.Samus.Artwork;
            class Consumer {
                void Inspect(GameplayHudPresentation hud, FileSelectPresentation menu,
                    MotherBrainRoomColorPresentation colors, GameOptionsPresentation options,
                    GameOverPresentation gameOver, PauseReserveUiPresentation reserve,
                    BeamPaletteCatalog beam, CeresRidleyColorCatalog ridley, CrocomireColorCatalog crocomire,
                    SporeSpawnColorCatalog spores, DraygonColorCatalog draygon, PhantoonColorCatalog phantoon,
                    TourianStatueColorCatalog statues,
                    MotherBrainRainbowPalettePresentation rainbow, MotherBrainDeathColorCatalog death,
                    ChozoAndTubeColorCatalog chozo, GameplayBasePaletteCatalog basePalette,
                    SamusDeathPaletteArtworkCatalog samusDeath,
                    SnesCgram cgram, Span<ushort> tiles, Span<byte> bytes,
                    OamBuffer oam, int dynamicIndex, string dynamicName) {
                    hud.ApplyAmmo(tiles, dynamicIndex, 123);
                    hud.MinimapCellIndex(99, 0);
                    menu.DrawCursor(oam, dynamicIndex, new(0,0));
                    menu.DynamicAnchor(FileSelectPresentationDefinitions.CopyConfirmSourceAnchor);
                    menu.DynamicAnchor("Missing.Anchor");
                    menu.ApplyPatch(tiles, dynamicName, new(0,0));
                    menu.CopyPage(dynamicName, tiles);
                    colors.ApplyRecoveryLights(cgram, dynamicIndex);
                    colors.ApplyFlash(cgram, 14);
                    options.CreatePage(GameOptionsPresentationDefinitions.PrimaryPage);
                    options.ApplySpecialToggle(bytes, dynamicIndex > 0
                        ? GameOptionsPresentationDefinitions.IconCancelToggle
                        : GameOptionsPresentationDefinitions.MoonwalkToggle, false);
                    options.CreatePage(dynamicName);
                    gameOver.DrawBaby(oam, SuperMetroid.Core.Frontend.GameOver.GameOverBabyFrame.Open);
                    gameOver.DrawBaby(oam, (SuperMetroid.Core.Frontend.GameOver.GameOverBabyFrame)99);
                    reserve.ApplyLabel(bytes, dynamicIndex > 0 ? "Auto" : "Manual");
                    reserve.ApplyLabel(bytes, dynamicIndex > 0 ? "Auto" : "Missing.Label");
                    reserve.ApplyArrowColors(cgram, true, dynamicIndex, 6, 11);
                    reserve.ApplyDigit(bytes, 0, 10);
                    beam.LoadTo(cgram, 0);
                    beam.LoadTo(cgram, 12);
                    ridley.ApplyHealth(cgram, 0);
                    ridley.ApplyHealth(cgram, 3);
                    crocomire.ApplyFightBody(cgram);
                    crocomire.ResolveFightBody(0);
                    spores.ResolveSpore(0);
                    spores.ResolveDeath(SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnDeathPaletteLayer.Level, 7, 0);
                    draygon.ApplyHealthBand(cgram, 14);
                    draygon.ApplyHealthBand(cgram, 1);
                    draygon.ApplyHurt(cgram, true, 1);
                    phantoon.ResolvePowerOn(111);
                    phantoon.ResolvePowerOn(112);
                    statues.ApplyEye(cgram, 6);
                    statues.ApplyEye(cgram, 1);
                    rainbow.TryReadBeamColor(152, out _);
                    rainbow.TryReadBeamColor(154, out _);
                    rainbow.ApplyRainbow(cgram, 9);
                    rainbow.ApplyRainbow(cgram, 10);
                    rainbow.ApplyFakeDeathToGrey(cgram, 7);
                    rainbow.ApplyFakeDeathFromGrey(cgram, 8);
                    death.BodyColor(15, 13);
                    death.CorpseColor(8, 0);
                    chozo.ApplyLowerNorfair(cgram);
                    chozo.ResolveLowerNorfair(0);
                    basePalette.LoadEnemyProjectileSprites(cgram, dynamicIndex);
                    samusDeath.SuitedColor(2, 9, 15);
                    samusDeath.SuitedColor(3, 9, 15);
                    samusDeath.ExplosionPaletteIndex(8);
                    samusDeath.ExplosionPaletteIndex(9);
                    rainbow.ApplyFromGrey(null!, cgram, 7);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("ClosedProviderFixture", trees,
            platforms.Append(typeof(GameplayHudPresentation).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var exports = new ResourceIndex();
        var adapter = new ClosedPresentationAudit(compilation, exports);
        var before = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, before);
        Require(before.UnresolvedCount == 49, "the fixture must reproduce the identified missing-adapter boundaries first");
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        Require(report.Classifications.Count == 26 && report.MissingCount == 2 && report.UnresolvedCount == 21,
            $"only reviewed valid-domain operations and installed constant names qualify; invalid/new/dynamic selections must fail " +
            $"(classified {report.Classifications.Count}/26, missing {report.MissingCount}/2, unresolved {report.UnresolvedCount}/21; " +
            $"revoked: {string.Join(", ", adapter.InvalidContracts)}; unresolved: " +
            string.Join(" | ", report.Findings.Where(finding => finding.Code != AuditReport.Missing).Select(finding => finding.Owner + " " + finding.Message)) + ")");
        Require(report.Findings.Where(item => item.Code == AuditReport.Missing).Select(item => item.Resource)
            .Order(StringComparer.Ordinal).SequenceEqual(["Missing.Anchor", "Missing.Label"]),
            "a compiler-resolved uninstalled name must remain a concrete missing identity");
        Require(report.Consumers.Count == 49, $"classifications must retain every inventoried call (consumers {report.Consumers.Count}/49; unbound: " +
            string.Join(" | ", report.Findings.Where(finding => finding.Message.Contains("could not be bound", StringComparison.Ordinal)).Select(finding => finding.Message)) + ")");

        InvocationExpressionSyntax hudCall = calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var extraUse = CSharpSyntaxTree.ParseText("class Mutator { void Change() { " +
            "SuperMetroid.Core.Hud.Artwork.GameplayHudDefinitions.IconNames[0] = \"Other\"; } }", path: "fixture-mutator.cs");
        var mutatedCompilation = compilation.AddSyntaxTrees(extraUse);
        var mutatedExports = new ResourceIndex();
        var mutated = new ClosedPresentationAudit(mutatedCompilation, mutatedExports);
        var mutationReport = new AuditReport();
        ConsumerAudit.Inspect(hudCall, mutatedCompilation.GetSemanticModel(calls), mutatedExports, mutationReport, mutated);
        Require(mutationReport.UnresolvedCount == 1 && mutationReport.Classifications.Count == 0,
            "an unreviewed use of the mutable definition array must invalidate closure");
    }

    /// <summary>Reports the exact closed-provider contract predicate that failed.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Closed provider confirmation failed: " + reason);
    }
}
