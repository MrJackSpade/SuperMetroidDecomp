namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>
/// The installed approach to Ceres (#1409): its palette, actor placements, and how many Mode 7 floor and OBJ characters its
/// artwork holds. The installed flight sheets picture the floors, characters and actor compositions; their maps, pixels
/// and parts belong to the import pipeline that reads the installed artwork, which also names the content's identity
/// (#1463, #1467).
/// Flight motion, DMA order and the SPACE COLONY letter script remain compiled logic.
/// </summary>
public abstract class CeresFlightArtwork
{
    /// <summary>Retains the selected palette and actor resources and the installed characters' counts.</summary>
    /// <param name="palette">The approach palette.</param>
    /// <param name="actors">The rear-view actors' placements.</param>
    /// <param name="floorCharacterCount">The installed Mode 7 floor characters.</param>
    /// <param name="objectCharacterCount">The installed OBJ characters.</param>
    internal CeresFlightArtwork(CeresFlightPalette palette, CeresFlightActorPlacements actors, int floorCharacterCount,
        int objectCharacterCount)
    {
        Palette = palette;
        Actors = actors;
        FloorCharacterCount = floorCharacterCount;
        ObjectCharacterCount = objectCharacterCount;
    }

    /// <summary>The installed Mode 7 floor characters, also reused by the ending flyaway.</summary>
    public int FloorCharacterCount { get; }
    /// <summary>The installed space and Ceres OBJ characters.</summary>
    public int ObjectCharacterCount { get; }
    /// <summary>Selected full 256-color RGB5 palette image for the approach; the native source is <c>$8C:E5E9</c>, separate from the PNGs' transport palettes.</summary>
    public CeresFlightPalette Palette { get; }
    /// <summary>Selected initial screen-pixel positions for the five named rear-view actors; their movement and instruction timing remain compiled logic.</summary>
    public CeresFlightActorPlacements Actors { get; }

    /// <summary>Identity of all selected approach maps, characters, colors, compositions and placements.</summary>
    public abstract string ContentIdentity { get; }
}
