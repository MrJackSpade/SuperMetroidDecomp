using SuperMetroid.EnsureAnalyzer.Contracts;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6455: a verification member that builds an installation of its own states why (#1487).</summary>
internal static class PrivateInstallationAnalyzerTests
{
    /// <summary>Stand-ins for the installer, a component extractor, an unrelated <c>Extract</c>, and the stating attribute.</summary>
    private const string Helpers = """
        using SuperMetroid.AssetExtraction.Installation;
        using SuperMetroid.AssetExtraction.Enemies;
        namespace SuperMetroid.AssetExtraction.Installation
        {
            public sealed class GameInstallation { }
            public static class GameAssetInstaller
            {
                public static GameInstallation Install(string source, string root) => new();
                public static GameInstallation? EnsureInstalled(string root) => null;
                public static GameInstallation? OpenOrRepair(string root) => null;
            }
        }
        namespace SuperMetroid.AssetExtraction.Enemies
        {
            public static class EnemyTileArtworkFiles
            {
                public static void Extract(string root) { }
                public static bool Exists(string root) => false;
            }
        }
        static class Archive { public static void Extract(string root) { } }
        [System.AttributeUsage(System.AttributeTargets.Method | System.AttributeTargets.Constructor | System.AttributeTargets.Property)]
        sealed class PrivateInstallationAttribute(string reason) : System.Attribute { public string Reason { get; } = reason; }

        """;

    /// <summary>Each building call, each stated owner, an empty reason, nested functions, and an unchecked assembly.</summary>
    internal static void VerifyPrivateInstallationAnalyzer()
    {
        var analyzer = new PrivateInstallationAnalyzer();
        void Expect(string source, string? message = null, string assembly = "SuperMetroid.Verification") =>
            VerifySnippet(Helpers + source, message is null ? null : PrivateInstallationAnalyzer.PrivateInstallationId, message, analyzer,
                assemblyName: assembly);

        // Each installer entry point and a component extractor build an installation of their own.
        Expect("static class C { static void M() => GameAssetInstaller.Install(\"rom\", \"root\"); }",
            "'GameAssetInstaller.Install' builds an installation of its own");
        Expect("static class C { static void M() => GameAssetInstaller.EnsureInstalled(\"root\"); }", "'GameAssetInstaller.EnsureInstalled'");
        Expect("static class C { static void M() => GameAssetInstaller.OpenOrRepair(\"root\"); }", "'GameAssetInstaller.OpenOrRepair'");
        Expect("static class C { static void M() => EnemyTileArtworkFiles.Extract(\"root\"); }",
            "'EnemyTileArtworkFiles.Extract' builds an installation of its own");
        // Every verification assembly is checked, not only SuperMetroid.Verification.
        Expect("static class C { static void M() => GameAssetInstaller.Install(\"rom\", \"root\"); }", "'GameAssetInstaller.Install'",
            "SuperMetroid.IntegrationVerification");
        // A method group handed on builds an installation where it is called.
        Expect("static class C { static System.Func<string, GameInstallation?> M() => GameAssetInstaller.EnsureInstalled; }",
            "'GameAssetInstaller.EnsureInstalled'");
        // A lambda or local function in an unstated member belongs to that member.
        Expect("static class C { static void M() { System.Action a = () => GameAssetInstaller.Install(\"rom\", \"root\"); a(); } }",
            "'GameAssetInstaller.Install'");
        Expect("static class C { static void M() { Run(); static void Run() => EnemyTileArtworkFiles.Extract(\"root\"); } }",
            "'EnemyTileArtworkFiles.Extract'");
        // An empty or whitespace reason states nothing.
        Expect("static class C { [PrivateInstallation(\"\")] static void M() => GameAssetInstaller.Install(\"rom\", \"root\"); }",
            "'GameAssetInstaller.Install'");
        Expect("static class C { [PrivateInstallation(\"  \")] static void M() => GameAssetInstaller.Install(\"rom\", \"root\"); }",
            "'GameAssetInstaller.Install'");

        // A stated method, constructor and property.
        Expect("static class C { [PrivateInstallation(\"exercises the installer's repair of a damaged installation\")] static void M() => GameAssetInstaller.OpenOrRepair(\"root\"); }");
        Expect("sealed class C { [PrivateInstallation(\"writes overrides into a fresh installation\")] C() => GameAssetInstaller.Install(\"rom\", \"root\"); }");
        Expect("sealed class C { [PrivateInstallation(\"exercises the installer\")] GameInstallation P => GameAssetInstaller.Install(\"rom\", \"root\"); }");
        // A lambda and a local function inside a stated method belong to it.
        Expect("static class C { [PrivateInstallation(\"exercises the installer\")] static void M() { System.Action a = () => GameAssetInstaller.Install(\"rom\", \"root\"); a(); } }");
        Expect("static class C { [PrivateInstallation(\"exercises the extractor\")] static void M() { Run(); static void Run() => EnemyTileArtworkFiles.Extract(\"root\"); } }");
        // Other members of an extraction type, and an Extract outside the import pipeline, build nothing.
        Expect("static class C { static bool M() => EnemyTileArtworkFiles.Exists(\"root\"); }");
        Expect("static class C { static void M() => Archive.Extract(\"root\"); }");
        // Assemblies that are not verification are not checked.
        Expect("static class C { static void M() => GameAssetInstaller.Install(\"rom\", \"root\"); }", assembly: "SuperMetroid.Tooling");
        Expect("static class C { static void M() => EnemyTileArtworkFiles.Extract(\"root\"); }", assembly: "SuperMetroid.Desktop");
        Console.WriteLine("Private installations: an installer or extractor call in verification is SME6455 unless its member states a reason.");
    }
}
