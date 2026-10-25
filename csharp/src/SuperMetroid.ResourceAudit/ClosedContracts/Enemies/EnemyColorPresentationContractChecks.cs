using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms identified enemy color coverage and exact short/long selections using constructed source only.</summary>
internal static class EnemyColorPresentationContractChecks
{
    /// <summary>Confirms audited color domains admit valid calls while preserving findings for short or invalid selections.</summary>
    internal static void Run()
    {
        var trees = RemainingEnemyColorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        const string consumer = """
            using SuperMetroid.Core.Areas.Ceres;
            using SuperMetroid.Core.Bosses.Botwoon;
            using SuperMetroid.Core.Bosses.Botwoon.Artwork;
            using SuperMetroid.Core.Bosses.Kraid;
            using SuperMetroid.Core.Bosses.Kraid.Artwork;
            using SuperMetroid.Core.Bosses.MotherBrain;
            using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
            using SuperMetroid.Core.Bosses.Ridley;
            using SuperMetroid.Core.Bosses.Ridley.Artwork;
            using SuperMetroid.Core.Enemies.Animals;
            using SuperMetroid.Core.Enemies.Artwork;
            class EnemyColorConsumer {
                void Inspect(EnemyAuxiliaryColorCatalog auxiliary, KraidColorCatalog kraid, BotwoonColorCatalog botwoon,
                    BabyMetroidCutsceneColorCatalog baby, NorfairRidleyColorCatalog ridley, ZebetiteColorCatalog zebetite,
                    ShitroidColorCatalog shitroid, CeresRidleyFloorColorCatalog mode7, DachoraColorCatalog dachora,
                    MotherBrainHealthPalettePresentation motherBrain, SuperMetroid.Core.Assets.GameplayColours cgram, SuperMetroid.Core.Assets.ColourRun pulse) {
                    auxiliary.Resolve(EnemyAuxiliaryPalette.FaceBlock, 7, 3);
                    auxiliary.Resolve(EnemyAuxiliaryPalette.FaceBlock, 7, 4);
                    auxiliary.Resolve(EnemyAuxiliaryPalette.DeadSidehopper, 7, 0);
                    auxiliary.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBody, 7, 15);
                    kraid.Resolve(KraidPaletteSource.Health, KraidPalettes.HealthBandCount * KraidPalettes.BandColors - 1);
                    kraid.Resolve(KraidPaletteSource.RoomBackdrop, KraidPalettes.BandColors);
                    botwoon.HealthColor(BotwoonHealthPaletteDefinitions.PaletteCount - 1, BotwoonHealthPaletteDefinitions.ColorsPerPalette - 1);
                    botwoon.HealthColor(BotwoonHealthPaletteDefinitions.PaletteCount, 0);
                    baby.InitialColor(BabyMetroidCutsceneColorDefinitions.InitialColorCount - 1);
                    baby.InitialColor(BabyMetroidCutsceneColorDefinitions.InitialColorCount);
                    baby.FadeColor(BabyMetroidCutsceneColorDefinitions.FadeFrameCount, BabyMetroidCutsceneColorDefinitions.FadeColorCount - 1);
                    baby.FadeColor(0, 0);
                    baby.FadeColor(BabyMetroidCutsceneColorDefinitions.FadeFrameCount, BabyMetroidCutsceneColorDefinitions.FadeColorCount);
                    ridley.ApplyInitial(cgram);
                    ridley.ApplyReveal(cgram, NorfairRidleyPaletteDefinitions.RevealRowCount - 1);
                    ridley.ApplyReveal(cgram, NorfairRidleyPaletteDefinitions.RevealRowCount);
                    zebetite.Apply(pulse, ZebetiteColorFormat.FrameCount - 1);
                    zebetite.Apply(pulse, ZebetiteColorFormat.FrameCount);
                    shitroid.NormalColor(ShitroidColorDefinitions.NormalFrameCount - 1, ShitroidColorDefinitions.NormalColorsPerFrame - 1);
                    shitroid.NormalColor(0, ShitroidColorDefinitions.NormalColorsPerFrame);
                    shitroid.TargetColor(ShitroidColorTarget.DeadSidehopper, ShitroidColorDefinitions.TargetColorCount - 1);
                    shitroid.TargetColor((ShitroidColorTarget)int.MaxValue, 0);
                    mode7.Apply(cgram, CeresRidleyPaletteDefinitions.GetawayZoomRowCount - 1);
                    mode7.Apply(cgram, CeresRidleyPaletteDefinitions.GetawayZoomRowCount);
                    dachora.Resolve(DachoraPalettePhase.Shine, DachoraColorDefinitions.AnimatedFrameCount - 1, DachoraColorDefinitions.ColorsPerFrame - 1);
                    dachora.Resolve(DachoraPalettePhase.Default, 1, 0);
                    motherBrain.Apply(cgram, MotherBrainHealthPaletteFormat.StateCount - 1);
                    motherBrain.Apply(cgram, MotherBrainHealthPaletteFormat.StateCount);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-enemy-color-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("EnemyColorProviderFixture", trees,
            platforms.Append(typeof(BotwoonColorCatalog).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var exports = new ResourceIndex();
        var adapter = new ClosedPresentationAudit(compilation, exports);
        var before = new AuditReport();
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, before);
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        }
        Require(before.UnresolvedCount == 28, "twenty-eight calls must reproduce absent adapters first" + " (" + before.Describe() + ")");
        Require(report.Classifications.Count == 14 && report.UnresolvedCount == 14 && report.MissingCount == 0 &&
            report.Consumers.Count == 28, "supported domains qualify but short arrays, zero-based baby fade and invalid constants still fail" + " (" + report.Describe() + ")");
        foreach (string selected in new[] { "FaceBlock", "DeadSidehopper", "RoomBackdrop", "Default" })
            Require(report.Findings.Any(item => item.Message.Contains("selected " + selected, StringComparison.Ordinal)),
                "the " + selected + " selection must not borrow another resource's larger dimensions");
    }

    /// <summary>Reports an enemy-color closure regression through the audit process boundary.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Enemy color provider confirmation failed: " + reason);
    }
}
