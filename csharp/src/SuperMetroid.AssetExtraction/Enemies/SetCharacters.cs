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
/// drawn over can show, and one for the set's own characters there.
/// </summary>
internal sealed class SetCharacters
{
    /// <summary>The OBJ character table as the room load leaves it.</summary>
    private readonly byte[] table;

    /// <summary>The uploads the set's encounters make.</summary>
    private readonly IReadOnlyList<RuntimeCharacterUpload> uploads;

    /// <summary>Resolves the artwork the uploads show.</summary>
    private readonly IObjCharacterSourceResolver resolver;

    /// <summary>Each picture's characters, by its name suffix.</summary>
    private readonly Dictionary<string, SpriteArtwork> pages = new(StringComparer.Ordinal);

    /// <summary>The table as the room load leaves it, the uploads the set's encounters make and the artwork they show.</summary>
    internal SetCharacters(byte[] table, IReadOnlyList<RuntimeCharacterUpload> uploads, IObjCharacterSourceResolver resolver)
    {
        this.table = Ensure.NotNull(table);
        this.uploads = Ensure.NotNull(uploads);
        this.resolver = Ensure.NotNull(resolver);
        Regions = RuntimeCharacterUploadCatalog.Regions(uploads);
        Loaded = ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.GameplayObjects), table);
    }

    /// <summary>The characters as the room load leaves them.</summary>
    internal SpriteArtwork Loaded { get; }

    /// <summary>The regions the uploads divide the characters into.</summary>
    internal IReadOnlyList<CharacterRegion> Regions { get; }

    /// <summary>
    /// The pictures of <paramref name="frame"/>, named for <paramref name="name"/>: the frame alone when it is drawn over
    /// no uploaded region, else one for each upload or the set's own characters in each region it is drawn over.
    /// </summary>
    internal FramePictures Pictures(string name, SpriteFrame frame)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        Ensure.NotNull(frame);
        CharacterRegion[] drawnOver = [.. Regions.Where(region => DrawsOver(frame, region))];
        if (drawnOver.Length == 0)
            return new FramePictures(name, [new SheetFrameSource(name, frame, Loaded)], []);
        return new FramePictures(name, [.. RuntimeCharacterUploadCatalog.Contents(drawnOver, uploads).Select(contents => new SheetFrameSource(
            RuntimeCharacterUploadDefinitions.Catalog.PictureName(name, contents), frame, Page(contents)))], drawnOver);
    }

    /// <summary>Whether a part of <paramref name="frame"/> reads a character in <paramref name="region"/>.</summary>
    private static bool DrawsOver(SpriteFrame frame, CharacterRegion region)
    {
        foreach (SpriteFramePart part in frame.Parts)
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
        }
        return false;
    }

    /// <summary>The characters with each upload <paramref name="contents"/> names shown over its region.</summary>
    private SpriteArtwork Page(IReadOnlyList<CharacterRegionContent> contents)
    {
        string key = RuntimeCharacterUploadDefinitions.Catalog.PictureName("page", contents);
        if (pages.TryGetValue(key, out SpriteArtwork? page))
            return page;
        byte[] shown = [.. table];
        foreach (CharacterRegionContent content in contents)
        {
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
/// <param name="Regions">The uploaded regions the frame is drawn over; none when its one picture is the frame alone.</param>
internal sealed record FramePictures(string Name, IReadOnlyList<SheetFrameSource> Sources, CharacterRegion[] Regions);
