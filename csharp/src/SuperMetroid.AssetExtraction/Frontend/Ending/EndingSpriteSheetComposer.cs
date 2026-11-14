using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Composes the ending's sprite sheets (#1392, #1513): each holds the poses its scene's actors draw from it
/// (<see cref="EndingSceneActors"/>), each family's composed over the characters the scene's transfers give it and coloured
/// as the scene loads its palette; the post-credits sheets also hold the shooting stars. Each frame is pictured once, its
/// palette-inheriting parts in the sprite set of the colouring it is first drawn in, and each colouring's drawn shade shows
/// its colours over the sets its frames are pictured in. The shooting scene's sheet holds the logo the scene ends on,
/// composed from the logo's own characters: no sheet shows a page partly overwritten by the next scene's transfers.
/// </summary>
internal static class EndingSpriteSheetComposer
{
    /// <summary>The visible shooting-star animation frames with a sheet frame of their own: the growing star's steps and the full star.</summary>
    private static IEnumerable<int> StarSheetFrames => Enumerable.Range(1, EndingShootingStarDefinitions.FullStarFrame);

    /// <summary>
    /// What ending sheet <paramref name="sheet"/> composes: the frames its scene's actors draw from it, coloured in the
    /// sprite colours its scene loads, and shaded in every ending shade (#1512), coloured from <paramref name="palettes"/>
    /// and <paramref name="paletteFx"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The scene draws a pose its families do not compose, a frame keeps its own colours in a set the drawn shade of one of
    /// its colourings shows in that colouring's colours, or a shade the ending shows is authored for no listed actors.
    /// </exception>
    internal static SpriteSheetSources Sources(SpriteSheetId sheet, ImportEndingObjectArtwork objects, EndingPaletteSet palettes,
        EndingFontCharacters font, IPaletteFxColorSource paletteFx)
    {
        Ensure.NotNull(objects);
        Ensure.NotNull(palettes);
        EndingPage page = Page(sheet);
        EndingSheetDraws draws = EndingSceneActors.Of(sheet);
        var transferArtwork = new EndingTransferArtwork(objects, font, page.Reward);
        int objectSelection = EndingSheetObjectSelectionDefinitions.Of(sheet);
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(objectSelection);
        SpriteArtwork characters = Characters(page.Transfers, transferArtwork, objectSelection);
        var frames = new List<ColouredFrame<EndingSpriteColouring>>();
        var blank = new List<string>();
        foreach (ICinematicSpriteFrames family in ShownFamilies(sheet, page.Families.Select(objects.SpritesOf), draws))
            AddFamily(frames, blank, family, draws, characters, sizes);
        if (page.Logo)
        {
            SpriteArtwork logo = Characters(LogoTransfers, transferArtwork, objectSelection);
            foreach (ICinematicSpriteFrames family in ShownFamilies(sheet, [objects.SpritesOf(CinematicSpriteFamily.EndingLogo)], draws))
                AddFamily(frames, blank, family, draws, logo, sizes);
        }
        if (draws.Stars)
        {
            int stars = EndingScreenInks.SpriteSet(EndingShootingStarDefinitions.Colouring);
            frames.AddRange(StarSheetFrames.Select(frame =>
            {
                string name = EndingShootingStarFrames.SheetFrameName(frame);
                return new ColouredFrame<EndingSpriteColouring>(new SheetFrameSource(name,
                    new SpriteFrame(name, [EndingShootingStarParts.Appearance(frame).ToFramePart(0, sizes)]), characters, stars),
                    [EndingShootingStarDefinitions.Colouring]);
            }));
        }
        ColouringPictures<EndingSpriteColouring> pictures = ColouringPictures<EndingSpriteColouring>.Of(frames, EndingScreenInks.SpriteSet);
        Dictionary<EndingSpriteColouring, SortedSet<int>> ownSets = OwnSets(frames);
        RgbColor[] sprites = page.SpriteColours(palettes);
        return new SpriteSheetSources([.. frames.Select(frame => frame.Frame)], sprites, sizes, PartsKeepOwnRows: true, Blank: blank)
        {
            Shades = EndingScreenShades.Sprites(EndingScreenShades.Screen(palettes, paletteFx), sprites, pictures,
                colouring => ownSets.TryGetValue(colouring, out SortedSet<int>? sets) ? sets : []),
        };
    }

