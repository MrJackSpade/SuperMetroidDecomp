using SuperMetroid.EnsureAnalyzer.Contracts;
using SuperMetroid.EnsureAnalyzer.Modules;
using static SuperMetroid.EnsureVerification.Harness.AnalyzerTests;

namespace SuperMetroid.EnsureVerification.Contracts;

/// <summary>Checks the #1406 transfer-word rule, SME6445.</summary>
internal static class TransferWordAnalyzerTests
{
    /// <summary>One positive per transfer word, the member kinds the rule reads, and the members it leaves alone.</summary>
    internal static void VerifyTransferWordAnalyzer()
    {
        var analyzer = new TransferWordAnalyzer();
        string core = CoreLayerAnalyzer.CoreAssembly;
        void Check(string source, string? message = null) =>
            VerifySnippet(source, message is null ? null : TransferWordAnalyzer.TransferWordId, message, analyzer, assemblyName: core);
        const string advice = " as an integer; select a frame of a sheet or a part image and publish it through the display model instead";

        // Each word, on an integral field, property and record component.
        Check("""
            class Melt { private readonly ushort transferSize = 0; }
            """, "'transferSize' holds a tile transfer" + advice);
        Check("""
            class Corpse { public int UploadOffset { get; } }
            """, "'UploadOffset' holds a tile upload" + advice);
        Check("""
            readonly record struct TileCopy(int Length, ushort DestinationWord);
            """, "'DestinationWord' holds a tile destination" + advice);
        // A constant, a plural and a sequence of words.
        Check("""
            class Escape { private const ushort TimerDestination = 0x7e00; }
            """, "'TimerDestination' holds a tile destination" + advice);
        Check("""
            class Treadmill { public System.ReadOnlySpan<ushort> PendingUploads => default; }
            """, "'PendingUploads' holds a tile upload" + advice);
        Check("""
            class Gunship { private readonly byte[] liftoffTransfers = []; }
            """, "'liftoffTransfers' holds a tile transfer" + advice);
        // A method returning a destination word, and an acronym before the word.
        Check("""
            class Sand { internal static ushort SandDestination(int line) => (ushort)line; }
            """, "'SandDestination' holds a tile destination" + advice);
        Check("""
            class Ridley { private uint DMATransferLength; }
            """, "'DMATransferLength' holds a tile transfer" + advice);

        // A frame identity is the selection that replaces the transfer.
        Check("""
            enum CorpseFrame { Whole, Rotted }
            readonly record struct CorpseRot(CorpseFrame UploadedFrame);
            class Corpse { public CorpseFrame TransferFrame { get; } }
            """);
        // A movement destination held as a typed place is the game's own.
        Check("""
            enum Perch { Left, Right }
            class Kraid { private readonly Perch destination = Perch.Left; }
            """);
        // Words that merely contain a transfer word.
        Check("""
            class Ending { private readonly int transferenceLevel = 0; public int UploaderCount { get; } public ushort Destinationless => 0; }
            """);
        // Parameters and locals are not members.
        Check("""
            class Saves { public void Copy(int sourceSlot, int destinationSlot) { int transferred = sourceSlot + destinationSlot; _ = transferred; } }
            """);
        // Outside Core the import pipeline reads the cartridge's transfers.
        VerifySnippet("""
            class Importer { private readonly ushort transferDestination = 0; }
            """, analyzer: analyzer, assemblyName: "SuperMetroid.AssetExtraction");
    }
}
