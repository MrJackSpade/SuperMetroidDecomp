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
            using SuperMetroid.Core.Areas.Escape.Artwork;
            using SuperMetroid.Core.Frontend.Ending.Artwork;
            using SuperMetroid.Core.Frontend.Intro.Artwork;
            using SuperMetroid.Core.Frontend.Map;
            using SuperMetroid.Core.Frontend.Map.Artwork;
            using SuperMetroid.Core.Frontend.Menus;
            using SuperMetroid.Core.Frontend.Pause;
            using SuperMetroid.Core.Hardware;
            using SuperMetroid.Core.Rooms;
            class TextMapConsumer {
                string UnknownFactory(AreaId area) => "World." + area;
                void Inspect(EscapeTypewriterPresentation escape, IntroNarrationPresentation intro,
                    EndingTextPresentation ending, CreditsPresentation credits, MapSpriteCatalog sprites,
                    MapArrowPresentation arrows, MapScreenPresentation screen, SnesVram vram,
                    OamBuffer oam, AreaId area, string unknown) {
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
                    sprites.Draw(MapSpriteId.WorldTitle, oam, default, 0);
                    sprites.Draw((MapSpriteId)1, oam, default, 0);
                    sprites.LoadArtworkTo(new PauseVram(vram).ObjectCharacters);
                    arrows.Get(MapScrollDirection.Left);
                    arrows.Get(MapScrollDirection.Down);
                    arrows.Get(MapScrollDirection.None);
                    arrows.Get((MapScrollDirection)99);
                    screen.LoadTo(new MenuVram(vram).Bg1Tilemap, MapScreenDefinitions.WorldForeground);
                    screen.LoadTo(new MenuVram(vram).Bg1Tilemap, MapScreenDefinitions.WorldBackground(AreaId.Maridia));
                    screen.LoadTo(new MenuVram(vram).Bg1Tilemap, MapScreenDefinitions.RoomFrame(area));
                    screen.LoadTo(new MenuVram(vram).Bg1Tilemap, MapScreenDefinitions.WorldBackground(AreaId.Ceres));
                    screen.LoadTo(new MenuVram(vram).Bg1Tilemap, unknown);
                    screen.LoadTo(new MenuVram(vram).Bg1Tilemap, UnknownFactory(area));
                    screen.LoadTo(new MenuVram(vram).Bg1Tilemap, "World.Missing");
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
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.UnresolvedCount == 28, "all twenty-eight calls must first reproduce absent provider adapters" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 15 && after.UnresolvedCount == 12 && after.MissingCount == 1 &&
            after.Consumers.Count == 28, "supported domains qualify; invalid enums, sparse IDs, rows and unknown names must remain failing " +
            "(expected 15/12/1/28; " + after.Describe() + ")");
        Require(after.Findings.Single(item => item.Code == AuditReport.Missing).Resource == "World.Missing",
            "an absent named page must be reported as a concrete identity, not waived by complete page coverage");
        Require(after.ReferenceCount == 24, "the dynamic bounded RoomFrame factory must require all six possible installed names");
    }

    /// <summary>Reports a specific text or map provider contract failure.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Text/map provider confirmation failed: " + reason);
    }
}
