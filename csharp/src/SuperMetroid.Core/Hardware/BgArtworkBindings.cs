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
    /// <summary>The VRAM the bindings compose over, whose write clock orders them against its writes; it tracks write order from now on.</summary>
    internal SnesVram Vram { get; } = Tracked(vram);

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
        var binding = new BgArtworkBinding(identity, firstWord, copy, Vram.NextWriteStamp());
        // A binding a newer one covers entirely can no longer show.
        bindings.RemoveAll(older => older.FirstWord >= binding.FirstWord &&
            older.FirstWord + older.WordCount <= binding.FirstWord + binding.WordCount);
        bindings.Add(binding);
    }

    /// <summary>
    /// What shows over <paramref name="wordCount"/> VRAM words from <paramref name="firstWord"/> (#1392): the newest binding
    /// over them, which must cover them all, and how far into its artwork they begin.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// No binding covers the words, the newest over them covers only some, or VRAM has been written over them since it was
    /// made, so they show bytes no installed artwork names.
    /// </exception>
    public BgArtworkShown Showing(int firstWord, int wordCount) =>
        TryShowing(firstWord, wordCount, out BgArtworkShown shown) ? shown
            : throw new InvalidDataException($"No background artwork is bound over VRAM words ${firstWord:X4}..${firstWord + wordCount - 1:X4}.");

    /// <summary>What shows over the words, as <see cref="Showing"/> finds it; false where no binding covers any of them.</summary>
    /// <exception cref="InvalidDataException">The newest binding over the words covers only some, or VRAM has written over them since.</exception>
    public bool TryShowing(int firstWord, int wordCount, out BgArtworkShown shown)
    {
        int end = firstWord + wordCount;
        for (int index = bindings.Count - 1; index >= 0; index--)
        {
            BgArtworkBinding binding = bindings[index];
            int bindingEnd = binding.FirstWord + binding.WordCount;
            if (binding.FirstWord >= end || bindingEnd <= firstWord)
                continue;
            if (binding.FirstWord > firstWord || bindingEnd < end)
                throw new InvalidDataException(
                    $"{binding.Identity.Name} shows over only some of VRAM words ${firstWord:X4}..${end - 1:X4}.");
            for (int word = firstWord; word < end; word++)
            {
                if (Vram.WriteStampAt(word) > binding.Stamp)
                    throw new InvalidDataException(
                        $"VRAM word ${word:X4} was written over {binding.Identity.Name} outside the background artwork bindings.");
            }
            shown = new BgArtworkShown(binding.Identity, firstWord - binding.FirstWord);
            return true;
        }
        shown = default;
        return false;
    }

    /// <summary>
    /// Whether bound artwork shows over VRAM word <paramref name="word"/> (#1392): the newest binding over it was made since
    /// VRAM last wrote it, so its bytes, not the word VRAM holds, show there.
    /// </summary>
    public bool ShowsOver(int word, out BgArtworkIdentity artwork)
    {
        for (int index = bindings.Count - 1; index >= 0; index--)
        {
            BgArtworkBinding binding = bindings[index];
            if (word < binding.FirstWord || word >= binding.FirstWord + binding.WordCount)
                continue;
            artwork = binding.Identity;
            return binding.Stamp > Vram.WriteStampAt(word);
        }
        artwork = default;
        return false;
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
                if (Vram.WriteStampAt(address) > binding.Stamp)
                    continue;
                image[address * 2] = bytes[word * 2];
                image[address * 2 + 1] = bytes[word * 2 + 1];
            }
        }
    }
}
