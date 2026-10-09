using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms the identified text/map adapters with constructed source; no content files, cinematic or menu runs.</summary>
internal static class TextAndMapPresentationContractChecks
{
    /// <summary>Confirms reviewed text and map presentation providers and selector domains.</summary>
    internal static void Run()
    {
        var trees = TextAndMapClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<text-map-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            using SuperMetroid.Core.Frontend;
            using SuperMetroid.Core.Hardware;
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
                    sprites.Draw(MapSpriteDefinitions.WorldTitle, oam, 0, 0, 0);
                    sprites.Draw(1, oam, 0, 0, 0);
                    sprites.LoadArtworkTo(vram, MapSpriteFormat.PauseDestination);
                    arrows.Get(MapScrollDirection.Left);
                    arrows.Get(MapScrollDirection.Down);
                    arrows.Get(MapScrollDirection.None);
                    arrows.Get((MapScrollDirection)99);
                    screen.LoadTo(vram, 0, MapScreenDefinitions.WorldForeground);
                    screen.LoadTo(vram, 0, MapScreenDefinitions.WorldBackground(AreaId.Maridia));
                    screen.LoadTo(vram, 0, MapScreenDefinitions.RoomFrame(area));
                    screen.LoadTo(vram, 0, MapScreenDefinitions.WorldBackground(AreaId.Ceres));
                    screen.LoadTo(vram, 0, unknown);
                    screen.LoadTo(vram, 0, UnknownFactory(area));
                    screen.LoadTo(vram, 0, "World.Missing");
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-text-map-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
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
        Require(before.UnresolvedCount == 28, "all twenty-eight calls must first reproduce absent provider adapters");
        Require(after.Classifications.Count == 15 && after.UnresolvedCount == 12 && after.MissingCount == 1 &&
            after.Consumers.Count == 28, "supported domains qualify; invalid enums, sparse IDs, rows and unknown names must remain failing");
        Require(after.Findings.Single(item => item.Code == AuditReport.Missing).Resource == "World.Missing",
            "an absent named page must be reported as a concrete identity, not waived by complete page coverage");
        Require(after.ReferenceCount == 24, "the dynamic bounded RoomFrame factory must require all six possible installed names");

        // A source/type lifecycle check, not an execution of the verification factory.
        SyntaxTree verificationUse = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            class AlternateCreditsOwner {
                object Create() => CreditsPresentation.FromCompiledRowsForVerification(new ushort[32]);
            }
            """, path: "fixture-alternate-credits-owner.cs");
        AuditReport alternate = Inspect(Compile(trees.Append(verificationUse)), true);
        Require(alternate.Classifications.Count == 14 && alternate.UnresolvedCount == 13 &&
            alternate.Findings.Where(item => item.Owner == "CreditsPresentation.GetRow")
                .All(item => item.Message.Contains("stale", StringComparison.Ordinal)),
            "a Core reference to the variable-row verification factory must revoke the fixed production row proof");

        const string mapDefinitionsPath = "csharp/src/SuperMetroid.Core/Assets/MapScreenDefinitions.cs";
        SyntaxTree original = trees.Single(tree => tree.FilePath == mapDefinitionsPath);
        SyntaxTree changed = CSharpSyntaxTree.ParseText(original.GetText() + "\n// changed factory contract\n", path: mapDefinitionsPath);
        AuditReport stale = Inspect(Compile(trees.Select(tree => tree == original ? changed : tree)), true);
        Require(stale.Findings.Count(item => item.Owner == "MapScreenPresentation.LoadTo" &&
            item.Message.Contains("stale", StringComparison.Ordinal)) == 7,
            "all map-name calls must fail when their reviewed factory/loader definition changes");
    }

    /// <summary>Throws when a text or map presentation expectation is not satisfied.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Text/map provider confirmation failed: " + reason);
    }
}
