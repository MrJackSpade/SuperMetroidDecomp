namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Whose colours a palette-FX object writes (#1405).</summary>
public enum PaletteFxColourOwner
{
    /// <summary>Gameplay's background colour sets.</summary>
    Background,

    /// <summary>Gameplay's sprite colour sets.</summary>
    Sprite,

    /// <summary>Samus's suit colours.</summary>
    SamusSuit,

    /// <summary>Samus's beam colours.</summary>
    SamusBeams,
}
