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
    /// <summary>Compiles 44 constructed consumer calls to check reviewed provider classifications, invalid selections, and mutable-source invalidation.</summary>
    internal static void Run()
    {
        string root = Directory.GetCurrentDirectory();
        var trees = ClosedPresentationContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(root, source.Path)), path: source.Path))
            .ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        const string consumer = """
            using SuperMetroid.AssetExtraction.Artwork;
            using SuperMetroid.AssetExtraction.Samus.Artwork;
            using SuperMetroid.AssetExtraction.Enemies.Artwork;
            using SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;
            using SuperMetroid.AssetExtraction.Bosses.Crocomire.Artwork;
            using SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;
            using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
            using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;
            using SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;
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
            using SuperMetroid.Core.Frontend.Options;
            using SuperMetroid.Core.Frontend.Options.Artwork;
            using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
            using SuperMetroid.Core.Frontend.Pause.Artwork;
            using SuperMetroid.Core.Hud.Artwork;
            using SuperMetroid.Core.Samus.Artwork;
            class Consumer {
                void Inspect(GameplayHudPresentation hud, FileSelectPresentation menu,
                    MotherBrainRoomColorPresentation colors, GameOptionsPresentation options,
                    GameOverPresentation gameOver, PauseReserveUiPresentation reserve, PauseReserveArrowColours arrow,
                    InstalledBeamPalettes beam, CrocomireColorCatalog crocomire,
                    SporeSpawnColorCatalog spores, DraygonColorCatalog draygon, PhantoonColorCatalog phantoon,
                    TourianStatuePalettes statues,
                    MotherBrainRainbowBeamColours rainbow, MotherBrainDeathColorCatalog death,
                    ChozoAndTubeColorCatalog chozo,
                    InstalledSamusDeathPalettes samusDeath,
                    Span<ushort> tiles, Span<byte> bytes, Span<BackgroundTile> cells,
                    ScreenSprites oam, SpriteSheet sheet, int dynamicIndex) {
                    hud.Ammo((SuperMetroid.Core.Hud.HudAmmoCounter)dynamicIndex, 123);
                    hud.MinimapCell(new SuperMetroid.Core.Spatial.Position16<SuperMetroid.Core.Spatial.MapTiles>(99, 0), SuperMetroid.Core.Hud.MinimapHiddenCell.Instance);
                    FileSelectPresentation.DrawCursor(oam, sheet, dynamicIndex, new(0,0));
                    menu.DynamicAnchor(FileSelectAnchor.CopyConfirmSource);
                    gameOver.DrawBaby(oam, sheet, SuperMetroid.Core.Frontend.GameOver.GameOverBabyFrame.Open, SuperMetroid.Core.Frontend.GameOver.GameOverBabyColouring.Idle);
                    gameOver.DrawBaby(oam, sheet, (SuperMetroid.Core.Frontend.GameOver.GameOverBabyFrame)99, SuperMetroid.Core.Frontend.GameOver.GameOverBabyColouring.Idle);
                    reserve.ApplyLabel(cells, dynamicIndex > 0 ? PauseReserveLabel.Auto : PauseReserveLabel.Manual);
                    _ = arrow.Pulse(dynamicIndex);
                    reserve.ApplyDigit(cells, 0, 10);
                    _ = beam.Colours(0);
                    _ = beam.Colours(SuperMetroid.Core.Progression.SamusBeamCombination.SpazerPlasma);
                    spores.ResolveSpore(0);
                    phantoon.ResolvePowerOn(SuperMetroid.AssetExtraction.Enemies.Artwork.WreckedShipPoweredPart.Lights, 15);
                    phantoon.ResolvePowerOn(SuperMetroid.AssetExtraction.Enemies.Artwork.WreckedShipPoweredPart.Lights, 16);
                    statues.ResolveEye(3, 0);
                    statues.ResolveEye(4, 0);
                    _ = rainbow.BeamColor(37);
                    _ = rainbow.BeamColor(38);
                    death.BodyColor(15, 13);
                    death.CorpseColor(8, 0);
                    chozo.ResolveTubeCracks(31);
                    chozo.ResolveTubeCracks(0);
                    samusDeath.SuitedColor(2, 9, 15);
                    samusDeath.SuitedColor(3, 9, 15);
                    samusDeath.ExplosionPaletteIndex(8);
                    samusDeath.ExplosionPaletteIndex(9);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("ClosedProviderFixture", trees,
            platforms.Append(typeof(GameplayHudPresentation).Assembly.Location)
                .Append(typeof(SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseReserveUiPresentation).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        FixtureConsumerBinding.Require(compilation, calls, "ClosedPresentationContractChecks");
        var exports = new ResourceIndex();
        var adapter = new ClosedPresentationAudit(compilation, exports);
        var before = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, before);
        Require(before.UnresolvedCount == 26, "the fixture must reproduce the identified missing-adapter boundaries first" + " (" + before.Describe() + ")");
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        Require(report.Classifications.Count == 16 && report.MissingCount == 0 && report.UnresolvedCount == 10,
            $"only reviewed valid-domain operations qualify; invalid selections must fail " +
            $"(classified {report.Classifications.Count}/16, missing {report.MissingCount}/0, unresolved {report.UnresolvedCount}/10; " +
            $"revoked: {string.Join(", ", adapter.InvalidContracts)}; unresolved: " +
            string.Join(" | ", report.Findings.Where(finding => finding.Code != AuditReport.Missing).Select(finding => finding.Owner + " " + finding.Message)) + ")" + " (" + report.Describe() + ")");
        Require(report.Consumers.Count == 26, $"classifications must retain every inventoried call (consumers {report.Consumers.Count}/26; unbound: " +
            string.Join(" | ", report.Findings.Where(finding => finding.Message.Contains("could not be bound", StringComparison.Ordinal)).Select(finding => finding.Message)) + ")" + " (" + report.Describe() + ")");

        InvocationExpressionSyntax hudCall = calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>().First();

        var extraUse = CSharpSyntaxTree.ParseText("class Mutator { void Change() { " +
            "SuperMetroid.Core.Hud.Artwork.GameplayHudDefinitions.IconNames[0] = \"Other\"; } }", path: "fixture-mutator.cs");
        var mutatedCompilation = compilation.AddSyntaxTrees(extraUse);
        var mutatedExports = new ResourceIndex();
        var mutated = new ClosedPresentationAudit(mutatedCompilation, mutatedExports);
        var mutationReport = new AuditReport();
        ConsumerAudit.Inspect(hudCall, mutatedCompilation.GetSemanticModel(calls), mutatedExports, mutationReport, mutated);
        Require(mutationReport.UnresolvedCount == 1 && mutationReport.Classifications.Count == 0,
            "an unreviewed use of the mutable definition array must invalidate closure" + " (" + mutationReport.Describe() + ")");
    }

    /// <summary>Reports the exact closed-provider contract predicate that failed.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Closed provider confirmation failed: " + reason);
    }
}
