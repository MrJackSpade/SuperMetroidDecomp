using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The owners whose colours palette-FX objects write (#1405, #1464).</summary>
public interface IPaletteFxColourOwners
{
    /// <summary>Every background set's colours, set after set.</summary>
    ColourRun BackgroundSets { get; }

    /// <summary>Every sprite set's colours, set after set.</summary>
    ColourRun SpriteSets { get; }

    /// <summary>Samus's suit colours.</summary>
    ColourRun SamusSuit { get; }

    /// <summary>Samus's beam colours.</summary>
    ColourRun SamusBeams { get; }
}
