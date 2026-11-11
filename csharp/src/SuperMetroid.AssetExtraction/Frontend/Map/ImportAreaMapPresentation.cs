using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map presentation as the import reads it to compose sheets (#1544): the installed presentation with the
/// colours only the import pre-colours sheets and pictures from. The game never reads these colours; the game's read of the
/// map presentation (<see cref="MapPresentationFiles.Load"/>) leaves their documents unread.
/// </summary>
public sealed class ImportAreaMapPresentation : InstalledAreaMapPresentation
{
    /// <summary>Takes the catalog the game binds, the source artwork and the colours only the import reads.</summary>
    internal ImportAreaMapPresentation(AreaMapPresentation game, InstalledPresentationSources sources, ImportPresentationColours colours)
        : base(game, sources)
    {
        Ensure.NotNull(colours);
        SamusColours = Ensure.NotNull(colours.SamusColours);
        TitlePalette = Ensure.NotNull(colours.TitlePalette);
        Palettes = Ensure.NotNull(colours.Palettes);
        CeresRidleyColors = Ensure.NotNull(colours.CeresRidleyColors);
        CeresRidleyFloorColors = Ensure.NotNull(colours.CeresRidleyFloorColors);
        MotherBrainHealthPalette = Ensure.NotNull(colours.MotherBrainHealthPalette);
        MotherBrainRoomColors = Ensure.NotNull(colours.MotherBrainRoomColors);
        RoomFxPaletteBlends = Ensure.NotNull(colours.RoomFxPaletteBlends);
        RoomPaletteFx = Ensure.NotNull(colours.RoomPaletteFx);
        MotherBrainRainbowColours = Ensure.NotNull(colours.MotherBrainRainbow);
    }

    /// <summary>
    /// Mother Brain's rainbow, drain, revival, fake-death and restoration colours, which the import pre-colours her sheets
    /// and body layer from (#1512). The game keeps only the beam backdrop's colours taken from them
    /// (<see cref="AreaMapPresentation.MotherBrainRainbowPalette"/>).
    /// </summary>
    public InstalledMotherBrainRainbowPalette MotherBrainRainbowColours { get; }

    /// <summary>
    /// The palette-effect colours, which the import pre-colours the sheets each palette-effect record shows from (#1512,
    /// #1583). The game keeps only the backdrops of the space behind the exploding planet taken from them
    /// (<see cref="AreaMapPresentation.EndingSpaceBackdrops"/>).
    /// </summary>
    public RoomPaletteFxPresentation RoomPaletteFx { get; }

    /// <summary>Mother Brain's health-band colours, which the import pre-colours her sheets and body layer from (#1527).</summary>
    public MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; }

    /// <summary>Mother Brain's room colours, which the import pre-colours her room's pictures and sheets from (#1527).</summary>
    public MotherBrainRoomColorPresentation MotherBrainRoomColors { get; }

    /// <summary>The room-FX blends, which the import pre-colours the effect overlay pictures with (#1527).</summary>
    public RoomFxBlendInks RoomFxPaletteBlends { get; }

    /// <summary>
    /// Ceres Ridley's colours, the shared Norfair Ridley health colours and the Ceres Baby's colours, which the import
    /// pre-colours their sheets and Ceres Ridley's room from (#1512).
    /// </summary>
    public CeresRidleyColorCatalog CeresRidleyColors { get; }

    /// <summary>Ceres Ridley's Mode 7 floor colours for each getaway zoom step, which the import pre-colours the floor's shades from (#1512).</summary>
    public CeresRidleyFloorColorCatalog CeresRidleyFloorColors { get; }

    /// <summary>The colours each map and menu screen loads, which the import pre-colours the screens' sheets from (#1512).</summary>
    public MapStaticPalettes Palettes { get; }

    /// <summary>The title's selected colours, which the import pre-colours the title's sheets from (#1512).</summary>
    public TitlePalettePresentation TitlePalette { get; }

    /// <summary>Samus's selected colour tables, which the import pre-colours her sheets from (#1512).</summary>
    public SamusMapColourTables SamusColours { get; }
}
