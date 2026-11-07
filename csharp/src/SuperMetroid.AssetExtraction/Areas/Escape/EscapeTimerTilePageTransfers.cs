using SuperMetroid.Core;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The two transfer pages the escape timer's characters upload as (#1502): each page's size, the OBJ character it begins
/// at, and the installed characters it carries. Only the import pipeline resolves the pages; the game draws the timer's
/// pre-coloured sheet.
/// </summary>
public static class EscapeTimerTilePageTransfers
{
    /// <summary>The page's upload size in bytes.</summary>
    public static ushort ByteCountOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstByteCount,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondByteCount,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {page}."),
    };

    /// <summary>The OBJ character the page begins at.</summary>
    public static int FirstCharacterOf(EscapeTimerTilePage page) => page switch
    {
        EscapeTimerTilePage.First => EscapeTimerTileAtlasFormat.FirstCharacter,
        EscapeTimerTilePage.Second => EscapeTimerTileAtlasFormat.SecondCharacter,
        _ => throw new InvalidOperationException($"Undefined {nameof(EscapeTimerTilePage)} {page}."),
    };

    /// <summary>Resolves one page of <paramref name="atlas"/> without combining or retiming its original transfer record.</summary>
    /// <param name="atlas">The installed timer characters.</param>
    /// <param name="asset">One of the two escape-timer page identities.</param>
    /// <exception cref="InvalidDataException"><paramref name="asset"/> names no escape-timer page.</exception>
    public static ReadOnlyMemory<byte> Resolve(EscapeTimerCharacters atlas, InstalledArtworkId asset)
    {
        ReadOnlySpan<byte> characters = Ensure.NotNull(atlas).characters;
        return asset switch
        {
            InstalledArtworkId.EscapeTimerFirstTiles => characters[..EscapeTimerTileAtlasFormat.FirstByteCount].ToArray(),
            InstalledArtworkId.EscapeTimerSecondTiles => characters.Slice(EscapeTimerTileAtlasFormat.FirstByteCount, EscapeTimerTileAtlasFormat.SecondByteCount).ToArray(),
            InstalledArtworkId.StandardHudTiles or
                InstalledArtworkId.BeamPowerTiles or
                InstalledArtworkId.BeamWaveTiles or
                InstalledArtworkId.BeamIceTiles or
                InstalledArtworkId.BeamIceWaveTiles or
                InstalledArtworkId.BeamSpazerTiles or
                InstalledArtworkId.BeamSpazerWaveTiles or
                InstalledArtworkId.BeamSpazerIceTiles or
                InstalledArtworkId.BeamSpazerIceWaveTiles or
                InstalledArtworkId.BeamPlasmaTiles or
                InstalledArtworkId.BeamPlasmaWaveTiles or
                InstalledArtworkId.BeamPlasmaIceTiles or
                InstalledArtworkId.BeamPlasmaIceWaveTiles or
                InstalledArtworkId.ProjectileIceWaveTrailTiles or
                InstalledArtworkId.ProjectileMissileTrailTiles or
                InstalledArtworkId.GrapplePointFirstTiles or
                InstalledArtworkId.GrapplePointSecondTiles or
                InstalledArtworkId.GrapplePointThirdTiles or
                InstalledArtworkId.GrapplePointFourthTiles or
                InstalledArtworkId.GrappleHorizontalSegmentTiles or
                InstalledArtworkId.GrappleDiagonalSegmentTiles or
                InstalledArtworkId.GrappleVerticalSegmentTiles or
                InstalledArtworkId.GunshipLiftoffFirstTiles or
                InstalledArtworkId.GunshipLiftoffSecondTiles or
                InstalledArtworkId.GunshipLiftoffThirdTiles or
                InstalledArtworkId.GunshipLiftoffFourthTiles or
                InstalledArtworkId.GunshipLiftoffFifthTiles or
                InstalledArtworkId.BeamChainsawTiles or
                InstalledArtworkId.BeamSpacetimeTiles or
                InstalledArtworkId.StandardObjectTiles or
                InstalledArtworkId.HudTopRowTilemap => throw new InvalidDataException($"Escape timer artwork cannot resolve tile asset {asset}."),
            _ => throw new InvalidDataException($"Escape timer artwork cannot resolve tile asset {asset}."),
        };
    }
}
