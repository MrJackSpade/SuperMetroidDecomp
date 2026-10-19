using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>Background artwork bindings for tests that own a VRAM image (#1350).</summary>
internal static class BgArtworkFixtures
{
    /// <summary>One bindings object per test VRAM, so bindings made through it accumulate like a runtime's.</summary>
    private static readonly ConditionalWeakTable<SnesVram, BgArtworkBindings> Bindings = new();

    /// <summary>The bindings composed over <paramref name="vram"/>.</summary>
    internal static BgArtworkBindings For(SnesVram vram) => Bindings.GetValue(vram, image => new BgArtworkBindings(image));

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
}
