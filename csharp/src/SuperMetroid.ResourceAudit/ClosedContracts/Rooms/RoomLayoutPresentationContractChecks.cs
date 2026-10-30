using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.Core.World.RoomStates;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Rooms;

/// <summary>Confirms the identified room-layout subset admission gap; never loads or enters a room.</summary>
internal static class RoomLayoutPresentationContractChecks
{
    /// <summary>Checks complete source ownership, input isolation, and audit revocation for a partial layout factory.</summary>
    internal static void Run()
    {
        RoomLevelId first = RoomVisualLayoutSourceDefinitions.All.First();
        RoomLevelArtwork Layout(RoomLevelId level) => new(level, new(1, 1), [default(RoomBlockAppearance)], [default(RoomBlockAppearance)]);
        var owned = RoomVisualLayoutSourceDefinitions.All.ToDictionary(level => level, Layout);
        var incomplete = new Dictionary<RoomLevelId, RoomLevelArtwork>(owned);
        incomplete.Remove(first);
        bool rejected = false;
        try { _ = new RoomLevelArtworks(incomplete, RoomVisualLayoutSourceDefinitions.All); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "production construction must reject a missing required level");
        var catalog = new RoomLevelArtworks(owned, RoomVisualLayoutSourceDefinitions.All);
        owned.Remove(first);
        Require(catalog.Get(first).Level == first, "catalog keys must be independent of the input dictionary");
        var mismatched = RoomVisualLayoutSourceDefinitions.All.ToDictionary(level => level, Layout);
        mismatched[first] = Layout(RoomVisualLayoutSourceDefinitions.All.Last());
        rejected = false;
        try { _ = new RoomLevelArtworks(mismatched, RoomVisualLayoutSourceDefinitions.All); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "installed keys must match their layout identities");

        var trees = RoomLayoutClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText($$"""
            using SuperMetroid.Core.Rooms;
            using SuperMetroid.Core.Rooms.LevelData;
            class LayoutConsumer {
                void Inspect(RoomLevelArtworks layouts, RoomLevelId level) {
                    layouts.Get(new RoomLevelId("{{first.Name}}"));
                    layouts.Get(level);
                    layouts.Get(new RoomLevelId("UnknownLevel"));
                }
            }
            """, path: "fixture-room-layout-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("RoomLayoutProviderFixture", sources,
            platforms.Append(typeof(RoomLevelArtworks).Assembly.Location).Distinct()
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
            after.MissingCount == 0 && after.Consumers.Count == 3, "required-level closure must reject known unowned keys" + " (" + after.Describe() + ")");
    }

    /// <summary>Reports a room-layout closure regression with its failed contract detail.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Room layout confirmation failed: " + reason);
    }
}
