using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms only the identified closed-provider adapter contracts, without gameplay or asset files.</summary>
internal static class ClosedPresentationContractChecks
{
    internal static void Run()
    {
        string root = Directory.GetCurrentDirectory();
        var trees = ClosedPresentationContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root, source.Path)), path: source.Path))
            .ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<fixture-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Hardware;
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
                    colors.ApplyFlash(cgram, 1);
                    options.CreatePage(GameOptionsPresentationDefinitions.PrimaryPage);
                    options.ApplySpecialToggle(bytes, dynamicIndex > 0
                        ? GameOptionsPresentationDefinitions.IconCancelToggle
                        : GameOptionsPresentationDefinitions.MoonwalkToggle, false);
                    options.CreatePage(dynamicName);
                    gameOver.DrawBaby(oam, SuperMetroid.Core.Frontend.GameOverBabyFrame.Open);
                    gameOver.DrawBaby(oam, (SuperMetroid.Core.Frontend.GameOverBabyFrame)99);
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
                    spores.ResolveDeath(SuperMetroid.Core.Game.SporeSpawnDeathPaletteLayer.Level, 7, 0);
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
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
            "only reviewed valid-domain operations and installed constant names qualify; invalid/new/dynamic selections must fail");
        Require(report.Findings.Where(item => item.Code == AuditReport.Missing).Select(item => item.Resource)
            .Order(StringComparer.Ordinal).SequenceEqual(["Missing.Anchor", "Missing.Label"]),
            "a compiler-resolved uninstalled name must remain a concrete missing identity");
        Require(report.Consumers.Count == 49, "classifications must retain every inventoried call");

        ReviewedSource guard = ClosedPresentationContractDefinitions.All.Single(contract =>
            contract.Type == typeof(GameplayHudPresentation).FullName).Sources[0];
        SyntaxTree provider = trees.Single(tree => tree.FilePath == guard.Path);
        string original = provider.GetText().ToString();
        string changed = original.Replace("glyphs[(value / divisor) % 10]", "glyphs[(value / divisor) % 9]", StringComparison.Ordinal);
        Require(changed != original && ClosedPresentationAudit.MatchesReviewedSource(original, guard) &&
            !ClosedPresentationAudit.MatchesReviewedSource(changed, guard), "a changed selector must revoke the reviewed source proof");
        var changedCompilation = compilation.ReplaceSyntaxTree(provider, CSharpSyntaxTree.ParseText(changed, path: guard.Path));
        var changedExports = new ResourceIndex();
        var stale = new ClosedPresentationAudit(changedCompilation, changedExports);
        var staleReport = new AuditReport();
        InvocationExpressionSyntax hudCall = calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        ConsumerAudit.Inspect(hudCall, changedCompilation.GetSemanticModel(calls), changedExports, staleReport, stale);
        Require(staleReport.UnresolvedCount == 1 && staleReport.Classifications.Count == 0,
            "stale source must surface as a failing gap at the real consumer adapter");

        var extraUse = CSharpSyntaxTree.ParseText("class Mutator { void Change() { " +
            "SuperMetroid.Core.Assets.GameplayHudDefinitions.IconNames[0] = \"Other\"; } }", path: "fixture-mutator.cs");
        var mutatedCompilation = compilation.AddSyntaxTrees(extraUse);
        var mutatedExports = new ResourceIndex();
        var mutated = new ClosedPresentationAudit(mutatedCompilation, mutatedExports);
        var mutationReport = new AuditReport();
        ConsumerAudit.Inspect(hudCall, mutatedCompilation.GetSemanticModel(calls), mutatedExports, mutationReport, mutated);
        Require(mutationReport.UnresolvedCount == 1 && mutationReport.Classifications.Count == 0,
            "an unreviewed use of the mutable definition array must invalidate closure");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Closed provider confirmation failed: " + reason);
    }
}
