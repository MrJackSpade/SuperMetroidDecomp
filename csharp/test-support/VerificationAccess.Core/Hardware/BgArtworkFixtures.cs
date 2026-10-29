using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Room appearances and background artwork bindings for tests that own a VRAM image (#1350, #1406, #1422, #1466).
/// Gameplay shows what it puts over a room's load through a <see cref="RoomAppearance"/>; the image a capture shows
/// composes what the test bound natively (a room's load, a test VRAM queue's drained uploads) and then what the appearance
/// shows, each from the bytes the import pipeline's resolver reads for its name.
/// </summary>
internal static class BgArtworkFixtures
{
    /// <summary>One appearance per test VRAM, so what is shown through it accumulates like a room's.</summary>
    private static readonly ConditionalWeakTable<SnesVram, RoomAppearance> Appearances = new();

    /// <summary>The bindings a test made natively under each appearance: a room's load and drained queue uploads.</summary>
    private static readonly ConditionalWeakTable<RoomAppearance, BgArtworkBindings> Bindings = new();

    /// <summary>The bytes a test VRAM queue resolved for each artwork it bound, per bindings object.</summary>
    private static readonly ConditionalWeakTable<BgArtworkBindings, Dictionary<BgArtworkIdentity, ReadOnlyMemory<byte>>> Drained = new();

    /// <summary>The tileset whose room each appearance's image shows the load of, once a test loaded one.</summary>
    private static readonly ConditionalWeakTable<RoomAppearance, LoadedTileset> Loads = new();

    /// <summary>The appearance of the room composed over <paramref name="vram"/>.</summary>
    internal static RoomAppearance For(SnesVram vram) => Appearances.GetValue(Ensure.NotNull(vram), _ => new RoomAppearance());

    /// <summary>Records that <paramref name="appearance"/>'s image shows the load of a room of <paramref name="tileset"/>.</summary>
    internal static void RecordLoad(RoomAppearance appearance, TilesetDefinition tileset) =>
        Loads.AddOrUpdate(Ensure.NotNull(appearance), new LoadedTileset(tileset));

    /// <summary>The bindings a test made natively under <paramref name="appearance"/>.</summary>
    internal static BgArtworkBindings BindingsOf(RoomAppearance appearance) => Bindings.GetValue(Ensure.NotNull(appearance), _ => new BgArtworkBindings());

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
    /// A VRAM holding the image a capture shows of <paramref name="vram"/>, for renderers that read a VRAM: what was bound
    /// over it and what its room's appearance shows, read through <paramref name="artwork"/>.
    /// </summary>
    [WholeImageFixture("a composed VRAM image")]
    internal static SnesVram ComposedVram(SnesVram vram, BgArtworkBytes artwork)
    {
        var composed = new SnesVram();
        composed.LoadBytes(0, Composed(vram, artwork));
        return composed;
    }

    /// <summary>The image a capture shows of <paramref name="vram"/>: what was bound over it and what its room's appearance shows.</summary>
    [WholeImageFixture("a composed VRAM image")]
    internal static byte[] Composed(SnesVram vram, BgArtworkBytes artwork)
    {
        RoomAppearance appearance = For(vram);
        byte[] image = vram.Bytes.ToArray();
        if (Loads.TryGetValue(appearance, out LoadedTileset? loaded))
            RoomAppearanceBindings.Load(loaded.Tileset, artwork).ComposeOver(image, artwork);
        BindingsOf(appearance).ComposeOver(image, artwork);
        RoomAppearanceBindings.Shown(appearance, artwork).ComposeOver(image, artwork);
        return image;
    }

    /// <summary>The image a capture shows: <paramref name="vram"/> with <paramref name="bindings"/> composed over it.</summary>
    [WholeImageFixture("a composed VRAM image")]
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
            Ensure.LengthEqual<byte>(image, SnesVram.ByteCount);
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

    /// <summary>The tileset whose room's load an image shows.</summary>
    /// <param name="Tileset">The tileset.</param>
    private sealed record LoadedTileset(TilesetDefinition Tileset);
}
