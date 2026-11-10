using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Native $90:AC8D beam-character upload geometry, separate from projectile mechanics.</summary>
public static class BeamTileAtlasDefinitions
{
    /// <summary>Indexed PNG width in pixels, arranging eight 8-pixel beam/impact characters left to right in native upload order.</summary>
    public const int Width = 64;
    /// <summary>Indexed PNG height in pixels: one row of 8-by-8 characters, not a projectile collision height.</summary>
    public const int Height = 8;
    /// <summary>Native $90:AC8D upload length, $0100 bytes: eight four-bit planar characters at 32 bytes each.</summary>
    public const int ByteCount = 256;
    /// <summary>$90:C3B1 contains twelve legal beam-combination pointers before palette data.</summary>
    public const int SelectionCount = BeamPaletteFile.SelectionCount;
    /// <summary>Twelve ordinary sheets plus the bounded Chainsaw and SpaceTime adjacent-table uploads.</summary>
    public const int ArtworkCount = SelectionCount + 2;
    /// <summary>Maps the complete artwork-catalog ordinal to its independently editable beam identity: ordinary selections $00..$0B, then bounded Chainsaw $0D and SpaceTime $0E.</summary>
    /// <param name="index">Zero-based artwork ordinal 0..13; the last two ordinals are not their returned beam-selection values.</param>
    /// <returns>The beam combination used to name and install the tile sheet; $0C and $0F have no artwork.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The artwork ordinal is outside 0..13.</exception>
    public static SamusBeamCombination SelectionAt(int index) => index == SelectionCount ? SuperMetroid.Core.Samus.Projectiles.ChainsawBeamGraphicsDefinitions.Selection :
        index == SelectionCount + 1 ? SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitions.Selection : (uint)index < SelectionCount
            ? SamusBeamCombinationTableOrder.FromTableIndex(index) : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>The artwork-catalog ordinal of a combination's sheet: the inverse of <see cref="SelectionAt"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination has no artwork.</exception>
    public static int ArtworkOrdinal(SamusBeamCombination selection) => selection switch
    {
        SuperMetroid.Core.Samus.Projectiles.ChainsawBeamGraphicsDefinitions.Selection => SelectionCount,
        SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitions.Selection => SelectionCount + 1,
        SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave =>
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no artwork."),
        SamusBeamCombination.Power or SamusBeamCombination.Wave or SamusBeamCombination.Ice or
            SamusBeamCombination.IceWave or SamusBeamCombination.Spazer or SamusBeamCombination.SpazerWave or
            SamusBeamCombination.SpazerIce or SamusBeamCombination.SpazerIceWave or SamusBeamCombination.Plasma or
            SamusBeamCombination.PlasmaWave or SamusBeamCombination.PlasmaIce or
            SamusBeamCombination.PlasmaIceWave => selection.TableIndex,
        _ => throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination."),
    };

    /// <summary>Creates a selection-keyed PNG filename, retaining separately editable combination identities even where native combinations share the same source sheet.</summary>
    /// <param name="selection">Native beam identity $00..$0B, Chainsaw $0D, or SpaceTime $0E; not an artwork-catalog ordinal.</param>
    /// <returns><c>beam-XX-tiles.png</c> with two uppercase hexadecimal selection digits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The selection is outside the supported ordinary/bounded domain.</exception>
    public static string FileName(SamusBeamCombination selection)
    {
        _ = ArtworkOrdinal(selection);
        return $"beam-{selection.TableIndex:X2}-tiles.png";
    }
}
