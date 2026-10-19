using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Constructed-source confirmation of the identified pause provider contracts, not menu behavior or sound.</summary>
internal static class PausePresentationContractChecks
{
    /// <summary>Compiles 26 pause consumer calls to confirm the closed adapters classify only supported resources, then changes label source identity to confirm those proofs are revoked.</summary>
    internal static void Run()
    {
        var trees = PauseClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        const string consumer = """
            using SuperMetroid.Core.Frontend.Pause;
            using SuperMetroid.Core.Frontend.Pause.Artwork;
            using SuperMetroid.Core.Hardware;
            using SuperMetroid.Core.Rooms;
            class PauseConsumer {
                void Inspect(PauseEquipmentBasePresentation baseImage, PauseBackdropPresentation backdrop,
                    PauseWireframePresentation wireframe, PauseEquipmentLabelPresentation labels,
                    PauseSelectorPresentation selector, PauseReserveTankPresentation tanks,
                    Span<byte> page, SnesVram vram, OamBuffer oam) {
                    baseImage.CreateTilemap();
                    baseImage.RebindBaseInto(page, labels);
                    baseImage.RebindBeforeInventoryRefreshInto(page);
                    backdrop.LoadTo(new PauseVram(vram).Bg2Tilemap, AreaId.Ceres);
                    backdrop.LoadTo(new PauseVram(vram).Bg2Tilemap, (AreaId)99);
                    backdrop.CreateButtonTilemap();
                    wireframe.ApplyTo(page, PauseWireframeKind.VariaSuitHiJump);
                    wireframe.ApplyTo(page, (PauseWireframeKind)99);
                    labels.ApplyInventory(page, 0, 0, 0, 0, false);
                    labels.ApplyLabel(page, (PauseEquipmentCategory)1, 4, PauseEquipmentLabelDefinitions.EquipmentWords, false);
                    labels.ApplyLabel(page, (PauseEquipmentCategory)1, 0, PauseEquipmentLabelDefinitions.EquipmentWords, false);
                    labels.ApplyLabel(page, (PauseEquipmentCategory)3, 2, PauseEquipmentLabelDefinitions.EquipmentWords, false);
                    labels.ApplyLabel(page, (PauseEquipmentCategory)3, 3, PauseEquipmentLabelDefinitions.EquipmentWords, false);
                    labels.ApplyLabel(page, 0, 0, 0, false);
                    labels.OwnsLiveCell(0);
                    selector.Anchor((PauseEquipmentCategory)3, 2);
                    selector.Anchor((PauseEquipmentCategory)3, 3);
                    selector.NormalizePhase(int.MaxValue);
                    selector.NormalizePhase(-1);
                    selector.Duration(1000);
                    selector.Draw(oam, 0, 1, int.MaxValue);
                    selector.Draw(oam, 0, 2, 0);
                    tanks.Draw(oam, (PauseReserveTankVisual)0x1b, PauseReserveTankDefinitions.AnchorCount - 1);
                    tanks.Draw(oam, 0, 0);
                    tanks.Anchor(PauseReserveTankDefinitions.AnchorCount);
                    tanks.Anchor(0);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-pause-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("PauseProviderFixture", sources,
            platforms.Append(typeof(PauseSelectorPresentation).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 26, "twenty-six calls must first reproduce the absent adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 16 && after.UnresolvedCount == 10 && after.MissingCount == 0 &&
            after.Consumers.Count == 26, "complete resources qualify, while wrong area/kind/tuple/overrun/anchor/phase/identity constants still fail (expected 16/10/0/26; " + after.Describe() + ")");
        Require(after.Findings.Count(item => item.Owner == "PauseEquipmentLabelPresentation.ApplyLabel") == 3 &&
            after.Findings.Count(item => item.Owner is "PauseSelectorPresentation.Anchor" or "PauseSelectorPresentation.Draw") == 2,
            "Boots and Reserve must not borrow larger categories; only Plasma may extend a beam label to nine words");
    }

    /// <summary>Fails the audit with the violated pause-provider contract predicate.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Pause provider confirmation failed: " + reason);
    }
}
