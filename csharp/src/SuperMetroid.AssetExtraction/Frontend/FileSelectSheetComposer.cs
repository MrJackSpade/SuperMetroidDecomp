using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the file-select sheet (#1392): each page border, cursor frame and helmet frame over the menu sprite page in
/// the menu object palette, and each file-copy arrow in its own palette, coloured as the screen loads the menu palette,
/// with a shade for each step of the arrows' colour rotation (#1512).
/// </summary>
internal static class FileSelectSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What the file-select sheet composes.</summary>
    internal static SpriteSheetSources Sources(ImportAreaMapPresentation maps) =>
        Sources(Ensure.NotNull(maps).FileSelect, maps);

    /// <summary>What the file-select sheet composes from <paramref name="fileSelect"/>, over the menus of <paramref name="maps"/>.</summary>
    internal static SpriteSheetSources Sources(FileSelectSourcePresentation fileSelect, ImportAreaMapPresentation maps)
    {
        Ensure.NotNull(fileSelect);
        Ensure.NotNull(maps);
        SpriteArtwork page = MapSpriteArtwork.Of(maps.Sprites).Page;
        FileSelectSourcePresentation source = fileSelect;
        int palette = source.ObjectPalette;
        var frames = new List<SheetFrameSource>();
        foreach (FileSelectBorder border in FileSelectPresentationFormat.Borders)
        {
            string name = FileSelectParts.BorderSpriteName(border);
            frames.Add(new(name, source.borderSprites[border].ToFrame(name, 0), page, palette));
        }
        for (int frame = 0; frame < FileSelectPresentationFormat.CursorFrameCount; frame++)
        {
            string name = FileSelectParts.CursorSpriteName(frame);
            frames.Add(new(name, source.cursorSprites[frame].ToFrame(name, 0), page, palette));
        }
        for (int frame = 0; frame < FileSelectPresentationFormat.HelmetFrameCount; frame++)
        {
            string name = FileSelectParts.HelmetSpriteName(frame);
            frames.Add(new(name, source.helmetSprites[frame].ToFrame(name, 0), page, palette));
        }
        foreach (FileCopyArrowShape shape in FileCopyArrowDefinitions.Shapes)
        {
            string name = FileCopyArrowDefinitions.FrameName(shape);
            // The arrows' parts are placed with their own attribute words, palette and all.
            frames.Add(new(name, new SpriteFrame(name, [.. FileCopyArrowPartDefinitions.Parts(shape)
                .SelectToArray(part => SpritePartFrames.Part(part.Part, firstCharacter: 0))]), page));
        }
        RgbColor[] sprites = maps.Palettes.FileSelect[SpriteColours..].ToArray();
        return new SpriteSheetSources(frames, sprites, ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection), PartsKeepOwnRows: true)
        {
            Shades = [.. CopyArrowShades(sprites)],
        };
    }

    /// <summary>The sprite colour group whose colours the copy arrows' rotation moves.</summary>
    private const int CopyArrowGroup = 1;
    /// <summary>The first colour of <see cref="CopyArrowGroup"/> the rotation moves; it moves the following ones with it.</summary>
    private const int CopyArrowFirstColour = 1;
    /// <summary>The colours of one sprite colour group.</summary>
    private const int GroupColours = 16;

    /// <summary>
    /// The copy arrows' rotation shades (#1512): after each step, every rotated colour shows the next one's colour and the
    /// last shows the first's.
    /// </summary>
    private static List<SheetShade> CopyArrowShades(RgbColor[] sprites)
    {
        int first = CopyArrowGroup * GroupColours + CopyArrowFirstColour;
        ReadOnlySpan<RgbColor> rotated = sprites.AsSpan(first, FileCopyArrowDefinitions.RotationSteps);
        var shades = new List<SheetShade>(FileCopyArrowDefinitions.RotationSteps);
        for (int step = 0; step < FileCopyArrowDefinitions.RotationSteps; step++)
        {
            var colours = new RgbColor[rotated.Length];
            for (int colour = 0; colour < colours.Length; colour++)
                colours[colour] = rotated[(colour + step) % rotated.Length];
            shades.Add(SheetShade.Run(FileCopyArrowShades.Rotation(step), first, colours));
        }
        return shades;
    }
}
