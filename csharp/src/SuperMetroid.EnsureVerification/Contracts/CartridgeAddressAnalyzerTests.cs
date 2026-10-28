using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1411 cartridge-address rule, SME6447.</summary>
internal static class CartridgeAddressAnalyzerTests
{
    /// <summary>A functional Core file whose name gives no cartridge context.</summary>
    private const string Functional = "C:/fixture-repo/csharp/src/SuperMetroid.Core/Samus/Beam.cs";

    /// <summary>The advice every SME6447 message ends with.</summary>
    private const string Advice = "; the address belongs to the AssetExtraction codec that reads it, which produces the definition or identity";

    /// <summary>One positive per address kind and value site, and the values the rule leaves alone.</summary>
    internal static void VerifyCartridgeAddressAnalyzer()
    {
        // A 24-bit address in each mapped kind of bank, in each value site.
        Expect("class Beam { private const int Trail = 0x8F91F8; }", "$8F91F8 is a 24-bit address in cartridge ROM bank $8F" + Advice);
        Expect("class Hud { private static readonly int[] cells = [0x7E0A1C]; }", "$7E0A1C is a 24-bit address in work RAM bank $7E" + Advice);
        Expect("class Saves { public int Slot { get; } = 0x700010; }", "$700010 is a 24-bit address in save RAM bank $70" + Advice);
        Expect("enum Lift { Start = 0xA28000 }", "$A28000 is a 24-bit address in cartridge ROM bank $A2" + Advice);
        Expect("""
            sealed class AtAttribute(int location) : System.Attribute { public int Location => location; }
            [At(0x7E0A1C)] class Probe { }
            """, "$7E0A1C is a 24-bit address in work RAM bank $7E" + Advice);
        Expect("class Walk { int Run() { const int Lift = 0xA28000; return Lift; } }", "$A28000 is a 24-bit address in cartridge ROM bank $A2" + Advice);
        // An address composed from literals is one address.
        Expect("class Beam { private const int Trail = (0x8F << 16) | 0x91F8; }", "$8F91F8 is a 24-bit address in cartridge ROM bank $8F" + Advice);
        // A reference to a reported constant is checked where the constant is declared.
        Expect("class Beam { private const int Trail = 0x8F91F8; private const int Same = Trail; }",
            "$8F91F8 is a 24-bit address in cartridge ROM bank $8F" + Advice);

        // A 16-bit address where the type, the namespace or the file names a cartridge structure.
        Expect("enum SamusProjectileRomData : ushort { Wave = 0x91F8 }",
            "$91F8 is a 16-bit address in the cartridge structure 'SamusProjectileRomData'" + Advice);
        Expect("class ProgramPointers { int Run(int p) => p switch { 0xA0B2 => 1, _ => 0 }; }",
            "$A0B2 is a 16-bit address in the cartridge structure 'ProgramPointers'" + Advice);
        Expect("namespace Game.Snes { class Dispatch { int Run(int p) { switch (p) { case 0x9438: return 1; default: return 0; } } } }",
            "$9438 is a 16-bit address in the cartridge namespace 'Game.Snes'" + Advice);
        Expect("class SamusXrayRomData { public static class Window { public const ushort Table = 0xC000; } }",
            "$C000 is a 16-bit address in the cartridge structure 'SamusXrayRomData'" + Advice);
        // A definitions catalog holds domain data, not a cartridge structure: its name alone names no address (#1450).
        Expect("class XrayScopeDefinitions { public static class Window { public const ushort Table = 0xC000; } }", count: 0);
        Expect("class BeamDefinitions { internal const ushort Program = 0x8000; }",
            "$8000 is a 16-bit address in the catalog file 'BeamDefinitions.cs'" + Advice,
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Samus/BeamDefinitions.cs");

        // A 16-bit value is a quantity where nothing names a cartridge structure.
        Expect("class SamusMovement { private const ushort SignBit = 0x8000; }", count: 0);
        // Definition data below the cartridge's half of a bank, even in a cartridge structure.
        Expect("class SamusXrayRomData { public const byte RightFrameOne = 0x19; public const int UnitVector = 0x0100; }", count: 0);
        // Unmapped banks, the ROM's lower half and the low mirrors are not addresses the game uses.
        Expect("class Speeds { private const int Boost = 0x18000; private const int Far = 0x5F8000; private const int Low = 0x8F1234; private const int Top = 0xE08000; }", count: 0);
        // Words that only contain a structure word name no cartridge structure.
        Expect("class RomanceBanker { private const ushort High = 0xC000; }", count: 0);
        // A value computed in a method body is not a declared value site.
        Expect("class Beam { int Run(int p) => p + 0x8F91F8; }", count: 0);
        // Outside Core the import pipeline reads the cartridge.
        Expect("class SamusProjectileRomData { private const int Trail = 0x8F91F8; }", count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>Compiles one source at <paramref name="path"/> and compares the SME6447 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string path = Functional,
        string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), path: path);
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new CartridgeAddressAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != CartridgeAddressAnalyzer.CartridgeAddressId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {CartridgeAddressAnalyzer.CartridgeAddressId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
