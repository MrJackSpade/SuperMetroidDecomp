using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified room-layout subset admission gap; never loads or enters a room.</summary>
internal static class RoomLayoutPresentationContractChecks
{
    /// <summary>Checks complete source ownership, input isolation, and audit revocation for a partial layout factory.</summary>
    internal static void Run()
    {
        int first = RoomVisualLayoutSourceDefinitions.All.First();
        RoomVisualLayout Layout(int source) => new(source, new(1, 1), [0], [0]);
        var owned = RoomVisualLayoutSourceDefinitions.All.ToDictionary(source => source, Layout);
        var incomplete = new Dictionary<int, RoomVisualLayout>(owned);
        incomplete.Remove(first);
        bool rejected = false;
        try { _ = new RoomVisualLayoutCatalog(incomplete); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "production construction must reject a missing required source");
        var catalog = new RoomVisualLayoutCatalog(owned);
        owned.Remove(first);
        Require(catalog.Get(first).SourceAddress == first, "catalog keys must be independent of the input dictionary");
        var mismatched = RoomVisualLayoutSourceDefinitions.All.ToDictionary(source => source, Layout);
        mismatched[first] = Layout(0);
        rejected = false;
        try { _ = new RoomVisualLayoutCatalog(mismatched); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "installed keys must match their layout identities");

        var trees = RoomLayoutClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Rooms;
            class LayoutConsumer {
                void Inspect(RoomVisualLayoutCatalog layouts, int source) {
                    layouts.Get({{first}});
                    layouts.Get(source);
                    layouts.Get(0);
                }
            }
            """, path: "fixture-room-layout-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("RoomLayoutProviderFixture", sources,
            platforms.Append(typeof(RoomVisualLayoutCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 3 && after.Classifications.Count == 2 && after.UnresolvedCount == 1 &&
            after.MissingCount == 0 && after.Consumers.Count == 3, "required-source closure must reject known unowned keys" + " (" + after.Describe() + ")");
    }

    /// <summary>Reports a room-layout closure regression with its failed contract detail.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room layout confirmation failed: " + reason);
    }
}
