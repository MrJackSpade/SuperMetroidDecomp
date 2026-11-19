using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6464: a value carried across rooms or pool places is read and left in Core only by a compatibility service
/// (#2572).
/// </summary>
internal static class CarriedValueAnalyzerTests
{
    /// <summary>The carried storage and the attribute the cases read, declared as Core declares them.</summary>
    private const string Declarations =
        "namespace SuperMetroid.Core.Compatibility { [System.AttributeUsage(System.AttributeTargets.All)]" +
        " public sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }" +
        " [CompatibilityService(\"the room mains' shared value\")] public sealed class RoomMainCarryover { public int Held; public int Value => Held; public void Leave(int value) => Held = value; }" +
        " [CompatibilityService(\"what a room leaves the next\")] public sealed class RoomHandOver { public int ShotHorizontalFraction(int place) => place; public void LeaveShots(int[] fractions) { } }" +
        " public sealed class WrappingCountdown { public int RemainingFrames => 0; } }";

    /// <summary>Each storage member the rule reports outside a compatibility service, and the uses it leaves alone.</summary>
    internal static void VerifyCarriedValueAnalyzer()
    {
        var analyzer = new CarriedValueAnalyzer();
        void Expect(string body, string? member = null, string assembly = CoreLayerAnalyzer.CoreAssembly,
            string space = "SuperMetroid.Core.World", string attributes = "") =>
            VerifySnippet(Declarations + " namespace " + space +
                " { using SuperMetroid.Core.Compatibility; " + attributes + " static class Probe { " + body + " } }",
                member is null ? null : CarriedValueAnalyzer.CarriedValueId,
                member is null ? null : $"'{member}' reads or leaves a value carried across rooms or pool places outside a compatibility service",
                analyzer, assemblyName: assembly);

        // Every member of the room mains' shared value, read or left in gameplay code, is reported.
        Expect("static int Debris(RoomMainCarryover carried) => carried.Value;", "RoomMainCarryover.Value");
        Expect("static void Quake(RoomMainCarryover carried) => carried.Leave(4);", "RoomMainCarryover.Leave");
        Expect("static int Raw(RoomMainCarryover carried) => carried.Held;", "RoomMainCarryover.Held");
        Expect("static System.Action<int> Leaver(RoomMainCarryover carried) => carried.Leave;", "RoomMainCarryover.Leave");
        // A room's hand-over of its pool places, read or left outside a service, is reported.
        Expect("static int Fraction(RoomHandOver handOver) => handOver.ShotHorizontalFraction(1);", "RoomHandOver.ShotHorizontalFraction");
        Expect("static void Leave(RoomHandOver handOver) => handOver.LeaveShots([]);", "RoomHandOver.LeaveShots");
        // A compatibility service uses the storage it carries, in its own members and in a type nested in it.
        Expect("static int Debris(RoomMainCarryover carried) => carried.Value;", attributes: "[CompatibilityService(\"the shaft's turn sets the hall's debris wait\")]");
        Expect("static class Inner { static int Fraction(RoomHandOver handOver) => handOver.ShotHorizontalFraction(0); }",
            attributes: "[CompatibilityService(\"a left beam starts from its place's last shot\")]");
        // Creating the storage and handing it on, other compatibility types and other assemblies are not reported.
        Expect("static RoomMainCarryover Shared() => new RoomMainCarryover();");
        Expect("static RoomHandOver Pass(RoomHandOver handOver) => handOver;");
        Expect("static int Left(WrappingCountdown countdown) => countdown.RemainingFrames;");
        Expect("static int Debris(RoomMainCarryover carried) => carried.Value;", assembly: "SuperMetroid.Verification");
        Console.WriteLine("Carried value rule: the room mains' shared value and a room's pool-place hand-over reported when read or left outside a compatibility service; services, types nested in them, creating and passing the storage, other types and other assemblies left alone.");
    }
}
