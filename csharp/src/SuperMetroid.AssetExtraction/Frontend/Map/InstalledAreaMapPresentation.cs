using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map presentation (#1466, #1544, #1616, #1636): the catalog the game binds (<see cref="Game"/>), and beside
/// it the installed documents its picture sheets' frames are named from and the import composes from, which the game never
/// receives. It reads no character page: the characters are import data the installation never holds, which the import
/// reads with <see cref="ImportAreaMapPresentation"/>, as it does the colours only the import reads.
/// </summary>
[ArchitectureReview("context-bag", "The installed map presentation's documents beside the catalog the game binds; every member is immutable artwork data the import composes pictures from, not a service.")]
public class InstalledAreaMapPresentation
{
    /// <summary>Takes the catalog the game binds and the installed documents it was read with.</summary>
    internal InstalledAreaMapPresentation(AreaMapPresentation game, InstalledPresentationSources sources)
    {
        Game = Ensure.NotNull(game);
        Ensure.NotNull(sources);
        StaffCredits = Ensure.NotNull(sources.StaffCredits);
        HighlightColours = Ensure.NotNull(sources.HighlightColours);
    }

    /// <summary>The catalog the game binds: only what the game reads of the installed presentation (#1616).</summary>
    public AreaMapPresentation Game { get; }

    /// <summary>
    /// The map highlight cycle with each frame's colours (#1512): only the import reads the colours, and the game takes each
    /// frame's hold from the same frames (<see cref="AreaMapPresentation.HighlightCycle"/>).
    /// </summary>
    public MapPaletteCycle HighlightColours { get; }

    /// <summary>The editable staff credits, which the import pictures into the ending's credits rows (#1467).</summary>
    public CreditsPresentation StaffCredits { get; }


}
