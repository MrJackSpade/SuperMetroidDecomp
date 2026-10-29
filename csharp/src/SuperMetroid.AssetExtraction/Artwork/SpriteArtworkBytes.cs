using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// The installed characters a sprite artwork source names (#1422): the content the import pipeline composes an encounter's
/// pre-coloured pictures from. Core names the artwork a run of sprite characters shows; this decodes each name against the
/// installed artwork, which is the one place the names meet their bytes.
/// </summary>
internal static class SpriteArtworkBytes
{
    /// <summary>The installed characters <paramref name="source"/> names.</summary>
    /// <param name="source">The artwork shown.</param>
    /// <param name="installed">The installed artwork; area, boss and enemy sources also need its installed catalogs.</param>
    /// <exception cref="InvalidDataException">The installed artwork holds no characters for the source.</exception>
    internal static ReadOnlyMemory<byte> Of(SpriteArtworkSource source, IInstalledArtworkContent installed)
    {
        Ensure.NotNull(source);
        Ensure.NotNull(installed);
        return source switch
        {
            InstalledArtworkCharacters named => installed.Resolve(named.Artwork),
            CeresDoorCharacters => (Areas(installed).CeresDoorVisual ?? throw new InvalidDataException(
                "Installed area artwork has no Ceres door visuals.")).Characters,
            CeresWarningTextPage warning => (Areas(installed).CeresEscapeTiles ?? throw new InvalidDataException(
                    "Installed Ceres escape has no editable character pages."))
                .TryResolve(CeresWarningTextPageDefinitions.InstalledPage, CeresWarningTextPages.ByteOffset(warning), CeresWarningTextPages.ByteCount(warning),
                    out ReadOnlyMemory<byte> characters)
                ? characters
                : throw new InvalidDataException($"Installed Ceres escape warning text has no characters for {warning}."),
            TourianStatuePictureCharacters statue => TourianStatue(statue, installed),
            CrocomireSkeletonChunkCharacters chunk => (Bosses(installed).CrocomireSkeleton ?? throw new InvalidOperationException(
                "Installed Crocomire artwork is missing its skeleton characters.")).Chunk(chunk.Entry),
            MotherBrainCorpseCharacters corpse => (Bosses(installed).MotherBrainCorpse ?? throw new InvalidDataException(
                "Installed enemy artwork is missing mother-brain-corpse-tiles.png.")).Characters.Slice(
                MotherBrainCorpseSheet.PageOffset(corpse.Page), MotherBrainCorpseArtworkDefinitions.PageByteCount),
            MotherBrainEscapeTextCharacters text => (Bosses(installed).MotherBrainEscapeText ?? throw new InvalidDataException(
                "Installed enemy artwork is missing mother-brain-escape-text-tiles.png.")).Characters.Slice(
                EscapeTextPages.PageOffset(text.Page), EscapeTextPages.PageByteCount(text.Page)),
            MotherBrainSpecialSpriteCharacters special => MotherBrainSpecialSprite(special, installed),
            TorizoArtworkSwapCharacters swap => TorizoSwap(swap, installed),
            EnemyTileCharacters tiles => Sprites(installed).EnemyTiles.TilesOf(tiles.Enemy),
            GraphicsSetTileCharacters tiles => Sprites(installed).EnemyTiles.TilesOf(tiles.Enemy),
            _ => throw new InvalidDataException($"No installed artwork resolves {source} ({source.GetType().Name})."),
        };
    }

    /// <summary>One Tourian statue picture's characters, as its animated-tile program frames them.</summary>
    private static ReadOnlyMemory<byte> TourianStatue(TourianStatuePictureCharacters statue, IInstalledArtworkContent installed)
    {
        TourianStatueAnimatedTileProgramDefinition definition =
            TourianStatueAnimatedTiles.All.Single(candidate => candidate.AnimatedObject == statue.Statue);
        return Sprites(installed).AnimatedTiles.TourianStatuePicture(definition.AnimatedObject, statue.Picture, definition.FrameByteCount);
    }

    /// <summary>Pages of one installed Mother Brain special sprite sheet.</summary>
    private static ReadOnlyMemory<byte> MotherBrainSpecialSprite(MotherBrainSpecialSpriteCharacters special, IInstalledArtworkContent installed)
    {
        MotherBrainSpecialSpriteSheetDefinition sheet = MotherBrainSpecialSpriteArtworkDefinitions.All[special.Sheet];
        MotherBrainSpecialSpriteArtworkCatalog artwork = Bosses(installed).MotherBrainSpecialSprites ??
            throw new InvalidDataException($"Installed enemy artwork is missing {sheet.FileName}.");
        return artwork.Get(sheet.Sheet).Characters.Slice(special.Page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount, MotherBrainSpecialSpriteSheetDefinition.PageByteCount);
    }

    /// <summary>The installed characters one Torizo artwork swap shows.</summary>
    private static ReadOnlyMemory<byte> TorizoSwap(TorizoArtworkSwapCharacters swap, IInstalledArtworkContent installed)
    {
        TorizoArtworkSwapDefinition definition = TorizoArtworkSwapPageDefinitions.Entries[swap.Entry];
        return (Bosses(installed).TorizoSwaps ?? throw new InvalidDataException("Torizo artwork swaps require installed artwork."))
            .Characters(definition.Page, definition.PageByte, definition.ByteCount);
    }

    /// <summary>The installed enemy tiles and room-FX animated tiles <paramref name="installed"/> holds.</summary>
    /// <exception cref="InvalidOperationException">It holds no installed gameplay artwork.</exception>
    private static IInstalledSpriteCharacterArtwork Sprites(IInstalledArtworkContent installed) =>
        installed as IInstalledSpriteCharacterArtwork ?? throw new InvalidOperationException(
            $"{installed.GetType().Name} holds no installed gameplay artwork to resolve a named sprite artwork source.");

    /// <summary>The installed boss artwork <paramref name="installed"/> holds.</summary>
    /// <exception cref="InvalidOperationException">It holds no installed boss artwork.</exception>
    private static BossSourceArtwork Bosses(IInstalledArtworkContent installed) =>
        (installed as IInstalledBossSpriteCharacterArtwork ?? throw new InvalidOperationException(
            $"{installed.GetType().Name} holds no installed boss artwork to resolve a named sprite artwork source.")).BossTiles;

    /// <summary>The installed area artwork <paramref name="installed"/> holds.</summary>
    /// <exception cref="InvalidOperationException">It holds no installed area artwork.</exception>
    private static AreaSourceArtwork Areas(IInstalledArtworkContent installed) =>
        (installed as IInstalledAreaSpriteCharacterArtwork ?? throw new InvalidOperationException(
            $"{installed.GetType().Name} holds no installed area artwork to resolve a named sprite artwork source.")).AreaTiles;
}
