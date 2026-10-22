using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// A graphics set's OBJ characters as its room load leaves them, and as each upload its encounters make after the load
/// leaves them (#1392). A frame drawn over characters an upload covers gets a picture for each upload each region it is
/// drawn over can show, and one for the set's own characters there; a frame drawn over the rows a rotting corpse uploads gets
/// a picture for each rot step those rows can show together.
/// </summary>
internal sealed class SetCharacters
{
    /// <summary>The OBJ character table as the room load leaves it.</summary>
    private readonly byte[] table;

    /// <summary>The regions that change after the room loads, and what each can show.</summary>
    private readonly SetRegions regions;

    /// <summary>Resolves the artwork the uploads show.</summary>
    private readonly IObjCharacterSourceResolver resolver;

    /// <summary>Each picture's characters, by its name suffix.</summary>
    private readonly Dictionary<string, SpriteArtwork> pages = new(StringComparer.Ordinal);

    /// <summary>The table as the room load leaves it, the regions that change after, and the artwork the uploads show.</summary>
    internal SetCharacters(byte[] table, SetRegions regions, IObjCharacterSourceResolver resolver)
    {
        this.table = Ensure.NotNull(table);
        this.regions = Ensure.NotNull(regions);
        this.resolver = Ensure.NotNull(resolver);
        Loaded = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), table);
    }

    /// <summary>The characters as the room load leaves them.</summary>
    internal SpriteArtwork Loaded { get; }

    /// <summary>The regions the uploads and the corpses' rows divide the characters into.</summary>
    internal IReadOnlyList<CharacterRegion> Regions => regions.Regions;

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
    internal FramePictures Pictures(string name, SpriteFrame frame, bool showsRot = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.NotNull(frame);
        CharacterRegion[][] read = [.. frame.Parts.Select(part => Regions.Where(region => DrawsOver(part, region)).ToArray())];
        if (read.All(regions => regions.Length == 0))
            return new FramePictures(name, [new SheetFrameSource(name, frame, Loaded)], []);
        var sources = new List<SheetFrameSource>();
        var layers = new List<CharacterRegion[]>();
        int first = 0;
        while (first < read.Length)
        {
            int end = first + 1;
            while (end < read.Length && read[end].SequenceEqual(read[first]))
                end++;
            int layer = layers.Count;
            CharacterRegion[] layerRegions = read[first];
            var parts = new SpriteFrame($"{frame.Name}#{layer}", frame.Parts[first..end]);
            sources.AddRange(regions.Contents(layerRegions, showsRot).Select(contents => new SheetFrameSource(
                RuntimeCharacterUploadDefinitions.Catalog.PictureName(name, layer, contents), parts, Page(contents))));
            layers.Add(layerRegions);
            first = end;
        }
        return new FramePictures(name, sources, [.. layers]);
    }

    /// <summary>Whether <paramref name="part"/> reads a character in <paramref name="region"/>.</summary>
    private static bool DrawsOver(SpriteFramePart part, CharacterRegion region)
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
        string key = RuntimeCharacterUploadDefinitions.Catalog.PictureName("page", 0, contents);
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        byte[] shown = [.. table];
        foreach (CharacterRegionContent content in contents)
        {
            if (content.RotStep is not null)
            {
                regions.RotBytes(content).CopyTo(shown.AsSpan(content.Region.FirstByte));
                continue;
            }
            if (content.Upload is not { } upload)
                continue;
            ReadOnlySpan<byte> bytes = upload.Source.Resolve(resolver).Span;
            int offset = content.Region.FirstByte - upload.FirstByte;
            if (bytes.Length < offset + content.Region.ByteCount)
                throw new InvalidDataException($"{upload.Source} holds {bytes.Length} bytes; its upload shows {upload.ByteCount}.");
            bytes.Slice(offset, content.Region.ByteCount).CopyTo(shown.AsSpan(content.Region.FirstByte));
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
internal sealed record FramePictures(string Name, IReadOnlyList<SheetFrameSource> Sources, CharacterRegion[][] Layers);
