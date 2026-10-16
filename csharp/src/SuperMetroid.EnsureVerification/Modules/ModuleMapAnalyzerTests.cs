using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Modules;

/// <summary>Checks the #626 module-map rules: SME6402-SME6405.</summary>
internal static class ModuleMapAnalyzerTests
{
    /// <summary>A fixture repository rooted at <see cref="Root"/> whose map is <c>csharp/module-map.json</c>.</summary>
    private const string Root = "C:/fixture-repo";

    /// <summary>A small map exercising every rule form.</summary>
    private const string Map = """
        {
          "modules": [
            { "path": "csharp/src/SuperMetroid.Core", "namespace": "SuperMetroid.Core", "role": "production", "references": [] },
            { "path": "csharp/src/SuperMetroid.Tooling", "namespace": "SuperMetroid.Tooling", "role": "tooling", "references": ["SuperMetroid.Core"],
              "mirrors": { "Core": "SuperMetroid.Core" } },
            { "path": "csharp/src/SuperMetroid.Verification", "namespace": "SuperMetroid.Verification", "role": "verification", "references": [],
              "domainOf": "SuperMetroid.Core", "sharedModules": ["Harness"] },
            { "path": "csharp/test-support/Fixtures", "namespace": "SuperMetroid.TestSupport.Fixtures", "role": "verification-support" },
            { "path": "csharp/test-fixtures", "namespace": "", "role": "fixture" }
          ],
          "coreLayers": [
            { "name": "Foundation", "modules": ["Hardware"] },
            { "name": "Simulation", "modules": ["Samus"] },
            { "name": "Session", "modules": ["Runtime"] }
          ]
        }
        """;

    /// <summary>Positive and negative cases for the layout and layering rules.</summary>
    internal static void VerifyModuleMapAnalyzers()
    {
        // SME6402: the namespace is the module's namespace plus the file's subdirectories; mirrors switch roots.
        Layout("csharp/src/SuperMetroid.Core/Hardware/Ppu.cs", "namespace SuperMetroid.Core.Hardware; class Ppu { }");
        Layout("csharp/src/SuperMetroid.Core/Hardware/Ppu.cs", "namespace SuperMetroid.Core; class Ppu { }",
            ModuleLayoutAnalyzer.NamespaceId, "'Ppu' declares namespace 'SuperMetroid.Core'; its path in module 'csharp/src/SuperMetroid.Core' implies 'SuperMetroid.Core.Hardware'");
        Layout("csharp/src/SuperMetroid.Core/Hardware/Ppu.cs", "class Ppu { }", ModuleLayoutAnalyzer.NamespaceId, "declares namespace '(global)'");
        Layout("csharp/src/SuperMetroid.Core/Hardware/Ppu.cs", "namespace SuperMetroid.Core.Hardware { class Ppu { } }");
        Layout("csharp/src/SuperMetroid.Core/GlobalUsings.cs", "global using System.Text;");
        Layout("csharp/src/SuperMetroid.Tooling/Core/Samus/SamusTooling.cs", "namespace SuperMetroid.Core.Samus; static class SamusTooling { }");
        Layout("csharp/src/SuperMetroid.Tooling/Core/Samus/SamusTooling.cs", "namespace SuperMetroid.Tooling.Core.Samus; static class SamusTooling { }",
            ModuleLayoutAnalyzer.NamespaceId, "implies 'SuperMetroid.Core.Samus'");
        Layout("csharp/src/SuperMetroid.Tooling/RequiredJson.cs", "namespace SuperMetroid.Tooling; static class RequiredJson { }");
        Layout("csharp/test-support/Fixtures/Room/F.cs", "namespace SuperMetroid.TestSupport.Fixtures.Room; class F { }");

        // SME6403: every compiled file lies in a registered module; fixtures, generated output and outside paths are exempt.
        Layout("csharp/elsewhere/X.cs", "namespace X; class X { }", ModuleLayoutAnalyzer.UnregisteredId, "'csharp/elsewhere/X.cs' is not inside a module");
        Layout("csharp/src/SuperMetroid.Coreish/X.cs", "namespace X; class X { }", ModuleLayoutAnalyzer.UnregisteredId);
        Layout("csharp/test-fixtures/build-policy/Y.cs", "namespace BuildPolicy; class Y { }");
        Layout("csharp/src/SuperMetroid.Core/obj/Release/Generated.cs", "class Generated { }");
        Layout("D:/elsewhere/Outside.cs", "class Outside { }", rooted: false);

        // SME6405: verification files sit in a Core domain module or a declared shared module.
        Layout("csharp/src/SuperMetroid.Verification/Samus/SamusVerification.cs", "namespace SuperMetroid.Verification.Samus; static class SamusVerification { }");
        Layout("csharp/src/SuperMetroid.Verification/Harness/Assertions.cs", "namespace SuperMetroid.Verification.Harness; static class Assertions { }");
        Layout("csharp/src/SuperMetroid.Verification/Misc/MiscVerification.cs", "namespace SuperMetroid.Verification.Misc; static class MiscVerification { }",
            ModuleLayoutAnalyzer.DomainId, "'MiscVerification' is in 'csharp/src/SuperMetroid.Verification/Misc', which is not a SuperMetroid.Core module or a declared shared module (Harness)");
        Layout("csharp/src/SuperMetroid.Verification/Loose.cs", "namespace SuperMetroid.Verification; static class Loose { }", ModuleLayoutAnalyzer.DomainId);
        Layout("csharp/src/SuperMetroid.Verification/GlobalUsings.cs", "global using System.Text;");

        // Without the map the analyzers are not configured for the compilation.
        Layout("csharp/elsewhere/X.cs", "class X { }", withMap: false);

        // SME6404: a Core module depends only on its own or a lower layer.
        Layers([("Samus/SamusState.cs", "namespace SuperMetroid.Core.Samus; public sealed class SamusState { }"),
                ("Hardware/Ppu.cs", "namespace SuperMetroid.Core.Hardware; using SuperMetroid.Core.Samus; class Ppu { SamusState? s; }")],
            "'SamusState' (Simulation module Samus) is referenced from Foundation module Hardware");
        Layers([("Samus/SamusState.cs", "namespace SuperMetroid.Core.Samus; using SuperMetroid.Core.Hardware; public sealed class SamusState { Ppu? p; }"),
                ("Hardware/Ppu.cs", "namespace SuperMetroid.Core.Hardware; public sealed class Ppu { }"),
                ("Runtime/Frame.cs", "namespace SuperMetroid.Core.Runtime; using SuperMetroid.Core.Samus; class Frame { SamusState? s = new(); }")]);
        Layers([("Runtime/Clock.cs", "namespace SuperMetroid.Core.Runtime; public static class Clock { public static int Tick(this int x) => x; }"),
                ("Samus/SamusState.cs", "namespace SuperMetroid.Core.Samus; using SuperMetroid.Core.Runtime; class SamusState { int M() => 1.Tick(); }")],
            "'Clock' (Session module Runtime) is referenced from Simulation module Samus");
        Layers([("Runtime/Frame.cs", "namespace SuperMetroid.Core.Runtime; public sealed class Frame { }"),
                ("Samus/SamusState.cs", "namespace SuperMetroid.Core.Samus; /// <summary>Stepped by <see cref=\"SuperMetroid.Core.Runtime.Frame\"/>.</summary> public sealed class SamusState { }")]);
        Layers([("Samus/SamusState.cs", "namespace SuperMetroid.Core.Samus; public sealed class SamusState { }"),
                ("Ensure.cs", "namespace SuperMetroid.Core; using SuperMetroid.Core.Samus; static class Ensure { static SamusState? s; }")],
            "referenced from Foundation module (Core root)");
        Layers([("Samus/SamusState.cs", "namespace SuperMetroid.Core.Samus; public sealed class SamusState { }"),
                ("Hardware/Ppu.cs", "namespace SuperMetroid.Core.Hardware; using SuperMetroid.Core.Samus; class Ppu { SamusState? s; }")],
            null, assembly: "SuperMetroid.Verification");
        Layers([("Mystery/Thing.cs", "namespace SuperMetroid.Core.Mystery; public sealed class Thing { }")],
            "Core module 'Mystery' is not assigned a layer", id: CoreLayerAnalyzer.UnlayeredId);
        Console.WriteLine("#626 module map: SME6402 namespaces, SME6403 registered modules, SME6404/SME6406 Core layers and SME6405 verification domains enforce the map.");
    }

