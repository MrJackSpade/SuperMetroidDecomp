using SuperMetroid.Core.Assets;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Background artwork bindings for tests that own a VRAM image (#1350, #1406, #1422). Bindings name their artwork and
/// count its characters; the image a capture shows composes the bytes the import pipeline's resolver reads for each name
/// from the artwork the test installed, and the bytes a test VRAM queue resolved as it drained.
/// </summary>
internal static class BgArtworkFixtures
{
    /// <summary>One bindings object per test VRAM, so bindings made through it accumulate like a runtime's.</summary>
    private static readonly ConditionalWeakTable<SnesVram, BgArtworkBindings> Bindings = new();

    /// <summary>The bytes a test VRAM queue resolved for each artwork it bound, per bindings object.</summary>
    private static readonly ConditionalWeakTable<BgArtworkBindings, Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>>> Drained = new();

    /// <summary>The bindings composed over <paramref name="vram"/>.</summary>
    internal static BgArtworkBindings For(SnesVram vram) => Bindings.GetValue(vram, _ => new BgArtworkBindings());

    /// <summary>
    /// Records the bytes a test VRAM queue resolved for <paramref name="identity"/> as it bound them into
    /// <paramref name="bindings"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">The queue resolved the artwork to other bytes before.</exception>
    internal static void RecordDrained(BgArtworkBindings bindings, BgArtworkIdentity identity, ReadOnlyMemory<byte> bytes)
    {
        Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>> drained = Drained.GetValue(Ensure.NotNull(bindings), _ => []);
        if (drained.TryGetValue(identity, out ReadOnlyMemory<byte> known) && !known.Span.SequenceEqual(bytes.Span))
            throw new InvalidDataException($"A test VRAM queue resolved {identity.Name} to two different byte runs.");
        drained[identity] = bytes.ToArray();
    }

    /// <summary>
    /// A VRAM holding the image a capture shows of <paramref name="vram"/>, for renderers that read a VRAM: its bindings'
    /// artwork read through <paramref name="artwork"/>.
    /// </summary>
    internal static SnesVram ComposedVram(SnesVram vram, BgArtworkBytes artwork)
    {
        var composed = new SnesVram();
        composed.LoadBytes(0, Composed(vram, For(vram), artwork));
        return composed;
    }

    /// <summary>The image a capture shows of <paramref name="vram"/>: its bindings' artwork read through <paramref name="artwork"/>.</summary>
    internal static byte[] Composed(SnesVram vram, BgArtworkBytes artwork) => Composed(vram, For(vram), artwork);

    /// <summary>The image a capture shows: <paramref name="vram"/> with <paramref name="bindings"/> composed over it.</summary>
    internal static byte[] Composed(SnesVram vram, BgArtworkBindings bindings, BgArtworkBytes artwork)
    {
        byte[] image = vram.Bytes.ToArray();
        bindings.ComposeOver(image, artwork);
        return image;
    }

    /// <summary>Composes bindings into the native VRAM image the cartridge would have built.</summary>
    extension(BgArtworkBindings self)
    {
        /// <summary>
        /// Writes each binding's artwork over <paramref name="image"/>, a VRAM image, from the word the cartridge keeps its
        /// set's first bound character at, oldest first. Artwork a test VRAM queue drained composes the bytes it resolved;
        /// other artwork composes what <paramref name="artwork"/> reads for its name.
        /// </summary>
        /// <exception cref="InvalidDataException">
        /// Neither names a binding's artwork, or both name it with different bytes.
        /// </exception>
        internal void ComposeOver(Span<byte> image, BgArtworkBytes artwork)
        {
            Ensure.LengthEqual(image, SnesVram.ByteCount);
            Ensure.NotNull(artwork);
            Drained.TryGetValue(self, out Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>>? drained);
            foreach (BgArtworkBinding binding in self.Bound)
                BytesOf(binding, drained, artwork).Span.CopyTo(image[(BgCharacterWordsConstants.WordOf(binding.Set, binding.FirstCharacter) * sizeof(ushort))..]);
        }
    }

    /// <summary>The bytes <paramref name="binding"/> shows, from what the queue drained or the installed artwork.</summary>
    private static ReadOnlyMemory<byte> BytesOf(BgArtworkBinding binding, Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>>? drained,
        BgArtworkBytes artwork)
    {
        if (drained is null || !drained.TryGetValue(binding.Identity, out ReadOnlyMemory<byte> queued))
            return artwork.Of(binding);
        ReadOnlyMemory<byte> shown = BgArtworkBytes.Shown(binding, queued);
        if (artwork.Names(binding.Identity) && !artwork.Of(binding).Span.SequenceEqual(shown.Span))
            throw new InvalidDataException($"{binding.Identity.Name} drained from the test VRAM queue differs from the installed artwork.");
        return shown;
    }
}
