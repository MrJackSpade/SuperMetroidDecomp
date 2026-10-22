using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1403 display-packet rule, SME6443.</summary>
internal static class DisplayPacketAnalyzerTests
{
    /// <summary>Memory images and register values reachable from the published packet, and the members the rule leaves alone.</summary>
    internal static void VerifyDisplayPacketAnalyzer()
    {
        var analyzer = new DisplayPacketAnalyzer();
        string core = CoreLayerAnalyzer.CoreAssembly;
        void Check(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : DisplayPacketAnalyzer.DisplayPacketId, message, analyzer, assemblyName: core);

        // The published frame carrying a memory image.
        Check("""
            class Memory { private readonly byte[] bytes = []; }
            class RenderFrameSnapshot { public Memory? Image { get; } }
            """, "'bytes' holds raw memory in the published display packet; describe what is shown, not the memory or registers that showed it");
        // A span of 16-bit words is raw memory too.
        Check("""
            class DisplayPacket { public System.ReadOnlyMemory<ushort> Tilemap { get; } }
            """, "'Tilemap' holds raw memory in the published display packet; describe what is shown, not the memory or registers that showed it");
        // A kind of layer is in the packet: its member named for a memory word is.
        Check("""
            using System.Collections.Immutable;
            abstract record Layer;
            sealed record TileLayer(int TilemapWord) : Layer;
            class DisplayPacket { public ImmutableArray<Layer> Layers { get; } }
            """, "'TilemapWord' is named for a register or memory word in the published display packet; describe what is shown, not the memory or registers that showed it");
        // A register value carried as a number is still a register.
        Check("""
            record struct Mode7Registers(short A);
            class DisplayPacket { public Mode7Registers FloorRegisters { get; } }
            """, "'FloorRegisters' is named for a register or memory word in the published display packet; describe what is shown, not the memory or registers that showed it");

        // A type the packet does not reach may hold memory: a simulation buffer or an extraction catalog.
        Check("""
            class Buffer { public byte[] Bytes { get; } = []; }
            class DisplayPacket { public int Layers { get; } }
            """);
        // Words that merely share letters with a hardware word name what is shown.
        Check("""
            class DisplayPacket { public string Wordmark { get; } = ""; public int Memoir { get; } public int Oamish { get; } }
            """);
        // Placed images and colours describe what is shown.
        Check("""
            record struct Placement(int X, int Y);
            class DisplayPacket { public Placement Origin { get; } public System.Collections.Immutable.ImmutableArray<int> Colours { get; } }
            """);
        // Outside Core the import pipeline reads memory.
        VerifySnippet("""
            class DisplayPacket { public byte[] Vram { get; } = []; }
            """, analyzer: analyzer, assemblyName: "SuperMetroid.AssetExtraction");
    }
}
