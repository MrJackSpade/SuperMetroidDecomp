using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1407 display-register rule, SME6449.</summary>
internal static class DisplayRegisterAnalyzerTests
{
    /// <summary>Each family of display register in Core, and the scene state that shares letters with one.</summary>
    internal static void VerifyDisplayRegisterAnalyzer()
    {
        var analyzer = new DisplayRegisterAnalyzer();
        string core = CoreLayerAnalyzer.CoreAssembly;
        void Check(string source, string? message = null, string? assembly = null) =>
            VerifySnippet(source, message is null ? null : DisplayRegisterAnalyzer.DisplayRegisterId, message, analyzer, assemblyName: assembly ?? core);

        // Register names in identifiers: a character base, a tilemap base, the screen designations, a window and Mode 7.
        Check("class Display { public byte Bg34Nba { get; set; } }", "'Bg34Nba' names the display register BG34NBA");
        Check("class Display { public ushort Bg1Sc; }", "'Bg1Sc' names the display register BG1SC");
        Check("class Display { public byte MainScreenTm { get; set; } }", "'MainScreenTm' names the display register TM");
        Check("class Windows { public byte W12Sel; }", "'W12Sel' names the display register W12SEL");
        Check("class Floor { public short M7A { get; init; } }", "'M7A' names the display register M7A");
        // Register names in documentation.
        Check("""
            class Blend { /// <summary>The CGADSUB shadow.</summary>
            public byte Mode { get; set; } }
            """, "'Mode' documents the display register CGADSUB");
        // A byte cache indexed by register offset.
        Check("class WindowRegisterCache { private readonly byte[] bytes = new byte[16]; }", "'bytes' is a byte cache indexed by register offset");

        // Scene state that shares letters with a register is not one.
        Check("class Camera { public int Bg1Scroll { get; set; } public int Items; }");
        Check("""
            class Fade { /// <summary>The TMX brightness step, not a register.</summary>
            public int Brightness { get; set; } }
            """);
        // Outside Core, the backend's register encoding is not reported.
        Check("class Display { public byte Bg34Nba { get; set; } }", assembly: "SuperMetroid.Rendering.Direct3D11");
    }
}
