using SuperMetroid.EnsureAnalyzer;
using static SuperMetroid.EnsureVerification.AnalyzerTests;

namespace SuperMetroid.EnsureVerification;

/// <summary>
/// Checks parameter-list diagnostics against small C# declarations, including inherited contracts,
/// optional callback groups, and contexts with narrower capability interfaces.
/// </summary>
internal static class ParameterListAnalyzerTests
{
    /// <summary>#633 parameter-list rules: positive and negative cases for each diagnostic.</summary>
    internal static void VerifyParameterListAnalyzer()
    {
        var analyzer = new ParameterListAnalyzer();
        void Expect(string source, string? id = null, string? message = null) =>
            VerifySnippet(source, id, message, analyzer);
        const string contract =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class ParameterListContractAttribute(string reason) : System.Attribute { } ";
        const string eight = "int a, int b, int c, int d, int e, int f, int g, int h";
        const string seven = "int a, int b, int c, int d, int e, int f, int g";

        // SME6330: the ceiling is seven, counting optional parameters, for every kind of signature.
        Expect($"class C {{ void M({eight}) {{ }} }}", ParameterListAnalyzer.OversizedId, "C.M takes 8 parameters (ceiling 7)");
        Expect($"class C {{ void M({seven}) {{ }} }}");
        Expect($"class C {{ void M({seven}, int h = 0) {{ }} }}", ParameterListAnalyzer.OversizedId, "takes 8 parameters");
        Expect($"class C {{ C({eight}) {{ }} }}", ParameterListAnalyzer.OversizedId, "C constructor takes 8");
        Expect($"delegate void D({eight});", ParameterListAnalyzer.OversizedId, "delegate D takes 8");
        Expect($"class C {{ void M() {{ void Local({eight}) {{ }} Local(1, 2, 3, 4, 5, 6, 7, 8); }} }}",
            ParameterListAnalyzer.OversizedId, "local function Local takes 8");
        // An implementation inherits its signature: only the declaration answers for it.
        Expect(contract + $"interface I {{ [ParameterListContract(\"external callback\")] void M({eight}); }} " +
            $"class C : I {{ public void M({eight}) {{ }} }}");
        Expect(contract + $"abstract class B {{ [ParameterListContract(\"framework\")] public abstract void M({eight}); }} " +
            $"class C : B {{ public override void M({eight}) {{ }} }}");
        // Native and documented immutable boundaries, and positional record data shapes, are exempt.
        Expect($"class C {{ [System.Runtime.InteropServices.DllImport(\"native\")] static extern void M({eight}); }}");
        Expect(contract + $"class C {{ [ParameterListContract(\"serialized constructor\")] C({eight}) {{ }} }}");
        Expect($"record R({eight});");

        // SME6331: three nullable delegates switch capabilities on and off.
        Expect("using System; class C { void M(Action? a = null, Func<int>? b = null, Action<int>? c = null) { } }",
            ParameterListAnalyzer.CallbackClusterId, "takes 3 nullable callbacks");
        Expect("using System; class C { void M(Action? a = null, Func<int>? b = null) { } }");
        Expect("using System; class C { void M(Action a, Func<int> b, Action<int> c) { } }");

        // SME6332: a broad context whose narrower implemented interface covers everything read.
        const string contexts =
            "interface INarrow { int A { get; } } " +
            "interface IWide : INarrow { int B { get; } } " +
            "sealed class Wide : IWide { public int A => 1; public int B => 2; public int C => 3; } ";
        Expect(contexts + "class C { int M(Wide w) => w.A; }",
            ParameterListAnalyzer.BroadContextId, "Parameter 'w' of C.M only reads members of the narrower capability interface INarrow");
        Expect(contexts + "class C { int M(IWide w) => w.A; }",
            ParameterListAnalyzer.BroadContextId, "accept INarrow");
        Expect(contexts + "class C { int M(Wide w) => w.A + w.B; }",
            ParameterListAnalyzer.BroadContextId, "accept IWide");
        // Reading a member no narrower contract has, or using the context as a whole value, keeps it.
        Expect(contexts + "class C { int M(Wide w) => w.A + w.C; }");
        Expect(contexts + "class C { object M(Wide w) => w; }");
        Expect(contexts + "class C { int M(INarrow w) => w.A; }");
        Expect(contexts + "abstract class B { public abstract int M(Wide w); } class C : B { public override int M(Wide w) => w.A; }");
        Expect("sealed class Plain { public int A => 1; } class C { int M(Plain p) => p.A; }");
    }
}
