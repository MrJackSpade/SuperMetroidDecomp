using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Independent static and animated color resources used by the ending.</summary>
public sealed class EndingPaletteCatalog
{
    /// <summary>Escape and cloud-scene color image.</summary>
    private readonly EndingPalette escape;
    /// <summary>Post-credits reward-scene color image.</summary>
    private readonly EndingPalette postCredits;
    /// <summary>Credits text and background color image.</summary>
    private readonly EndingPalette credits;
    /// <summary>Explosion and flyaway color image with separate BG and OBJ transfers.</summary>
    private readonly EndingPalette explosion;
    /// <summary>Final gunship BG palette restored before the operation text.</summary>
    private readonly EndingPalette finalGunship;
    /// <summary>Initial assembling-logo OBJ palette.</summary>
    private readonly EndingPalette logoInitial;
    /// <summary>Sixteen ordered BG and OBJ palette pairs for the final logo transition.</summary>
    private readonly EndingPalette logoCrossfade;

    /// <summary>Groups seven independently selected immutable palette resources without copying them or validating their sizes.</summary>
    /// <param name="escape">Full palette for escape and cloud imagery.</param>
    /// <param name="postCredits">Full palette for the post-credits reward sequence.</param>
    /// <param name="credits">Full credits resource, of which scrolling text uses the BG half.</param>
    /// <param name="explosion">Full destruction/flyaway resource with separately transferred BG and OBJ halves.</param>
    /// <param name="finalGunship">Sixteen colors for the final gunship's BG palette 5.</param>
    /// <param name="logoInitial">Sixteen initial assembling-logo colors for OBJ palette 7.</param>
    /// <param name="logoCrossfade">Sixteen ordered BG/OBJ palette pairs for the final logo transition.</param>
    public EndingPaletteCatalog(EndingPalette escape, EndingPalette postCredits,
        EndingPalette credits, EndingPalette explosion, EndingPalette finalGunship,
        EndingPalette logoInitial, EndingPalette logoCrossfade)
    {
        this.escape = escape;
        this.postCredits = postCredits;
        this.credits = credits;
        this.explosion = explosion;
        this.finalGunship = finalGunship;
        this.logoInitial = logoInitial;
        this.logoCrossfade = logoCrossfade;
    }

    /// <summary>Returns the retained palette resource selected by its ending-scene role.</summary>
    /// <param name="id">Independent static palette or complete logo-crossfade resource to retrieve.</param>
    /// <returns>The same palette instance supplied for that role at construction.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a defined palette role.</exception>
    public EndingPalette this[EndingPaletteId id] => id switch
    {
        EndingPaletteId.Escape => escape,
        EndingPaletteId.PostCredits => postCredits,
        EndingPaletteId.Credits => credits,
        EndingPaletteId.Explosion => explosion,
        EndingPaletteId.FinalGunship => finalGunship,
        EndingPaletteId.LogoInitial => logoInitial,
        EndingPaletteId.LogoCrossfade => logoCrossfade,
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    /// <summary>Identity of all selected static colors and every ordered logo-crossfade color.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(EndingPaletteCatalog), content =>
    {
        content.Append("palette-count", 7);
        content.Append("palette", escape.Transfer.Span);
        content.Append("palette", postCredits.Transfer.Span);
        content.Append("palette", credits.Transfer.Span);
        content.Append("palette", explosion.Transfer.Span);
        content.Append("palette", finalGunship.Transfer.Span);
        content.Append("palette", logoInitial.Transfer.Span);
        content.Append("palette", logoCrossfade.Transfer.Span);
    });
}
