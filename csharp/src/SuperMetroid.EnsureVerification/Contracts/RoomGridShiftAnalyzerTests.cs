using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6457: Core converts a room pixel to its block by the room grid's named conversion, not a raw shift (#1753).</summary>
internal static class RoomGridShiftAnalyzerTests
{
    /// <summary>The spatial types the cases read, declared as Core declares them.</summary>
    private const string Spatial =
        "namespace SuperMetroid.Core.Spatial { public interface ISpatialSpace { } public sealed class RoomPixels : ISpatialSpace { }" +
        " public sealed class ScreenPixels : ISpatialSpace { }" +
        " public readonly record struct Position16<TSpace>(ushort X, ushort Y) where TSpace : ISpatialSpace;" +
        " public readonly record struct FixedCoordinate(ushort Whole, ushort Fraction);" +
        " public readonly record struct FixedPosition<TSpace>(FixedCoordinate X, FixedCoordinate Y) where TSpace : ISpatialSpace;" +
        " public static class RoomGridConversions { public static int PixelToBlock(int pixel) => pixel >> 4;" +
        " public static int Column(Position16<RoomPixels> p) => p.X >> 4; } }";

    /// <summary>Each room pixel coordinate the rule reports when shifted to its block, and the shifts it leaves alone.</summary>
    internal static void VerifyRoomGridShiftAnalyzer()
    {
        var analyzer = new RoomGridShiftAnalyzer();
        void Expect(string body, string? shifted = null, string assembly = CoreLayerAnalyzer.CoreAssembly) =>
            VerifySnippet(Spatial + " namespace SuperMetroid.Core.Rooms { using SuperMetroid.Core.Spatial; static class Probe { " + body + " } }",
                shifted is null ? null : RoomGridShiftAnalyzer.RoomGridShiftId,
                shifted is null ? null : $"'{shifted}' converts a room pixel to its block", analyzer, assemblyName: assembly);

        // An axis of a room pixel position, read directly or in parentheses, and the whole pixel of a fixed-point one.
        Expect("static int Column(Position16<RoomPixels> p) => p.X >> 4;", "p.X >> 4");
        Expect("static int Row(Position16<RoomPixels> p) => (p.Y) >> 4;", "(p.Y) >> 4");
        Expect("static int Column(FixedPosition<RoomPixels> p) => p.X.Whole >> 4;", "p.X.Whole >> 4");
        Expect("static int Row(FixedPosition<RoomPixels> p) => p.Y.Whole >>> 4;", "p.Y.Whole >>> 4");
        // Other shifts, other spaces, plain integers and a fixed coordinate outside a room position are not the conversion.
        Expect("static int Screen(Position16<RoomPixels> p) => p.X >> 8;");
        Expect("static int Column(Position16<ScreenPixels> p) => p.X >> 4;");
        Expect("static int Column(FixedPosition<ScreenPixels> p) => p.X.Whole >> 4;");
        Expect("static int Sixteenth(int speed) => speed >> 4;");
        Expect("static int Sixteenth(FixedCoordinate c) => c.Whole >> 4;");
        // The spatial namespace owns the conversion, and only Core is read.
        Expect("static int Column(Position16<RoomPixels> p) => SuperMetroid.Core.Spatial.RoomGridConversions.PixelToBlock(p.X);");
        Expect("static int Column(Position16<RoomPixels> p) => p.X >> 4;", assembly: "SuperMetroid.Verification");
        Console.WriteLine("Room grid shift rule: room pixel axes shifted to their block reported; other shifts, spaces, integers, the spatial types and other assemblies left alone.");
    }
}
