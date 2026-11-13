using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Source-only confirmation of the identified complete player color domains and rejected constants.</summary>
internal static class SamusColorPresentationContractChecks
{
    /// <summary>Checks Samus color domain closure while leaving invalid or short selections unresolved.</summary>
    internal static void Run()
    {
        var trees = SamusColorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.AssetExtraction.Samus.Artwork;
            using SuperMetroid.Core.Samus.Artwork;
            using SuperMetroid.Core.Samus.Palettes;
            class ColorConsumer {
                void Inspect(SamusFullBodyCycleColours body, InstalledSamusSuitColors suit,
                    SamusChargeColours charge, InstalledSamusHurtColors hurt,
                    SamusHyperBeamColours hyper, InstalledSamusVisorColors visor,
                    InstalledCrystalFlashColors flash, InstalledPowerBombGlowColors bomb,
                    HyperBeamFxColours fx) {
                    body.Resolve(new SamusFullBodyCyclePalette(SamusFullBodyCycleFamily.ActiveShinespark,
                        SamusSuit.Gravity, 3), SamusColourSets.ColoursPerSet - 1);
                    body.Resolve(new SamusFullBodyCyclePalette(SamusFullBodyCycleFamily.ActiveShinespark,
                        SamusSuit.Gravity, 3), SamusColourSets.ColoursPerSet);
                    hurt.Resolve(SamusHurtColorVariant.Intro, SamusHurtColorFormat.ColorsPerPalette - 1);
                    hurt.Resolve((SamusHurtColorVariant)int.MaxValue, 0);
                    hyper.Resolve(SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount - 1,
                        SamusColourSets.ColoursPerSet - 1);
                    hyper.Resolve(SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount, 0);
                    visor.Resolve(SamusVisorColorFormat.ColorCount - 1);
                    visor.Resolve(SamusVisorColorFormat.ColorCount);
                    bomb.Resolve(PowerBombGlowSequence.Explosion, SamusPaletteDefinitions.PowerBomb.ExplosionColorCount - 1);
                    bomb.Resolve(PowerBombGlowSequence.PreExplosion, SamusPaletteDefinitions.PowerBomb.PreExplosionColorCount - 1);
                    bomb.Resolve(PowerBombGlowSequence.PreExplosion, SamusPaletteDefinitions.PowerBomb.PreExplosionColorCount);
                    bomb.Resolve((PowerBombGlowSequence)int.MaxValue, 0);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-samus-color-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("SamusColorProviderFixture", trees,
            platforms.Append(typeof(SamusChargePalette).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        FixtureConsumerBinding.Require(compilation, calls, "Samus color provider confirmation");
        var exports = new ResourceIndex();
        var adapter = new ClosedPresentationAudit(compilation, exports);
        var before = new AuditReport();
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, before);
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        }
        Require(before.UnresolvedCount == 12, "twelve calls must reproduce the missing adapters first" + " (" + before.Describe() + ")");
        Require(report.Classifications.Count == 6 && report.UnresolvedCount == 6 && report.MissingCount == 0 &&
            report.Consumers.Count == 12, "complete supported domains qualify; bad selectors still fail" + " (" + report.Describe() + ")");
        Require(report.Findings.Any(item => item.Message.Contains("selected PreExplosion", StringComparison.Ordinal)),
            "a known short pre-explosion stream must not borrow explosion capacity");
    }

    /// <summary>Reports a Samus-color source contract regression with its failed condition.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus color provider confirmation failed: " + reason);
    }
}
