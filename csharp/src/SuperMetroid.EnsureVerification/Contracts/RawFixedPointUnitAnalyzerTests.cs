using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using SuperMetroid.EnsureVerification.Harness;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #2555 raw fixed-point unit rule, SME6463.</summary>
internal static class RawFixedPointUnitAnalyzerTests
{
    /// <summary>The advice a raw-part finding ends with.</summary>
    private const string RawAdvice = "writes a constant in a measure's raw parts";

    /// <summary>The advice a fraction not in lowest terms ends with.</summary>
    private const string LowestTermsAdvice = "writes a fraction not in its lowest terms";

    /// <summary>The advice a literal shift ends with.</summary>
    private const string ShiftAdvice = "shifts a value by a literal count";

    /// <summary>A measure with raw-unit and domain factories, the encoding owner attribute and a coordinate, as Core declares them.</summary>
    private const string Declarations = """
        namespace SuperMetroid.Core
        {
            [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
            public sealed class EncodingOwnerAttribute(string format) : System.Attribute
            {
                public string Format { get; } = format;
            }
        }
        namespace SuperMetroid.Core.Spatial
        {
            [EncodingOwner("signed 8.8 fixed-point number")]
            public readonly record struct Speed(int Raw)
            {
                public static Speed Zero => default;
                public static Speed FromTwoHundredFiftySixthsOfPixel(int twoHundredFiftySixths) => new(twoHundredFiftySixths);
                public static Speed FromSixteenthsOfPixel(int sixteenths) => new(sixteenths * 16);
                public static Speed FromParts(int wholePixels, int fraction) => new(wholePixels * 256 + fraction);
                public static Speed FromPixels(int pixels) => FromParts(pixels, 0);
                public static Speed FromPixelFraction(int numerator, int denominator) => new(numerator * (256 / denominator));
                public static Speed Half => FromTwoHundredFiftySixthsOfPixel(128);
                public int Whole => Raw >> 8;
            }
            public readonly struct Coordinate
            {
                public Coordinate(int whole, int fraction)
                {
                    Whole = whole;
                    Fraction = fraction;
                }
                public int Whole { get; }
                public int Fraction { get; }
                public Coordinate WithFraction(int fraction) => new(Whole, fraction);
                public static Coordinate Of(int whole, int fraction) => new(whole, fraction);
            }
        }
        """;

    /// <summary>One positive per way a raw unit is written, and the uses the rule leaves alone.</summary>
    internal static void VerifyRawFixedPointUnitAnalyzer()
    {
        // A constant to a raw-unit factory, whatever its parameter is named, a constant expression and a named constant.
        Expect(Body("return Speed.FromTwoHundredFiftySixthsOfPixel(96);"), "'Speed.FromTwoHundredFiftySixthsOfPixel(96)' " + RawAdvice);
        Expect(Body("return Speed.FromSixteenthsOfPixel(-3);"), RawAdvice);
        Expect(Body("return Speed.FromParts(1, 0);"), RawAdvice);
        Expect(Body("return Speed.FromParts(input, 0);"), RawAdvice);
        Expect(Body("return Speed.FromTwoHundredFiftySixthsOfPixel(12 * 255);"), RawAdvice);
        Expect(Body("return Speed.FromTwoHundredFiftySixthsOfPixel(Step);"), RawAdvice);
        // A constant to a parameter that counts a raw fraction, a constructor's included.
        Expect(Body("_ = new Coordinate(5, 0).WithFraction(0x8000);\n        return Speed.Zero;"), count: 2);
        Expect(Body("_ = Coordinate.Of(input, 0);\n        return Speed.Zero;"), RawAdvice);
        // A fraction factory given a fraction not in lowest terms, or a whole number as a fraction.
        Expect(Body("return Speed.FromPixelFraction(96, 256);"), LowestTermsAdvice);
        Expect(Body("return Speed.FromPixelFraction(2, 1);"), LowestTermsAdvice);
        // A computed value shifted by a literal count, directly or in a compound assignment.
        Expect(Body("return Speed.FromPixels(input >> 4);"), "'input >> 4' " + ShiftAdvice);
        Expect(Body("int scaled = input;\n        scaled <<= 2;\n        return Speed.FromPixels(scaled);"), ShiftAdvice);

        // The domain factories, computed raw amounts, constant shifts and the encoding owner's own counting pass.
        Expect(Body("return Speed.FromPixelFraction(3, 8);"), count: 0);
        Expect(Body("return Speed.FromPixelFraction(-1, 2);"), count: 0);
        Expect(Body("return Speed.FromPixels(7);"), count: 0);
        Expect(Body("return Speed.FromTwoHundredFiftySixthsOfPixel(input * 3);"), count: 0);
        Expect(Body("return Speed.FromParts(input, input);"), count: 0);
        Expect(Body("_ = new Coordinate(input, input).WithFraction(input);\n        return Speed.FromPixels(1 << 3);"), count: 0);
        Expect(Body("return Speed.FromPixels(input >> input);"), count: 0);
        Expect(Body("return Speed.FromPixels(input >> Shift);"), count: 0);
        Expect(Body("return Speed.Half;"), count: 0);
        // Outside Core the rule does not apply.
        Expect(Body("return Speed.FromTwoHundredFiftySixthsOfPixel(96);"), count: 0, assemblyName: "SuperMetroid.AssetExtraction");
    }

    /// <summary>A Core class whose method <c>Make</c> runs <paramref name="statements"/> over an input and a named step.</summary>
    private static string Body(string statements) => Declarations + $$"""

        namespace SuperMetroid.Core.Enemies
        {
            using SuperMetroid.Core.Spatial;
            internal static class Fixture
            {
                private const int Step = 48;
                private const int Shift = 4;
                internal static Speed Make(int input)
                {
                    {{statements}}
                }
            }
        }
        """;

    /// <summary>Compiles one source and compares the SME6463 count and an optional message fragment.</summary>
    private static void Expect(string source, string? message = null, int count = 1, string assemblyName = CoreLayerAnalyzer.CoreAssembly)
    {
        var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview),
            path: "C:/fixture-repo/csharp/src/SuperMetroid.Core/Enemies/Fixture.cs");
        var compilation = CSharpCompilation.Create(assemblyName, [tree], AnalyzerTests.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(e => e.ToString()))}");
        var diagnostics = compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new RawFixedPointUnitAnalyzer()))
            .GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count ||
            diagnostics.Any(d => d.Id != RawFixedPointUnitAnalyzer.RawFixedPointUnitId || d.Severity != DiagnosticSeverity.Error) ||
            (message is not null && !diagnostics.All(d => d.GetMessage().Contains(message, StringComparison.Ordinal))))
        {
            throw new InvalidOperationException(
                $"Expected {count} {RawFixedPointUnitAnalyzer.RawFixedPointUnitId} diagnostic(s) containing '{message}', " +
                $"got [{string.Join("; ", diagnostics.Select(d => d.ToString()))}] in: {source}");
        }
    }
}
