using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Source-only confirmation of the identified complete player color domains and rejected constants.</summary>
internal static class SamusColorPresentationContractChecks
{
    /// <summary>Confirms reviewed Samus color providers and palette selection boundaries.</summary>
    internal static void Run()
    {
        var trees = SamusColorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<samus-color-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            using SuperMetroid.Core.Hardware;
            static class FixtureBodyPalette { internal const ushort LastShine = @LAST_SHINE@; }
            class ColorConsumer {
                void Inspect(SamusFullBodyCycleColorCatalog body, SamusSuitColorCatalog suit,
                    SamusChargeColorCatalog charge, SamusHurtColorCatalog hurt,
                    SamusHyperBeamColorCatalog hyper, SamusVisorColorCatalog visor,
                    CrystalFlashColorCatalog flash, PowerBombFixedColorCatalog bomb,
                    HyperBeamFxColorCatalog fx, SnesCgram cgram, int destination) {
                    body.Apply(cgram, FixtureBodyPalette.LastShine);
                    body.Apply(cgram, 0);
                    body.Resolve(FixtureBodyPalette.LastShine, SamusFullBodyCycleColorFormat.ColorsPerPalette - 1);
                    body.Resolve(FixtureBodyPalette.LastShine, SamusFullBodyCycleColorFormat.ColorsPerPalette);
                    suit.Apply(cgram, 4);
                    suit.Apply(cgram, 1);
                    charge.ApplyCharge(cgram, false, SamusChargeColorFormat.SuitCount - 1, SamusChargeColorFormat.PhasesPerSuit - 1);
                    charge.ApplyCharge(cgram, true, 0, SamusChargeColorFormat.PhasesPerSuit);
                    charge.ApplyHyper(cgram, SamusChargeColorFormat.HyperFrameCount - 1);
                    charge.ApplyHyper(cgram, SamusChargeColorFormat.HyperFrameCount);
                    hurt.Resolve(SamusHurtColorVariant.Intro, SamusHurtColorFormat.ColorsPerPalette - 1);
                    hurt.Resolve((SamusHurtColorVariant)int.MaxValue, 0);
                    hyper.Resolve(SamusHyperBeamColorFormat.FrameCount - 1, SamusHyperBeamColorFormat.ColorsPerFrame - 1);
                    hyper.Resolve(SamusHyperBeamColorFormat.FrameCount, 0);
                    visor.TryResolveByteOffset(SamusVisorColorFormat.ColorCount * sizeof(ushort) - 1, out _);
                    visor.Resolve(SamusVisorColorFormat.ColorCount - 1);
                    visor.Resolve(SamusVisorColorFormat.ColorCount);
                    flash.ApplyBody(cgram, CrystalFlashColorFormat.BodyFrameCount - 1);
                    flash.ApplyBody(cgram, CrystalFlashColorFormat.BodyFrameCount);
                    flash.ApplyBubble(cgram, CrystalFlashColorFormat.BubbleFrameCount - 1);
                    flash.ApplyBubble(cgram, CrystalFlashColorFormat.BubbleFrameCount);
                    bomb.Resolve(PowerBombFixedColorSequence.Explosion, SamusPaletteRomData.PowerBomb.ExplosionColorCount - 1);
                    bomb.Resolve(PowerBombFixedColorSequence.PreExplosion, SamusPaletteRomData.PowerBomb.PreExplosionColorCount - 1);
                    bomb.Resolve(PowerBombFixedColorSequence.PreExplosion, SamusPaletteRomData.PowerBomb.PreExplosionColorCount);
                    bomb.Resolve((PowerBombFixedColorSequence)int.MaxValue, 0);
                    fx.Apply(cgram, HyperBeamFxColorFormat.FrameCount - 1, destination);
                    fx.Apply(cgram, HyperBeamFxColorFormat.FrameCount, destination);
                }
            }
            """;
        ushort lastShine = SamusFullBodyCycleColorFormat.Pointer(SamusFullBodyCycleFamily.ActiveShinespark,
            SamusFullBodyCycleColorFormat.SuitCount - 1, SamusFullBodyCycleColorFormat.ShadesPerSuit - 1);
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer.Replace("@LAST_SHINE@", $"0x{lastShine:X4}",
            StringComparison.Ordinal), path: "fixture-samus-color-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
        Require(before.UnresolvedCount == 27, "twenty-seven calls must reproduce the missing adapters first");
        Require(report.Classifications.Count == 14 && report.UnresolvedCount == 13 && report.MissingCount == 0 &&
            report.Consumers.Count == 27, "complete supported domains and false membership queries qualify; bad selectors still fail");
        Require(report.Findings.Any(item => item.Message.Contains("selected PreExplosion", StringComparison.Ordinal)),
            "a known short pre-explosion stream must not borrow explosion capacity");
        Require(report.Classifications.Any(item => item.Owner == "SamusVisorColorCatalog.TryResolveByteOffset"),
            "an unsupported queried visor offset is valid false, not a demanded resource");
    }

    /// <summary>Throws when a Samus color presentation expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Samus color provider confirmation failed: " + reason);
    }
}
