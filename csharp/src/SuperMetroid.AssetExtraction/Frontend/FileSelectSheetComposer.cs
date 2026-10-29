using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the file-select sheet (#1392): each page border, cursor frame and helmet frame over the menu sprite page in
/// the menu object palette, and each file-copy arrow in its own palette, coloured as the screen loads the menu palette.
/// The arrows' cycling colours recolour them through the sheet's palette map.
/// </summary>
internal static class FileSelectSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the file-select sheet composes.</summary>
    internal static SpriteSheetSources Sources(AreaMapPresentation maps) =>
        Sources(Ensure.NotNull(maps).FileSelect, maps);

    /// <summary>What the file-select sheet composes from <paramref name="fileSelect"/>, over the menus of <paramref name="maps"/>.</summary>
    internal static SpriteSheetSources Sources(FileSelectPresentation fileSelect, AreaMapPresentation maps)
    {
        Ensure.NotNull(fileSelect);
        Ensure.NotNull(maps);
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
        FileSelectSourcePresentation source = FileSelectSourcePresentation.Of(fileSelect);
        int palette = fileSelect.ObjectPalette;
        var frames = new List<SheetFrameSource>();
        foreach (FileSelectBorder border in FileSelectPresentationDefinitions.Borders)
        {
            string name = FileSelectParts.BorderSpriteName(border);
            frames.Add(new(name, source.borderSprites[border].ToFrame(name, 0), page, palette));
        }
        for (int frame = 0; frame < FileSelectPresentationDefinitions.CursorFrameCount; frame++)
        {
            string name = FileSelectParts.CursorSpriteName(frame);
            frames.Add(new(name, source.cursorSprites[frame].ToFrame(name, 0), page, palette));
        }
        for (int frame = 0; frame < FileSelectPresentationDefinitions.HelmetFrameCount; frame++)
        {
            string name = FileSelectParts.HelmetSpriteName(frame);
            frames.Add(new(name, source.helmetSprites[frame].ToFrame(name, 0), page, palette));
        }
        for (int shape = 0; shape < FileCopyArrowDefinitions.ShapeCount; shape++)
        {
            string name = FileCopyArrowDefinitions.FrameName(shape);
            // The arrows' parts are placed with their own attribute words, palette and all.
            frames.Add(new(name, new SpriteFrame(name, [.. FileCopyArrowParts.Parts(shape)
                .SelectToArray(part => SpritePartFrames.Part(part.Part, firstCharacter: 0))]), page));
        }
        var cgram = new SnesCgram();
        new CgramPalettes(cgram).LoadImage(maps.Palettes.FileSelect);
        return new SpriteSheetSources(frames, cgram.Colors[SpriteColours..].ToArray(), ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true);
    }
}
