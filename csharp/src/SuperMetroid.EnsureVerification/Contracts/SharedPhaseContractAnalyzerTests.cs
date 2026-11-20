using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>
/// Checks SME6473: in Core, a System.Enum value, a dictionary keyed by Type and a species owner named outside its own
/// namespaces are reported; the species composition alone may name owners, hands none out and is the only one (#2571).
/// </summary>
internal static class SharedPhaseContractAnalyzerTests
{
    /// <summary>The species contracts, the composition mark and one species with its owner, declared as Core declares them.</summary>
    private const string Declarations =
        "namespace SuperMetroid.Core { [System.AttributeUsage(System.AttributeTargets.Class)] sealed class SpeciesCompositionAttribute : System.Attribute { } }" +
        " namespace SuperMetroid.Core.Enemies { interface IEnemySpecies { } interface IEnemyEntityFactory { } interface IRoomGlow { void StepGlow(); } }" +
        " namespace SuperMetroid.Core.Enemies.Robots { using SuperMetroid.Core.Enemies; sealed class RobotBehavior : IEnemySpecies, IRoomGlow { public void StepGlow() { } internal int Count; }" +
        " sealed class RobotParts { int M(RobotBehavior robot) => robot.Count; } }";

    /// <summary>Each shape the rule reports, and the uses it leaves alone.</summary>
    internal static void VerifySharedPhaseContractAnalyzer()
    {
        var analyzer = new SharedPhaseContractAnalyzer();
        const string Id = SharedPhaseContractAnalyzer.SharedPhaseContractId;
        void Expect(string body, string ns = "SuperMetroid.Core.World", string assembly = CoreLayerAnalyzer.CoreAssembly, params string[] ids) =>
            VerifySnippetIds(Declarations + " namespace " + ns + " { using System; using System.Collections.Generic; using System.Linq;" +
                " using SuperMetroid.Core.Enemies; using SuperMetroid.Core.Enemies.Robots; " + body + " }", analyzer, assembly, ids);

        // A value typed System.Enum is reported wherever it is declared or named as a value.
        Expect("class Probe { Enum? Reaction; }", ids: [Id]);
        Expect("class Probe { IReadOnlyList<Enum> Reactions() => []; }", ids: [Id]);
        Expect("class Probe { string Key(System.Enum reaction) => reaction.ToString(); }", ids: [Id]);
        Expect("class Probe { object[] Cast(DayOfWeek[] days) => days.Cast<Enum>().ToArray(); }", ids: [Id]);
        Expect("class Probe { bool Is(object value) => value is Enum; }", ids: [Id]);
        // A constraint, a static member of Enum, typeof and nameof are not values.
        Expect("class Probe { T[] All<T>() where T : struct, Enum => Enum.GetValues<T>(); }");
        Expect("class Probe { bool Defined(DayOfWeek day) => System.Enum.IsDefined(day); Type T => typeof(Enum); string N => nameof(Enum); }");
        // A dictionary keyed by Type is reported, declared or created.
        Expect("class Probe { Dictionary<Type, object> _catalogs = new(); }", ids: [Id]);
        Expect("class Probe { IReadOnlyDictionary<Type, string>? Names; }", ids: [Id]);
        Expect("class Probe { object Make() => new Dictionary<Type, int>(); }", ids: [Id]);
        Expect("class Probe { Dictionary<string, Type> _types = new(); }");
        // A species owner named outside its own namespaces is reported: a member typed as it, a local, a construction and a
        // member read through it.
        Expect("class Probe { RobotBehavior? Robot; }", ids: [Id]);
        Expect("class Probe { void Step(object owner) { RobotBehavior robot = (RobotBehavior)owner; } }", ids: [Id, Id]);
        Expect("class Probe { object Make() => new RobotBehavior(); }", ids: [Id]);
        Expect("class Probe { int Read(IRoomGlow glow) => glow is RobotBehavior robot ? robot.Count : 0; }", ids: [Id, Id]);
        Expect("class Probe { int Read(RobotBehavior robot) => robot.Count; }", "SuperMetroid.Core.Enemies", ids: [Id, Id]);
        // The species' own namespaces, a shared contract and another assembly are left alone.
        Expect("class Probe { void Step(IReadOnlyList<IRoomGlow> glows) { foreach (IRoomGlow glow in glows) glow.StepGlow(); } }");
        Expect("class Probe { RobotBehavior Robot = new(); }", "SuperMetroid.Core.Enemies.Robots.Arms");
        Expect("class Probe { RobotBehavior? Robot; Enum? Reaction; }", assembly: "SuperMetroid.Verification");
        // The species composition may build and keep owners privately, and hands out contracts.
        Expect("[SuperMetroid.Core.SpeciesComposition] sealed class Species { private readonly RobotBehavior _robot = new();" +
            " internal IReadOnlyList<IRoomGlow> Glows => [_robot]; private sealed class Holder(RobotBehavior robot) { } }",
            "SuperMetroid.Core.Enemies");
        // A member of the composition that hands an owner out is reported, as is a second composition.
        Expect("[SuperMetroid.Core.SpeciesComposition] sealed class Species { internal RobotBehavior Robot { get; } = new(); }",
            "SuperMetroid.Core.Enemies", ids: [Id]);
        Expect("[SuperMetroid.Core.SpeciesComposition] sealed class Species { internal IReadOnlyList<RobotBehavior> Robots { get; } = []; }",
            "SuperMetroid.Core.Enemies", ids: [Id]);
        Expect("[SuperMetroid.Core.SpeciesComposition] sealed class First { } [SuperMetroid.Core.SpeciesComposition] sealed class Second { }",
            "SuperMetroid.Core.Enemies", ids: [Id, Id]);
        Console.WriteLine("Shared phase contract rule: System.Enum values, Type-keyed dictionaries and species owners named outside their " +
            "own namespaces are reported; constraints, Enum's static members, typeof, nameof, the species' own namespaces, shared " +
            "contracts and other assemblies are left alone, and the one species composition names owners and hands out only contracts.");
    }
}
