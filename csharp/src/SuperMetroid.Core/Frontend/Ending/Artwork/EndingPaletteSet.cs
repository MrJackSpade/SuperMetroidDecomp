
namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Independent static and animated color resources used by the ending.</summary>
[ArchitectureReview("context-bag", "Seven installed ending palettes selected by id; every member is immutable color data, not a service.")]
public sealed class EndingPaletteSet
{
    /// <summary>Escape and cloud-scene color image.</summary>
    internal readonly EndingPalette escape;
    /// <summary>Post-credits reward-scene color image.</summary>
    internal readonly EndingPalette postCredits;
    /// <summary>Credits text and background color image.</summary>
    internal readonly EndingPalette credits;
    /// <summary>Explosion and flyaway color image with separately loaded background and sprite halves.</summary>
    internal readonly EndingPalette explosion;
    /// <summary>Final gunship background palette restored before the operation text.</summary>
    internal readonly EndingPalette finalGunship;
    /// <summary>Initial assembling-logo OBJ palette.</summary>
    internal readonly EndingPalette logoInitial;
    /// <summary>Sixteen ordered background and sprite palette pairs for the final logo transition.</summary>
    internal readonly EndingPalette logoCrossfade;

    /// <summary>Groups seven independently selected immutable palette resources without copying them or validating their sizes.</summary>
    /// <param name="escape">Full palette for escape and cloud imagery.</param>
    /// <param name="postCredits">Full palette for the post-credits reward sequence.</param>
    /// <param name="credits">Full credits resource, of which scrolling text uses the background half.</param>
    /// <param name="explosion">Full destruction/flyaway resource with separately loaded background and sprite halves.</param>
    /// <param name="finalGunship">Sixteen colors for the final gunship's background palette 5.</param>
    /// <param name="logoInitial">Sixteen initial assembling-logo colors for OBJ palette 7.</param>
    /// <param name="logoCrossfade">Sixteen ordered background and sprite palette pairs for the final logo transition.</param>
    public EndingPaletteSet(EndingPalette escape, EndingPalette postCredits,
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
}
