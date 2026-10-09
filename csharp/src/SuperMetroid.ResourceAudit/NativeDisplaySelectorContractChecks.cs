using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified non-resource selector is separate from the actual artwork query.</summary>
internal static class NativeDisplaySelectorContractChecks
{
    /// <summary>Confirms reviewed native display selectors remain within their compiled domains.</summary>
    internal static void Run()
    {
        var trees = NativeDisplaySelectorClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<display-selector-usings>"));
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            class SelectorConsumer {
                void Inspect(EnemyExtendedFrameCatalog frames, byte bank, ushort pointer) {
                    frames.GetDisplayPointer(bank, pointer);
                    frames.GetDisplayPointer(0, 0);
                    frames.GetDisplayPointer(255, 65535);
                    frames.TryGetDisplay(bank, pointer, out _);
                }
            }
            """, path: "fixture-display-selector-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
            "the unadapted classifier incorrectly requires artwork for two constant selector projections");
        Require(after.Classifications.Count == 3 && after.UnresolvedCount == 1 &&
            after.MissingCount == 0 && after.Consumers.Count == 4,
            "only the total selector projection qualifies; actual artwork availability must remain unresolved");
        const string provider = "csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs";
        var changed = trees.Select(tree => tree.FilePath == provider ? CSharpSyntaxTree.ParseText(
            tree.GetText().ToString().Replace(": nativePointer;", ": throw new InvalidDataException(\"missing selector\");", StringComparison.Ordinal),
                path: tree.FilePath) : tree);
        AuditReport revoked = Inspect(Compile(changed), true);
        Require(revoked.Classifications.Count == 0 && revoked.UnresolvedCount == 4,
            "removing total passthrough behavior must revoke the projection proof");
    }

    /// <summary>Throws when a native display selector expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Native display-selector confirmation failed: " + reason);
    }
}
