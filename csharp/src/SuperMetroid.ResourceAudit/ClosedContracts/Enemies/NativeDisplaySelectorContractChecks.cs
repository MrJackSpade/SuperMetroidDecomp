using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Confirms the identified non-resource selector is separate from the actual artwork query.</summary>
internal static class NativeDisplaySelectorContractChecks
{
    /// <summary>Compiles fixture calls to distinguish total display-pointer projection from actual installed artwork lookup, then revokes the proof when passthrough changes.</summary>
    internal static void Run()
    {
        var trees = NativeDisplaySelectorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Enemies.Artwork;
            class SelectorConsumer {
                void Inspect(EnemyExtendedFrameCatalog frames, EnemyFrameId frame) {
                    frames.GetDisplayFrame(frame);
                    frames.GetDisplayFrame(EnemyFrameId.Empty);
                    frames.GetDisplayFrame(EnemyFrameId.EmptyExtended);
                    frames.TryGetDisplay(frame, out _);
                }
            }
            """, path: "fixture-display-selector-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("NativeDisplaySelectorFixture", sources,
            platforms.Append(typeof(EnemyExtendedFrameCatalog).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AuditReport Inspect(CSharpCompilation compilation, bool adapted)
        {
            var exports = new ResourceIndex();
            var adapter = adapted ? new ClosedPresentationAudit(compilation, exports) : null;
            var report = new AuditReport();
            foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
            return report;
        }
        CSharpCompilation compilation = Compile(trees);
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.MissingCount == 2 && before.UnresolvedCount == 2,
            "the unadapted classifier incorrectly requires artwork for two constant selector projections" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 3 && after.UnresolvedCount == 1 &&
            after.MissingCount == 0 && after.Consumers.Count == 4,
            "only the total selector projection qualifies; actual artwork availability must remain unresolved" + " (" + after.Describe() + ")");
    }

    /// <summary>Reports a failed selector-contract expectation with its audit-specific reason.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Native display-selector confirmation failed: " + reason);
    }
}
