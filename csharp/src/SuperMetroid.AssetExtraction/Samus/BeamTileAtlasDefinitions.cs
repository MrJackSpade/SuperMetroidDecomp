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
    /// <summary>The twelve combinations Samus can equip, in the order the cartridge's beam tables list them.</summary>
    public static IReadOnlyList<SamusBeamCombination> Equippable { get; } =
    [
        SamusBeamCombination.Power, SamusBeamCombination.Wave, SamusBeamCombination.Ice, SamusBeamCombination.IceWave,
        SamusBeamCombination.Spazer, SamusBeamCombination.SpazerWave, SamusBeamCombination.SpazerIce,
        SamusBeamCombination.SpazerIceWave, SamusBeamCombination.Plasma, SamusBeamCombination.PlasmaWave,
        SamusBeamCombination.PlasmaIce, SamusBeamCombination.PlasmaIceWave,
    ];

    /// <summary>Every combination with a tile sheet: the twelve Samus can equip, then the bounded Chainsaw and SpaceTime ones.</summary>
    public static IReadOnlyList<SamusBeamCombination> Pictured { get; } =
    [
        .. Equippable,
        SuperMetroid.Core.Samus.Projectiles.ChainsawBeamGraphicsDefinitions.Selection,
        SuperMetroid.Core.Samus.Projectiles.SpacetimeBeamGraphicsDefinitions.Selection,
    ];

    /// <summary>Creates a combination-keyed PNG filename, retaining separately editable combination identities even where native combinations share the same source sheet.</summary>
    /// <param name="selection">A combination with a tile sheet (<see cref="Pictured"/>).</param>
    /// <returns><c>beam-{key}-tiles.png</c>, the combination's written-out installed key.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The selection has no tile sheet.</exception>
    public static string FileName(SamusBeamCombination selection)
    {
        RequireArtwork(selection);
        return $"beam-{SamusBeamCombinationKeys.Installed.Of(selection)}-tiles.png";
    }

    /// <summary>Requires that <paramref name="selection"/> has a tile sheet: every combination but the two Spazer-Plasma ones.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The combination is undefined or has no artwork.</exception>
    public static void RequireArtwork(SamusBeamCombination selection)
    {
        if (!Enum.IsDefined(selection))
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "Undefined beam combination.");
        if (selection is SamusBeamCombination.SpazerPlasma or SamusBeamCombination.SpazerPlasmaIceWave)
            throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam combination has no artwork.");
    }
}
