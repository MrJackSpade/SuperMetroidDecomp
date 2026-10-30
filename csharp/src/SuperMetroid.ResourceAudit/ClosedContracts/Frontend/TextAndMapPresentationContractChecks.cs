using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Confirms the identified text/map adapters with constructed source; no content files, cinematic or menu runs.</summary>
internal static class TextAndMapPresentationContractChecks
{
    /// <summary>Compiles text and map consumers to check bounded providers, unknown names, and stale source proofs.</summary>
    internal static void Run()
    {
        var trees = TextAndMapClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Escape.Artwork;
            using SuperMetroid.Core.Frontend.Ending.Artwork;
            using SuperMetroid.Core.Frontend.Intro.Artwork;
            using SuperMetroid.Core.Frontend.Map;
            using SuperMetroid.Core.Frontend.Map.Artwork;
            using SuperMetroid.Core.Frontend.Menus;
            using SuperMetroid.Core.Frontend.Pause;
            using SuperMetroid.Core.Progression;
            using SuperMetroid.Core.Rooms;
            class TextMapConsumer {
                void Inspect(EscapeTypewriterPresentation escape, IntroNarrationPresentation intro,
                    EndingTextPresentation ending, CreditsPresentation credits, MapSpriteCompositions sprites, SpriteSheet mapSheet,
                    MapArrowPresentation arrows,
                    ScreenSprites objects, System.Span<BackgroundTile> tilemap, AreaId area) {
                    escape.Get(EscapeTypewriterProgramId.Ceres);
                    escape.Get(EscapeTypewriterProgramId.Zebes);
                    escape.Get(EscapeTypewriterProgramId.None);
                    escape.Get((EscapeTypewriterProgramId)99);
                    intro.GetLines(IntroNarrationPageId.Page1);
                    intro.Compile(IntroNarrationPageId.Page6);
                    intro.Compile((IntroNarrationPageId)0);
                    ending.Compile(EndingTextSequence.FinalMessage);
                    ending.Compile((EndingTextSequence)99);
                    ending.BuildResultPanel();
                    ending.BuildCopyrightPanel();
                    credits.GetRow(CreditsPresentationDefinitions.ExpectedCompiledRows - 1);
                    credits.GetRow(CreditsPresentationDefinitions.ExpectedCompiledRows);
                    credits.GetRow(-1);
                    MapSpriteSheet.Draw(mapSheet, MapSpriteId.WorldTitle, objects, default, SuperMetroid.Core.Frontend.ScreenColourGroup.MapLabels);
                    MapSpriteSheet.Draw(mapSheet, (MapSpriteId)0, objects, default, SuperMetroid.Core.Frontend.ScreenColourGroup.MapLabels);
                    arrows.Get(MapScrollDirection.Left);
                    arrows.Get(MapScrollDirection.Down);
                    arrows.Get(MapScrollDirection.None);
                    arrows.Get((MapScrollDirection)99);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-text-map-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("TextMapProviderFixture", sources,
            platforms.Append(typeof(CreditsPresentation).Assembly.Location).Distinct()
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
        FixtureConsumerBinding.Require(compilation, calls, "TextAndMapPresentationContractChecks");
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.UnresolvedCount == 20, "all twenty calls must first reproduce absent provider adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 12 && after.UnresolvedCount == 8 && after.MissingCount == 0 &&
            after.Consumers.Count == 20, "supported domains qualify; invalid enums, sparse IDs and rows must remain failing " +
            "(expected 12/8/0/20; " + after.Describe() + ")");
        Require(after.ReferenceCount == 12, "each classified call requires its reviewed provider contract" + " (" + after.Describe() + ")");
    }

    /// <summary>Reports a specific text or map provider contract failure.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Text/map provider confirmation failed: " + reason);
    }
}
