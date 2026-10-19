namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The background character artwork a frame shows (#1350): room tilesets, library backgrounds, collectibles, animated
/// tiles and the HUD's characters, bound by identity instead of written into VRAM. Capture composes them over the VRAM
/// image, and a binding shows only over words VRAM has not written since it was made, so writes not yet converted to
/// bindings (the boss tickets #1352-#1357) keep their order with it.
/// </summary>
[PpuLayoutBoundary("backend: composes background artwork bindings over the VRAM image in write order")]
public sealed class BgArtworkBindings(SnesVram vram)
{
    /// <summary>The VRAM whose write clock orders bindings against its writes; it tracks write order from now on.</summary>
    private readonly SnesVram vram = Tracked(vram);

    /// <summary>The live bindings, oldest first.</summary>
    private readonly List<BgArtworkBinding> bindings = [];

    /// <summary>One owned copy of each artwork bound, by identity: a tileset or animation frame bound again reuses it.</summary>
    [NonSerialized] private Dictionary<BgArtworkIdentity, byte[]>? owned;

    /// <summary>Binds <paramref name="bytes"/> of <paramref name="identity"/> from VRAM word <paramref name="firstWord"/> onward.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The bytes are not whole words inside VRAM.</exception>
    public void Bind(BgArtworkIdentity identity, int firstWord, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty || (bytes.Length & 1) != 0 || firstWord < 0 || firstWord + bytes.Length / sizeof(ushort) > SnesVram.WordCount)
            throw new ArgumentOutOfRangeException(nameof(firstWord), firstWord, $"{bytes.Length} bytes are not whole words inside VRAM there.");
        owned ??= [];
        if (!owned.TryGetValue(identity, out byte[]? copy) || !bytes.SequenceEqual(copy))
            owned[identity] = copy = bytes.ToArray();
        var binding = new BgArtworkBinding(firstWord, copy, vram.NextWriteStamp());
        // A binding a newer one covers entirely can no longer show.
        bindings.RemoveAll(older => older.FirstWord >= binding.FirstWord &&
            older.FirstWord + older.WordCount <= binding.FirstWord + binding.WordCount);
        bindings.Add(binding);
    }

    /// <summary>Starts <paramref name="vram"/> tracking write order, which bindings are ordered against.</summary>
    private static SnesVram Tracked(SnesVram vram)
    {
        Ensure.NotNull(vram).TrackWriteOrder();
        return vram;
    }

    /// <summary>Writes each binding's bytes over <paramref name="image"/>, a copy of the VRAM, where VRAM has not written since.</summary>
    internal void ComposeOver(Span<byte> image)
    {
        Ensure.LengthEqual(image, SnesVram.ByteCount);
        foreach (BgArtworkBinding binding in bindings)
        {
            ReadOnlySpan<byte> bytes = binding.Bytes.Span;
            for (int word = 0; word < binding.WordCount; word++)
            {
                int address = binding.FirstWord + word;
                if (vram.WriteStampAt(address) > binding.Stamp)
                    continue;
                image[address * 2] = bytes[word * 2];
                image[address * 2 + 1] = bytes[word * 2 + 1];
            }
        }
    }
}
