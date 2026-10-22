using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): the composition each installed projectile and grapple sheet frame was composed from, so a draw of
/// that frame can be compared with cartridge OAM oracles as the parts it shows. Frames are found by sheet, rectangle
/// and origin, since a runtime and the fixtures load the sheets separately.
/// </summary>
internal static class SheetFrameSources
{
    /// <summary>Each projectile sheet frame's composition and the page its characters index.</summary>
    private static readonly Lazy<Dictionary<FrameKey, FrameSource>> sources = new(Build);

    /// <summary>The character draw an image draw of a projectile sheet frame shows: its composition over its page, placed and mirrored alike.</summary>
    /// <exception cref="InvalidOperationException">The frame is not a projectile sheet frame.</exception>
    internal static CharacterSpriteDraw CharacterDraw(ImageSpriteDraw draw)
    {
        Ensure.NotNull(draw);
        FrameSource source = sources.Value.TryGetValue(Key(draw.Image), out FrameSource? found) ? found
            : throw new InvalidOperationException($"{draw} shows a frame no projectile sheet was composed with.");
        SnesTileFlipFlags flip = ((draw.Mirror & SpriteMirror.Horizontal) != 0 ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
            ((draw.Mirror & SpriteMirror.Vertical) != 0 ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None);
        return new CharacterSpriteDraw(draw.Source, source.Frame, source.Page, draw.Origin, flip, 0, 0);
    }

    /// <summary>Pairs every installed projectile sheet frame with what it was composed from.</summary>
    private static Dictionary<FrameKey, FrameSource> Build()
    {
        var result = new Dictionary<FrameKey, FrameSource>();
        SamusSheetInputs inputs = SamusSheetInputs.Load(RepositoryInstallation.Installation.ContentDirectory);
        foreach (SpriteSheetId id in SpriteSheetDefinitions.Sheets.Where(id => id != SpriteSheetId.EscapeTimer))
        {
            SpriteSheetSources composed = id == SpriteSheetId.Grapple ? GrappleSheetComposer.Sources(inputs) : ProjectileSheetComposer.Sources(inputs, id);
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            // Every empty frame shows nothing, so the first of those that share a key stands for all of them.
            foreach (SheetFrameSource frame in composed.Frames)
                result.TryAdd(Key(sheet.Frame(frame.Name)), new FrameSource(frame.Frame, frame.Characters));
        }
        return result;
    }

    /// <summary>A frame's identity across separately loaded sheets.</summary>
    private static FrameKey Key(SpriteSheetFrame frame) => new(frame.Sheet.Name, frame.Corner, frame.Size, frame.Origin);

    /// <summary>A sheet frame's identity: its sheet, rectangle and origin.</summary>
    /// <param name="Sheet">The sheet's installed name.</param>
    /// <param name="Corner">The rectangle's top-left pixel.</param>
    /// <param name="Size">The rectangle's size.</param>
    /// <param name="Origin">The frame's origin.</param>
    private readonly record struct FrameKey(string Sheet, Position16<ImagePixels> Corner, Extent16<ImagePixels> Size, Offset16<ImagePixels> Origin);

    /// <summary>What a sheet frame was composed from.</summary>
    /// <param name="Frame">The composition.</param>
    /// <param name="Page">The OBJ page its characters index.</param>
    private sealed record FrameSource(SpriteFrame Frame, SpriteArtwork Page);
}
