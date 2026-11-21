using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6484: in Core and the import pipeline, a pattern test or a chain of equality tests listing two or more members
/// of one identity enum is reported, while a single member's test, a switch, a named constant, a flags enum and another
/// assembly are left alone (#2559).
/// </summary>
internal static class IdentityListingAnalyzerTests
{
    /// <summary>Each listing test the rule reports, and the tests it leaves alone.</summary>
    internal static void VerifyIdentityListingAnalyzer()
    {
        var analyzer = new IdentityListingAnalyzer();
        const string Id = IdentityListingAnalyzer.IdentityListingId;
        const string Types = "enum ThingId { Ridley, RidleyCeres, Kraid } enum ShotKind { Beam, Missile, Bomb } " +
            "[System.Flags] enum ButtonId { None = 0, A = 1, B = 2 } ";
        void Expect(string body, string assembly = CoreLayerAnalyzer.CoreAssembly, params string[] ids) =>
            VerifySnippetIds("namespace SuperMetroid.Core.World { " + Types + "class Probe { " + body + " } }", analyzer, assembly, ids);

        // Pattern tests and equality chains listing two members of one identity are reported, in Core and the import pipeline.
        Expect("bool Ridleys(ThingId thing) => thing is ThingId.Ridley or ThingId.RidleyCeres;", ids: [Id]);
        Expect("bool NotRidleys(ThingId thing) => thing is not (ThingId.Ridley or ThingId.RidleyCeres);", ids: [Id]);
        Expect("bool Ridleys(ThingId thing) => thing == ThingId.Ridley || thing == ThingId.RidleyCeres;", ids: [Id]);
        Expect("bool NotRidleys(ThingId thing) => thing != ThingId.Ridley && thing != ThingId.RidleyCeres;", ids: [Id]);
        Expect("bool Ridleys(ThingId thing, bool ready) => ready && (thing == ThingId.Ridley || thing == ThingId.RidleyCeres);", ids: [Id]);
        Expect("bool Ridleys(ThingId thing) => thing is ThingId.Ridley or ThingId.RidleyCeres;", "SuperMetroid.AssetExtraction", ids: [Id]);

        // One member's test, members of different values, a switch mapping members, a named constant, a non-identity enum,
        // a flags identity and another assembly are left alone.
        Expect("bool IsRidley(ThingId thing) => thing == ThingId.Ridley;");
        Expect("bool Either(ThingId first, ThingId second) => first == ThingId.Ridley || second == ThingId.Kraid;");
        Expect("int Size(ThingId thing) => thing switch { ThingId.Ridley or ThingId.RidleyCeres => 2, _ => 1 };");
        Expect("const ThingId Boss = ThingId.Ridley; bool IsBoss(ThingId thing) => thing == Boss;");
        Expect("bool Ordnance(ShotKind kind) => kind is ShotKind.Missile or ShotKind.Bomb;");
        Expect("bool Buttons(ButtonId button) => button is ButtonId.A or ButtonId.B;");
        Expect("bool Ridleys(ThingId thing) => thing is ThingId.Ridley or ThingId.RidleyCeres;", "SuperMetroid.Verification");

        Console.WriteLine("Identity listing rule: pattern tests and equality chains listing identity members reported; single " +
            "members, separate values, switches, named constants, other enums, flags identities and other assemblies left alone.");
    }
}
