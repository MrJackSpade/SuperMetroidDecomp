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
                    MotherBrainRoomColorPresentation colors, SnesCgram cgram, Span<ushort> tiles,
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
        Require(before.UnresolvedCount == 9, "the fixture must reproduce the identified missing-adapter boundaries first");
        var report = new AuditReport();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
        Require(report.Classifications.Count == 4 && report.MissingCount == 1 && report.UnresolvedCount == 4,
            "only reviewed valid-domain operations and installed constant names qualify; invalid/new/dynamic selections must fail");
        Require(report.Findings.Single(item => item.Code == AuditReport.Missing).Resource == "Missing.Anchor",
            "a compiler-resolved uninstalled name must remain a concrete missing identity");
        Require(report.Consumers.Count == 9, "classifications must retain every inventoried call");

        ReviewedSource guard = ClosedPresentationContractDefinitions.All[0].Sources[0];
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
