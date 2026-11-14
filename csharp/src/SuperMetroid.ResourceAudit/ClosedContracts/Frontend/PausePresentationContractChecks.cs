using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Constructed-source confirmation of the identified pause provider contracts, not menu behavior or sound.</summary>
internal static class PausePresentationContractChecks
{
    /// <summary>Compiles 18 pause consumer calls to confirm the closed adapters classify only supported resources, then changes label source identity to confirm those proofs are revoked.</summary>
    internal static void Run()
    {
        var trees = PauseClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        const string consumer = """
            using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
            using SuperMetroid.Core.Frontend.Pause;
            using SuperMetroid.Core.Frontend.Pause.Artwork;
            using SuperMetroid.Core.Progression;
            using SuperMetroid.Core.Rooms;
            class PauseConsumer {
                void Inspect(PauseEquipmentBasePresentation baseImage, PauseBackdropPresentation backdrop,
                    PauseWireframePresentation wireframe, PauseEquipmentLabelPresentation labels,
                    PauseSelectorPresentation selector, PauseReserveTankPresentation tanks,
                    Span<SuperMetroid.AssetExtraction.Artwork.BackgroundTile> page, Span<SuperMetroid.AssetExtraction.Artwork.BackgroundTile> tilemap, SuperMetroid.Core.Assets.ScreenSprites oam,
                    SuperMetroid.Core.Assets.SpriteSheet sheet) {
                    baseImage.CreateTilemap();
                    backdrop.LoadTo(tilemap, AreaId.Ceres);
                    backdrop.LoadTo(tilemap, (AreaId)99);
                    backdrop.CreateButtonTilemap();
                    wireframe.ApplyTo(page, PauseWireframeKind.VariaSuitHiJump);
                    wireframe.ApplyTo(page, (PauseWireframeKind)99);
                    labels.ApplyInventory(page, 0, 0, 0, 0, false);
                    selector.Anchor(PauseEquipmentControls.SpaceJump);
                    selector.Anchor(PauseEquipmentControls.SpeedBooster);
                    selector.NormalizePhase(int.MaxValue);
                    selector.NormalizePhase(-1);
                    selector.Duration(1000);
                    selector.Draw(oam, sheet, PauseEquipmentControls.ReserveTransfer, int.MaxValue);
                    selector.Draw(oam, sheet, PauseEquipmentControls.ReserveMode, 0);
                    tanks.Draw(oam, sheet, PauseReserveTankVisual.Full, PauseReserveTankDefinitions.AnchorCount - 1);
                    tanks.Draw(oam, sheet, (PauseReserveTankVisual)99, 0);
                    tanks.Anchor(PauseReserveTankDefinitions.AnchorCount);
                    tanks.Anchor(0);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-pause-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("PauseProviderFixture", sources,
            platforms.Append(typeof(PauseSelectorPresentation).Assembly.Location).Append(typeof(PauseEquipmentBasePresentation).Assembly.Location).Distinct()
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
        FixtureConsumerBinding.Require(compilation, calls, "PausePresentationContractChecks");
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.UnresolvedCount == 18, "eighteen calls must first reproduce the absent adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 13 && after.UnresolvedCount == 5 && after.MissingCount == 0 &&
            after.Consumers.Count == 18, "complete resources qualify, while wrong area/kind/phase/identity constants still fail (expected 13/5/0/18; " + after.Describe() + ")");
        // A selector is placed for a named control (#1772), so no selector can borrow another column's anchor.
        Require(after.Findings.Count(item => item.Owner is "PauseSelectorPresentation.Anchor") == 0,
            "every named control's selector anchor qualifies");
    }

    /// <summary>Fails the audit with the violated pause-provider contract predicate.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Pause provider confirmation failed: " + reason);
    }
}
