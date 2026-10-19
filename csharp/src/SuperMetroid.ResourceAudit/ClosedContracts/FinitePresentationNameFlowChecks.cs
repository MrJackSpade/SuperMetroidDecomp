using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Confirms the identified closed menu-name flows and their explicit unknown/alias boundaries.</summary>
internal static class FinitePresentationNameFlowChecks
{
    /// <summary>Compiles finite, unknown, escaped, and cyclic menu-name flows
    /// to check which selections the audit may resolve from source.</summary>
    internal static void Run()
    {
        const string fixture = """
            using System;
            using SuperMetroid.Core.Frontend.FileSelect.Artwork;
            using SuperMetroid.Core.Frontend.Options.Artwork;
            class Names {
                FileSelectPresentation art = null!;
                private string finiteField = FileSelectPresentationDefinitions.MainEmptyPage;
                private string unknownField = FileSelectPresentationDefinitions.MainEmptyPage;
                private string refField = FileSelectPresentationDefinitions.MainEmptyPage;
                private string tupleField = FileSelectPresentationDefinitions.MainEmptyPage;
                private string cycleA = "Main.Empty", cycleB = "Main.WithData";
                public string publicField = "Main.Empty";
                public void ValidLocal(GameOptionsPresentation p, bool v) {
                    string name = v ? GameOptionsPresentationDefinitions.PrimaryPage : GameOptionsPresentationDefinitions.ControllerEnglishPage;
                    name = GameOptionsPresentationDefinitions.ControllerJapanesePage;
                    p.CreatePage(name);
                }
                public void UnknownLocal(GameOptionsPresentation p, string unknown) {
                    string name = GameOptionsPresentationDefinitions.PrimaryPage; name = unknown; p.CreatePage(name);
                }
                public void RefLocal(GameOptionsPresentation p) {
                    string name = GameOptionsPresentationDefinitions.PrimaryPage; Mutate(out name); p.CreatePage(name);
                }
                public void TupleLocal(GameOptionsPresentation p, string unknown) {
                    string name = GameOptionsPresentationDefinitions.PrimaryPage; int number;
                    (name, number) = (unknown, 0); p.CreatePage(name);
                }
                private static string Menu(int v) => v switch {
                    0 => GameOptionsPresentationDefinitions.PrimaryMenu,
                    1 => GameOptionsPresentationDefinitions.ControllerMenu,
                    _ => throw new InvalidOperationException()
                };
                public void Factory(GameOptionsPresentation p, bool v) { p.CursorPosition(Menu(v ? 0 : 1), 0); }
                private static string MissingMenu(bool v) => v ? GameOptionsPresentationDefinitions.PrimaryMenu : "Missing.Menu";
                public void MissingFactory(GameOptionsPresentation p, bool v) { p.CursorPosition(MissingMenu(v), 0); }
                private static string UnknownMenu(string text) => "Menu." + text;
                public void UnknownFactory(GameOptionsPresentation p, string text) { p.CursorPosition(UnknownMenu(text), 0); }
                private void Receive(string name) { art.CopyPage(name, new ushort[1024]); }
                public void Calls() { Receive("Main.Empty"); Receive("Main.WithData"); }
                private void ReceiveUnknown(string name) { art.CopyPage(name, new ushort[1024]); }
                public void UnknownCall(string text) { ReceiveUnknown("Main.Empty"); ReceiveUnknown(text); }
                private void Escaped(string name) { art.CopyPage(name, new ushort[1024]); }
                public void Delegate() { Action<string> callback = Escaped; }
                public void Field() { art.CopyPage(finiteField, new ushort[1024]); }
                public void WriteFinite(bool v) { finiteField = v ? "Main.Empty" : "Main.WithData"; }
                public void UnknownField() { art.CopyPage(unknownField, new ushort[1024]); }
                public void WriteUnknown(string text) { unknownField = text; }
                public void RefField() { art.CopyPage(refField, new ushort[1024]); }
                public ref string Alias() => ref refField;
                public void TupleField() { art.CopyPage(tupleField, new ushort[1024]); }
                public void TupleWrite(string text) { int number; (tupleField, number) = (text, 0); }
                public void CyclicField() { art.CopyPage(cycleA, new ushort[1024]); }
                public void CycleWrite() { cycleA = cycleB; cycleB = cycleA; }
                public void PublicField() { art.CopyPage(publicField, new ushort[1024]); }
                public void LocalFunction(GameOptionsPresentation p) {
                    Page(GameOptionsPresentationDefinitions.PrimaryPage);
                    Page(GameOptionsPresentationDefinitions.SpecialEnglishPage);
                    void Page(string name) { p.CreatePage(name); }
                }
                public void Border(bool copy) {
                    string border = copy ? FileSelectPresentationDefinitions.CopyBorder : FileSelectPresentationDefinitions.MainBorder;
                    art.DrawBorder(null!, border);
                }
                static void Mutate(out string name) { name = "Missing"; }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(fixture, path: "fixture-name-flow.cs");
        SyntaxTree provider = CSharpSyntaxTree.ParseText(File.ReadAllText(
            "csharp/src/SuperMetroid.Core/Frontend/FileSelect/Artwork/FileSelectPresentation.cs"),
            path: "csharp/src/SuperMetroid.Core/Frontend/FileSelect/Artwork/FileSelectPresentation.cs");
        SyntaxTree optionsProvider = CSharpSyntaxTree.ParseText(File.ReadAllText(
            "csharp/src/SuperMetroid.Core/Frontend/Options/Artwork/GameOptionsPresentation.cs"),
            path: "csharp/src/SuperMetroid.Core/Frontend/Options/Artwork/GameOptionsPresentation.cs");
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        var compilation = CSharpCompilation.Create("FiniteNameFixture", [calls, provider, optionsProvider,
            CoreGlobalUsings.Tree()],
            platforms.Append(typeof(GameOptionsPresentation).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        SemanticModel semantic = compilation.GetSemanticModel(calls);
        var exports = new ResourceIndex();
        NamedPresentationAudit.Install(exports);
        var results = new Dictionary<string, (NamedSelectionResult Result, AuditReport Report)>();
        foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (semantic.GetOperation(call) is not IInvocationOperation operation) continue;
            var report = new AuditReport();
            NamedSelectionResult? result = NamedPresentationAudit.Inspect(operation, semantic, "fixture", calls.FilePath, exports, report);
            if (result is not { } selection) continue;
            string method = call.Ancestors().OfType<MethodDeclarationSyntax>().First().Identifier.ValueText;
            results.Add(method, (selection, report));
        }
        foreach (string valid in new[] { "ValidLocal", "Factory", "Receive", "Field", "LocalFunction", "Border" })
            Require(results[valid].Result == NamedSelectionResult.Resolved, valid + " must resolve its complete finite set");
        foreach (string unknown in new[] { "UnknownLocal", "RefLocal", "TupleLocal", "UnknownFactory", "ReceiveUnknown",
            "Escaped", "UnknownField", "RefField", "TupleField", "CyclicField", "PublicField" })
            Require(results[unknown].Result == NamedSelectionResult.Unresolved, unknown + " must retain its unknown boundary");
        Require(results["MissingFactory"].Result == NamedSelectionResult.Missing &&
            results["MissingFactory"].Report.MissingCount == 1, "finite absent names must remain concrete missing identities" + " (" + results["MissingFactory"].Report.Describe() + ")");
        Require(results.Count == 18 && results["ValidLocal"].Report.ReferenceCount == 3 &&
            results["Receive"].Report.ReferenceCount == 2 && results["LocalFunction"].Report.ReferenceCount == 2,
            "all potential assignments/call inputs must be inventoried, not only the nearest one");
    }

    /// <summary>Reports a violated finite-name classification contract.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Finite presentation-name confirmation failed: " + reason);
    }
}