    /// <summary>
    /// The frames ending sheet <paramref name="sheet"/> holds: the poses its scene's actors draw from it, and the stars where
    /// they cross it. They are named from the families' poses (#1638): the compositions are import data.
    /// </summary>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet)
    {
        EndingPage page = Page(sheet);
        EndingSheetDraws draws = EndingSceneActors.Of(sheet);
        IEnumerable<CinematicSpriteFamily> families = page.Logo ? page.Families.Append(CinematicSpriteFamily.EndingLogo) : page.Families;
        IEnumerable<string> names = families.SelectMany(family => ShownPoses(sheet, family, draws)
            .Where(pose => family != EndingTypedTexts.Family || EndingTypedTexts.Texts.Find(pose) is null)
            .Select(pose => CinematicSpriteSheet.FrameName(family, pose)).Concat(GlyphNames(family)));
        return draws.Stars ? names.Concat(StarSheetFrames.Select(EndingShootingStarFrames.SheetFrameName)) : names;
    }

    /// <summary>
    /// The poses of <paramref name="family"/> the scene of <paramref name="sheet"/> draws, as <paramref name="draws"/>
    /// records them, in pose order.
    /// </summary>
    /// <exception cref="InvalidDataException">The scene draws a pose the family does not compose.</exception>
    private static IEnumerable<CinematicSpritePose> ShownPoses(SpriteSheetId sheet, CinematicSpriteFamily family, EndingSheetDraws draws)
    {
        var poses = new HashSet<CinematicSpritePose>(draws.Poses(family).Keys);
        var composed = new HashSet<CinematicSpritePose>(CinematicSpritePoses.Of(family));
        foreach (CinematicSpritePose pose in poses)
        {
            if (!composed.Contains(pose))
                throw new InvalidDataException($"The scene of {sheet} shows pose {pose}, which none of its families composes.");
        }
        return composed.Where(poses.Contains).OrderBy(CinematicSpritemapSources.CartridgeOrder);
    }

    /// <summary>
    /// <paramref name="families"/> keeping only the poses the scene of <paramref name="sheet"/> draws from each, as
    /// <paramref name="draws"/> records them.
    /// </summary>
    /// <exception cref="InvalidDataException">The scene draws a pose its family does not compose.</exception>
    private static ICinematicSpriteFrames[] ShownFamilies(SpriteSheetId sheet, IEnumerable<ICinematicSpriteFrames> families, EndingSheetDraws draws) =>
        [.. families.Select(family => CinematicShownFrames.Of(family, new HashSet<CinematicSpritePose>(ShownPoses(sheet, family.Family, draws)), _ => true))];

    /// <summary>
    /// Adds each of <paramref name="family"/>'s poses over <paramref name="characters"/>, its palette-inheriting parts in the
    /// sprite set of the colouring the pose is first drawn in, with every colouring the scene draws it in.
    /// </summary>
    private static void AddFamily(List<ColouredFrame<EndingSpriteColouring>> frames, List<string> blank, ICinematicSpriteFrames family,
        EndingSheetDraws draws, SpriteArtwork characters, ScreenObjectSizes sizes)
    {
        Dictionary<CinematicSpritePose, List<EndingSpriteColouring>> poses = draws.Poses(family.Family);
        foreach ((CinematicSpritePose pose, SpriteComposition composition) in family.Frames.OrderBy(pair => CinematicSpritemapSources.CartridgeOrder(pair.Key)))
        {
            if (family.Family == EndingTypedTexts.Family && EndingTypedTexts.Texts.Find(pose) is not null)
                continue;
            string name = CinematicSpriteSheet.FrameName(family.Family, pose);
            EndingSpriteColouring[] colourings = [.. poses[pose]];
            frames.Add(new ColouredFrame<EndingSpriteColouring>(
                new SheetFrameSource(name, composition.ToFrame(name, 0, sizes), characters, EndingScreenInks.SpriteSet(colourings[0])), colourings));
        }
        if (family.Family != EndingTypedTexts.Family)
            return;
        // Each typed text's glyphs, each the parts its step adds, drawn in every colouring a later step is drawn in.
        foreach (CinematicSpriteSegment text in EndingTypedTexts.Texts.Texts)
        {
            IReadOnlyList<CinematicSpritePose> steps = EndingTypedTexts.Texts.Steps(text);
            SpriteFramePart[][] glyphs = TypedTextGlyphs.Of(text.ToString(), [.. steps.Select(pose => family.Frames[pose].ToFrame(text.ToString(), 0, sizes))],
                Offset16<ScreenPixels>.Zero);
            for (int glyph = 0; glyph < glyphs.Length; glyph++)
            {
                string name = TypedTextSpriteSheet.GlyphFrameName(family.Family, text, glyph);
                if (glyphs[glyph].Length == 0)
                {
                    blank.Add(name);
                    continue;
                }
                EndingSpriteColouring[] colourings = [.. steps.Skip(glyph).SelectMany(pose => poses[pose]).Distinct()];
                frames.Add(new ColouredFrame<EndingSpriteColouring>(new SheetFrameSource(name, new SpriteFrame(name, [.. glyphs[glyph]]), characters,
                    EndingScreenInks.SpriteSet(colourings[0])), colourings));
            }
        }
    }

