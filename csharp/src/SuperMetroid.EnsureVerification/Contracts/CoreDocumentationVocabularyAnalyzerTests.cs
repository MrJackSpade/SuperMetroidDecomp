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
        Expect("/// <summary>WRAM $07E1.</summary>\nclass P { }", "names the hardware or the debugger ('$07E1')");
        Expect("class P { /// <summary>$AA:C95E, the shared no-op movement step.</summary>\nvoid Step() { } }", "('$AA:C95E')");
        Expect("class P { void Run() { // copies the tiles to VRAM\n } }", "('VRAM')");
        Expect("class P { /* the NMI handler */ }", "('NMI')");
        Expect("class P { string Name => \"OAM capacity\"; }", "('OAM')");
        Expect("class P { string Name => $\"{1} native words\"; }", "('native')");
        Expect("class P { /// <summary>Reads the cartridge's banks.</summary>\nvoid Read() { } }", "('cartridge')");
        Expect("class P { // matches the 65C816 routine in the disassembly\n }", "('65C816')");
        // The processor's direct page, spaced or hyphenated, and the debugger in any word form (#1501).
        Expect("class P { /// <summary>The frame's direct page scratch word.</summary>\nint W => 0; }", "('direct page')");
        Expect("class P { void Run() { // keeps the direct-page copy\n } }", "('direct-page')");
        Expect("/// <summary>Excluded from debugger-state serialization.</summary>\nclass P { }", "('debugger')");
        Expect("class P { /* exposed so frame-by-frame debugging is useful */ }", "('debugging')");
        Expect("class P { string Name => \"Debugger watch\"; }", "('Debugger')");
        // Save RAM, RAM, the sprite and background layers, unsigned and byte citations, disassembly files, routine labels and
        // natively (#1528).
        Expect("class P { /// <summary>Copied into SRAM at save.</summary>\nvoid Save() { } }", "('SRAM')");
        Expect("class P { // the word lives in RAM\n }", "('RAM')");
        Expect("/// <summary>Draws the OBJ layer.</summary>\nclass P { }", "('OBJ')");
        Expect("class P { /* scrolls BG2 with the floor */ }", "('BG2')");
        Expect("class P { /// <summary>The routine at 88:DE43.</summary>\nvoid Run() { } }", "('88:DE43')");
        Expect("class P { // the timer starts at $1E\n }", "('$1E')");
        Expect("class P { /// <summary>Matches bank_87.asm.</summary>\nvoid Run() { } }", "('bank_87.asm')");
        Expect("class P { // as Handle_MapScrollArrows does\n }", "('Handle_MapScrollArrows')");
        Expect("class P { /* the game natively wraps it */ }", "('natively')");
        // Routine labels are read only in comments: a string literal's underscore-joined frame name is a presentation key.
        Expect("class P { string Frame => \"mother_brain_body_initial\"; }");
        // A lower-case frame name is a presentation key, not a routine label, even in a comment.
        Expect("class P { /// <summary>The draygon_frame_12 picture.</summary>\nint F => 0; }");
        // Identifiers are not prose, and a word that only begins the same is not the term.
        Expect("class P { /// <summary>The ramp, the object and the background fade up; time is 12:30.</summary>\nint Ram => 0; }");
        Expect("class P { bool Debugger => false; int DirectPage => 0; }");
        Expect("/// <summary>The cursor moves directly to the next page; the debug grapple selection stays.</summary>\nclass P { }");
        // Outside Core the rule reads nothing.
        VerifySnippet("/// <summary>A debugger-visible direct page word at WRAM $07E1.</summary>\nclass P { }", analyzer: analyzer);
        // An allowance marker is enforcement syntax; its own words are not read.
        Expect("class P { int M(int a) => a; // cartridge-encoding: allow(Wrap) - a signed delta word\n }");
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
