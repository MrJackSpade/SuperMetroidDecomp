using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Constructed-source confirmation of the identified pause provider contracts, not menu behavior or sound.</summary>
internal static class PausePresentationContractChecks
{
    internal static void Run()
    {
        var trees = PauseClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<pause-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            using SuperMetroid.Core.Hardware;
            class PauseConsumer {
                void Inspect(PauseEquipmentBasePresentation baseImage, PauseBackdropPresentation backdrop,
                    PauseWireframePresentation wireframe, PauseEquipmentLabelPresentation labels,
                    PauseSelectorPresentation selector, PauseReserveTankPresentation tanks,
                    Span<byte> page, SnesVram vram, OamBuffer oam) {
                    baseImage.CreateTilemap();
                    baseImage.RebindBaseInto(page, labels);
                    baseImage.RebindBeforeInventoryRefreshInto(page);
                    backdrop.LoadTo(vram, 0, AreaId.Ceres);
                    backdrop.LoadTo(vram, 0, (AreaId)99);
                    backdrop.CreateButtonTilemap();
                    wireframe.ApplyTo(page, PauseWireframeKind.VariaSuitHiJump);
                    wireframe.ApplyTo(page, (PauseWireframeKind)99);
                    labels.ApplyInventory(page, 0, 0, 0, 0, false);
                    labels.ApplyLabel(page, 1, 4, PauseEquipmentLabelDefinitions.EquipmentWords, false);
                    labels.ApplyLabel(page, 1, 0, PauseEquipmentLabelDefinitions.EquipmentWords, false);
                    labels.ApplyLabel(page, 3, 2, PauseEquipmentLabelDefinitions.EquipmentWords, false);
                    labels.ApplyLabel(page, 3, 3, PauseEquipmentLabelDefinitions.EquipmentWords, false);
                    labels.ApplyLabel(page, 0, 0, 0, false);
                    labels.OwnsLiveCell(0);
                    selector.Anchor(3, 2);
                    selector.Anchor(3, 3);
                    selector.NormalizePhase(int.MaxValue);
                    selector.NormalizePhase(-1);
                    selector.Duration(1000);
                    selector.Draw(oam, 0, 1, int.MaxValue);
                    selector.Draw(oam, 0, 2, 0);
                    tanks.Draw(oam, 0x1b, PauseReserveTankDefinitions.AnchorCount - 1);
                    tanks.Draw(oam, 0, 0);
                    tanks.Anchor(PauseReserveTankDefinitions.AnchorCount);
                    tanks.Anchor(0);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-pause-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
        Require(before.UnresolvedCount == 26, "twenty-six calls must first reproduce the absent adapters");
        Require(after.Classifications.Count == 16 && after.UnresolvedCount == 10 && after.MissingCount == 0 &&
            after.Consumers.Count == 26, "complete resources qualify, while wrong area/kind/tuple/overrun/anchor/phase/identity constants still fail");
        Require(after.Findings.Count(item => item.Owner == "PauseEquipmentLabelPresentation.ApplyLabel") == 3 &&
            after.Findings.Count(item => item.Owner == "PauseSelectorPresentation.Anchor" || item.Owner == "PauseSelectorPresentation.Draw") == 2,
            "Boots and Reserve must not borrow larger categories; only Plasma may extend a beam label to nine words");

        SyntaxTree externalUse = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            class ExternalPauseKeys {
                void Change() { PauseEquipmentLabelDefinitions.Keys[1][0] = "Absent.Key"; }
            }
            """, path: "fixture-external-pause-keys.cs");
        AuditReport revoked = Inspect(Compile(trees.Append(externalUse)), true);
        Require(revoked.Classifications.Count == 12 && revoked.UnresolvedCount == 14 &&
            revoked.Findings.Count(item => item.Owner.StartsWith("PauseEquipmentLabelPresentation.", StringComparison.Ordinal) &&
                item.Message.Contains("stale", StringComparison.Ordinal)) == 7,
            "external access to mutable label Keys must revoke every label proof, not certify post-publication identity mutations");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Pause provider confirmation failed: " + reason);
    }
}
