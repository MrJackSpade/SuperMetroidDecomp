using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6488: in Core and the import pipeline, a switch or a dictionary initializer mapping every member of one enum
/// to the member of another named for it, two enums with the same members, and a member named by joining two or more
/// members of one type are reported, while a lookup to unrelated values, a partial map, a flags enum, a one-word part and
/// another assembly are left alone (#2574).
/// </summary>
internal static class IdentityDuplicationAnalyzerTests
{
    /// <summary>Each second identity and joined name the rule reports, and the shapes it leaves alone.</summary>
    internal static void VerifyIdentityDuplicationAnalyzer()
    {
        var analyzer = new IdentityDuplicationAnalyzer();
        const string Id = IdentityDuplicationAnalyzer.IdentityDuplicationId;
        const string Types = "enum Dust { Small, Smoke, Big, Rock } enum DeathPuff { Small, Smoke, Big } enum Piece { Left, Right } " +
            "enum PieceLook { PieceLeft, PieceRight, Slime } enum Shot { Beam, Missile, MissileBlast } enum Reaction { Beam, Missile } " +
            "enum Colour { Red, Green } [System.Flags] enum Bits { None = 0, Small = 1, Smoke = 2, Big = 4 } ";
        void Expect(string body, string assembly = CoreLayerAnalyzer.CoreAssembly, params string[] ids) =>
            VerifySnippetIds("namespace SuperMetroid.Core.World { " + Types + body + " }", analyzer, assembly, ids);

        // A total map onto members named for each key, each to its own or reaching every member of the other, is a mirror.
        Expect("class Map { Dust Of(DeathPuff puff) => puff switch { DeathPuff.Small => Dust.Small, DeathPuff.Smoke => Dust.Smoke, DeathPuff.Big => Dust.Big, _ => throw new System.Exception() }; }", ids: [Id]);
        Expect("class Map { System.Collections.Generic.Dictionary<Piece, PieceLook> Looks = new() { [Piece.Left] = PieceLook.PieceLeft, [Piece.Right] = PieceLook.PieceRight }; }", ids: [Id]);
        Expect("class Map { Reaction Of(Shot shot) => shot switch { Shot.Beam or Shot.MissileBlast => Reaction.Beam, Shot.Missile => Reaction.Missile, _ => throw new System.Exception() }; }", ids: [Id]);
        Expect("class Map { Dust Of(DeathPuff puff) => puff switch { DeathPuff.Small => Dust.Small, DeathPuff.Smoke => Dust.Smoke, DeathPuff.Big => Dust.Big, _ => throw new System.Exception() }; }", "SuperMetroid.AssetExtraction", ids: [Id]);

        // Two enums with the same members are one identity defined twice; both are reported.
        Expect("enum Side { Up, Down } enum Facing { Down, Up }", ids: [Id, Id]);

        // A name joined from two members of one type, each of two words, is reported.
        Expect("static class Rooms { static int EastShaft; static int WestShaft; } static class Effects { static int EastShaftWestShaft; }", ids: [Id]);

        // A map to unrelated names, a partial map, a map within one enum, a flags enum, one-word parts and another assembly
        // are left alone.
        Expect("class Map { Colour Of(Piece piece) => piece switch { Piece.Left => Colour.Red, Piece.Right => Colour.Green, _ => throw new System.Exception() }; }");
        Expect("class Map { Dust Of(DeathPuff puff) => puff switch { DeathPuff.Small => Dust.Small, _ => Dust.Rock }; }");
        Expect("class Map { DeathPuff Next(DeathPuff puff) => puff switch { DeathPuff.Small => DeathPuff.Smoke, DeathPuff.Smoke => DeathPuff.Big, DeathPuff.Big => DeathPuff.Small, _ => throw new System.Exception() }; }");
        Expect("class Map { Bits Of(DeathPuff puff) => puff switch { DeathPuff.Small => Bits.Small, DeathPuff.Smoke => Bits.Smoke, DeathPuff.Big => Bits.Big, _ => throw new System.Exception() }; }");
        Expect("static class Rooms { static int East; static int West; } static class Effects { static int EastWest; }");
        Expect("class Map { Dust Of(DeathPuff puff) => puff switch { DeathPuff.Small => Dust.Small, DeathPuff.Smoke => Dust.Smoke, DeathPuff.Big => Dust.Big, _ => throw new System.Exception() }; }", "SuperMetroid.Verification");

        Console.WriteLine("Identity duplication rule: mirroring maps, same-member enums and joined names reported; unrelated, " +
            "partial and self maps, flags enums, one-word parts and other assemblies left alone.");
    }
}