    /// <summary>Checks the layout analyzer on one file at a fixture path.</summary>
    private static void Layout(string relativePath, string source, string? id = null, string? message = null, bool rooted = true, bool withMap = true)
    {
        string path = rooted ? Root + "/" + relativePath : relativePath;
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);
        var compilation = CSharpCompilation.Create("LayoutCase", [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var diagnostics = Run(compilation, new ModuleLayoutAnalyzer(), withMap);
        Require(diagnostics, id, message, $"{relativePath}: {source}");
    }

    /// <summary>Checks the layer analyzer on a set of Core files compiled as <paramref name="assembly"/>.</summary>
    private static void Layers((string Path, string Source)[] files, string? message = null, string assembly = CoreLayerAnalyzer.CoreAssembly,
        string id = CoreLayerAnalyzer.Id)
    {
        var trees = files.Select(file => CSharpSyntaxTree.ParseText(file.Source, new CSharpParseOptions(LanguageVersion.Preview),
            path: Root + "/csharp/src/SuperMetroid.Core/" + file.Path)).ToArray();
        var compilation = CSharpCompilation.Create(assembly, trees, AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid layer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = Run(compilation, new CoreLayerAnalyzer(), withMap: true);
        Require(diagnostics, message is null ? null : id, message, string.Join(" | ", files.Select(file => file.Path)));
    }

    private static ImmutableArray<Diagnostic> Run(Compilation compilation, DiagnosticAnalyzer analyzer, bool withMap)
    {
        ImmutableArray<AdditionalText> additional = withMap
            ? [new MemoryAdditionalText(Root + "/csharp/" + ModuleMap.FileName, Map)]
            : [];
        return compilation.WithAnalyzers([analyzer], new AnalyzerOptions(additional))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
    }

    private static void Require(ImmutableArray<Diagnostic> diagnostics, string? id, string? message, string context)
    {
        if (id is null)
        {
            if (diagnostics.Length != 0)
                throw new InvalidOperationException($"Unexpected {diagnostics[0].Id}: {diagnostics[0].GetMessage()} for {context}");
            return;
        }
        if (diagnostics.Length == 0 || diagnostics.Any(d => d.Id != id || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {id} containing '{message}', got [{string.Join("; ", diagnostics.Select(d => d.Id + ": " + d.GetMessage()))}] for {context}");
        }
    }
}
