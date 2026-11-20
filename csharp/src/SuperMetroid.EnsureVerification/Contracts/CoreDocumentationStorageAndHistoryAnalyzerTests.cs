using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks SME6479: Core documentation tells the game, never storage, the original program or the port's history (#2550).</summary>
internal static class CoreDocumentationStorageAndHistoryAnalyzerTests
{
    /// <summary>Each vocabulary the rule reads, the comments it reads them in, the owners it exempts and the English it leaves.</summary>
    internal static void VerifyCoreDocumentationStorageAndHistoryAnalyzer()
    {
        var analyzer = new CoreDocumentationStorageAndHistoryAnalyzer();
        void Expect(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : CoreDocumentationStorageAndHistoryAnalyzer.DocumentationStorageAndHistoryId,
                message, analyzer, assemblyName: CoreLayerAnalyzer.CoreAssembly);

        // A value told by its storage's range or a machine number.
        Expect("class P { /// <summary>The game's short frame counter for the pass, 0 through 255, which cadences sample.</summary>\nint C => 0; }",
            "('short frame counter for the pass, 0 through 255')");
        Expect("class P { /// <summary>The chance: the lowest 16384 of the 65536 samples, a quarter.</summary>\nint C => 0; }", "('of the 65536 samples')");
        Expect("class P { /// <summary>Initial whole-pixel screen X, -32768 through 32767.</summary>\nint X => 0; }", "('screen X, -32768 through 32767')");
        Expect("class P { /// <summary>Whether it jumps left: its lowest bit.</summary>\nbool L => false; }", "('its lowest bit')");
        // A unit of the original's storage.
        Expect("class P { /// <summary>Moves the Baby by its velocity, in 256ths of a pixel.</summary>\nvoid Move() { } }", "('in 256ths of a pixel')");
        Expect("class P { // It starts at 24/256 and each hit adds more.\n }", "('24/256')");
        Expect("class P { /// <summary>Starts its first animation and fifteen nontransparent colors.</summary>\nvoid Start() { } }", "('fifteen nontransparent colors')");
        // Reasoning about a value's storage range.
        Expect("class P { void Run() { // adding the acceleration stays far inside its range\n } }", "('far inside its range')");
        Expect("class P { /// <summary>The room is at most fifty screens tall.</summary>\nint G => 0; }", "('at most fifty screens')");
        // The original's program flow.
        Expect("class P { void Run() { // The setup falls directly into the suspense wait.\n } }", "('falls directly into')");
        Expect("class P { /// <summary>Runs the body's main/hurt entry.</summary>\nvoid Run() { } }", "('main/hurt entry')");
        // The original's routines and engine parts.
        Expect("class P { /// <summary>Marks the shot after the projectile prelude.</summary>\nvoid Run() { } }", "('projectile prelude')");
        Expect("class P { void Run() { // before the common mover consumes its new velocity\n } }", "('common mover')");
        Expect("class P { /// <summary>Defers init-AI camera reads.</summary>\nbool D => false; }", "('init-AI')");
        // Installing animations and loading colours.
        Expect("class P { /// <summary>Installs the laser head animation and starts the wait.</summary>\nvoid Run() { } }",
            "('Installs the laser head animation')");
        Expect("class P { /// <summary>The colours his ceiling rocks draw in, which his initialization loads.</summary>\nint S => 0; }",
            "('which his initialization loads')");
        // The display hardware in gameplay prose.
        Expect("class P { /// <summary>Emits the low-priority frames.</summary>\nvoid Draw() { } }", "('low-priority frames')");
        Expect("class P { /// <summary>The body is drawn partly as a background layer.</summary>\nvoid Draw() { } }", "('as a background layer')");
        // Storage named for what the original kept.
        Expect("class P { /// <summary>Sets the statue's own animating flag.</summary>\nvoid Mark() { } }", "('animating flag')");
        Expect("class P { void Run() { // The setup rewrites only the block's type and parameter.\n } }", "('rewrites only')");
        // Tables, entries, reads past an end and allocation.
        Expect("class P { /// <summary>The gestures, in allocation order.</summary>\nint G => 0; }", "('allocation order')");
        Expect("class P { /// <summary>Places the tongue: a normal second enemy.</summary>\nvoid Place() { } }", "('a normal second enemy')");
        // The port's reasoning and history.
        Expect("class P { /// <summary>This object deliberately owns only the image.</summary>\nint I => 0; }", "('deliberately owns')");
        Expect("class P { /// <summary>Models the shared freeze being set.</summary>\nvoid Freeze() { } }", "('Models the')");
        Expect("class P { /// <summary>Ends on vertical collision rather than a saved perch coordinate.</summary>\nint A => 0; }",
            "('rather than a saved perch')");
        // The segment template and the original's drawing history.
        Expect("enum P { /// <summary>Its animations: one segment per animation a spawn, branch or wake enters, in authored order.</summary>\nIdle }",
            "('a spawn, branch or wake enters')");
        Expect("class P { /// <summary>The closed eye: the game draws it for two updates and then again.</summary>\nint E => 0; }",
            "('the game draws it')");
        // Read across a documentation comment's line breaks, and across a run of line comments.
        Expect("class P { /// <summary>Moves the position in 256ths\n/// of a pixel.</summary>\nvoid Move() { } }", "('in 256ths of a pixel')");
        Expect("class P { void Run() {\n // The ascent then falls\n // directly into the wait.\n } }", "('falls directly into')");
        Expect("class P { void Run() {\n // The ascent ends.\n // The wait starts.\n } }");
        // English stays legal: a 256th of a pixel, the background, a physical fall through a bridge, the game's music, the
        // installed artwork, a cycle of sixteen colours, a placed picture, a turn's steps and the room's foreground layer.
        Expect("class P { /// <summary>A 256th of a pixel; the background shakes; he falls through the bridge; the game's music; the host installs its artwork; the beam cycles through sixteen colours; the turn's steps; the foreground layer.</summary>\nint E => 0; }");
        // Hardware owners describe the encoding or the behaviour they reproduce.
        const string CompatibilityService =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class CompatibilityServiceAttribute(string behaviour) : System.Attribute { public string Behaviour { get; } = behaviour; }\n";
        const string EncodingOwner =
            "[System.AttributeUsage(System.AttributeTargets.All)] sealed class EncodingOwnerAttribute(string format) : System.Attribute { public string Format { get; } = format; }\n";
        Expect(CompatibilityService + "/// <summary>Evidence: the dissipation adds eight to the low byte and falls through into the finish, in 256ths of a pixel.</summary>\n[CompatibilityService(\"line\")] class Line { }");
        Expect(EncodingOwner + "[EncodingOwner(\"fixed point\")] readonly struct F { /// <summary>The value in 256ths of a pixel, 0 through 4095.</summary>\nint Raw => 0; }");
        // Other assemblies, the import pipeline among them, tell the cartridge's storage.
        VerifySnippet("class P { /// <summary>The value in 256ths of a pixel; falls directly into the next step.</summary>\nint F => 0; }",
            analyzer: analyzer, assemblyName: "SuperMetroid.AssetExtraction");
    }
}