    /// <summary>The names of the glyph frames of every text typed in <paramref name="family"/>; none for a family that types no text.</summary>
    private static IEnumerable<string> GlyphNames(CinematicSpriteFamily family) => family == EndingTypedTexts.Family
        ? EndingTypedTexts.Texts.Texts.SelectMany(text => Enumerable.Range(0, EndingTypedTexts.Texts.Steps(text).Count)
            .Select(glyph => TypedTextSpriteSheet.GlyphFrameName(family, text, glyph)))
        : [];

    /// <summary>
    /// The sets the own-palette parts of each colouring's frames keep (#1544): such a part is its actor's, so it shows the
    /// effects authored for the actors drawn in each colouring its frame is drawn in, in its own colours.
    /// </summary>
    private static Dictionary<EndingSpriteColouring, SortedSet<int>> OwnSets(List<ColouredFrame<EndingSpriteColouring>> frames)
    {
        var sets = new Dictionary<EndingSpriteColouring, SortedSet<int>>();
        foreach (ColouredFrame<EndingSpriteColouring> frame in frames)
        {
            foreach (SpriteFramePart part in frame.Frame.Frame.Parts)
            {
                if (part.Palette is not int own)
                    continue;
                foreach (EndingSpriteColouring colouring in frame.Colourings)
                {
                    if (!sets.TryGetValue(colouring, out SortedSet<int>? kept))
                    {
                        kept = [];
                        sets.Add(colouring, kept);
                    }
                    kept.Add(own);
                }
            }
        }
        return sets;
    }

    /// <summary>The characters <paramref name="transfers"/> leave on the sprite page <paramref name="objectSelection"/> selects.</summary>
    private static SpriteArtwork Characters(IReadOnlyList<EndingTransfer> transfers, EndingTransferArtwork artwork, int objectSelection) =>
        ObjCharacterImage.Page(EndingScreenImage.Compose(transfers, artwork), objectSelection);

    /// <summary>The transfers that give the logo its characters: the post-credits art, then every post-shot part.</summary>
    private static IReadOnlyList<EndingTransfer> LogoTransfers =>
    [
        new EndingTransfer(EndingTransferKind.CreditsCharacterArt),
        .. Enumerable.Range(0, EndingPostShotUploadDefinitions.Count).Select(index => new EndingTransfer(EndingTransferKind.PostShotPart, index)),
    ];

