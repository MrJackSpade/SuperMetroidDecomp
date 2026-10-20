using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Hardware;

/// <summary>Checks PPU snapshots against native transfer layout and source-array ownership.</summary>
internal static class PpuMemorySnapshotTestsVerification
{
    /// <summary>Confirms VRAM and CGRAM byte order, immutable capture of memories and draws, and size validation.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    internal static void VerifyPpuMemorySnapshotOwnership()
    {
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        var sprites = new SpriteFrameBuffer();
        byte[] characters = new byte[SnesVram.ByteCount];
        new Random(321).NextBytes(characters);
        vram.LoadBytes(0, characters);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            cgram.SetColor(index, Bgr555.FromWord((ushort)(index * 97)));
        var frame = new SpriteFrame("Fixture", [new(new(0, 0), new(8, 8), 0, SnesTileFlipFlags.None, null, null)]);
        var artwork = new SpriteArtwork(new(SpriteArtworkKind.EscapeTimerTiles), new byte[SpriteArtwork.CharacterByteCount]);
        sprites.BeginFrame();
        sprites.Add(new SpriteDraw(new(SpriteSourceKind.EscapeTimer), frame, artwork, new(10, 20), SnesTileFlipFlags.None, 1, 2));
        sprites.FinalizeFrame();
        Bgr555[] expectedPalette = cgram.Colors.ToArray();

        PpuMemorySnapshot first = PpuMemorySnapshotConstants.Capture(vram, cgram, sprites);
        PpuMemorySnapshot repeated = PpuMemorySnapshotConstants.Capture(vram, cgram, sprites);
        AssertTrue(first.Vram.SequenceEqual(characters), "VRAM capture changed byte order");
        AssertTrue(first.Cgram.SequenceEqual(expectedPalette), "CGRAM capture changed native words");
        AssertTrue(first.SpriteDraws.SequenceEqual(sprites.Draws), "capture keeps the frame's draws in emission order");
        AssertTrue(repeated.Vram.SequenceEqual(first.Vram) && repeated.Cgram.SequenceEqual(first.Cgram)
            && repeated.SpriteDraws.SequenceEqual(first.SpriteDraws), "repeat capture changed the image");
        AssertTrue(vram.Bytes.SequenceEqual(characters) && cgram.Colors.SequenceEqual(expectedPalette), "capture mutated its source");

        vram.LoadBytes(0, new byte[SnesVram.ByteCount]);
        cgram.SetColor(1, new Bgr555(0, 0, 0));
        sprites.BeginFrame();
        sprites.FinalizeFrame();
        AssertTrue(first.Vram.SequenceEqual(characters) && first.Cgram.SequenceEqual(expectedPalette)
            && first.SpriteDraws.Length == 1, "later simulation writes changed a published image");

        var fromFixture = new PpuMemorySnapshot(characters, expectedPalette, first.SpriteDraws);
        characters[0] ^= byte.MaxValue;
        // Flip every color bit (the PPU stores fifteen).
        expectedPalette[0] = Bgr555.FromWord((ushort)(expectedPalette[0].ToWord() ^ 0x7fff));
        AssertTrue(fromFixture.Vram.SequenceEqual(first.Vram) && fromFixture.Cgram.SequenceEqual(first.Cgram)
            && fromFixture.SpriteDraws.SequenceEqual(first.SpriteDraws), "fixture arrays escaped into snapshot ownership");

        AssertThrows<ArgumentException>(() => new PpuMemorySnapshot(new byte[1], expectedPalette, []), "short VRAM");
        AssertThrows<ArgumentException>(() => new PpuMemorySnapshot(characters, ToColors(new ushort[1]), []), "short CGRAM");
        AssertThrows<ArgumentException>(() => new PpuMemorySnapshot(characters, expectedPalette, default), "missing draw list");
        Console.WriteLine("PPU memory snapshot: byte parity, draw retention, ownership, observation and size checks passed.");

    }
}
