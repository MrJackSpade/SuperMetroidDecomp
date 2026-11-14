using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6457: Core spells the room grid's operations through their named conversions, not the machine's shifts and
/// masks (#1753, #1786).
/// </summary>
internal static class RoomGridIdiomAnalyzerTests
{
    /// <summary>The spatial types the cases read, declared as Core declares them.</summary>
    private const string Spatial =
        "namespace SuperMetroid.Core.Spatial { public interface ISpatialSpace { } public sealed class RoomPixels : ISpatialSpace { }" +
        " public sealed class RoomBlocks : ISpatialSpace { } public sealed class ScreenPixels : ISpatialSpace { }" +
        " public readonly record struct Position16<TSpace>(ushort X, ushort Y) where TSpace : ISpatialSpace;" +
        " public readonly record struct FixedCoordinate(ushort Whole, ushort Fraction);" +
        " public readonly record struct FixedPosition<TSpace>(FixedCoordinate X, FixedCoordinate Y) where TSpace : ISpatialSpace;" +
        " public static class CoordinateAxis { public static int Wrap(int coordinate) => unchecked((ushort)coordinate); }" +
        " public static class RoomGridConversions { public static int PixelToBlock(int pixel) => pixel >> 4;" +
        " public static int BlockStart(Position16<RoomPixels> p) => p.X & ~15; public static int Column(Position16<RoomPixels> p) => p.X >> 4; } }";

    /// <summary>Each room grid idiom the rule reports, and the operations it leaves alone.</summary>
    internal static void VerifyRoomGridIdiomAnalyzer()
    {
        var analyzer = new RoomGridIdiomAnalyzer();
        void Expect(string body, string? idiom = null, string assembly = CoreLayerAnalyzer.CoreAssembly) =>
            VerifySnippet(Spatial + " namespace SuperMetroid.Core.Rooms { using SuperMetroid.Core.Spatial; static class Probe { " + body + " } }",
                idiom is null ? null : RoomGridIdiomAnalyzer.RoomGridIdiomId,
                idiom is null ? null : $"'{idiom}' spells a room grid operation as a machine bit idiom", analyzer, assemblyName: assembly);

        // #1753: an axis of a room pixel position, read directly or in parentheses, and the whole pixel of a fixed-point
        // one, shifted to its block.
        Expect("static int Column(Position16<RoomPixels> p) => p.X >> 4;", "p.X >> 4");
        Expect("static int Row(Position16<RoomPixels> p) => (p.Y) >> 4;", "(p.Y) >> 4");
        Expect("static int Column(FixedPosition<RoomPixels> p) => p.X.Whole >> 4;", "p.X.Whole >> 4");
        Expect("static int Row(FixedPosition<RoomPixels> p) => p.Y.Whole >>> 4;", "p.Y.Whole >>> 4");
        // #1786: the block and half-block idioms on a room pixel.
        Expect("static int Within(Position16<RoomPixels> p) => p.X & 15;", "p.X & 15");
        Expect("static int Within(FixedPosition<RoomPixels> p) => 15 & p.Y.Whole;", "15 & p.Y.Whole");
        Expect("static bool SecondHalf(FixedPosition<RoomPixels> p) => (p.X.Whole & 8) != 0;", "p.X.Whole & 8");
        Expect("static int WithinHalf(Position16<RoomPixels> p) => p.Y & 7;", "p.Y & 7");
        Expect("static int Start(Position16<RoomPixels> p) => p.X & ~15;", "p.X & ~15");
        Expect("static int Start(Position16<RoomPixels> p) => p.X & 0xFFF0;", "p.X & 0xFFF0");
        Expect("static int HalfStart(FixedPosition<RoomPixels> p) => p.X.Whole & ~7;", "p.X.Whole & ~7");
        Expect("static int End(Position16<RoomPixels> p) => p.X | 15;", "p.X | 15");
        Expect("static int HalfEnd(FixedPosition<RoomPixels> p) => p.X.Whole | 7;", "p.X.Whole | 7");
        Expect("static int Mirrored(FixedPosition<RoomPixels> p) => p.X.Whole ^ 15;", "p.X.Whole ^ 15");
        Expect("static int Half(Position16<RoomPixels> p) => p.X >> 3;", "p.X >> 3");
        Expect("static int Pixel(Position16<RoomBlocks> b) => b.Y << 4;", "b.Y << 4");
        // A room pixel offset by a length, or wrapped to sixteen bits, is still a room pixel.
        Expect("static int Within(FixedPosition<RoomPixels> p, int r) => (p.Y.Whole - r) & 15;", "(p.Y.Whole - r) & 15");
        Expect("static int Start(FixedPosition<RoomPixels> p, int r) => (r + p.X.Whole - 1) & ~15;", "(r + p.X.Whole - 1) & ~15");
        Expect("static bool SecondHalf(FixedPosition<RoomPixels> p, int r) => (CoordinateAxis.Wrap(p.X.Whole - r) & 8) != 0;",
            "CoordinateAxis.Wrap(p.X.Whole - r) & 8");
        // Other shifts and masks, other spaces, plain integers, a distance between two room pixels and a fixed coordinate
        // outside a room position are not grid operations.
        Expect("static int Screen(Position16<RoomPixels> p) => p.X >> 8;");
        Expect("static int Offset(Position16<RoomPixels> p) => p.X & 255;");
        Expect("static int Parity(Position16<RoomPixels> p) => p.X & 1;");
        Expect("static int Column(Position16<ScreenPixels> p) => p.X >> 4;");
        Expect("static int Within(Position16<ScreenPixels> p) => p.X & 15;");
        Expect("static int Column(FixedPosition<ScreenPixels> p) => p.X.Whole >> 4;");
        Expect("static int Pixel(Position16<RoomPixels> p) => p.X << 4;");
        Expect("static int Sixteenth(int speed) => speed >> 4;");
        Expect("static int Fraction(int speed) => speed & 15;");
        Expect("static int Sixteenth(FixedCoordinate c) => c.Whole & 15;");
        Expect("static int Apart(Position16<RoomPixels> p, Position16<RoomPixels> q) => (p.X - q.X) & 15;");
        // The spatial namespace owns the operations, and only Core is read.
        Expect("static int Column(Position16<RoomPixels> p) => SuperMetroid.Core.Spatial.RoomGridConversions.PixelToBlock(p.X);");
        Expect("static int Within(Position16<RoomPixels> p) => p.X & 15;", assembly: "SuperMetroid.Verification");
        Console.WriteLine("Room grid idiom rule: room pixel shifts, masks, ors and mirrors and room block shifts reported; other operations, spaces, integers, distances, the spatial types and other assemblies left alone.");
    }
}
