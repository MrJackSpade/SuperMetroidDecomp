using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6463: a sixteen-bit coordinate wrap is taken in Core only by a compatibility service or the spatial types
/// that declare the coordinate family (#2568).
/// </summary>
internal static class CoordinateWrapAnalyzerTests
{
    /// <summary>The spatial and compatibility types the cases read, declared as Core declares them.</summary>
    private const string Declarations =
        "namespace SuperMetroid.Core.Spatial { public interface ISpatialSpace { } public sealed class RoomPixels : ISpatialSpace { }" +
        " public static class CoordinateAxis { public static int Wrap(int c) => unchecked((ushort)c);" +
        " public static int Difference(int to, int from) => unchecked((short)(to - from)); public static int AsSigned(int c) => unchecked((short)c);" +
        " public static int Distance(int to, int from) => System.Math.Abs(Difference(to, from)); public static bool Precedes(int c, int r) => Difference(c, r) < 0;" +
        " public const int LastNonNegative = short.MaxValue; }" +
        " public readonly record struct Position16<TSpace>(int X, int Y) where TSpace : ISpatialSpace {" +
        " public static Position16<TSpace> Wrapped(int x, int y) => new(CoordinateAxis.Wrap(x), CoordinateAxis.Wrap(y)); }" +
        " public readonly record struct Offset16<TSpace>(int X, int Y) where TSpace : ISpatialSpace {" +
        " public static Offset16<TSpace> Wrapped(int x, int y) => new(CoordinateAxis.AsSigned(x), CoordinateAxis.AsSigned(y)); }" +
        " public readonly record struct FixedCoordinate(int Whole) { public FixedCoordinate WithWrappedWhole(int w) => new(CoordinateAxis.Wrap(w));" +
        " public FixedCoordinate WithWhole(int w) => new(w); }" +
        " public readonly record struct FixedPosition<TSpace>(FixedCoordinate X, FixedCoordinate Y) where TSpace : ISpatialSpace {" +
        " public FixedPosition<TSpace> WithWrappedWholeX(int w) => this with { X = X.WithWrappedWhole(w) };" +
        " public FixedPosition<TSpace> WithWrappedWholeY(int w) => this with { Y = Y.WithWrappedWhole(w) };" +
        " public FixedPosition<TSpace> WithWholeX(int w) => this with { X = X.WithWhole(w) }; }" +
        " public readonly record struct TrigonometryAngle(int RawValue) { public static TrigonometryAngle Wrapped(int units) => new(units & 255); } }" +
        " namespace SuperMetroid.Core.Compatibility { [System.AttributeUsage(System.AttributeTargets.All)]" +
        " public sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; } }";

    /// <summary>Each wrapping member the rule reports outside a compatibility service, and the uses it leaves alone.</summary>
    internal static void VerifyCoordinateWrapAnalyzer()
    {
        var analyzer = new CoordinateWrapAnalyzer();
        void Expect(string body, string? member = null, string assembly = CoreLayerAnalyzer.CoreAssembly,
            string space = "SuperMetroid.Core.Bosses", string attributes = "") =>
            VerifySnippet(Declarations + " namespace " + space +
                " { using SuperMetroid.Core.Spatial; using SuperMetroid.Core.Compatibility; " + attributes + " static class Probe { " + body + " } }",
                member is null ? null : CoordinateWrapAnalyzer.CoordinateWrapId,
                member is null ? null : $"'{member}' wraps a sixteen-bit coordinate outside a compatibility service", analyzer,
                assemblyName: assembly);

        // Every method of the per-axis wrap, called in gameplay code, is reported.
        Expect("static int Place(int x) => CoordinateAxis.Wrap(x - 80);", "CoordinateAxis.Wrap");
        Expect("static int Apart(int a, int b) => CoordinateAxis.Difference(a, b);", "CoordinateAxis.Difference");
        Expect("static bool Before(int y) => CoordinateAxis.AsSigned(y) < 0;", "CoordinateAxis.AsSigned");
        Expect("static int Near(int a, int b) => CoordinateAxis.Distance(a, b);", "CoordinateAxis.Distance");
        Expect("static bool Left(int a, int b) => CoordinateAxis.Precedes(a, b);", "CoordinateAxis.Precedes");
        // A method group naming one, and one in a field initialiser or a lambda, are reported too.
        Expect("static System.Func<int, int> Wrapper() => CoordinateAxis.Wrap;", "CoordinateAxis.Wrap");
        Expect("static readonly int Parked = CoordinateAxis.Wrap(-80);", "CoordinateAxis.Wrap");
        Expect("static System.Func<int, int> Signed() => y => CoordinateAxis.AsSigned(y);", "CoordinateAxis.AsSigned");
        // The positions' wrapping factories and the fixed positions' wrapping members.
        Expect("static Position16<RoomPixels> At(int x) => Position16<RoomPixels>.Wrapped(x, -16);", "Position16.Wrapped");
        Expect("static Offset16<RoomPixels> View(int x) => Offset16<RoomPixels>.Wrapped(x, x);", "Offset16.Wrapped");
        Expect("static FixedCoordinate Moved(FixedCoordinate c) => c.WithWrappedWhole(c.Whole - 4);", "FixedCoordinate.WithWrappedWhole");
        Expect("static FixedPosition<RoomPixels> Up(FixedPosition<RoomPixels> p) => p.WithWrappedWholeY(p.Y.Whole - 4);", "FixedPosition.WithWrappedWholeY");
        Expect("static FixedPosition<RoomPixels> Left(FixedPosition<RoomPixels> p) => p.WithWrappedWholeX(p.X.Whole - 4);", "FixedPosition.WithWrappedWholeX");
        // A compatibility service takes the wrap it reproduces, in its own members and in a type nested in it.
        Expect("static int Place(int x) => CoordinateAxis.Wrap(x);", attributes: "[CompatibilityService(\"an actor parked left of the room\")]");
        Expect("static class Inner { static bool Before(int a, int b) => CoordinateAxis.Precedes(a, b); }",
            attributes: "[CompatibilityService(\"distances across the room's edge\")]");
        // The spatial namespace declares the family and its conversions.
        Expect("static int Place(int x) => CoordinateAxis.Wrap(x);", space: "SuperMetroid.Core.Spatial.Conversions");
        // Plain arithmetic, the family's plain members, its constant, other wrapping types and other assemblies are not reported.
        Expect("static int Apart(int a, int b) => a - b;");
        Expect("static FixedPosition<RoomPixels> Down(FixedPosition<RoomPixels> p) => p.WithWholeX(p.X.Whole + 4);");
        Expect("static int Last() => CoordinateAxis.LastNonNegative;");
        Expect("static TrigonometryAngle Back() => TrigonometryAngle.Wrapped(-8);");
        Expect("static int Place(int x) => CoordinateAxis.Wrap(x);", assembly: "SuperMetroid.Verification");
        Console.WriteLine("Coordinate wrap rule: every CoordinateAxis method, the positions' Wrapped factories and the fixed positions' WithWrappedWhole members reported outside a compatibility service; services, nested types in them, the spatial namespace, plain arithmetic, constants, other wrapping types and other assemblies left alone.");
    }
}
