using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SuperMetroid.EnsureAnalyzer.Contracts;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6456: native-sized integers and raw pointers appear only in native interop (#1509).</summary>
internal static class NativeAddressAnalyzerTests
{
    /// <summary>Each spelling the rule reports, and the interop it leaves alone.</summary>
    internal static void VerifyNativeAddressAnalyzer()
    {
        var analyzer = new NativeAddressAnalyzer();
        void Expect(string source, string? spelled = null, bool allowUnsafe = false) =>
            VerifySnippet(source, spelled is null ? null : NativeAddressAnalyzer.NativeAddressId,
                spelled is null ? null : $"'{spelled}' is a native address", analyzer, allowUnsafe: allowUnsafe);

        // Native-sized integers by every spelling, and raw pointers.
        Expect("class P { nint handle; }", "nint");
        Expect("class P { void Share(System.IntPtr memory) { } }", "IntPtr");
        Expect("class P { System.UIntPtr Size() => default; }", "UIntPtr");
        Expect("class P { object Address(int value) => (nint)value; }", "nint");
        Expect("class P { unsafe void Write(byte* bytes) { } }", "byte*", allowUnsafe: true);
        // A local typed by 'var' is the expression's: only the spelled type is reported.
        Expect("class P { object Zero() { var none = System.IntPtr.Zero; return none; } }", "IntPtr");
        // Interop is outside the rule: a type that declares a platform invoke, and what is nested in it.
        Expect("static class Native { [System.Runtime.InteropServices.DllImport(\"kernel32.dll\")] static extern nint GetCurrentProcess();" +
            " static nint Current() => GetCurrentProcess(); }");
        Expect("static class Host { static class Native { [System.Runtime.InteropServices.DllImport(\"user32.dll\")]" +
            " static extern bool DestroyWindow(nint window); } static nint window; }");
        // The native host assemblies hand the operating system and the GPU handles; the rule does not read them.
        VerifySnippet("class Upload { nint buffer; }", analyzer: analyzer, assemblyName: "SuperMetroid.Rendering.Direct3D11");
        VerifySnippet("class Device { nint device; }", analyzer: analyzer, assemblyName: "SuperMetroid.Desktop");
        // Ordinary integers and handles typed by the domain are not native addresses.
        Expect("class P { int index; long ticks; System.Runtime.InteropServices.SafeHandle? device; }");
        // Test-support's generated access to a native host type's private members restates its interop (#2570); an
        // extension block of any other type does not.
        ExpectAccess("SuperMetroid.Desktop", count: 0);
        ExpectAccess("HostlessCase", count: 1);
        Console.WriteLine("Native address rule: native integers and raw pointers outside interop reported; interop, host assemblies and their generated access left alone.");
    }

    /// <summary>
    /// Compiles a window type in an assembly named <paramref name="host"/> and, in a check referencing it, an extension block
    /// of that window restating a window-handle signature; compares the SME6456 count with <paramref name="count"/>.
    /// </summary>
    private static void ExpectAccess(string host, int count)
    {
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable);
        CSharpCompilation window = CSharpCompilation.Create(host,
            [CSharpSyntaxTree.ParseText("public class Window { }", new CSharpParseOptions(LanguageVersion.Preview))], References, options);
        CSharpCompilation check = CSharpCompilation.Create("SuperMetroid.DesktopVerification",
            [CSharpSyntaxTree.ParseText("""
                internal static class WindowPrivateAccess
                {
                    extension(Window self)
                    {
                        internal int Start(System.Func<nint, int> create) => create(0);
                    }
                }
                """, new CSharpParseOptions(LanguageVersion.Preview))],
            [.. References, window.ToMetadataReference()], options);
        var errors = check.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length != 0)
            throw new InvalidOperationException($"Invalid analyzer fixture: {string.Join("; ", errors.Select(error => error.ToString()))}");
        var diagnostics = check.WithAnalyzers([new NativeAddressAnalyzer()]).GetAnalyzerDiagnosticsAsync().GetAwaiter().GetResult();
        if (diagnostics.Length != count || diagnostics.Any(diagnostic => diagnostic.Id != NativeAddressAnalyzer.NativeAddressId))
            throw new InvalidOperationException(
                $"Expected {count} SME6456 diagnostic(s) in an extension of {host}'s window, got [{string.Join("; ", diagnostics.Select(diagnostic => diagnostic.ToString()))}].");
    }
}
