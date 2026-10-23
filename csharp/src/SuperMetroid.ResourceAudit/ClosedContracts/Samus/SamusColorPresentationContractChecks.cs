using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

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
            using SuperMetroid.Core.Hardware;
            using SuperMetroid.Core.Samus.Artwork;
            using SuperMetroid.Core.Samus.Palettes;
            class ColorConsumer {
                void Inspect(SamusFullBodyCycleColorCatalog body, SamusSuitColorCatalog suit,
                    SamusChargeColorCatalog charge, SamusHurtColorCatalog hurt,
                    SamusHyperBeamColorCatalog hyper, SamusVisorColorCatalog visor,
                    CrystalFlashColorCatalog flash, PowerBombFixedColorCatalog bomb,
                    HyperBeamFxColorCatalog fx, SamusColours colours, ColourRun run) {
                    body.Apply(colours, new SamusFullBodyCyclePalette(SamusFullBodyCycleFamily.ActiveShinespark,
                        SamusFullBodyCycleColorFormat.SuitCount - 1, SamusFullBodyCycleColorFormat.ShadesPerSuit - 1));
                    body.Apply(colours, new SamusFullBodyCyclePalette(SamusFullBodyCycleFamily.ActiveShinespark,
                        SamusFullBodyCycleColorFormat.SuitCount, 0));
                    body.Resolve(new SamusFullBodyCyclePalette(SamusFullBodyCycleFamily.ActiveShinespark,
                        SamusFullBodyCycleColorFormat.SuitCount - 1, SamusFullBodyCycleColorFormat.ShadesPerSuit - 1),
                        SamusFullBodyCycleColorFormat.ColorsPerPalette - 1);
                    body.Resolve(new SamusFullBodyCyclePalette(SamusFullBodyCycleFamily.ActiveShinespark,
                        SamusFullBodyCycleColorFormat.SuitCount - 1, SamusFullBodyCycleColorFormat.ShadesPerSuit - 1),
                        SamusFullBodyCycleColorFormat.ColorsPerPalette);
                    suit.Apply(run, 4);
                    suit.Apply(run, 1);
                    charge.ApplyCharge(colours, false, SamusChargeColorFormat.SuitCount - 1, SamusChargeColorFormat.PhasesPerSuit - 1);
                    charge.ApplyCharge(colours, true, 0, SamusChargeColorFormat.PhasesPerSuit);
                    charge.ApplyHyper(colours, SamusChargeColorFormat.HyperFrameCount - 1);
                    charge.ApplyHyper(colours, SamusChargeColorFormat.HyperFrameCount);
                    hurt.Resolve(SamusHurtColorVariant.Intro, SamusHurtColorFormat.ColorsPerPalette - 1);
                    hurt.Resolve((SamusHurtColorVariant)int.MaxValue, 0);
                    hyper.Resolve(SamusHyperBeamColorFormat.FrameCount - 1, SamusHyperBeamColorFormat.ColorsPerFrame - 1);
                    hyper.Resolve(SamusHyperBeamColorFormat.FrameCount, 0);
                    visor.TryResolveByteOffset(SamusVisorColorFormat.ColorCount * sizeof(ushort) - 1, out _);
                    visor.Resolve(SamusVisorColorFormat.ColorCount - 1);
                    visor.Resolve(SamusVisorColorFormat.ColorCount);
                    flash.ApplyBody(colours, CrystalFlashColorFormat.BodyFrameCount - 1);
                    flash.ApplyBody(colours, CrystalFlashColorFormat.BodyFrameCount);
                    flash.ApplyBubble(colours, CrystalFlashColorFormat.BubbleFrameCount - 1);
                    flash.ApplyBubble(colours, CrystalFlashColorFormat.BubbleFrameCount);
                    bomb.Resolve(PowerBombFixedColorSequence.Explosion, SamusPaletteRomData.PowerBomb.ExplosionColorCount - 1);
                    bomb.Resolve(PowerBombFixedColorSequence.PreExplosion, SamusPaletteRomData.PowerBomb.PreExplosionColorCount - 1);
                    bomb.Resolve(PowerBombFixedColorSequence.PreExplosion, SamusPaletteRomData.PowerBomb.PreExplosionColorCount);
                    bomb.Resolve((PowerBombFixedColorSequence)int.MaxValue, 0);
                    fx.Apply(run, HyperBeamFxColorFormat.FrameCount - 1);
                    fx.Apply(run, HyperBeamFxColorFormat.FrameCount);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-samus-color-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("SamusColorProviderFixture", trees,
            platforms.Append(typeof(SamusSuitColorCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 27, "twenty-seven calls must reproduce the missing adapters first" + " (" + before.Describe() + ")");
        Require(report.Classifications.Count == 14 && report.UnresolvedCount == 13 && report.MissingCount == 0 &&
            report.Consumers.Count == 27, "complete supported domains and false membership queries qualify; bad selectors still fail" + " (" + report.Describe() + ")");
        Require(report.Findings.Any(item => item.Message.Contains("selected PreExplosion", StringComparison.Ordinal)),
            "a known short pre-explosion stream must not borrow explosion capacity");
        Require(report.Classifications.Any(item => item.Owner == "SamusVisorColorCatalog.TryResolveByteOffset"),
            "an unsupported queried visor offset is valid false, not a demanded resource");
    }

    /// <summary>Reports a Samus-color source contract regression with its failed condition.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus color provider confirmation failed: " + reason);
    }
}
