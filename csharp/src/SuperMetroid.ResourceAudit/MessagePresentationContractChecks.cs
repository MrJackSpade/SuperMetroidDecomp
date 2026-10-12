using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified message-family adapter and its save-only selection ownership.</summary>
internal static class MessagePresentationContractChecks
{
    internal static void Run()
    {
        var trees = MessageClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<message-fixture-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            class MessageConsumer {
                void Inspect(GameplayMessageTitlePresentation titles, GameplayMessagePanelPresentation panels,
                    GameplayMessageNoticePresentation notices, System.Span<ushort> cells) {
                    titles.Build(GameplayMessageId.EnergyTank);
                    titles.Build(GameplayMessageId.MissileTank);
                    panels.Build(GameplayMessageId.MissileTank);
                    panels.Build(GameplayMessageId.EnergyTank);
                    notices.Build(GameplayMessageId.EnergyRechargeCompleted);
                    notices.ApplySelection(GameplayMessageId.SaveConfirmation, cells, true);
                    notices.ApplySelection(GameplayMessageId.EnergyRechargeCompleted, cells, true);
                    notices.Contains((GameplayMessageId)99);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-message-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        var compilation = CSharpCompilation.Create("MessageProviderFixture", trees,
            platforms.Append(typeof(GameplayMessageTitlePresentation).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 8, "all eight calls must reproduce the missing family adapter first");
        Require(report.Classifications.Count == 5 && report.UnresolvedCount == 3 && report.MissingCount == 0 &&
            report.Consumers.Count == 8, "five owned operations qualify; three cross-family/save-only requests must fail");
        Require(report.Consumers.Single(item => item.Owner.EndsWith(".Contains", StringComparison.Ordinal))
            .Resolution == "closed-provider", "an unowned membership query is valid and does not demand its artwork");
        Require(report.Findings.All(item => item.Message.Contains("not owned", StringComparison.Ordinal)),
            "invalid constants must fail for exact message ownership, not generic missing-adapter reasons");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Message provider confirmation failed: " + reason);
    }
}
