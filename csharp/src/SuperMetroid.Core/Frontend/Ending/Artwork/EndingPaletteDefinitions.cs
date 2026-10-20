namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Native source addresses, exact sizes, and file identities for ending colors.</summary>
public static class EndingPaletteDefinitions
{
    /// <summary>Revision 1 of the RGB5 palette document, retained for compatibility with existing authored overrides.</summary>
    /// <remarks>The document shape is unchanged; only the installation manifest gains files. Keeping version one lets
    /// existing player-authored palette overrides survive.</remarks>
    public const int Version = 1;
    /// <summary>Installation manifest filename identifying the independently selected ending palette files.</summary>
    public const string ManifestFileName = "ending-palettes-manifest.json";

    /// <summary>Six named contiguous resources selected by native ending load operations.
    /// Crossfade has two interleaved sources per step and deliberately has no single address.</summary>
    public static int SourceAddress(EndingPaletteId id) => id switch
    {
        EndingPaletteId.Escape => EndingCreditsRomData.Assets.EscapePalette,
        EndingPaletteId.PostCredits => EndingCreditsRomData.Assets.PostCreditsPalette,
        EndingPaletteId.Credits => EndingCreditsRomData.Assets.CreditsPalette,
        EndingPaletteId.Explosion => EndingCreditsRomData.Assets.ExplosionPalette,
        EndingPaletteId.FinalGunship => EndingCreditsRomData.Assets.FinalGunshipPalette,
        EndingPaletteId.LogoInitial => EndingLogoDefinitions.InitialPalette,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>Full editable resource sizes, not individual CGRAM transfer lengths:
    /// four 256-color bank-$8C allocations, two 16-color palettes, and sixteen pairs
    /// of 16-color crossfade palettes. Native credits transfers can select a subset.</summary>
    public static int ColorCount(EndingPaletteId id) => id switch
    {
        EndingPaletteId.Escape or EndingPaletteId.PostCredits or EndingPaletteId.Credits or
            EndingPaletteId.Explosion => CgramPalettes.ColorCount,
        EndingPaletteId.FinalGunship => 16,
        EndingPaletteId.LogoInitial => 16,
        EndingPaletteId.LogoCrossfade => EndingLogoDefinitions.PaletteSteps * 2 * 16,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>Published asset-file identities selected by semantic palette role.
    /// Explicit cases preserve independently editable stock and override filenames.</summary>
    public static string FileName(EndingPaletteId id) => id switch
    {
        EndingPaletteId.Escape => "ending-escape-palette.json",
        EndingPaletteId.PostCredits => "ending-post-credits-palette.json",
        EndingPaletteId.Credits => "ending-credits-palette.json",
        EndingPaletteId.Explosion => "ending-explosion-palette.json",
        EndingPaletteId.FinalGunship => "ending-final-gunship-palette.json",
        EndingPaletteId.LogoInitial => "ending-logo-initial-palette.json",
        EndingPaletteId.LogoCrossfade => "ending-logo-crossfade-palette.json",
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };
}
