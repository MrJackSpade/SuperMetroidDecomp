using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1326 fixed-point, angle and carry rules, SME6351-SME6353.</summary>
internal static class FixedPointVocabularyAnalyzerTests
{
    /// <summary>Positive and negative cases for each rule.</summary>
    internal static void VerifyFixedPointVocabularyAnalyzer()
    {
        // SME6351: sibling members, parameters and tuples that split one value.
        Expect("class C { public int YSpeed { get; set; } public int YSubspeed { get; set; } }", FixedPointVocabularyAnalyzer.SplitFixedPointId, "'YSpeed' and 'YSubspeed'");
        Expect("class C { void M(ushort position, ushort subposition) { } }", FixedPointVocabularyAnalyzer.SplitFixedPointId, "'position' and 'subposition'");
        Expect("class C { (short Whole, ushort Fraction) M() => default; }", FixedPointVocabularyAnalyzer.SplitFixedPointId, "'Whole' and 'Fraction'");
        Expect("class C { int Speed; int SpeedFraction; }", FixedPointVocabularyAnalyzer.SplitFixedPointId, "'Speed' and 'SpeedFraction'");
        Expect("class C { public int Speed { get; set; } public int Health { get; set; } }");
        Expect("class C { (int X, int Y) M() => default; }");

        // SME6352: raw angles.
        Expect("class C { public byte HeadAngle { get; set; } }", FixedPointVocabularyAnalyzer.RawAngleId, "'HeadAngle' holds an angle as byte");
        Expect("class C { void M(ushort angle) { } }", FixedPointVocabularyAnalyzer.RawAngleId, "'angle' holds an angle as ushort");
        Expect("class C { void M(int angleIndex) { } }");
        Expect("class C { void M(byte angleStep, ushort angleDelta, ushort angleMask, ushort angleOffset) { } }");
        Expect("class C { public short Heading { get; set; } }", FixedPointVocabularyAnalyzer.RawAngleId, "'Heading' holds an angle as short");
        Expect("class C { public short HeadingY { get; set; } }");
        Expect("class C { public byte Health { get; set; } }");

        // SME6353: carry protocols.
        Expect("class C { void M(ref bool carry) { } }", FixedPointVocabularyAnalyzer.CarryProtocolId, "'carry' carries a processor flag");
        Expect("record R(bool Carry);", FixedPointVocabularyAnalyzer.CarryProtocolId, "'Carry' carries a processor flag");
        Expect("class C { void M(bool carry) { } }");

        // Codec boundaries and Hardware are exempt; other assemblies are not Core.
        Expect("[SnesEncodingOwner] class C { void M(ushort angle) { } }");
        Expect("class C { void M(ushort angle) { } }", path: "Hardware/C.cs");
        Expect("class C { void M(ushort angle) { } }", assembly: "SuperMetroid.Verification");
    }

    /// <summary>Compiles one Core file and checks that it reports exactly one expected diagnostic, or none.</summary>
    private static void Expect(string source, string? id = null, string? message = null, string path = "Enemies/C.cs",
        string assembly = CoreLayerAnalyzer.CoreAssembly)
    {
        const string attribute = "[System.AttributeUsage(System.AttributeTargets.All)] sealed class SnesEncodingOwnerAttribute : System.Attribute { } ";
        SyntaxTree[] trees =
        [
            CSharpSyntaxTree.ParseText(attribute + "namespace System.Runtime.CompilerServices { static class IsExternalInit { } }", path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Hardware/Attribute.cs"),
            CSharpSyntaxTree.ParseText(source, path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/" + path),
        ];
        var compilation = CSharpCompilation.Create(assembly, trees, AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Diagnostic[] errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid fixed-point fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        Diagnostic[] diagnostics = compilation.WithAnalyzers([new FixedPointVocabularyAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult().ToArray();
        string context = $"{assembly} {path}: {source}";
        if (id is null)
        {
            if (diagnostics.Length != 0)
                throw new InvalidOperationException($"Unexpected {diagnostics[0].Id}: {diagnostics[0].GetMessage()} for {context}");
            return;
        }
        if (diagnostics.Length != 1 || diagnostics[0].Id != id || !diagnostics[0].GetMessage().Contains(message ?? string.Empty, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Expected one {id} containing '{message}' for {context}; got " +
                string.Join("; ", diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}")));
        }
    }
}
