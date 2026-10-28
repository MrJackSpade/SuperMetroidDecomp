using SuperMetroid.Core.Assets;
using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Core.Hardware;

/// <summary>Background artwork bindings for tests that own a VRAM image (#1350, #1406).</summary>
internal static class BgArtworkFixtures
{
    /// <summary>One bindings object per test VRAM, so bindings made through it accumulate like a runtime's.</summary>
    private static readonly ConditionalWeakTable<SnesVram, BgArtworkBindings> Bindings = new();

    /// <summary>The bindings composed over <paramref name="vram"/>.</summary>
    internal static BgArtworkBindings For(SnesVram vram) => Bindings.GetValue(vram, _ => new BgArtworkBindings());

    /// <summary>The image a capture shows: <paramref name="vram"/> with its bindings composed over it.</summary>
    internal static byte[] Composed(SnesVram vram) => Composed(vram, For(vram));

    /// <summary>A VRAM holding the image a capture shows of <paramref name="vram"/>, for renderers that read a VRAM.</summary>
    internal static SnesVram ComposedVram(SnesVram vram)
    {
        var composed = new SnesVram();
        composed.LoadBytes(0, Composed(vram));
        return composed;
    }

    /// <summary>The image a capture shows: <paramref name="vram"/> with <paramref name="bindings"/> composed over it.</summary>
    internal static byte[] Composed(SnesVram vram, BgArtworkBindings bindings)
    {
        byte[] image = vram.Bytes.ToArray();
        bindings.ComposeOver(image);
        return image;
    }

    /// <summary>Composes bindings into the native VRAM image the cartridge would have built.</summary>
    extension(BgArtworkBindings self)
    {
        /// <summary>
        /// Writes each binding's bytes over <paramref name="image"/>, a VRAM image, from the word the cartridge keeps its
        /// set's first bound character at, oldest first.
        /// </summary>
        [PpuLayoutBoundary("exact hardware test: composes bound background characters at their native VRAM words")]
        internal void ComposeOver(Span<byte> image)
        {
            Ensure.LengthEqual(image, SnesVram.ByteCount);
            foreach (BgArtworkBinding binding in self.Bound)
                binding.Bytes.Span.CopyTo(image[(BgCharacterWordsConstants.WordOf(binding.Set, binding.FirstCharacter) * sizeof(ushort))..]);
        }
    }
}
