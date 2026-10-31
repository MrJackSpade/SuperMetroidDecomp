using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6454: Core comments and string literals name no hardware and cite no address (#1482).</summary>
internal static class CoreDocumentationVocabularyAnalyzerTests
{
    /// <summary>Each kind of text the rule reads, the owners it exempts, and the assemblies it skips.</summary>
    internal static void VerifyCoreDocumentationVocabularyAnalyzer()
    {
        var analyzer = new CoreDocumentationVocabularyAnalyzer();
        void Expect(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : CoreDocumentationVocabularyAnalyzer.DocumentationHardwareId, message, analyzer,
                assemblyName: CoreLayerAnalyzer.CoreAssembly);

        // Addresses and hardware words, in documentation, line comments, block comments and string literals.
        Expect("/// <summary>WRAM $07E1.</summary>\nclass P { }", "names the hardware ('$07E1')");
        Expect("class P { /// <summary>$AA:C95E, the shared no-op movement step.</summary>\nvoid Step() { } }", "('$AA:C95E')");
        Expect("class P { void Run() { // copies the tiles to VRAM\n } }", "('VRAM')");
        Expect("class P { /* the NMI handler */ }", "('NMI')");
        Expect("class P { string Name => \"OAM capacity\"; }", "('OAM')");
        Expect("class P { string Name => $\"{1} native words\"; }", "('native')");
        Expect("class P { /// <summary>Reads the cartridge's banks.</summary>\nvoid Read() { } }", "('cartridge')");
        // Game prose is not hardware: whole words only.
        Expect("/// <summary>Samus jumps from the riverbank toward Kraid's chamber.</summary>\nclass P { }");
        Expect("class P { string Name => \"Brinstar\"; }");
        // Compatibility services and encoding owners may describe the hardware they reproduce.
        Expect("[System.AttributeUsage(System.AttributeTargets.All)] sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }\n" +
            "/// <summary>Reproduces the NMI frame counter at $05B5.</summary>\n[CompatibilityService(\"frame counter\")] class Clock { }");
        Expect("[System.AttributeUsage(System.AttributeTargets.All)] sealed class EncodingOwnerAttribute(string format) : System.Attribute { public string Format { get; } = format; }\n" +
            "[EncodingOwner(\"pose numbers\")] static class Numbers { /// <summary>The native pose number.</summary>\nstatic int N => 0; }");
    }
}