    /// <summary>The page ending sheet <paramref name="sheet"/> shows.</summary>
    private static EndingPage Page(SpriteSheetId sheet)
    {
        return sheet switch
        {
            SpriteSheetId.EndingCloudSprites => new(EndingReward.Armored, [new EndingTransfer(EndingTransferKind.EscapeClouds)],
                [CinematicSpriteFamily.EndingCloud], Logo: false, EscapeColours),
            SpriteSheetId.EndingExplosionSprites => new(EndingReward.Armored,
                [
                    new EndingTransfer(EndingTransferKind.ExplosionObjects),
                    .. Enum.GetValues<EndingObjectFragmentId>().Select(fragment => new EndingTransfer(EndingTransferKind.ExplosionMap, Fragment: fragment)),
                    new EndingTransfer(EndingTransferKind.EndingFont),
                ],
                [CinematicSpriteFamily.EndingExplosion, CinematicSpriteFamily.EndingCompletionText, CinematicSpriteFamily.EndingAnimalEscape], Logo: false,
                palettes => SpritesOf(palettes, EndingPaletteId.Explosion)),
            SpriteSheetId.EndingSuitedRewardSprites => Reward(EndingReward.Armored),
            SpriteSheetId.EndingSuitlessRewardSprites => Reward(EndingReward.Suitless),
            SpriteSheetId.EndingShootingSprites => Shooting,
            SpriteSheetId.EscapeTimer or SpriteSheetId.Grapple or SpriteSheetId.SamusBody or
                SpriteSheetId.SamusOverlays or SpriteSheetId.SamusAtmosphere or
                SpriteSheetId.SamusDeath or SpriteSheetId.GameOver or SpriteSheetId.FileSelect or
                SpriteSheetId.GameOptions or SpriteSheetId.MapSprites or SpriteSheetId.PauseSprites or
                SpriteSheetId.Title or SpriteSheetId.IntroSprites or SpriteSheetId.CeresFlightSprites or
                SpriteSheetId.CeresDestructionSprites or SpriteSheetId.ZebesRevealSprites or
                SpriteSheetId.IntroProjectiles or SpriteSheetId.ProjectilesPower or SpriteSheetId.ProjectilesWave or
                SpriteSheetId.ProjectilesIce or SpriteSheetId.ProjectilesIceWave or
                SpriteSheetId.ProjectilesSpazer or SpriteSheetId.ProjectilesSpazerWave or
                SpriteSheetId.ProjectilesSpazerIce or SpriteSheetId.ProjectilesSpazerIceWave or
                SpriteSheetId.ProjectilesPlasma or SpriteSheetId.ProjectilesPlasmaWave or
                SpriteSheetId.ProjectilesPlasmaIce or SpriteSheetId.ProjectilesPlasmaIceWave or
                SpriteSheetId.ProjectilesSpazerPlasmaWave or SpriteSheetId.ProjectilesSpazerPlasmaIce or
                SpriteSheetId.MenuPictures or SpriteSheetId.RoomMapPictures or SpriteSheetId.PausePictures or
                SpriteSheetId.AreaMapForegroundCrateria or SpriteSheetId.AreaMapForegroundBrinstar or
                SpriteSheetId.AreaMapForegroundNorfair or SpriteSheetId.AreaMapForegroundWreckedShip or
                SpriteSheetId.AreaMapForegroundMaridia or SpriteSheetId.AreaMapForegroundTourian or
                SpriteSheetId.AreaMapBackgroundCrateria or SpriteSheetId.AreaMapBackgroundBrinstar or
                SpriteSheetId.AreaMapBackgroundNorfair or SpriteSheetId.AreaMapBackgroundWreckedShip or
                SpriteSheetId.AreaMapBackgroundMaridia or SpriteSheetId.AreaMapBackgroundTourian or
                SpriteSheetId.PauseHud or
                SpriteSheetId.IntroScenePictures or SpriteSheetId.IntroNarrationPictures or
                SpriteSheetId.CeresCinematicPictures or SpriteSheetId.EndingCreditsRows or
                SpriteSheetId.EndingPostCreditsPictures or SpriteSheetId.EndingWavySamusPicture or SpriteSheetId.EndingExplosionPictures or
                SpriteSheetId.TitleFloor or SpriteSheetId.CeresFloor or SpriteSheetId.EndingSideCloudEscapeFloor or
                SpriteSheetId.EndingTopAndBottomCloudEscapeFloor or SpriteSheetId.EndingPlanetExplosionFloor or
                SpriteSheetId.EndingFlyawayFloor or SpriteSheetId.EndingRewardIconFloor =>
                throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Not an ending sheet."),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
        };
    }

    /// <summary>The post-credits reward page of an ending of <paramref name="reward"/>.</summary>
    private static EndingPage Reward(EndingReward reward) => new(reward,
        [new EndingTransfer(EndingTransferKind.CreditsCharacterArt)], [CinematicSpriteFamily.EndingReward], Logo: false, PostCreditsColours);

    /// <summary>
    /// The shooting scene: the falling, landing and shooting Samus and the stars over the post-credits art, and the logo
    /// the scene ends on over its own characters. The reward's characters lie outside this page, so any reward shows it.
    /// </summary>
    private static EndingPage Shooting => new(EndingReward.Armored,
        [new EndingTransfer(EndingTransferKind.CreditsCharacterArt)], [CinematicSpriteFamily.EndingReward], Logo: true, PostCreditsColours);

    /// <summary>The escape scenes' sprite colours.</summary>
    private static RgbColor[] EscapeColours(EndingPaletteSet palettes) => SpritesOf(palettes, EndingPaletteId.Escape);

    /// <summary>The post-credits scenes' sprite colours, which the credits' end restores.</summary>
    private static RgbColor[] PostCreditsColours(EndingPaletteSet palettes) => SpritesOf(palettes, EndingPaletteId.PostCredits);

    /// <summary>The sprite colours of the whole screen <paramref name="palette"/>.</summary>
    private static RgbColor[] SpritesOf(EndingPaletteSet palettes, EndingPaletteId palette) => palettes[palette].Sprites.ToArray();

    /// <summary>
    /// A scene of the ending: the reward the scene's transfers dress, those transfers in order, the families it shows over them, whether it also holds the logo over the logo's own characters, and the
    /// sprite colours its scene loads.
    /// </summary>
    private sealed record EndingPage(EndingReward Reward, IReadOnlyList<EndingTransfer> Transfers,
        IReadOnlyList<CinematicSpriteFamily> Families, bool Logo,
        Func<EndingPaletteSet, RgbColor[]> SpriteColours);
}
