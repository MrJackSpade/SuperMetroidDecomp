using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms identified enemy color coverage and exact short/long selections using constructed source only.</summary>
internal static class EnemyColorPresentationContractChecks
{
    /// <summary>Confirms the reviewed enemy color presentation providers and selector domains.</summary>
    internal static void Run()
    {
        var trees = RemainingEnemyColorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<enemy-color-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            using SuperMetroid.Core.Hardware;
            class EnemyColorConsumer {
                void Inspect(EnemyAuxiliaryColorCatalog auxiliary, KraidColorCatalog kraid, BotwoonColorCatalog botwoon,
                    BabyMetroidCutsceneColorCatalog baby, NorfairRidleyColorCatalog ridley, ZebetiteColorCatalog zebetite,
                    ShitroidColorCatalog shitroid, CeresRidleyMode7ColorCatalog mode7, DachoraColorCatalog dachora,
                    MotherBrainHealthPalettePresentation motherBrain, SnesCgram cgram, int destination) {
                    auxiliary.Resolve(EnemyAuxiliaryPalette.FaceBlock, 7, 3);
                    auxiliary.Resolve(EnemyAuxiliaryPalette.FaceBlock, 7, 4);
                    auxiliary.Resolve(EnemyAuxiliaryPalette.DeadSidehopper, 7, 0);
                    auxiliary.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBody, 7, 15);
                    kraid.Resolve(KraidPaletteSource.Health, KraidPaletteRomData.HealthBandCount * KraidPaletteRomData.BandColors - 1);
                    kraid.Resolve(KraidPaletteSource.RoomBackdrop, KraidPaletteRomData.BandColors);
                    botwoon.HealthColor(BotwoonHealthPaletteDefinitions.PaletteCount - 1, BotwoonHealthPaletteDefinitions.ColorsPerPalette - 1);
                    botwoon.HealthColor(BotwoonHealthPaletteDefinitions.PaletteCount, 0);
                    baby.InitialColor(BabyMetroidCutsceneColorRomData.InitialColorCount - 1);
                    baby.InitialColor(BabyMetroidCutsceneColorRomData.InitialColorCount);
                    baby.FadeColor(BabyMetroidCutsceneColorRomData.FadeFrameCount, BabyMetroidCutsceneColorRomData.FadeColorCount - 1);
                    baby.FadeColor(0, 0);
                    baby.FadeColor(BabyMetroidCutsceneColorRomData.FadeFrameCount, BabyMetroidCutsceneColorRomData.FadeColorCount);
                    ridley.ApplyInitial(cgram);
                    ridley.ApplyReveal(cgram, NorfairRidleyPaletteRomData.RevealRowCount - 1);
                    ridley.ApplyReveal(cgram, NorfairRidleyPaletteRomData.RevealRowCount);
                    zebetite.Apply(cgram, ZebetiteColorFormat.FrameCount - 1, destination);
                    zebetite.Apply(cgram, ZebetiteColorFormat.FrameCount, destination);
                    shitroid.NormalColor(ShitroidColorRomData.NormalFrameCount - 1, ShitroidColorRomData.NormalColorsPerFrame - 1);
                    shitroid.NormalColor(0, ShitroidColorRomData.NormalColorsPerFrame);
                    shitroid.TargetColor(ShitroidColorTarget.DeadSidehopper, ShitroidColorRomData.TargetColorCount - 1);
                    shitroid.TargetColor((ShitroidColorTarget)int.MaxValue, 0);
                    mode7.Apply(cgram, CeresRidleyPaletteRomData.Mode7ZoomRowCount - 1);
                    mode7.Apply(cgram, CeresRidleyPaletteRomData.Mode7ZoomRowCount);
                    dachora.Resolve(DachoraPalettePhase.Shine, DachoraColorRomData.AnimatedFrameCount - 1, DachoraColorRomData.ColorsPerFrame - 1);
                    dachora.Resolve(DachoraPalettePhase.Default, 1, 0);
                    motherBrain.Apply(cgram, MotherBrainHealthPaletteFormat.StateCount - 1);
                    motherBrain.Apply(cgram, MotherBrainHealthPaletteFormat.StateCount);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-enemy-color-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
        Require(before.UnresolvedCount == 28, "twenty-eight calls must reproduce absent adapters first");
        Require(report.Classifications.Count == 14 && report.UnresolvedCount == 14 && report.MissingCount == 0 &&
            report.Consumers.Count == 28, "supported domains qualify but short arrays, zero-based baby fade and invalid constants still fail");
        foreach (string selected in new[] { "FaceBlock", "DeadSidehopper", "RoomBackdrop", "Default" })
            Require(report.Findings.Any(item => item.Message.Contains("selected " + selected, StringComparison.Ordinal)),
                "the " + selected + " selection must not borrow another resource's larger dimensions");
    }

    /// <summary>Throws when an enemy color presentation contract expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Enemy color provider confirmation failed: " + reason);
    }
}
