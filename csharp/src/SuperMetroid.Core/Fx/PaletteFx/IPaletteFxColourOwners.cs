using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The owners whose colours palette-FX objects write (#1405, #1464): the colours a frontend screen holds for a title or
/// cinematic effect, the sprite sets, and Samus's suit and beams. The room's own colour groups are shades of its pictures
/// (#1512), which no owner holds as colours.
/// </summary>
public interface IPaletteFxColourOwners
{
    /// <summary>
    /// The colours a frontend screen holds for the effect colour group <paramref name="colours"/>, from the group's
    /// first written colour.
    /// </summary>
    /// <exception cref="InvalidOperationException">The owners are gameplay's, which hold no screen effect's colours.</exception>
    ColourRun ScreenColours(PaletteFxColours colours);

    /// <summary>Every sprite set's colours, set after set.</summary>
    ColourRun SpriteSets { get; }

    /// <summary>Samus's suit colours.</summary>
    ColourRun SamusSuit { get; }

    /// <summary>Samus's beam colours.</summary>
    ColourRun SamusBeams { get; }
}
