using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// A graphics set's OBJ characters as its room load leaves them, and as each upload its encounters make after the load
/// leaves them (#1392). A frame drawn over characters an upload covers gets a picture for each upload each region it is
/// drawn over can show, and one for the set's own characters there. A corpse's frame drawn over the rows its kind uploads
/// gets a picture of each rot the kind can show (#1406); a frame drawn over Mother Brain's corpse rows gets a picture for
/// each rot step those rows can show together.
/// </summary>
internal sealed class SetCharacters
{
    /// <summary>The OBJ character table as the room load leaves it.</summary>
    private readonly byte[] table;

    /// <summary>The regions that change after the room loads, and what each can show.</summary>
    private readonly SetRegions regions;

    /// <summary>Resolves the artwork the uploads show.</summary>
    private readonly IObjCharacterSourceResolver resolver;

    /// <summary>The corpses of a kind that rot in the set's rooms, whose frames select the rot they show.</summary>
    private readonly IReadOnlyList<CorpseRotSimulation> corpses;

    /// <summary>Each picture's characters, by its name suffix.</summary>
    private readonly Dictionary<string, SpriteArtwork> pages = new(StringComparer.Ordinal);

    /// <summary>
    /// The table as the room load leaves it, the regions that change after, the artwork the uploads show, and the corpses of a
    /// kind that rot in the set's rooms.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A corpse is not of a kind, or uploads a row over characters an encounter or Mother Brain's corpse uploads.
    /// </exception>
    internal SetCharacters(byte[] table, SetRegions regions, IObjCharacterSourceResolver resolver, IReadOnlyList<CorpseRotSimulation> corpses)
    {
        this.table = Ensure.NotNull(table);
        this.regions = Ensure.NotNull(regions);
        this.resolver = Ensure.NotNull(resolver);
        this.corpses = Ensure.NotNull(corpses);
        foreach (CorpseRotSimulation corpse in corpses)
        {
            if (corpse.Kind is null)
                throw new InvalidDataException($"{corpse.Flesh} is pictured by its uploaded rows, not by a kind's rot.");
            if (corpse.Rows.Any(row => regions.Regions.Any(region => Overlaps(row, region))))
                throw new InvalidDataException($"{corpse.Flesh} uploads a row over characters an encounter or Mother Brain's corpse uploads.");
        }
        Loaded = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), table);
    }

    /// <summary>The characters as the room load leaves them.</summary>
    internal SpriteArtwork Loaded { get; }

    /// <summary>The regions the uploads and the corpses' rows divide the characters into.</summary>
    internal IReadOnlyList<CharacterRange> Regions => regions.Regions;

    /// <summary>The set's regions, which name them by ordinal and know what covers each.</summary>
    internal SetRegions RegionSet => regions;

    /// <summary>
    /// The pictures of <paramref name="frame"/>, named for <paramref name="name"/>: the frame whole when it is drawn over no
    /// uploaded region, else its layers, each run of its parts in draw order reading the same uploaded regions, with a
    /// picture of each layer for each upload or the set's own characters in each region it reads. A layer reads few
    /// regions, so its pictures stay few where the whole frame's combinations would not. Only a corpse's own frames show its
    /// rot: another frame drawn over a corpse's rows has a picture of them as the set or an encounter leaves them only, so a
    /// draw over rotted rows finds no picture and fails rather than showing what no frame of the room draws.
    /// </summary>
    /// <param name="name">The frame's name, which names its pictures.</param>
    /// <param name="frame">The frame.</param>
    /// <param name="showsRot">Whether the frame is a corpse's own, which shows the rot of the corpses' rows it reads.</param>
    /// <exception cref="InvalidDataException">A corpse's frame reads the rows of two kinds of corpse.</exception>
    internal FramePictures Pictures(string name, SpriteFrame frame, bool showsRot = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.NotNull(frame);
        if (showsRot)
        {
            CorpseRotSimulation[] rotting = [.. corpses.Where(corpse => frame.Parts.Any(part =>
                corpse.Rows.Any(row => DrawsOver(part, new CharacterRange(row.FirstByte, row.ByteCount)))))];
            if (rotting.Length > 1)
                throw new InvalidDataException($"Frame {name} reads the rows of {string.Join(" and ", rotting.Select(corpse => corpse.Flesh))}.");
            if (rotting.Length == 1)
                return RotPictures(name, frame, rotting[0]);
        }
        CharacterRange[][] read = [.. frame.Parts.Select(part => Regions.Where(region => DrawsOver(part, region)).ToArray())];
        if (read.All(regions => regions.Length == 0))
            return new FramePictures(name, [new SheetFrameSource(name, frame, Loaded)], [], null);
        var sources = new List<SheetFrameSource>();
        var layers = new List<CharacterRange[]>();
        int first = 0;
        while (first < read.Length)
        {
            int end = first + 1;
            while (end < read.Length && read[end].SequenceEqual(read[first]))
                end++;
            int layer = layers.Count;
            CharacterRange[] layerRegions = read[first];
            var parts = new SpriteFrame($"{frame.Name}#{layer}", frame.Parts[first..end]);
            sources.AddRange(regions.Contents(layerRegions, showsRot).Select(contents => new SheetFrameSource(
                SuperMetroid.AssetExtraction.Enemies.RuntimeCharacterUploadPlacements.Catalog.PictureName(name, layer, contents), parts, Page(contents))));
            layers.Add(layerRegions);
            first = end;
        }
        return new FramePictures(name, sources, [.. layers], null);
    }

    /// <summary>
    /// The pictures of a frame drawn over the rows of <paramref name="corpse"/> (#1406): the frame whole over the set's
    /// characters, which it shows before the corpse's kind first rots, then over each rot the kind can show.
    /// </summary>
    private FramePictures RotPictures(string name, SpriteFrame frame, CorpseRotSimulation corpse) =>
        new(name, [new SheetFrameSource(name, frame, Loaded), .. corpse.States.Select(state =>
            new SheetFrameSource(state.View.PictureName(name), frame, RotPage(corpse, state)))], [], corpse.Flesh);

    /// <summary>The characters with each of <paramref name="corpse"/>'s rows showing the step <paramref name="state"/> gives it.</summary>
    private SpriteArtwork RotPage(CorpseRotSimulation corpse, CorpseRotState state)
    {
        string key = state.View.PictureName(corpse.Flesh);
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        byte[] shown = [.. table];
        foreach ((CorpseRotRow row, int? step) in state.Steps)
        {
            if (step is int taken)
                corpse.RowBytes(row, taken).CopyTo(shown.AsSpan(row.FirstByte));
        }
        page = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), shown);
        pages.Add(key, page);
        return page;
    }

    /// <summary>Whether <paramref name="row"/> shares a byte with <paramref name="region"/>.</summary>
    private static bool Overlaps(CorpseRotRow row, CharacterRange region) =>
        row.FirstByte < region.FirstByte + region.ByteCount && region.FirstByte < row.FirstByte + row.ByteCount;

    /// <summary>Whether <paramref name="part"/> reads a character in <paramref name="region"/>.</summary>
    private static bool DrawsOver(SpriteFramePart part, CharacterRange region)
    {
        for (int row = 0; row < part.Size.Y / SpriteArtwork.CharacterPixels; row++)
        {
            for (int column = 0; column < part.Size.X / SpriteArtwork.CharacterPixels; column++)
            {
                // A character past the table reads the page's blank overhang.
                int firstByte = (part.Character + row * SpriteArtwork.CharactersPerRow + column) * SpriteArtwork.CharacterByteCount;
                if (firstByte < ObjCharacterUploads.TableByteCount && firstByte < region.FirstByte + region.ByteCount
                    && region.FirstByte < firstByte + SpriteArtwork.CharacterByteCount)
                    return true;
            }
        }
        return false;
    }

    /// <summary>The characters with each upload <paramref name="contents"/> names shown over its region.</summary>
    private SpriteArtwork Page(IReadOnlyList<CharacterRegionContent> contents)
    {
        string key = SuperMetroid.AssetExtraction.Enemies.RuntimeCharacterUploadPlacements.Catalog.PictureName("page", 0, contents);
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        byte[] shown = [.. table];
        foreach (CharacterRegionContent content in contents)
        {
            CharacterRange range = regions.RangeOf(content.Region);
            if (content.RotStep is not null)
            {
                regions.RotBytes(content).CopyTo(shown.AsSpan(range.FirstByte));
                continue;
            }
            if (content.Upload is not { } upload)
                continue;
            ReadOnlySpan<byte> bytes = upload.Source.Resolve(resolver).Span;
            int offset = range.FirstByte - upload.FirstByte;
            if (bytes.Length < offset + range.ByteCount)
                throw new InvalidDataException($"{upload.Source} holds {bytes.Length} bytes; its upload shows {upload.ByteCount}.");
            bytes.Slice(offset, range.ByteCount).CopyTo(shown.AsSpan(range.FirstByte));
        }
        page = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), shown);
        pages.Add(key, page);
        return page;
    }
}

/// <summary>The pictures of one frame (#1392).</summary>
/// <param name="Name">The frame's name, which names its pictures.</param>
/// <param name="Sources">Each picture's composition.</param>
/// <param name="Layers">The frame's layers, front first, each with the uploaded regions it reads; none when it is drawn whole.</param>
/// <param name="RotCorpse">The kind of corpse whose rot the frame shows, which names its pictures (#1406); none when it shows no rot.</param>
internal sealed record FramePictures(string Name, IReadOnlyList<SheetFrameSource> Sources, CharacterRange[][] Layers, string? RotCorpse);
